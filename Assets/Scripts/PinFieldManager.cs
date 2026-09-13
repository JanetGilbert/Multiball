using System.Collections.Generic;
using UnityEngine;

namespace MultiballVR
{
    /// <summary>
    /// Spawns bowling pins evenly distributed across the entire floor area.
    /// </summary>
    public class PinFieldManager : MonoBehaviour
    {
        [Header("Pin Prefab & Count")]
        [SerializeField] private GameObject m_PinPrefab;
        [Range(10, 30)]
        [SerializeField] private int m_PinCount = 16;

        [Header("Floor Bounds")]
        [Tooltip("Floor X bounds (Left to Right).")]
        [SerializeField] private Vector2 m_RangeX = new Vector2(-10.0f, 10.0f);

        [Tooltip("Floor Z bounds (Front to Back).")]
        [SerializeField] private Vector2 m_RangeZ = new Vector2(-3.5f, 4.2f);

        [Tooltip("Floor Y position for pin base.")]
        [SerializeField] private float m_FloorY = 0f;

        [Tooltip("Minimum distance between pins.")]
        [SerializeField] private float m_MinSpacing = 2.2f;

        [Header("Player Exclusion Zone")]
        [Tooltip("Position to avoid spawning directly on top of.")]
        [SerializeField] private Vector3 m_ExclusionCenter = Vector3.zero;
        [Tooltip("Radius around exclusion center where pins won't spawn.")]
        [SerializeField] private float m_ExclusionRadius = 1.3f;

        [Header("Timing")]
        [Tooltip("Reset delay passed to each pin (default 120s = 2 minutes).")]
        [SerializeField] private float m_PinResetDelay = 120f;

        [Header("Runtime Options")]
        [SerializeField] private bool m_SpawnOnStart = true;

        private readonly List<GameObject> m_SpawnedPins = new List<GameObject>();

        public List<GameObject> SpawnedPins => m_SpawnedPins;

        private void Start()
        {
            if (m_SpawnOnStart)
            {
                GeneratePinField();
            }
        }

        [ContextMenu("Generate Pin Field")]
        public void GeneratePinField()
        {
            ClearPins();

            if (m_PinPrefab == null)
            {
                Debug.LogWarning("[PinFieldManager] No pin prefab assigned!");
                return;
            }

            // Generate evenly spaced points using a jittered grid
            List<Vector3> candidatePositions = GenerateEvenGridPositions();
            List<Vector3> placedPositions = new List<Vector3>();

            // Shuffle candidates
            for (int i = 0; i < candidatePositions.Count; i++)
            {
                int r = Random.Range(i, candidatePositions.Count);
                Vector3 temp = candidatePositions[i];
                candidatePositions[i] = candidatePositions[r];
                candidatePositions[r] = temp;
            }

            foreach (var candidate in candidatePositions)
            {
                if (placedPositions.Count >= m_PinCount) break;

                // Check exclusion zone (player area)
                if (Vector2.Distance(new Vector2(candidate.x, candidate.z), new Vector2(m_ExclusionCenter.x, m_ExclusionCenter.z)) < m_ExclusionRadius)
                {
                    continue;
                }

                // Check distance against already placed pins
                bool farEnough = true;
                foreach (var pos in placedPositions)
                {
                    if (Vector2.Distance(new Vector2(candidate.x, candidate.z), new Vector2(pos.x, pos.z)) < m_MinSpacing)
                    {
                        farEnough = false;
                        break;
                    }
                }

                if (farEnough)
                {
                    placedPositions.Add(candidate);
                }
            }

            // Instantiate pins
            foreach (var pos in placedPositions)
            {
                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject pin = Instantiate(m_PinPrefab, pos, rot, transform);

                if (pin.TryGetComponent(out BowlingPinController controller))
                {
                    controller.InitialPosition = pos;
                    controller.InitialRotation = rot;
                    controller.ResetDelay = m_PinResetDelay;
                }

                m_SpawnedPins.Add(pin);
            }
        }

        private List<Vector3> GenerateEvenGridPositions()
        {
            List<Vector3> positions = new List<Vector3>();

            float totalWidth = m_RangeX.y - m_RangeX.x;
            float totalDepth = m_RangeZ.y - m_RangeZ.x;

            int cols = Mathf.Max(3, Mathf.RoundToInt(totalWidth / m_MinSpacing));
            int rows = Mathf.Max(2, Mathf.RoundToInt(totalDepth / m_MinSpacing));

            float cellW = totalWidth / cols;
            float cellD = totalDepth / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float baseX = m_RangeX.x + (c + 0.5f) * cellW;
                    float baseZ = m_RangeZ.x + (r + 0.5f) * cellD;

                    // Add slight jitter within cell
                    float jitterX = (Random.value - 0.5f) * cellW * 0.4f;
                    float jitterZ = (Random.value - 0.5f) * cellD * 0.4f;

                    positions.Add(new Vector3(baseX + jitterX, m_FloorY, baseZ + jitterZ));
                }
            }

            return positions;
        }

        public void ClearPins()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
            m_SpawnedPins.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
            Vector3 center = new Vector3((m_RangeX.x + m_RangeX.y) * 0.5f, m_FloorY + 0.5f, (m_RangeZ.x + m_RangeZ.y) * 0.5f);
            Vector3 size = new Vector3(m_RangeX.y - m_RangeX.x, 1f, m_RangeZ.y - m_RangeZ.x);
            Gizmos.DrawCube(center, size);
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_ExclusionCenter, m_ExclusionRadius);
        }
    }
}

