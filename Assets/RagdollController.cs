using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private RagdollCollision[] _ragdollScripts;
    [SerializeField] private Joint2D[] _ragDollJoints;
    [SerializeField] private Rigidbody2D[] _ragdollRigidbodies;
    [SerializeField] private Collider2D[] _ragdollColliders;
    [SerializeField] private GameObject _IKParent;
    [SerializeField] private Rigidbody2D[] _normalRigidbodies;
    [SerializeField] private Collider2D[] _normalColliders;
    [SerializeField] private ShadowCaster2D[] _shadows;
    [SerializeField] private GameObject _rigParent;
    public Rigidbody2D spine;
    private IPlayer _player;
    public bool turnOnRagdoll = false, ragdoll = false;

    private void Awake()
    {
        _player = LevelManager.GetPlayer;

    }

    void Start()
    {
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Die, TurnOnRagdoll);
    }

    public void TurnOnRagdoll(IPlayer _ = null)
    {
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
        //SwitchShadows(_shadows, true);
        _normalRigidbodies[0].velocity = new();
        SwitchHinges(_ragDollJoints, true);
        ragdoll = true;

    }

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


    static void SwitchColliders(Collider2D[] colliders, bool isOn)
    {
        foreach(var collider in colliders)
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
        foreach(var joint in joints)
        {
            joint.enabled = isOn;
        }
    }

    static void SwitchShadows(ShadowCaster2D[] shadows, bool isSelfShadow)
    {
        foreach (var shadow in shadows)
        {
            if (isSelfShadow) {
                shadow.castingOption = ShadowCaster2D.ShadowCastingOptions.CastAndSelfShadow;

            } else
            {
                shadow.castingOption = ShadowCaster2D.ShadowCastingOptions.CastShadow;
            }
        }
    }

    static void SwitchScripts(RagdollCollision[] scripts, bool isOn)
    {
        foreach(var script in scripts)
        {
            script.enabled = isOn;
        }
    }

}
