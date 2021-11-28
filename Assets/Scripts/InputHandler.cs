using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
	public GameManager gm;

	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool sprint;

	[Header("Movement Settings")]
	public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;
#endif

	public void OnMove(InputValue value)
	{
		if (!gm.paused)
		{
			MoveInput(value.Get<Vector2>());
		}
        else
        {
			MoveInput(Vector2.zero);
        }
	}

	public void OnLook(InputValue value)
	{
		if (!gm.paused && cursorInputForLook)
		{
			LookInput(value.Get<Vector2>());
		}
        else
        {
			LookInput(Vector2.zero);
        }
	}

	public void OnJump(InputValue value)
	{
		if (!gm.paused)
		{
			JumpInput(value.isPressed);
		}
        else
        {
			JumpInput(false);
        }
	}

	public void OnSprint(InputValue value)
	{
		if (!gm.paused)
		{
			SprintInput(value.isPressed);
		}
        else
        {
			SprintInput(false);
        }
	}

	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	}

	public void LookInput(Vector2 newLookDirection)
	{
		look = newLookDirection;
	}

	public void JumpInput(bool newJumpState)
	{
		jump = newJumpState;
	}

	public void SprintInput(bool newSprintState)
	{
		sprint = newSprintState;
	}
}
