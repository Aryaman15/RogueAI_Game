import type { Request, Response } from "express";
import { createMissionSchema } from "../models/schemas";
import { missionService, reportService } from "../services/serviceContext";

export async function createMission(request: Request, response: Response): Promise<void> {
  const input = createMissionSchema.parse(request.body);
  const mission = await missionService.createMission(input);
  response.status(201).json(mission);
}

export async function listMissions(_request: Request, response: Response): Promise<void> {
  const missions = await missionService.listMissions();
  response.json(missions);
}

export async function getMissionById(request: Request, response: Response): Promise<void> {
  const mission = await missionService.getMissionById(request.params.id);
  response.json(mission);
}

export async function getMissionByCode(request: Request, response: Response): Promise<void> {
  const mission = await missionService.getGameReadyMissionByCode(request.params.code);
  response.json(mission);
}

export async function getMissionReport(request: Request, response: Response): Promise<void> {
  const report = await reportService.getMissionReport(request.params.id);
  response.json(report);
}
