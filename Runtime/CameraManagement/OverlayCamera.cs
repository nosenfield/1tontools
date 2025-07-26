using System;
using UnityEngine;

namespace OneTon.CameraManagement
{
    [RequireComponent(typeof(Camera))]
    public class OverlayCamera : MonoBehaviour
    {
        public static Action<Camera> CameraAdded;
        public static Action<Camera> CameraDestroyed;
        void Awake()
        {
            CameraAdded?.Invoke(GetComponent<Camera>());
        }

        void OnDestroy()
        {
            CameraDestroyed?.Invoke(GetComponent<Camera>());
        }
    }
}