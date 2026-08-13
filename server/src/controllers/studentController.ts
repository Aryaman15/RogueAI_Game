import type { Request, Response } from "express";
import { reportService } from "../services/serviceContext";

export async function getStudentReport(request: Request, response: Response): Promise<void> {
  const report = await reportService.getStudentReport(request.params.studentId);
  response.json(report);
}
