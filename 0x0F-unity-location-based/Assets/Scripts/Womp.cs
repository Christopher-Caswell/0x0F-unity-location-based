using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class Womp : MonoBehaviour
{

// button text used to display the current refreshing location.
public GameObject AltText;
public GameObject LatText;
public GameObject LonText;
public GameObject VariableText;

// used to capture the current static location
// captured location is used to calculate the distance between the current location and the captured location
public GameObject RecAltText;
public GameObject RecLatText;
public GameObject RecLonText;
public GameObject UnityPositionText;

// Padoru prefab
public GameObject Padoru;
public GameObject camera;
public GameObject PadoruText;
public GameObject PadoruBubble;
UnityEngine.TouchScreenKeyboard keyboard;
public static string PadoruTextInputField = "Padoru!";

// QoL buttons to make user interaction more obvious
// calculates distance, and resets the captured location
public Button AltButton;
public Button LatButton;
public Button LonButton;
public Button RevealButton;
public Button ClaculateDistanceButton;

// location registered information
// the building blocks for the whole script
private float latitude;
private float longitude;
private float altitude;
private float RecLatSaved;
private float RecLonSaved;
private float RecAltSaved;
private double distance;

/// <summary>
/// Start is called on the frame when a script is enabled just before
/// any of the Update methods is called the first time.
/// </summary>
private void Start()
{
    #if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission (Permission.FineLocation))
        {
            Permission.RequestUserPermission (Permission.FineLocation);
        }
    #endif
    StartCoroutine(StartLocationService());
}

/// <summary>
/// So, I'll tell you what I want what I really really want.
/// I want to get the current location of the user.
/// I want to get the current location of the user.
/// I want a ziggy ziggy ziggy wanna once per frame.
/// </summary>
private void Update()
{
    if (Input.location.status == LocationServiceStatus.Running)
    {
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        altitude = Input.location.lastData.altitude;
    }

    AltText.GetComponent<Text>().text = altitude.ToString();
    LatText.GetComponent<Text>().text = latitude.ToString();
    LonText.GetComponent<Text>().text = longitude.ToString();
    OnGUI();
    UnityPositionText.GetComponent<Text>().text = "Unity Position: (" + GPSEncoder.GPSToUCS(latitude, longitude).ToString() + ")";
}

/// <summary>
/// Outputs error code to the variable text field,
/// given the location services don't find a location.
/// </summary>
private void OnGUI()
{
    if (Input.location.status == LocationServiceStatus.Failed)
    {
        VariableText.GetComponent<Text>().text = "Unable to determine device location";
    }
}

/// <summary>
/// utilize the three
/// text UI elements to record a location stamp
/// </summary>

public void RecordLocation()
{
    RecAltText.GetComponent<Text>().text = altitude.ToString();
    RecLatText.GetComponent<Text>().text = latitude.ToString();
    RecLonText.GetComponent<Text>().text = longitude.ToString();
    RecLatSaved = latitude;
    RecLonSaved = longitude;
    RecAltSaved = altitude;
}

/// <summary>
/// simple button to reveal the RecordLocation function
/// QoL button
/// </summary>
public void Reveal()
{
    RecordLocation();
    RevealButton.gameObject.SetActive(false);
    AltButton.gameObject.SetActive(true);
    LatButton.gameObject.SetActive(true);
    LonButton.gameObject.SetActive(true);
    ClaculateDistanceButton.gameObject.SetActive(true);
}

/// <summary>
/// change the text inside Padoru's bubble
/// to whatever the user says
/// or remove the bubble if the user didn't write anything
/// </summary>
public void PadoruPrecurse()
{

    keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "Padoru!");
    PadoruTextInputField = keyboard.text;
    if (PadoruTextInputField == "")
    {
        PadoruBubble.SetActive(false);
        instantiatePadoru();
    }
    else
    {
        PadoruBubble.SetActive(true);
        Padoru.GetComponent<Text>().text = PadoruTextInputField;
        instantiatePadoru();
    }
}


/// <summary>
/// instantiates a mesh at the camera's location
/// cheekily, it uses the location of the camera
/// and reports the location in unity space
/// </summary>
public void instantiatePadoru()
{
    GameObject newObject = Instantiate(Padoru, new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z), Quaternion.identity);
    Debug.Log(newObject.transform.position);
    newObject.transform.localScale = new Vector3(.3f, .3f, .3f);
    newObject.transform.Rotate(0,0,50*Time.deltaTime);
    VariableText.GetComponent<Text>().text = ("Created at: " + GPSEncoder.GPSToUCS(latitude, longitude).ToString()); // the cheek. Tells the user the mesh in Unity space
    newObject.transform.parent = GameObject.Find("Canvas").transform; // Make the new object a child of the Canvas for UI data ease
    Debug.Log("Padoru instantiated");
}

/// <summary>
/// grab the information recorded
/// in RecordLocation and calculate the distance between
/// the current location and the recorded location.
/// </summary>
/// <returns>The distance between here and RecordLocation in real space.</returns>
public void ReturnDistance()
{
    var R = 6378.137; // Radius of earth in KM
    var dLat = RecLatSaved * Mathf.PI / 180 - latitude * Mathf.PI / 180;
    var dLon = RecLonSaved * Mathf.PI / 180 - longitude * Mathf.PI / 180;
    float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
      Mathf.Cos(latitude * Mathf.PI / 180) * Mathf.Cos(RecLatSaved * Mathf.PI / 180) *
      Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
    var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
    distance = R * c;
    distance = distance * 1000f; // meters
    //set the distance text on the canvas
    VariableText.GetComponent<Text>().text = "Distance: " + (((int)(distance * 100))*0.01) + " meters";
}

/// <summary>
/// I picked a bad day to quit hohos and smoking
/// nevermind, I'm no quitter
/// </summary>
public void Quit()
{
    Application.Quit();
}


/// <summary>
/// Start the location service,
/// and set the location update interval to 1 second.
/// </summary>
private IEnumerator StartLocationService()
{
    if (!Input.location.isEnabledByUser)
    {
        VariableText.GetComponent<Text>().text = "User has not enabled location";
        yield break;
    }
    Input.location.Start(5f, 5f);
    while(Input.location.status == LocationServiceStatus.Initializing)
    {
        VariableText.GetComponent<Text>().text = "Determining device location";
        yield return new WaitForSeconds(1);
    }
    if (Input.location.status == LocationServiceStatus.Failed)
    {
        VariableText.GetComponent<Text>().text = "Unable to determine device location";
        yield break;
    }
}
}
