using System.Collections.Generic;
using UnityEngine;

namespace Vehicle
{
    public class Movement : MonoBehaviour
    {
        public bool isActive = false;
        public Vector2 moveInput;

        private InputManager inputManager;
        private Vehicle.Controller vehicleController;
        private Rigidbody rb;

        [SerializeField]
        private float driveForce = 8000f;

        [SerializeField]
        private float turnForce = 4000f;

        [SerializeField]
        private float maxSpeed = 20f;

        [SerializeField]
        private Transform driverSeat;

        private Transform playerTransform;
        private Transform rotationTransform;

        [SerializeField]
        private List<Collider> trackColliders;

        [SerializeField]
        private LayerMask groundLayer;

        void Awake()
        {
            vehicleController = GetComponentInParent<Vehicle.Controller>();
            rb = GetComponent<Rigidbody>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleMovement();
            LimitSpeed();
        }

        private void HandleMovement()
        {
            if (!isActive || !IsGrounded()) return;

            Vector2 input = inputManager.GetMovementInput();
            float drive = (input.x + input.y) * 0.5f;
            float rotation = (input.x - input.y) * 0.5f;


            rb.AddForce(rb.gameObject.transform.right * drive * driveForce);
            rb.AddRelativeTorque(Vector3.up * rotation * turnForce);
        }

        public void EnterVehicle(Transform playerTransform, Transform rotationTransform)
        {
            this.playerTransform = playerTransform;
            this.rotationTransform = rotationTransform;
            isActive = true;

            playerTransform.position = driverSeat.position;
          //  playerTransform.rotation = Quaternion.Euler(driverSeat.eulerAngles.x, playerTransform.eulerAngles.y, driverSeat.eulerAngles.z);
           // rotationTransform.rotation = driverSeat.rotation;

            
        }

        public void ExitVehicle()
        {
            isActive = false;
            playerTransform = null;
        }

        void LateUpdate()
        {


        }

        private void LimitSpeed()
        {
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        bool IsGrounded()
        {
            int numGrounded = 0;
            
            foreach(Collider cd in trackColliders)
            {
                float distToGround = cd.bounds.extents.y;
               // Debug.DrawRay(cd.bounds.center, -cd.transform.up, Color.red);

                if (Physics.Raycast(cd.bounds.center, -cd.transform.up, 0.75f, groundLayer))
                {
                    numGrounded++;
                }


            }
            
            if(numGrounded >= 2)
            {
                return true;
            }
            return false;
       
        }

    }
}
