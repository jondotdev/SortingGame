using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    #region VARIABLES
    public GameManager gm;

    [Header("Player")]
    public float MoveSpeed = 6f;
    public float SprintSpeed = 10f;
    public float RotationSpeed = 0.2f;
    public float SpeedChangeRate = 10f;
    [Space(5)]
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.1f; // (Time before jumping again)
    public float FallTimeout = 0.15f; // (Time before entering the "fall" state) 
    [Space(5)]
    public float interactionRange = 3f;
    public float throwForce = 10f;
    public float carryMoveSpeed = 3f;
    public float carrySprintSpeed = 5f;

    [Header("Pushing Rigidbodies")]
    public LayerMask pushLayers;
    public bool canPush = true;
    [Range(0.5f, 5f)] public float strength = 0.5f;

    [Header("Grounded Checking")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -90.0f;

    [Header("Internal Variables")]
    public bool canInteract = true;

    private float _cinemachineTargetPitch;

    private float currentMoveSpeed;
    private float currentSprintSpeed;
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private CharacterController _controller;
    private InputHandler _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private Transform Head;
    private Transform Hand;
    private CharacterController cc;

    private Interactable heldObject = null;
    #endregion VARIABLES

    private void Start() {
        if(isLocalPlayer) {
            gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

            gm.Unpause();

            if (_mainCamera == null) {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            GameObject.FindWithTag("PlayerFollowCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = transform.Find("PlayerCameraRoot");

            _controller = GetComponent<CharacterController>();
            _input = gm.transform.GetComponent<InputHandler>();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            Head = transform.Find("PlayerCameraRoot");
            Hand = transform.Find("Hand");
            cc = transform.GetComponent<CharacterController>();

            currentMoveSpeed = MoveSpeed;
            currentSprintSpeed = SprintSpeed;

            cc.enabled = false;
            Transform spawn = GameObject.FindWithTag("SpawnLocation").transform;
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
            cc.enabled = true;
        }
    }

    private void Update()
    {
        if(isLocalPlayer) { 

            HandleInteractions();

            JumpAndGravity();
            GroundedCheck();
            Move();
        }
    }

    private void LateUpdate()
    {
        if(isLocalPlayer) {
            CameraRotation();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (canPush) PushRigidBodies(hit);
    }

    private void PushRigidBodies(ControllerColliderHit hit)
    {
        // https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

        // make sure we hit a non kinematic rigidbody
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        // make sure we only push desired layer(s)
        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & pushLayers.value) == 0) return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f) return;

        // Calculate push direction from move direction, horizontal motion only
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        // Apply the push and take strength into account
        body.AddForce(pushDir * strength, ForceMode.Impulse);
    }

    private void HandleInteractions()
    {
        if (heldObject != null)
        {
            if (heldObject.throwable && Input.GetKeyDown(KeyCode.Q))
            {
                heldObject.Throw(Head.forward, throwForce);
                heldObject = null;
                canInteract = true;
                currentMoveSpeed = MoveSpeed;
                currentSprintSpeed = SprintSpeed;
            }
        }

        RaycastHit hit;
        if (canInteract && Physics.Raycast(Head.position, Head.forward, out hit, interactionRange))
        {
            Interactable i;
            if (hit.transform.TryGetComponent<Interactable>(out i))
            {
                gm.ShowInteractionCursor(i.hintText);
                if (i.canPickUp && Input.GetKeyDown(KeyCode.E))
                {
                    i.PickUp(Hand);
                    heldObject = i;
                    canInteract = false;
                    currentMoveSpeed = carryMoveSpeed;
                    currentSprintSpeed = carrySprintSpeed;
                }
            }
            else
            {
                gm.HideInteractionCursor();
            }
        }
        else
        {
            gm.HideInteractionCursor();
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Collider[] colliders = Physics.OverlapSphere (spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        Grounded = colliders.Length > 0 ? true : false;
        foreach (Collider c in colliders)
        {
            ConveyorBelt cb;
            if(c.transform.TryGetComponent<ConveyorBelt>(out cb))
            {
                cc.Move(cb.transform.forward * cb.speed * Time.deltaTime);
            }
        }
    }

    private void CameraRotation()
    {
        // if there is an input
        if (_input.look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
            _rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? currentSprintSpeed : currentMoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            // move
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        // move the player
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}
