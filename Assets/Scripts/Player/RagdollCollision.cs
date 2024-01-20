using UnityEngine;

public class RagdollCollision : MonoBehaviour
{
    //[SerializeField] private CollisionManager _collisionManager;
    [SerializeField] private ColliderCategory _category;
    [SerializeField] private TrackingType _trackingType;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Player _player;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        _player.CollisionManager.AddCollision(collision, _category, _trackingType);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {       
        _player.CollisionManager.RemoveCollision(collision, _category);
    }

}
