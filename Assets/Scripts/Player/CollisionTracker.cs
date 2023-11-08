using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerCollider { LWheel, RWheel, Board, Body };
public class CollisionTracker : MonoBehaviour
{
    private List<PlayerCollider> collidedList;
    private OrderedDictionary<PlayerCollider, float> collisionTimes = new();
    private float uncollideTime = 0.15f;
    [SerializeField] PlayerAudio playerAudio;
    [SerializeField] EagleScript eagleScript;


    void Awake()
    {
        collidedList = new() { PlayerCollider.LWheel, PlayerCollider.RWheel };
        eagleScript = gameObject.GetComponent<EagleScript>();
    }

    //Set float in collisionTimers to startTime instead of current timer. Then look at currentTime-startTime.
    void Update()
    {
        while (collisionTimes.Count > 0)
        {
            if (Time.time - collisionTimes.Value(0) < uncollideTime)
            {
                break;
            }
            collidedList.Remove(collisionTimes.Key(0));
            playerAudio.Uncollide(collisionTimes.Key(0));
            collisionTimes.Remove(0);
            if(collidedList.Count == 0)
            {
                eagleScript.JumpCount = 1;
            }
        }
    }

    public void SetCollisionTimer(float newTimer)
    {
        uncollideTime = newTimer;
    }

    public void UpdateCollision(Collision2D collision, bool isCollided)
    {
        PlayerCollider collider = ParseCollider(collision);
        //If collision exit, add collider to timerDict to register collision exit after delay.
        if (!isCollided)
        {
            collisionTimes.Add(collider, Time.time);
            return;
        }
        //If collision enter and collider is already on collider list,
        //remove it from timerDict of collisions pending removal so that timer stops
        if(collidedList.Contains(collider))
        {
            collisionTimes.Remove(collider);
            return;
        }
        playerAudio.Collide(collider);
        //Send collision to player audio and add collision to list of colliders that are collided.
        collidedList.Add(collider);

    }

    public void RemoveAllCollisions()
    {
        collidedList = new();
        collisionTimes = new();
    }

    private PlayerCollider ParseCollider(Collision2D collision)
    {
        switch (collision.otherCollider.name)
        {
            case "Skate Eagle":
                return PlayerCollider.Body;
            case "LWheelCollider":
                return PlayerCollider.LWheel;
            case "RWheelCollider":
                return PlayerCollider.RWheel;
            case "BoardCollider":
                return PlayerCollider.Board;
            default:
                return PlayerCollider.Board;
        }
    }

    public bool WheelsCollided
    {
        get
        {
            return collidedList.Contains(PlayerCollider.LWheel) || collidedList.Contains(PlayerCollider.RWheel);
        }
    }

    public bool LWheel
    {
        get
        {
            return collidedList.Contains(PlayerCollider.LWheel);
        }
    }

    public bool RWheel
    {
        get
        {
            return collidedList.Contains(PlayerCollider.RWheel);
        }
    }

    public bool Body
    {
        get
        {
            return collidedList.Contains(PlayerCollider.Body);
        }
    }

    public bool Board
    {
        get
        {
            return collidedList.Contains(PlayerCollider.Board);
        }
    }

    public bool Collided
    {
        get
        {
            return collidedList.Count > 0;
        }
    }

    private struct TimedCollision
    {
        public PlayerCollider colliderName;
        public float collisionTime;

        public TimedCollision(PlayerCollider name, float time)
        {
            colliderName = name;
            collisionTime = time;
        }
    }
}
