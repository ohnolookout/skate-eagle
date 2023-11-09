using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    
    [SerializeField] private Animator animator;
    [SerializeField] private HingeJoint2D[] ragDollJoints;
    [SerializeField] private Rigidbody2D[] ragdollRigidbodies;
    [SerializeField] private Collider2D[] ragdollColliders;
    [SerializeField] private GameObject IKParent;
    [SerializeField] private Rigidbody2D[] normalRigidbodies;
    [SerializeField] private Collider2D[] normalColliders;
    [SerializeField] private GameObject rigParent;
    [SerializeField] public Rigidbody2D spine;
    private EagleScript eagleScript;
    //[SerializeField] private Joint2D fixedJoint;
    public bool turnOnRagdoll = false, ragdoll = false;

    // Start is called before the first frame update
    void Start()
    {
        GetCollidersAndBodies();
        eagleScript = this.GetComponent<EagleScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (turnOnRagdoll)
        {
            TurnOnRagdoll();
            turnOnRagdoll = false;
            ragdoll = true;
            Camera.main.GetComponent<CameraScript>().birdBody = spine;
            Camera.main.GetComponent<CameraScript>().bird = spine.transform;
            GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>().bird = spine.gameObject;
            normalRigidbodies[0].velocity = new();
        }
    }

    public void TurnOnRagdoll()
    {

        animator.enabled = false;

        IKParent.SetActive(false);

        SwitchColliders(ragdollColliders, true);
        SwitchColliders(normalColliders, false);
        SwitchRigidbodies(ragdollRigidbodies, true);
        SwitchRigidbodies(normalRigidbodies, false);
        SwitchHinges(ragDollJoints, true);

    }

    void GetCollidersAndBodies()
    {
        ragdollColliders = rigParent.GetComponentsInChildren<Collider2D>();
        ragdollRigidbodies = rigParent.GetComponentsInChildren<Rigidbody2D>();
        ragDollJoints = rigParent.GetComponentsInChildren<HingeJoint2D>();

    }


    void SwitchColliders(Collider2D[] colliders, bool isOn)
    {
        foreach(var collider in colliders)
        {
            collider.enabled = isOn;
        }
    }

    void SwitchRigidbodies(Rigidbody2D[] bodies, bool isOn)
    {
        foreach (var body in bodies)
        {
            body.isKinematic = !isOn;
            body.velocity = normalRigidbodies[0].velocity;
        }
    }

    void SwitchHinges(HingeJoint2D[] hinges, bool isOn)
    {
        foreach(var hinge in hinges)
        {
            hinge.enabled = isOn;
        }
    }
}
