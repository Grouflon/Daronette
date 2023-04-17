using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Sprinting,
    InAir,
    Crouching,
}

public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement Speed")]
    public float walkingMoveSpeed = 6.0F;
    public float sprintingMoveSpeed = 10F;
    public float crouchingMoveSpeed = 1.5F;

    public float crouchTime = 0.1F;
    public float uncrouchTime = 0.1F;

    [Header("In air")]
    [Range(1F, 100F)]
    public float gravity = 20.0F;
    [Range(0.01F,5F)]
    public float coyoteTimeMax = 0.2F;
    [Range(0F,1F)]
    public float airControl = 0.6F;

    [Header("Jump")]
    [Range(0,3)]
    public int maxJumps = 1;
    public float jumpHeight = 4.0F;
    public bool firstJumpGrounded = true;
    [Range(1F, 50F)]
    public float crouchingJumpHeightDivider = 10.0F;
    public bool canMove = true;


    private float xInput;
    private float zInput;
    private Vector3 velocity;
    private Vector3 move;
    private Vector3 airInertia;
    private Vector3 currentAirDampingRate = Vector3.zero;
    private float airInertiaDampingDuration = 0.5f;
    private CharacterController controller;
    private bool doJump = false;
    private bool doSprint = false;
    private bool doCrouch = false;
    private int jumps = 0;
    private float coyoteTime;

    public PlayerState state;

    public float moveSpeed;

    private bool IsGrounded() //return true if the player has still coyote time
    {
        return coyoteTime > 0;
    }

    private bool NoMovingInputs()
    {
        return xInput == 0 && zInput == 0;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        moveSpeed = walkingMoveSpeed;
        
    }

    void Update()
    {
        if (!canMove)
        {
            xInput = 0.0f;
            zInput = 0.0f;
            move = Vector3.zero;
            return;
        }

        //GET THE DIRECTIONAL MOVES
        xInput = Input.GetAxis("Horizontal"); //get the "A" and "D" input
        zInput = Input.GetAxis("Vertical"); //get the "W" and "Z" input
        move = (transform.right * xInput + transform.forward * zInput); //save the inputs as a local transform
        

        //JUMP CALLING
        if (Input.GetButtonDown("Jump"))
        {
            doJump = true;
        }

        //SPRINT CALLING
        if (Input.GetButton("Sprint"))
        {
            doSprint = true;
        }
        else
        {
            doSprint = false;
        }

        //CROUCH CALLING
        if (Input.GetButtonDown("Crouch"))
        {
            if (doCrouch)
            {
                doCrouch = false;
            }
            else
            {
                doCrouch = true;
            }
        }

        //EXIT FEATURE
        /*
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }
        */
    }

    void FixedUpdate()
    {
        //CHECK IF GROUNDED
        if (controller.isGrounded)
        {
            coyoteTime = coyoteTimeMax; //reset the coyote time
            jumps = 0; //reset jump count
        }
        else
        {
            coyoteTime -= Time.deltaTime; //decrease the coyote time with time
            coyoteTime = Mathf.Clamp(coyoteTime, 0F, coyoteTimeMax); //prevent the coyote time value to be under 0 for readability
        }

        //APPLY GROUNDED ONLY MOVES
        if (IsGrounded())
        {
            //RESET VELOCITY
            if (velocity.y < 0F) // reset the velocity if the player is grounded
            {
                velocity.y = 0F;
            }

            //LANDING
            if (state == PlayerState.InAir)
            {
                DefaultState();
            }

            if (doSprint) //APPLY SPRINT
            {
                Sprint();
            }
            else if (doCrouch) //APPLY CROUCH
            {
                Crouch();
            }
            else //OR APPLY DEFAULT STATE
            {
                DefaultState();
            }

            controller.Move(move * moveSpeed * Time.deltaTime);
        }
        else
        {
            if (state != PlayerState.InAir)
            {
                state = PlayerState.InAir; //the player is now in air

                airInertia = move * moveSpeed;
                currentAirDampingRate = Vector3.zero;
            }

            Vector3 currentAirInertia = Vector3.SmoothDamp(airInertia, Vector3.zero, ref currentAirDampingRate, airInertiaDampingDuration);
            controller.Move((airControl * (move * moveSpeed) + (1.0f - airControl) * currentAirInertia) * Time.deltaTime);
        }

        //APPLY JUMP
        if (doJump)
        {
            Jump();
            doJump = false;
        }

        //APPLY MOVE
        // controller.Move(move * moveSpeed * Time.deltaTime + (jumpDirection * Time.deltaTime)); //apply the saved inputs to the character controller

        //APPLY GRAVITY AND VELOCITY
        velocity.y -= gravity * Time.deltaTime; // move the player downward at the speed on gravity.
        controller.Move(velocity * Time.deltaTime); // make the fall faster and faster.
    }

    //JUMP FUNCTION
    public void Jump()
    {
        bool canJump = true; //tell if the player can actually jump

        if (jumps >= maxJumps) //check if the player has jumps available
        {
            canJump = false;
        }

        if (jumps == 0 && state == PlayerState.InAir && firstJumpGrounded) //check if the player is on the ground for the first jump (if the rule is activated)
        {
            canJump = false;
        }

        if (canJump) //apply jump if every check is passed
        {
            jumps += 1; //increment jump count
            coyoteTime = 0F; //skip coyote time

            if (state == PlayerState.Crouching) //check if the player is jumping while crouching
            {
                velocity.y = Mathf.Sqrt((jumpHeight / crouchingJumpHeightDivider) * 2F * gravity); //divide the jump height
                jumps = maxJumps; //the player can not jump more
            }
            else //apply default jump
                velocity.y = Mathf.Sqrt(jumpHeight * 2F * gravity);
        }
    }

    //SPRINT FUNCTION
    public void Sprint()
    {
        bool canSprint = true;

        if (state != PlayerState.Idle && state != PlayerState.Walking && state != PlayerState.Crouching && state != PlayerState.Sprinting)
        {
            canSprint = false;
        }

        if (zInput <= 0)
        {
            canSprint = false;
            DefaultState();
        }

        if (canSprint)
        {
            state = PlayerState.Sprinting;
            Uncrouch();
            moveSpeed = sprintingMoveSpeed;
        }
    }

    //CROUCH FUNCTION
    public void Crouch()
    {
        bool canCrouch = true;

        if (state != PlayerState.Idle && state != PlayerState.Walking && state != PlayerState.Crouching)
        {
            canCrouch = false;
        }

        if(canCrouch)
        {
            state = PlayerState.Crouching;
            moveSpeed = crouchingMoveSpeed;

            if (controller.height != 1.0F)
            {
                velocity.y = -20F;
                controller.height = Mathf.MoveTowards(controller.height, 1.0F, Time.deltaTime / crouchTime); //duck the character down
            }            
        }
    }

    //UNCROUCH FUNCTION
    public void Uncrouch()
    {
        doCrouch = false;

        if (controller.height != 2.0F)
        {
            controller.height = Mathf.MoveTowards(controller.height, 2.0F, Time.deltaTime / uncrouchTime); //rise the character to default height
        }
    }

    //DEFAULT STATE FUNCTION
    public void DefaultState()
    {
        Uncrouch();

        airInertia = Vector3.zero;

        if (NoMovingInputs())
        {
                state = PlayerState.Idle;
                moveSpeed = 0;
        }
        else
        {
                state = PlayerState.Walking;
                moveSpeed = walkingMoveSpeed;
        }
    }
}



