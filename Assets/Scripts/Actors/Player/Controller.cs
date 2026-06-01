using UnityEngine;

namespace Player
{


    public class Controller : MonoBehaviour
    {

        [HideInInspector]
        public Player.Movement movement;
        public Player.Interact interact;
        public Player.Look look;
        
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
        }

        public void DisableCharacterController()
        {
            characterController.enabled = false;
        }

        public void EnableCharacterController()
        {
            characterController.enabled = true;
        }


        private void LateUpdate()
        {
          
        }



        // Update is called once per frame
        void Update()
        {

        }
    }
}
