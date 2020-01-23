using UnityEngine;

namespace UnityTools {
    /*
        Exponential Level Caps: (really steep....)

        the amount of xp between levels grows exponentially
        so levelling at higher levels will slow down severely:

    
        B = points cap for first level up
        D = Difficulty curve


        To Get Points Upper Cap (Y) for Level (X):
            Y = BD^(X - 1)
        
        To Get Level (X) from Current Points (Y)
                =>  Y = BD^(X - 1)
                =>  Y / B = D^(X - 1)
                =>  log(Y / B) / log(D) = X - 1
                =>  (log(Y / B) / log(D)) + 1 = X

            X = (log(Y / B) / log(D)) + 1    
    */

    [CreateAssetMenu(menuName="Unity Tools/Levelling/Exponential Level Formula", fileName="LevelFormula_Exponential")]
    public class LevelFormula_Exponential : LevelFormula
    {
        [Tooltip("The base amount of points needed to level up from 1 -> 2")]
        public int baseLevel = 100;
        [Range(1.001f, 3.0f)] public float difficultyCurve = 2;
        protected override float GetUpperPointsCap(int level) {
            return baseLevel * Mathf.Pow(difficultyCurve, level - 1);
        }
        protected override float GetLevelRaw(float pointsValue) {
            return (Mathf.Log10(pointsValue / baseLevel) / Mathf.Log10(difficultyCurve)) + 1;
        }
    }
}
