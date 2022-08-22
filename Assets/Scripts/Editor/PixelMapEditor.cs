using System.Collections.Generic; //Need for dictionary and list
using UnityEngine;
using UnityEditor;
using System.IO; //Allows access to directory and file functions

public class PixelMapEditor : EditorWindow
{
    #region Editor Window
    //Create menu item to open editor window
    [MenuItem("Rubber Duck/Tool Windows/Pixel Map Window")]
    //Create the window with a type of this class
    public static void ShowWindow()
    {
        GetWindow(typeof(PixelMapEditor));
    }
    #endregion
    #region Variables
    //Attach a GUISkin to allow for customizing look of editor fields
    [SerializeField] GUISkin skin;
    //Name to give to parent object for created scene
    string _sceneName;
    //Variable to store the pixel map we will use
    Texture2D _mapImage;
    //Create an oldMap to store the previous set map so we can determine when it has changed
    Texture2D _oldMap;
    //Serializeable struct containing object to spawn and associated colour to determine where on the map it will spawn.
    [System.Serializable]
    struct Objects
    {
        public GameObject spawnObj;
        public Color spawnColour;
    }
    //Array from struct so we can edit in Unity Editor
    [SerializeField] Objects[] _objectsToSpawn;
    //Create a serialized object
    SerializedObject so;
    //Colour variable to store the colour at the current position in the map
    private Color _pixelColour;
    //Dictionary to hold the Prefabs we will put in _objectsToSpawn array
    Dictionary<string, GameObject> _prefabs;
    //Parent object we will parent the generated gameobjects to
    GameObject _parentObject;
    //Path to folder containing Prefabs
    string path = "Assets/Resources/Prefabs/";
    #endregion
    #region Editor Fields
    private void OnEnable()
    {
        //Create a ScriptableObject reference to target this script and turn it into a SerializedObject so we can display the Array in the custom editor
        so = new SerializedObject(this);
        //Clear the scene name on load in case it holds a previous value.
        _sceneName = string.Empty;
        //Fill the dictionary with the GameObject prefabs we will use to fill the scene if they exist, or debug log lack of objects
        if (File.Exists(path + "Red.prefab"))
        {
            _prefabs = new Dictionary<string, GameObject>()
            {
                {"Red", Resources.Load("Prefabs/Red") as GameObject},
                {"Blue", Resources.Load("Prefabs/Blue") as GameObject},
                {"Green", Resources.Load("Prefabs/Green") as GameObject},
                {"White", Resources.Load("Prefabs/White") as GameObject},
                {"Black", Resources.Load("Prefabs/Black") as GameObject}
            };
        }
        else
        {
            Debug.Log("No Prefabs to load. Please add Red, Blue, Green, White & Black Prefabs to the Resources/Prefabs/ folder");
        }
        //If we don't have a folder at the path set above, create one.
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
    private void OnGUI()
    {
        //Set the minimum size of the window so content fits appropriately
        minSize = new Vector2(275f, 393f);
        //Set the skin to use the custom skin
        GUI.skin = skin;
        //Create label for Map section
        GUILayout.Label("<color=aqua><b>Mapping Options</b></color>");
        //Name of the scene to create
        _sceneName = EditorGUILayout.TextField(new GUIContent("Scene Name", "Create a unique name for your created level"), _sceneName);
        //Return skin to default to stop issues with Texture2D and array fields
        GUI.skin = null;
        //ObjectField to select the pixel map to use
        _mapImage = (Texture2D) EditorGUILayout.ObjectField(new GUIContent("Pixel Map", "Select the map to use for level creation"),_mapImage, typeof(Texture2D), false, GUILayout.Height(16f));
        //If mapImage and oldMap do not match we have changed, set them to equal each other and fill the array with information from new pixel map
        if (_mapImage != null & _mapImage != _oldMap)
        {
            _oldMap = _mapImage;
            FillArray();
        }
        //Display the SerializedObject as a PropertyField so we can view and edit the array of GameObjects and Colors. Array will auto-fill when selecting a new map
        so.Update();
        EditorGUILayout.PropertyField(so.FindProperty("_objectsToSpawn"),GUIContent.none,true); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties
        //Reset the skin to our custom skin
        GUI.skin = skin;
        //Separate label for the array as I couoldn't modify the look of the attached label
        GUI.Label(new Rect(20f, 65f, 150f, 20f), new GUIContent("<color=aqua><b>Objects To Spawn</b></color>", "Array should auto-fill when map is selected. Wrong object may be loaded in places where a prefab doesn't exist in dictionary for that colour"));
        //If we have a name set we can show the buttons
        if (_sceneName != string.Empty)
        {
            //Change background colour to green for the create button
            GUI.backgroundColor = Color.green;
            //Horizontal layout group with flexible space before and after buttons to position them correctly
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //If we have a map and the array is filled we can show the Spawn Scene button
            if (_mapImage != null && _objectsToSpawn[0].spawnObj != null && GUILayout.Button("Spawn Scene", GUILayout.Width(position.width / 2.1f)))
            {
                //If we have a Prefab matching the set name, clear the set name and log an error
                if (File.Exists(path + _sceneName + ".prefab"))
                {
                    _sceneName = string.Empty;
                    Debug.LogError("Please choose a different name or delete the existing prefab of this name");
                }
                //Else generate the level
                else
                {
                    GenerateLevel();
                }                
            }
            //Change background colour to red for the delete button
            GUI.backgroundColor = Color.red;
            //Delete button to delete the object matching the set name from the scene and clear the set name
            if (GUILayout.Button("Delete Scene", GUILayout.Width(position.width / 2.1f)))
            {
                DestroyImmediate(GameObject.Find(_sceneName));
                _sceneName = string.Empty;
            }
            //Flexible space only shows if Spawn button is active to avoid it moving delete button when it is not
            if (_mapImage != null && _objectsToSpawn[0].spawnObj != null)
            {
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        //If we dont have a name set prompt user to set name
        else
        {
            GUI.backgroundColor = Color.clear;
            GUILayout.TextArea("<color=red><b>Please enter a name for the scene you want to create or delete</b></color>");
        }
        //If we have a map selected display the selected map at just below the minimum width of the window
        if (_mapImage != null)
        {
            //Using custom skin style to set the map as a background so it scales with the box
            skin.customStyles[3].normal.background = _mapImage;
            //Change background color back to white so it doesn't change the colours of the map
            GUI.backgroundColor = Color.white;
            //Using horizontal layout group and flexible space to position image in centre
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("", "custombutton", GUILayout.Width(265), GUILayout.Height(265));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endregion
    #region Spawning
    //Function to spawn the objects with an x and y position passed to it
    void GenerateObject(int x, int y)
    {
        //Read pixel colour
        _pixelColour = _mapImage.GetPixel(x, y);
        if (_pixelColour.a == 0)
        {
            //There is no colour, do nothing
            return;
        }
        //for each entry in the mappedElement array
        foreach (Objects colourMapping in _objectsToSpawn)
        {
            //Scan pixel colour mappings for a matching colour
            if (colourMapping.spawnColour.Equals(_pixelColour))
            {
                //Turn the pixel x and y into a Vector2 position
                Vector3 pos = new Vector3(x, 0f, y);
                //Spawn object that matches pixel colour at pixel position
                Instantiate(colourMapping.spawnObj, pos, Quaternion.identity, _parentObject.transform);
            }
        }
    }
    //Level generator function
    void GenerateLevel()
    {        
        //Instantiate the parent object for the scene
        _parentObject = Instantiate(new GameObject(_sceneName));
        //Scan whole texture and get pixel positions
        for (int x = 0; x < _mapImage.width; x++)
        {
            for (int y = 0; y < _mapImage.height; y++)
            {
                //Run the object spawning function passing through the current value of x and y as a position
                GenerateObject(x, y);
            }
        }
        //Destroy GameObject matching scene name as function was creating a clone as the actual parent
        DestroyImmediate(GameObject.Find(_sceneName));
        //Change the name of the clone to match scene name then clear the scene name
        _parentObject.name = _sceneName;
        _sceneName = string.Empty;
        //Create a prefab of the parent object in the Prefabs folder
        PrefabUtility.SaveAsPrefabAsset(_parentObject,path + _parentObject.name + ".prefab");        
    }
    #endregion
    #region Array Auto-Fill
    //Function to auto-fill the array of colors and GameObjects
    void FillArray()
    {
        //List to fill with different colours we find in the map
        List<Color> colors = new List<Color>();        
        //Search each pixel of the map
        for (int x = 0; x < _mapImage.width; x++)
        {
            for (int y = 0; y < _mapImage.height; y++)
            {
                //Bool to determine if we have found a match previously defaulted to false at start of each iteration
                bool foundColour = false;
                //Read pixel colour at current pixel location
                _pixelColour = _mapImage.GetPixel(x, y);
                //If the pixel is transparent we have no colour, continue to next iteration
                if (_pixelColour.a == 0)
                {
                    continue;
                }
                //If list is currently empty we can just add the first found colour
                if (colors.Count == 0)
                {
                    colors.Add(_pixelColour);
                }
                //Else search the list to see if we have store the current pixel colour already
                else
                {
                    foreach (Color color in colors)
                    {
                        //If we have found a match change the bool to true to reflect we have match
                        if (color == _pixelColour)
                        {
                            foundColour = true;
                        }                        
                    }
                    //If did not find colour in list already add it to list
                    if (!foundColour)
                    {
                        colors.Add(_pixelColour);
                    }
                }
            }
        }
        //Create a new _objectsToSpawn array at the size of our created list
        _objectsToSpawn = new Objects[colors.Count];
        //For each entry in our newly created array
        for (int i = 0; i < _objectsToSpawn.Length; i++)
        {
            //Set the color to the color from the matching index of the list
            _objectsToSpawn[i].spawnColour = colors[i];
            //If colour is white add the white prefab to the array
            if (colors[i].r == 1 && colors[i].g == 1)
            {
                _objectsToSpawn[i].spawnObj = _prefabs["White"];
            }
            //If colour is red add the red prefab to the array
            else if (colors[i].r == 1)
            {
                _objectsToSpawn[i].spawnObj = _prefabs["Red"];
            }
            //If colour is green add the green prefab to the array
            else if (colors[i].g == 1)
            {
                _objectsToSpawn[i].spawnObj = _prefabs["Green"];
            }
            //If colour is blue add the blue prefab to the array
            else if (colors[i].b == 1)
            {
                _objectsToSpawn[i].spawnObj = _prefabs["Blue"];
            }
            //Else object is black, add the black object to the array
            else
            {
                _objectsToSpawn[i].spawnObj = _prefabs["Black"];
            }
        }
    }
    #endregion
}
