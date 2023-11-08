using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerCollider { LWheel, RWheel, Board, Body };
public class CollisionTracker : MonoBehaviour
{
    private List<PlayerCollider> collidedList;
    private Dictionary<PlayerCollider, float> timerDict = new();
    private float collisionTimer = 0.15f;
    [SerializeField] PlayerAudio playerAudio;


    void Awake()
    {
        collidedList = new() { PlayerCollider.LWheel, PlayerCollider.RWheel };
    }
    void Update()
    {
        PlayerCollider[] colliders = timerDict.Keys.ToArray();
        for(int i = 0; i < colliders.Length; i++)
        {
            PlayerCollider colliderName = colliders[i];
            timerDict[colliderName] += Time.deltaTime;
            //Move onto next iteration if current collider is pending uncollide for less than time limit.
            if (timerDict[colliderName] < collisionTimer)
            {
                continue;
            }
            //If collider waiting to uncollide exceeds time limit, remove it from collided list and timer dict.
            collidedList.Remove(colliderName);
            timerDict.Remove(colliderName);
            //If wheel was removed from collided list and both wheels are no longer collided, start freespin sound.
            playerAudio.Uncollide(colliderName);
        }
    }

    public void SetCollisionTimer(float newTimer)
    {
        collisionTimer = newTimer;
    }

    public void UpdateCollision(Collision2D collision, bool isCollided)
    {
        PlayerCollider collider = ParseCollider(collision);
        //If collision exit, add collider to timerDict to register collision exit after delay.
        if (!isCollided)
        {
            timerDict[collider] = 0;
            return;
        }
        //If collision enter and collider is already on collider list,
        //remove it from timerDict of collisions pending removal so that timer stops
        if(collidedList.Contains(collider))
        {
            timerDict.Remove(collider);
            return;
        }
        playerAudio.Collide(collider);
        //Send collision to player audio and add collision to list of colliders that are collided.
        collidedList.Add(collider);

    }

    public void RemoveAllCollisions()
    {
        timerDict = new();
        collidedList = new();
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
}
