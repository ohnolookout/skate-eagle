using UnityEngine;

public class RagdollCollision : MonoBehaviour
{
    //[SerializeField] private CollisionManager _collisionManager;
    [SerializeField] private ColliderCategory _category;
    [SerializeField] private TrackingType _trackingType;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private RagdollController _ragdollController;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _ragdollController.SendCollision(collision, _category, _trackingType, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _ragdollController.SendCollision(collision, _category, _trackingType, false);
    }

}
