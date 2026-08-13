import type { SubmitAttemptInput } from "../models/schemas";
import type { Attempt } from "../models/types";
import type { AttemptRepository } from "../repositories/attemptRepository";
import type { MissionRepository } from "../repositories/missionRepository";
import type { StudentRepository } from "../repositories/studentRepository";
import { badRequest } from "../utils/httpError";
import { generateId } from "../utils/ids";

export class AttemptService {
  constructor(
    private readonly attempts: AttemptRepository,
    private readonly missions: MissionRepository,
    private readonly students: StudentRepository,
  ) {}

  async submitAttempt(input: SubmitAttemptInput): Promise<Attempt> {
    const mission = await this.missions.findByCode(input.missionCode);

    if (!mission) {
      throw badRequest(`Mission code ${input.missionCode} does not exist`);
    }

    const challenge = mission.challenges.find((candidate) => candidate.id === input.challengeId);

    if (!challenge) {
      throw badRequest(`Challenge ${input.challengeId} does not exist on mission ${mission.code}`);
    }

    const mapping = mission.mappings.find(
      (candidate) => candidate.challengeId === input.challengeId && candidate.slotId === input.slotId,
    );

    if (!mapping) {
      throw badRequest(`Challenge ${input.challengeId} is not mapped to slot ${input.slotId}`);
    }

    const createdAt = new Date().toISOString();
    await this.students.upsert(input.studentId, input.studentName, createdAt);

    const attempt: Attempt = {
      id: generateId("attempt"),
      missionId: mission.id,
      missionCode: mission.code,
      studentId: input.studentId,
      studentName: input.studentName,
      challengeId: input.challengeId,
      slotId: input.slotId,
      submittedAnswer: input.submittedAnswer,
      correct: input.correct,
      attemptNumber: input.attemptNumber,
      timeTakenSeconds: input.timeTakenSeconds,
      createdAt,
    };

    return this.attempts.save(attempt);
  }
}
