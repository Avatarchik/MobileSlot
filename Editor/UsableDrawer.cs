using UnityEngine;
using UnityEditor;

namespace lpesign.UnityEditor
{
    public abstract class UsableDrawer : PropertyDrawer
    {
        protected Color _defaultBGColor;
        protected Color _defaultColor;
        protected float _singleHeight;

        /// <summary>
        /// Initialize is must called when OnGUI
        /// </summary>
        protected void Initialize()
        {
            _defaultColor = GUI.color;
            _defaultBGColor = GUI.backgroundColor;

            _singleHeight = EditorGUIUtility.singleLineHeight;
        }

        protected Rect GetNextRect(ref Rect position)
        {
            var r = new Rect(position.xMin, position.yMin, position.width, _singleHeight);
            var h = _singleHeight + 1f;
            position = new Rect(position.xMin, position.yMin + h, position.width, position.height = h);
            return r;
        }
    }
}