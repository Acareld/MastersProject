using UnityEngine;

namespace Player
{
    public class Interact1 : MonoBehaviour
    {
        private Player.Controller m_playerController;
        private Camera m_mainCamera;

        [Header("Settings")]
        public Transform m_sphereCastOrigin;
        public float m_sphereCastSize = .25f;
        public float m_interactableDistance = 3f;
        public LayerMask m_interactableLayerMask;

        private void Start()
        {
            m_playerController = GetComponentInParent<Player.Controller>();
            m_mainCamera = Camera.main;
        }

        private void Update()
        {
            DetectInteractables();
        }

        public void PerformInteract()
        {
         //   if (m_playerController.m_availableInteractable != null)
          //  {
          //      m_playerController.m_availableInteractable.Interact(m_playerController);
          //  }
        }

        private void DetectInteractables()
        {
            // ----- setup spherecast -----
            Ray ray = new Ray(m_sphereCastOrigin.position, m_mainCamera.transform.forward);
         //   if (m_playerController.m_debugModeEnabled) Debug.DrawRay(ray.origin, ray.direction * m_interactableDistance);
            RaycastHit hitInfo;

            // ----- reset -----
           // m_playerController.m_availableInteractable = null;

            // ----- spherecast check -----
            if (Physics.SphereCast(
                m_sphereCastOrigin.position,
                m_sphereCastSize,
                m_mainCamera.transform.forward,
                out hitInfo,
                m_interactableDistance,
                m_interactableLayerMask
            )) {
              //  if (hitInfo.collider.GetComponent<Interactables.Interactable>() != null)
                {
             //       m_playerController.m_availableInteractable = hitInfo.collider.GetComponent<Interactables.Interactable>();
                }
            }
        }
    }
}