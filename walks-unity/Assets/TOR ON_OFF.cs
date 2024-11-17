using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // for CSV
using System;   // For DateTime

public class TORON_OFF : MonoBehaviour
{
    public Light mainLight;

    // Define colors based on hex values
    private Color torOnColor = new Color(234f / 255f, 223f / 255f, 192f / 255f); // Hex EADFC0 
    private Color nonCriticalTorColor = new Color(255f / 255f, 166f / 255f, 0f); // Hex FFA600 
    private Color criticalTorColor = new Color(255f / 255f, 38f / 255f, 0f);     // Hex FF2600 
    private Color darkGreyColor = new Color(76f / 255f, 76f / 255f, 76f / 255f); // Hex 4C4C4C 

    private bool isBlinking = false;
    private float torTimer = 0f;
    private bool isNonCriticalActive = false;
    private bool isCriticalActive = false;

    private string filePath; // pathing to the CSV file and creating new one if there is none

    void Start()
    {
        // creating the file path for the CSV file
        string folderPath = Path.Combine(Application.dataPath, "CSV");
        filePath = Path.Combine(folderPath, "TORData.csv");

        // does the CSV folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // creting the file and write the header if it doesn't exist
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp,Keypress,Color\n");
        }

        // this is the initial color is the beige torOnColor
        if (mainLight != null)
        {
            mainLight.color = torOnColor;
        }
        Camera.main.backgroundColor = torOnColor;

        // logging the initial state
        LogInteraction("0", torOnColor);
    }

    void Update()
    {
        // checking the key input to reset the state if any key is pressed
        if (Input.GetKeyDown(KeyCode.A))
        {
            ActivateTorOn();
            LogInteraction("A", torOnColor);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ActivateNonCriticalTor();
            LogInteraction("S", nonCriticalTorColor);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ActivateCriticalTor();
            LogInteraction("D", criticalTorColor);
        }

        // the the timer and blinking logic
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

    void ActivateDarkGrey()
    {
        if (mainLight != null) mainLight.color = darkGreyColor;
        Camera.main.backgroundColor = darkGreyColor;
        Debug.Log("Dark Grey State: Light and Background color set to 4C4C4C");
        LogInteraction("0", darkGreyColor); // Log dark grey activation
    }

    void HandleTorState()
    {
        if (isNonCriticalActive)
        {
            torTimer += Time.deltaTime;
            if (torTimer >= 10f) // after the 10 seconds, switch to criticalTor 
            {
                ActivateCriticalTor();
                LogInteraction("0", criticalTorColor);
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
            else if (torTimer >= 10f) // switching to safety after the 10 seconds of CriticalTOR
            {
                StopCoroutine(BlinkRedLight());
                ActivateDarkGrey();
            }
        }
    }

    IEnumerator BlinkRedLight()
    {
        while (isBlinking && torTimer < 10f)
        {
            if (mainLight != null) mainLight.color = (mainLight.color == criticalTorColor) ? torOnColor : criticalTorColor;
            Camera.main.backgroundColor = (Camera.main.backgroundColor == criticalTorColor) ? torOnColor : criticalTorColor;
            yield return new WaitForSeconds(0.5f); // toggle color every 0.5 seconds
        }

        //  the colors stay at the critical color after blinking ends
        if (mainLight != null) mainLight.color = criticalTorColor;
        Camera.main.backgroundColor = criticalTorColor;
    }

    void LogInteraction(string keypress, Color color)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        string logEntry = $"{timestamp},{keypress},{colorHex}\n";

        // creates the log entry to the file
        File.AppendAllText(filePath, logEntry);

        Debug.Log($"Logged: {logEntry}");
    }
}
