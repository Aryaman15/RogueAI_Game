using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RogueAI.ClassQuest
{
    public static class ClassQuestApiClient
    {
        public static IEnumerator GetHealth(
            Action onSuccess,
            Action<string> onFailure)
        {
            string url = $"{ClassQuestApiConfig.CurrentBaseUrl}/api/health";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = ClassQuestApiConfig.CurrentTimeoutSeconds;
            yield return SendWithWatchdog(request, url, onFailure);

            if (!request.isDone)
            {
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success || request.responseCode < 200 || request.responseCode >= 300)
            {
                onFailure?.Invoke(GetFriendlyError(request));
                yield break;
            }

            onSuccess?.Invoke();
        }

        public static IEnumerator GetMissionByCode(
            string missionCode,
            Action<ClassQuestMissionDto> onSuccess,
            Action<string> onFailure)
        {
            string url = $"{ClassQuestApiConfig.CurrentBaseUrl}/api/missions/code/{UnityWebRequest.EscapeURL(missionCode.Trim())}";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = ClassQuestApiConfig.CurrentTimeoutSeconds;
            yield return SendWithWatchdog(request, url, onFailure);

            if (!request.isDone)
            {
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFailure?.Invoke(GetFriendlyError(request));
                yield break;
            }

            if (request.responseCode < 200 || request.responseCode >= 300)
            {
                onFailure?.Invoke(request.responseCode == 404 ? "MISSION NOT FOUND" : "COULD NOT CONNECT TO CLASSQUEST");
                yield break;
            }

            ClassQuestMissionDto mission;

            try
            {
                mission = JsonUtility.FromJson<ClassQuestMissionDto>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ClassQuest mission response could not be parsed: {exception.Message}");
                onFailure?.Invoke("COULD NOT CONNECT TO CLASSQUEST");
                yield break;
            }

            if (mission == null || string.IsNullOrWhiteSpace(mission.id) || mission.challenges == null)
            {
                onFailure?.Invoke("COULD NOT CONNECT TO CLASSQUEST");
                yield break;
            }

            onSuccess?.Invoke(mission);
        }

        public static IEnumerator SubmitAttempt(ClassQuestAttemptRequest payload)
        {
            string url = $"{ClassQuestApiConfig.CurrentBaseUrl}/api/attempts";
            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.timeout = ClassQuestApiConfig.CurrentTimeoutSeconds;
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return SendWithWatchdog(request, url, error => Debug.LogWarning($"ClassQuest attempt upload failed: {error}"));

            if (!request.isDone)
            {
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success || request.responseCode < 200 || request.responseCode >= 300)
            {
                Debug.LogWarning($"ClassQuest attempt upload failed: {GetFriendlyError(request)}");
            }
        }

        private static IEnumerator SendWithWatchdog(UnityWebRequest request, string url, Action<string> onFailure)
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            float startedAt = Time.realtimeSinceStartup;
            float timeoutSeconds = ClassQuestApiConfig.CurrentTimeoutSeconds;

            while (!operation.isDone)
            {
                if (Time.realtimeSinceStartup - startedAt > timeoutSeconds)
                {
                    request.Abort();
                    string message = $"COULD NOT CONNECT TO CLASSQUEST\n{url}";
                    Debug.LogWarning($"ClassQuest request timed out after {timeoutSeconds:0.#}s: {url}");
                    onFailure?.Invoke(message);
                    yield break;
                }

                yield return null;
            }
        }

        private static string GetFriendlyError(UnityWebRequest request)
        {
            if (request.responseCode == 404)
            {
                return "MISSION NOT FOUND";
            }

            if (!string.IsNullOrWhiteSpace(request.error))
            {
                return $"COULD NOT CONNECT TO CLASSQUEST\n{request.error}";
            }

            return "COULD NOT CONNECT TO CLASSQUEST";
        }
    }
}
