using UnityEngine;
using System.Threading;

public class Player : MonoBehaviour, IPlayer
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
    private InputEventController _inputEvents;
    private PlayerEventAnnouncer _eventAnnouncer;
    private PlayerAnimationManager _animationManager;
    public MomentumTracker MomentumTracker { get; set; }


    private void Awake()
    {
        AssignComponents();
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.centerOfMass = new Vector2(0, -2f);
        LevelManager.OnRestart += () => _tokenSource.Cancel();
    }

    private void Start()
    {
        _stateMachine.InitializeState(PlayerStateType.Standby);
        _animator.SetBool("Airborne", false);
    }

    private void OnEnable()
    {
        _collisionManager.AddPlayer(this);
        _inputEvents = new(InputType.Player);
    }
    private void OnDisable()
    {
        //_stateMachine.ExitStates();
        _eventAnnouncer.ClearActions();
    }

    private void AssignComponents()
    {
        _animator = GetComponent<Animator>();
        _params = new(_initialStompCharge);
        _stateMachine = new(this);
        _jumpManager = new(this);
        _eventAnnouncer = new(this);
        _animationManager = new(this, _animator);
        MomentumTracker = new(_body, _ragdollBoard, _ragdollBody, 12);
        _tokenSource = new();
        _boostToken = _tokenSource.Token;
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
        _eventAnnouncer.EnterCollision(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        _eventAnnouncer.ExitCollision(collision);
    }

    public void TriggerBoost(float boostValue, float boostMultiplier)
    {
        PlayerAsyncUtility.AddBoost(_boostToken, _body, boostValue, boostMultiplier);
        PlayerAsyncUtility.BoostTrail(_boostToken, _trail, _facingForward);
    }

    public void DismountSound()
    {
        _eventAnnouncer.InvokeAction(PlayerEvent.Dismount);
    }

    public void CancelAsyncTokens()
    {
        _tokenSource.Cancel();
    }

    public void SwitchDirection()
    {
        _animator.SetBool("FacingForward", _facingForward);
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }


    public ICollisionManager CollisionManager { get => _collisionManager; }
    public Rigidbody2D NormalBody { get => _body; }
    public Rigidbody2D RagdollBoard { get => _ragdollBoard; }
    public Rigidbody2D RagdollBody { get => _ragdollBody; }
    public PlayerParameters Params { get => _params; }
    public bool FacingForward { get => _facingForward; set => _facingForward = value; }
    public bool Collided { get => _collisionManager.Collided; }
    public bool Stomping { get => _stomping; set => _stomping = value; }
    public bool IsRagdoll { get => _isRagdoll; set => _isRagdoll = value; }
    public InputEventController InputEvents { get => _inputEvents; set => _inputEvents = value; }
    public bool DoLanding { get => _doLanding; set => _doLanding = value; }
    public JumpManager JumpManager { get => _jumpManager; set => _jumpManager = value; }
    public PlayerEventAnnouncer EventAnnouncer { get => _eventAnnouncer; set => _eventAnnouncer = value; }
    public TrailRenderer Trail { get => _trail; }
    public PlayerAnimationManager AnimationManager { get => _animationManager; set => _animationManager = value; }
}
