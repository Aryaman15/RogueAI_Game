using System.Text.RegularExpressions;
using UnityEngine;

namespace RogueAI.ClassQuest
{
    public class ClassQuestApiConfig : MonoBehaviour
    {
        public const string ServerIpPrefsKey = "ClassQuestServerIP";

        private const string DefaultDevelopmentServerIp = "192.168.1.76";
        private const int DefaultPort = 4000;
        private static readonly Regex Ipv4Regex = new Regex(@"^(\d{1,3}\.){3}\d{1,3}$", RegexOptions.Compiled);
        private static readonly string DefaultDevelopmentBaseUrl = BuildBaseUrl(DefaultDevelopmentServerIp);

        [SerializeField] private string baseUrl = DefaultDevelopmentBaseUrl;
        [SerializeField] private int timeoutSeconds = 8;

        public string BaseUrl => NormalizeBaseUrl(baseUrl);
        public int TimeoutSeconds => Mathf.Max(1, timeoutSeconds);

        public static string CurrentBaseUrl
        {
            get
            {
                if (TryGetSavedServerIp(out string savedServerIp))
                {
                    return BuildBaseUrl(savedServerIp);
                }

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

        public static string GetSavedOrDefaultServerIp()
        {
            if (TryGetSavedServerIp(out string savedServerIp))
            {
                return savedServerIp;
            }

            ClassQuestApiConfig config = FindAnyObjectByType<ClassQuestApiConfig>();
            if (config && TryExtractServerIp(config.BaseUrl, out string configuredIp))
            {
                return configuredIp;
            }

            return DefaultDevelopmentServerIp;
        }

        public static bool TryApplyServerIp(string serverIp, out string appliedBaseUrl, out string validationError)
        {
            appliedBaseUrl = string.Empty;
            validationError = string.Empty;

            if (!IsValidServerIp(serverIp))
            {
                validationError = "VALID SERVER IP REQUIRED";
                return false;
            }

            string normalizedIp = serverIp.Trim();
            appliedBaseUrl = BuildBaseUrl(normalizedIp);

            PlayerPrefs.SetString(ServerIpPrefsKey, normalizedIp);
            PlayerPrefs.Save();

            ClassQuestApiConfig config = FindAnyObjectByType<ClassQuestApiConfig>();
            if (config)
            {
                config.baseUrl = appliedBaseUrl;
            }

            return true;
        }

        public static bool IsValidServerIp(string serverIp)
        {
            if (string.IsNullOrWhiteSpace(serverIp))
            {
                return false;
            }

            string value = serverIp.Trim();
            if (!Ipv4Regex.IsMatch(value))
            {
                return false;
            }

            string[] parts = value.Split('.');
            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int octet) || octet < 0 || octet > 255)
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizeBaseUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultDevelopmentBaseUrl;
            }

            return value.Trim().TrimEnd('/');
        }

        private static bool TryGetSavedServerIp(out string serverIp)
        {
            serverIp = PlayerPrefs.GetString(ServerIpPrefsKey, string.Empty).Trim();
            return IsValidServerIp(serverIp);
        }

        private static bool TryExtractServerIp(string baseUrl, out string serverIp)
        {
            serverIp = string.Empty;
            string normalized = NormalizeBaseUrl(baseUrl);

            const string prefix = "http://";
            if (normalized.StartsWith(prefix))
            {
                string hostAndPort = normalized.Substring(prefix.Length);
                int colonIndex = hostAndPort.IndexOf(':');
                serverIp = colonIndex >= 0 ? hostAndPort.Substring(0, colonIndex) : hostAndPort;
                return IsValidServerIp(serverIp);
            }

            return false;
        }

        private static string BuildBaseUrl(string serverIp)
        {
            return $"http://{serverIp.Trim()}:{DefaultPort}";
        }
    }
}
