// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.EditorTools;

namespace UnityTools {

    /*
        Modifier Entry Points:          Runtime Subjects:
        EffectRadius                    "Caster", "EffectCollection",
        EffectMagnitude                 "Caster", "AffectedObject", "EffectCollection", "Effect",
        EffectDuration                  "Caster", "AffectedObject", "EffectCollection", "Effect",
    */

    public abstract class GameEffect : ScriptableObject
    {
        [NeatArray("Subjects: 'Caster', 'AffectedObject'")] public Conditions conditions;
        [NeatArray] public NeatStringList keywords;
        
        public bool HasKeyword (string keyWord) {
            return keywords.list.Contains(keyWord);
        }
        
        public abstract string GetDescription (float magnitude, float duration);
        protected abstract bool PlayerOnly();
        protected abstract bool CreateCopy();
        protected abstract bool AddToEffectsList();
        
        public static void TriggerEffectsOnSelf (GameEffectCollection effects, DynamicObject caster) {
            TriggerEffectsOnSelf(effects, caster, caster.transform.position);
        }
        public static void TriggerEffectsOnSelf (GameEffectCollection effects, DynamicObject caster, Vector3 pos) {
            TriggerEffectsOnObject(effects, caster, pos, caster);
        }
        public static void TriggerEffectsOnObject (GameEffectCollection effects, DynamicObject caster, DynamicObject obj) {
            TriggerEffectsOnObject(effects, caster, obj.GetPosition(), obj);
        }
        public static void TriggerEffectsOnObject (GameEffectCollection effects, DynamicObject caster, Vector3 pos, DynamicObject obj) {
            TriggerEffects (effects, caster, pos, obj, true);
        }

        static void RetriggerEffectsForArea (GameEffectCollection effects, DynamicObject caster, Vector3 origin, float radius, LayerMask mask, bool checkLOSOnRadiusCast, bool isCast) {

            HashSet<DynamicObject> handledObjects = new HashSet<DynamicObject>();
            Collider[] colliders = Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore);
            
