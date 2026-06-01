using UnityEngine;

namespace Player
{
    public class Controller1 : MonoBehaviour
    {
       // private TimeManager m_timeManager;

        [HideInInspector]
        public Player.Movement m_movement;
        public Player.Hold m_hold;
        public Transform m_holdingPosition;

        [Header("Ghost Controller")]
        public bool m_isOnShip = false;
        public bool m_isShipControllerActive = false;
       // public GhostPlayer.Controller m_ghostPlayerController;
        public Transform m_realShip;

        [Header("Statuses")]
        public bool m_debugModeEnabled = false;
        public bool m_canMove = true;
        public bool m_canRun = true;
        public bool m_canInteract = true;

        [Header("Interactions")]
        public Player.Interact m_interact;
       // public Interactables.Interactable m_availableInteractable = null;

        [Header("EventInteractions")]
        public Player.EventInteract m_eventInteract;
       // public Interactables.Eventable m_availableEventable = null;

      //  public WorldSpaceUI m_worldSpaceUI;
        public bool m_isUIOn = false;

        [Header("SphereCasting")]
        public int m_shipLayerNumber = 0;

        private void Awake()
        {
            m_movement = GetComponentInChildren<Player.Movement>();
            m_interact = GetComponentInChildren<Player.Interact>();
            m_hold = GetComponentInChildren<Player.Hold>();

         //   m_timeManager = GameObject.FindGameObjectWithTag("TimeManager").GetComponent<TimeManager>();
        }

        private void Update()
        {
         /*   // ----- set world space ui -----
            if (m_worldSpaceUI != null)
            {
                if (
                    m_availableEventable != null
                    && m_timeManager.m_currentTimeState == TimeManager.TimeState.Slowed
                    && m_isUIOn == false
                )
                {
                    m_worldSpaceUI.SetPosition(m_availableEventable.transform.position);
                    m_worldSpaceUI.SetOptions(m_availableEventable.m_options);
                    m_worldSpaceUI.TurnOn();

                    m_isUIOn = true;
                }
                else if (m_availableEventable == null || m_timeManager.m_currentTimeState != TimeManager.TimeState.Slowed)
                {
                    m_worldSpaceUI.TurnOff();
                    m_isUIOn = false;
                }
            }
         */
            // ----- check if is on ship -----
            RaycastHit hit;
            if(Physics.SphereCast(
                m_movement.transform.position,
                m_movement.gameObject.GetComponent<CharacterController>().radius,
                Vector3.down,
                out hit
            ))
            {
                m_isOnShip = hit.collider.gameObject.layer == m_shipLayerNumber ? true : false;
            }

            // ----- handle toggling ship controller -----
            if(!m_isOnShip && m_isShipControllerActive)
            {
             //   m_movement.m_isActive = true;
              //  m_ghostPlayerController.Deactivate(this);
                m_isShipControllerActive = false;
            }
            else if(m_isOnShip && !m_isShipControllerActive)
            {
              //  m_movement.m_isActive = false;
              //  m_ghostPlayerController.Activate(
               //     this,
                //    m_movement.transform,
                //    m_realShip
               // );
                m_isShipControllerActive = true;
            }
        }
    }
}