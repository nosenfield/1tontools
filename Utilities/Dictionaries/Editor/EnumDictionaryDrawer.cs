using UnityEngine;
using UnityEditor;
using OneTon.Utilities;

namespace OneTon.Dictionaries
{
    [CustomPropertyDrawer(typeof(EnumDictionary<,>))]
    public class EnumDictionaryDrawer : PropertyDrawer
    {
        private SerializedProperty keys;
        private SerializedProperty values;

        public float spacing = 2;
        public bool foldout = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetLists(property);

            float height = EditorGUIUtility.singleLineHeight;

            if (foldout)
            {
                if (keys.arraySize > 0)
                {
                    if (values.GetArrayElementAtIndex(0).hasChildren)
                    {
                        height += spacing * (keys.arraySize - 1);
                        for (int i = 0; i < keys.arraySize; i++)
                        {
                            height += EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(i), label);
                        }
                    }
                    else
                    {
                        float unitSize = Mathf.Max(base.GetPropertyHeight(keys.GetArrayElementAtIndex(0), label), base.GetPropertyHeight(values.GetArrayElementAtIndex(0), label)) + spacing;
                        height += unitSize * keys.arraySize;
                    }
                }
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
                EditorGUI.indentLevel++;

                bool valueHasChildren = false;

                float latestY = position.y + labelHeight + spacing;

                float unitSize = EditorGUIUtility.singleLineHeight;
                if (keys.arraySize > 0)
                {
                    valueHasChildren = values.GetArrayElementAtIndex(0).hasChildren;
                    unitSize = Mathf.Max(base.GetPropertyHeight(keys.GetArrayElementAtIndex(0), label), base.GetPropertyHeight(values.GetArrayElementAtIndex(0), label));
                }
                float fieldWidth = position.width;

                Rect pairRect;
                SerializedProperty key;
                for (int i = 0; i < keys.arraySize; i++)
                {
                    if (valueHasChildren)
                    {
                        unitSize = EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(i), label);
                    }
                    key = keys.GetArrayElementAtIndex(i);
                    pairRect = new Rect(position.x, latestY, fieldWidth, unitSize);
                    EditorGUI.PropertyField(pairRect, values.GetArrayElementAtIndex(i), new GUIContent(key.enumDisplayNames[key.enumValueIndex]), true);
                    latestY += unitSize + spacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void GetLists(SerializedProperty property)
        {
            keys = property.FindPropertyRelative("listE");
            values = property.FindPropertyRelative("listV");

            if (keys.arraySize == 0)
            {
                keys.InsertArrayElementAtIndex(0);
                values.InsertArrayElementAtIndex(0);
            }

            int targetLength = keys.GetArrayElementAtIndex(0).enumDisplayNames.Length;
            bool needsNew;
            for (int i = 0; i < targetLength; i++)
            {
                needsNew = true;
                for (int j = i; j < keys.arraySize; j++)
                {
                    if (keys.GetArrayElementAtIndex(j).enumValueIndex == i)
                    {
                        keys.MoveArrayElement(j, i);
                        values.MoveArrayElement(j, i);
                        needsNew = false;
                        break;
                    }
                }

                if (needsNew)
                {
                    keys.InsertArrayElementAtIndex(i);
                    values.InsertArrayElementAtIndex(i);
                    keys.GetArrayElementAtIndex(i).enumValueIndex = i;
                }
            }

            keys.arraySize = targetLength;
            values.arraySize = targetLength;
        }
    }
}