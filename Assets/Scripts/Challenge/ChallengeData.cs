using System;

namespace RogueAI.Challenges
{
    [Serializable]
    public class ChallengeData
    {
        public string challengeId;
        public string title;
        public string statusText;
        public string question;
        public string codeSnippet;
        public string expectedAnswer;
        public string concept;
        public string type;
    }
}
