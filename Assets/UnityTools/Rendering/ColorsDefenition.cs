using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.GameSettingsSystem;
namespace UnityTools.Rendering {

    [CreateAssetMenu(menuName="Unity Tools/Rendering/Colors Defenition", fileName="Colors")]
    public class ColorsDefenition : GameSettingsObject
    {
        public BasicColorDefs colors;

        public Color32 GetColor (BasicColor color) {
            return colors.GetColor(color);
        }
    }
}
