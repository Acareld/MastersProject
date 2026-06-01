using UnityEngine;
using UnityEngine.Networking;

namespace Player
{

    public class Movement : MonoBehaviour
    {

        public bool isActive = true;
        public Vector2 moveInput;

        private InputManager inputManager;
        private Player.Controller playerController;
        private CharacterController characterController;
        private Transform playerLookOrientation;


        [SerializeField]
        private float gravityForce = 9.8f;
        [SerializeField]
        private float moveSpeed = 12f;
        [SerializeField]
        private float moveAcceleration = 5f;
        [SerializeField]
        private float jumpHeight = 2f;

        private Vector3 currentMoveVel;
        private Vector3 targetMoveVel;

        private float verticalVelocity;

        private void Awake()
        {
            playerController = GetComponentInParent<Player.Controller>();
            characterController = GetComponent<CharacterController>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
            playerLookOrientation = GameObject.FindWithTag("LookOrientation").GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleMovement();              
        }

        public void Jump()
        {
            if (!characterController.isGrounded) return;

            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravityForce); 
        }

        private void HandleMovement()
        {
            if (!isActive) return;

            Vector2 input = inputManager.GetMovementInput();
            moveInput = input;

            if (!playerController.canMove) input = Vector2.zero;

            Vector3 lookDirection = new Vector3(playerLookOrientation.forward.x, 0, playerLookOrientation.forward.z);
            Vector3 movementInput = lookDirection * input.y + playerLookOrientation.right * input.x;
            movementInput = Vector3.ClampMagnitude(movementInput, 1f);

            targetMoveVel = movementInput * moveSpeed;
           // targetMoveVel.y = -gravityForce;
            currentMoveVel = Vector3.Lerp(currentMoveVel, targetMoveVel, moveAcceleration * Time.deltaTime);
            
            if(characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravityForce * Time.deltaTime;
            currentMoveVel.y = verticalVelocity;

            characterController.Move(currentMoveVel * Time.deltaTime);

        }
    }
}