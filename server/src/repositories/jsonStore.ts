import { promises as fs } from "node:fs";
import path from "node:path";
import type { DataStore } from "../models/types";

const initialStore: DataStore = {
  missions: [],
  attempts: [],
  students: [],
};

export class JsonStore {
  private readonly filePath: string;
  private writeQueue: Promise<unknown> = Promise.resolve();

  constructor(dataDirectory: string) {
    this.filePath = path.join(dataDirectory, "store.json");
  }

  async read(): Promise<DataStore> {
    await this.ensureStore();
    const content = await fs.readFile(this.filePath, "utf8");
    const parsed = JSON.parse(content) as Partial<DataStore>;

    return {
      missions: parsed.missions ?? [],
      attempts: parsed.attempts ?? [],
      students: parsed.students ?? [],
    };
  }

  async write(data: DataStore): Promise<void> {
    await this.ensureStore();
    await fs.writeFile(this.filePath, `${JSON.stringify(data, null, 2)}\n`, "utf8");
  }

  async mutate<T>(mutator: (data: DataStore) => T | Promise<T>): Promise<T> {
    const nextWrite = this.writeQueue.then(async () => {
      const data = await this.read();
      const result = await mutator(data);
      await this.write(data);
      return result;
    });

    this.writeQueue = nextWrite.catch(() => undefined);
    return nextWrite;
  }

  async reset(data: DataStore = initialStore): Promise<DataStore> {
    const cloned = structuredClone(data);
    await this.write(cloned);
    return cloned;
  }

  private async ensureStore(): Promise<void> {
    await fs.mkdir(path.dirname(this.filePath), { recursive: true });

    try {
      await fs.access(this.filePath);
    } catch {
      await fs.writeFile(this.filePath, `${JSON.stringify(initialStore, null, 2)}\n`, "utf8");
    }
  }
}
