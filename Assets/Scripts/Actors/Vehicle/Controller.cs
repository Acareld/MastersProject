using UnityEditor;
using UnityEngine;

namespace Vehicle
{
    public class Controller : MonoBehaviour
    {
        [HideInInspector]
        public Vehicle.Movement movement;
        public Vehicle.LookVehicle look;

        [Header("Statuses")]
        public bool canMove = true;

        private void Awake()
        {
            movement = GetComponentInChildren<Vehicle.Movement>();
            look = GetComponentInChildren<Vehicle.LookVehicle>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        


        
    }
}
