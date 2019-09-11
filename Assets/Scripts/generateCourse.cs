using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;

/// <summary>
/// Die Klasse generateCourse wird dazu genutzt, den Kurs mit den gewählten Eingaben zu erzeugen.
/// </summary>
public class generateCourse : MonoBehaviour
{
    public GameObject controls;
    public Slider sliderElementsCount;
    public Dropdown dropdownChooseRoomType;
    public Dropdown dropdownChooseMovementType;
    public GameObject northWall;
    public GameObject eastWall;
    public GameObject sectionContainer;
    public GameObject searchingPictureFrame;
    public GameObject searchingObjectContainer;

    public GameObject prefabCorner;
    public GameObject prefabEmpty;
    public GameObject prefabPicture;
    public GameObject prefabObject;
    public GameObject prefabEnd;

    public List<GameObject> prefabObjectList = new List<GameObject>();
    public List<GameObject> tempPrefabObjectList = new List<GameObject>();
    public List<GameObject> usedObjectList = new List<GameObject>();
    public List<Texture2D> prefabImageList = new List<Texture2D>();
    public List<Texture2D> tempPrefabImageList = new List<Texture2D>();
    public List<GameObject> usedImageList = new List<GameObject>();

    private GameObject[] prefabs;
    private int sectionCounter;
    private int cornerCounter;
    private int randomCounter;
    private int usedPictureCounter;
    private int usedObjectCounter;
    private BoxCollider usedBoxCollider;

    //Randomzahlen für das Bild das gesucht werden muss
    private int searchingPicture;
    private int searchingObject;

    private int xValue;
    private int zValue;

    private bool openNorth;
    private bool openEast;
    private bool closeNorth;
    private bool closeEast;
    private bool turned;
    private bool generateAtStart;

    private string lastPicture;
    private string lastObject;

    const string configFileName = "config.xml";
    const string timeSaveFilename = "times.csv";
    private myTimer timer;

    // Wird zur Initialisierung genutzt.
    void Start()
    {
        timer = FindObjectOfType<myTimer>();
        prefabs = new GameObject[] { prefabEmpty, prefabPicture, prefabObject, prefabCorner, prefabEnd };

        //Hier werden die prefabObjekte in eine Liste geladen, um sie später dynamisch in die Szene einzubauen.
        GameObject[] prefabObjects = Resources.LoadAll<GameObject>("prefabObjects");
        foreach (GameObject go in prefabObjects) prefabObjectList.Add(go);

        //Hier werden die Bilder in eine Liste geladen, um sie später dynamisch in die Szene einzubauen.
        Texture2D[] prefabImages = Resources.LoadAll<Texture2D>("prefabImages");
        foreach (Texture2D img in prefabImages) prefabImageList.Add(img);

        usedBoxCollider = new BoxCollider();

        sectionCounter = 0;
        cornerCounter = 0;
        randomCounter = 0;
        usedPictureCounter = 0;
        usedObjectCounter = 0;
        xValue = 3;
        zValue = 0;
        generateAtStart = false;
        openNorth = false;
        openEast = false;
        lastObject = "";
        lastPicture = "";

        resetPictureList();
        resetObjectList();
        loadConfig();
        if (generateAtStart) generate();
    }

    /// <summary>
    /// Methode zum Laden der Konfiguration zum Start der Anwendung aus einer XML Datei.
    /// Diese Datei muss im Projektordner "Resources" vorhanden sein.
    /// </summary>
    private void loadConfig()
    {
        XmlDocument xmlDoc = new XmlDocument();
        if (System.IO.File.Exists(getPath("xml")))
        {
            xmlDoc.LoadXml(System.IO.File.ReadAllText(getPath("xml")));
        }
        else
        {
            TextAsset textXml = (TextAsset)Resources.Load(configFileName, typeof(TextAsset));
            xmlDoc.LoadXml(textXml.text);
        }

        foreach (XmlElement node in xmlDoc.SelectNodes("ConfigCollection/Configs/Config"))
        {
            sliderElementsCount.value = int.Parse(node.SelectSingleNode("elementsCount").InnerText);
            dropdownChooseRoomType.value = int.Parse(node.SelectSingleNode("roomType").InnerText);
            dropdownChooseMovementType.value = int.Parse(node.SelectSingleNode("movementType").InnerText);
            generateAtStart = bool.Parse(node.SelectSingleNode("generateAtStart").InnerText);
        }
    }

