using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    private InputMaster inputMaster;
    private Player.Controller playerController;
    private Vehicle.Controller vehicleController;

    private bool inVehicle = false;

    private void Awake()
    {
        inputMaster = new InputMaster();

        inputMaster.Land.Jump.performed += JumpPerformed;
        inputMaster.Land.Interact.performed += InteractPerformed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<Player.Controller>();
        vehicleController = GameObject.FindWithTag("Vehicle").GetComponent<Vehicle.Controller>();
    }

    public Vector2 GetMovementInput()
    {
        if(inVehicle)
        {
            // tracks independently
            Vector2 vehicleInput = new Vector2();
            /*vehicleInput.x = inputMaster.Vehicle.LeftTrack.ReadValue<float>();
            vehicleInput.y = inputMaster.Vehicle.RightTrack.ReadValue<float>();*/

            float move = inputMaster.Vehicle.Move.ReadValue<float>();
            float turn = inputMaster.Vehicle.Turn.ReadValue<float>();

            vehicleInput.x = move;
            vehicleInput.y = turn;

            return vehicleInput;
        }
        else
        {
            return inputMaster.Land.Move.ReadValue<Vector2>();
        }
    }

    private void InteractPerformed(InputAction.CallbackContext context)
    {
        if(inVehicle)
        {
            inVehicle = false;
            playerController.gameObject.SetActive(true);
            vehicleController.look.DisableCamera();
            vehicleController.movement.isActive = false; 
        }
        else
        {
            inVehicle = true;
            vehicleController.look.EnableCamera();
            playerController.gameObject.SetActive(false);
            vehicleController.movement.isActive = true;
        }      
    }


    public Vector2 GetMouseInput()
    {
        return inputMaster.Land.Look.ReadValue<Vector2>();
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        if(!inVehicle)
        {
            playerController.movement.Jump();
        } 
        else
        {
            vehicleController.movement.SwitchHandbrake();
        }
    }

    void OnEnable()
    {
        inputMaster.Enable();    
    }

    void OnDisable()
    {
        inputMaster.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
