using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Vehicle
{
    public class Controller : MonoBehaviour
    {
        [HideInInspector]
        public Vehicle.Movement movement;
        public Vehicle.LookVehicle look;
        public Vehicle.InteractVehicle interact;
        public Vehicle.VehicleDamage vehicleDamage;
        public GameObject driverSpawnTransformL;
        public GameObject driverSpawnTransformR;

        [Header("Statuses")]
        public bool canMove = true;

        private bool doorOpenR = false;
        private bool doorOpenL = false;

        [SerializeField]
        private TMP_Text infoText;

        [SerializeField]
        private TMP_Text hpText;

        private Coroutine infoTextCoroutine;

        private void Awake()
        {
            movement = GetComponentInChildren<Vehicle.Movement>();
            look = GetComponentInChildren<Vehicle.LookVehicle>();
            interact = GetComponentInChildren<Vehicle.InteractVehicle>(); 
            vehicleDamage = GetComponentInChildren<Vehicle.VehicleDamage>();
        }

        private void Start()
        {
            driverSpawnTransformL = GameObject.FindWithTag("DriverSpawnPointLeft");
            driverSpawnTransformR = GameObject.FindWithTag("DriverSpawnPointRight");

            hpText.text = "Vehicle Health: " + vehicleDamage.maxHealth;
            vehicleDamage.damageReceivedAction += UpdateHPText;
        }

        private void UpdateHPText(float currentHP)
        {
            hpText.text = "Vehicle Health: " + Mathf.RoundToInt(currentHP);
        }

        public Transform GetDriverSpawnPointTransform()
        {
            if(doorOpenL)
            {
                return driverSpawnTransformL.transform;
            }
            else if(doorOpenR)
            {
                return driverSpawnTransformR.transform;
            }
            return driverSpawnTransformL.transform;
        }

        public void Enable()
        {
            movement.Enable();
            StartCoroutine(DamageStartDelay());
        }

        public void Disable()
        {
            movement.Disable();
            vehicleDamage.Disable();
        }

        private IEnumerator DamageStartDelay()
        {
            yield return new WaitForSeconds(1);
            vehicleDamage.Enable();
        }

        public DamageData GetVehicleDamageData()
        {
            return vehicleDamage.CollectAndResetDataCollection();
        }

        public void ResetVehicleHealth()
        {
            vehicleDamage.ResetHealth();
            UpdateHPText(vehicleDamage.maxHealth);
        }

        public void UpdateDoorState(bool isOpen, float openDirection)
        {
            // left door
            if(openDirection > 0f)
            {
                doorOpenL = isOpen;
            }
            else
            {
                doorOpenR = isOpen;
            }
        }

        public bool CanExit()
        {
            if(doorOpenR || doorOpenL)
            {
                return true;
            }

            SetInfoText("A Door has to be open so you can exit the vehicle!");
            return false;
        }    

        public void SetInfoText(string text)
        { 
            if(infoTextCoroutine != null)
            {
                StopCoroutine(infoTextCoroutine);
            }

            infoTextCoroutine = StartCoroutine(InfoTextRoutine(text));
        }

        private IEnumerator InfoTextRoutine(string text)
        {
            infoText.text = text;
            infoText.gameObject.SetActive(true);

            yield return new WaitForSeconds(5);

            infoText.gameObject.SetActive(false);
        }

    }
}
