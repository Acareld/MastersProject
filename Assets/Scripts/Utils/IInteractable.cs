using UnityEngine;

public interface IInteractable
{
    public bool CanInteract();

    public bool Interact(Player.Interact interactor);
    public bool Interact(Vehicle.InteractVehicle interactor);

    public string GetInteractionText();
}
