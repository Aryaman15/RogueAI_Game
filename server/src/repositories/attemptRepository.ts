import type { Attempt } from "../models/types";
import type { JsonStore } from "./jsonStore";

export class AttemptRepository {
  constructor(private readonly store: JsonStore) {}

  async list(): Promise<Attempt[]> {
    const data = await this.store.read();
    return [...data.attempts].sort((first, second) => first.createdAt.localeCompare(second.createdAt));
  }

  async listByMissionId(missionId: string): Promise<Attempt[]> {
    const attempts = await this.list();
    return attempts.filter((attempt) => attempt.missionId === missionId);
  }

  async listByStudentId(studentId: string): Promise<Attempt[]> {
    const attempts = await this.list();
    return attempts.filter((attempt) => attempt.studentId === studentId);
  }

  async save(attempt: Attempt): Promise<Attempt> {
    return this.store.mutate((data) => {
      data.attempts.push(attempt);
      return attempt;
    });
  }
}
