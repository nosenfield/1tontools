using System;
using System.Threading.Tasks;
using UnityEngine;

namespace OneTon.OverlaySystem
{
    public abstract class OverlayBase : MonoBehaviour
    {
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
        /// Optional hide/cleanup hook.
        /// </summary>
        public virtual void Dismiss()
        {
            Cleanup();
        }

        protected virtual void Cleanup()
        {
            Destroy(gameObject);
        }
    }
}