using UnityEngine;

namespace Player
{


    public class Controller : MonoBehaviour
    {

        [HideInInspector]
        public Player.Movement movement;
        public Player.Interact interact;
        public Player.Look look;
        public MeshRenderer modelMeshRenderer;
        
        private CharacterController characterController;
        private LockToPosition cameraLock;

        [Header("Statuses")]
        public bool debugModeEnabled = false;
        public bool canMove = true;

        private void Awake()
        {
            movement = GetComponentInChildren<Player.Movement>();
            interact = GetComponentInChildren<Player.Interact>();
            look = GetComponentInChildren<Player.Look>();
            characterController = GetComponentInChildren<CharacterController>();
            cameraLock = GetComponentInChildren<LockToPosition>();
            modelMeshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public void DisableCharacterController()
        {
            characterController.enabled = false;
        }

        public void EnableCharacterController()
        {
            characterController.enabled = true;
        }

        public void Disable()
        {
            movement.isActive = false;
            
        }

        public void Enable()
        {
            movement.isActive = true;
        }

        private void LateUpdate()
        {
          
        }

        public void EnterVehicle()
        {
            movement.isActive = false;
            look.DisableCamera();
            modelMeshRenderer.enabled = false;
            DisableCharacterController();
        }

        public void ExitVehicle(Transform exitPosition)
        {
            DisableCharacterController();
            look.EnableCamera();
            characterController.gameObject.transform.position = exitPosition.position; 
            modelMeshRenderer.enabled = true;
            EnableCharacterController();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
