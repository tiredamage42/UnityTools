using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.Rendering {
    [ExecuteInEditMode] public class TagTest : MonoBehaviour {
        public bool seeThru, flash;
        public BasicColor debugColor = BasicColor.Red;
        public bool doTag, untag;

        void Update () {
            HandleDebug();
        }

        void HandleDebug () {
            if (doTag) {
                Tag();
                doTag = false;
            }
            if (untag) {
                Untag();
                untag = false;
            }
        }

        void Tag () {
            Tagging.TagRenderers(GetComponentsInChildren<Renderer>(), debugColor, seeThru, flash);
        }
        void Untag () {
            Tagging.UntagRenderers(GetComponentsInChildren<Renderer>());
        }
    }
}
