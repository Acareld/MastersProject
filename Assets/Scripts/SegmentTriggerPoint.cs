using UnityEngine;

public class SegmentTriggerPoint : MonoBehaviour
{

    private bool bHasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (bHasTriggered) return;

        if (!other.CompareTag("PlayerChar") && !other.CompareTag("VehicleRigidbody")) return;
            
        bHasTriggered = true;

        GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.EvaluateAndGenerateNext();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
