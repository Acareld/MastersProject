using System.Collections.Generic;
using Unity.VisualScripting;
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

        [Header("Wheel Transforms")]
        public Transform frontLeftTransform;
        public Transform frontRightTransform;
        public Transform backLeftTransform;
        public Transform backRightTransform;

        [Header("Car Settings")]
        public float maxSpeed = 15f;
        [SerializeField]
        private float maxDriveForce = 4000f;
        [SerializeField]
        private float turnForce = 2000f;
        [SerializeField]
        AnimationCurve accelerationCurve;

        [Space(10)]

        [Header("Lateral Grip")]
        public float lateralGrip = 90000f;
        public float maxLateralGripForce = 1000f;
        [SerializeField] private float longitudinalDrag = 80f;
        [SerializeField] private float maxLongitudinalDragForce = 300f;
        [SerializeField] private float brakingDrag = 600f;
        [SerializeField] private float maxBrakingForce = 1200f;

        [Header("Handbrake")]
        [SerializeField] private float handbrakeLongitudinalGrip = 5000f;
        [SerializeField] private float maxHandbrakeLongitudinalForce = 4000f;
        [SerializeField] private float handbrakeLateralGrip = 5000f;
        [SerializeField] private float maxHandbrakeLateralForce = 4000f;
        [SerializeField] private float handbrakeAngularDamping = 4f;

        [Range(0f, 1f)]
        public float steeringGripReduction = 0.55f;

        private float carSpeed;

        private bool handbrakeActive = false;
        


        [SerializeField]
        private List<Collider> trackColliders;

        [SerializeField]
        private LayerMask groundLayer;

        void Awake()
        {
            vehicleController = GetComponentInParent<Vehicle.Controller>();
            rb = GetComponent<Rigidbody>();
            /*Vector3 inertiaTensor = rb.inertiaTensor;

            inertiaTensor.y *= 0.2f;

            rb.inertiaTensor = inertiaTensor;*/

            Vector3 center = transform.localPosition;
//center.y -= 3f;
            center.z -= 1f;
            rb.centerOfMass = new Vector3(0f, -1f, -1f); 


        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
        }

       

        void FixedUpdate()
        {
            float move = 0f;
            float turn = 0f;

            if (isActive)
            {
                Vector2 input = inputManager.GetMovementInput();
                float driveInput = (input.x + input.y) * 0.5f;

                move = input.x;
                turn = input.y;

                Debug.Log("Speed: " + carSpeed);

                if (carSpeed > maxSpeed) move = 0f;

                if(!handbrakeActive)
                {
                    HandleForceMovement(move, turn);
                } 
            }

            if (handbrakeActive)
            {
                AddHandbrakeAtPoint(frontLeftTransform);
                AddHandbrakeAtPoint(frontRightTransform);
                AddHandbrakeAtPoint(backLeftTransform);
                AddHandbrakeAtPoint(backRightTransform);

                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, handbrakeAngularDamping * Time.fixedDeltaTime);
            }
            else
            {
                AddFriction(frontLeftTransform, move, turn);
                AddFriction(frontRightTransform, move, turn);
                AddFriction(backLeftTransform, move, turn);
                AddFriction(backRightTransform, move, turn);
            }
            carSpeed = rb.linearVelocity.magnitude;
        }

        private void HandleForceMovement(float move, float turn)
        {
            float normalizedSpeed = Mathf.InverseLerp(0f, maxSpeed, carSpeed);
            float driveForce = accelerationCurve.Evaluate(normalizedSpeed) * maxDriveForce;
            driveForce /= 4f;

            Debug.DrawLine(rb.worldCenterOfMass, frontLeftTransform.position, Color.magenta);
            Debug.DrawLine(rb.worldCenterOfMass, frontRightTransform.position, Color.magenta);
            Debug.DrawLine(rb.worldCenterOfMass, backLeftTransform.position, Color.magenta);
            Debug.DrawLine(rb.worldCenterOfMass, backRightTransform.position, Color.magenta);

            // move forward / backwards
            if (move != 0f && turn == 0f)
            {
                // FL
                AddDriveForceAtPoint(frontLeftTransform, move, driveForce);
                // FR
                AddDriveForceAtPoint(frontRightTransform, move, driveForce);
                // BL
                AddDriveForceAtPoint(backLeftTransform, move, driveForce);
                // BR
                AddDriveForceAtPoint(backRightTransform, move, driveForce);


            }

            // pivot turn
            else if (move == 0f && turn != 0f)
            {
                // FL
                AddTurnForceAtPoint(frontLeftTransform, turn, turnForce);
                // FR
                AddTurnForceAtPoint(frontRightTransform, turn, turnForce);
                // BL
                AddTurnForceAtPoint(backLeftTransform, -turn, turnForce);
                // BR
                AddTurnForceAtPoint(backRightTransform, -turn, turnForce);

            }
            // slight pivot turn move and turn != 0
            else if (move != 0f && turn != 0f)
            {
                // left
                if(turn < 0)
                {
                    //FR
                    AddDriveForceAtPoint(frontRightTransform, move, driveForce);
                    //BR
                    AddDriveForceAtPoint(backRightTransform, move, driveForce);
                }
                // right
                else
                {
                    //FL
                    AddDriveForceAtPoint(frontLeftTransform, move, driveForce);
                    //BL
                    AddDriveForceAtPoint(backLeftTransform, move, driveForce);
                }
                /*
                // FL
                AddTurnForceAtPoint(frontLeftTransform, turn, turnForce);
                AddDriveForceAtPoint(frontLeftTransform, move, driveForce);
                // FR
                AddTurnForceAtPoint(frontRightTransform, turn, turnForce);
                AddDriveForceAtPoint(frontRightTransform, move, driveForce);
                // BL
                AddTurnForceAtPoint(backLeftTransform, -turn, turnForce);
                AddDriveForceAtPoint(backLeftTransform, move, driveForce);
                // BR
                AddTurnForceAtPoint(backRightTransform, -turn, turnForce);
                AddDriveForceAtPoint(backRightTransform, move, driveForce);
                */
            }
            // no input move and turn == 0
            else
            {

            }
        }

        private void AddFriction(Transform point, float moveInput, float turnInput)
        {
            RaycastHit groundHit;
            if(!Physics.Raycast(point.position, -transform.up, out groundHit, 5f, groundLayer))
            {
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundHit.normal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, groundHit.normal).normalized;

            Vector3 velocity = rb.GetPointVelocity(point.position); ;

            float forwardSpeed = Vector3.Dot(velocity, forward);
            float lateralSpeed = Vector3.Dot(velocity, right);

            float turnAmount = Mathf.Abs(turnInput);

            float lateralGripScale = Mathf.Lerp(1f, 1f - steeringGripReduction, turnAmount);

            Vector3 lateralForce = -right * lateralSpeed * lateralGrip * lateralGripScale;

            lateralForce = Vector3.ClampMagnitude(lateralForce, maxLateralGripForce);

            rb.AddForceAtPosition(lateralForce, point.position, ForceMode.Force);

            float currentDrag = Mathf.Abs(moveInput) > 0.01f ? longitudinalDrag * 0.25f : longitudinalDrag;

            Vector3 forwardDragForce = -forward * forwardSpeed * currentDrag;

            forwardDragForce = Vector3.ClampMagnitude(
                forwardDragForce,
                maxLongitudinalDragForce
            );

            rb.AddForceAtPosition(forwardDragForce, point.position, ForceMode.Force);

            
            if (Mathf.Abs(moveInput) < 0.01f)
            {
                Vector3 brakingForce = -forward * forwardSpeed * brakingDrag;

                brakingForce = Vector3.ClampMagnitude(
                    brakingForce,
                    maxBrakingForce
                );

                rb.AddForceAtPosition(brakingForce, point.position, ForceMode.Force);
            }
        }


        private bool GetGroundHit(Transform point, out RaycastHit hit)
        {
            Vector3 direction = -transform.up;

            return Physics.Raycast(point.position, direction, out hit, 2f, groundLayer);
        }

        private void AddDriveForceAtPoint(Transform point, float move, float driveForce)
        {
            if (!GetGroundHit(point, out RaycastHit groundHit))
                return;

            Vector3 slopeForward = Vector3.ProjectOnPlane(transform.forward, groundHit.normal).normalized;

            float slopeDot = Vector3.Dot(Vector3.up, groundHit.normal);
            float slopeSteepness = 1f - slopeDot;

            float uphillAmount = Vector3.Dot(slopeForward * Mathf.Sign(move), Vector3.up);
            uphillAmount = Mathf.Clamp01(uphillAmount);

            float slopeMultiplier = 1f + uphillAmount * 2.3f;
            float finalDriveForce = driveForce * slopeMultiplier;

            //Debug.Log("FinalDriveForce: " + finalDriveForce);

            rb.AddForceAtPosition(
                slopeForward * move * finalDriveForce,
                point.position,
                ForceMode.Force
            );
        }

        private void AddTurnForceAtPoint(Transform point, float turn, float turnForce)
        {

            if (!GetGroundHit(point, out RaycastHit groundHit))
                return;

            Vector3 slopeRight = Vector3.ProjectOnPlane(transform.right, groundHit.normal).normalized;

            rb.AddForceAtPosition(slopeRight * turn * turnForce, point.position);

        }

        private void AddHandbrakeAtPoint(Transform point)
        {
            if (!GetGroundHit(point, out RaycastHit groundHit))
                return;

            // gravity counterforce
            Vector3 gravity = Physics.gravity * rb.mass;

            Vector3 downhillGravity = Vector3.ProjectOnPlane(gravity, groundHit.normal);

            Vector3 slopeHoldForce = -downhillGravity / 4f;

            rb.AddForceAtPosition(slopeHoldForce, point.position, ForceMode.Force);

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundHit.normal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, groundHit.normal).normalized;

            Vector3 velocity = rb.GetPointVelocity(point.position);

            float forwardSpeed = Vector3.Dot(velocity, forward);
            float lateralSpeed = Vector3.Dot(velocity, right);


            Vector3 longitudinalBrakeForce = -forward * forwardSpeed * handbrakeLongitudinalGrip;

            longitudinalBrakeForce = Vector3.ClampMagnitude(longitudinalBrakeForce, maxHandbrakeLongitudinalForce);

            rb.AddForceAtPosition(longitudinalBrakeForce, point.position, ForceMode.Force);

            Vector3 lateralBrakingForce = -right * lateralSpeed * handbrakeLateralGrip;

            lateralBrakingForce = Vector3.ClampMagnitude(lateralBrakingForce, maxHandbrakeLateralForce);

            rb.AddForceAtPosition(lateralBrakingForce, point.position, ForceMode.Force);
        }


        public void SwitchHandbrake()
        {
            handbrakeActive = !handbrakeActive;
        }

        void Update()
        {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            /* for (int i = 0; i < frontLeftColliders.Count; i++)
             {
                 frontLeftColliders[i].GetWorldPose(out pos, out rot);
                 frontLeftTransforms[i].position = pos;
                 frontLeftTransforms[i].rotation = rot;

                 frontRightColliders[i].GetWorldPose(out pos, out rot);
                 frontRightTransforms[i].position = pos;
                 frontRightTransforms[i].rotation = rot; 
             }*/


            //HandleMovement();
            //LimitSpeed();
        }

        private void HandleMovement()
        {
            //if (!isActive || !IsGrounded()) return;

            Vector2 input = inputManager.GetMovementInput();
            float drive = (input.x + input.y) * 0.5f;
            float rotation = (input.x - input.y) * 0.5f;


            //rb.AddForce(rb.gameObject.transform.forward * drive * driveForce);
            rb.AddRelativeTorque(Vector3.up * rotation * turnForce);
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

        bool IsGrounded(Vector3 point)
        {
            Vector3 origin = point + transform.up * 0.15f;

            Debug.DrawLine(origin, origin + transform.up * (-0.7f), Color.red);


            /*return Physics.SphereCast(
                origin,
                0.18f,
                -transform.up,
                out _,
                0.7f,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );*/

            return Physics.Raycast(origin, -transform.up, 0.7f, groundLayer);

        }

    }
}
