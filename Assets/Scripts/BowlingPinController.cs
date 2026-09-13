using System.Collections;
using UnityEngine;

namespace MultiballVR
{
    /// <summary>
    /// Controls a bowling pin's physics, tracks when it gets knocked over,
    /// and resets/stands it back up after a configurable duration.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BowlingPinController : MonoBehaviour
    {
        [Header("Reset Settings")]
        [Tooltip("Time in seconds after being knocked down before standing back up.")]
        [SerializeField] private float m_ResetDelay = 120f; // 2 minutes

        [Tooltip("Tilt angle (in degrees from upright) to be considered knocked over.")]
        [SerializeField] private float m_KnockedAngleThreshold = 40f;

        [Tooltip("Distance moved from initial position to be considered knocked away.")]
        [SerializeField] private float m_KnockedDistanceThreshold = 0.35f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioClip m_PinHitSound;
        [Range(0f, 1f)]
        [SerializeField] private float m_HitVolume = 0.7f;
        [SerializeField] private float m_MinHitSpeed = 0.8f;

        private Vector3 m_InitialPosition;
        private Quaternion m_InitialRotation;
        private Rigidbody m_Rigidbody;

        private bool m_IsKnocked = false;
        private float m_KnockedTimer = 0f;
        private bool m_IsResetting = false;

        public Vector3 InitialPosition
        {
            get => m_InitialPosition;
            set => m_InitialPosition = value;
        }

        public Quaternion InitialRotation
        {
            get => m_InitialRotation;
            set => m_InitialRotation = value;
        }

        public float ResetDelay
        {
            get => m_ResetDelay;
            set => m_ResetDelay = value;
        }

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_InitialPosition = transform.position;
            m_InitialRotation = transform.rotation;

            // Shift center of mass lower for stable upright resting on floor
            m_Rigidbody.centerOfMass = new Vector3(0, 0.25f, 0);
        }

        private void Update()
        {
            if (m_IsResetting) return;

            if (!m_IsKnocked)
            {
                // Check if knocked over (by tilt angle or distance moved)
                float tilt = Vector3.Angle(transform.up, Vector3.up);
                float dist = Vector3.Distance(transform.position, m_InitialPosition);

                if (tilt > m_KnockedAngleThreshold || dist > m_KnockedDistanceThreshold)
                {
                    m_IsKnocked = true;
                    m_KnockedTimer = 0f;
                }
            }
            else
            {
                // Count down to reset
                m_KnockedTimer += Time.deltaTime;
                if (m_KnockedTimer >= m_ResetDelay)
                {
                    StartCoroutine(ResetPinRoutine());
                }
            }
        }

        private IEnumerator ResetPinRoutine()
        {
            m_IsResetting = true;

            // Smoothly lift and place the pin upright
            m_Rigidbody.isKinematic = true;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 targetPos = m_InitialPosition;
            Quaternion targetRot = m_InitialRotation;

            // Lift slightly first if lying down
            Vector3 liftPos = targetPos + Vector3.up * 0.2f;

            float duration = 0.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                if (t < 0.5f)
                {
                    transform.position = Vector3.Lerp(startPos, liftPos, t * 2f);
                    transform.rotation = Quaternion.Slerp(startRot, targetRot, t * 2f);
                }
                else
                {
                    transform.position = Vector3.Lerp(liftPos, targetPos, (t - 0.5f) * 2f);
                    transform.rotation = targetRot;
                }

                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;

            m_Rigidbody.isKinematic = false;
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            m_IsKnocked = false;
            m_KnockedTimer = 0f;
            m_IsResetting = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (m_PinHitSound == null) return;

            float speed = collision.relativeVelocity.magnitude;
            if (speed >= m_MinHitSpeed)
            {
                AudioSource.PlayClipAtPoint(m_PinHitSound, collision.GetContact(0).point, Mathf.Clamp01(speed / 6f) * m_HitVolume);
            }
        }

        /// <summary>
        /// Instantly stands up the pin at its initial position.
        /// </summary>
        public void InstantReset()
        {
            StopAllCoroutines();
            m_IsResetting = false;
            m_IsKnocked = false;
            m_KnockedTimer = 0f;

            transform.position = m_InitialPosition;
            transform.rotation = m_InitialRotation;

            if (m_Rigidbody != null)
            {
                m_Rigidbody.isKinematic = false;
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}
