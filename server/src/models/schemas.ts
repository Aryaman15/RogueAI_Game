import { z } from "zod";

export const challengeInputSchema = z.object({
  concept: z.string().trim().min(1),
  type: z.string().trim().min(1),
  question: z.string().trim().min(1),
  codeSnippet: z.string().optional(),
  options: z.array(z.string()).optional(),
  expectedAnswer: z.string().trim().min(1),
});

export const createMissionSchema = z.object({
  name: z.string().trim().min(1),
  className: z.string().trim().min(1),
  subject: z.string().trim().min(1),
  topic: z.string().trim().min(1),
  estimatedDuration: z.union([z.string().trim().min(1), z.number().nonnegative()]),
  worldId: z.string().trim().min(1).default("rogue-ai-headquarters"),
  mapId: z.string().trim().min(1).default("power-sector"),
  challenges: z.array(challengeInputSchema).min(1),
});

export const submitAttemptSchema = z.object({
  missionCode: z.string().trim().min(1),
  studentId: z.string().trim().min(1),
  studentName: z.string().trim().min(1),
  challengeId: z.string().trim().min(1),
  slotId: z.string().trim().min(1),
  submittedAnswer: z.string(),
  correct: z.boolean(),
  attemptNumber: z.number().int().positive(),
  timeTakenSeconds: z.number().nonnegative(),
});

export type CreateMissionInput = z.infer<typeof createMissionSchema>;
export type SubmitAttemptInput = z.infer<typeof submitAttemptSchema>;
