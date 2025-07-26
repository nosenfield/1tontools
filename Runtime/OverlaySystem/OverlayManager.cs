using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneTon.Dictionaries;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneTon.OverlaySystem
{
    [CreateAssetMenu(fileName = "_OverlayManager.asset", menuName = "1ton/OverlaySystem/OverlayManager")]
    public class OverlayManager : ScriptableObject
    {
        private static OneTon.Logging.Logger logger = new();
        [SerializeField] private SceneReference popupParentScene = null;
        private Transform parentTransform = null;
        private static bool initializationCalled = false;
        private Task initializationTask;
        [SerializeField] OverlayPrefabDatabase overlayPrefabDatabase;

        /// <summary>
        /// Ensures the overlay scene is loaded and sets the parent transform.
        /// </summary>
        private async Task Initialize()
        {
            logger.Trace();
            logger.Log($"initializationCalled: {initializationCalled}");

            string sceneName = popupParentScene.SceneName;
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

            logger.Log($"{sceneName} load complete");

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

        public async Task<TOverlay> ShowOverlay<TOverlay>(GameObject prefab) where TOverlay : OverlayBase
        {
            await Initialize();
            var instance = Instantiate(prefab, parentTransform).GetComponent<TOverlay>();
            instance.Initialize();
            await instance.ShowAsync();
            return instance;
        }

        public async Task<(TOverlay overlay, Task<TResult> resultTask)> ShowOverlayWithResult<TOverlay, TResult>(GameObject prefab) where TOverlay : OverlayWithResult<TResult>
        {
            await Initialize();
            var instance = Instantiate(prefab, parentTransform).GetComponent<TOverlay>();
            instance.Initialize();
            await instance.ShowAsync();
            return (instance, instance.WaitForResult());
        }

        public GameObject GetPrefabForType<TOverlay>() where TOverlay : OverlayBase
        {
            return overlayPrefabDatabase.GetPrefab<TOverlay>();
        }
    }
}