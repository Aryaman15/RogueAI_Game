import type { Request, Response } from "express";
import { seedService } from "../services/serviceContext";

export async function resetDemoData(_request: Request, response: Response): Promise<void> {
  const data = await seedService.resetWithDemoData();
  response.json({
    status: "reset",
    missions: data.missions.length,
    students: data.students.length,
    attempts: data.attempts.length,
    demoMissionCode: "CQ-DEMO",
  });
}
