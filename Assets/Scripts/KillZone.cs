using UnityEngine;

namespace MultiballVR
{
    /// <summary>
    /// Destroys objects that fall below or into this trigger zone.
    /// </summary>
    public class KillZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var splitter = other.gameObject.GetComponentInParent<BallImpactSplitter>();
            if (splitter != null)
            {
                Destroy(splitter.gameObject);
            }
        }
    }
}

