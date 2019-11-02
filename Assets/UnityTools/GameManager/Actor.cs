﻿using System.Collections.Generic;
using UnityEngine;

using System;
using UnityTools.EditorTools;

namespace UnityTools {
    [System.Serializable] public class ActorState : SceneObjectState {
        public string prefabName;
        public bool isPlayer;
        public List<GameValue> gameValues;
        
        public ActorState (Actor instance) : base (instance)
        {
            this.prefabName = instance.basePrefabName;
            this.gameValues = instance.gameValues;
            this.isPlayer = instance.isPlayer;
        }
    }

    // used for npc's or player
    [System.Serializable] public class Actor : MonoBehaviour, ISaveableObject<ActorState>
    {

        public const string ActorPrefabsObjectName = "ActorPrefabs";

        public Vector3 GetPosition () {
            if (getPosition == null) {
                Debug.LogWarning("Actor: " + gameObject.name + " is not initialized!");
                return Vector3.zero;
            }
            return getPosition();
        }
        public Vector3 GetForward () {
            if (getForward == null) {
                Debug.LogWarning("Actor: " + gameObject.name + " is not initialized!");
                return Vector3.forward;
            }
            return getForward();
        }
        
        Func<Vector3> getPosition, getForward;
        public void InitializeActor (Func<Vector3> getPosition, Func<Vector3> getForward) {
            this.getPosition = getPosition;
            this.getForward = getForward;
        }

        [NeatArray] public GameValueList gameValues;
        Dictionary<string, GameValue> gameValuesDict;

        public void AddModifiers (GameValueModifier[] modifiers, string description, bool assertPermanent, GameObject subject, GameObject target) {
            MakeValuesDictionaryIfNull();
            GameValue.AddModifiers(gameValuesDict, modifiers, 1, description, assertPermanent, subject, target);
        }

        public void RemoveModifiers (GameValueModifier[] modifiers, string description) {
            MakeValuesDictionaryIfNull();
            GameValue.RemoveModifiers(gameValuesDict, modifiers, 1, description);
        }

        void MakeValuesDictionaryIfNull () {
            if (gameValuesDict == null) {
                gameValuesDict = new Dictionary<string, GameValue>(gameValues.Count);
            }
            
            if ((gameValuesDict.Count != gameValues.Count)) {
                gameValuesDict.Clear();
                
                for (int i = 0; i< gameValues.Count; i++) 
                    gameValuesDict[gameValues[i].name] = gameValues[i];
            }
        }

        

        public bool HasGameValue (string name) {
            MakeValuesDictionaryIfNull();
            return gameValuesDict.ContainsKey(name);
        }
        public GameValue GetGameValueObject (string name) {
            if (!HasGameValue(name)) {
                Debug.Log(this.name + " does not have a game value named "+ name);
                return null;
            }
            return gameValuesDict[name];
        }

        public float GetGameValueComponent (string name, GameValue.GameValueComponent component) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) return 0;
            return gv.GetValueComponent(component);
        }
        public float GetGameValue (string name) {
            return GetGameValueComponent(name, GameValue.GameValueComponent.Value);
        }

        



        public void LoadFromSavedObject (ActorState savedActor) {
            gameValues.CopyFrom( savedActor.gameValues );
        }

        public static PrefabPool<Actor> pool = new PrefabPool<Actor>();
        public string basePrefabName;
        public static Actor playerActor;
        public bool isPlayer;

        public void BuildGameValues (GameValue[] template) {
            for (int i = 0; i < template.Length; i++) gameValues.list.Add(new GameValue(template[i]));
            MakeValuesDictionaryIfNull();
        }

        public GameValuesTemplate gameValuesTemplate;

        bool initializedWithTemplate;

        void Awake () {
            if (isPlayer) {
                if (playerActor != null) {
                    Destroy(gameObject);
                    return;
                }
                playerActor = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                pool.AddManualInstance(PrefabReferences.GetPrefabReference<Actor>(ActorPrefabsObjectName, basePrefabName), this);
            }

            if (gameValuesTemplate != null) {
                if (!initializedWithTemplate) {
                    BuildGameValues(gameValuesTemplate.gameValues);
                    initializedWithTemplate = true;
                }
            }
            ReinitializeActor();
        }
        public void ReinitializeActor () {
            for (int i = 0; i < gameValues.Count; i++) {
                gameValues[i].ReInitialize();
            }
        }
        void OnDisable () {
            if (isPlayer) {
                if (playerActor != null && playerActor == this) {
                    playerActor = null;
                }
            }
        }
    }
}
