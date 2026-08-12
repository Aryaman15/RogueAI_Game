import type { Challenge, MapConfig, MapChallengeSlot, MissionMapping } from './models'

export function mapChallengesToMission(
  mapConfig: MapConfig,
  challenges: Challenge[],
): MissionMapping[] {
  return challenges.map((challenge, index) => {
    const slot = mapConfig.challengeSlots[index]

    if (!slot) {
      throw new Error(`Map ${mapConfig.name} supports only ${mapConfig.maxChallenges} challenges.`)
    }

    return {
      challengeId: challenge.challengeId,
      slotId: slot.id,
      order: index + 1,
    }
  })
}

export function getSlotForMapping(
  mapConfig: MapConfig,
  mapping: MissionMapping,
): MapChallengeSlot | undefined {
  return mapConfig.challengeSlots.find((slot) => slot.id === mapping.slotId)
}
