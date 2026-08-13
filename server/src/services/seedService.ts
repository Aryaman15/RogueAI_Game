import type { DataStore, Mission } from "../models/types";
import { POWER_SECTOR_MAP_ID, ROGUE_AI_HEADQUARTERS_WORLD_ID } from "../config/maps";
import type { JsonStore } from "../repositories/jsonStore";

export class SeedService {
  constructor(private readonly store: JsonStore) {}

  async resetWithDemoData(): Promise<DataStore> {
    const createdAt = new Date().toISOString();
    const mission: Mission = {
      id: "mission_demo_power_sector",
      code: "CQ-DEMO",
      name: "Python Loops Revision",
      className: "Grade 8",
      subject: "Computer Science",
      topic: "Python Loops",
      estimatedDuration: 20,
      worldId: ROGUE_AI_HEADQUARTERS_WORLD_ID,
      mapId: POWER_SECTOR_MAP_ID,
      status: "published",
      createdAt,
      challenges: [
        {
          id: "challenge_demo_generator",
          concept: "Python Loops",
          type: "predict-output",
          question: "What is the output of the loop?",
          codeSnippet: "for i in range(1, 4):\n    print(i)",
          expectedAnswer: "1 2 3",
        },
        {
          id: "challenge_demo_security",
          concept: "Loop Conditions",
          type: "short-answer",
          question: "Which keyword stops a loop early?",
          expectedAnswer: "break",
        },
        {
          id: "challenge_demo_module",
          concept: "Lists",
          type: "multiple-choice",
          question: "Which expression returns the first item in a Python list named items?",
          options: ["items[0]", "items[1]", "items.first()", "items.start"],
          expectedAnswer: "items[0]",
        },
        {
          id: "challenge_demo_exit",
          concept: "Range",
          type: "short-answer",
          question: "How many numbers does range(5) produce?",
          expectedAnswer: "5",
        },
      ],
      mappings: [
        { challengeId: "challenge_demo_generator", slotId: "generator-terminal", order: 1 },
        { challengeId: "challenge_demo_security", slotId: "security-terminal", order: 2 },
        { challengeId: "challenge_demo_module", slotId: "power-module-terminal", order: 3 },
        { challengeId: "challenge_demo_exit", slotId: "exit-terminal", order: 4 },
      ],
    };

    const data: DataStore = {
      missions: [mission],
      students: [
        student("student_ada", "Ada", createdAt),
        student("student_grace", "Grace", createdAt),
        student("student_katherine", "Katherine", createdAt),
      ],
      attempts: [
        attempt("attempt_1", "student_ada", "Ada", "challenge_demo_generator", "generator-terminal", "1 2 3", true, 1, 38),
        attempt("attempt_2", "student_ada", "Ada", "challenge_demo_security", "security-terminal", "continue", false, 1, 44),
        attempt("attempt_3", "student_ada", "Ada", "challenge_demo_security", "security-terminal", "break", true, 2, 31),
        attempt("attempt_4", "student_grace", "Grace", "challenge_demo_generator", "generator-terminal", "0 1 2", false, 1, 52),
        attempt("attempt_5", "student_grace", "Grace", "challenge_demo_module", "power-module-terminal", "items[0]", true, 1, 29),
        attempt("attempt_6", "student_katherine", "Katherine", "challenge_demo_generator", "generator-terminal", "1 2 3", true, 1, 33),
        attempt("attempt_7", "student_katherine", "Katherine", "challenge_demo_security", "security-terminal", "break", true, 1, 27),
        attempt("attempt_8", "student_katherine", "Katherine", "challenge_demo_module", "power-module-terminal", "items[0]", true, 1, 24),
        attempt("attempt_9", "student_katherine", "Katherine", "challenge_demo_exit", "exit-terminal", "5", true, 1, 21),
      ],
    };

    return this.store.reset(data);
  }
}

function student(id: string, name: string, seenAt: string) {
  return {
    id,
    name,
    firstSeenAt: seenAt,
    lastSeenAt: seenAt,
  };
}

function attempt(
  id: string,
  studentId: string,
  studentName: string,
  challengeId: string,
  slotId: string,
  submittedAnswer: string,
  correct: boolean,
  attemptNumber: number,
  timeTakenSeconds: number,
) {
  return {
    id,
    missionId: "mission_demo_power_sector",
    missionCode: "CQ-DEMO",
    studentId,
    studentName,
    challengeId,
    slotId,
    submittedAnswer,
    correct,
    attemptNumber,
    timeTakenSeconds,
    createdAt: new Date().toISOString(),
  };
}
