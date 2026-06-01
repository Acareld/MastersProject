using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 *  Streamlines input control through one class that either passes information onto the PlayerController
 *  Or is called by the PlayerController to access streamed input values.
 * 
 *      public GetMovementInput()           => vector2 of wasd input between 0 and 1. Called from other scripts as needed.
 *      void InteractPerformed()            => calls appropriate method in player controller.
 */

public class InputManager1 : MonoBehaviour
{
    private InputMaster m_input;
    private Player.Controller m_playerControlller;
   // private TimeManager m_timeManager;

    void Awake()
    {
        m_input = new InputMaster();

       /* m_input.Land.Interact.performed += InteractPerformed;
        m_input.Player.Drop.performed += DropPerformed;
        m_input.Player.SlowTime.performed += SlowTimePerformed;
        m_input.Player.SlowTime.canceled += SlowTimeCanceled;*/
    }

    private void Start()
    {
        m_playerControlller = GameObject.FindWithTag("Player").GetComponent<Player.Controller>();
       // m_timeManager = GameObject.FindWithTag("TimeManager").GetComponent<TimeManager>();
    }

    public Vector2 GetMovementInput()
    {
        return m_input.Land.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseInput()
    {
        return m_input.Land.Look.ReadValue<Vector2>();
    }

    private void InteractPerformed(InputAction.CallbackContext context)
    {
      //  m_playerControlller.m_interact.PerformInteract();
    }

    private void DropPerformed(InputAction.CallbackContext context)
    {
       // m_playerControlller.m_hold.PerformDrop();
    }

    private void SlowTimePerformed(InputAction.CallbackContext context)
    {
      //  m_timeManager.ChangeState(TimeManager.TimeState.Slowed);
    }

    private void SlowTimeCanceled(InputAction.CallbackContext context)
    {
      //  m_timeManager.ChangeState(TimeManager.TimeState.Normal);
    }

    void OnEnable()
    {
        m_input.Enable();
    }

    void OnDisable()
    {
        m_input.Disable();
    }
}
