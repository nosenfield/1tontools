
using UnityEngine;

public class ChildCollisionReporter : MonoBehaviour
{
    [SerializeField] bool requireCollisionReceiver;
    [SerializeField] bool reportCollision;

    [SerializeField] bool requireTriggerReceiver;
    [SerializeField] bool reportTrigger;

    public class ChildCollision
    {
        public Collider2D Collider;
        public GameObject Child;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!reportCollision) return;

        SendMessageUpwards("OnChildCollisionEnter2D", collision, requireCollisionReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!reportTrigger) return;

        ChildCollision childCollision = new();
        childCollision.Collider = collider;
        childCollision.Child = this.gameObject;
        SendMessageUpwards("OnChildTriggerEnter2D", childCollision, requireTriggerReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
    }
}