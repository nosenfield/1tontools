using System;
using System.Threading.Tasks;
using OneTon.ScriptableObjectHelpers;
using UnityEngine;

namespace OneTon.OverlaySystem
{
    public abstract class OverlayBase : MonoBehaviour
    {
        public virtual bool UseUniversalDimmer => true; // override to use custom dimming layer with this overlay

        /// <summary>
        /// Called immediately after instantiation. Used for dependency injection.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Can be called anytime to pass a delayed dependency.
        /// </summary>
        public virtual void Inject(object dependency) { }

        /// <summary>
        /// Optional async display hook for consistency.
        /// </summary>
        public virtual Task ShowAsync()
        {
            gameObject.SetActive(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Entrypoint for overlay dismissal. Dismisses the overlay via the OverlayManager
        /// </summary>
        public void Dismiss()
        {
            ScriptableObjectSingleton<OverlayManager>.Instance.DismissOverlay(this);
        }

        /// <summary>
        /// You can override this method with any additional teardown requirements.
        /// Be sure to include super.Teardown() or Destroy(gameObject) in your override.
        /// </summary>
        internal virtual async Task Teardown()
        {
            Destroy(gameObject);
        }
    }
}