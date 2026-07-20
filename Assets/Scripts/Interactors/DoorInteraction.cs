using Player;
using System.Collections;
using UnityEngine;
using Vehicle;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private float openAngle = 50f;
    [SerializeField] private float openSpeed = 6f;

    [SerializeField] private float openDirection = 1f;

    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine rotateRoutine;

    private Vehicle.Controller vehicleController;

    private void Awake()
    {
        closedRotation = transform.localRotation;

        openRotation = closedRotation * Quaternion.AngleAxis(openAngle * openDirection, Vector3.forward);
    }

    void Start()
    {
        vehicleController = GameObject.FindWithTag("Vehicle").GetComponent<Vehicle.Controller>();
    }

    public bool CanInteract()
    {
        return true;
    }

    public bool Interact(Interact interactor)
    {
        isOpen = !isOpen;

        vehicleController.UpdateDoorState(isOpen, openDirection);

        if(rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
        }

        rotateRoutine = StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));

        return true;
    }

    

    public bool Interact(InteractVehicle interactor)
    {
        isOpen = !isOpen;

        vehicleController.UpdateDoorState(isOpen, openDirection);

        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
        }

        rotateRoutine = StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));

        return true;
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        while(Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);

            yield return null;
        }

        transform.localRotation = targetRotation;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetInteractionText()
    {
        if (isOpen) return "Close Door";
        return "Open Door";
    }
}
