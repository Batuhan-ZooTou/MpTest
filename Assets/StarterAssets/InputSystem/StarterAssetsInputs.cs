using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Unity.Netcode;

namespace StarterAssets
{
	public class StarterAssetsInputs : NetworkBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public override void OnNetworkSpawn()
		{
			//If this is not the owner, turn of player inputs
			if (!IsOwner) gameObject.GetComponent<PlayerInput>().enabled = false;
		}
#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif
		public void OnMove(InputAction.CallbackContext context)
		{
            if (IsOwner)
            {
				move = context.ReadValue<Vector2>();

			}
		}
		public void OnLook(InputAction.CallbackContext context)
		{
			if (IsOwner)
			{
				look = context.ReadValue<Vector2>();

			}
		}
		public void OnJump(InputAction.CallbackContext context)
		{
			if (IsOwner)
			{
				jump = context.ReadValueAsButton();

			}
		}
		public void OnSprint(InputAction.CallbackContext context)
		{
			if (IsOwner)
			{
				sprint = context.ReadValueAsButton();

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
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}