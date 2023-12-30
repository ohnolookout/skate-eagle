using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollCollision : MonoBehaviour
{
    [SerializeField] private CollisionTracker collisionTracker;
    [SerializeField] private ColliderCategory category;
    [SerializeField] private Rigidbody2D _rigidbody;
    private Vector2 _lastVector; 

    private void Awake()
    {
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        _lastVector = _rigidbody.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Adding collision for object name: " + collision.otherCollider.name);
        collisionTracker.AddCollision(collision, MagnitudeDelta());
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collisionTracker.RemoveCollision(collision, _rigidbody.velocity.magnitude);
    }

    public float MagnitudeDelta()
    {
        Vector2 delta = VectorChange;
        float forceDelta = 0;
        if (_lastVector.x > 0 && delta.x < 0)
        {
            forceDelta -= delta.x;
        }
        else if (_lastVector.x < 0 && delta.x > 0)
        {
            forceDelta += delta.x;
        }
        forceDelta += delta.y;
        return forceDelta;
    }

    public Vector2 VectorChange { get => new(_rigidbody.velocity.x - _lastVector.x, _rigidbody.velocity.y - _lastVector.y); }
}
