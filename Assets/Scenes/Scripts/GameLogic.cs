using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private bool cursorLocked = false;

    public float slowMotionFactor = 0.5f; 

    private bool slowMotionActive = false;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !cursorLocked;
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (slowMotionActive == false)
            {
                ActivateSlowMotion();
            } else {
                DeactivateSlowMotion();
            }
        }
    }
    
    public void ActivateSlowMotion()
    {
        Time.timeScale = slowMotionFactor;
        slowMotionActive = true;
    }

    public void DeactivateSlowMotion()
    {
        Time.timeScale = 1.0f; 
        slowMotionActive = false;
    }
}
