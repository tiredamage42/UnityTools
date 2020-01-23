using System.Collections;
using UnityEngine;
using UnityTools;
using UnityEngine.UI;
namespace UnityToolsDemo {

    public class LevelHandlerUI : MonoBehaviour
    {
        public string pointsName = "XP";
        public float animMinT = .25f;
        public float animTime = .25f;
        PlayerLevelHandler handler;

        void SetBarT (float t) {
            // Debug.Log("Animating Bar " + t);
            
            GameObject.Find(pointsName+"01_UI").GetComponent<Image>().fillAmount = t;
        }


        void Awake () {
            Initialize();
        }

        void Initialize () {

            PlayerLevelHandler[] handlers = GameObject.FindObjectsOfType<PlayerLevelHandler>();
            for (int i = 0; i < handlers.Length; i++) {
                if (handlers[i].pointsName == pointsName) {
                    handler = handlers[i];
                    break;
                }
            }
            if (handler == null) {
                Debug.LogError(name + " LevelHandlerUI cant find level handler for points: " + pointsName);
            }
            else {
                handler.onPointsChange01 += AnimatePointsChange01;
            }
        }

        

        void AnimatePointsChange01 (float targetT, float lastT) {
            // Debug.Log("Animating Bar " + lastT + " To " + targetT);
            StartCoroutine(Animate01Bar(targetT, lastT));   
        }
            
        IEnumerator Animate01Bar (float targetT, float lastT) {
            float startT = Mathf.Max (0f, targetT - Mathf.Max(targetT - lastT, animMinT));
            float t = 0;
            SetBarT(Mathf.Lerp(startT, targetT, t));
            while (true) {
                yield return null;
                t += Time.deltaTime * (1f/animTime);
                SetBarT(Mathf.Lerp(startT, targetT, Mathf.Clamp01(t)));
                if (t >= 1.0f) 
                    break;
            }
        }            
    }
}
