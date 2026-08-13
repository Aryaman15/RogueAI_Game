using System.Text.RegularExpressions;
using RogueAI.Challenges;
using UnityEngine;

namespace RogueAI.ClassQuest
{
    public static class ClassQuestMissionRuntime
    {
        public static string MissionId { get; private set; }
        public static string MissionCode { get; private set; }
        public static string MissionName { get; private set; }
        public static string WorldId { get; private set; }
        public static string MapId { get; private set; }
        public static string StudentId { get; private set; }
        public static string StudentName { get; private set; }
        public static ClassQuestChallengeDto[] Challenges { get; private set; }

        public static bool HasMission => !string.IsNullOrWhiteSpace(MissionId);

        public static void SetMission(ClassQuestMissionDto mission, string studentName)
        {
            MissionId = mission.id;
            MissionCode = mission.code;
            MissionName = mission.name;
            WorldId = mission.worldId;
            MapId = mission.mapId;
            StudentName = studentName.Trim();
            StudentId = CreateStudentId(StudentName);
            Challenges = mission.challenges ?? new ClassQuestChallengeDto[0];
        }

        public static bool TryGetChallenge(string slotId, out ClassQuestChallengeDto challenge)
        {
            challenge = null;

            if (!HasMission || Challenges == null)
            {
                return false;
            }

            foreach (ClassQuestChallengeDto candidate in Challenges)
            {
                if (candidate != null && candidate.slotId == slotId)
                {
                    challenge = candidate;
                    return true;
                }
            }

            return false;
        }

        public static ChallengeData ToChallengeData(ClassQuestChallengeDto challenge, ChallengeData fallback)
        {
            return new ChallengeData
            {
                challengeId = challenge.id,
                slotId = challenge.slotId,
                title = fallback != null && !string.IsNullOrWhiteSpace(fallback.title)
                    ? fallback.title
                    : "GENERATOR CONTROL TERMINAL",
                statusText = fallback != null && !string.IsNullOrWhiteSpace(fallback.statusText)
                    ? fallback.statusText
                    : "POWER GRID OFFLINE\nMANUAL OVERRIDE REQUIRED",
                question = challenge.question,
                codeSnippet = challenge.codeSnippet,
                expectedAnswer = challenge.expectedAnswer,
                concept = challenge.concept,
                type = challenge.type
            };
        }

        private static string CreateStudentId(string studentName)
        {
            string slug = Regex.Replace(studentName.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');

            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = "student";
            }

            return $"student-{slug}";
        }
    }
}
