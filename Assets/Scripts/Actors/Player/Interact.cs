using TMPro;
using UnityEngine;

namespace Player
{
    public class Interact : MonoBehaviour
    {
        [SerializeField]
        private float castDistance = 5f;
        [SerializeField]
        private Vector3 raycastOffset = new Vector3(0f, 0f, 0f);

        [SerializeField]
        private Transform lookPosition;

        [SerializeField]
        private TMP_Text text;

        private IInteractable currentInteractable;


        void Awake()
        {
            text.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateInteractionTarget();
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

            Ray ray = new Ray(transform.position + raycastOffset, lookPosition.forward);

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
    }
}
