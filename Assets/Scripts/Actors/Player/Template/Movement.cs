using UnityEngine;

namespace Player
{
    public class Movement1 : MonoBehaviour
    {
        public bool m_isActive = true;
        public Vector2 m_moveInput;
        public struct MoveParameters
        {
            public float gravityForce;
            public float moveSpeed;
            public float moveAcceleration;
            public Vector3 currentMoveVelocity;
            public Vector3 TargetMoveVelocity;
        }

        private InputManager m_inputManager;
        private Player.Controller m_playerController;
        private CharacterController m_characterController;
        private Transform m_playerLookOrientation;

        [SerializeField] private float m_gravityForce = -5.0f;
        [SerializeField] private float m_moveSpeed = 12.0f;
        [SerializeField] private float m_moveAcceleration = 5.0f;
        private Vector3 m_currentMoveVelocity;
        private Vector3 m_targetMoveVelocity;

        private void Awake()
        {
            m_playerController = GetComponentInParent<Player.Controller>();
            m_characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            m_inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
            m_playerLookOrientation = GameObject.FindWithTag("LookOrientation").GetComponent<Transform>();
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (!m_isActive) return;
            // ----- handle move input -----
            Vector2 input = m_inputManager.GetMovementInput();
            m_moveInput = input;
            

            Vector3 lookDirection = new Vector3(m_playerLookOrientation.forward.x, 0 ,m_playerLookOrientation.forward.z);
            Vector3 movementInput = lookDirection  * input.y + m_playerLookOrientation.right * input.x;
            movementInput = Vector3.ClampMagnitude(movementInput, 1f);

            // ----- handle move velocity -----
            m_targetMoveVelocity = movementInput * m_moveSpeed;
            m_targetMoveVelocity.y = -m_gravityForce;
            m_currentMoveVelocity = Vector3.Lerp(m_currentMoveVelocity, m_targetMoveVelocity, m_moveAcceleration * Time.deltaTime);
            m_characterController.Move(m_currentMoveVelocity * Time.deltaTime);
        }

        public MoveParameters GetMovementParameters()
        {
            MoveParameters moveParameters;
            moveParameters.gravityForce = m_gravityForce;
            moveParameters.moveSpeed = m_moveSpeed;
            moveParameters.moveAcceleration = m_moveAcceleration;
            moveParameters.currentMoveVelocity = m_currentMoveVelocity;
            moveParameters.TargetMoveVelocity = m_targetMoveVelocity;

            return moveParameters;
        }
    }
}