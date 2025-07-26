using UnityEngine;

namespace OneTon.Animation
{
    public class DestroyOnAnimationComplete : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        public void DestroyContainer()
        {
            GameObject.Destroy(container);
        }
    }
}