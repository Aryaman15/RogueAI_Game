using System.Collections;
using UnityEngine;

namespace RogueAI.Level
{
    public class DoorController : MonoBehaviour
    {
        public enum DoorState
        {
            Locked,
            Unlocked,
            Opening,
            Open
        }

        [SerializeField] private Transform movingDoor;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 3.2f, 0f);
        [SerializeField] private float openSeconds = 1.4f;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private DoorState state = DoorState.Locked;

        public DoorState State => state;

        public void Configure(Transform doorPanel, TextMesh label)
        {
            movingDoor = doorPanel;
            statusLabel = label;
            CachePositions();
        }

        private void Awake()
        {
            CachePositions();
        }

        public void Lock()
        {
            CachePositions();
            state = DoorState.Locked;
            if (movingDoor)
            {
                movingDoor.position = closedPosition;
                movingDoor.gameObject.SetActive(true);
            }

            SetStatus("SECURITY DOOR\nPOWER OFFLINE");
        }

        public IEnumerator UnlockAndOpen()
        {
            if (!movingDoor || state == DoorState.Open || state == DoorState.Opening)
            {
                yield break;
            }

            state = DoorState.Unlocked;
            yield return Open();
        }

        private IEnumerator Open()
        {
            state = DoorState.Opening;

            float elapsed = 0f;
            Vector3 start = movingDoor.position;
            while (elapsed < openSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / openSeconds));
                movingDoor.position = Vector3.Lerp(start, openPosition, t);
                yield return null;
            }

            movingDoor.position = openPosition;
            state = DoorState.Open;
            SetStatus("SECURITY ACCESS\nAVAILABLE");
        }

        private void CachePositions()
        {
            if (!movingDoor)
            {
                return;
            }

            closedPosition = movingDoor.position;
            openPosition = closedPosition + openOffset;
        }

        private void SetStatus(string message)
        {
            if (statusLabel)
            {
                statusLabel.text = message;
            }
        }
    }
}
