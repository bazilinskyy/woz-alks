using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TORON_OFF : MonoBehaviour
{
    public Light mainLight;

    // define colors based on hex values
    private Color torOnColor = new Color(234f / 255f, 223f / 255f, 192f / 255f); // Hex EADFC0 (TOR ON color)
    private Color nonCriticalTorColor = new Color(255f / 255f, 166f / 255f, 0f); // Hex FFA600 (Non-Critical TOR)
    private Color criticalTorColor = new Color(255f / 255f, 38f / 255f, 0f);     // Hex FF2600 (Critical TOR)
    private Color cyanColor = new Color(0f / 255f, 212f / 255f, 255f / 255f);    // Hex 00D4FF (Cyan)

    private bool isBlinking = false;          // Tracks if the red light is blinking
    private float torTimer = 0f;              // Timer for non-critical and critical TOR
    private bool isNonCriticalActive = false; // Tracks if Non-Critical TOR is active
    private bool isCriticalActive = false;    // Tracks if Critical TOR is active

    void Start()
    {
        // initial color is the beige torOnColor
        if (mainLight != null)
        {
            mainLight.color = torOnColor;
        }
        Camera.main.backgroundColor = torOnColor;
    }

    void Update()
    {
        // check key input to reset the state if any key is pressed
        if (Input.GetKeyDown(KeyCode.O))
        {
            ActivateTorOn();
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            ActivateNonCriticalTor();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            ActivateCriticalTor();
        }

        // Handle the timer and blinking logic
        HandleTorState();
    }

    void ActivateTorOn()
    {
        if (mainLight != null) mainLight.color = torOnColor;
        Camera.main.backgroundColor = torOnColor;
        isNonCriticalActive = false;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;
        Debug.Log("TOR ON: Light and Background color set to EADFC0");
    }

    void ActivateNonCriticalTor()
    {
        if (mainLight != null) mainLight.color = nonCriticalTorColor;
        Camera.main.backgroundColor = nonCriticalTorColor;
        isNonCriticalActive = true;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;
        Debug.Log("Non-Critical TOR: Light and Background color set to FFA600");
    }

    void ActivateCriticalTor()
    {
        if (mainLight != null) mainLight.color = criticalTorColor;
        Camera.main.backgroundColor = criticalTorColor;
        isNonCriticalActive = false;
        isCriticalActive = true;
        isBlinking = false;
        torTimer = 0f;
        Debug.Log("Critical TOR: Light and Background color set to FF2600");
    }

    void ActivateCyan()
    {
        if (mainLight != null) mainLight.color = cyanColor;
        Camera.main.backgroundColor = cyanColor;
        Debug.Log("Cyan State: Light and Background color set to 00D4FF");
    }

    void HandleTorState()
    {
        if (isNonCriticalActive)
        {
            torTimer += Time.deltaTime;
            if (torTimer >= 10f) // after 10 seconds, switch to criticalTor 
            {
                ActivateCriticalTor();
            }
        }
        else if (isCriticalActive)
        {
            torTimer += Time.deltaTime;
            if (torTimer >= 7f && torTimer < 10f) // after 7 seconds of red, 3 seconds of blinking
            {
                if (!isBlinking)
                {
                    isBlinking = true;
                    StartCoroutine(BlinkRedLight());
                }
            }
            else if (torTimer >= 10f) // switch to safety after the 10seconds of CriticalTOr
            {
                StopCoroutine(BlinkRedLight());  // makes blinking stop
                ActivateCyan();  // Set to Cyan after 10 seconds
            }
        }
    }

    IEnumerator BlinkRedLight()
    {
        while (isBlinking && torTimer < 10f)
        {
            if (mainLight != null) mainLight.color = (mainLight.color == criticalTorColor) ? torOnColor : criticalTorColor;
            Camera.main.backgroundColor = (Camera.main.backgroundColor == criticalTorColor) ? torOnColor : criticalTorColor;
            yield return new WaitForSeconds(0.5f); // Toggle color every 0.5 seconds
        }

        // Ensure the colors stay at the critical color after blinking ends
        if (mainLight != null) mainLight.color = criticalTorColor;
        Camera.main.backgroundColor = criticalTorColor;
    }
}

