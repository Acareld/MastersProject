using TMPro;
using UnityEngine;

namespace Vehicle
{
    public class InteractVehicle : MonoBehaviour
    {
        [SerializeField]
        private float castDistance = 3f;
        [SerializeField]
        private Vector3 raycastOffset = new Vector3 (0f, 0f, 0f);

        [SerializeField]
        private Transform lookPosition;

        [SerializeField]
        private TMP_Text text;

        private IInteractable currentInteractable;

        private bool bUseVehicleInteract = false;


        void Awake()
        {
            text.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if(bUseVehicleInteract)
            {
                UpdateInteractionTarget();
            }
            
        }

        private void UpdateInteractionTarget()
        {
            if (DoInteractTest(out IInteractable interactable) && interactable.CanInteract())
            {
                currentInteractable = interactable;
                text.text = interactable.GetInteractionText();
                text.gameObject.SetActive(true);
            }
            else
            {
                currentInteractable = null;
                text.gameObject.SetActive(false);
            }
        }

        public void OnInteract()
        {
            if (currentInteractable == null) return;


            if (currentInteractable.CanInteract())
            {
                currentInteractable.Interact(this);
            }

        }

        private bool DoInteractTest(out IInteractable interactable)
        {
            interactable = null;

            Ray ray = new Ray(lookPosition.position + raycastOffset, lookPosition.forward);

            //Debug.DrawRay(transform.position + raycastOffset, lookPosition.forward * castDistance, Color.red, 100f);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, castDistance))
            {
                interactable = hitInfo.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void Enable()
        {
            bUseVehicleInteract = true;
        }
        public void Disable()
        {
            bUseVehicleInteract = false;
        }
    }
}
