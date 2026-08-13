import cors from "cors";
import express, { type NextFunction, type Request, type Response } from "express";
import { ZodError } from "zod";
import { apiRouter } from "./routes/apiRoutes";
import { HttpError } from "./utils/httpError";

const app = express();
const port = Number(process.env.PORT ?? 4000);
const host = "0.0.0.0";

app.use(cors());
app.use(express.json({ limit: "1mb" }));
app.use("/api", apiRouter);

app.use((_request, response) => {
  response.status(404).json({
    error: {
      code: "NOT_FOUND",
      message: "Route not found",
    },
  });
});

app.use((error: unknown, _request: Request, response: Response, _next: NextFunction) => {
  if (error instanceof ZodError) {
    response.status(400).json({
      error: {
        code: "VALIDATION_ERROR",
        message: "Request validation failed",
        issues: error.issues,
      },
    });
    return;
  }

  if (error instanceof HttpError) {
    response.status(error.statusCode).json({
      error: {
        code: error.code,
        message: error.message,
      },
    });
    return;
  }

  console.error(error);
  response.status(500).json({
    error: {
      code: "INTERNAL_SERVER_ERROR",
      message: "Unexpected server error",
    },
  });
});

app.listen(port, host, () => {
  console.log(`ClassQuest API listening on http://${host}:${port}`);
});
