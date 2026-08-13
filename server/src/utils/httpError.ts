export class HttpError extends Error {
  constructor(
    public readonly statusCode: number,
    message: string,
    public readonly code = "HTTP_ERROR",
  ) {
    super(message);
  }
}

export function badRequest(message: string): HttpError {
  return new HttpError(400, message, "BAD_REQUEST");
}

export function notFound(message: string): HttpError {
  return new HttpError(404, message, "NOT_FOUND");
}
