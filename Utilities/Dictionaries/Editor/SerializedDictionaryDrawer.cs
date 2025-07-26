using UnityEngine;
using UnityEditor;

namespace OneTon.Dictionaries
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private SerializedProperty keys;
        private SerializedProperty values;

        public float spacing = 2;
        public bool foldout;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetLists(property);

            float height = EditorGUIUtility.singleLineHeight;

            if (foldout)
            {
                float unitSize = EditorGUIUtility.singleLineHeight + spacing;
                if (keys.arraySize > 0)
                {
                    unitSize = Mathf.Max(base.GetPropertyHeight(keys.GetArrayElementAtIndex(0), label), base.GetPropertyHeight(values.GetArrayElementAtIndex(0), label)) + spacing;
                    height += unitSize * keys.arraySize;
                }
                height += unitSize;
            }

            return height;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetLists(property);

            EditorGUI.BeginProperty(position, label, property);

            float labelHeight = EditorGUIUtility.singleLineHeight;
            Rect foldoutPosition = new Rect(position.x, position.y, position.width, labelHeight);
            foldout = EditorGUI.Foldout(foldoutPosition, foldout, label);

            if (foldout)
            {
                float latestY = position.y + labelHeight + spacing;

                float unitSize = EditorGUIUtility.singleLineHeight;
                if (keys.arraySize > 0)
                {
                    unitSize = Mathf.Max(base.GetPropertyHeight(keys.GetArrayElementAtIndex(0), label), base.GetPropertyHeight(values.GetArrayElementAtIndex(0), label));
                }
                float buttonSide = EditorGUIUtility.fieldWidth / 2;
                float fieldWidth = position.width - buttonSide - spacing;

                Rect keyRect = new Rect(position.x, latestY, fieldWidth / 2 - spacing / 2, unitSize);
                Rect valueRect = new Rect(position.x + fieldWidth / 2 + spacing / 2, latestY, fieldWidth / 2 - spacing / 2, unitSize);

                Rect buttonRect = new Rect(position.x + position.width - buttonSide, latestY, buttonSide, unitSize);

                for (int i = 0; i < keys.arraySize; i++)
                {
                    EditorGUI.PropertyField(keyRect, keys.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUI.PropertyField(valueRect, values.GetArrayElementAtIndex(i), GUIContent.none);

                    if (GUI.Button(buttonRect, "-"))
                    {
                        keys.DeleteArrayElementAtIndex(i);
                        values.DeleteArrayElementAtIndex(i);
                    }
                    keyRect.y += unitSize + spacing;
                    valueRect.y += unitSize + spacing;
                    buttonRect.y += unitSize + spacing;
                    latestY += unitSize + spacing;
                }

                if (GUI.Button(buttonRect, "+"))
                {
                    keys.InsertArrayElementAtIndex(keys.arraySize);
                    values.InsertArrayElementAtIndex(values.arraySize);
                }

            }

            EditorGUI.EndProperty();
        }

        private void GetLists(SerializedProperty property)
        {
            keys = property.FindPropertyRelative("listK");
            values = property.FindPropertyRelative("listV");
        }
    }
}