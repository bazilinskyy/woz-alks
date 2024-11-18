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
    private Color nonCriticalTorColor = new Color(255f / 255f, 166f / 255f, 0f); // Hex FFA600 
    private Color criticalTorColor = new Color(255f / 255f, 38f / 255f, 0f);     // Hex FF2600 
    private Color darkGreyColor = new Color(76f / 255f, 76f / 255f, 76f / 255f); // Hex 4C4C4C 

    private bool isBlinking = false;
    private float torTimer = 0f;
    private bool isNonCriticalActive = false;
    private bool isCriticalActive = false;

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
            alksStatusText.text = "TOR ON - ALKS OFF";
        }

        LogInteraction("0", "TOR ON - ALKS OFF", "White");
    }

    void Update()
    {
        // see key input
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayPopSound();
            ActivateTorOn();
            LogInteraction("A", "TOR ON - ALKS OFF", "White");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            PlayPopSound();
            ActivateNonCriticalTor();
            LogInteraction("S", "ALKS ON - non critical", "Orange");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            PlayPopSound();
            ActivateCriticalTor();
            LogInteraction("D", "ALKS ON - critical", "Red");
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

        UpdateStatusText("TOR ON - ALKS OFF");
        StopBeepSound();
    }

    void ActivateNonCriticalTor()
    {
        SetLightAndBackground(nonCriticalTorColor);
        isNonCriticalActive = true;
        isCriticalActive = false;
        isBlinking = false;
        torTimer = 0f;

        UpdateStatusText("ALKS ON - non critical");
        StopBeepSound();
    }

    void ActivateCriticalTor()
    {
        SetLightAndBackground(criticalTorColor);
        isNonCriticalActive = false;
        isCriticalActive = true;
        isBlinking = false;
        torTimer = 0f;

        UpdateStatusText("ALKS ON - critical");
        StartBeepSound();
    }

    void ActivateDarkGrey()
    {
        SetLightAndBackground(darkGreyColor);
        UpdateStatusText("System Shut Down");
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
                LogInteraction("0", "ALKS ON - critical", "Red");
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
                LogInteraction("0", "System Shut Down", "Black");
            }
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
        while (isCriticalActive)
        {
            if (beepAudioSource != null) beepAudioSource.Play();
            float interval = torTimer >= 7f ? 0.5f : 1f;
            yield return new WaitForSeconds(interval);
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
