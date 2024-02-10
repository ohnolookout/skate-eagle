using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private RagdollCollision[] ragdollScripts;
    [SerializeField] private Joint2D[] ragDollJoints;
    [SerializeField] private Rigidbody2D[] ragdollRigidbodies;
    [SerializeField] private Collider2D[] ragdollColliders;
    [SerializeField] private GameObject IKParent;
    [SerializeField] private Rigidbody2D[] normalRigidbodies;
    [SerializeField] private Collider2D[] normalColliders;
    [SerializeField] private GameObject rigParent;
    [SerializeField] public Rigidbody2D spine;
    private IPlayer _player;
    public bool turnOnRagdoll = false, ragdoll = false;

    void Start()
    {
        _player = LevelManager.GetPlayer;
        _player.OnDie += TurnOnRagdoll;
    }
    private void OnDisable()
    {
        _player.OnDie -= TurnOnRagdoll;
    }

    public void TurnOnRagdoll()
    {
        if (ragdoll)
        {
            return;
        }
        animator.enabled = false;

        IKParent.SetActive(false);
        SwitchColliders(normalColliders, false);
        SwitchColliders(ragdollColliders, true);
        SwitchRigidbodies(normalRigidbodies, false, new(0, 0));
        SwitchRigidbodies(ragdollRigidbodies, true, _player.MomentumTracker.VectorChange(TrackingType.PlayerNormal));
        normalRigidbodies[0].velocity = new();
        SwitchHinges(ragDollJoints, true);
        ragdoll = true;

    }
    public void TurnOnRagdoll(Vector2 vectorChange)
    {
        if (ragdoll)
        {
            return;
        }
        animator.enabled = false;

        IKParent.SetActive(false);
        SwitchColliders(normalColliders, false);
        SwitchColliders(ragdollColliders, true);
        SwitchRigidbodies(normalRigidbodies, false, new(0, 0));
        SwitchRigidbodies(ragdollRigidbodies, true, vectorChange);
        normalRigidbodies[0].velocity = new();
        SwitchHinges(ragDollJoints, true);
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
        ragdollColliders = rigParent.GetComponentsInChildren<Collider2D>();
        ragdollRigidbodies = rigParent.GetComponentsInChildren<Rigidbody2D>();
        ragDollJoints = rigParent.GetComponentsInChildren<Joint2D>();
        ragdollScripts = rigParent.GetComponentsInChildren<RagdollCollision>();

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
                body.velocity = normalRigidbodies[0].velocity + new Vector2(Mathf.Max(vectorChange.x * 0.1f, 5), Mathf.Max(vectorChange.y * 0.3f, 10));
            }
        }
        spine.angularVelocity = normalRigidbodies[0].angularVelocity * 20;
    }

    static void SwitchHinges(Joint2D[] joints, bool isOn)
    {
        foreach(var joint in joints)
        {
            joint.enabled = isOn;
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
