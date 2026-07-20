using UnityEngine;
using System.Collections;

namespace Vehicle
{


    public class LookVehicle : MonoBehaviour
    {
        private InputManager inputManager;

        [SerializeField]
        private Transform playerLookOrientation;

        private Transform camerasHolder;

        private Transform vehicleCamerasHolder;


        public bool canLook = false;
        public Vector2 mouseInput;

        public bool inVehicle = false;

        public float mouseSensitivityX = 0.15f;
        public float mouseSensitivityY = 0.15f;
        public float lookUpperLimit = -90f;
        public float lookLowerLimit = 60f;
        public float maximumYaw = 90f;
        public float lookBodyRotationSpeed = 8.9f;

        private float mouseRotationX;
        private float mouseRotationY;

        private float smoothedPitch;
        private float smoothedYaw;

        private float pitchSmoothVelocity;
        private float yawSmoothVelocity;


        private void Awake()
        {

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
            vehicleCamerasHolder = GameObject.FindWithTag("VehicleCamerasHolder").GetComponent<Transform>();
            camerasHolder = vehicleCamerasHolder;
            DisableCamera();
            
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
            mouseRotationY = Mathf.Clamp(mouseRotationY, -maximumYaw, maximumYaw);
        }

        private void ApplyMouseMovementToCameraRotation()
        {
            //smoothedPitch = Mathf.SmoothDampAngle(smoothedPitch, mouseRotationX, ref pitchSmoothVelocity, 0.08f, 720f, Time.deltaTime);
            //smoothedYaw = Mathf.SmoothDampAngle(smoothedYaw, mouseRotationY, ref yawSmoothVelocity, 0.08f, 720f, Time.deltaTime);

            Quaternion localRotation = Quaternion.Euler(mouseRotationX, mouseRotationY, 0f);

            camerasHolder.localRotation = localRotation;

            if (playerLookOrientation != null)
            {
                playerLookOrientation.localRotation = Quaternion.Euler(
                    mouseRotationX,
                    mouseRotationY,
                    0f
                );
            }

            /*
            Quaternion lookRotation = Quaternion.Euler(mouseRotationX, mouseRotationY, 0f);

            camerasHolder.rotation = lookRotation;
            playerLookOrientation.rotation = lookRotation;

            */
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