using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools {

    public abstract class LevelFormula : ScriptableObject
    {
        public int LevelReached (float pointsValue) {

            float levelCalculated = GetLevelRaw(pointsValue);
            /*
                cast to int and + 1 is to ensure that having the amount of xp 
                at the level's upper limit is considered being at the next level
                
                e.g. 100 xp would return level 2 (if B == 100) as that means
                we've gotten enough xp to be "leveled up" for the first time
                anything below 100xp would return level 1
            */
            int level = ((int)levelCalculated) + 1;

            // make sure it's >= 1
            return Mathf.Max(level, 1);
        }

        
        /*
            the amount of points we have to accumulate to reach the level
        */
        public float PointsThreshold (int level) {
            
            if (level <= 1)
                return 0;
            
            return GetUpperPointsCap(level - 1);
        }

        protected abstract float GetLevelRaw (float pointsValue);
        protected abstract float GetUpperPointsCap (int level);
    }

        

    #if UNITY_EDITOR
    [CustomEditor(typeof(LevelFormula), true)] class LevelFormulaEditor : Editor {
        const string showHints = "Points Cap: The total points accumulated needed to reach level X.\nPoints Difference: The points difference between levels.";
        public enum CurveShowMode { PointsCap, PointsDifference }
        CurveShowMode curveShowMode;
        int samplesRange = 25, samplesStart = 1;
        string samplesString;
        AnimationCurve showCurve;
        CurveDrawer curveDrawer = new CurveDrawer();

        void OnEnable () {
            curveDrawer.OnEnable();
            InitializeEditor();
        }
        void OnDisable () {
            curveDrawer.OnDisable();
        }

        void InitializeEditor () {
            samplesRange = 25;
            samplesStart = 1;
            RebuildVisualization(target as LevelFormula);
        }

        void ClearCurve () {
            for (int i = showCurve.length - 1; i >= 0; i--) 
                showCurve.RemoveKey(i);
        }
        void RebuildVisualization (LevelFormula formula) {
            if (showCurve == null) 
                showCurve = new AnimationCurve();
            
            ClearCurve();
            
            samplesString = "Level\tPointsNeeded\tPoints Delta (From Last Level Up)\n\n";
            int lastNeeded = (int)formula.PointsThreshold(samplesStart-1);
            
            for (int i = 0; i < samplesRange; i++) {
                int l = samplesStart + i;

                int needed = (int)formula.PointsThreshold(l);
                int delta = needed - lastNeeded;
                lastNeeded = needed;

                if (curveShowMode == CurveShowMode.PointsCap) 
                    showCurve.AddKey(l, needed);
                else if (curveShowMode == CurveShowMode.PointsDifference) 
                    showCurve.AddKey(l, delta);
                
                samplesString += l + "\t" + needed.LargeNumberToString() + "\t\t+" + delta.LargeNumberToString() + "\n";
            }
            curveDrawer.OnDisable();
        }

        public override void OnInspectorGUI() 
        {
            if (showCurve == null) 
                InitializeEditor();
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            
            GUITools.Space(2);

            EditorGUILayout.LabelField("Curve Visualization:", GUITools.boldLabel);
            
            curveShowMode = (CurveShowMode)EditorGUILayout.EnumPopup(new GUIContent("Curve Show Mode", showHints), curveShowMode);
            
            samplesRange = EditorGUILayout.IntSlider("Levels Range", samplesRange, 3, 50);
            
            samplesStart = EditorGUILayout.IntField("Samples Start Level", samplesStart);
            if (samplesStart < 1)
                samplesStart = 1;

            if (EditorGUI.EndChangeCheck())
                RebuildVisualization(target as LevelFormula);
            
            curveDrawer.OnGUI (showCurve, 200, Color.green);

            EditorGUILayout.HelpBox(samplesString, MessageType.None);
        }
    }
    #endif
}
