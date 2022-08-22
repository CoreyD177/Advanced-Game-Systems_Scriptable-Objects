using System.Collections;
using System.Collections.Generic;
using UnityEditor; //Alllows us to access elements of the editor code
using UnityEngine;

//https://docs.unity3d.com/ScriptReference/EditorWindow.html
//Create a script that derives from EditorWindow
public class TestEditorWindow : EditorWindow
{
    #region Editor Window
    //This is the menu, sub menu and window names
    [MenuItem("Rubber Duck/Tool Windows/Test Window")]
    //This function is what creates the window
    public static void ShowWindow()
    {
        //GetWindow comes from EditorWindow
        GetWindow(typeof(TestEditorWindow));
    }
    #endregion
    #region Variables
    //Attach a GUISkin to allow for customizing look of editor fields
    [SerializeField] GUISkin skin;
    //String array to contain the different parenting options
    string[] _positionOptions = new string[] { "Unparented", "Parented: At Parent's Location", "Parented: At Set Position" };
    //Int variable to store the current selection for parenting options
    int _positionIndex = 0;
    //String for the object's name
    string _objectBaseName = "";
    //String for the object's tag
    string _objectTag = "";
    //Game Object to spawn into the scene
    GameObject _objectToSpawn;
    //Create a GameObject variable to spawn, delete and modify the object
    GameObject _newObject;
    //Game Object in heirarchy to use as the parent object
    GameObject _parentObject;
    //List to store all spawned objects as they get spawned
    List<GameObject> _spawnedObjects = new List<GameObject>();
    //Float value to multiply by one to set size of object
    float _objectScale = 1f;
    //Vector3 position to use as spawning location defaulted to Vector3.zero
    Vector3 _objectPosition = Vector3.zero;
    //Int variable for the amount of objects to spawn
    int _spawnAmount;
    //Float variable for the radius to spawn them in
    float _spawnRadius;
    #endregion
    #region Editor Fields
    private void OnGUI()
    {
        //Apply the attached skin to the GUI
        GUI.skin = skin;
        //The display of the actual WINDOW!
        //Label acts like a [Header]
        GUILayout.Label("<color=#00ffffff><b>Object Settings</b></color>");
        //Create a field in the window to select the prefab to spawn
        //Change check allows us to set the value of the tag without blocking it from being editable in the window
        EditorGUI.BeginChangeCheck();
        _objectToSpawn = EditorGUILayout.ObjectField(new GUIContent("Prefab To Spawn", "Select the prefab you wish to spawn into the scene"), _objectToSpawn, typeof(GameObject), false) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            _objectTag = _objectToSpawn.tag;
        }
        //If we have selected a prefab show the following options
        if (_objectToSpawn != null)
        {
            //Create a text field to set the objects name
            _objectBaseName = EditorGUILayout.TextField(new GUIContent("Object Name", "Name the object to be spawned. Single objects will require unique names."), _objectBaseName);
            //Create a tag field to select the objects tag
            _objectTag = EditorGUILayout.TagField(new GUIContent("Object Tag", "Select the tag for the game object. Will be the same as the Prefabs tag by default."), _objectTag);
            //Create Horizontal layout group so we can have a reset button alongside the editor field
            GUILayout.BeginHorizontal();
            //Create a Slider to allow the user to set the scale of the object. GUIContent can be used to add tooltips.
            _objectScale = EditorGUILayout.Slider(new GUIContent("Object Scale", "Adjust the scale to determine how big the object will be"), _objectScale, 0.25f, 10f);
            //Reset button resets Scale value to default of 1f
            if (GUILayout.Button("Reset", GUILayout.Width(50f)))
            {
                _objectScale = 1f;
            }
            //End horizontal group
            GUILayout.EndHorizontal();
            //Create label for next section. Skin allows us to use rich text markup tags to modify text style
            GUILayout.Label("<color=#00ffffff><b>Positioning</b></color>");
            //Int slider for the amount of objects to spawn with following reset button defaulted to 1
            GUILayout.BeginHorizontal();
            _spawnAmount = EditorGUILayout.IntSlider(new GUIContent("Amount To Spawn", "Choose how many objects to spawn"), _spawnAmount,1,10);
            if (GUILayout.Button("Reset", GUILayout.Width(50f)))
            {
                _spawnAmount = 1;
            }
            GUILayout.EndHorizontal();
            //If we have more than 1 object to spawn, create a slider to select the radius to spawn them within
            if (_spawnAmount > 1)
            {
                //Radius for multiple objects to spawn within with accompanying reset button defaulted to 2
                GUILayout.BeginHorizontal();
                _spawnRadius = EditorGUILayout.Slider(new GUIContent("Spawn Radius", "Set the size of the area you want objects to spawn within"), _spawnRadius, 2f, 100f);
                if (GUILayout.Button("Reset", GUILayout.Width(50f)))
                {
                    _spawnRadius = 2f;
                }
                GUILayout.EndHorizontal();
            }
            //Create a Vector3 field so you can set the position to spawn the object at with accompanying reset button defaulted to Vector3.zero
            GUILayout.BeginHorizontal();
            _objectPosition = EditorGUILayout.Vector3Field(new GUIContent("Position", "Set the position you want to spawn the object to. For multiple objects this will act as the centre of the radius."), _objectPosition);
            if (GUILayout.Button("Reset", GUILayout.Width(50f)))
            {
                _objectPosition = Vector3.zero;
            }
            GUILayout.EndHorizontal();
            //Creat label for next section of fields
            GUILayout.Label(new GUIContent("<color=#00ffffff><b>Parenting Options</b></color>", "Choose whether to have a parent object for the spawned object, and whether you want it to spawn at the parents location or at a set location."));
            //Create a popup menu to select the parenting options
            _positionIndex = EditorGUILayout.Popup(_positionIndex, _positionOptions);
            //If we are creating a parented object, create the field for us to add the object from the heirarchy to parent the objects to
            if (_positionIndex != 0)
            {
                _parentObject = EditorGUILayout.ObjectField(new GUIContent("Parent Object", "Select the object in the scene to use as a parent object"), _parentObject, typeof(GameObject), true) as GameObject;
            }
            //Change colour of background to green for Spawn button
            GUI.backgroundColor = Color.green;
            //Create a button to run the spawning function
            if (GUILayout.Button("Spawn Object"))
            {
                //run spawn code
                SpawnObject();
            }
            //Reset background colour to white
            GUI.backgroundColor = Color.white;
            //Create a blank slider to use as a separation line
            GUILayout.Label("", GUI.skin.horizontalSlider, GUILayout.Height(15f));
            //Create label for delete section
            GUILayout.Label("<color=#00ffffff><b>Delete Objects</b></color>");
            //Change background colour to red for delete buttons
            GUI.backgroundColor = Color.red;
            //Create buttons to Delete Last and Delete All and tie them to their corresponding functions
            GUILayout.BeginHorizontal();
            //FlexibleSpace takes up all available space beside an object forcing it to stay on the other side
            GUILayout.FlexibleSpace();
            //If we have an object we have just spawned, Delete last button will show
            if (_newObject != null)
            {
                if (GUILayout.Button("Delete Last Object", GUILayout.Width(position.width / 2.05f)))
                {
                    DeleteLastObject();
                }
            }
            //If we have a list of spawned objects, delete all button will show.
            if (_spawnedObjects.Count > 0)
            {
                if (GUILayout.Button("Delete All Objects", GUILayout.Width(position.width / 2.05f)))
                {
                    DeleteAllObjects();
                }
            }
            GUILayout.EndHorizontal();
        }        
    }
    #endregion
    #region Object Spawning
    //Function to spawn the selected objects
    private void SpawnObject()
    {
        //If we haven't set a name in the object or the name is duplicated and we are spawning a single object log an error and exit function
        if (_objectBaseName == string.Empty || GameObject.Find(_objectBaseName) && _spawnAmount == 1)
        {
            Debug.LogError("Error: Please enter a name for the object");
            return;
        }
        //If spawn amount is greater than 1, create a for loop to instantiate the correct amount of objects
        if (_spawnAmount > 1)
        {
            //Create an int we can use to check for matches and adjust the for loop if necessary
            int indexCheck = 0;
            //Check if we have a numbered object match at the current index and increment until we have a unique name
            if (GameObject.Find(_objectBaseName + indexCheck))
            {
                do { indexCheck++; } while (GameObject.Find(_objectBaseName + indexCheck));
            }
            //Once we have a unique name, add the indexCheck value to the for loop so we start and end at the correct index
            for (int i = 0 + indexCheck; i < _spawnAmount + indexCheck; i++)
            {
                //If we are spawning an unparented object, spawn it to the position set by the _objectPosition field
                if (_positionIndex == 0)
                {
                    //Instantiate the object
                    _newObject = Instantiate(_objectToSpawn, new Vector3(Random.Range(_objectPosition.x - _spawnRadius, _objectPosition.x + _spawnRadius), Random.Range(_objectPosition.y - _spawnRadius, _objectPosition.y + _spawnRadius), Random.Range(_objectPosition.z - _spawnRadius, _objectPosition.z + _spawnRadius)), Quaternion.identity);
                }
                //Else if we are spawning to a position and then using the _parentObject.transform to parent it to that object.
                else if (_positionIndex == 2)
                {
                    //Instantiate the object
                    _newObject = Instantiate(_objectToSpawn, new Vector3(Random.Range(_objectPosition.x - _spawnRadius, _objectPosition.x + _spawnRadius), Random.Range(_objectPosition.y - _spawnRadius, _objectPosition.y + _spawnRadius), Random.Range(_objectPosition.z - _spawnRadius, _objectPosition.z + _spawnRadius)), Quaternion.identity, _parentObject.transform);
                }
                //Else, log an error because we don't want to spawn multiple objects in the same place
                else
                {
                    Debug.LogError("Can't spawn multiple objects in same area");
                    return;
                }
                //Set the scale of the instantiated object based off scale from window
                _newObject.transform.localScale = Vector3.one * _objectScale;
                //Set the name of the object based on the name set in the window
                _newObject.name = _objectBaseName + i;
                //Set the tag based on the tag set in the window
                _newObject.tag = _objectTag;
                //Add object to list of spawned objects
                _spawnedObjects.Add(_newObject);
            }
        }
        //Else we are only spawning one object
        else
        {
            //If we are spawning an unparented object, spawn it to the position set by the _objectPosition field
            if (_positionIndex == 0)
            {
                //Instantiate the object
                _newObject = Instantiate(_objectToSpawn, _objectPosition, Quaternion.identity);
            }
            //Else if we are spawning to the parent's location, use the _parentObject's transform
            else if (_positionIndex == 1)
            {
                //Instantiate the object
                _newObject = Instantiate(_objectToSpawn, _parentObject.transform);
            }
            //Else we are spawning to a position and then using the _parentObject.transform to parent it to that object.
            else
            {
                //Instantiate the object
                _newObject = Instantiate(_objectToSpawn, _objectPosition, Quaternion.identity, _parentObject.transform);
            }
            //Set the scale of the instantiated object based off scale from window
            _newObject.transform.localScale = Vector3.one * _objectScale;
            //Set the name of the object based on the name set in the window
            _newObject.name = _objectBaseName;
            //Set the tag based on the tag set in the window
            _newObject.tag = _objectTag;
            //Add object to list of spawned objects
            _spawnedObjects.Add(_newObject);
        }        
    }
    #endregion
    #region Delete Objects
    //Create a function to delete the last object created and remove it from the list of spawned objects
    private void DeleteLastObject()
    {
        _spawnedObjects.Remove(_newObject);
        //DestroyImmediate allows us to destroy objects outside of playmode
        DestroyImmediate(_newObject);
    }
    //Create a function to destroy all objects on the spawned objects list and clear the list.
    private void DeleteAllObjects()
    {
        foreach (GameObject obj in _spawnedObjects)
        {
            DestroyImmediate(obj);
        }
        _spawnedObjects.Clear();
    }
    #endregion
}
