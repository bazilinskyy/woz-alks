using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO; // this is for the CSV
using System;   // this is DateTime

public class TORON_OFF : MonoBehaviour
{
    public Light mainLight;
    public TMP_Text alksStatusText; // TMP_Text for status updates
    public AudioSource popAudioSource;
    public AudioSource beepAudioSource;

    private Color torOnColor = new Color(234f / 255f, 223f / 255f, 192f / 255f); // Hex EADFC0 
    private Color torOffColor = new Color(0f, 212f / 255f, 255f / 255f);         // hex is 00D4FF
    private Color nonCriticalTorColor = new Color(255f / 255f, 166f / 255f, 0f); // #FFA600 
    private Color criticalTorColor = new Color(255f / 255f, 38f / 255f, 0f);     // #FF2600 
    private Color darkGreyColor = new Color(76f / 255f, 76f / 255f, 76f / 255f); // #4C4C4C 
    private Color alksReadyColor = new Color(140f / 255f, 115f / 255f, 255f / 255f); //#8C73FF


    private bool isBlinking = false;
    private float torTimer = 0f;
    private bool isNonCriticalActive = false;
    private bool isCriticalActive = false;
    private bool isShutdownLogged = false;

    private string filePath;
    private Coroutine beepCoroutine;

    void Start()
    {
        // generating the unique CSV file
        string folderPath = Path.Combine(Application.dataPath, "CSV");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string fileName = DateTime.Now.ToString("dd-MM-yy-HH-mm-ss") + ".csv";
        filePath = Path.Combine(folderPath, fileName);
        File.WriteAllText(filePath, "Timestamp,Keypress,State,Color\n");

        // initial light and background
        if (mainLight != null)
        {
            mainLight.color = torOnColor;
        }
        Camera.main.backgroundColor = torOnColor;

        // initial text status
        if (alksStatusText != null)
        {
            alksStatusText.text = "Manual Mode";
        }

        LogInteraction("0", "Manual Mode", "Beige");
    }

    void Update()
    {
        // see key input
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayPopSound();
            ActivateTorOn();
            LogInteraction("A", "Manual Mode", "Beige");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            PlayPopSound();
            ActivateAlksReady();
            LogInteraction("S", "ALKS Ready", "Purple");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            PlayPopSound();
            ActivateTorOff();
            LogInteraction("D", "ALKS ON", "Cyan");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            PlayPopSound();
            ActivateNonCriticalTor();
            LogInteraction("F", "Requesting Manual Take Over", "Orange");
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            PlayPopSound();
            ActivateCriticalTor();
            LogInteraction("G", "Requesting Manual Take Over", "Red");
        }

        HandleTorState();
    }

    void ActivateTorOn()
    {
        SetLightAndBackground(torOnColor);
        isNonCriticalActive = false;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;

        UpdateStatusText("Manual Mode");
        StopBeepSound();
    }

    void ActivateTorOff()
    {
        SetLightAndBackground(torOffColor);
        isNonCriticalActive = false;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;

        UpdateStatusText("ALKS ON");
        StopBeepSound();
    }

    void ActivateNonCriticalTor()
    {
        SetLightAndBackground(nonCriticalTorColor);
        isNonCriticalActive = true;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;

        //text but log "Non Critical" for CSV
        UpdateStatusText("Requesting Manual Take Over");
        StartBeepSound();

        // logging "Non Critical" for the CSV
        LogInteraction("D", "Non Critical", "Orange");
    }

    void ActivateCriticalTor()
    {
        SetLightAndBackground(criticalTorColor);
        isNonCriticalActive = false;
        isCriticalActive = true;
        isBlinking = false;
        torTimer = 0f;

        UpdateStatusText("Requesting Manual Take Over");
        StartBeepSound();

        LogInteraction("0", "Critical", "Red");
    }

    void ActivateDarkGrey()
    {
        SetLightAndBackground(darkGreyColor);
        UpdateStatusText("System Shut Down");
        StopBeepSound();
    }

    void ActivateAlksReady()
    {
        SetLightAndBackground(alksReadyColor);
        isNonCriticalActive = false;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;

        UpdateStatusText("ALKS Ready");
        StopBeepSound();
    }


    void SetLightAndBackground(Color color)
    {
        if (mainLight != null) mainLight.color = color;
        Camera.main.backgroundColor = color;
    }

    void HandleTorState()
    {
        if (isNonCriticalActive)
        {
            torTimer += Time.deltaTime;
            if (torTimer >= 10f)
            {
                ActivateCriticalTor();
                LogInteraction("0", "Requesting Manual Take Over", "Red");
            }
        }
        else if (isCriticalActive)
        {
            torTimer += Time.deltaTime;
            if (torTimer >= 7f && torTimer < 10f)
            {
                if (!isBlinking)
                {
                    isBlinking = true;
                    StartCoroutine(BlinkRedLight());
                }
            }
            else if (torTimer >= 10f)
            {
                StopCoroutine(BlinkRedLight());
                ActivateDarkGrey();

                // Log shutdown only if it hasn't been logged yet
                if (!isShutdownLogged)
                {
                    LogInteraction("0", "System Shut Down", "Black");
                    isShutdownLogged = true; // Mark shutdown as logged
                }
            }
        }
        else
        {
            // Reset the shutdown flag when not in critical state
            isShutdownLogged = false;
        }
    }

    IEnumerator BlinkRedLight()
    {
        while (isBlinking && torTimer < 10f)
        {
            SetLightAndBackground((mainLight.color == criticalTorColor) ? torOnColor : criticalTorColor);
            yield return new WaitForSeconds(0.5f);
        }
        SetLightAndBackground(criticalTorColor);
    }

    void StartBeepSound()
    {
        if (beepCoroutine != null) StopCoroutine(beepCoroutine);
        beepCoroutine = StartCoroutine(BeepSound());
    }

    void StopBeepSound()
    {
        if (beepCoroutine != null)
        {
            StopCoroutine(beepCoroutine);
            beepCoroutine = null;
        }
    }

    IEnumerator BeepSound()
    {
        while (isNonCriticalActive)
        {
            if (beepAudioSource != null)
            {
                beepAudioSource.pitch = 0.5f;
                beepAudioSource.volume = 0.5f;
                beepAudioSource.Play();
            }
            yield return new WaitForSeconds(1.5f); // Beep every 1.5 seconds in non-critical state
        }

        while (isCriticalActive)
        {
            float interval = torTimer >= 7f ? 0.5f : 1.0f; // Last 3 seconds: beep every 0.5s
            if (beepAudioSource != null)
            {
                beepAudioSource.pitch = 0.8f;
                beepAudioSource.volume = 1.0f;
                beepAudioSource.Play();
            }
            yield return new WaitForSeconds(interval); // Adjust interval dynamically
        }
    }



    void PlayPopSound()
    {
        if (popAudioSource != null) popAudioSource.Play();
    }

    void UpdateStatusText(string status)
    {
        if (alksStatusText != null)
        {
            alksStatusText.text = status;
        }
    }

    void LogInteraction(string keypress, string state, string color)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss:fff");
        string logEntry = $"{timestamp},{keypress},{state},{color}\n";
        File.AppendAllText(filePath, logEntry);
        Debug.Log($"Logged: {logEntry}");
    }
}
