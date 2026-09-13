using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MultiballVR
{
    /// <summary>
    /// Handles splitting the ball into smaller balls upon high-velocity impact with target surfaces (walls).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallImpactSplitter : MonoBehaviour
    {
        [Header("Target Detection")]
        [Tooltip("If true, only surfaces with the target tag trigger splitting.")]
        [SerializeField] private bool m_RequireTargetTag = true;
        [Tooltip("Tag of the wall or surface that triggers ball splitting.")]
        [SerializeField] private string m_TargetTag = "Wall";

        [Header("Impact Thresholds & Count")]
        [Tooltip("Minimum impact velocity (m/s) required to trigger a split.")]
        [SerializeField] private float m_MinImpactSpeed = 2.5f;
        [Tooltip("Impact velocity (m/s) that produces the maximum split count.")]
        [SerializeField] private float m_MaxImpactSpeed = 12.0f;
        [Tooltip("Minimum number of child balls spawned when split threshold is reached.")]
        [SerializeField] private int m_MinSplitCount = 2;
        [Tooltip("Maximum number of child balls spawned at maximum impact velocity.")]
        [SerializeField] private int m_MaxSplitCount = 8;

        [Header("Generation & Scaling")]
        [Tooltip("Scale factor applied to child balls relative to this ball.")]
        [SerializeField] private float m_ScaleMultiplier = 0.65f;
        [Tooltip("Current generation of this ball. 0 is the initial spawned ball.")]
        [SerializeField] private int m_CurrentGeneration = 0;
        [Tooltip("Maximum generations allowed. Balls at max generation will not split further.")]
        [SerializeField] private int m_MaxGenerations = 2;
        [Tooltip("Prefab to instantiate for child balls. If null, this ball's GameObject is used.")]
        [SerializeField] private GameObject m_ChildBallPrefab;

        [Header("Ejection Physics")]
        [Tooltip("Cone half-angle (in degrees) for dispersing child balls around the reflection normal.")]
        [SerializeField] private float m_SpreadAngle = 35f;
        [Tooltip("Multiplier applied to the incoming impact speed for child ball launch velocity.")]
        [SerializeField] private float m_VelocityMultiplier = 0.85f;
        [Tooltip("Additional outward burst speed added to child balls.")]
        [SerializeField] private float m_AdditionalBurstSpeed = 1.5f;

        [Header("Feedback & Effects")]
        [SerializeField] private GameObject m_SplitVFXPrefab;
        [SerializeField] private AudioClip m_SplitSound;
        [Range(0f, 1f)]
        [SerializeField] private float m_SoundVolume = 0.8f;

        [Header("Lifetime & Cleanup")]
        [Tooltip("Lifetime in seconds for child balls. Set to 0 to disable auto-despawn.")]
        [SerializeField] private float m_ChildLifetime = 20f;

        private Rigidbody m_Rigidbody;
        private XRGrabInteractable m_GrabInteractable;
        private bool m_HasSplit = false;
        private float m_SpawnTime;

        public int CurrentGeneration
        {
            get => m_CurrentGeneration;
            set => m_CurrentGeneration = value;
        }

        public GameObject ChildBallPrefab
        {
            get => m_ChildBallPrefab;
            set => m_ChildBallPrefab = value;
        }

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_GrabInteractable = GetComponent<XRGrabInteractable>();
            m_SpawnTime = Time.time;
        }

        private void Start()
        {
            // Auto-despawn child balls after their lifetime
            if (m_CurrentGeneration > 0 && m_ChildLifetime > 0f)
            {
                Destroy(gameObject, m_ChildLifetime);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (m_HasSplit) return;

            // Do not split if the ball is currently held in hand
            if (m_GrabInteractable != null && m_GrabInteractable.isSelected)
            {
                return;
            }

            // Check tag if required
            if (m_RequireTargetTag && !string.IsNullOrEmpty(m_TargetTag))
            {
                if (!collision.gameObject.CompareTag(m_TargetTag) && 
                    (collision.transform.parent == null || !collision.transform.parent.CompareTag(m_TargetTag)))
                {
                    return;
                }
            }

            // Check generation limit
            if (m_CurrentGeneration >= m_MaxGenerations)
            {
                return;
            }

            // Determine impact speed
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < m_MinImpactSpeed)
            {
                return;
            }

            PerformSplit(collision, impactSpeed);
        }

        private void PerformSplit(Collision collision, float impactSpeed)
        {
            m_HasSplit = true;

            // Calculate child count based on impact speed
            float speedFactor = Mathf.Clamp01((impactSpeed - m_MinImpactSpeed) / Mathf.Max(0.1f, m_MaxImpactSpeed - m_MinImpactSpeed));
            int splitCount = Mathf.RoundToInt(Mathf.Lerp(m_MinSplitCount, m_MaxSplitCount, speedFactor));

            // Contact point and normal
            ContactPoint contact = collision.GetContact(0);
            Vector3 contactPoint = contact.point;
            Vector3 contactNormal = contact.normal;

            // Reflection direction based on incoming relative velocity
            Vector3 incomingDir = -collision.relativeVelocity.normalized;
            Vector3 reflectDir = Vector3.Reflect(incomingDir, contactNormal).normalized;
            if (reflectDir == Vector3.zero || Vector3.Dot(reflectDir, contactNormal) < 0f)
            {
                reflectDir = contactNormal;
            }

            // Visual effect
            if (m_SplitVFXPrefab != null)
            {
                GameObject vfx = Instantiate(m_SplitVFXPrefab, contactPoint + contactNormal * 0.05f, Quaternion.LookRotation(contactNormal));
                Destroy(vfx, 3f);
            }

            // Audio feedback
            PlaySplitAudio(contactPoint, speedFactor);

            // Spawn child balls
            GameObject prefabToSpawn = m_ChildBallPrefab != null ? m_ChildBallPrefab : gameObject;
            Vector3 baseChildScale = m_ChildBallPrefab != null ? m_ChildBallPrefab.transform.localScale : (transform.localScale * m_ScaleMultiplier);
            float childLaunchSpeed = (impactSpeed * m_VelocityMultiplier) + m_AdditionalBurstSpeed;

            for (int i = 0; i < splitCount; i++)
            {
                // Calculate dispersed trajectory within spread cone
                Quaternion randomSpread = Quaternion.AngleAxis(Random.Range(0f, 360f), reflectDir) *
                                         Quaternion.AngleAxis(Random.Range(5f, m_SpreadAngle), Vector3.Cross(reflectDir, Vector3.up).normalized == Vector3.zero ? Vector3.right : Vector3.Cross(reflectDir, Vector3.up).normalized);
                Vector3 spawnDir = (randomSpread * reflectDir).normalized;

                // Position offset slightly from wall contact to avoid clipping
                Vector3 spawnPos = contactPoint + contactNormal * 0.1f + Random.insideUnitSphere * 0.05f;

                GameObject child = Instantiate(prefabToSpawn, spawnPos, Random.rotation);
                child.transform.localScale = baseChildScale;

                // Configure child components
                if (child.TryGetComponent(out Rigidbody childRb))
                {
                    childRb.linearVelocity = spawnDir * (childLaunchSpeed * Random.Range(0.85f, 1.15f));
                    childRb.angularVelocity = Random.insideUnitSphere * 15f;
                }

                if (child.TryGetComponent(out BallImpactSplitter childSplitter))
                {
                    childSplitter.CurrentGeneration = m_CurrentGeneration + 1;
                    // If the instantiated child didn't already have a child prefab configured, pass down or use own
                    if (childSplitter.ChildBallPrefab == null && m_ChildBallPrefab == null)
                    {
                        childSplitter.ChildBallPrefab = gameObject;
                    }
                }
            }

            // Destroy the parent ball
            Destroy(gameObject);
        }

        private void PlaySplitAudio(Vector3 position, float intensity)
        {
            if (m_SplitSound != null)
            {
                AudioSource.PlayClipAtPoint(m_SplitSound, position, Mathf.Lerp(0.5f, 1.0f, intensity) * m_SoundVolume);
            }
        }
    }
}
