using System.Collections.Generic;
using UnityEngine;
namespace UnityTools {
    public class GameEffectsHandler : DynamicObjectScript<GameEffectsHandler> {
        class GEUpdater {
            float magnitude, duration, timeAdded;
            GameEffect effect;

            public string GetDescription () {
                return effect.GetDescription(magnitude, duration);
            }

            public DynamicObject caster;
            public GEUpdater (DynamicObject caster, GameEffect effect, float magnitude, float duration){
                this.caster = caster;
                this.effect = effect;
                this.magnitude = magnitude;
                this.duration = duration;
                timeAdded = Time.time;
            }
            public void UpdateEffect (DynamicObject obj, float deltaTime, float currentTime, out bool removeEffect) {
                effect.UpdateEffect (caster, obj, deltaTime, timeAdded, currentTime, magnitude, duration, out removeEffect);
            }
            public void OnEffectRemove (DynamicObject obj) {
                effect.OnEffectRemove (caster, obj, magnitude, duration);
            }
        }

        Dictionary<GameEffectCollection, List<GEUpdater>> updaters = new Dictionary<GameEffectCollection, List<GEUpdater>>();

        public bool IsAffectedBy (GameEffectCollection collection) {
            return updaters.ContainsKey(collection);
        }
        public void AddEffectsToList (GameEffectCollection collection, DynamicObject caster, List<GameEffect> effects, List<float> magnitudes, List<float> durations) {
            if (updaters.ContainsKey(collection)) {
                Debug.LogWarning(name + " is already being affected by effect colleciton " + collection.name);
                return;
            }

            List<GEUpdater> u = new List<GEUpdater>();
            for (int i = 0; i < effects.Count; i++) 
                u.Add(new GEUpdater(caster, effects[i], magnitudes[i], durations[i]));
            
            updaters[collection] = u;
        }

        string GetDescription (GameEffectCollection effects) {
            string d = effects.description + ":\n";
            List<GEUpdater> updaterList = updaters[effects];
            for (int i = 0; i < updaterList.Count; i++) {
                string description = updaterList[i].GetDescription();
                if (!string.IsNullOrEmpty(description)) {
                    d += "\t" + updaterList[i].GetDescription() + "\n";
                }
            }
            return d;
        }

        List<GameEffectCollection> _effectsRemoveOnUpdate = new List<GameEffectCollection>();
        void UpdateEffects (float deltaTime, float currentTime) {
            _effectsRemoveOnUpdate.Clear();
            foreach (var k in updaters.Keys) {
                List<GEUpdater> effects = updaters[k];
                for (int i = effects.Count - 1; i >= 0; i--) {
                    effects[i].UpdateEffect (dynamicObject, deltaTime, currentTime, out bool removeEffect);
                    if (removeEffect) {
                        effects[i].OnEffectRemove(dynamicObject);
                        effects.RemoveAt(i);
                    }
                }
                if (effects.Count == 0) 
                    _effectsRemoveOnUpdate.Add(k);
            }
            RemoveEffects(_effectsRemoveOnUpdate);
        }

        void RemoveEffects (List<GameEffectCollection> effectsRemove) {
            for (int i = 0; i < effectsRemove.Count; i++) {
                GEUpdater updater = updaters[effectsRemove[i]][0];
                updaters.Remove(effectsRemove[i]);
                GameEffect.TriggerEffects (effectsRemove[i], updater.caster, dynamicObject.GetPosition(), dynamicObject, false);
            }
        }

        void Update () {
            if (GameManager.isPaused)
                return;
            UpdateEffects(Time.deltaTime, Time.time);
        }

        List<GameEffectCollection> _effectsRemoveOnRemove = new List<GameEffectCollection>();
        public void RemoveEffects (List<string> keywords) {
            _effectsRemoveOnRemove.Clear();
            // Debug.Log("Removing effects with keywords: " + string.Join(", ", keywords));
            foreach (var k in updaters.Keys) {
                bool remove = false;
                for (int j = 0; j < keywords.Count; j++) {
                    if (k.HasKeyword(keywords[j])) {
                        remove = true;
                        break;
                    }
                }
                if (remove) {
                    List<GEUpdater> effects = updaters[k];
                    for (int i = effects.Count - 1; i >= 0; i--)
                        effects[i].OnEffectRemove(dynamicObject);
                    _effectsRemoveOnRemove.Add(k);
                }
            }
            RemoveEffects(_effectsRemoveOnRemove);
        }
    }
}
