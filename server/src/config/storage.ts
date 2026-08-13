import path from "node:path";

export const dataDirectory =
  process.env.CLASSQUEST_DATA_DIR ?? path.resolve(__dirname, "../../data");
