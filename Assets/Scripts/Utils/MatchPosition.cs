using UnityEngine;

public class MatchPosition : MonoBehaviour
{
    public Transform m_targetTransform;
    public float m_matchSpeed = 1.0f;

    private Vector3 m_currentPosition;
    private Vector3 m_targetPosition;

    private void Update()
    {
        m_targetPosition = m_targetTransform.position;

        m_currentPosition = Vector3.Lerp(transform.position, m_targetPosition, Time.deltaTime * m_matchSpeed);

        transform.position = m_currentPosition;
    }
}
