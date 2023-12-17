using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    PlayerBaseState _currentState;
    PlayerStateFactory _states;
    public LiveRunManager runManager;
    public PlayerController playerController;
    public Animator animator;
    public Rigidbody2D rigidEagle;

    public PlayerBaseState CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            _currentState = value;
        }
    }

    void Awake()
    {
        runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        _states = new PlayerStateFactory(this);
        _currentState = _states.Inactive();
        _currentState.EnterState();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
