using EN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EN
{
    public class PlayerManager : MonoBehaviour
    {
        InputHandler inputHandler;
        Animator anim;
        CameraHandler cameraHandler;
        PlayerLocomotion playerLocomotion;


        public bool isInteracting;
        public bool isSprinting;

        public float health = 100f;

        public bool isGrounded; // Флаг, указывающий, что персонаж на земле
        [SerializeField] private float groundCheckDistance = 0.5f; // Расстояние для проверки земли
        [SerializeField] private LayerMask groundLayer;
        private void Awake()
        {
        }
        void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            cameraHandler = CameraHandler.singleton;
        }

        // Update is called once per frame
        void Update()
        {
            float delta = Time.deltaTime;
            isInteracting = anim.GetBool("isInteracting");


            if (inputHandler.moveAmount <= 0)
            {
                isSprinting = false;
            }
            else
            {
                isSprinting = inputHandler.b_Input;
            }

            if (inputHandler.jump_Input)
            {
                if (!isGrounded)
                {
                    Debug.LogWarning("Cannot jump: not grounded!");
                }
                else
                {
                    Debug.Log("jump");
                    playerLocomotion.HandleJumping();
                }
            }
            playerLocomotion.HandleRollingAndSprinting(delta);

            CheckIfGrounded();
        }

        private void FixedUpdate()
        {
            float delta = Time.deltaTime;
            inputHandler.TickInput(delta);
            playerLocomotion.HandleMovement(delta);
            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
            }
            

        }
        private void LateUpdate()
        {
            inputHandler.rollFlag = false;
            inputHandler.sprintFlag = false;
            isSprinting = inputHandler.b_Input;
        }
        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
        }
        private void Die()
        {
            Debug.Log("Player Died!");
            // Логика смерти персонажа
        }
        private void CheckIfGrounded()
        {
            Vector3 g = new Vector3(0,1.2f,0);
            bool result = Physics.Raycast(transform.position + g, Vector3.down, groundCheckDistance, groundLayer);
            isGrounded = result;

            // Отрисовка луча для визуальной отладки
            Debug.DrawRay(transform.position + g, Vector3.down * groundCheckDistance, Color.red);
        }
        
    }
}

