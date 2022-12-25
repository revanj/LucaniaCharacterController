using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class Player : MonoBehaviour
{
    // components
    private Rigidbody2D _body;
    private PlayerInput _input;

    // config vars
    private const float WalkSpeed = 4f;
    private const float Gravity = 0.2f;
    private const float MaxFallSpeed = 0.1f;
    private const float JumpForce = 0.1f;
    private const float JumpInc = 2f;
    
    // for the resolution of external forces.
    // may or may not be useful
    private Dictionary<Object, Vector2> _constForceDict = new();
    private Dictionary<Object, Vector2> _transientForceDict = new();
    
    // input variables
    private Vector2 _walkDirection = Vector2.zero;
    private bool _jump = false;
    
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

    private void Update()
    {
        // process input
        // not processing deadband, assuming the walk is 0 when joystick is not held
        var walkVec = new Vector2(Math.Sign(_walkDirection.x) * WalkSpeed * Time.deltaTime, 0);
        var jump = _input.actions["Jump"].triggered;
        var jumpHeld = _input.actions["Jump"].IsPressed();
        
        // Update transient state variables
        // first reset them
        _isOnFloor = false;
        _isOnWall = 0;
        
        List<ContactPoint2D> contactPoints = new List<ContactPoint2D>();
        _body.GetContacts(contactPoints);
        foreach (var contact in contactPoints)
        {
            if (contact.normal == Vector2.up) { _isOnFloor = true; }
            
            // realistically the player will never touch both walls at the same time
            // this should be fine
            if (contact.normal == Vector2.right) { _isOnWall = -1; }
            if (contact.normal == Vector2.left) { _isOnWall = 1; }
        }
        // calculate current state
        var currentState = State.OnFloor;
        if (_isOnFloor) { currentState = State.OnFloor; }
        else {
            if (_isOnWall != 0) { currentState = State.OnWall; } 
            else { currentState = State.InAir; }
        }

        switch (currentState)
        {
            case State.OnFloor:
                if (_velocity.y < 0) { _velocity.y = 0; }
                if (jump) {
                    _velocity.y += JumpForce;
                    _isOnFloor = false;
                }
                _body.MovePosition(_body.position + _velocity + walkVec);
                break;
            case State.OnWall:
                break;
            case State.InAir:
                // handle gravity
                Debug.Log(_velocity);
                _velocity += Vector2.down * (Gravity * Time.deltaTime);
                if (_velocity.y < -MaxFallSpeed) { _velocity.y = -MaxFallSpeed; }
                // the player can still walk in the air, as is the custom
                _body.MovePosition(_body.position + _velocity + walkVec);
                break;
        }
    }
    

    // triggers that update the input state
    public void Move(InputAction.CallbackContext context)
    {
        _walkDirection = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            
        }
    }

}
