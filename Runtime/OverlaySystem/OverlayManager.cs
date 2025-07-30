using System.Linq;
using System.Threading.Tasks;
using OneTon.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;


namespace OneTon.OverlaySystem
{
    [CreateAssetMenu(fileName = "_OverlayManager.asset", menuName = "1ton/OverlaySystem/OverlayManager")]
    public class OverlayManager : ScriptableObject
    {
        private static readonly LogService logger = LogService.Get<OverlayManager>();

        [SerializeField] private SceneReference popupParentScene = null;
        private Transform parentTransform = null;
        private static bool initializationCalled = false;
        [SerializeField] OverlayPrefabDatabase overlayPrefabDatabase;

        /// <summary>
        /// Ensures the overlay scene is loaded and sets the parent transform.
        /// </summary>
        private async Task Initialize()
        {
            logger.Trace();

            string sceneName = popupParentScene.Name;
            Scene scene = SceneManager.GetSceneByName(sceneName);

            if (initializationCalled)
            {
                while (!scene.isLoaded)
                {
                    await Task.Delay(100);
                }

                if (parentTransform == null)
                {
                    parentTransform = FindParentCanvasTransform(scene);
                }

                return;
            }

            initializationCalled = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await TaskCompletionSourceFromOperation(op);

            logger.Debug($"{sceneName} load complete");

            scene = SceneManager.GetSceneByName(sceneName);
            parentTransform = FindParentCanvasTransform(scene);
        }

        private async Task TaskCompletionSourceFromOperation(AsyncOperation op)
        {
            var tcs = new TaskCompletionSource<bool>();
            op.completed += _ => tcs.SetResult(true);
            await tcs.Task;
        }

        private Transform FindParentCanvasTransform(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Canvas>(true))
                .FirstOrDefault()?.transform;
        }

        public async Task<TOverlay> ShowOverlay<TOverlay>(GameObject prefab, bool initializeAndShow = true) where TOverlay : OverlayBase
        {
            logger.Trace();
            await Initialize();
            var instance = Instantiate(prefab, parentTransform).GetComponent<TOverlay>();
            if (initializeAndShow)
            {
                instance.Initialize();
                await instance.ShowAsync();
            }
            return instance;
        }

        public async Task<(TOverlay overlay, Task<TResult> resultTask)> ShowOverlayWithResult<TOverlay, TResult>(GameObject prefab, bool initializeAndShow = true) where TOverlay : OverlayWithResult<TResult>
        {
            logger.Trace();
            await Initialize();
            var instance = Instantiate(prefab, parentTransform).GetComponent<TOverlay>();
            if (initializeAndShow)
            {
                instance.Initialize();
                await instance.ShowAsync();
            }
            return (instance, instance.WaitForResult());
        }

        public GameObject GetPrefabForType<TOverlay>() where TOverlay : OverlayBase
        {
            return overlayPrefabDatabase.GetPrefab<TOverlay>();
        }
    }
}