using UnityEngine;

namespace Player
{
    public class Hold : MonoBehaviour
    {
        [SerializeField] private Transform m_dropPosition;
      //  public Interactables.Holdable m_heldObject;
        public Transform m_heldObjectLookPoint;
        public Player.Controller m_playerController;

        public void Update()
        {
           /* if (m_heldObject != null)
            {
                m_heldObject.transform.position = transform.position;
                m_heldObject.transform.rotation = transform.rotation;
            }*/
        }

        public void PerformDrop()
        {
           /* if (m_heldObject == null) return;

            m_heldObject.transform.position = m_dropPosition.position;
            m_heldObject.transform.rotation = m_dropPosition.rotation;
            m_heldObject.Drop(m_playerController);*/
        }

        public void RemoveHeldObject()
        {
           // GameObject.Destroy(m_heldObject.gameObject);
           // m_heldObject = null;
        }
    }
}
