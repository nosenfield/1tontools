using UnityEditor;
using UnityEngine;

namespace OneTon.SceneManagement
{

    [CustomEditor(typeof(AbstractSceneTransition), true)]
    public class AbstractSceneTransitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AbstractSceneTransition scene = target as AbstractSceneTransition;

            if (GUILayout.Button("Enter Scene"))
            {
                if (Application.isPlaying)
                {
                    scene.SendMessage("EnterScene");
                }
            }

            if (GUILayout.Button("Exit Scene"))
            {
                if (Application.isPlaying)
                {
                    scene.SendMessage("ExitScene");
                }
            }
        }
    }
}