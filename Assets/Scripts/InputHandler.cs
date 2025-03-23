using EN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EN
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        public bool b_Input;
        public bool rollFlag;
        public bool sprintFlag;
        public float rollInputTimer;


        PlayerControls inputActions;
        

        Vector2 movementInput;
        Vector2 cameraInput;


        public bool jump_Input;



        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.PlayerSpaceMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerSpaceMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
                inputActions.PlayerActions.Jump.performed += ctx =>
                {
                    jump_Input = true;
                    Debug.Log("Jump input detected!"); // Лог для отладки
                };

            }

            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
            HandleRollingInput(delta);
            jump_Input = inputActions.PlayerActions.Jump.phase == UnityEngine.InputSystem.InputActionPhase.Performed;
        }
        private void MoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }
        private void HandleRollingInput(float delta)
        {
            b_Input = inputActions.PlayerActions.Roll.phase == UnityEngine.InputSystem.InputActionPhase.Performed;
            if (b_Input)
            {
                rollInputTimer += delta;
                if (moveAmount > 0)
                {
                    sprintFlag = true;
                }
                else
                {
                    sprintFlag = false;
                }
            }
            else
            {
                if(rollInputTimer>0 && rollInputTimer < 0.5f)
                {
                    sprintFlag = false;
                    rollFlag = true;
                }
                rollInputTimer = 0;
            }
        }

    }
}
   



