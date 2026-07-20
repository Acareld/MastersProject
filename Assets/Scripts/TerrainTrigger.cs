using UnityEngine;

public class TerrainTrigger : MonoBehaviour
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
            gameManager.GenerateNextTerrain();
        }
    }
}
