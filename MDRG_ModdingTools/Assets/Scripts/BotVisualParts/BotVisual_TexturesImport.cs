using UnityEngine;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using System;

public class BotVisual_TexturesImport : MonoBehaviour
{

    private string targetFolder;
    //private List<string> JSONpaths = new List<string>();

    public GameObject prefab;

    public GameObject BotOverlays;

    //For JSON parsing:
    [System.Serializable]
    public class rectInt
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    [System.Serializable]
    public class drawable
    {
        public string Name;
        public rectInt RectInt;
        public int ColorIndex;
        public int DrawableType;
        public bool BypassColorScaler;
    }

    [System.Serializable]
    public class pack
    {
        public string TextureName;
        public int Layer;
        public drawable[] PackedDrawables;
        public string[] Targets;
        public bool DontIncludeVanillaLayers;
    }

    [System.Serializable]
    public class packedTextures
    {
        public pack[] PackedTextures;
    }
    //End of JSON structure


    //Opens the save dialogue
    public void OpenFileBrowser()
    {
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.ShowLoadDialog((paths) => { targetFolder = paths[0]; LoadData(); }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Folders, false, null, null, "Select Load Folder", "Select");
    }


    public void LoadData()
    {
        //Removes any previously loaded textures on reload/load
        CleanPreviousTextures();

        // Get information about the source directory
        var dir = new DirectoryInfo(targetFolder);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        foreach (FileInfo file in dir.GetFiles())
        {
            if (file.Name.Contains(".json"))
            {
                ReadJSONdata(targetFolder + "/" + file.Name);
            }
        }
    }


    //Data is prepared (preprocessed) to be sent for image generation
    private void ReadJSONdata(string path)
    {
        string jsonData = File.ReadAllText(path);
        packedTextures textureData = JsonUtility.FromJson<packedTextures>(jsonData);

        for (int i=0; i < textureData.PackedTextures.Length; i++)
        {
            for (int j=0; j < textureData.PackedTextures[i].PackedDrawables.Length; j++)
            {
                Vector4 coordinates = new Vector4(textureData.PackedTextures[i].PackedDrawables[j].RectInt.x, textureData.PackedTextures[i].PackedDrawables[j].RectInt.y, textureData.PackedTextures[i].PackedDrawables[j].RectInt.width, textureData.PackedTextures[i].PackedDrawables[j].RectInt.height);

                var rawData = System.IO.File.ReadAllBytes(targetFolder + "/" + textureData.PackedTextures[i].TextureName);
                Texture2D texture = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
                texture.LoadImage(rawData);

                for (int k=0; k < textureData.PackedTextures[i].Targets.Length; k++)
                {
                    for (int w=0; w < BotOverlays.transform.childCount; w++)
                    {
                        if (textureData.PackedTextures[i].Targets[k] == BotOverlays.transform.GetChild(w).name)
                        {
                            for (int q=0; q < BotOverlays.transform.GetChild(w).transform.childCount; q++)
                            {
                                if (BotOverlays.transform.GetChild(w).transform.GetChild(q).name == textureData.PackedTextures[i].PackedDrawables[j].Name)
                                {
                                    GenerateTexturedGameobject(coordinates, texture, BotOverlays.transform.GetChild(w).transform.GetChild(q).gameObject, textureData.PackedTextures[i].Layer);
                                }
                            }
                        }
                    }
                }

            }
        }

        //Finally, list is updated
        this.GetComponent<BotVisual_Visualizer>().UpdateList();
    }


    private void GenerateTexturedGameobject(Vector4 coordinates, Texture2D texture, GameObject parent, int layerOrder)
    {
        Rect rect = new Rect(coordinates.x, coordinates.y, coordinates.z, coordinates.w);   //This comes from JSONs data "x", "y", "width" & "height"
        Sprite slicedSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));

        GameObject instance = Instantiate(prefab, parent.transform);
        instance.GetComponent<SpriteRenderer>().sprite = slicedSprite;
        instance.GetComponent<SpriteRenderer>().sortingOrder = parent.GetComponent<SpriteRenderer>().sortingOrder;
        instance.name = "layer" + layerOrder.ToString();
        instance.transform.localPosition = new Vector3(0, 0, -layerOrder);
    }


    private void CleanPreviousTextures()
    {
        for (int i=0; i < BotOverlays.transform.childCount; i++)
        {
            for (int j=0; j < BotOverlays.transform.GetChild(i).transform.childCount; j++)
            {
                for (int k=0; k < BotOverlays.transform.GetChild(i).transform.GetChild(j).transform.childCount; k++)
                {
                    Destroy(BotOverlays.transform.GetChild(i).transform.GetChild(j).transform.GetChild(k).gameObject);
                }
            }
        }
    }
}