    /// <summary>
    /// Methode zum Speichern der Zeit, sobald der Spieler das nötige Ziel erreicht hat.
    /// Die Daten werden in einer CSV-Datei gespeichert und im Projektordner "Resources" abgelegt.
    /// Wenn die Datei bereits vorhanden ist, werden die darin enthaltenen Daten übernommen.
    /// </summary>
    private void saveCourseTime()
    {
        bool fileNotFound = false;
        string line = "";
        try
        {
            StreamReader reader = new StreamReader(getPath("csv"));
            line = reader.ReadToEnd();
            reader.Close();
        }
        catch (FileNotFoundException)
        {
            fileNotFound = true;
        }

        try
        {
            StreamWriter writer = new StreamWriter(getPath("csv"));
            if (fileNotFound) writer.WriteLine("ElementsCount,RoomType,MovementType,NeededTime");
            else if (!fileNotFound) writer.Write(line);
            writer.WriteLine(sliderElementsCount.value.ToString() + "," + dropdownChooseRoomType.value + "," + dropdownChooseMovementType.value + "," + timer.getTime());
            writer.Flush();
            writer.Close();
        }
        catch (IOException ex)
        {
            Debug.Log(ex.StackTrace);
        }
    }

    /// <summary>
    /// Methode zum Zurückgeben des korrekten Pfades.
    /// Dabei wird der Pfad der XML- und der CSV-Datei unterschieden.
    /// </summary>
    /// <returns>Den Pfad je nach dem ob der Unity Editor aktiviert ist oder nicht.
    /// Gibt Null zurück wenn ein falscher Wert angegeben wurde.
    /// Korrekte Werte sind: "xml" und "csv".</returns>
    private string getPath(string path)
    {
        if (path.Equals("xml"))
        {
#if UNITY_EDITOR
            return Application.dataPath + "/Resources/" + configFileName;
#else
            return Application.dataPath + "/" + configFileName;
#endif
        }
        else if (path.Equals("csv"))
        {
#if UNITY_EDITOR
            return Application.dataPath + "/Resources/" + timeSaveFilename;
#else
            return Application.dataPath + "/" + timeSaveFilename;
#endif
        }
        return null;
    }

    /// <summary>
    /// Methode zum Auswerten einer Kollision anhand des Collidernamens und dem kollidierenden Objekt.
    /// </summary>
    /// <param name="collider">Das Objekt welches mit dem Collider in Berührung kam.</param>
    /// <param name="boxColliderName">Der Name des Colliders.</param>
    public void myTriggerEnter(Collider collider, string boxColliderName)
    {
        switch (boxColliderName)
        {
            case "cornerTrigger":
                GameObject.Find("Section_Corner_1").transform.GetChild(4).GetComponent<BoxCollider>().enabled = false;
                openEast = true;
                break;
            case "startpointTrigger":
                timer.start();
                GameObject.Find("startpointTrigger").GetComponent<BoxCollider>().enabled = false;
                if (dropdownChooseRoomType.value != 2) GameObject.Find("endpointTrigger").GetComponent<BoxCollider>().enabled = true;
                break;
            case "endpointTrigger":
                timer.stop();
                saveCourseTime();
                GameObject.Find("startpointTrigger").GetComponent<BoxCollider>().enabled = true;
                GameObject.Find("endpointTrigger").GetComponent<BoxCollider>().enabled = false;
                break;
            case "pictureTrigger":
                timer.stop();
                saveCourseTime();
                usedBoxCollider.enabled = false;
                resetSearching();
                break;
            case "objectTrigger":
                timer.stop();
                saveCourseTime();
                Destroy(searchingObjectContainer.transform.GetChild(0).gameObject);
                usedBoxCollider.enabled = false;
                resetSearching();
                break;
        }
    }

    /// <summary>
    /// Methode zum Deaktivieren des Colliders auf einer Ecke, sodass dieser nicht zweimal aktiviert werden kann.
    /// </summary>
    private void triggeredCornerCollider()
    {
        GameObject.Find("Section_Corner_1").transform.GetChild(4).GetComponent<BoxCollider>().enabled = false;
    }

