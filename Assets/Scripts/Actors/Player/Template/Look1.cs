using UnityEngine;
using System.Collections;

namespace Player
{
    public class Look1 : MonoBehaviour
    {
        private InputManager m_inputManager;
        [SerializeField] private Transform m_playerLookOrientation;
        [SerializeField] private Transform m_ghostPlayerLookOrientation;
        private Transform m_camerasHolder;

        [Header("Look Parameters")]
        public bool m_canLook = true;
        public Vector2 m_mouseInput;

        public float m_mouseSensitivityX = 0.15f;
        public float m_mouseSensitivityY = 0.15f;
        public float m_lookUpperLimit = -90f;
        public float m_lookLowerLimit = 60f;
        public float m_lookBodyRotationSpeed = 8.9f;

        private float m_mouseRotationX;
        private float m_mouseRotationY;

        private float m_bodyRotationY;

        void Awake()
        {
            // ----- corountine for smooth camera movement -----
            StartCoroutine(PostPhysicsUpdate());
        }

        void Start()
        {
            // ----- get components by tags or layers -----
            m_inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
            m_camerasHolder = GameObject.FindWithTag("CamerasHolder").GetComponent<Transform>();

            // ----- set cursor -----
            CursorController.HideCursor();
        }

        void Update()
        {
            CalcMouseMovement();
            if (m_canLook)
            {
                ApplyMouseMovementToCameraRotation();
            }
        }

        private void CalcMouseMovement()
        {
            // ----- handle mouse inputs -----
            m_mouseInput = m_inputManager.GetMouseInput();
            float mouseX = m_mouseInput.x * m_mouseSensitivityX;
            float mouseY = m_mouseInput.y * m_mouseSensitivityY;

            // ----- calc x/y rotations with limits -----
            m_mouseRotationY += mouseX;
            m_mouseRotationX -= mouseY;
            m_mouseRotationX = Mathf.Clamp(m_mouseRotationX, m_lookUpperLimit, m_lookLowerLimit);
        }

        private void ApplyMouseMovementToCameraRotation()
        {
            m_camerasHolder.rotation = Quaternion.Euler(m_mouseRotationX, m_mouseRotationY, 0);
            m_playerLookOrientation.rotation = Quaternion.Euler(m_mouseRotationX, m_mouseRotationY, 0);
            m_ghostPlayerLookOrientation.rotation = Quaternion.Euler(m_mouseRotationX, m_mouseRotationY, 0);
        }

        private void ApplyMouseMovementToPlayerRotation()
        {
            m_bodyRotationY = Mathf.Lerp(m_bodyRotationY, m_mouseRotationY, m_lookBodyRotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, m_bodyRotationY, 0);
        }

        IEnumerator PostPhysicsUpdate()
        {
            YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                yield return waitForFixedUpdate;
                if (m_canLook)
                {
                    ApplyMouseMovementToPlayerRotation();
                }
            }
        }
    }
}