            foreach (Collider c in colliders) {

                DynamicObject obj = c.GetComponentInParent<DynamicObject>();
                if (obj == null)
                    continue;
                if (handledObjects.Contains(obj)) 
                    continue;
                
                if (!RaycastToCollider(checkLOSOnRadiusCast, c, origin, radius, mask, out Vector3 pos))
                    continue;

                handledObjects.Add(obj);
                TriggerEffects (effects, caster, pos, obj, isCast, true);
                
            }
        }
        static bool RaycastToCollider(bool checkLOS, Collider toCollider, Vector3 origin, float radius, LayerMask mask, out Vector3 hitPos) {
            Vector3 colliderPos = toCollider.transform.position;
            Vector3 dir = (colliderPos - origin);

            hitPos = Vector3.zero;
                
            RaycastHit[] multiHitInfo = Physics.RaycastAll(origin, dir, radius, mask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(multiHitInfo, (x,y) => x.distance.CompareTo(y.distance));
            for (int i = 0; i < multiHitInfo.Length; i++) {
                
                Collider potentialBlocker = multiHitInfo[i].collider;
                if (potentialBlocker == toCollider) {
                    hitPos = multiHitInfo[i].point;
                    return true;
                }

                if (checkLOS) {
                    if (potentialBlocker.gameObject.isStatic) 
                        return false;
                    
                    Rigidbody attachedRigidbody = potentialBlocker.attachedRigidbody;
                    if (attachedRigidbody != null && attachedRigidbody.isKinematic) 
                        return false;
                }
            }
            hitPos = colliderPos;
            return true;
        }

        // only workds for radius
        public static void TriggerEffectsAtPosition (GameEffectCollection effects, DynamicObject caster, Vector3 pos) {
            if (effects.cast.radius <= 0) {
                Debug.LogError(effects.name + " effects collection cant be triggered at position wihtout a target, radius <= 0");
                return;
            }

            TriggerEffects (effects, caster, pos, null, true);
        }

        public static void TriggerEffects (GameEffectCollection effects, DynamicObject caster, Vector3 pos, DynamicObject obj, bool isCast, bool isFromRadiusRecast=false) {
            ModifierEntryPoints casterEntryPoints = null; 
            if (caster != null)
                casterEntryPoints = caster.GetObjectScript<ModifierEntryPoints>();
            
            EffectContext context = isCast ? effects.cast : effects.dispell;

            float radius = context.radius;
            if (!isFromRadiusRecast && radius > 0) {
                
                if (casterEntryPoints != null)
                    radius = casterEntryPoints.ModifyValue ("EffectRadius", radius, new Dictionary<string, object>() { { "Caster", caster }, { "EffectCollection", effects } });
                    
                RetriggerEffectsForArea (effects, caster, pos, radius, context.mask, context.checkLOSOnRadiusCast, isCast);
                return;
            }

            if (obj == null)
                return;

            GameEffectsHandler targetEffectsHandler = obj.GetObjectScript<GameEffectsHandler>();
            if (targetEffectsHandler != null && targetEffectsHandler.IsAffectedBy(effects))
                return;
            
            Dictionary<string, object> epRunSubjects = new Dictionary<string, object>() { { "Caster", caster }, { "AffectedObject", obj }, { "EffectCollection", effects }, { "Effect", null } };
            Dictionary<string, object> runSubjects = new Dictionary<string, object>() { { "Caster", caster }, { "AffectedObject", obj } };
            
            List<GameEffect> effectsToHold = new List<GameEffect>();
            List<float> magnitudes = new List<float>();
            List<float> durations = new List<float>();
            for (int i = 0; i < context.effects.Length; i++) {
                GameEffectItem effectItem = context.effects[i];
                GameEffect effect = effectItem.effect;

                if (effect.PlayerOnly() && !obj.isPlayer) 
                    continue;

                bool needsToBeAdded = effect.AddToEffectsList();

                if (needsToBeAdded) {
                    if (isCast) {
                        if (targetEffectsHandler == null) {
                            Debug.LogWarning("'" + obj.name + "' Doesnt have a GameEffectsHandler, and game effect '" + effect.name + " Needs To Be Kept In An Effect Handler List");
                            continue;
                        }
                    }
                    else {
                        Debug.LogWarning("Game effect '" + effect.name + " Needs To Be Kept In An Effect Handler List, So It Cant Be Used As a Dispell Effect....");
                        continue;
                    }
                }
                if (!effect.EffectValid (caster, obj))
                    continue;

                if (!Conditions.ConditionsMet(effectItem.conditions, runSubjects))
                    continue;
                if (!Conditions.ConditionsMet(effect.conditions, runSubjects))
                    continue;

                float magnitude = effectItem.magnitude;
                float duration = effectItem.duration;
                if (casterEntryPoints != null) {
                    epRunSubjects["Effect"] = effectItem.effect;
                    magnitude = casterEntryPoints.ModifyValue ("EffectMagnitude", magnitude, epRunSubjects);
                    if (duration > 0)
                        duration = casterEntryPoints.ModifyValue ("EffectDuration", duration, epRunSubjects);
                }

                
                /*
                    TODO: figure out garbage created by create copy
                */
                // if dispell we should already be out by now if needsToBeAdded...
                if (needsToBeAdded && effect.CreateCopy()) 
                    effect = Object.Instantiate(effect);
            
                // in case there was a problem with the effect start
                // return out before we add the effect possibly
                if (!effect.OnEffectStart (caster, obj, magnitude, duration))
                    continue;
                
                if (needsToBeAdded) {
                    effectsToHold.Add(effect);
                    magnitudes.Add(magnitude);
                    durations.Add(duration);
                }
            }

            if (effectsToHold.Count > 0)
                targetEffectsHandler.AddEffectsToList(effects, caster, effectsToHold, magnitudes, durations);

            // TODO: choose scheme for msg...
            if (obj.isPlayer) 
                if (!string.IsNullOrEmpty(context.message)) 
                    UIEvents.ShowMessage(0, context.message, false, UIColorScheme.Normal, false);
        }



















            

        protected abstract bool EffectValid (DynamicObject caster, DynamicObject obj); 
        protected abstract bool OnEffectStart (DynamicObject caster, DynamicObject obj, float magnitude, float duration); 
        protected abstract void OnEffectUpdate (DynamicObject caster, DynamicObject obj, float deltaTime, float timeAdded, float currentTime, float magnitude, float duration, out bool removeEffect);
        public abstract void OnEffectRemove (DynamicObject caster, DynamicObject obj, float magnitude, float duration);

        // called by actor or effects holder
        public void UpdateEffect (DynamicObject caster, DynamicObject obj, float deltaTime, float timeAdded, float currentTime, float magnitude, float duration, out bool removeEffect) {

            removeEffect = false;
            if (duration > 0) 
                if (currentTime - timeAdded >= duration) 
                    removeEffect = true;
                
            if (!removeEffect) 
                OnEffectUpdate (caster, obj, deltaTime, timeAdded, currentTime, magnitude, duration, out removeEffect);
        }   
    }
}