using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    
    [SerializeField] private Animator animator;
    [SerializeField] private Joint2D[] ragDollJoints;
    [SerializeField] private Rigidbody2D[] ragdollRigidbodies;
    [SerializeField] private Collider2D[] ragdollColliders;
    [SerializeField] private GameObject IKParent;
    [SerializeField] private Rigidbody2D[] normalRigidbodies;
    [SerializeField] private Collider2D[] normalColliders;
    [SerializeField] private GameObject rigParent;
    [SerializeField] public Rigidbody2D spine;
    [SerializeField] private Rigidbody2D[] feet;
    //[SerializeField] private Joint2D fixedJoint;
    public bool turnOnRagdoll = false, ragdoll = false;

    // Start is called before the first frame update
    void Start()
    {
        GetCollidersAndBodies();
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
        feet[0].gameObject.transform.Translate( new Vector2(0, 1));
        SwitchColliders(ragdollColliders, true);
        SwitchRigidbodies(normalRigidbodies, false, new(0, 0));
        SwitchRigidbodies(ragdollRigidbodies, true, vectorChange);
        normalRigidbodies[0].velocity = new();
        SwitchHinges(ragDollJoints, true);
        ragdoll = true;
        //SeperateFootAndBoard();

    }

    void GetCollidersAndBodies()
    {
        ragdollColliders = rigParent.GetComponentsInChildren<Collider2D>();
        ragdollRigidbodies = rigParent.GetComponentsInChildren<Rigidbody2D>();
        ragDollJoints = rigParent.GetComponentsInChildren<Joint2D>();

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

    void SeperateFootAndBoard()
    {
        foreach(var foot in feet)
        {
            foot.AddForce(new Vector2(0, 2));
        }
    }
}
