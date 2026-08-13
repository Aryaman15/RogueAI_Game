import type { Mission } from "../models/types";
import type { JsonStore } from "./jsonStore";

export class MissionRepository {
  constructor(private readonly store: JsonStore) {}

  async list(): Promise<Mission[]> {
    const data = await this.store.read();
    return [...data.missions].sort((first, second) => second.createdAt.localeCompare(first.createdAt));
  }

  async findById(id: string): Promise<Mission | undefined> {
    const data = await this.store.read();
    return data.missions.find((mission) => mission.id === id);
  }

  async findByCode(code: string): Promise<Mission | undefined> {
    const normalizedCode = code.trim().toUpperCase();
    const data = await this.store.read();
    return data.missions.find((mission) => mission.code.toUpperCase() === normalizedCode);
  }

  async codeExists(code: string): Promise<boolean> {
    return Boolean(await this.findByCode(code));
  }

  async save(mission: Mission): Promise<Mission> {
    return this.store.mutate((data) => {
      data.missions.push(mission);
      return mission;
    });
  }
}
