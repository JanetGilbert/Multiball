using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MultiballVR
{
    /// <summary>
    /// Spawns interactive balls on a pedestal and automatically dispenses replacements when picked up.
    /// </summary>
    public class BallSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [Tooltip("The ball prefab to spawn.")]
        [SerializeField] private GameObject m_BallPrefab;
        [Tooltip("Transform representing the spawn point. Defaults to this transform if null.")]
        [SerializeField] private Transform m_SpawnPoint;
        [Tooltip("Delay in seconds before spawning a new ball after the previous one is removed.")]
        [SerializeField] private float m_RespawnDelay = 1.2f;
        [Tooltip("Distance from spawn point before ball is considered taken.")]
        [SerializeField] private float m_PickupDistanceThreshold = 0.35f;

        [Header("Initial Setup")]
        [Tooltip("Whether to spawn a ball on Start.")]
        [SerializeField] private bool m_SpawnOnStart = true;

        [Header("Audio & Visuals")]
        [SerializeField] private AudioClip m_SpawnSound;
        [Range(0f, 1f)]
        [SerializeField] private float m_SoundVolume = 0.6f;
        [SerializeField] private ParticleSystem m_SpawnVFX;

        private GameObject m_CurrentBall;
        private XRGrabInteractable m_CurrentGrabInteractable;
        private bool m_IsWaitingToRespawn = false;
        private float m_RespawnTimer = 0f;

        public GameObject BallPrefab
        {
            get => m_BallPrefab;
            set => m_BallPrefab = value;
        }

        public Transform SpawnPoint
        {
            get => m_SpawnPoint != null ? m_SpawnPoint : transform;
            set => m_SpawnPoint = value;
        }

        private void Start()
        {
            if (m_SpawnPoint == null)
            {
                m_SpawnPoint = transform;
            }

            if (m_SpawnOnStart)
            {
                SpawnBall();
            }
        }

        private void Update()
        {
            if (m_CurrentBall == null)
            {
                if (!m_IsWaitingToRespawn)
                {
                    StartRespawnCountdown();
                }
            }
            else
            {
                // Check if current ball was picked up or moved away
                bool isGrabbed = m_CurrentGrabInteractable != null && m_CurrentGrabInteractable.isSelected;
                float distance = Vector3.Distance(m_CurrentBall.transform.position, SpawnPoint.position);

                if (isGrabbed || distance > m_PickupDistanceThreshold)
                {
                    // Ball was taken! Detach from tracking and schedule respawn
                    m_CurrentBall = null;
                    m_CurrentGrabInteractable = null;
                    StartRespawnCountdown();
                }
            }

            // Handle countdown
            if (m_IsWaitingToRespawn)
            {
                m_RespawnTimer -= Time.deltaTime;
                if (m_RespawnTimer <= 0f)
                {
                    m_IsWaitingToRespawn = false;
                    SpawnBall();
                }
            }
        }

        public void SpawnBall()
        {
            if (m_BallPrefab == null)
            {
                Debug.LogWarning("[BallSpawner] No ball prefab assigned!");
                return;
            }

            Vector3 spawnPos = SpawnPoint.position;
            Quaternion spawnRot = SpawnPoint.rotation;

            m_CurrentBall = Instantiate(m_BallPrefab, spawnPos, spawnRot);
            m_CurrentGrabInteractable = m_CurrentBall.GetComponent<XRGrabInteractable>();

            if (m_SpawnVFX != null)
            {
                m_SpawnVFX.Play();
            }

            if (m_SpawnSound != null)
            {
                AudioSource.PlayClipAtPoint(m_SpawnSound, spawnPos, m_SoundVolume);
            }
        }

        private void StartRespawnCountdown()
        {
            m_IsWaitingToRespawn = true;
            m_RespawnTimer = m_RespawnDelay;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 pos = m_SpawnPoint != null ? m_SpawnPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, m_PickupDistanceThreshold);
        }
    }
}
