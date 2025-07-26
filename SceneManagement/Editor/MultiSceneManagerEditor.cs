using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneTon.SceneManagement
{

    [CustomEditor(typeof(MultiSceneManager), true)]
    public class MultiSceneManagerEditor : OdinEditor
    {
        public SceneCollectionPointer pointer;
        public LoadSceneMode mode;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MultiSceneManager multiSceneManager = target as MultiSceneManager;

            if (GUILayout.Button("Initialize"))
            {
                multiSceneManager.Initialize();
            }

            GUILayout.Space(20f);

            pointer = (SceneCollectionPointer)EditorGUILayout.ObjectField("Scene pointer", pointer, typeof(SceneCollectionPointer), false);
            mode = (LoadSceneMode)EditorGUILayout.EnumPopup("LoadSceneMode", mode);


            if (GUILayout.Button("Load Scene"))
            {
                if (Application.isPlaying)
                {
                    if (mode == LoadSceneMode.Single)
                    {
                        multiSceneManager.LoadSceneCollectionSingle(pointer);
                    }

                    if (mode == LoadSceneMode.Additive)
                    {
                        multiSceneManager.LoadSceneCollectionAdditive(pointer);
                    }
                }
            }
        }
    }
}