using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityTools.GameSettingsSystem;

namespace UnityTools.Rendering {

    [CreateAssetMenu(menuName="Unity Tools/Rendering/Tag Settings", fileName="UnityTools_TagSettings")]
    public class TagSettings : GameSettingsObjectSingleton<TagSettings>
    {
        public float defaultRimPower = 1.25f;
        public Vector2 flashRimPowerRange = new Vector2(10, .5f);
        public float flashSteepness = .5f;
        public float flashSpeed = 2;

        public BasicColorDefs colors;

        public Color32 GetColor (BasicColor color) {
            return colors.GetColor(color);
        }

    }
}
