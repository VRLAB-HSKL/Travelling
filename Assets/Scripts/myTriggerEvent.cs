using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse myTriggerEvent wird dazu genutzt, zu erkennen,
/// ob eine Person einen Trigger berührt hat.
/// </summary>
public class myTriggerEvent : MonoBehaviour
{
    /// <summary>
    /// Diese Methode wird immer dann aufgerufen, wenn ein Objekt mit dem festgelegten Collider (Collider als isTrigger) kollidiert.
    /// </summary>
    /// <param name="collider"> Das Objekt, das mit dem Collider in Berührung kommt.</param>
    private void OnTriggerEnter(Collider collider)
    {
        //Ruft die Methode "MyTriggerEnter" im Skript "generateCourse" mit dem aktuellen collider auf.
        GameObject.FindGameObjectWithTag("GameController").GetComponent<generateCourse>().myTriggerEnter(collider, this.gameObject.name);
    }
}