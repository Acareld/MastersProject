using UnityEngine;

namespace Player
{
    public class EventInteract : MonoBehaviour
    {
        private Player.Controller m_playerController;
        private Camera m_mainCamera;

        [Header("Settings")]
        public Transform m_sphereCastOrigin;
        public float m_sphereCastSize = .25f;
        public float m_eventableDistance = 3f;
        public LayerMask m_eventableLayerMask;

        private void Start()
        {
            m_playerController = GetComponentInParent<Player.Controller>();
            m_mainCamera = Camera.main;
        }

        private void Update()
        {
            DetectEventables();
        }

        public void PerformInteract()
        {
         //   if (m_playerController.m_availableEventable != null)
         //   {
          //      m_playerController.m_availableEventable.Interact(m_playerController);
         //   }
        }

        private void DetectEventables()
        {
            // ----- setup spherecast -----
            Ray ray = new Ray(m_sphereCastOrigin.position, m_mainCamera.transform.forward);
         //   if (m_playerController.m_debugModeEnabled) Debug.DrawRay(ray.origin, ray.direction * m_eventableDistance);
            RaycastHit hitInfo;

            // ----- reset -----
         //   m_playerController.m_availableEventable = null;

            // ----- spherecast check -----
            if (Physics.SphereCast(
                m_sphereCastOrigin.position,
                m_sphereCastSize,
                m_mainCamera.transform.forward,
                out hitInfo,
                m_eventableDistance,
                m_eventableLayerMask
            )) {
          //      if (hitInfo.collider.GetComponent<Interactables.Eventable>() != null)
                {
          //          m_playerController.m_availableEventable = hitInfo.collider.GetComponent<Interactables.Eventable>();
                }
            }
        }
    }
}