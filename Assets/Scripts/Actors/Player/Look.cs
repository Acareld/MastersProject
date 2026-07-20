using UnityEngine;
using System.Collections;

namespace Player
{


    public class Look : MonoBehaviour
    {
        private InputManager inputManager;
        [SerializeField]
        private Transform playerLookOrientation;
        private Transform camerasHolder;

        private Transform playerCamerasHolder;
        private Transform vehicleCamerasHolder;

        [SerializeField]
        private Transform vehicleRotationSource;

        public bool canLook = true;
        public Vector2 mouseInput;

        public bool inVehicle = false;

        public float mouseSensitivityX = 0.15f;
        public float mouseSensitivityY = 0.15f;
        public float lookUpperLimit = -90f;
        public float lookLowerLimit = 60f;
        public float lookBodyRotationSpeed = 8.9f;

        private float mouseRotationX;
        private float mouseRotationY;

        private float bodyRotationY;

        private void Awake()
        {
            StartCoroutine(PostPhysicsUpdate());
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
            playerCamerasHolder = GameObject.FindWithTag("CamerasHolder").GetComponent<Transform>();
            camerasHolder = playerCamerasHolder;
        }

        // Update is called once per frame
        void Update()
        {
            CalcMouseMovement();
            if (canLook)
            {
                ApplyMouseMovementToCameraRotation();
            }
        }

        private void CalcMouseMovement()
        {
            mouseInput = inputManager.GetMouseInput();
            float mouseX = mouseInput.x * mouseSensitivityX;
            float mouseY = mouseInput.y * mouseSensitivityY;

            mouseRotationY += mouseX;
            mouseRotationX -= mouseY;
            mouseRotationX = Mathf.Clamp(mouseRotationX, lookUpperLimit, lookLowerLimit);
        }

        private void ApplyMouseMovementToCameraRotation()
        {
            Quaternion lookRotation = Quaternion.Euler(mouseRotationX, mouseRotationY, 0f);

            camerasHolder.rotation = lookRotation;
            playerLookOrientation.rotation = lookRotation;
        }


        private void ApplyMouseMovementToPlayerRotation()
        {
            bodyRotationY = Mathf.Lerp(bodyRotationY, mouseRotationY, lookBodyRotationSpeed * Time.fixedDeltaTime);

            transform.rotation = Quaternion.Euler(0, bodyRotationY, 0);


        }

        IEnumerator PostPhysicsUpdate()
        {
            YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                yield return waitForFixedUpdate;
                if (canLook)
                {
                    ApplyMouseMovementToPlayerRotation();
                }
            }
        }

        public void EnableCamera()
        {
            camerasHolder.gameObject.SetActive(true);
            canLook = true;
        }

        public void DisableCamera()
        {
            camerasHolder.gameObject.SetActive(false);
            canLook = false;
        }


    }
}