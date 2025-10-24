using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager: MonoBehaviour, ICollisionManager
{
    private PendingExitManager _exitManager;
    private bool _checkForExits = false;
    [SerializeField] private IPlayer _player;

    //Tracks number of active collisions for each collider category.
    //-1 indicates no active collision, 0 indicates a pending exit.
    private Dictionary<ColliderCategory, int> _colliderCategories = new ()
    {
       { ColliderCategory.LWheel, -1 },
       { ColliderCategory.RWheel, -1 },
       { ColliderCategory.Body, -1 },
       { ColliderCategory.Board, -1 },
       { ColliderCategory.BodyAndBoard, -1 }
    };
    public Action<ColliderCategory, float> OnCollide { get; set; }
    public Action<Collider2D> OnNewColliderEnter { get; set; }
    public Action<ColliderCategory, float> OnUncollide { get; set; }
    public Action OnAirborne { get; set; }

    void OnEnable()
    {
        LevelManager.OnGameOver += RemoveNonragdollColliders;
        //Increase uncollide timer on gameOver to reduce number of incidental sound hits.
        LevelManager.OnGameOver += () => IncreaseCollisionTimer(CollisionType.Ground, 1.5f);
        LevelManager.OnAttempt += () => _checkForExits = true;
        LevelManager.OnResultsScreen += () => _checkForExits = false;
    }


    void Awake()
    {
        _exitManager = new();
        _exitManager.ExitCollision += CollisionExitCompleted;
    }


    void Update()
    {
        if (_checkForExits)
        {
            _exitManager.CheckAllExits();
        }
    }

    public void AddPlayer(IPlayer player)
    {
        _player = player;
        _player.EventAnnouncer.SubscribeToAddCollision(AddCollision);
        _player.EventAnnouncer.SubscribeToRemoveCollision(RemoveCollision);
    }

    void OnDisable()
    {
        _exitManager.ExitCollision -= CollisionExitCompleted;
        OnCollide = null;
        OnUncollide = null;
    }
   
    public void AddCollision(Collision2D collision, MomentumTracker momentumTracker, ColliderCategory category, TrackingType trackingType)
    {
        category = FindCategory(collision, category);

        int collisionCount = _colliderCategories[category];

        if (collisionCount < 0)
        {
            _colliderCategories[category] = 0;

            float magDelta = momentumTracker.ReboundMagnitudeFromBody(collision.otherRigidbody, trackingType);
            OnCollide?.Invoke(category, magDelta);

        } else if (collisionCount == 0)
        {
            //If collision count is 0, send to exit manager to remove pending collision exit.
            _exitManager.AddCollision(category);
        }

        //Increment collision count
        _colliderCategories[category]++;
    }

    private ColliderCategory FindCategory(Collision2D collision, ColliderCategory inputCategory)
    {
        if (collision.collider.name != "Collider" && collision.collider.name != "Bottom Collider")
        {
            return ColliderCategory.BodyAndBoard;
        }
        else
        {
            return inputCategory;
        }
    }

    private void IncreaseCollisionTimer(CollisionType type, float multiplier)
    {
        _exitManager.SetUncollideTimer(type, _exitManager.GetUncollideTimer(type) * multiplier);
    }

    public void RemoveCollision(Collision2D collision, ColliderCategory inputCategory)
    {
        ColliderCategory category = FindCategory(collision, inputCategory);

        _colliderCategories[category]--;

        if (_colliderCategories[category] == 0)
        {
            TimedCollisionExit collisionExit = new(category, Time.time, collision.otherRigidbody.linearVelocity.magnitude);
            _exitManager.AddPendingExit(collisionExit);
        }

    }

    private void CollisionExitCompleted(TimedCollisionExit collision)
    {
        _colliderCategories[collision.Category] = -1;
        OnUncollide?.Invoke(collision.Category, collision.MagnitudeAtCollisionExit);

        if(!WheelsCollided)
        {
            OnAirborne?.Invoke();
        }
    }
    public bool IsCollided(ColliderCategory category)
    {
        return _colliderCategories[category] > -1;
    }

    private void RemoveNonragdollColliders()
    {
        // Reset all collider categories to -1
        _colliderCategories = new()
        {
           { ColliderCategory.LWheel, -1 },
           { ColliderCategory.RWheel, -1 },
           { ColliderCategory.Body, -1 },
           { ColliderCategory.Board, -1 },
           { ColliderCategory.BodyAndBoard, -1 }
        };

        _exitManager.ResetAllExits();
    }

    public bool WheelsCollided
    {
        get => IsCollided(ColliderCategory.LWheel)
            || IsCollided(ColliderCategory.RWheel);
    }
    public bool BothWheelsCollided
    {
        get => IsCollided(ColliderCategory.LWheel)
            && IsCollided(ColliderCategory.RWheel);
    }

}
