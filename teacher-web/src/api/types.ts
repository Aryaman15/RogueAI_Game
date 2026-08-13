import type { ChallengeType } from '../data/models'

export interface BackendChallenge {
  id: string
  concept: string
  type: string
  question: string
  codeSnippet?: string
  options?: string[]
  expectedAnswer: string
}

export interface BackendMissionMapping {
  challengeId: string
  slotId: string
  order: number
}

export interface BackendMission {
  id: string
  code: string
  name: string
  className: string
  subject: string
  topic: string
  estimatedDuration: string | number
  worldId: string
  mapId: string
  challenges: BackendChallenge[]
  mappings: BackendMissionMapping[]
  status: 'published'
  createdAt: string
}

export interface CreateMissionChallengeRequest {
  concept: string
  type: string
  question: string
  codeSnippet?: string
  options?: string[]
  expectedAnswer: string
}

export interface CreateMissionRequest {
  name: string
  className: string
  subject: string
  topic: string
  estimatedDuration: string
  worldId: string
  mapId: string
  challenges: CreateMissionChallengeRequest[]
}

export interface BackendChallengePerformance {
  challengeId: string
  slotId: string
  order: number
  concept: string
  studentsAttempted: number
  correctStudents: number
  incorrectAttempts: number
  averageAttempts: number
  averageTime: number
}

export interface BackendConceptPerformance {
  concept: string
  studentsAttempted: number
  correctStudents: number
  incorrectAttempts: number
  averageAttempts: number
  averageTime: number
}

export interface BackendStudentSummary {
  studentId: string
  studentName: string
  attempts: number
  attemptedChallenges: number
  correctChallenges: number
  completed: boolean
  totalTimeSeconds: number
  lastAttemptAt: string | null
}

export interface BackendMissionReport {
  mission: {
    id: string
    code: string
    name: string
    className: string
    subject: string
    topic: string
  }
  uniqueStudents: number
  completedStudents: number
  averageAttempts: number
  averageTime: number
  challengePerformance: BackendChallengePerformance[]
  conceptPerformance: BackendConceptPerformance[]
  studentSummaries: BackendStudentSummary[]
}

export interface BackendStudentReport {
  studentId: string
  studentName: string | null
  missionsAttempted: unknown[]
  attempts: unknown[]
  conceptLevelResults: unknown[]
  challengeHistory: unknown[]
}

export function toBackendChallengeType(type: ChallengeType): string {
  switch (type) {
    case 'Predict Output':
      return 'predict-output'
    case 'Multiple Choice':
      return 'multiple-choice'
    case 'Text Answer':
      return 'short-answer'
    case 'Debug Code':
      return 'debug-code'
    case 'SQL Query':
      return 'sql-query'
    default:
      return type
  }
}

export function toFrontendChallengeType(type: string): ChallengeType {
  switch (type) {
    case 'predict-output':
      return 'Predict Output'
    case 'multiple-choice':
      return 'Multiple Choice'
    case 'short-answer':
      return 'Text Answer'
    case 'debug-code':
      return 'Debug Code'
    case 'sql-query':
      return 'SQL Query'
    default:
      return 'Text Answer'
  }
}
