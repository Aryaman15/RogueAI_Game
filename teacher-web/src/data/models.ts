export type ChallengeType =
  | 'Predict Output'
  | 'Multiple Choice'
  | 'Text Answer'
  | 'Debug Code'
  | 'SQL Query'

export type MissionStatus = 'draft' | 'published'
export type WorldAvailability = 'available' | 'coming-soon'
export type StudentStatus = 'Strong' | 'On Track' | 'Needs Review' | 'Attention'

export interface Teacher {
  id: string
  name: string
  subject: string
}

export interface ClassGroup {
  id: string
  name: string
  grade: string
  studentCount: number
}

export interface WorldMap {
  id: string
  name: string
  mapConfigId: string
}

export interface World {
  id: string
  name: string
  genre: string
  availability: WorldAvailability
  description: string
  maps: WorldMap[]
}

export interface Challenge {
  challengeId: string
  concept: string
  type: ChallengeType
  question: string
  codeSnippet?: string
  options?: string[]
  expectedAnswer: string
}

export interface MapChallengeSlot {
  id: string
  displayName: string
  gameAction: string
  order: number
}

export interface MapConfig {
  id: string
  worldId: string
  name: string
  maxChallenges: number
  challengeSlots: MapChallengeSlot[]
}

export interface MissionMapping {
  challengeId: string
  slotId: string
  order: number
}

export interface Mission {
  id: string
  missionCode: string
  name: string
  classId: string
  className: string
  subject: string
  topic: string
  estimatedDuration: string
  worldId: string
  worldName: string
  mapId: string
  mapName: string
  status: MissionStatus
  challengeIds: string[]
  challenges: Challenge[]
  mappings: MissionMapping[]
  createdAt: string
}

export interface ConceptMastery {
  concept: string
  mastery: number
}

export interface LearningInsight {
  id: string
  title: string
  evidence: string
  misconception: string
  recommendedAction: string
  severity: 'medium' | 'high'
  affectedStudents: number
}

export interface Student {
  id: string
  name: string
  classId: string
}

export interface Attempt {
  id: string
  challengeId: string
  checkpoint: string
  question: string
  codeSnippet?: string
  answer: string
  isCorrect: boolean
  timeSeconds: number
}

export interface MissionResult {
  missionId: string
  studentId: string
  completion: number
  mastery: number
  attempts: number
  timeMinutes: number
  status: StudentStatus
  attemptHistory: Attempt[]
}

export interface StudentDiagnostic {
  studentId: string
  overallMastery: number
  assignmentsCompleted: string
  currentTrend: string
  skillBreakdown: ConceptMastery[]
  pattern: string
  recommendedAction: string
}
