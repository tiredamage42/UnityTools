using UnityEngine;
using UnityEditor;
using System.Linq;

using System.Collections.Generic;
using System.Reflection;

namespace UnityTools.EditorTools
{


    [System.Serializable]
    internal class CurveDrawer 
    {
        internal static class Styles
        {
            public static GUIStyle labelTickMarksY = "CurveEditorLabelTickMarks";
            public static GUIStyle labelTickMarksX = "CurveEditorLabelTickmarksOverflow";
            public static GUIStyle curveEditorBackground = "PopupCurveEditorBackground";
            public static GUIStyle rectangleToolSelection = "RectangleToolSelection";
        }

        const float kSegmentWindowResolution = 1000;
        const int kMaximumSampleCount = 50;
        const string kCurveRendererMeshName = "NormalCurveRendererMesh";
        
        private Mesh m_CurveMesh;

        private static Material s_CurveMaterial;
        public static Material curveMaterial {
            get {
                if (!s_CurveMaterial) {
                    Shader shader = (Shader)EditorGUIUtility.LoadRequired("Editors/AnimationWindow/Curve.shader");
                    s_CurveMaterial = new Material(shader);
                    s_CurveMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_CurveMaterial;
            }
        }



        private Vector3[] GetPoints(AnimationCurve curve, Rect valueBounds)
        {
            List<Vector3> points = new List<Vector3>();

            if (curve.length == 0)
                return points.ToArray();

            points.Capacity = 1000 + curve.length;

            AddPoints(curve, ref points, valueBounds.xMin, valueBounds.xMax);
            
            return points.ToArray();
        }


        protected virtual int GetSegmentResolution(float minTime, float maxTime, float keyTime, float nextKeyTime)
        {
            return Mathf.Clamp(Mathf.RoundToInt(kSegmentWindowResolution * ((nextKeyTime - keyTime) / (maxTime - minTime))), 1, kMaximumSampleCount);
        }

        protected virtual void AddPoint(ref List<Vector3> points, ref float lastTime, float sampleTime, ref float lastValue, float sampleValue)
        {
            points.Add(new Vector3(sampleTime, sampleValue));
            lastTime = sampleTime;
            lastValue = sampleValue;
        }

        private void AddPoints(AnimationCurve curve, ref List<Vector3> points, float minTime, float maxTime)
        {
         
            for (int i = 0; i < curve.length - 1; i++)
            {
                Keyframe key = curve[i];
                Keyframe nextKey = curve[i + 1];

                // Ignore segments that are outside of the range from minTime to maxTime
                if (nextKey.time < minTime || key.time > maxTime)
                    continue;

                // Get first value from actual key rather than evaluating curve (to correctly handle stepped interpolation)
                points.Add(new Vector3(key.time, key.value));

                // Place second sample very close to first one (to correctly handle stepped interpolation)
                int segmentResolution = GetSegmentResolution(minTime, maxTime, key.time, nextKey.time);
                float newTime = Mathf.Lerp(key.time, nextKey.time, 0.001f / segmentResolution);
                float lastTime = key.time;
                float lastValue = (key.value);
                float value = curve.Evaluate(newTime);
                AddPoint(ref points, ref lastTime, newTime, ref lastValue, value);

                // Iterate through curve segment
                for (float j = 1; j < segmentResolution; j++)
                {
                    newTime = Mathf.Lerp(key.time, nextKey.time, j / segmentResolution);
                    value = curve.Evaluate(newTime);
                    AddPoint(ref points, ref lastTime, newTime, ref lastValue, value);
                }

                // Place second last sample very close to last one (to correctly handle stepped interpolation)
                newTime = Mathf.Lerp(key.time, nextKey.time, 1 - 0.001f / segmentResolution);
                value = value = curve.Evaluate(newTime);
                AddPoint(ref points, ref lastTime, newTime, ref lastValue, value);

                // Get last value from actual key rather than evaluating curve (to correctly handle stepped interpolation)
                newTime = nextKey.time;
                AddPoint(ref points, ref lastTime, newTime, ref lastValue, value);
            }

            if (curve[curve.length - 1].time <= maxTime)
            {
                float clampedValue = (curve[curve.length - 1].value);
                points.Add(new Vector3(curve[curve.length - 1].time, clampedValue));
                points.Add(new Vector3(maxTime, clampedValue));
            }
        }

        private void BuildCurveMesh(AnimationCurve curve, Rect valueBounds)
        {
            if (m_CurveMesh != null)
                return;

            Vector3[] vertices = GetPoints(curve, valueBounds);

            m_CurveMesh = new Mesh();
            m_CurveMesh.name = kCurveRendererMeshName;
            m_CurveMesh.hideFlags |= HideFlags.DontSave;
            m_CurveMesh.vertices = vertices;

            if (vertices.Length > 0)
            {
                int nIndices = vertices.Length - 1;
                int index = 0;

                List<int> indices = new List<int>(nIndices * 2);
                while (index < nIndices)
                {
                    indices.Add(index);
                    indices.Add(++index);
                }

                m_CurveMesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            }
        }

        [SerializeField] private TickHandler hTicks;
        [SerializeField] private TickHandler vTicks;

        void DrawLine(Vector2 lhs, Vector2 rhs)
        {
            GL.Vertex(DrawingToViewTransformPoint(lhs));
            GL.Vertex(DrawingToViewTransformPoint(rhs));
        }

        private const int kMaxDecimals = 15;

        internal static int GetNumberOfDecimalsForMinimumDifference(float minDifference)
        {
            return Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(minDifference))), 0, kMaxDecimals);
        }



        [System.Serializable]
        internal class TickStyle
        {
            public Color tickColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);//, new Color(.45f, .45f, .45f, 0.2f)); // color and opacity of ticks
            public Color labelColor = new Color(0.0f, 0.0f, 0.0f, 1f);//, new Color(0.8f, 0.8f, 0.8f, 0.32f)); // color and opacity of tick labels
            public int distMin = 10; // min distance between ticks before they disappear completely
            public int distFull = 80; // distance between ticks where they gain full strength
            public int distLabel = 50; // min distance between tick labels
        }

        
        internal TickStyle tickStyle = new TickStyle();
        

        public void GridGUI(Rect rect, Rect innerRect, Rect bounds)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.BeginClip(rect);

            Color tempCol = GUI.color;

            hTicks.SetRanges(innerRect.xMin, innerRect.xMax, rect.xMin, rect.xMax);
            vTicks.SetRanges(innerRect.yMin, innerRect.yMax, rect.yMin, rect.yMax);

            ApplyWireMaterialMethod.Invoke(null, null);
            GL.Begin(GL.LINES);

            float lineStart, lineEnd;

            hTicks.SetTickStrengths(tickStyle.distMin, tickStyle.distFull);
            lineStart = Mathf.Max(innerRect.yMin, bounds.yMin);//vRangeMin);
            lineEnd = Mathf.Min(innerRect.yMax, bounds.yMax);// vRangeMax);
            
            for (int l = 0; l < hTicks.tickLevels; l++)
            {
                float strength = hTicks.GetStrengthOfLevel(l);
                if (strength > 0f)
                {
                    GL.Color(tickStyle.tickColor);// * new Color(1, 1, 1, strength * .75f));
                    float[] tickss = hTicks.GetTicksAtLevel(l, true);
                    for (int j = 0; j < tickss.Length; j++)
                    {
                        if (tickss[j] > bounds.xMin && tickss[j] < bounds.xMax)
                            DrawLine(new Vector2(tickss[j], lineStart), new Vector2(tickss[j], lineEnd));
                        
                    }
                }
            }

            // Draw bounds of allowed range
            GL.Color(tickStyle.tickColor);// * new Color(1, 1, 1, 1));//.75f));
            DrawLine(new Vector2(bounds.xMin, lineStart), new Vector2(bounds.xMin, lineEnd));
            DrawLine(new Vector2(bounds.xMax, lineStart), new Vector2(bounds.xMax, lineEnd));

            vTicks.SetTickStrengths(tickStyle.distMin, tickStyle.distFull);
            
            lineStart = Mathf.Max(innerRect.xMin, bounds.xMin);
            lineEnd = Mathf.Min(innerRect.xMax, bounds.xMax);
            
            
            // Draw value markers of various strengths
            for (int l = 0; l < vTicks.tickLevels; l++)
            {
                float strength = vTicks.GetStrengthOfLevel(l);
                if (strength > 0f)
                {
                    GL.Color(tickStyle.tickColor);// * new Color(1, 1, 1, strength * .75f) );
                    float[] tickss = vTicks.GetTicksAtLevel(l, true);
                    for (int j = 0; j < tickss.Length; j++)
                    {
                        if (tickss[j] > bounds.yMin && tickss[j] < bounds.yMax)
                            DrawLine(new Vector2(lineStart, tickss[j]), new Vector2(lineEnd, tickss[j]));
                    }
                }
            }
            // Draw bounds of allowed range
            GL.Color(tickStyle.tickColor);// * new Color(1, 1, 1, 1));//.75f));
            
            DrawLine(new Vector2(lineStart, bounds.yMin), new Vector2(lineEnd, bounds.yMin));
            DrawLine(new Vector2(lineStart, bounds.yMax), new Vector2(lineEnd, bounds.yMax));

            GL.End();

            GUI.color = tickStyle.labelColor;
            
            // X Axis labels
            int labelLevel = hTicks.GetLevelWithMinSeparation(tickStyle.distLabel);

            // Calculate how many decimals are needed to show the differences between the labeled ticks
            int decimals = GetNumberOfDecimalsForMinimumDifference(hTicks.GetPeriodOfLevel(labelLevel));

            // now draw
            float[] ticks = hTicks.GetTicksAtLevel(labelLevel, false);
            float[] ticksPos = (float[])ticks.Clone();
            float vpos = Mathf.Floor(rect.height);

            Styles.labelTickMarksX.alignment = TextAnchor.UpperCenter;

            for (int i = 0; i < ticks.Length; i++)
            {
                if (ticksPos[i] < bounds.xMin || ticksPos[i] > bounds.xMax + 10)
                    continue;
                Vector2 pos = DrawingToViewTransformPoint(new Vector2(ticksPos[i], 0));
                GUI.Label(new Rect(Mathf.Floor(pos.x), vpos - 20, 50, 16), ticks[i].ToString("n" + decimals), Styles.labelTickMarksX);
            }

            
            // Draw value labels
            labelLevel = vTicks.GetLevelWithMinSeparation(tickStyle.distLabel);

            ticks = vTicks.GetTicksAtLevel(labelLevel, false);
            ticksPos = (float[])ticks.Clone();

            // Calculate how many decimals are needed to show the differences between the labeled ticks
            decimals =  GetNumberOfDecimalsForMinimumDifference(vTicks.GetPeriodOfLevel(labelLevel));
            string format = "n" + decimals;
            
            // Calculate the size of the biggest shown label
            float labelSize = 35;
            if ( ticks.Length > 1)
            {
                float min = ticks[1];
                float max = ticks[ticks.Length - 1];
                
                labelSize = Mathf.Max(Styles.labelTickMarksY.CalcSize(new GUIContent(min.ToString(format))).x, Styles.labelTickMarksY.CalcSize(new GUIContent(max.ToString(format))).x) + 6;
            }

            // Now draw
            Styles.labelTickMarksY.alignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < ticks.Length; i++)
            {   
                if (ticksPos[i] < bounds.yMin || ticksPos[i] > bounds.yMax + 10)
                    continue;
                
                Vector2 pos = DrawingToViewTransformPoint(new Vector2(0, ticksPos[i]));
                
                GUI.Label(new Rect(5, Mathf.Floor(pos.y) - 13, labelSize, 16), ticks[i].ToString(format), Styles.labelTickMarksY);
            }
        
            // Cleanup
            GUI.color = tempCol;

            GUI.EndClip();
        }

        MethodInfo ApplyWireMaterialMethod;
        public void OnEnable()
        {

            ApplyWireMaterialMethod = typeof(HandleUtility).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault(x => x.Name.Equals("ApplyWireMaterial") && x.GetParameters().Length == 0);
            margin = 20;
            float[] modulos = new float[]
            {
                0.0000001f, 0.0000005f, 0.000001f, 0.000005f, 0.00001f, 0.00005f, 0.0001f, 0.0005f,
                0.001f, 0.005f, 0.01f, 0.05f, 0.1f, 0.5f, 1, 5, 10, 50, 100, 500,
                1000, 5000, 10000, 50000, 100000, 500000, 1000000, 5000000, 10000000
            };
            hTicks = new TickHandler();
            hTicks.SetTickModulos(modulos);
            vTicks = new TickHandler();
            vTicks.SetTickModulos(modulos);
        }
        
        public void OnDisable()
        {
            UnityEngine.Object.DestroyImmediate(m_CurveMesh);
        }

        
        Bounds CalulateFrameBounds(AnimationCurve curve)
        {
            const float kMinRange = 0.1F;
            Bounds b = m_CurveMesh.bounds;
            b.size = new Vector3(Mathf.Max(b.size.x, kMinRange), Mathf.Max(b.size.y, kMinRange), 0);
            return b;
        }
        Rect CalculateValueBounds (AnimationCurve curve) {
            float maxValue = -Mathf.Infinity;
            float minValue = Mathf.Infinity;
            for (int i = 0; i < curve.keys.Length; i++) {
                float v = curve.keys[i].value;
                if (v > maxValue) maxValue = v;
                if (v < minValue) minValue = v;
            }
            float rangeStart = curve.keys[0].time;
            float rangeEnd   = curve.keys[curve.length - 1].time;
            return new Rect(rangeStart, minValue, rangeEnd - rangeStart, maxValue - minValue);
        }


        public void OnGUI(AnimationCurve curve, float height, Color color)
        {

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.box, GUILayout.Height(height));
            rect.width = EditorGUIUtility.currentViewWidth-20;
            rect.height = height;
            
            
            Rect valueBounds = CalculateValueBounds(curve);
            BuildCurveMesh(curve, valueBounds);
            
            Bounds frameBounds = CalulateFrameBounds(curve);
            Rect innerRect = GetInnerRect(rect);
            

            if (rect != m_DrawArea)
            {
                m_DrawArea = rect;
                Set_shownAreaInsideMarginsInternal(m_LastShownAreaInsideMargins, rect);
            }
                
            GUI.Label(rect, GUIContent.none, Styles.curveEditorBackground);
            GridGUI(rect, innerRect, valueBounds);
            CurveGUI(rect, innerRect, frameBounds, curve, color);            
            Set_shownAreaInsideMarginsInternal(new Rect(frameBounds.min.x, frameBounds.min.y, frameBounds.max.x - frameBounds.min.x, frameBounds.max.y - frameBounds.min.y), rect);
            EnforceScaleAndRange(rect);
        }

        Rect GetInnerRect (Rect rect) {
            return new Rect(-m_Translation.x / m_Scale.x, -(m_Translation.y - rect.height) / m_Scale.y, rect.width / m_Scale.x, rect.height / -m_Scale.y);
        }

        void CurveGUI(Rect drawRect, Rect innerRect, Bounds frameBounds, AnimationCurve curve, Color color)
        {

            GUI.BeginGroup(drawRect);
            Event evt = Event.current;
            if (evt.type == EventType.Repaint) {
                
                if (curve.length > 0)
                    DrawCurve(curve, color, drawingToViewMatrix);
                DrawInnerRect(drawRect, innerRect, frameBounds);
            }

            GUI.color = Color.white;
            GUI.EndGroup();
        }

        float TimeToPixel(Rect rect, Rect shownArea, float x) {
            return (x - shownArea.xMin) * rect.width / (shownArea.xMax - shownArea.xMin);
        }
        float ValueToPixel(Rect rect, Rect shownArea, float yScale, float x) {
            return rect.height - (x * -yScale + (shownArea.yMin * yScale));
        }
        
        public void DrawInnerRect(Rect rect, Rect shownArea, Bounds bounds)
        {
            
            Color oldColor = GUI.color;
            GUI.color = Color.white;
            
            float xMin = TimeToPixel(rect, shownArea, bounds.min.x);
            float yMin = ValueToPixel(rect, shownArea, m_Scale.y, bounds.max.y);
            
            Rect m_Layout = new Rect(xMin, yMin, TimeToPixel(rect, shownArea, bounds.max.x) - xMin, ValueToPixel(rect, shownArea, m_Scale.y, bounds.min.y) - yMin);

            if (!Mathf.Approximately(m_Layout.width * m_Layout.height, 0f)) {
                GUI.Label(m_Layout, GUIContent.none, Styles.rectangleToolSelection);
            }
            GUI.color = oldColor;
        }

        
        void DrawCurve(AnimationCurve curve, Color color, Matrix4x4 transform)
        {
            if (m_CurveMesh.vertexCount == 0)
                return;
            
            curveMaterial.SetColor("_Color", color);
            // Previous camera may still be active when calling DrawMeshNow.
            Camera.SetupCurrent(null);
            curveMaterial.SetPass(0);
            Graphics.DrawMeshNow(m_CurveMesh, Handles.matrix * transform);            
        }
               
    
        [SerializeField] private Rect m_LastShownAreaInsideMargins = new Rect(0, 0, 100, 100);
        [SerializeField] private Rect m_DrawArea = new Rect(0, 0, 100, 100);
        [SerializeField] internal Vector2 m_Scale = new Vector2(1, -1);
        [SerializeField] internal Vector2 m_Translation = new Vector2(0, 0);
        float margin;


        public void Set_shownAreaInsideMarginsInternal (Rect value, Rect rect) {
            
            m_Scale.x = (rect.width - margin - margin) / (value.width);
            m_Scale.y = -(rect.height - margin - margin) / (value.height);
            
            m_Translation.x = -value.x * m_Scale.x + margin;
            m_Translation.y = rect.height - value.y * m_Scale.y - margin;
        }

        
        Rect GetShownAreaInsideInternal (Rect rect) {
            float xMargin = margin / m_Scale.x;
            float yMargin = margin / m_Scale.y;
            
            Rect area = GetInnerRect(rect);
            area.x += xMargin;
            area.y -= yMargin;
            area.width -= xMargin + xMargin;
            area.height += yMargin + yMargin;
            return area;
        }


        float GetInsideMargins(float withMargins)
        {
            return withMargins - margin - margin;
        }

        public Matrix4x4 drawingToViewMatrix { get { return Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1)); } }

        public Vector3 DrawingToViewTransformPoint(Vector3 lhs) { 
            return new Vector3(lhs.x * m_Scale.x + m_Translation.x, lhs.y * m_Scale.y + m_Translation.y, 0); 
        }


        public void EnforceScaleAndRange(Rect rect)
        {
            // Minimum scale might also be constrained by maximum range
            
            Rect old = m_LastShownAreaInsideMargins;
            Rect newA = GetShownAreaInsideInternal(rect);
            if (newA == old)
                return;

            float epsilon = 0.00001f;
            float minChange = 0.01f;

            if (!Mathf.Approximately(newA.width, old.width))
            {
                float t = Mathf.InverseLerp(old.width, newA.width, GetInsideMargins(newA.width < old.width - epsilon ? rect.width / rect.xMax : (rect.width / rect.xMin)));
                float w = Mathf.Lerp(old.width, newA.width, t);
                newA = new Rect(Mathf.Abs(w - newA.width) > minChange ? Mathf.Lerp(old.x, newA.x, t) : newA.x, newA.y, w, newA.height);
            }
            if (!Mathf.Approximately(newA.height, old.height))
            {
                float t = Mathf.InverseLerp(old.height, newA.height, GetInsideMargins(newA.height < old.height - epsilon ? rect.height / rect.yMax : (rect.height / rect.yMin)));
                float h = Mathf.Lerp(old.height, newA.height, t);
                newA = new Rect(newA.x, Mathf.Abs(h - newA.height) > minChange ? Mathf.Lerp(old.y, newA.y, t) : newA.y, newA.width, h);
            }

            Set_shownAreaInsideMarginsInternal (newA, rect);
            m_LastShownAreaInsideMargins = newA;
        }

        
    }




















    [System.Serializable]
    internal class TickHandler
    {
        // Variables related to drawing tick markers
        [SerializeField] private float[] m_TickModulos = new float[] {}; // array with possible modulo numbers to choose from
        [SerializeField] private float[] m_TickStrengths = new float[] {}; // array with current strength of each modulo number
        [SerializeField] private int m_SmallestTick = 0; // index of the currently smallest modulo number used to draw ticks
        [SerializeField] private int m_BiggestTick = -1; // index of the currently biggest modulo number used to draw ticks
        [SerializeField] private float m_MinValue = 0; // shownArea min (in curve space)
        [SerializeField] private float m_MaxValue = 1; // shownArea max (in curve space)
        [SerializeField] private float m_PixelRange = 1; // total width/height of curveeditor

        public int tickLevels { get { return m_BiggestTick - m_SmallestTick + 1; } }

        private List<float> m_TickList = new List<float>(1000);

        public void SetTickModulos(float[] tickModulos)
        {
            m_TickModulos = tickModulos;
        }

        public void SetRanges(float minValue, float maxValue, float minPixel, float maxPixel)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_PixelRange = maxPixel - minPixel;
        }

        public float[] GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels)
        {
            if (level < 0)
                return new float[0] {};

            m_TickList.Clear();
            GetTicksAtLevel(level, excludeTicksFromHigherlevels, m_TickList);
            return m_TickList.ToArray();
        }

        public void GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels, List<float> list)
        {
            
            int l = Mathf.Clamp(m_SmallestTick + level, 0, m_TickModulos.Length - 1);
            int startTick = Mathf.FloorToInt(m_MinValue / m_TickModulos[l]);
            int endTick = Mathf.CeilToInt(m_MaxValue / m_TickModulos[l]);
            for (int i = startTick; i <= endTick; i++)
            {
                // Return if tick mark is at same time as larger tick mark
                if (excludeTicksFromHigherlevels && l < m_BiggestTick && (i % Mathf.RoundToInt(m_TickModulos[l + 1] / m_TickModulos[l]) == 0))
                    continue;
                list.Add(i * m_TickModulos[l]);
            }
        }

        public float GetStrengthOfLevel(int level)
        {
            return m_TickStrengths[m_SmallestTick + level];
        }

        public float GetPeriodOfLevel(int level)
        {
            return m_TickModulos[Mathf.Clamp(m_SmallestTick + level, 0, m_TickModulos.Length - 1)];
        }

        public int GetLevelWithMinSeparation(float pixelSeparation)
        {
            for (int i = 0; i < m_TickModulos.Length; i++)
            {
                // How far apart (in pixels) these modulo ticks are spaced:
                float tickSpacing = m_TickModulos[i] * m_PixelRange / (m_MaxValue - m_MinValue);
                if (tickSpacing >= pixelSeparation)
                    return i - m_SmallestTick;
            }
            return -1;
        }

        public void SetTickStrengths(float tickMinSpacing, float tickMaxSpacing)
        {
            m_TickStrengths = new float[m_TickModulos.Length];
            m_SmallestTick = 0;
            m_BiggestTick = m_TickModulos.Length - 1;

            // Find the strength for each modulo number tick marker
            for (int i = m_TickModulos.Length - 1; i >= 0; i--)
            {
                // How far apart (in pixels) these modulo ticks are spaced:
                float tickSpacing = m_TickModulos[i] * m_PixelRange / (m_MaxValue - m_MinValue);

                // Calculate the strength of the tick markers based on the spacing:
                m_TickStrengths[i] =
                    (tickSpacing - tickMinSpacing) / (tickMaxSpacing - tickMinSpacing);

                // Beyond kTickHeightFatThreshold the ticks don't get any bigger or fatter,
                // so ignore them, since they are already covered by smalle modulo ticks anyway:
                if (m_TickStrengths[i] >= 1) m_BiggestTick = i;

                // Do not show tick markers less than 3 pixels apart:
                if (tickSpacing <= tickMinSpacing) { m_SmallestTick = i; break; }
            }

            // Use sqrt on actively used modulo number tick markers
            for (int i = m_SmallestTick; i <= m_BiggestTick; i++)
            {
                m_TickStrengths[i] = Mathf.Clamp01(m_TickStrengths[i]);
            }
        }
    }

}