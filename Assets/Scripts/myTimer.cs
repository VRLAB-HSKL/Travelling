using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Die Klasse myTimer wird zum abmessen der vergangenen Zeit genutzt.
/// </summary>
public class myTimer : MonoBehaviour
{
    public Text timerText;

    private bool countTime;
    private float startTime;

    // Wird zur Initialisierung genutzt.
    void Start()
    {
        countTime = false;
        timerText.text = "00:00.00";
    }

    /// <summary>
    /// Methode zum Aktivieren und Deaktivieren des Timertextes.
    /// </summary>
    public void toggleTimerText()
    {
        if (timerText.IsActive()) timerText.enabled = false;
        else timerText.enabled = true;
    }

    /// <summary>
    /// Methode zum Setzen der Startzeit und Starten des Timers.
    /// </summary>
    public void start()
    {
        countTime = true;
        startTime = Time.time;
    }

    /// <summary>
    /// Methode zum Stoppen des Timers.
    /// </summary>
    public void stop()
    {
        countTime = false;
    }

    /// <summary>
    /// Methode zur Zurückgabe der vergangenen Zeit.
    /// </summary>
    /// <returns>Die vergangene Zeit als String.</returns>
    public string getTime()
    {
        return timerText.text;
    }

    // Update wird einmal pro Frame aufgerufen.
    void Update()
    {
        if (countTime)
        {
            float tmpTime = Time.time - startTime;
            string minutes = "";
            string seconds = "";
            if (((int)tmpTime / 60) < 10) minutes = "0" + ((int)tmpTime / 60).ToString();
            else minutes = ((int)tmpTime / 60).ToString();
            if ((tmpTime % 60) < 10) seconds = "0" + (tmpTime % 60).ToString("f2");
            else seconds = (tmpTime % 60).ToString("f2");

            timerText.text = minutes + ":" + seconds;
        }
    }
}