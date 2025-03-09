using UnityEngine;
using System.Threading;
using TMPro;

public class Player : MonoBehaviour, IPlayer
{
    #region Declarations
    [SerializeField] private Rigidbody2D _body, _ragdollBody, _ragdollBoard;
    [SerializeField] private int _initialStompCharge = 0;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private CollisionManager _collisionManager;
    private JumpManager _jumpManager;
    private Animator _animator;
    private bool _facingForward = true, _isRagdoll = false, _doLanding = true, _stomping = false;
    private PlayerParameters _params;
    private PlayerStateMachine _stateMachine;
    private CancellationTokenSource _boostTokenSource, _freezeTokenSource;
    private CancellationToken _boostToken, _freezeToken;
    private InputEventController _inputEvents;
    private PlayerEventAnnouncer _eventAnnouncer;
    private PlayerAnimationManager _animationManager;

    public MomentumTracker MomentumTracker { get; set; }
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
    public Transform Transform { get => transform; }
    public CancellationToken FreezeToken { get => _freezeToken; }
    public CancellationTokenSource BoostTokenSource { get => _boostTokenSource; }
    public CancellationTokenSource FreezeTokenSource { get => _freezeTokenSource; }
    #endregion

    #region Monobehaviours
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _params = new(_initialStompCharge);
        _stateMachine = new(this);
        _jumpManager = new(this);
        _eventAnnouncer = new(this);
        _animationManager = new(this, _animator);
        MomentumTracker = new(_body, _ragdollBoard, _ragdollBody, 12);
        _boostTokenSource = new();
        _boostToken = _boostTokenSource.Token;
        _freezeTokenSource = new();
        _freezeToken = _freezeTokenSource.Token;
    }

    private void Start()
    {
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.centerOfMass = new Vector2(0, -2f);
        _stateMachine.InitializeState(PlayerStateType.Standby);
        _animator.SetBool("Airborne", false);
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

    private void OnEnable()
    {
        _collisionManager.AddPlayer(this);
        _inputEvents = new(InputType.Player);
    }
    private void OnDisable()
    {
        _inputEvents.DisableInputs();
        _eventAnnouncer.ClearActions();
        CancelAsyncTokens();
    }
    #endregion

    #region Manual Triggers
    public void TriggerBoost(float boostValue, float boostMultiplier)
    {
        PlayerAsyncUtility.AddBoost(_boostToken, _body, boostValue, boostMultiplier);
        PlayerAsyncUtility.BoostTrail(_boostToken, _trail, _facingForward);
    }
    
    public void InvokeEvent(PlayerEvent eventType)
    {
        Debug.Log("Invoking event: " + eventType);
        _eventAnnouncer.InvokeAction(eventType);
    }
    
    public void InvokeDismount()
    {
        _eventAnnouncer.InvokeAction(PlayerEvent.Dismount);
    }

    public void InvokeBodySound()
    {
        _eventAnnouncer.InvokeAction(PlayerEvent.BodySound);
    }

    public void InvokeLandSound()
    {
        _eventAnnouncer.InvokeAction(PlayerEvent.LandSound);
    }

    public void CancelAsyncTokens()
    {
        _boostTokenSource.Cancel();
        _freezeTokenSource.Cancel();
    }

    public void SwitchDirection()
    {
        _animator.SetBool("FacingForward", _facingForward);
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _eventAnnouncer.InvokeAction(PlayerEvent.SwitchDirection);
    }

    #endregion
}
