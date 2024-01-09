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
    public bool turnOnRagdoll = false, ragdoll = false;

    void Start()
    {
        if (ragdollColliders.Length == 0)
        {
            GetCollidersAndBodies();
        }
    }
    private void OnEnable()
    {
        LevelManager.OnGameOver += TurnOnRagdoll;
    }
    private void OnDisable()
    {
        LevelManager.OnGameOver -= TurnOnRagdoll;
    }

    public void TurnOnRagdoll(ILevelManager runManager)
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
        SwitchRigidbodies(ragdollRigidbodies, true, runManager.Player.VectorChange(TrackingBody.PlayerNormal));
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

    void GetCollidersAndBodies()
    {
        ragdollColliders = rigParent.GetComponentsInChildren<Collider2D>();
        ragdollRigidbodies = rigParent.GetComponentsInChildren<Rigidbody2D>();
        ragDollJoints = rigParent.GetComponentsInChildren<Joint2D>();
        ragdollScripts = rigParent.GetComponentsInChildren<RagdollCollision>();

    }


    void SwitchColliders(Collider2D[] colliders, bool isOn)
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

    void SwitchHinges(Joint2D[] joints, bool isOn)
    {
        foreach(var joint in joints)
        {
            joint.enabled = isOn;
        }
    }

    void SwitchScripts(RagdollCollision[] scripts, bool isOn)
    {
        foreach(var script in scripts)
        {
            script.enabled = isOn;
        }
    }

}
