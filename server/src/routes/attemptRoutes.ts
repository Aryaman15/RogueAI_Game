import { Router } from "express";
import { submitAttempt } from "../controllers/attemptController";
import { asyncHandler } from "../utils/asyncHandler";

export const attemptRouter = Router();

attemptRouter.post("/", asyncHandler(submitAttempt));
