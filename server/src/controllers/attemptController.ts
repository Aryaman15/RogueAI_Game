import type { Request, Response } from "express";
import { submitAttemptSchema } from "../models/schemas";
import { attemptService } from "../services/serviceContext";

export async function submitAttempt(request: Request, response: Response): Promise<void> {
  const input = submitAttemptSchema.parse(request.body);
  const attempt = await attemptService.submitAttempt(input);
  response.status(201).json(attempt);
}
