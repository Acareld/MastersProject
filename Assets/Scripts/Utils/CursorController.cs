using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class simply provides public static methods for hiding/locking the cursor
 */
public class CursorController : MonoBehaviour
{
    public static void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
