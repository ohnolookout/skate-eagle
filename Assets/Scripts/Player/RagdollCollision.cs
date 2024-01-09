using UnityEngine;

public class RagdollCollision : MonoBehaviour
{
    //[SerializeField] private CollisionManager _collisionManager;
    [SerializeField] private ColliderCategory _category;
    [SerializeField] private TrackingBody _body;
    [SerializeField] private Player _player;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        _player.CollisionManager.AddCollision(collision, _player.MagnitudeDelta(_body), _category);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _player.CollisionManager.RemoveCollision(collision, _player.BodyTracker.Velocity(_body).magnitude, _category);
    }

}
