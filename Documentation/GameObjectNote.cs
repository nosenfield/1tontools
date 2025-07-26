using UnityEngine;

namespace OneTon.Documentation
{
    public class GameObjectNote : MonoBehaviour
    {
        [TextArea(20, 40)][SerializeField] private string note;
    }
}