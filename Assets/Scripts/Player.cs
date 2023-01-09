using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public partial class Player : MonoBehaviour
{
    // components
    private Rigidbody2D _body;
    
    // this is the PlayerInput provided by the iput package
    private PlayerInput _input;
    // this is a struct that warps all possible inputs
    // to pass to state execution
    private InputState _inputState = new();
    // all adjustable player parameters
    private ConfigState _config = new();
    // all checks, such as on floor and on wall checks
    private CheckState _checkState = new();

    // cache the walk input for polling
    private Vector2 _walkDirection = Vector2.zero;

    private PlayerState _playerState = new InAirDownState();
    private PlayerState _nextPlayerState; 
    
    // transient state variables, updated every frame
    private enum State { OnFloor, InAir, OnWall }
    private State _state = State.OnFloor;
    private bool _isOnFloor;
    private int _isOnWall; // -1 for left, +1 for right, 0 for not on wall

    // persistent state variables
    public Vector2 _velocity;
    
    
    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
    }

    private void ProcessInput()
    {
        _inputState.Jump = _input.actions["Jump"].triggered;
        if (_inputState.Jump == true)
        {
            Debug.Log("Jump pressed");
        }
        _inputState.JumpHeld = _input.actions["Jump"].IsPressed();
        // input direction is updated by a callback
        _inputState.WalkDirection = _walkDirection;
    }

    private void ProcessChecks()
    {
        // Update transient state variables
        // first reset them
        _checkState.IsOnFloor = false;
        _checkState.IsOnWall = 0;
        
        var contactPoints = new List<ContactPoint2D>();
        _body.GetContacts(contactPoints);
        foreach (var contact in contactPoints)
        {
            if (contact.normal == Vector2.up) { _checkState.IsOnFloor = true; }
            // realistically the player will never touch both walls at the same time
            // this should be fine
            if (contact.normal == Vector2.right) { _checkState.IsOnWall = -1; }
            if (contact.normal == Vector2.left) { _checkState.IsOnWall = 1; }
        }
    }
    
    
    private void Update()
    {
        // process input, stores in _inputState
        // the func is not processing deadband
        // assuming the walk is exactly 0 when joystick is not used
        ProcessInput();
        // check for on wall and on floor
        ProcessChecks();

        // state machine main logic
        if (_nextPlayerState != null)
        {
            _playerState = _nextPlayerState;
            _playerState.OnEnter(this);
        }
        
        _nextPlayerState = _playerState.Execute(this, _inputState, _config, _checkState);
        
        if (_nextPlayerState != null)
        {
            _playerState.OnExit();
        }
        

    }
    

    // triggers that update the input state
    public void Move(InputAction.CallbackContext context)
    {
        _walkDirection = context.ReadValue<Vector2>();
    }

}
