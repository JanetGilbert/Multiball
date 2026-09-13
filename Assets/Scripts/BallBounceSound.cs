using UnityEngine;

namespace MultiballVR
{
    /// <summary>
    /// Plays an audio cue when the ball bounces against surfaces.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallBounceSound : MonoBehaviour
    {
        [SerializeField] private AudioClip m_BounceSound;
        [SerializeField] private float m_MinSpeedForSound = 0.8f;
        [SerializeField] private float m_MaxVolumeSpeed = 8.0f;
        [Range(0f, 1f)]
        [SerializeField] private float m_BaseVolume = 0.7f;
        [SerializeField] private float m_Cooldown = 0.08f;

        private float m_LastSoundTime;

        private void OnCollisionEnter(Collision collision)
        {
            if (m_BounceSound == null) return;
            if (Time.time - m_LastSoundTime < m_Cooldown) return;

            float speed = collision.relativeVelocity.magnitude;
            if (speed < m_MinSpeedForSound) return;

            m_LastSoundTime = Time.time;
            float volume = Mathf.Clamp01(speed / m_MaxVolumeSpeed) * m_BaseVolume;
            AudioSource.PlayClipAtPoint(m_BounceSound, collision.GetContact(0).point, volume);
        }
    }
}
