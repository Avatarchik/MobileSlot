﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using lpesign;

namespace Game
{
    [CustomEditor(typeof(SlotSoundPlayer))]
    public class SlotSoundListInspector : SoundListInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}