const defaultApiBaseUrl = 'http://127.0.0.1:4000'

export const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ?? defaultApiBaseUrl

export class ApiError extends Error {
  readonly status?: number
  readonly payload?: unknown

  constructor(message: string, status?: number, payload?: unknown) {
    super(message)
    this.status = status
    this.payload = payload
  }
}

export async function apiRequest<TResponse>(
  path: string,
  options: RequestInit = {},
): Promise<TResponse> {
  let response: Response

  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      ...options,
      headers: {
        ...(options.body ? { 'Content-Type': 'application/json' } : {}),
        ...options.headers,
      },
    })
  } catch {
    throw new ApiError('Could not reach ClassQuest Mission Server.')
  }

  const payload = await parseJson(response)

  if (!response.ok) {
    const message =
      typeof payload === 'object' &&
      payload !== null &&
      'error' in payload &&
      typeof payload.error === 'object' &&
      payload.error !== null &&
      'message' in payload.error &&
      typeof payload.error.message === 'string'
        ? payload.error.message
        : 'ClassQuest Mission Server returned an error.'

    throw new ApiError(message, response.status, payload)
  }

  return payload as TResponse
}

async function parseJson(response: Response): Promise<unknown> {
  const text = await response.text()
  return text ? JSON.parse(text) : null
}
