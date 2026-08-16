using UnityEngine;

namespace RogueAI.ClassQuest
{
    public class ClassQuestApiConfig : MonoBehaviour
    {
        private const string DefaultDevelopmentBaseUrl = "http://192.168.1.76:4000";

        [SerializeField] private string baseUrl = DefaultDevelopmentBaseUrl;
        [SerializeField] private int timeoutSeconds = 8;

        public string BaseUrl => NormalizeBaseUrl(baseUrl);
        public int TimeoutSeconds => Mathf.Max(1, timeoutSeconds);

        public static string CurrentBaseUrl
        {
            get
            {
                ClassQuestApiConfig config = FindAnyObjectByType<ClassQuestApiConfig>();
                return config ? config.BaseUrl : NormalizeBaseUrl(DefaultDevelopmentBaseUrl);
            }
        }

        public static int CurrentTimeoutSeconds
        {
            get
            {
                ClassQuestApiConfig config = FindAnyObjectByType<ClassQuestApiConfig>();
                return config ? config.TimeoutSeconds : 8;
            }
        }

        private static string NormalizeBaseUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultDevelopmentBaseUrl;
            }

            return value.Trim().TrimEnd('/');
        }
    }
}
