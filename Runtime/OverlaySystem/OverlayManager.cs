using System.Linq;
using System.Threading.Tasks;
using OneTon.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using System.Collections.Generic;


namespace OneTon.OverlaySystem
{
    public enum OverlayDisplayMode
    {
        Additive,
        Replacement
    }

    [CreateAssetMenu(fileName = "_OverlayManager.asset", menuName = "1ton/OverlaySystem/OverlayManager")]
    public class OverlayManager : ScriptableObject
    {
        private static readonly LogService logger = LogService.Get<OverlayManager>();

        [SerializeField] private SceneReference popupParentScene = null;
        [SerializeField] private OverlayPrefabDatabase overlayPrefabDatabase;
        [SerializeField] private GameObject universalDimmerPrefab = null;

        private Transform parentTransform = null;
        private static bool initializationCalled = false;

        private readonly Stack<List<OverlayBase>> overlayGroups = new Stack<List<OverlayBase>>();
        private GameObject universalDimmerInstance;

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

            // Setup Universal Dimmer (inactive by default)
            if (universalDimmerPrefab != null)
            {
                universalDimmerInstance = Instantiate(universalDimmerPrefab, parentTransform);
                universalDimmerInstance.SetActive(false);
            }
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

        // --- Public Overlay API ---

        public TOverlay GetPrefabForType<TOverlay>() where TOverlay : OverlayBase
        {
            return overlayPrefabDatabase.GetPrefab<TOverlay>().GetComponentInChildren<TOverlay>();
        }

        public async Task<TOverlay> ShowOverlay<TOverlay>(TOverlay overlay, OverlayDisplayMode displayMode = OverlayDisplayMode.Additive) where TOverlay : OverlayBase
        {
            logger.Trace();
            await Initialize();
            var instance = Instantiate(overlay.gameObject, parentTransform).GetComponent<TOverlay>();
            if (displayMode == OverlayDisplayMode.Replacement)
            {
                HideCurrentGroup();
                overlayGroups.Push(new List<OverlayBase> { instance });
            }
            else // Additive
            {
                if (overlayGroups.Count == 0)
                    overlayGroups.Push(new List<OverlayBase>());

                overlayGroups.Peek().Add(instance);
            }
            instance.Initialize();
            await instance.ShowAsync();
            HandleUniversalDimmer(instance);

            return instance;
        }

        public async Task<(TOverlay overlay, Task<TResult> resultTask)> ShowOverlayWithResult<TOverlay, TResult>(TOverlay overlay, OverlayDisplayMode displayMode = OverlayDisplayMode.Additive) where TOverlay : OverlayWithResult<TResult>
        {
            logger.Trace();
            var instance = await ShowOverlay(overlay, displayMode);
            return (instance, instance.WaitForResult());
        }

        public async Task DismissOverlay(OverlayBase overlay)
        {
            if (overlayGroups.Count == 0)
            {
                logger.Warn("DismissOverlay called but no overlays are active.");
                return;
            }

            var currentGroup = overlayGroups.Peek();

            if (!currentGroup.Remove(overlay))
            {
                logger.Warn("Overlay not found in current group.");
                return;
            }

            await overlay.Teardown();

            // If this group is now empty, pop it and restore the previous group
            if (currentGroup.Count == 0)
            {
                overlayGroups.Pop();
                RestorePreviousGroup();
            }

            UpdateUniversalDimmer();
        }

        // --- Internal Group Handling ---

        private void HideCurrentGroup()
        {
            if (overlayGroups.Count > 0)
            {
                foreach (var overlay in overlayGroups.Peek())
                {
                    overlay.gameObject.SetActive(false);
                }
            }
        }

        private void RestorePreviousGroup()
        {
            if (overlayGroups.Count > 0)
            {
                foreach (var overlay in overlayGroups.Peek())
                {
                    overlay.gameObject.SetActive(true);
                }
            }
        }

        // --- Universal Dimmer ---

        private void HandleUniversalDimmer(OverlayBase topOverlay)
        {
            if (universalDimmerInstance == null) return;

            if (topOverlay.UseUniversalDimmer)
            {
                universalDimmerInstance.SetActive(true);
                int overlayIndex = topOverlay.transform.GetSiblingIndex();
                int dimmerIndex = universalDimmerInstance.transform.GetSiblingIndex();
                if (dimmerIndex < overlayIndex)
                {
                    universalDimmerInstance.transform.SetSiblingIndex(Mathf.Max(0, overlayIndex - 1));
                }
                else
                {
                    universalDimmerInstance.transform.SetSiblingIndex(overlayIndex);
                }
            }
            else
            {
                universalDimmerInstance.SetActive(false);
            }
        }

        private void UpdateUniversalDimmer()
        {
            if (universalDimmerInstance == null) return;

            if (overlayGroups.Count == 0)
            {
                universalDimmerInstance.SetActive(false);
                return;
            }

            HandleUniversalDimmer(overlayGroups.Peek().LastOrDefault());
        }
    }
}