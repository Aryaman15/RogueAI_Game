import { mapConfigs, worlds } from '../data/mockData'
import type { Challenge, Mission } from '../data/models'
import { apiRequest } from './client'
import {
  type BackendMission,
  type CreateMissionRequest,
  toBackendChallengeType,
  toFrontendChallengeType,
} from './types'

export async function createMission(input: {
  name: string
  className: string
  subject: string
  topic: string
  estimatedDuration: string
  worldId: string
  mapId: string
  challenges: Challenge[]
}): Promise<Mission> {
  const request: CreateMissionRequest = {
    name: input.name,
    className: input.className,
    subject: input.subject,
    topic: input.topic,
    estimatedDuration: input.estimatedDuration,
    worldId: input.worldId,
    mapId: input.mapId,
    challenges: input.challenges.map((challenge) => ({
      concept: challenge.concept,
      type: toBackendChallengeType(challenge.type),
      question: challenge.question,
      codeSnippet: challenge.codeSnippet || undefined,
      options: challenge.options?.filter(Boolean),
      expectedAnswer: challenge.expectedAnswer,
    })),
  }

  const mission = await apiRequest<BackendMission>('/api/missions', {
    method: 'POST',
    body: JSON.stringify(request),
  })

  return adaptMission(mission)
}

export async function listMissions(): Promise<Mission[]> {
  const missions = await apiRequest<BackendMission[]>('/api/missions')
  return missions.map(adaptMission)
}

export async function getMission(id: string): Promise<Mission> {
  const mission = await apiRequest<BackendMission>(`/api/missions/${id}`)
  return adaptMission(mission)
}

function adaptMission(mission: BackendMission): Mission {
  const world = worlds.find((item) => item.id === mission.worldId)
  const map = world?.maps.find((item) => item.id === mission.mapId)
  const mapConfig = mapConfigs.find(
    (config) => config.worldId === mission.worldId && config.challengeSlots.some(Boolean),
  )

  return {
    id: mission.id,
    missionCode: mission.code,
    name: mission.name,
    classId: mission.className.toLowerCase().replace(/\W+/g, '-'),
    className: mission.className,
    subject: mission.subject,
    topic: mission.topic,
    estimatedDuration: String(mission.estimatedDuration),
    worldId: mission.worldId,
    worldName: world?.name ?? mission.worldId,
    mapId: mission.mapId,
    mapName: map?.name ?? mapConfig?.name ?? mission.mapId,
    status: mission.status,
    challengeIds: mission.challenges.map((challenge) => challenge.id),
    challenges: mission.challenges.map((challenge) => ({
      challengeId: challenge.id,
      concept: challenge.concept,
      type: toFrontendChallengeType(challenge.type),
      question: challenge.question,
      codeSnippet: challenge.codeSnippet,
      options: challenge.options,
      expectedAnswer: challenge.expectedAnswer,
    })),
    mappings: mission.mappings,
    createdAt: mission.createdAt,
  }
}
