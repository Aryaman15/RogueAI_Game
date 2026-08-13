import { randomBytes, randomUUID } from "node:crypto";

const CODE_ALPHABET = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

export function generateId(prefix: string): string {
  return `${prefix}_${randomUUID()}`;
}

export function generateMissionCode(): string {
  const bytes = randomBytes(4);
  let suffix = "";

  for (let index = 0; index < 4; index += 1) {
    suffix += CODE_ALPHABET[bytes[index] % CODE_ALPHABET.length];
  }

  return `CQ-${suffix}`;
}
