export type MissionStatus = "published";

export type ChallengeType =
  | "multiple-choice"
  | "short-answer"
  | "predict-output"
  | "debug-code"
  | (string & {});

export interface Challenge {
  id: string;
  concept: string;
  type: ChallengeType;
  question: string;
  codeSnippet?: string;
  options?: string[];
  expectedAnswer: string;
}

export interface MissionMapping {
  challengeId: string;
  slotId: string;
  order: number;
}

export interface Mission {
  id: string;
  code: string;
  name: string;
  className: string;
  subject: string;
  topic: string;
  estimatedDuration: string | number;
  worldId: string;
  mapId: string;
  challenges: Challenge[];
  mappings: MissionMapping[];
  status: MissionStatus;
  createdAt: string;
}

export interface Attempt {
  id: string;
  missionId: string;
  missionCode: string;
  studentId: string;
  studentName: string;
  challengeId: string;
  slotId: string;
  submittedAnswer: string;
  correct: boolean;
  attemptNumber: number;
  timeTakenSeconds: number;
  createdAt: string;
}

export interface Student {
  id: string;
  name: string;
  firstSeenAt: string;
  lastSeenAt: string;
}

export interface DataStore {
  missions: Mission[];
  attempts: Attempt[];
  students: Student[];
}
