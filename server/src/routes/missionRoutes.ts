import { Router } from "express";
import {
  createMission,
  getMissionByCode,
  getMissionById,
  getMissionReport,
  listMissions,
} from "../controllers/missionController";
import { asyncHandler } from "../utils/asyncHandler";

export const missionRouter = Router();

missionRouter.post("/", asyncHandler(createMission));
missionRouter.get("/", asyncHandler(listMissions));
missionRouter.get("/code/:code", asyncHandler(getMissionByCode));
missionRouter.get("/:id/report", asyncHandler(getMissionReport));
missionRouter.get("/:id", asyncHandler(getMissionById));
