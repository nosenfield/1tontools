using OneTon.ScriptableObjectHelpers;
using OneTon.Logging;
using UnityEngine;

namespace OneTon.SceneManagement
{

    public abstract class AbstractSceneTransition : MonoBehaviour
    {
        public static LogService logger = LogService.Get<AbstractSceneTransition>();

        protected virtual void EnterScene()
        {
            logger.Warn($"No custom EnterScene behaviour for {this.name}.");
        }

        protected virtual void ExitScene()
        {
            ScriptableObjectSingleton<MultiSceneManager>.Instance.SceneTransitionComplete(this);
        }
    }
}