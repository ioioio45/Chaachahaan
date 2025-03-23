using EN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EN
{
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject;
        InputHandler inputHandler;
        Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Stats")]
        [SerializeField]
        float movementSpeed = 7;
        [SerializeField]
        float sprintSpeed = 13;
        [SerializeField]
        float rotationSpeed = 10;
        [SerializeField]
        float rollDistance = 5f;
        [SerializeField]
        float rollDuration = 0.5f;
        [SerializeField]
        float rollCooldown = 1.0f;
        [SerializeField]
        float jumpForce = 0.8f;
        [SerializeField]
        float gravityMultiplier = 2f;


        [Header("Gravity")]
        [SerializeField] private float gravity = -9.81f;

        private Vector3 velocity;

        [Header("Fall Damage")]
        [SerializeField] private float fallDamageThreshold = 5f;
        [SerializeField] private float maxFallDamageHeight = 15f;
        [SerializeField] private float fallDamageMultiplier = 10f;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 2f;

        private float fallStartHeight;
        private bool isFalling;

        
        void Start()
        {
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            playerManager = GetComponent<PlayerManager>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();
        }
        public void Update()
        {
            float delta = Time.deltaTime;
            HandleFallDamage();
            animatorHandler.anim.SetBool("isFalling", !playerManager.isGrounded);
        }
        public void FixedUpdate()
        {
            float delta = Time.deltaTime;
            HandleGravity(delta);
        }
        #region Movement
        Vector3 normalVector;
        Vector3 targetPosition;

        private void HandleRotation(float delta)
        {
            if (!inputHandler.rollFlag)
            {
                Vector3 targetDir = Vector3.zero;
                float moveOverride = inputHandler.moveAmount;

                targetDir = cameraObject.forward * inputHandler.vertical;
                targetDir += cameraObject.right * inputHandler.horizontal;

                targetDir.Normalize();
                targetDir.y = 0;

                if (targetDir == Vector3.zero)
                    targetDir = myTransform.forward;

                float rs = rotationSpeed;

                Quaternion tr = Quaternion.LookRotation(targetDir);
                Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

                myTransform.rotation = targetRotation;
            }
            
        }
        public void HandleMovement(float delta)
        {
            

            if (inputHandler.rollFlag)
                return;

            inputHandler.TickInput(delta);

            // Определяем направление движения
            moveDirection = cameraObject.forward * inputHandler.vertical + cameraObject.right * inputHandler.horizontal;
            moveDirection.y = 0;
            moveDirection.Normalize();

            // Выбираем скорость
            float speed = inputHandler.sprintFlag && inputHandler.moveAmount > 0 ? sprintSpeed : movementSpeed;
            playerManager.isSprinting = inputHandler.sprintFlag && inputHandler.moveAmount > 0;

            // Вычисляем целевую скорость
            Vector3 targetVelocity = moveDirection * speed;

            // Если игрок не двигается, быстро тормозим
            if (inputHandler.moveAmount == 0)
            {
                targetVelocity = Vector3.zero;
            }

            // Проверяем, на земле ли игрок
            if (playerManager.isGrounded)
            {
                rigidbody.velocity = new Vector3(targetVelocity.x, -2f, targetVelocity.z); // Притягиваем к земле
            }
            else
            {
                rigidbody.velocity = new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.z); // Сохраняем вертикальную скорость
            }

            // Обновление анимаций
            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);

            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }





        public void HandleRollingAndSprinting(float delta)
        {
            if (animatorHandler.anim.GetBool("isInteracting"))
                return;
            if (inputHandler.rollFlag)
            {
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;

                if(inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("RollForward", true);
                    moveDirection.y = 0;

                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                    StartCoroutine(Roll(moveDirection));

                }
                else
                {
                    animatorHandler.PlayTargetAnimation("RunBackward", true);
                    StartCoroutine(RunBackward(moveDirection));
                }
            }
        }

        private IEnumerator Roll(Vector3 moveDirection)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            float elapsedTime = 0.0f;

            while (elapsedTime < rollDuration)
            {
                Vector3 movement = moveDirection.normalized * (rollDistance / rollDuration) * Time.deltaTime;
                rb.MovePosition(rb.position + movement);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(rollCooldown);
        }


        private IEnumerator RunBackward(Vector3 moveDirection)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + moveDirection.normalized * rollDistance;

            float elapsedTime = 0.0f;
            while (elapsedTime < rollDuration)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / rollDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = endPosition;
            yield return new WaitForSeconds(rollCooldown);
            inputHandler.rollFlag = false; // Сброс флага после завершения анимации
        }

        private float CalculateFallDamage(float fallHeight)
        {
            if (fallHeight < fallDamageThreshold)
                return 0;

            fallHeight = Mathf.Min(fallHeight, maxFallDamageHeight);

            return (fallHeight - fallDamageThreshold) * fallDamageMultiplier;
        }

        private void HandleFallDamage()
        {
            if (playerManager.isGrounded)
            {
                if (isFalling)
                {
                    float fallHeight = fallStartHeight - transform.position.y;

                    if (fallHeight > fallDamageThreshold)
                    {
                        float damage = CalculateFallDamage(fallHeight);
                        ApplyDamage(damage);
                        Debug.Log($"Fall Damage: {damage}");
                    }

                    isFalling = false;
                }
            }
            else
            {
                if (!isFalling)
                {
                    fallStartHeight = transform.position.y;
                    isFalling = true;
                }
            }
        }
        private void ApplyDamage(float damage)
        {
            // Здесь можно добавить логику для уменьшения здоровья персонажа
            // Например:
            // playerManager.TakeDamage(damage);

            // Воспроизвести анимацию или эффект падения
            animatorHandler.PlayTargetAnimation("Jump", true);
        }

        private void HandleGravity(float delta)
        {
            if (playerManager.isGrounded)
            {
                if (rigidbody.velocity.y < 0)
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, -2f, rigidbody.velocity.z); // Плавное торможение при приземлении
                }
            }
            else
            {
                rigidbody.velocity += new Vector3(0, gravity * delta, 0);  // Добавляем гравитацию
                if (rigidbody.velocity.y < -20f)  // Ограничение на максимальную скорость падения
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, -20f, rigidbody.velocity.z);
                }
            }
        }



        //private void HandleJump()
        //{
        //    if (playerManager.isGrounded && Input.GetButtonDown("Jump"))
        //    {
        //        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        //        animatorHandler.PlayTargetAnimation("Jump", true);
        //    }
        //}
        public void HandleJumping()
        {
            if (!playerManager.isGrounded) return;

            animatorHandler.anim.SetTrigger("JumpTrigger");
            Debug.Log("JUMP");

            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            playerManager.isGrounded = false;
        }

        #endregion
    }
}

