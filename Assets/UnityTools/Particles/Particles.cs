﻿using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Particles {

    public static class ParticlesTools
    {
        public static bool IsPlaying (this ParticleSystem ps) {
            ParticleSystem[] children = GetParticlesChildren(ps);
            for (int i = 0; i < children.Length; i++) {
                if (children[i].isPlaying) {
                    return true;
                }
            }
            return false;
        }
            
        static Dictionary<int, ParticleSystem[]> particle2Children = new Dictionary<int, ParticleSystem[]>();
        public static ParticleSystem Play (this ParticleSystem ps, ParticlesFX particlesFX, float scaleMultiplier) {
            ps.transform.localScale = Vector3.one * particlesFX.size * scaleMultiplier;

            ParticleSystem[] particlesChildren = GetParticlesChildren(ps);
            
            for (int i = 0; i < particlesChildren.Length; i++) {
                var main = particlesChildren[i].main;
                main.simulationSpeed = particlesFX.playbackSpeed;
            }

            for (int i = 0; i < particlesChildren.Length; i++) {
                particlesChildren[i].Play(false);
            }
            return ps;
        }

        static ParticleSystem[] GetParticlesChildren (ParticleSystem ps) {
            int instanceID = ps.GetInstanceID();

            ParticleSystem[] particlesChildren;
            if (particle2Children.TryGetValue(instanceID, out particlesChildren)) {
                particlesChildren = ps.GetComponentsInChildren<ParticleSystem>();
                particle2Children[instanceID] = particlesChildren;
            }
            return particlesChildren;
        }

        static PrefabPool<ParticleSystem> pool = new PrefabPool<ParticleSystem>();
        static void OnParticleSystemPrefabCreate (ParticleSystem ps) {
            ps.gameObject.AddComponent<PooledParticleSystem>();
        }
        public static ParticleSystem Play (ParticlesFX particlesFX, Vector3 position, Quaternion rotation, float scaleMultiplier) {
            ParticleSystem ps = pool.GetAvailable(particlesFX.particle, null, true, position, rotation, OnParticleSystemPrefabCreate, null);
            return ps.Play(particlesFX, scaleMultiplier);
        }
        public static ParticleSystem Play (ParticlesFX particlesFX, Transform parent, Vector3 localPosition, Quaternion localRotation, float scaleMultiplier) {
            ParticleSystem ps = pool.GetAvailable(particlesFX.particle, parent, true, localPosition, localRotation, OnParticleSystemPrefabCreate, null);
            return ps.Play(particlesFX, scaleMultiplier);
        }
    }

    class PooledParticleSystem : MonoBehaviour {
        ParticleSystem ps;
        void Update () {
            if (!gameObject.GetComponentIfNull<ParticleSystem>(ref ps, true).IsPlaying()) {
                transform.SetParent(null);
                gameObject.SetActive(false);
            }
        }
    }

}
