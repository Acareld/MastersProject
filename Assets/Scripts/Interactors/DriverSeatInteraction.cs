using Player;
using UnityEngine;
using Vehicle;

public class DriverSeatInteraction : MonoBehaviour, IInteractable
{
    private Player.Controller playerController;
    private Vehicle.Controller vehicleController;
    private InputManager inputManager;

    private void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<Player.Controller>();
        vehicleController = GameObject.FindWithTag("Vehicle").GetComponent<Vehicle.Controller>();
        inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
    }

    public bool CanInteract()
    {
        return true;
    }

    public bool Interact(Interact interactor)
    {
        vehicleController.look.EnableCamera();
        playerController.EnterVehicle();
        vehicleController.movement.isActive = true;
        inputManager.inVehicle = true;
        vehicleController.interact.Enable();

        return true;
    }

    public bool Interact(InteractVehicle interactor)
    {
        return false;
    }

    public string GetInteractionText()
    {
        return "Get in Driver Seat";
    }
}
