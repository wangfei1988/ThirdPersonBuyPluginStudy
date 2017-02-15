using System;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
using System.Collections.Generic;
#endif
using UnityEngine;

namespace UnityStandardAssets.Effects
{
    public class WaterHoseParticles : MonoBehaviour
    {
        public static float lastSoundTime;
        public float force = 1;

#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
        private List<ParticleCollisionEvent> m_CollisionEvents = new List<ParticleCollisionEvent>(16);
#else
        private ParticleCollisionEvent[] m_CollisionEvents = new ParticleCollisionEvent[16];
#endif
        private ParticleSystem m_ParticleSystem;


        private void Start()
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();

        }


        private void OnParticleCollision(GameObject other)
        {
            int safeLength = m_ParticleSystem.GetSafeCollisionEventSize();
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            int numColEvents = m_CollisionEvents.Count;
            if (numColEvents < safeLength)
            {
                m_CollisionEvents = new List<ParticleCollisionEvent>(safeLength);
            }
#else
            int numColEvents = m_CollisionEvents.Length;
                        if (numColEvents < safeLength)
            {
                m_CollisionEvents = new ParticleCollisionEvent[safeLength];
            }
#endif


            int numCollisionEvents = m_ParticleSystem.GetCollisionEvents(other, m_CollisionEvents);
            int i = 0;

            while (i < numCollisionEvents)
            {
                if (Time.time > lastSoundTime + 0.2f)
                {
                    lastSoundTime = Time.time;
                }
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
                var col = m_CollisionEvents[i].colliderComponent;
#else
                var col = m_CollisionEvents[i].collider;
#endif
                var attachedRigidbody = col.GetComponent<Rigidbody>();
                if (attachedRigidbody != null)
                {
                    Vector3 vel = m_CollisionEvents[i].velocity;
                    attachedRigidbody.AddForce(vel*force, ForceMode.Impulse);
                }

                other.BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);

                i++;
            }
        }
    }
}
