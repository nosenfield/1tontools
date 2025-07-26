using OneTon.Logging;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OneTon.CameraManagement
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-1)]
    public class BaseCamera : MonoBehaviour
    {
        private static LogService logger = LogService.Get<BaseCamera>();
        new Camera camera;
        void Awake()
        {
            camera = GetComponent<Camera>();
            OverlayCamera.CameraAdded += AddCameraToStack;
            OverlayCamera.CameraDestroyed += RemoveCameraFromStack;
        }

        void AddCameraToStack(Camera overlayCamera)
        {
            overlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            camera.GetUniversalAdditionalCameraData().cameraStack.Add(overlayCamera);
            camera.GetUniversalAdditionalCameraData().cameraStack.Sort(CameraDepthSort);
        }

        int CameraDepthSort(Camera a, Camera b)
        {
            if (a.depth < b.depth)
            {
                return -1;
            }
            else if (a.depth > b.depth)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        void RemoveCameraFromStack(Camera overlayCamera)
        {
            camera.GetUniversalAdditionalCameraData().cameraStack.Remove(overlayCamera);
        }

        void OnDestroy()
        {
            OverlayCamera.CameraAdded -= AddCameraToStack;
            OverlayCamera.CameraDestroyed -= RemoveCameraFromStack;
        }
    }
}