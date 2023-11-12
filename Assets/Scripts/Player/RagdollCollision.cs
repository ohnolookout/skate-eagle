using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollCollision : MonoBehaviour
{
    [SerializeField] private CollisionTracker collisionTracker;
    [SerializeField] private ColliderCategory category;
    private string colliderName;

    private void Awake()
    {
        colliderName = gameObject.name;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(colliderName, category, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(colliderName, category, false);
    }
}
