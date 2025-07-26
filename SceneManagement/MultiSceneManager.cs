using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using OneTon.Dictionaries;
using OneTon.Logging;
using Sirenix.OdinInspector;
using OneTon.ScriptableObjectHelpers;

namespace OneTon.SceneManagement
{
    [CreateAssetMenu(fileName = "_MultiSceneManager.asset", menuName = "nosenfield/SceneManagement/MultiSceneManager")]
    public class MultiSceneManager : ScriptableObject, INeedsInitialization
    {
        private static LogService logger = LogService.Get<MultiSceneManager>();
        [SerializeField] private SerializedDictionary<SceneCollectionPointer, SceneCollection> SceneDictionary;
        [SerializeField] private bool inTransition;
        [SerializeField][ReadOnly] private List<SceneReference> loadedScenes;
        [SerializeField][ReadOnly] private List<AbstractSceneTransition> exitingScenes;

        public void Initialize()
        {
            loadedScenes = new();
            exitingScenes = new();
            inTransition = false;
        }

        private void GetScenesInCollection(SceneCollection sceneCollection, List<SceneReference> scenes)
        {
            for (int i = 0; i < sceneCollection.Subcollections.Length; i++)
            {
                GetScenesInCollection(sceneCollection.Subcollections[i], scenes);
            }

            for (int i = 0; i < sceneCollection.Scenes.Length; i++)
            {
                scenes.Add(sceneCollection.Scenes[i]);
            }
        }

        public async void LoadSceneCollectionSingle(SceneCollectionPointer pointer)
        {
            logger.Debug($"LoadSceneCollectionSingle({pointer.name})");
            logger.Debug($"inTransition: {inTransition}");
            if (inTransition) return;
            inTransition = true;

            List<SceneReference> outgoingScenes = new(loadedScenes);
            List<SceneReference> requestedScenes = new();
            List<SceneReference> sharedScenes = new();

            GetScenesInCollection(SceneDictionary[pointer], requestedScenes);

            logger.Debug("Requested scenes:");
            foreach (SceneReference sceneRef in requestedScenes)
            {
                logger.Debug(sceneRef.SceneName);
            }

            SceneReference scene;
            for (int i = requestedScenes.Count - 1; i >= 0; i--)
            {
                scene = requestedScenes[i];
                if (outgoingScenes.Contains(scene))
                {
                    outgoingScenes.Remove(scene);
                    requestedScenes.Remove(scene);
                    sharedScenes.Add(scene);
                }
            }

            logger.Debug("Incoming scenes:");
            foreach (SceneReference sceneRef in requestedScenes)
            {
                logger.Debug(sceneRef.SceneName);
            }

            logger.Debug("Outgoing scenes:");
            foreach (SceneReference sceneRef in outgoingScenes)
            {
                logger.Debug(sceneRef.SceneName);
                foreach (GameObject rootObject in sceneRef.SceneRootObjects)
                {
                    AbstractSceneTransition[] transitionScripts = rootObject.GetComponentsInChildren<AbstractSceneTransition>();
                    if (transitionScripts != null)
                    {
                        exitingScenes.AddRange(transitionScripts);
                    }
                }
            }

            await LoadCollection(SceneDictionary[pointer], false);

            ExitScenes(outgoingScenes);
            while (exitingScenes.Count > 0)
            {
                await Task.Yield();
            }

            SetScenesActive(requestedScenes, true);
            EnterScenes(requestedScenes);
            UnloadScenes(outgoingScenes);

            // NOTE
            // Because the unload is not awaited it is maybe possible to trigger a scene to reload while it is still unloading from a previous call.
            // Consider awaiting any in progress unload before allowing more scene loads
            ///

            inTransition = false;
        }

        public async Task LoadSceneCollectionAdditive(SceneCollectionPointer pointer)
        {
            logger.Debug($"LoadSceneCollectionAdditive({pointer.name})");
            await LoadCollection(SceneDictionary[pointer], true);
        }

        public void SceneTransitionComplete(AbstractSceneTransition scene)
        {
            logger.Trace();
            exitingScenes.Remove(scene);
        }

        private async Task LoadCollection(SceneCollection collection, bool startActive)
        {
            logger.Debug($"LoadCollection({collection.name})");

            for (int i = 0; i < collection.Subcollections.Length; i++)
            {
                await LoadCollection(collection.Subcollections[i], startActive);
            }

            int scenesToLoad = collection.Scenes.Length;
            for (int i = 0; i < collection.Scenes.Length; i++)
            {
                SceneReference sceneReference = collection.Scenes[i];
                string sceneName = sceneReference.SceneName;

                if (loadedScenes.Contains(sceneReference))
                {
                    logger.Warn($"SceneReference {sceneName} already loaded.");
                    scenesToLoad--;
                }
                else
                {
                    sceneReference.SceneRootObjects = new();

                    loadedScenes.Add(sceneReference);
                    logger.Debug($"Loading collection/scene: {collection.name}/{sceneName}");

                    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    op.completed += (asyncOperation) =>
                    {
                        sceneReference.SceneRootObjects.AddRange(SceneManager.GetSceneByName(sceneName).GetRootGameObjects());

                        if (!startActive)
                        {
                            SetSceneActive(sceneReference, false);
                        }

                        logger.Debug($"{sceneName} loaded: {SceneManager.GetSceneByName(sceneName).isLoaded}");
                        scenesToLoad--;
                    };
                }
            }

            while (scenesToLoad > 0)
            {
                await Task.Yield();
            }
        }

        private void SetScenesActive(List<SceneReference> scenes, bool active)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                SetSceneActive(scenes[i], active);
            }
        }


        private void SetSceneActive(SceneReference sceneReference, bool active)
        {
            logger.Debug($"SetSceneActive({sceneReference.SceneName}, {active})");
            foreach (GameObject gameObject in sceneReference.SceneRootObjects)
            {
                gameObject.SetActive(active);
            }
        }

        private void EnterScenes(List<SceneReference> scenes)
        {
            logger.Trace();
            SendMessageToScenes(scenes, "EnterScene");
        }

        private void ExitScenes(List<SceneReference> scenes)
        {
            logger.Trace();
            SendMessageToScenes(scenes, "ExitScene");
        }

        private void SendMessageToScenes(List<SceneReference> scenes, string message)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                foreach (GameObject gameObject in SceneManager.GetSceneByName(scenes[i].SceneName).GetRootGameObjects())
                {
                    gameObject.SendMessage(message, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        /*
        // DestroyCollection destroys the GameObjects that were loaded from any of the Scenes in the provided SceneCollection
        */
        private void UnloadScenes(List<SceneReference> scenes)
        {
            logger.Trace();

            foreach (SceneReference sceneReference in scenes)
            {
                foreach (GameObject gameObject in sceneReference.SceneRootObjects)
                {
                    Destroy(gameObject);
                }

                loadedScenes.Remove(sceneReference);
                SceneManager.UnloadSceneAsync(sceneReference.SceneName);
            }
        }
    }
}