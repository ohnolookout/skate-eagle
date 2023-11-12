using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ColliderCategory { LWheel, RWheel, Board, Body };
public class CollisionTracker : MonoBehaviour
{
    private OrderedDictionary<string, TimedCollision> pendingUncollisions = new();
    private Dictionary<ColliderCategory, List<string>> collidedCategories = new();
    private float uncollideTime = 0.15f;
    [SerializeField] PlayerAudio playerAudio;
    [SerializeField] EagleScript eagleScript;


    void Awake()
    {
        collidedCategories[ColliderCategory.LWheel] = new List<string> { "LWheelCollider" };
        collidedCategories[ColliderCategory.RWheel] = new List<string> { "RWheelCollider" };
        eagleScript = gameObject.GetComponent<EagleScript>();
        playerAudio = eagleScript.Audio;
    }

    //Set float in collisionTimers to startTime instead of current timer. Then look at currentTime-startTime.
    void FixedUpdate()
    {
        //While there are pending collisions, continue checking the first pending collision
        //until one is found that has not exceeded the time limit
        while (pendingUncollisions.Count > 0)
        {
            TimedCollision collision = pendingUncollisions.Value(0);
            if (Time.time - collision.Time < uncollideTime)
            {
                //If first collision in list does not exceed time limit, break loop
                break;
            }
            //Debug.Log($"{collision.ColliderName} has exceeded timer. Removing from {collision.Category}.");
            //If collision does exceed time limit, remove it from the list of colliders that are currently collided in its category
            //And remove it from the list of pending collisions.
            collidedCategories[collision.Category].Remove(collision.ColliderName);
            pendingUncollisions.Remove(0);
            //If the current category has no more active collisions, remove it from the dictionary and send call to audio
            if(collidedCategories[collision.Category].Count == 0)
            {
                collidedCategories.Remove(collision.Category);
                playerAudio.Uncollide(collision.Category);
                //Debug.Log($"Category {collision.Category} has no colliders. Removing category from collidedCategories.");
            }
            //If no categories are collided, send eagle into airborne mode
            if(collidedCategories.Count == 0)
            {
                eagleScript.GoAirborne();
            }
        }
    }

    public void SetCollisionTimer(float newTimer)
    {
        uncollideTime = newTimer;
    }

    public void UpdateCollision(Collision2D collision, bool isCollided)
    {
        UpdateCollision(collision.otherCollider.name, ParseCollider(collision), isCollided);
    }

    public void UpdateCollision(string name, ColliderCategory category, bool isCollided)
    {
        //If collision exit, add collider to timerDict to register collision exit after delay.
        if (!isCollided)
        {
            pendingUncollisions.Add(name, new(name, category, Time.time));
            return;
        }
        //If collision enter and collider is already on collider list,
        //remove it from timerDict of collisions pending removal so that timer stops
        if (!collidedCategories.ContainsKey(category))
        {
            collidedCategories[category] = new();
            playerAudio.Collide(category);
        } else if (collidedCategories[category].Contains(name))
        {
            pendingUncollisions.Remove(name);
            return;
        }
        //Send collision to player audio and add collision to list of colliders that are collided.
        collidedCategories[category].Add(name);
        //Debug.Log($"Updated category {category} to add {name}");

    }

    public void RemoveAllCollisions()
    {
        //pendingUncollisions = new();
        //collidedCategories = new();
    }

    private ColliderCategory ParseCollider(Collision2D collision)
    {
        switch (collision.otherCollider.name)
        {
            case "Skate Eagle":
                return ColliderCategory.Body;
            case "LWheelCollider":
            case "LWheelRagdoll":
                return ColliderCategory.LWheel;
            case "RWheelCollider":
            case "RWheelRagdoll":
                return ColliderCategory.RWheel;
            case "BoardCollider":
            case "BoardRagdoll":
                return ColliderCategory.Board;
            default:
                return ColliderCategory.Board;
        }
    }

    public void RemoveNonragdollColliders()
    {
        if (collidedCategories.ContainsKey(ColliderCategory.Body)){
            collidedCategories[ColliderCategory.Body].Remove("Skate Eagle");
        }
        if (collidedCategories.ContainsKey(ColliderCategory.LWheel))
        {
            collidedCategories[ColliderCategory.Body].Remove("LWheelCollider");
        }
        if (collidedCategories.ContainsKey(ColliderCategory.RWheel))
        {
            collidedCategories[ColliderCategory.Body].Remove("RWheelCollider");
        }
        if (collidedCategories.ContainsKey(ColliderCategory.Board))
        {
            collidedCategories[ColliderCategory.Body].Remove("BoardCollider");
        }
    }

    public bool WheelsCollided
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.LWheel) || collidedCategories.ContainsKey(ColliderCategory.RWheel);
        }
    }

    public bool LWheel
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.LWheel);
        }
    }

    public bool RWheel
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.RWheel);
        }
    }

    public bool Body
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.Body);
        }
    }

    public bool Board
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.Board);
        }
    }

    public bool Collided
    {
        get
        {
            return collidedCategories.Count > 0;
        }
    }

    private struct TimedCollision
    {
        public ColliderCategory Category;
        public string ColliderName;
        public float Time;

        public TimedCollision(string colliderName, ColliderCategory category, float time)
        {
            ColliderName = colliderName;
            Category = category;
            Time = time;
        }
    }
}
