import { dataDirectory } from "../config/storage";
import { AttemptRepository } from "./attemptRepository";
import { JsonStore } from "./jsonStore";
import { MissionRepository } from "./missionRepository";
import { StudentRepository } from "./studentRepository";

export const jsonStore = new JsonStore(dataDirectory);
export const missionRepository = new MissionRepository(jsonStore);
export const attemptRepository = new AttemptRepository(jsonStore);
export const studentRepository = new StudentRepository(jsonStore);
