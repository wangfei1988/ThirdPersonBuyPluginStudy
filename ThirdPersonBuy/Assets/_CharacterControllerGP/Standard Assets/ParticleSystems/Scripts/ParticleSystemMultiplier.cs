using System;
using UnityEngine;

namespace UnityStandardAssets.Effects
{
    public class ParticleSystemMultiplier : MonoBehaviour
    {
        // a simple script to scale the size, speed and lifetime of a particle system

        public float multiplier = 1;


        private void Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in systems)
            {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
                ParticleSystem.MainModule mm = system.main;
                mm.startSizeMultiplier *= multiplier;
                mm.startSpeedMultiplier *= multiplier;
                mm.startLifetimeMultiplier *= Mathf.Lerp(multiplier, 1, 0.5f);
#else
                system.startSize *= multiplier;
                system.startSpeed *= multiplier;
                system.startLifetime *= Mathf.Lerp(multiplier, 1, 0.5f);
#endif
                system.Clear();
                system.Play();
            }
        }
    }
}
