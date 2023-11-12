using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SkateboardRagdoll : MonoBehaviour
{
    [SerializeField] private CollisionTracker collisionTracker;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(collision, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(collision, false);
    }

}
