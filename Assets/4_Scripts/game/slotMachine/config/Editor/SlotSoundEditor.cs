using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using lpesign;

namespace Game
{
    [CustomEditor(typeof(SlotSoundList))]
    public class SlotSoundEditor : SoundListEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}