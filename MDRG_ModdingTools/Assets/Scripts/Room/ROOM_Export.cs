using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections.Generic;

public class ROOM_Export : MonoBehaviour
{
    private string LUA;
    private string LUAcoroutineAUX1;
    private string LUAcoroutineAUX2;
    private int count;
    private int coroutineCount;

    public GameObject content;

    public GameObject popUp;
    public TMP_Text path;

    public Export exportManager;

    private List<string> copyPaths = new List<string>();
    private List<string> relPaths = new List<string>();
    private string savePath;
    private string[] filePaths;
    private string[] relativePaths;

    private Guid guid;
    private long tick;


    //For JSON parsing:
    [System.Serializable]
    public class onStart
    {
        public string[] luaFiles;
    }

    [System.Serializable]
    public class GUID
    {
        public string serializedGuid;
    }

    [System.Serializable]
    public class doNotChange
    {
        public long timeCreated;
        public GUID guid;
    }

    [System.Serializable]
    public class JSONstructure
    {
        public string name;
        public string description;
        public onStart OnGameStart;
        public string targetVersion;
        public doNotChange doNotChangeVariablesBelowThis;
    }

    public JSONstructure JSON;


    //Opens the save dialogue
    public void OpenFileBrowser()
    {
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.ShowSaveDialog((paths) => { savePath = paths[0]; ExportMod(); }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Folders, false, null, null, "Select Save Folder", "Select");
    }


    //Launches the main functions
    public void ExportMod()
    {
        guid = Guid.NewGuid();

        DateTime now = DateTime.Now;
        tick = now.Ticks;

        count = 0;
        coroutineCount = 0;

        path.text = "";

        //Create a folder
        var folder = Directory.CreateDirectory(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text));

        //Lists are cleared
        copyPaths.Clear();
        relPaths.Clear();

        //Strings and files are generated
        createJSON();
        createLUA();

        //File compression/preparation is trigerred
        filePaths = new string[copyPaths.Count + 2];
        relativePaths = new string[relPaths.Count + 2];
        filePaths[0] = savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json";
        filePaths[1] = savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua";
        for (int i=2; i < filePaths.Length; i++)
        {
            filePaths[i] = copyPaths[i - 2];
            relativePaths[i] = relPaths[i - 2];
        }

        //Final compression is launched
        exportManager.ExportCompressedMod(filePaths, relativePaths, savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.zip");

        path.text = savePath;
        popUp.SetActive(true);
    }

