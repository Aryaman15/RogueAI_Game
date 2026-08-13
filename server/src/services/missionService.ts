import { getMapConfig } from "../config/maps";
import type { Challenge, Mission } from "../models/types";
import type { CreateMissionInput } from "../models/schemas";
import { generateId, generateMissionCode } from "../utils/ids";
import { badRequest, notFound } from "../utils/httpError";
import type { MissionRepository } from "../repositories/missionRepository";

export interface GameReadyChallenge extends Challenge {
  slotId: string;
}

export interface GameReadyMission {
  id: string;
  code: string;
  name: string;
  worldId: string;
  mapId: string;
  challenges: GameReadyChallenge[];
}

export class MissionService {
  constructor(private readonly missions: MissionRepository) {}

  async createMission(input: CreateMissionInput): Promise<Mission> {
    const mapConfig = getMapConfig(input.worldId, input.mapId);

    if (!mapConfig) {
      throw badRequest(`Unsupported world/map combination: ${input.worldId}/${input.mapId}`);
    }

    if (input.challenges.length > mapConfig.maxChallenges) {
      throw badRequest(`Map ${input.mapId} supports at most ${mapConfig.maxChallenges} challenges`);
    }

    const challenges = input.challenges.map((challenge): Challenge => ({
      id: generateId("challenge"),
      concept: challenge.concept,
      type: challenge.type,
      question: challenge.question,
      codeSnippet: challenge.codeSnippet,
      options: challenge.options,
      expectedAnswer: challenge.expectedAnswer,
    }));

    const mappings = challenges.map((challenge, index) => ({
      challengeId: challenge.id,
      slotId: mapConfig.slots[index].id,
      order: index + 1,
    }));

    const mission: Mission = {
      id: generateId("mission"),
      code: await this.generateUniqueMissionCode(),
      name: input.name,
      className: input.className,
      subject: input.subject,
      topic: input.topic,
      estimatedDuration: input.estimatedDuration,
      worldId: input.worldId,
      mapId: input.mapId,
      challenges,
      mappings,
      status: "published",
      createdAt: new Date().toISOString(),
    };

    return this.missions.save(mission);
  }

  async listMissions(): Promise<Mission[]> {
    return this.missions.list();
  }

  async getMissionById(id: string): Promise<Mission> {
    const mission = await this.missions.findById(id);

    if (!mission) {
      throw notFound(`Mission ${id} was not found`);
    }

    return mission;
  }

  async getMissionByCode(code: string): Promise<Mission> {
    const mission = await this.missions.findByCode(code);

    if (!mission) {
      throw notFound(`Mission code ${code} was not found`);
    }

    return mission;
  }

  async getGameReadyMissionByCode(code: string): Promise<GameReadyMission> {
    const mission = await this.getMissionByCode(code);

    const challenges = [...mission.mappings]
      .sort((first, second) => first.order - second.order)
      .map((mapping) => {
        const challenge = mission.challenges.find((candidate) => candidate.id === mapping.challengeId);

        if (!challenge) {
          throw badRequest(`Mission ${mission.id} contains an invalid challenge mapping`);
        }

        return {
          ...challenge,
          slotId: mapping.slotId,
        };
      });

    return {
      id: mission.id,
      code: mission.code,
      name: mission.name,
      worldId: mission.worldId,
      mapId: mission.mapId,
      challenges,
    };
  }

  private async generateUniqueMissionCode(): Promise<string> {
    for (let attempt = 0; attempt < 10; attempt += 1) {
      const code = generateMissionCode();

      if (!(await this.missions.codeExists(code))) {
        return code;
      }
    }

    throw badRequest("Could not generate a unique mission code");
  }
}
