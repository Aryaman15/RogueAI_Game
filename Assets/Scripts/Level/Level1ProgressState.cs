using System;

namespace RogueAI.Level
{
    [Serializable]
    public class Level1ProgressState
    {
        public bool terminalChallengeCompleted;
        public bool generatorPowered;
        public bool securityDoorUnlocked;
        public bool powerModuleCollected;
        public bool levelCompleted;
    }
}
