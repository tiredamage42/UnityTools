using System.Collections.Generic;
using UnityEngine;
using System;
namespace UnityTools {
    public class PlayerLevelHandler : MonoBehaviour
    {
        void Awake () {
            GameManager.onPlayerCreate += OnPlayerCreate;
            GameManager.onPlayerDestroy += OnPlayerDestroy;
        }
        
        public string pointsName = "XP";
        public string levelName = "Level";
        [Header("Empty If No Reward")] public string rewardName = "SkillPoints";

        string highestLevelName { get { return "_Highest" + levelName; } }
        bool useRewards { get { return !string.IsNullOrEmpty(rewardName); } }
        public LevelFormula levelFormula;
        GameValuesContainer container;
        GameValue levelV, pointsV, highestLevelV, rewardsV;
        
        void OnPlayerDestroy () {
            levelV = null;
            pointsV = null;
            highestLevelV = null;
            rewardsV = null;
        }

        void OnPlayerCreate () {
            OnPlayerDestroy();

            container = DynamicObject.playerObject.GetObjectScript<GameValuesContainer>();

            if (useRewards) {
                if (GameValueChecker.CheckGameValues(name, container, new GameValueCheck[] {
                    new GameValueCheck(pointsName, false, false),
                    new GameValueCheck(levelName, false, false),
                    new GameValueCheck(highestLevelName, false, false),
                    new GameValueCheck(rewardName, false, false),
                })) {

                    levelV = container.GetGameValueObject(levelName);
                    pointsV = container.GetGameValueObject(pointsName);
                    highestLevelV = container.GetGameValueObject(highestLevelName);
                    rewardsV = container.GetGameValueObject(rewardName);

                    pointsV.AddChangeListener(OnPointsChange, false);
                    levelV.AddChangeListener(OnLevelChange, false);         
                    highestLevelV.AddChangeListener(OnHighestLevelChange, false);
                }
            }
            else {
                if (GameValueChecker.CheckGameValues(name, container, new GameValueCheck[] {
                    new GameValueCheck(pointsName, false, false),
                    new GameValueCheck(levelName, false, false),
                })) {

                    levelV = container.GetGameValueObject(levelName);
                    pointsV = container.GetGameValueObject(pointsName);
                
                    pointsV.AddChangeListener(OnPointsChange, false);
                    levelV.AddChangeListener(OnLevelChange, false);         
                }
            }
        }


        void OnPointsChange (GameValue valueChanged, GameValueChangedComponent changedComponent, float baseDelta, float rangedValueDelta) {
            int basePoints = (int)pointsV.baseValue;
            
            int newLevel = levelFormula.LevelReached(basePoints);
            if (levelV.baseValue != newLevel) {

                levelV.SetBaseValue(newLevel);
            }

            // update 0-1 til next level up (xp bar in ui)
            if (baseDelta > 0) {
                BroadcastPointsChange01(basePoints, (int)baseDelta, newLevel);
            }
        }

        void BroadcastPointsChange01 (int basePoints, int basePointsDelta, int level) {
            if (onPointsChange01 != null) {
                int lastLevelPoints = (int)levelFormula.PointsThreshold(level);
                int nextLevelPoints = (int)levelFormula.PointsThreshold(level + 1);
                float range = nextLevelPoints - lastLevelPoints;
                float targetT = (basePoints - lastLevelPoints) / range;
                float lastT = Mathf.Max((basePoints - basePointsDelta) - lastLevelPoints, 0f) / range;
                onPointsChange01(targetT, lastT);
            }
        }

        public event Action<float, float> onPointsChange01;
            
        void OnLevelChange (GameValue valueChanged, GameValueChangedComponent changedComponent, float baseDelta, float rangedValueDelta) {

            int newLevel = (int)levelV.baseValue;
            // update xp in case we changed level value outside:
            if (levelFormula.LevelReached(pointsV.baseValue) != newLevel) {
                pointsV.SetBaseValue(levelFormula.PointsThreshold(newLevel));
            }
            
            if (useRewards) {
                // prevent farming points
                if (newLevel > highestLevelV.baseValue) {
                    highestLevelV.SetBaseValue(newLevel);
                }
            }
        }
            
        void OnHighestLevelChange (GameValue valueChanged, GameValueChangedComponent changedComponent, float baseDelta, float rangedValueDelta) {
            int deltaPoints = (int)baseDelta;
            if (deltaPoints > 0)
                rewardsV.AddToBaseValue(deltaPoints);
        }
    }
}

