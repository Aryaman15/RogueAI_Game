import type { Student } from "../models/types";
import type { JsonStore } from "./jsonStore";

export class StudentRepository {
  constructor(private readonly store: JsonStore) {}

  async list(): Promise<Student[]> {
    const data = await this.store.read();
    return [...data.students].sort((first, second) => first.name.localeCompare(second.name));
  }

  async findById(id: string): Promise<Student | undefined> {
    const data = await this.store.read();
    return data.students.find((student) => student.id === id);
  }

  async upsert(id: string, name: string, seenAt: string): Promise<Student> {
    return this.store.mutate((data) => {
      const existing = data.students.find((student) => student.id === id);

      if (existing) {
        existing.name = name;
        existing.lastSeenAt = seenAt;
        return existing;
      }

      const student: Student = {
        id,
        name,
        firstSeenAt: seenAt,
        lastSeenAt: seenAt,
      };

      data.students.push(student);
      return student;
    });
  }
}
