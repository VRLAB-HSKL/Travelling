using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Die Klasse updateValue wird genutzt um den elementsCountSlider bei Bewegung den richtigen Wert zuzweisen.
/// </summary>
public class updateValue : MonoBehaviour
{
    Text elementsCountText;

    // Wird zur Initialisierung genutzt.
    void Start()
    {
        elementsCountText = GetComponent<Text>();
    }

    /// <summary>
    /// Methode zum Aktualisieren des Textes.
    /// </summary>
    /// <param name="value"></param>
    public void updateText(float value)
    {
        elementsCountText.text = value.ToString();
    }

    // Update wird einmal pro Frame aufgerufen.
    void Update()
    {

    }
}