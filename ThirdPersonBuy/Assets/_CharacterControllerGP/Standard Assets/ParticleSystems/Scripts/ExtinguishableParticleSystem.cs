using System;
using UnityEngine;


namespace UnityStandardAssets.Effects
{
    public class ExtinguishableParticleSystem : MonoBehaviour
    {
        public float multiplier = 1;

        private ParticleSystem[] m_Systems;


        private void Start()
        {
            m_Systems = GetComponentsInChildren<ParticleSystem>();
        }


        public void Extinguish()
        {
            foreach (var system in m_Systems)
            {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
                var emission = system.emission;
                emission.enabled = false;
#else
                system.enableEmission = false;
#endif
            }
        }
    }
}
