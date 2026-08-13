import { Router } from "express";
import { attemptRouter } from "./attemptRoutes";
import { devRouter } from "./devRoutes";
import { missionRouter } from "./missionRoutes";
import { studentRouter } from "./studentRoutes";

export const apiRouter = Router();

apiRouter.get("/health", (_request, response) => {
  response.json({ status: "ok" });
});

apiRouter.use("/missions", missionRouter);
apiRouter.use("/attempts", attemptRouter);
apiRouter.use("/students", studentRouter);
apiRouter.use("/dev", devRouter);
