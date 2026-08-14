using System.Collections;
using StarterAssets;
using UnityEngine;

namespace RogueAI.HQ
{
    public class SecurityCameraController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform player;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float fieldOfViewDegrees = 55f;
        [SerializeField] private float detectionCooldownSeconds = 1.8f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;

        [Header("Sweep")]
        [SerializeField] private Transform yawPivot;
        [SerializeField] private float sweepAngle = 55f;
        [SerializeField] private float sweepSpeed = 0.35f;
        [SerializeField] private float startingYaw;

        [Header("Visuals")]
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material disabledMaterial;
        [SerializeField] private LineRenderer fieldOfViewRenderer;

        [Header("Flow")]
        [SerializeField] private HQFlowController flowController;

        private bool active = true;
        private bool alertInProgress;
        private float nextAllowedDetectionTime;

        public bool IsActive => active;

        public void Configure(
            HQFlowController flow,
            Transform playerTransform,
            Transform pivot,
            Renderer status,
            Material activeStateMaterial,
            Material disabledStateMaterial,
            LineRenderer fovRenderer)
        {
            flowController = flow;
            player = playerTransform;
            yawPivot = pivot;
            statusRenderer = status;
            activeMaterial = activeStateMaterial;
            disabledMaterial = disabledStateMaterial;
            fieldOfViewRenderer = fovRenderer;
            ApplyVisualState();
            UpdateFieldOfViewRenderer();
        }

        private void Awake()
        {
            if (!yawPivot)
            {
                yawPivot = transform;
            }

            if (!player)
            {
                StarterAssetsInputs playerInputs = FindAnyObjectByType<StarterAssetsInputs>();
                if (playerInputs)
                {
                    player = playerInputs.transform;
                }
            }

            startingYaw = yawPivot.eulerAngles.y;
            ApplyVisualState();
            UpdateFieldOfViewRenderer();
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            UpdateSweep();
            UpdateFieldOfViewRenderer();

            if (!alertInProgress && Time.time >= nextAllowedDetectionTime && CanSeePlayer())
            {
                StartCoroutine(HandleDetectionRoutine());
            }
        }

        public void DisableCamera()
        {
            active = false;
            alertInProgress = false;
            ApplyVisualState();
        }

        private void UpdateSweep()
        {
            if (!yawPivot)
            {
                return;
            }

            float t = Mathf.PingPong(Time.time * sweepSpeed, 1f);
            float yaw = startingYaw + Mathf.Lerp(-sweepAngle * 0.5f, sweepAngle * 0.5f, t);
            yawPivot.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private bool CanSeePlayer()
        {
            if (!player || !yawPivot)
            {
                return false;
            }

            Vector3 origin = yawPivot.position;
            Vector3 target = player.position + Vector3.up;
            Vector3 toPlayer = target - origin;
            float distance = toPlayer.magnitude;

            if (distance > detectionRange)
            {
                return false;
            }

            Vector3 direction = toPlayer.normalized;
            float angle = Vector3.Angle(yawPivot.forward, direction);
            if (angle > fieldOfViewDegrees * 0.5f)
            {
                return false;
            }

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, detectionRange, lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return hit.collider.GetComponentInParent<StarterAssetsInputs>() != null;
        }

        private IEnumerator HandleDetectionRoutine()
        {
            alertInProgress = true;
            nextAllowedDetectionTime = Time.time + detectionCooldownSeconds;

            if (flowController)
            {
                flowController.HandleSecurityCameraDetection(this);
            }

            yield return new WaitForSeconds(detectionCooldownSeconds);
            alertInProgress = false;
        }

        private void ApplyVisualState()
        {
            if (statusRenderer)
            {
                Material targetMaterial = active ? activeMaterial : disabledMaterial;
                if (targetMaterial)
                {
                    statusRenderer.sharedMaterial = targetMaterial;
                }
            }

            if (fieldOfViewRenderer)
            {
                fieldOfViewRenderer.enabled = active;
                fieldOfViewRenderer.startColor = active ? new Color(1f, 0.1f, 0.05f, 0.75f) : new Color(0.2f, 1f, 0.85f, 0.35f);
                fieldOfViewRenderer.endColor = fieldOfViewRenderer.startColor;
            }
        }

        private void UpdateFieldOfViewRenderer()
        {
            if (!fieldOfViewRenderer || !yawPivot)
            {
                return;
            }

            fieldOfViewRenderer.positionCount = 3;
            fieldOfViewRenderer.useWorldSpace = true;

            Vector3 origin = yawPivot.position + Vector3.down * 1.35f;
            Quaternion leftRotation = Quaternion.AngleAxis(-fieldOfViewDegrees * 0.5f, Vector3.up);
            Quaternion rightRotation = Quaternion.AngleAxis(fieldOfViewDegrees * 0.5f, Vector3.up);
            Vector3 left = origin + leftRotation * yawPivot.forward * detectionRange;
            Vector3 right = origin + rightRotation * yawPivot.forward * detectionRange;

            fieldOfViewRenderer.SetPosition(0, origin);
            fieldOfViewRenderer.SetPosition(1, left);
            fieldOfViewRenderer.SetPosition(2, right);
        }
    }
}