    //Parses all information to the JSON file
    private void createJSON()
    {
        JSON.name = content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text;
        JSON.description = ScapeText(content.transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text);
        JSON.OnGameStart.luaFiles = new string[1];
        JSON.OnGameStart.luaFiles[0] = "script.lua";
        JSON.targetVersion = "0.90.15";
        JSON.doNotChangeVariablesBelowThis.timeCreated = tick;
        JSON.doNotChangeVariablesBelowThis.guid.serializedGuid = guid.ToString();

        string json = JsonUtility.ToJson(JSON, true);
        exportManager.ExportFile(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json", json);
    }

    //Parses all information to the LUA file
    private void createLUA()
    {
        LUA = "";

        //Loop through all user defined parameters for new room items
        for (int i = 2; i < content.transform.childCount; i++)
        {
            //If no coroutine:
            if (!content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemAnimated.isOn)
            {
                //Related images are copied from original directories
                string[] auxPath1 = (content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.value].text).Split("/");
                string[] auxPath2 = (content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().targetSprite.text).Split("\\");
                Directory.CreateDirectory(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + auxPath1[auxPath1.Length - 1]);
                File.Copy(content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().targetSprite.text, savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + auxPath1[auxPath1.Length - 1] + "/" + auxPath2[auxPath2.Length - 1], true);
                copyPaths.Add(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + auxPath1[auxPath1.Length - 1] + "/" + auxPath2[auxPath2.Length - 1]);
                relPaths.Add(auxPath1[auxPath1.Length - 1] + "/");

                //LUA text is generated
                LUA += "-- Item ID" + count.ToString() + ":\n" +
                    "function ChangeSprite" + count.ToString() + "(value, item" + count.ToString() + ")\n" +
                    "  if item" + count.ToString() + ".IsEquipped() and value then\n" +
                    "     TextureOverriderManager.RoomManager.SetSprite(\"" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.value].text + "\", refPath" + count.ToString() + ")\n" +
                    "  else\n" +
                    "     TextureOverriderManager.RoomManager.RestoreSprite(\"" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.value].text + "\", refPath" + count.ToString() + ")\n" +
                    "  end\n" +
                    "end\n\n" +
                    "itemprefab" + count.ToString() + " = ModUtilities.CreateItemPrefab()\n" +
                    "itemprefab" + count.ToString() + ".Name = '" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemName.text + "'\n" +
                    "itemprefab" + count.ToString() + ".Description = '" + ScapeText(content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemDescription.text) + "'\n" +
                    "itemprefab" + count.ToString() + ".Price =" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemPrice.text + "\n" +
                    "itemprefab" + count.ToString() + ".PossibleEquipmentSlots = {'" + auxPath1[auxPath1.Length - 1] + "'}\n" +
                    "itemprefab" + count.ToString() + ".IsIllegal = false\n" +
                    "itemprefab" + count.ToString() + ".HasQuality = false\n" +
                    "itemprefab" + count.ToString() + ".IsStackable = false\n" +
                    "itemprefab" + count.ToString() + ".Category = ItemCategory.Room\n" +
                    "itemprefab" + count.ToString() + ".CanChangeColor = false\n" +
                    "itemprefab" + count.ToString() + ".ColorSlots = {}\n" +
                    "itemprefab" + count.ToString() + ".ScratchType = ScratchTextureType.Universal\n" +
                    "itemprefab" + count.ToString() + ".SusModifiers = {}\n" +
                    "itemprefab" + count.ToString() + ".SpecialEffectAction = ChangeSprite" + count.ToString() + "\n\n";

                LUA += "refPath" + count.ToString() + " = ModUtilities. GetSpriteReference(CurrentModGuid, \"" + auxPath1[auxPath1.Length - 1] + "/" + auxPath2[auxPath2.Length - 1] + "\")\n\n" +
                    "item" + count.ToString() + " = ModUtilities.CreateNewItemAutoAssignId(CurrentModGuid, itemprefab" + count.ToString() + ")\n" +
                    "ModUtilities.AddSingleBuyItemToShop('" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemShop.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemShop.value].text + "', item" + count.ToString() + ")\n\n\n";

            }
            //If coroutine
            else if (content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemAnimated.isOn)
            {
                LUA += "-- Item ID" + count.ToString() + ":\n";

                // Related images are copied from original directories (and the LUA references are set as well!)
                string[] auxPath1 = (content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.value].text).Split("/");
                Directory.CreateDirectory(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + auxPath1[auxPath1.Length - 1]);
                CopyAndPrepareCoroutine(content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().targetSprite.text, savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + auxPath1[auxPath1.Length - 1], content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.value].text, auxPath1, (content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().animationFrameTime.text).Replace(",", "."));

                LUA += LUAcoroutineAUX1 + "\n" +
                    "local unityCoroutine\n\n" +
                    "function ChangeSprite" + count.ToString() + "(value, item" + count.ToString() + ")\n" +
                    "	if value and item" + count.ToString() + ".IsEquipped() then\n" +
                    "		function Coroutine()\n" +
                    "			while true do\n" +
                    LUAcoroutineAUX2 +
                    "			end\n" +
                    "		end\n"+
                    "		unityCoroutine = ModUtilities.StartCoroutine(Coroutine)\n" +
                    "	else\n" +
                    "		ModUtilities.StopCoroutine(unityCoroutine)\n" +
                    "	  TextureOverriderManager.RoomManager.RestoreSprite(item" + count.ToString() + ".GetUniqueGuid(), \"" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemType.value].text + "\")\n" +
                    "		unityCoroutine = nil\n" +
                    "	end\n" +
                    "end\n\n" +
                    "itemprefab" + count.ToString() + " = ModUtilities.CreateItemPrefab()\n" +
                    "itemprefab" + count.ToString() + ".Name = '" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemName.text + "'\n" +
                    "itemprefab" + count.ToString() + ".Description = '" + ScapeText(content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemDescription.text) + "'\n" +
                    "itemprefab" + count.ToString() + ".Price =" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemPrice.text + "\n" +
                    "itemprefab" + count.ToString() + ".PossibleEquipmentSlots = {'" + auxPath1[auxPath1.Length - 1] + "'}\n" +
                    "itemprefab" + count.ToString() + ".IsIllegal = false\n" +
                    "itemprefab" + count.ToString() + ".HasQuality = false\n" +
                    "itemprefab" + count.ToString() + ".IsStackable = false\n" +
                    "itemprefab" + count.ToString() + ".Category = ItemCategory.Room\n" +
                    "itemprefab" + count.ToString() + ".CanChangeColor = false\n" +
                    "itemprefab" + count.ToString() + ".ColorSlots = {}\n" +
                    "itemprefab" + count.ToString() + ".ScratchType = ScratchTextureType.Universal\n" +
                    "itemprefab" + count.ToString() + ".SusModifiers = {}\n" +
                    "itemprefab" + count.ToString() + ".SpecialEffectAction = ChangeSprite" + count.ToString() + "\n\n"+
                    "item" + count.ToString() + " = ModUtilities.CreateNewItemAutoAssignId(CurrentModGuid, itemprefab" + count.ToString() + ")\n" +
                    "ModUtilities.AddSingleBuyItemToShop('" + content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemShop.options[content.transform.GetChild(i).GetComponent<ROOM_RoomElement>().itemShop.value].text + "', item" + count.ToString() + ")\n\n\n";
            }

            //We update count so next item is different from current
            count++;
        }

        exportManager.ExportFile(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua", LUA);
    }

    //Per each LUA entry (character line), the script parses it and sets it in a single scaped line
    private string ParseLuaSegment(string raw, string name)
    {
        string[] splitted = raw.Split("\n");
        string parsed = "";

        for (int k = 0; k < splitted.Length; k++)
        {
            parsed += "\\n" + name + ": " + ScapeText(splitted[k]);
        }

        return parsed;
    }

    //Scapes the lines for LUA
    private string ScapeText(string raw)
    {
        return raw.Replace("'", "\\'");
    }

    //Removes new lines
    private string CleanNextLines(string raw)
    {
        return raw.Replace("\n", "");
    }

    //Cleans the files names to avoid forbidden special characters
    private string CleanName(string raw)
    {
        raw = raw.Replace("<", "");
        raw = raw.Replace(">", "");
        raw = raw.Replace(":", "");
        raw = raw.Replace("\"", "");
        raw = raw.Replace("\\", "");
        raw = raw.Replace("/", "");
        raw = raw.Replace("|", "");
        raw = raw.Replace("?", "");
        raw = raw.Replace("*", "");
        raw = raw.Replace(" ", "_");
        return raw;
    }


    private void CopyAndPrepareCoroutine(string sourceDir, string destinationDir, string targetObject, string[] auxPath1, string animationDelay)
    {
        LUAcoroutineAUX1 = "";
        LUAcoroutineAUX2 = "";

        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            copyPaths.Add(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + auxPath1[auxPath1.Length - 1] + "/" + file.Name);
            relPaths.Add(auxPath1[auxPath1.Length - 1] + "/");
            file.CopyTo(targetFilePath);

            LUAcoroutineAUX1 += "local coroutineRefPath" + coroutineCount.ToString() + " = ModUtilities.GetSpriteReference(CurrentModGuid, \"" + auxPath1[auxPath1.Length - 1] + "/" + file.Name + "\")\n";

            LUAcoroutineAUX2 += "			    TextureOverriderManager.RoomManager.SetSprite(item" + count.ToString() + ".GetUniqueGuid(), \"" + targetObject + "\", coroutineRefPath" + coroutineCount.ToString() + ")\n" +
                "			    coroutine.yield(ModUtilities.WaitForSeconds(" + animationDelay + "))\n\n";

            coroutineCount++;
        }
    }
}