    /// <summary>
    /// Methode zum Wählen des Objektes, das während des Durchganges gesucht werden muss.
    /// Das gewählte Objekt wird kopiert und im Anfangsbereich des Kurses angezeigt.
    /// </summary>
    private void chooseSearching()
    {
        int rand = Random.Range(0, 2);
        bool empty = false;

        if (usedObjectCounter == 0 && usedPictureCounter == 0) empty = true;
        else
        {
            if (rand == 0 && usedPictureCounter == 0) rand = 1;
            if (rand == 1 && usedObjectCounter == 0) rand = 0;
        }
        if (rand == 0 && !empty)
        {
            rand = Random.Range(0, usedPictureCounter);
            searchingPictureFrame.SetActive(true);
            searchingPictureFrame.GetComponentInChildren<RawImage>().texture = usedImageList[rand].GetComponentInChildren<RawImage>().texture;

            foreach (BoxCollider collider in usedImageList[rand].GetComponentsInChildren<BoxCollider>())
            {
                if (collider.name.Equals("pictureTrigger"))
                {
                    usedBoxCollider = collider;
                    usedBoxCollider.enabled = true;
                }
            }
        }
        else if (rand == 1 && !empty)
        {
            rand = Random.Range(0, usedObjectCounter);
            searchingObjectContainer.SetActive(true);

            GameObject tempObj = usedObjectList[rand].transform.Find("objectContainer").GetChild(0).gameObject;
            GameObject clonedObject = Instantiate(tempObj, new Vector3(-1.0f, 0.3f, 0), Quaternion.identity) as GameObject;
            clonedObject.transform.parent = searchingObjectContainer.transform;

            foreach (BoxCollider collider in usedObjectList[rand].GetComponentsInChildren<BoxCollider>())
            {
                if (collider.name.Equals("objectTrigger"))
                {
                    usedBoxCollider = collider;
                    usedBoxCollider.enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Methode zur Erzeugung der verschiedenen Gänge.
    /// </summary>
    public void generate()
    {
        generateAtStart = false;
        int room = dropdownChooseRoomType.value;

        controls.GetComponent<toggleControls>().startIn();

        switch (room)
        {
            case 0:
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    addRandomGameObject();
                    xValue = xValue + 3;
                }
                addEnd();
                openNorth = true;
                break;
            case 1:
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    addRandomGameObject();
                    xValue = xValue + 3;
                }
                addCorner();
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    zValue = zValue - 3;
                    addRandomGameObject();
                }
                addEnd();
                openNorth = true;
                break;
            case 2:
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    addRandomGameObject();
                    xValue = xValue + 3;
                }
                addCorner();
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    zValue = zValue - 3;
                    addRandomGameObject();
                }
                addCorner();
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    xValue = xValue - 3;
                    addRandomGameObject();
                }
                addCorner();
                for (int i = 0; i < sliderElementsCount.value; i++)
                {
                    zValue = zValue + 3;
                    addRandomGameObject();
                }
                openNorth = true;
                break;
        }
        chooseSearching();
    }


    /// <summary>
    /// Methode zum Löschen aller erzeugten Elemente. 
    /// Zusätzlich werden die Kontrollen ausgefahren.
    /// </summary>     
    public void deleteControlsOut()
    {
        zValue = 0;
        xValue = 3;
        destroyObjects();
        controls.GetComponent<toggleControls>().startOut();
    }

    /// <summary>
    /// Methode zum Löschen aller erzeugten Elemente.
    /// </summary>
    public void delete()
    {
        zValue = 0;
        xValue = 3;
        destroyObjects();
    }

    /// <summary>
    /// Methode zur zufälligen Erzeugung von Sections.
    /// </summary>
    private void addRandomGameObject()
    {
        if (randomCounter != 0 && randomCounter % 3 == 0)
        {
            int rand = Random.Range(1, 3);
            addGameObject(rand);
        }
        else
        {
            addGameObject(0);
        }
        randomCounter++;
    }

    /// <summary>
    /// Methode zum Öffnen von beiden Wänden.
    /// </summary>
    private void openBothWalls()
    {
        openNorth = true;
        openEast = true;
    }

    /// <summary>
    /// Methode zum Schließen von beiden Wänden.
    /// </summary>
    private void closeBothWalls()
    {
        closeNorth = true;
        closeEast = true;
    }


    /// <summary>
    /// Methode zum Öffnen der nördlichen Wand.
    /// </summary>    
    private void openNorthWall()
    {
        northWall.transform.position = northWall.transform.position - new Vector3(0, 0.02f, 0);
        if (northWall.transform.position.y < -1.3f)
        {
            northWall.SetActive(false);
            openNorth = false;
        }
    }

    /// <summary>
    /// Methode zum Schließen der nördlichen Wand.
    /// </summary>
    private void closeNorthWall()
    {
        if (northWall.transform.position.y < 1.25f)
        {
            northWall.SetActive(true);
            northWall.transform.position = northWall.transform.position + new Vector3(0, 0.02f, 0);
        }
        if (northWall.transform.position.y >= 1.25f)
        {
            northWall.transform.position = new Vector3(1.5f, 1.25f, 0);
            closeNorth = false;
        }
    }

    /// <summary>
    /// Methode zum Öffnen der östlichen Wand.
    /// </summary>
    private void openEastWall()
    {
        eastWall.transform.position = eastWall.transform.position - new Vector3(0, 0.02f, 0);
        if (eastWall.transform.position.y < -1.3f)
        {
            eastWall.SetActive(false);
            openEast = false;
        }
    }

    /// <summary>
    /// Methode zum Schließen der östlichen Wand.
    /// </summary>
    private void closeEastWall()
    {
        if (eastWall.transform.position.y < 1.25f)
        {
            eastWall.SetActive(true);
            eastWall.transform.position = eastWall.transform.position + new Vector3(0, 0.02f, 0);
        }
        if (eastWall.transform.position.y >= 1.25f)
        {
            eastWall.transform.position = new Vector3(0, 1.25f, -1.5f);
            closeEast = false;
        }
    }

    /// <summary>
    /// Methode zur Erzeugung eines Section Abschnittes.
    /// </summary>
    /// <param name="gameObject">Das zuvor zufällig ausgewählte Objekt. Es kann folgende Werte annehmen: 
    /// 0 = EmptyPrefab, 1 = picturePrefab, 2 = objectPrefab</param>
    private void addGameObject(int gameObject)
    {
        GameObject newSection = Instantiate(prefabs[gameObject]) as GameObject;

        string sectionName = "Section_" + sectionCounter++;

        newSection.name = sectionName;
        if (zValue < 0 && Mathf.Abs(zValue) < xValue || zValue < 0 && xValue == 0)
        {
            newSection.transform.rotation = Quaternion.Euler(0, 90f, 0);
            turned = true;
        }
        newSection.transform.position = new Vector3(xValue, 0, zValue);
        newSection.transform.parent = sectionContainer.transform;

        if (gameObject == 1)
        {
            if (prefabImageList.Count > 0)
            {
                if (tempPrefabImageList.Count == 1)
                {
                    newSection.gameObject.GetComponentInChildren<RawImage>().texture = tempPrefabImageList[0];
                    lastPicture = tempPrefabImageList[0].name;
                    resetPictureList();
                }
                else
                {
                    int rand;
                    do
                    {
                        rand = Random.Range(0, tempPrefabImageList.Count);
                    } while (lastPicture.Equals(tempPrefabImageList[rand].name));
                    lastPicture = "";
                    newSection.gameObject.GetComponentInChildren<RawImage>().texture = tempPrefabImageList[rand];
                    tempPrefabImageList.RemoveAt(rand);
                }
            }
            if (usedImageList.Count() < prefabImageList.Count()) usedImageList.Add(newSection);
            if (usedPictureCounter < prefabImageList.Count()) usedPictureCounter++;
        }

        if (gameObject == 2)
        {
            if (prefabObjectList.Count > 0)
            {
                if (tempPrefabObjectList.Count == 1)
                {
                    if (turned)
                    {
                        GameObject myObject = Instantiate(tempPrefabObjectList[0], new Vector3(xValue - 1f, 0.3f, zValue), Quaternion.identity) as GameObject;
                        myObject.transform.parent = newSection.transform.GetChild(1).gameObject.transform;
                    }
                    else
                    {
                        GameObject myObject = Instantiate(tempPrefabObjectList[0], new Vector3(xValue, 0.3f, zValue - 1f), Quaternion.identity) as GameObject;
                        myObject.transform.parent = newSection.transform.GetChild(1).gameObject.transform;
                    }
                    lastObject = tempPrefabObjectList[0].name;
                    resetObjectList();
                }
                else
                {
                    int rand;
                    do
                    {
                        rand = Random.Range(0, tempPrefabObjectList.Count);
                    } while (lastObject.Equals(tempPrefabObjectList[rand].name));
                    lastObject = "";

                    if (turned)
                    {
                        GameObject myObject = Instantiate(tempPrefabObjectList[rand], new Vector3(xValue - 1f, 0.3f, zValue), Quaternion.identity) as GameObject;
                        myObject.transform.parent = newSection.transform.GetChild(1).gameObject.transform;
                    }
                    else
                    {
                        GameObject myObject = Instantiate(tempPrefabObjectList[rand], new Vector3(xValue, 0.3f, zValue - 1f), Quaternion.identity) as GameObject;
                        myObject.transform.parent = newSection.transform.GetChild(1).gameObject.transform;
                    }

                    tempPrefabObjectList.RemoveAt(rand);
                }
            }
            if (usedObjectList.Count() < prefabObjectList.Count()) usedObjectList.Add(newSection);
            if (usedObjectCounter < prefabObjectList.Count()) usedObjectCounter++;
        }

        if (gameObject != 0 && Random.Range(0, 2) == 1)
        {
            if (newSection.transform.rotation.y > 0) newSection.transform.rotation = Quaternion.Euler(newSection.transform.rotation.x, -90f, newSection.transform.rotation.z);
            else newSection.transform.rotation = Quaternion.Euler(newSection.transform.rotation.x, newSection.transform.rotation.y + 180f, newSection.transform.rotation.z);
        }

        turned = false;
    }

    /// <summary>
    /// Methode zum Hinzufügen des Ende-Abschnittes.
    /// </summary>     
    private void addEnd()
    {
        GameObject newSection = Instantiate(prefabs[4]) as GameObject;

        string sectionName = "Section_End";
        newSection.name = sectionName;
        if (zValue != 0)
        {
            newSection.transform.position = new Vector3(xValue, 0, zValue - 3);
            newSection.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else newSection.transform.position = new Vector3(xValue, 0, zValue);
        newSection.transform.parent = sectionContainer.transform;
    }

    /// <summary>
    /// Methode zum Hinzufügen einer Ecke mit entsprechendem Winkel.
    /// </summary>
    private void addCorner()
    {
        GameObject newSection = Instantiate(prefabs[3]) as GameObject;

        string sectionName = "Section_Corner_" + cornerCounter++;
        if (zValue < 0 && Mathf.Abs(zValue) < xValue)
        {
            zValue = zValue - 3;
            newSection.transform.rotation = Quaternion.Euler(0, 90, 0);
            newSection.transform.GetChild(4).GetComponent<BoxCollider>().enabled = true;
        }
        if (zValue < 0 && xValue == 3)
        {
            xValue = xValue - 3;
            newSection.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        newSection.name = sectionName;
        newSection.transform.position = new Vector3(xValue, 0, zValue);
        newSection.transform.parent = sectionContainer.transform;
    }

    /// <summary>
    /// Methode zum Entfernen aller generierten Objekte.
    /// Zudem wird das zu Suchende Objekt zurückgesetzt.
    /// </summary>
    private void destroyObjects()
    {
        closeBothWalls();
        sectionCounter = 0;
        cornerCounter = 0;
        randomCounter = 0;
        if (sectionContainer.transform.childCount > 0)
        {
            for (int i = 0; i < sectionContainer.transform.childCount; i++)
            {
                Destroy(sectionContainer.transform.GetChild(i).gameObject);
            }
        }
        resetSearching();
    }

    /// <summary>
    /// Methode um die temporäre Bilderliste wieder zu füllen.
    /// </summary>
    private void resetPictureList()
    {
        tempPrefabImageList = new List<Texture2D>();
        foreach (Texture2D img in prefabImageList) tempPrefabImageList.Add(img);
    }

    /// <summary>
    /// Methode um die temporäre Objektliste wieder zu füllen.
    /// </summary>
    private void resetObjectList()
    {
        tempPrefabObjectList = new List<GameObject>();
        foreach (GameObject obj in prefabObjectList) tempPrefabObjectList.Add(obj);
    }

    /// <summary>
    /// Setzt die entsprechenden Werte für das zu Suchende Bild oder Objekt zurück.
    /// </summary>
    private void resetSearching()
    {
        usedImageList = new List<GameObject>();
        usedObjectList = new List<GameObject>();
        searchingPictureFrame.SetActive(false);
        searchingObjectContainer.SetActive(false);
        usedObjectCounter = 0;
        usedPictureCounter = 0;
        usedBoxCollider = new BoxCollider();
        GameObject.Find("startpointTrigger").GetComponent<BoxCollider>().enabled = true;
    }

    // Update wird einmal pro Frame aufgerufen.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && sectionContainer.transform.childCount == 0) generate();
        if (Input.GetKeyDown(KeyCode.C)) deleteControlsOut();
        if (Input.GetKeyDown(KeyCode.D)) delete();
        if (Input.GetKeyDown(KeyCode.T)) timer.toggleTimerText();
        if (Input.GetKeyDown(KeyCode.O)) timer.start();
        if (Input.GetKeyDown(KeyCode.L)) timer.stop();
        if (Input.GetKeyDown(KeyCode.S)) saveCourseTime();

        if (openNorth) openNorthWall();
        if (openEast) openEastWall();

        if (closeNorth) closeNorthWall();
        if (closeEast) closeEastWall();
    }
}