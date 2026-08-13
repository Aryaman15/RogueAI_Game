import { Router } from "express";
import { getStudentReport } from "../controllers/studentController";
import { asyncHandler } from "../utils/asyncHandler";

export const studentRouter = Router();

studentRouter.get("/:studentId/report", asyncHandler(getStudentReport));
