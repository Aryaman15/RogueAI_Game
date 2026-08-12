import { missions } from './mockData'
import type { Challenge, Mission, MissionMapping } from './models'

const localMissionKey = 'classquest.localMissions'
const missionCodeKey = 'classquest.lastMissionCode'

function readLocalMissions(): Mission[] {
  if (typeof window === 'undefined') {
    return []
  }

  try {
    const stored = window.localStorage.getItem(localMissionKey)
    return stored ? (JSON.parse(stored) as Mission[]) : []
  } catch {
    return []
  }
}

function writeLocalMissions(localMissions: Mission[]) {
  window.localStorage.setItem(localMissionKey, JSON.stringify(localMissions))
}

export function getMissions(): Mission[] {
  return [...readLocalMissions(), ...missions]
}

export function getMissionById(missionId: string): Mission | undefined {
  return getMissions().find((mission) => mission.id === missionId)
}

export function generateMissionCode() {
  if (typeof window === 'undefined') {
    return 'CQ-7X42'
  }

  const existing = window.localStorage.getItem(missionCodeKey)
  if (existing) {
    return existing
  }

  const code = 'CQ-7X42'
  window.localStorage.setItem(missionCodeKey, code)
  return code
}

export interface DraftMissionInput {
  name: string
  className: string
  subject: string
  topic: string
  estimatedDuration: string
  worldId: string
  worldName: string
  mapId: string
  mapName: string
  challenges: Challenge[]
  mappings: MissionMapping[]
}

export function publishMission(input: DraftMissionInput): Mission {
  const missionCode = generateMissionCode()
  const mission: Mission = {
    id: `mission-${Date.now()}`,
    missionCode,
    name: input.name,
    classId: input.className.toLowerCase().replace(/\W+/g, '-'),
    className: input.className,
    subject: input.subject,
    topic: input.topic,
    estimatedDuration: input.estimatedDuration,
    worldId: input.worldId,
    worldName: input.worldName,
    mapId: input.mapId,
    mapName: input.mapName,
    status: 'published',
    challengeIds: input.challenges.map((challenge) => challenge.challengeId),
    challenges: input.challenges,
    mappings: input.mappings,
    createdAt: new Date().toISOString(),
  }

  const localMissions = readLocalMissions()
  writeLocalMissions([mission, ...localMissions])
  return mission
}
