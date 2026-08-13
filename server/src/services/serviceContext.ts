import { attemptRepository, jsonStore, missionRepository, studentRepository } from "../repositories/repositoryContext";
import { AttemptService } from "./attemptService";
import { MissionService } from "./missionService";
import { ReportService } from "./reportService";
import { SeedService } from "./seedService";

export const missionService = new MissionService(missionRepository);
export const attemptService = new AttemptService(attemptRepository, missionRepository, studentRepository);
export const reportService = new ReportService(missionRepository, attemptRepository);
export const seedService = new SeedService(jsonStore);
