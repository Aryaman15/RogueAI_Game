export interface ChallengeSlotConfig {
  id: string;
  displayName: string;
  gameAction: string;
}

export interface MapConfig {
  worldId: string;
  mapId: string;
  maxChallenges: number;
  slots: ChallengeSlotConfig[];
}

export const ROGUE_AI_HEADQUARTERS_WORLD_ID = "rogue-ai-headquarters";
export const POWER_SECTOR_MAP_ID = "power-sector";

export const mapConfigs: MapConfig[] = [
  {
    worldId: ROGUE_AI_HEADQUARTERS_WORLD_ID,
    mapId: POWER_SECTOR_MAP_ID,
    maxChallenges: 4,
    slots: [
      {
        id: "generator-terminal",
        displayName: "Generator Control",
        gameAction: "Restore sector power",
      },
      {
        id: "security-terminal",
        displayName: "Security Override",
        gameAction: "Unlock secured area",
      },
      {
        id: "power-module-terminal",
        displayName: "Power Module Access",
        gameAction: "Retrieve shutdown hardware",
      },
      {
        id: "exit-terminal",
        displayName: "Exit Authorization",
        gameAction: "Complete Power Sector",
      },
    ],
  },
];

export function getMapConfig(worldId: string, mapId: string): MapConfig | undefined {
  return mapConfigs.find((config) => config.worldId === worldId && config.mapId === mapId);
}
