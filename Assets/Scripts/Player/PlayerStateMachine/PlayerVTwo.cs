using System.Collections;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

public class PlayerVTwo : MonoBehaviour, IPlayer
{
    [SerializeField] private Rigidbody2D _body, _ragdollBody, _ragdollBoard;
    [SerializeField] private int _initialStompCharge = 0;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private CollisionManager _collisionManager;
    private JumpManager _jumpManager;
    private Animator _animator;
    private bool _facingForward = true, _isRagdoll = false, _doLanding = true, _stomping = false;
    private PlayerParameters _params;
    private PlayerStateMachine _stateMachine;
    private CancellationTokenSource _tokenSource;
    private CancellationToken _boostToken;
    private ILevelManager _levelManager;
    private InputEventController _inputEvents;
    public MomentumTracker MomentumTracker { get; set; }
    public Action FinishStop { get; set; }
    public Action OnStomp { get; set; }
    public Action OnDismount { get; set; }
    public Action<IPlayer, double> OnFlip { get; set; }
    public Action<IPlayer> OnStartWithStomp { get; set; }
    public Action<IPlayer> OnJump { get; set; }
    public Action<IPlayer> OnSlowToStop { get; set; }
    public Action OnDie { get; set; }
    public Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> AddCollision { get; set; }
    public Action<Collision2D, ColliderCategory> RemoveCollision { get; set; }
    public Action OnStartAttempt { get; set; }


    private void Awake()
    {
        AssignComponents();
        _jumpManager = new(this);
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.centerOfMass = new Vector2(0, -2f);
        MomentumTracker = new(_body, _ragdollBoard, _ragdollBody, 12);
        _tokenSource = new();
        _boostToken = _tokenSource.Token;
        LevelManager.OnRestart += () => _tokenSource.Cancel();
    }

    private void Start()
    {
        _stateMachine.InitializeState(PlayerStateType.Standby);
        /*
        if (_initialStompCharge > 0)
        {
            OnStartWithStomp?.Invoke(this);
        }*/
    }

    private void OnEnable()
    {
        _collisionManager.AddPlayer(this);
        _inputEvents = new(InputType.Player);
    }
    private void OnDisable()
    {
        _stateMachine.ExitStates();
    }

    private void AssignComponents()
    {
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
        _animator = GetComponent<Animator>();
        _params = new(_initialStompCharge);
        _stateMachine = new(this);
    }

    void Update()
    {
        _stateMachine.UpdateCurrentState();
    }

    private void FixedUpdate()
    {
        _stateMachine.FixedUpdateCurrentStates();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        ColliderCategory category = ParseColliderName(collision.otherCollider.name);
        AddCollision?.Invoke(collision, MomentumTracker, category, TrackingType.PlayerNormal);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        RemoveCollision?.Invoke(collision, ParseColliderName(collision.otherCollider.name));
    }

    private static ColliderCategory ParseColliderName(string colliderName)
    {
        switch (colliderName)
        {
            case "LWheelCollider":
                return ColliderCategory.LWheel;
            case "RWheelCollider":
                return ColliderCategory.RWheel;
            case "BoardCollider":
                return ColliderCategory.Board;
            default:
                return ColliderCategory.Body;
        }
    }


    public void Die()
    {
        OnDie?.Invoke();
        _inputEvents.DisableInputs();
        _trail.emitting = false;
        _isRagdoll = true;
        _tokenSource.Cancel();
    }

    public void SlowToStop()
    {
        _levelManager.Finish();
        OnSlowToStop?.Invoke(this);
        _trail.emitting = false;
    }

    public void TriggerBoost(float boostValue, float boostMultiplier)
    {
        PlayerAsyncUtility.AddBoost(_boostToken, _body, boostValue, boostMultiplier);
        PlayerAsyncUtility.BoostTrail(_boostToken, _trail, _facingForward);
    }

    public void DismountSound()
    {
        OnDismount?.Invoke();
    }



    public ICollisionManager CollisionManager { get => _collisionManager; }
    public Rigidbody2D NormalBody { get => _body; }
    public Rigidbody2D RagdollBoard { get => _ragdollBoard; }
    public Rigidbody2D RagdollBody { get => _ragdollBody; }
    public Animator Animator { get => _animator; }
    public PlayerParameters Params { get => _params; }
    public bool FacingForward { get => _facingForward; set => _facingForward = value; }
    public bool Collided { get => _collisionManager.Collided; }
    public bool Stomping { get => _stomping; set => _stomping = value; }
    public bool IsRagdoll { get => _isRagdoll; set => _isRagdoll = value; }
    public InputEventController InputEvents { get => _inputEvents; set => _inputEvents = value; }
    public bool DoLanding { get => _doLanding; set => _doLanding = value; }
    public JumpManager JumpManager { get => _jumpManager; set => _jumpManager = value; }
}
