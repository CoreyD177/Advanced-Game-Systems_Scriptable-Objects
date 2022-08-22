using UnityEngine;

public class PixelMap : MonoBehaviour
{
    //Variable to store the pixel map we will use
    public Texture2D mapImage;
    //Serializeable struct containing object to spawn and associated colour to determine where on the map it will spawn.
    [System.Serializable]
    public struct Mappings
    {
        public GameObject spawnObj;
        public Color spawnColour;
    }
    //Array from struct so we can edit in Unity Editor
    public Mappings[] mappedElement;
    //Colour variable to store the colour at the current position in the map
    private Color _pixelColour;
    //Function to spawn the objects with an x and y position passed to it
    void GenerateObject(int x, int y)
    {
        //Read pixel colour
        _pixelColour = mapImage.GetPixel(x, y);
        if (_pixelColour.a == 0)
        {
            //There is no colour, do nothing
            Debug.Log("This pixel is empty, skip");
            return;
        }
        //for each entry in the mappedElement array
        foreach (Mappings colourMapping in mappedElement)
        {
            //Debug the colour of the current map pixel compared to the value we are checking from array
            Debug.Log("Check Colour Match: " + _pixelColour + " - " + colourMapping.spawnColour);
            //Scan pixel colour mappings for a matching colour
            if (colourMapping.spawnColour.Equals(_pixelColour))
            {
                //Debug that we have a match
                Debug.Log("Colour Match");
                //Turn the pixel x and y into a Vector2 position
                Vector2 pos = new Vector2(x, y);
                //Spawn object that matches pixel colour at pixel position
                Instantiate(colourMapping.spawnObj, pos, Quaternion.identity, transform);
            }
        }
    }
    //Level generator function
    void GenerateLevel()
    {
        //Scan whole texture and get pixel positions
        for(int x = 0; x < mapImage.width; x++)
        {
            for(int y = 0; y < mapImage.height; y++)
            {
                //Run the object spawning function passing through the current value of x and y as a position
                GenerateObject(x, y);
            }
        }
    }
    private void Start()
    {
        //Run the level generator
        GenerateLevel();
    }
}
