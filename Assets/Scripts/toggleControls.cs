using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse toggleControls wird dazu verwendet, die Kontrollschaltfläche zu aktivieren und zu deaktivieren.
/// </summary>
public class toggleControls : MonoBehaviour
{
    public Canvas canvas;
    public GameObject block;
    public GameObject wall;

    bool open;
    bool close;
    bool closeControls;
    bool openControls;
    bool blockIn;
    bool blockOut;
    bool wallUp;
    bool wallDown;

    // Wird zur Initialisierung genutzt.
    void Start()
    {
        open = false;
        close = false;
        closeControls = false;
        openControls = false;
        blockIn = false;
        blockOut = false;
        wallUp = false;
        wallDown = false;
    }

    /// <summary>
    /// Methode zum Einfahren des Controlpanels in den Secretraum.
    /// </summary>
    private void controlsIn()
    {
        if (block.gameObject.transform.position.z < 1.9f || wall.gameObject.transform.position.y > 1.25f)
        {
            if (closeControls)
            {
                Vector3 vector = new Vector3(0.01f, 0, 0);
                canvas.gameObject.transform.localScale = canvas.gameObject.transform.localScale - vector;

                if (canvas.gameObject.transform.localScale.x < 0)
                {
                    closeControls = false;
                    wallUp = true;
                    canvas.gameObject.transform.localScale = new Vector3(0, 1, 2);
                }
            }

            if (wallUp) wallUpFunc();

            if (blockOut)
            {
                Vector3 vector = new Vector3(0, 0, -0.01f);
                block.gameObject.transform.position = block.gameObject.transform.position - vector;

                if (block.gameObject.transform.position.z > 1.9f)
                {
                    blockOut = false;
                    wallDown = true;
                }
            }

            if (wallDown) wallDownFunc();
        }
    }

    /// <summary>
    /// Methode zum Ausfahren des Controlpanels aus dem Secretraum.
    /// </summary>
    private void controlsOut()
    {
        if (wallUp) wallUpFunc();

        if (blockIn)
        {
            Vector3 vector = new Vector3(0, 0, -0.01f);
            block.gameObject.transform.position = block.gameObject.transform.position + vector;

            if (block.gameObject.transform.position.z < 1.0f)
            {
                blockIn = false;
                wallDown = true;
            }
        }

        if (wallDown) wallDownFunc();

        if (openControls)
        {
            Vector3 vector = new Vector3(0.1f, 0, 0);
            canvas.gameObject.transform.localScale = canvas.gameObject.transform.localScale + vector;

            if (canvas.gameObject.transform.localScale.x > 1f)
            {
                openControls = false;
                open = false;
                canvas.gameObject.transform.localScale = new Vector3(1, 1, 2);
            }
        }
    }

    /// <summary>
    /// Methode zum Starten des Einfahrens in den Secretraum.
    /// </summary>
    public void startIn()
    {
        close = true;
        closeControls = true;
    }

    /// <summary>
    /// Methode zum Starten des Ausfahrens in den Secretraum.
    /// </summary>
    public void startOut()
    {
        open = true;
        wallUp = true;
    }

    /// <summary>
    /// Methode zum Heben der Wand.
    /// </summary>
    private void wallUpFunc()
    {
        wall.gameObject.transform.position = wall.gameObject.transform.position + new Vector3(0, 0.01f, 0);
        if (wall.gameObject.transform.position.y > 2.4f)
        {
            wallUp = false;
            if (block.gameObject.transform.position.z > 1.8f) blockIn = true;
            else blockOut = true;
        }
    }

    /// <summary>
    /// Methode zum Senken der Wand.
    /// </summary>
    private void wallDownFunc()
    {
        wall.gameObject.transform.position = wall.gameObject.transform.position + new Vector3(0, -0.01f, 0);
        if (wall.gameObject.transform.position.y <= 1.25f)
        {
            wall.gameObject.transform.position = new Vector3(0, 1.25f, 1.5f);
            wallDown = false;
            close = false;

            if (block.gameObject.transform.position.z < 1.1f) openControls = true;
        }
    }

    // Update wird einmal pro Frame aufgerufen.
    void Update()
    {
        if (close) controlsIn();
        if (open) controlsOut();
    }
}