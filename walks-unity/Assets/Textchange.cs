using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Textchange : MonoBehaviour
{
    public TMP_Text alksStatusText; // this refers to the TMP text component

    void Start()
    {
        if (alksStatusText == null)
        {
            Debug.LogError("TMP_Text component is not assigned.");
        }
        else
        {
            // set an initial status
            alksStatusText.text = "TOR ON - ALKS OFF";
        }
    }

    void Update()
    {
        if (alksStatusText == null) return; // closes  if the TMP_Text is not assigned

        // check for key presses and changes text
        if (Input.GetKeyDown(KeyCode.A))
        {
            alksStatusText.text = "ALKS OFF";
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            alksStatusText.text = "ALKS ON - non critical";
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            alksStatusText.text = "ALKS ON - critical";
        }
    }
}


