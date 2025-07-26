using System.Collections;
using System.Threading.Tasks;
using OneTon.SceneManagement;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using OneTon.Logging;

public class UniversalSceneObject : MonoBehaviour
{
    private static LogService logger = LogService.Get<UniversalSceneObject>();
    [SerializeField] private SceneReference UniversalScene = null;
    private static bool universalSceneInitialized = false;

    void Awake()
    {
        logger.Trace();

        string sceneName = UniversalScene.SceneName;
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (universalSceneInitialized)
        {
            StartCoroutine(Routine());
            IEnumerator Routine()
            {
                while (!scene.isLoaded)
                {
                    yield return null;
                }

                SceneManager.MoveGameObjectToScene(gameObject, scene);
            }
        }
        else
        {
            universalSceneInitialized = true;
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            op.completed += (asyncOperation) =>
            {
                scene = SceneManager.GetSceneByName(sceneName);
                SceneManager.MoveGameObjectToScene(gameObject, scene);
            };
        }
    }
}