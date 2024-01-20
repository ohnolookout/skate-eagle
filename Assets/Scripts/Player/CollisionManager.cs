using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollisionManager: MonoBehaviour, ICollisionManager
{
    private PendingExitManager _exitManager;
    private Dictionary<ColliderCategory, List<string>> _collidedCategories = new();
    private MomentumTracker _momentumTracker;
    private bool _checkForExits = false;
    [SerializeField] private IPlayer _player;
    private float _uncollideTime = 0.15f, _bodyBoardUncollideTime = 0.5f;
    public Action<ColliderCategory, float> OnCollide { get; set; }
    public Action<ColliderCategory, float> OnUncollide { get; set; }
    public Action OnAirborne { get; set; }

    void OnEnable()
    {
        LevelManager.OnGameOver += _ => RemoveNonragdollColliders();
        //Increase uncollide timer on gameOver to reduce number of incidental sound hits.
        LevelManager.OnGameOver += _ => IncreaseCollisionTimer(CollisionType.Ground, 1.5f);
        LevelManager.OnAttempt += () => _checkForExits = true;
        LevelManager.OnResultsScreen += () => _checkForExits = false;
    }

    void Awake()
    {
        _exitManager = new();
        _exitManager.RemoveCollision += CollisionExitCompleted;
    }


    void Update()
    {
        _exitManager.CheckAllExits();
        //If there are no more active collisions, invoke the OnAirborne event.
        if (_collidedCategories.Count == 0)
        {
            OnAirborne?.Invoke();
        }
    }

    public void AddPlayerEvents(IPlayer player)
    {
        _player = player;
        _momentumTracker = _player.MomentumTracker;
        _player.AddCollision += AddCollision;
        _player.RemoveCollision += RemoveCollision;
    }

    void OnDisable()
    {
        _player.AddCollision -= AddCollision;
        _player.RemoveCollision -= RemoveCollision;
        _exitManager.RemoveCollision -= CollisionExitCompleted;
    }
   
    public void AddCollision(Collision2D collision, ColliderCategory? inputCategory = null, TrackingType? trackingType = null)
    {
        string colliderName = collision.otherCollider.name;
        ColliderCategory category = FindCategory(collision, inputCategory);
        if (!_collidedCategories.ContainsKey(category))
        {
            float magDelta = _momentumTracker.ReboundMagnitudeFromBody(collision.otherRigidbody, ParseTrackingType(trackingType));
            _collidedCategories[category] = new();
            OnCollide?.Invoke(category, magDelta);
        }
        else if (_collidedCategories[category].Contains(colliderName))
        {
            _exitManager.RemovePendingExit(category, colliderName);
            return;
        }
        //Send collision to player audio and add collision to list of colliders that are collided.
        _collidedCategories[category].Add(colliderName);
    }

    private ColliderCategory FindCategory(Collision2D collision, ColliderCategory? inputCategory)
    {
        if (collision.collider.name != "Collider")
        {
            return ColliderCategory.BodyAndBoard;
        }
        else
        {
            return inputCategory ?? ParseCollider(collision);
        }
    }

    private void IncreaseCollisionTimer(CollisionType type, float multiplier)
    {
        _exitManager.SetUncollideTimer(type, _exitManager.GetUncollideTimer(type) * multiplier);
    }

    public void RemoveCollision(Collision2D collision, ColliderCategory? inputCategory = null)
    {
        string colliderName = collision.otherCollider.name;
        ColliderCategory category = FindCategory(collision, inputCategory);
        //Instantly remove collision if it's not the last collider in the cateory to minimize the number of pending exits.
        if (_collidedCategories.ContainsKey(category) && _collidedCategories[category].Count > 1)
        {
            _collidedCategories[category].Remove(colliderName);
            return;
        }
        TimedCollisionExit collisionExit = new(colliderName, category, Time.time, collision.otherRigidbody.velocity.magnitude);
        _exitManager.AddPendingExit(collisionExit);
    }

    private void CollisionExitCompleted(TimedCollisionExit collision)
    {
        if (IsCollided(collision.Category))
        {
            RemoveColliderFromCategory(collision);
        }
    }
    private bool IsCollided(ColliderCategory category)
    {
        return _collidedCategories.ContainsKey(category);
    }

    private void RemoveColliderFromCategory(TimedCollisionExit collision)
    {
        _collidedCategories[collision.Category].Remove(collision.ColliderName);
        RemoveCategoryIfEmpty(collision.Category, collision.MagnitudeAtCollisionExit);
    }
    private void RemoveCategoryIfEmpty(ColliderCategory category, float magnitudeAtCollisionExit)
    {
        if (_collidedCategories[category].Count == 0)
        {
            _collidedCategories.Remove(category);
            OnUncollide?.Invoke(category, magnitudeAtCollisionExit);
        }
    }

    private void RemoveNonragdollColliders()
    {
        _collidedCategories = new();
        _exitManager.ResetAllExits();
    }
    
    private static ColliderCategory ParseCollider(Collision2D collision)
    {
        switch (collision.otherCollider.name)
        {
            case "Player":
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
                return ColliderCategory.Body;
        }
    }

    private static TrackingType ParseTrackingType(TrackingType? inputType)
    {
        if(inputType == null)
        {
            return TrackingType.PlayerNormal;
        }
        return (TrackingType)inputType;
    }
    public bool WheelsCollided
    {
        get => _collidedCategories.ContainsKey(ColliderCategory.LWheel)
        || _collidedCategories.ContainsKey(ColliderCategory.RWheel);
    }
    public bool BothWheelsCollided
    {
        get => _collidedCategories.ContainsKey(ColliderCategory.LWheel)
        && _collidedCategories.ContainsKey(ColliderCategory.RWheel);
    }
    public bool LWheel { get => _collidedCategories.ContainsKey(ColliderCategory.LWheel); }
    public bool RWheel { get => _collidedCategories.ContainsKey(ColliderCategory.RWheel); }
    public bool Body { get => _collidedCategories.ContainsKey(ColliderCategory.Body); }
    public bool Board { get => _collidedCategories.ContainsKey(ColliderCategory.Board); }
    public bool Collided { get => _collidedCategories.Count > 0; }
    public Dictionary<ColliderCategory, List<string>> CollidedCategories { get => _collidedCategories; set => _collidedCategories = value; }

}
