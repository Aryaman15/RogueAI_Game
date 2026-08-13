using RogueAI.Challenges;
using UnityEngine;

namespace RogueAI.ClassQuest
{
    public static class ClassQuestAttemptReporter
    {
        public static void ReportAttempt(
            MonoBehaviour runner,
            ChallengeData challenge,
            string submittedAnswer,
            bool correct,
            int attemptNumber,
            float timeTakenSeconds)
        {
            if (!runner || challenge == null || !ClassQuestMissionRuntime.HasMission)
            {
                return;
            }

            ClassQuestAttemptRequest payload = new ClassQuestAttemptRequest
            {
                missionCode = ClassQuestMissionRuntime.MissionCode,
                studentId = ClassQuestMissionRuntime.StudentId,
                studentName = ClassQuestMissionRuntime.StudentName,
                challengeId = challenge.challengeId,
                slotId = challenge.slotId,
                submittedAnswer = submittedAnswer ?? string.Empty,
                correct = correct,
                attemptNumber = attemptNumber,
                timeTakenSeconds = Mathf.Max(0f, timeTakenSeconds)
            };

            runner.StartCoroutine(ClassQuestApiClient.SubmitAttempt(payload));
        }
    }
}
