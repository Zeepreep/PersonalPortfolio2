using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;
using Input = UnityEngine.Input;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    
    public float moveSpeed = 5.0f;

    public float sprintSpeedIncrease = 5.0f;

    public float groundDrag;
    
    public float jumpForce = 10f;
    
    public float leapForce = 10f;
    
    [Header("Ground Check")] 
    public float playerHeight;

    public LayerMask whatIsGround;
    
    [Header("Wall Jumping")]
    public LayerMask whatIsWall;

    public float wallCheckDistance;


    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    
    private bool grounded;
    
    private float horizontalInput;
    private float verticalInput;
    
    public Transform orientation;
    
    private Rigidbody rigidbody;
    
    float originalSpeed;
    bool isSprinting;
    private float originalMass;
    bool isDiving;


    private Vector3 moveDirection;

    private int leapCount = 1;


    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance,
            whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, 
            whatIsWall);
    }
    
    enum State
    {
        Idle,
        Walk,
        Jump,
        Walljump,
        Run,
        Leap,
        Dive,
        Dead
    };

    private State currentState = State.Idle;

    private void HandleStateTransition(State previousState, State nextState)
    {
        
    }
    
    void SetState(State newState)
    {
        if (currentState != newState)
        {
            HandleStateTransition(currentState, newState);
            currentState = newState;
        }
    }

    bool IsGrounded()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        return grounded;
    }
    
    void Start()
    {
        HandleRigidbody();
        
        originalSpeed = moveSpeed;
    }

    void HandleRigidbody()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
    }

    void Inputs()
    {
        SetState(State.Walk);
        
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            SetState(State.Jump);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !IsGrounded() && (wallLeft || wallRight))
        {
            SetState(State.Walljump);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SetState(State.Leap);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            SetState(State.Dive);
        } 
        else if (Input.GetKeyUp(KeyCode.Mouse1) && isDiving)
        {
            Debug.Log("STOPPED diving");
            isDiving = false;
        }

        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            HandleRunState(); 
        } 
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Debug.Log("STOPPED SPRINTING");
            isSprinting = false;
            moveSpeed = originalSpeed;
            SetState(State.Walk);
        }
    }

    void HandleRunState()
    {
        if (!isSprinting) {
            moveSpeed += sprintSpeedIncrease;
            isSprinting = true;
            Debug.Log("Sprinting!");
        }
    }
    
    void HandleWalkState()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    
        Vector3 forwardDirection = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 rightDirection = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;

        
            
        moveDirection = forwardDirection * verticalInput + rightDirection * horizontalInput;
    
        rigidbody.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
    }

    void HandleDiveState()
    {
        Debug.Log("start leap " + leapCount);
        
        if (!isDiving)
            {
                rigidbody.AddForce(-orientation.up * (leapForce * 4), ForceMode.Impulse);

                isDiving = true;
                Debug.Log("Diving down!");
            }
    }

    void HandleLeapState()
    {
        if (!IsGrounded())
        {
            if (moveDirection != Vector3.zero)
                {
                    Vector3 forceToApply = moveDirection * leapForce + rigidbody.position.normalized * leapForce +
                                           (orientation.up * (leapForce / 5));

                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
                    rigidbody.AddForce(forceToApply, ForceMode.Impulse);

                    Debug.Log("leap moving " + leapCount);
                }
                else
                {

                    Vector3 forceToApply = (orientation.forward * leapForce) + (orientation.up * (leapForce / 2));

                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
                    rigidbody.AddForce(forceToApply, ForceMode.Impulse);

                    Debug.Log("leap not moving " + leapCount);

                }
        }
    }
    


    private void HandleWalljumpState()
    {
        Debug.Log("walljump");
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * jumpForce + wallNormal * jumpForce;

        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        rigidbody.AddForce(forceToApply, ForceMode.Impulse);
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rigidbody.velocity = new Vector3(limitedVel.x, rigidbody.velocity.y, limitedVel.z);
        }
        
    }


    void HandleJumpState()
    {
        if (IsGrounded())
        {
            rigidbody.AddForce(UnityEngine.Vector2.up * jumpForce, ForceMode.Impulse);
            Debug.Log("JUMP!");
        }
    }

    

    void HandleState()
    {
        switch (currentState)
        {
            case State.Walk:
                HandleWalkState();
                break;
            
            case State.Run:
                HandleRunState();
                break;
            
            case State.Jump:
                HandleJumpState();
                break;
            
            case State.Leap:
                HandleLeapState();
                break;
            
            case State.Dive:
                HandleDiveState();
                break;
            
            case State.Walljump :
                HandleWalljumpState();
                break;
        }
    }

    void groundedDragHandler()
    {
        if (grounded)
        {
            rigidbody.drag = groundDrag;
            leapCount = 1;
        }
        else
        {
            rigidbody.drag = 0;
            leapCount = 0;
        }
    }
    
    void Update()
    {
        HandleState();
        Inputs();
        SpeedControl();
        groundedDragHandler();
        CheckForWall();
    }
}