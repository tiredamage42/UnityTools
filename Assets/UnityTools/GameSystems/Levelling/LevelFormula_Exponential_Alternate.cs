

using UnityEngine;

namespace UnityTools {
    /*
        an alternate exponential levelling formula
        
        B = Points cap for first level up
        D = Difficulty curve
    
        To Get Points Upper Cap (Y) for Level (X):
            Y = BX^D
            
        To Get Level (X) from Current points (Y)
            X = (Y/B)^(1/D)
    */
            
    [CreateAssetMenu(menuName="Unity Tools/Levelling/Exponential Level Formula (Alt)", fileName="LevelFormula_Exponential_Alternate")]
    public class LevelFormula_Exponential_Alternate : LevelFormula
    {
        [Tooltip("The base amount of points needed to level up from 1 -> 2")]
        public int baseLevel = 100;
        [Range(1.001f, 10.0f)] public float difficultyCurve = 2;

        protected override float GetUpperPointsCap(int level) {
            return baseLevel * Mathf.Pow(level, difficultyCurve);
        }

        protected override float GetLevelRaw(float pointsValue) {
            return Mathf.Pow(pointsValue / baseLevel, 1f / difficultyCurve);
        }
    }
}
