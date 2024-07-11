using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RagdollController : MonoBehaviour
{
    #region Declarations
    [SerializeField] private Animator _animator;
    [SerializeField] private RagdollCollision[] _ragdollScripts;
    [SerializeField] private Joint2D[] _ragDollJoints;
    [SerializeField] private Rigidbody2D[] _ragdollRigidbodies;
    [SerializeField] private Collider2D[] _ragdollColliders;
    [SerializeField] private GameObject _IKParent, _backWing;
    [SerializeField] private Rigidbody2D[] _normalRigidbodies;
    [SerializeField] private Collider2D[] _normalColliders;
    [SerializeField] private ShadowCaster2D[] _shadows;
    [SerializeField] private GameObject _rigParent;
    [SerializeField] private FixedJoint2D _backwingFixedJoint;
    [SerializeField] private SpringJoint2D _backwingSpringJoint, _lowerwingSpringJoint;
    public Rigidbody2D spine;
    private IPlayer _player;
    public bool turnOnRagdoll = false, ragdoll = false;
    #endregion

    #region Monobehaviors
    private void Awake()
    {
        _player = LevelManager.GetPlayer;

    }

    void Start()
    {
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Die, TurnOnRagdoll);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.PreDie, CheckForStompJoints);
    }
    #endregion

    #region RagdollSwitching
    public void TurnOnRagdoll(IPlayer _ = null)
    {
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.Die, TurnOnRagdoll);
        if (ragdoll)
        {
            return;
        }
        _animator.enabled = false;

        _IKParent.SetActive(false);
        SwitchScripts(_ragdollScripts, true);
        SwitchColliders(_normalColliders, false);
        SwitchColliders(_ragdollColliders, true);
        SwitchRigidbodies(_normalRigidbodies, false, new(0, 0));
        SwitchRigidbodies(_ragdollRigidbodies, true, _player.MomentumTracker.VectorChange(TrackingType.PlayerNormal));
        _normalRigidbodies[0].velocity = new();
        SwitchHinges(_ragDollJoints, true);
        ragdoll = true;

    }

    private void CheckForStompJoints(IPlayer _ = null)
    {
        if (!_player.Stomping)
        {
            _backwingFixedJoint.enabled = true;
            _backwingSpringJoint.enabled = true;
            _lowerwingSpringJoint.enabled = true;
        }
    }

    static void SwitchColliders(Collider2D[] colliders, bool isOn)
    {
        foreach (var collider in colliders)
        {
            collider.enabled = isOn;
        }
    }

    void SwitchRigidbodies(Rigidbody2D[] bodies, bool isOn, Vector2 vectorChange)
    {
        foreach (var body in bodies)
        {
            body.isKinematic = !isOn;
            if (isOn)
            {
                body.velocity = _normalRigidbodies[0].velocity + new Vector2(Mathf.Max(vectorChange.x * 0.1f, 5), Mathf.Max(vectorChange.y * 0.3f, 10));
            }
        }
        spine.angularVelocity = _normalRigidbodies[0].angularVelocity * 20;
    }

    static void SwitchHinges(Joint2D[] joints, bool isOn)
    {
        foreach (var joint in joints)
        {
            joint.enabled = isOn;
        }
    }

    static void SwitchScripts(RagdollCollision[] scripts, bool isOn)
    {
        foreach (var script in scripts)
        {
            script.enabled = isOn;
        }
    }

    #endregion

    #region Colliders
    public void SendCollision(Collision2D collision, ColliderCategory category, TrackingType tracking, bool isEnter)
    {
        if (isEnter)
        {
            _player.CollisionManager.AddCollision(collision, _player.MomentumTracker, category, tracking);
        }
        else
        {
            _player.CollisionManager.RemoveCollision(collision, category);
        }
    }

    void GetCollidersAndBodies()
    {
        _ragdollColliders = _rigParent.GetComponentsInChildren<Collider2D>();
        _ragdollRigidbodies = _rigParent.GetComponentsInChildren<Rigidbody2D>();
        _ragDollJoints = _rigParent.GetComponentsInChildren<Joint2D>();
        _ragdollScripts = _rigParent.GetComponentsInChildren<RagdollCollision>();

    }

    #endregion

}
