using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OneTon.Utilities
{
    [CustomEditor(typeof(ScrollRectSnapper))]
    public class ScrollRectSnapperEditor : OdinEditor
    {
        public RectTransform childTransform;

        public override void OnInspectorGUI()
        {
            ScrollRectSnapper scrollRectSnapper = (ScrollRectSnapper)target;

            DrawDefaultInspector();

            GUILayout.Space(20f);

            childTransform = (RectTransform)EditorGUILayout.ObjectField("Child Transform", childTransform, typeof(RectTransform), true);


            if (GUILayout.Button("Snap To Child"))
            {
                if (childTransform.IsChildOf(scrollRectSnapper.ScrollRect.content))
                {
                    scrollRectSnapper.SnapToChild(childTransform, 1f);
                }
                else
                {
                    Debug.LogWarning("ScrollRect content does not contain specified child.");
                }
            }
        }
    }
}