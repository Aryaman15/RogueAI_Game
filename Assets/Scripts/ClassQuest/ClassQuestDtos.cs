using System;

namespace RogueAI.ClassQuest
{
    [Serializable]
    public class ClassQuestMissionDto
    {
        public string id;
        public string code;
        public string name;
        public string worldId;
        public string mapId;
        public ClassQuestChallengeDto[] challenges;
    }

    [Serializable]
    public class ClassQuestChallengeDto
    {
        public string id;
        public string slotId;
        public string concept;
        public string type;
        public string question;
        public string codeSnippet;
        public string[] options;
        public string expectedAnswer;
    }

    [Serializable]
    public class ClassQuestAttemptRequest
    {
        public string missionCode;
        public string studentId;
        public string studentName;
        public string challengeId;
        public string slotId;
        public string submittedAnswer;
        public bool correct;
        public int attemptNumber;
        public float timeTakenSeconds;
    }
}
