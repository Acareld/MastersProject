using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    private InputMaster inputMaster;
    private Player.Controller playerController;
    private Vehicle.Controller vehicleController;
    private GameManager gameManager;

    public bool inVehicle = false;

    private void Awake()
    {
        inputMaster = new InputMaster();

        inputMaster.Land.Jump.performed += JumpPerformed;
        inputMaster.Land.Interact.performed += InteractPerformed;
        inputMaster.Land.Respawn.performed += RespawnPerformed;
        inputMaster.Vehicle.Exit.performed += ExitVehiclePerformed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<Player.Controller>();
        vehicleController = GameObject.FindWithTag("Vehicle").GetComponent<Vehicle.Controller>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public Vector2 GetMovementInput()
    {
        if(inVehicle)
        {
            // tracks independently
            Vector2 vehicleInput = new Vector2();

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
            /* inVehicle = false;
             vehicleController.look.DisableCamera();
             playerController.ExitVehicle(vehicleController.GetDriverSpawnPointTransform());
             vehicleController.movement.isActive = false; 
             playerController.movement.isActive = true;*/
            vehicleController.interact.OnInteract();
        }
        else
        {
            playerController.interact.OnInteract();
            /*inVehicle = true;
            vehicleController.look.EnableCamera();
            playerController.EnterVehicle();
            vehicleController.movement.isActive = true;*/
        }      
    }

    private void ExitVehiclePerformed(InputAction.CallbackContext context)
    {
        if(inVehicle)
        {
            if(vehicleController.CanExit())
            {
                vehicleController.interact.Disable();
                inVehicle = false;
                vehicleController.look.DisableCamera();
                playerController.ExitVehicle(vehicleController.GetDriverSpawnPointTransform());
                vehicleController.movement.isActive = false;
                playerController.movement.isActive = true;
            }
            else
            {
                Debug.LogWarning("Door not open, cannot exit");
            }
            
        }
        else
        {
            Debug.LogWarning("Not in Vehicle, cannot exit");
        }
    }

    private void RespawnPerformed(InputAction.CallbackContext context)
    {
        if (inVehicle) inVehicle = !inVehicle;
        gameManager.Respawn();
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
