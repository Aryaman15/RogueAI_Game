import { Router } from "express";
import { resetDemoData } from "../controllers/devController";
import { asyncHandler } from "../utils/asyncHandler";

export const devRouter = Router();

devRouter.post("/reset", asyncHandler(resetDemoData));
