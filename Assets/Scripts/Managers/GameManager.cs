using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor.SpeedTree.Importer;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Difficulty difficulty;

    private TerrainGeneratorManager generatorManager;
    private DataCollector dataCollector;

    [SerializeField]
    private bool bShouldGenerate = true;

    [SerializeField] private GameObject vehiclePrefab;
    [SerializeField] private GameObject playerPrefab;

    private bool firstSegment = true;

    private Player.Controller playerController;
    private Vehicle.Controller vehicleController;
    private InputManager inputManager;
    private LoadingScreenManager loadingScreenManager;

    private int currentResets = 0;
    private int segmentIndex = -1;

    private bool bUseFixedDifficulty = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generatorManager = GameObject.FindWithTag("TerrainGeneratorManager").GetComponent<TerrainGeneratorManager>();
        dataCollector = GameObject.FindWithTag("DataCollector").GetComponent<DataCollector>();
        inputManager = GameObject.FindWithTag("InputManager").GetComponent<InputManager>();
        loadingScreenManager = GameObject.FindWithTag("LoadingScreenManager").GetComponent<LoadingScreenManager>();

        playerController = GameObject.FindWithTag("Player").GetComponent<Player.Controller>();
        vehicleController = GameObject.FindWithTag("Vehicle").GetComponent<Vehicle.Controller>();

        playerController.Disable();
        vehicleController.Disable();

        vehicleController.vehicleDamage.noHealthAction += VehicleForcedRespawn;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if(PersistantManager.instance.UseFixedDifficulty())
        {
            bUseFixedDifficulty = true;
            difficulty = PersistantManager.instance.GetFixedDifficulty();
        }

        if (bShouldGenerate)
        {
            StartCoroutine(InitialGenerationRoutine());
            bShouldGenerate = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if(bShouldGenerate)
        {
            DifficultyState state = DifficultyManager.instance.GetDifficultySettings(difficulty);
            GenerateNextTerrain();
            if (firstSegment)
            {
                generatorManager.GenerateNextPath(state);
                dataCollector.StartTimeEvaluation();
                Spawn();
                firstSegment = false;
            }

            bShouldGenerate = false;
        }*/
    }
    private IEnumerator InitialGenerationRoutine()
    {
        DifficultyState state =
            DifficultyManager.instance.GetDifficultySettings(difficulty);

        generatorManager.GenerateNextTerrainSegment();

        yield return StartCoroutine(
            generatorManager.GenerateNextPathCoroutine(state)
        );

        // At this point the path-generation coroutine has finished.
        dataCollector.StartTimeEvaluation();

        Respawn();

        loadingScreenManager.StartFadeOut();

        firstSegment = false;

        Debug.Log("Initial generation finished and player spawned.");
    }

    public void EvaluateAndGenerateNext()
    {
        if(bUseFixedDifficulty)
        {
            DifficultyState fixedState = DifficultyManager.instance.GetDifficultySettings(difficulty);
            generatorManager.GenerateNextPath(fixedState);
        }
        else
        {
            dataCollector.SetData(vehicleController.GetVehicleDamageData(), currentResets);

            difficulty = dataCollector.Evaluate(difficulty, segmentIndex);

            DifficultyState state = DifficultyManager.instance.GetDifficultySettings(difficulty);
            generatorManager.GenerateNextPath(state);

            dataCollector.StartTimeEvaluation();
        }

        

        vehicleController.vehicleDamage.ResetHealth();
        currentResets = 0;
        segmentIndex++;
    }

    public void GenerateNextTerrain()
    {
        generatorManager.GenerateNextTerrainSegment();
    }

    public void Spawn()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(5);

        Respawn();
        loadingScreenManager.StartFadeOut();
        Debug.Log("Spawned");
    }

    private void VehicleForcedRespawn(float damage)
    {
        Debug.Log("GameManger ForceRespawn, too much damage, last tick: " + damage);
        if (inputManager.inVehicle) inputManager.inVehicle = false;
        Respawn();
    }

    public void Respawn()
    {
        Vector3 respawnPosition = generatorManager.GetCurrentRespawnPoint(out int generatorIndex);
        generatorManager.ForceColliderUpdate(generatorIndex);
        respawnPosition.y += 2.5f;

        playerController.Disable();       
        vehicleController.Disable();

        GameObject vehicle = GameObject.FindWithTag("VehicleRigidbody");

        vehicle.transform.position = respawnPosition;
        vehicle.transform.rotation = Quaternion.identity; 

        playerController.ExitVehicle(vehicleController.GetDriverSpawnPointTransform());
        playerController.Enable();
        vehicleController.Enable();
        vehicleController.vehicleDamage.ResetHealth();
        currentResets++;
        
    }

    private void ResetGameData()
    {
        currentResets = 0;
    }
   
}
