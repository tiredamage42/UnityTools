
using UnityEngine;

namespace UnityTools {
    /*
        ============================================
        Linear Level Caps: (More forgiving...)
        
        the amount of xp betwen levels gros linearly (+DB each level)
        more levelling up at higher levels than exponential


        To Get Points Upper Cap (Y) for Level (X):
            Y = BD(X(X + 1) / 2)
        
        To Get Level (X) from Current Points (Y)

                =>  Y = BD(X(X + 1) / 2)
                =>  Y / BD = X(X + 1) / 2
                =>  2Y / BD = X(X + 1)
                =>  2Y / BD = X ^ 2 + X
                =>  0 = X ^ 2 + X - 2Y / BD
                
                use quadratic formula: x = (-b + sqrt(b ^ 2 - 4ac)) / 2a
                where:  a, b == 1 and c == -2Y / BD
                
                => X = (-1 + sqrt(1 - 4(-2Y / BD))) / 2
                => X = (-1 + sqrt(1 - (-8Y / BD))) / 2
                
            X = (-1 + sqrt(1 + 8Y / BD)) / 2
    */

    
    [CreateAssetMenu(menuName="Unity Tools/Levelling/Linear Level Formula", fileName="LevelFormula_Linear")]
    public class LevelFormula_Linear : LevelFormula
    {
        [Tooltip("The base amount of points needed to level up from 1 -> 2")]
        public int baseLevel = 100;
        [Range(.001f, 10.0f)] public float difficultyCurve = 2;

        /* 
            sum of all numbers [1 -> I] (inclusive)
        */
        int Sum (int i) {
            return i * (i + 1) / 2;
        }
        protected override float GetUpperPointsCap(int level) {
            return baseLevel * difficultyCurve * Sum(level);
        }
        protected override float GetLevelRaw(float pointsValue) {
            return (-1 + Mathf.Sqrt(1 + ( (8 * pointsValue) / (baseLevel * difficultyCurve) ))) * .5f;
        }
    }
}
