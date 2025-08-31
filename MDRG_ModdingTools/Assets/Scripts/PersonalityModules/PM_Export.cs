using UnityEngine;
using System.IO;
using System.IO.Compression;
using System;
using TMPro;
using UnityEngine.UI;
using SimpleFileBrowser;

public class PM_Export : MonoBehaviour
{

    private string LUA;
    private int count;

    public GameObject[] content;

    public GameObject popUp;
    public TMP_Text path;

    private string savePath;

    private Guid guid;


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

        count = 0;

        path.text = "";

        //Create a folder
        var folder = Directory.CreateDirectory(savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text));


        createJSON();
        createLUA();
        CompressFiles();

        path.text = savePath;
        popUp.SetActive(true);
    }

    //Parses all information to the JSON file
    private void createJSON()
    {
        JSON.name = content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text;
        JSON.description = ScapeText(content[0].transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text);
        JSON.OnGameStart.luaFiles = new string[1];
        JSON.OnGameStart.luaFiles[0] = "script.lua";
        JSON.targetVersion = "0.90.15";
        JSON.doNotChangeVariablesBelowThis.timeCreated = 638842586268180000;
        JSON.doNotChangeVariablesBelowThis.guid.serializedGuid = guid.ToString();

        string json = JsonUtility.ToJson(JSON, true);
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json"))
            {
                sw.WriteLine(json);
            }
        }
        else
        {
            System.IO.File.WriteAllText(savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json", json);
        }
    }

    //Parses all information to the LUA file
    private void createLUA()
    {
        LUA = "";
        LUA = "do\n\n" +
        "local itemprefab0 = ModUtilities.CreateItemPrefab()\n" +
        "itemprefab0.Name = '" + ScapeText(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "'\n" +
        "itemprefab0.Description = '" + ScapeText(content[0].transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text) + "'\n" +
        "itemprefab0.Price = " + ScapeText((content[0].transform.GetChild(2).GetComponentInChildren<TMP_InputField>().text).ToString()) + "\n" +
        "itemprefab0.PossibleEquipmentSlots = { 'PersonalityModule'}\n" +
        "itemprefab0.RequiredSlots = { }\n";

        if (content[0].transform.GetChild(3).GetComponentInChildren<Toggle>().isOn)
        {
            LUA += "itemprefab0.IsIllegal = true\n";
        }
        else
        {
            LUA += "itemprefab0.IsIllegal = false\n";
        }

        LUA += "itemprefab0.HasQuality = false\n" +
        "itemprefab0.IsPersonalityModule = true\n" +
        "itemprefab0.IsStackable = false\n" +
        "itemprefab0.Category = ItemCategory.Other\n" +
        "itemprefab0.CanChangeColor = false\n" +
        "itemprefab0.ColorSlots = { }\n" +
        "itemprefab0.Partners = { }\n" +
        "itemprefab0.ScratchType = ScratchTextureType.Universal\n" +
        "itemprefab0.SusModifiers = { }\n\n" +
        "local itemgameid0 = ModUtilities.CreateNewItemAutoAssignId(CurrentModGuid, itemprefab0)\n";

        switch (content[0].transform.GetChild(3).GetComponentInChildren<TMP_Dropdown>().value)
        {
            case 0:
                LUA += "ModUtilities.AddSingleBuyItemToShop('ladyparts.ic', itemgameid0)\n";
                break;
            case 1:
                LUA += "ModUtilities.AddSingleBuyItemToShop('clothier', itemgameid0)\n";
                break;
            case 2:
                LUA += "ModUtilities.AddSingleBuyItemToShop('pharmacy', itemgameid0)\n";
                break;
            case 3:
                LUA += "ModUtilities.AddSingleBuyItemToShop('grocery', itemgameid0)\n";
                break;
        }

        LUA += "local personality = ModUtilities.PrepareNewPersonalityDefinition()\n";

        for (int i=1; i<content.Length; i++)
        {
            for(int j=0; j<content[i].transform.childCount; j++)
            {
                //If the "You" section is not meant to be an answer for the specific block, and there is text in both sections (Bot and You):
                if (!content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().hasAnswer.isOn && !string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text) && !string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text))
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text, "Bot") + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text, "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //If "You" is meant to be used as an answer on the specific block, there is text in both sections (Bot and You):
                else if (content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().hasAnswer.isOn && !string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text) && !string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text))
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text, "Bot") + "\\n#end\\n#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text, "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //There is only text on "Bot" (answer toogle is ignored):
                else if (!string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text) && string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text))
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text, "Bot") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //There is only text on "You" (answer toogle is ignored):
                else if (string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text) && !string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text))
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text, "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //If none of the conditions are met, it means there is no text on the block, so it is ignored.
            }
        }

        LUA += "\n\n" +
            "itemprefab0.TurnIntoPersonalityModule(itemgameid0, personality)\n\n" +
            "end";


        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua"))
            {
                sw.WriteLine(LUA);
            }
        }
        else
        {
            System.IO.File.WriteAllText(savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua", LUA);
        }
    }

    //Per each LUA entry (character line), the script parses it and sets it in a single scaped line
    private string ParseLuaSegment(string raw, string name)
    {
        string[] splitted = raw.Split("\n");
        string parsed = "";

        for (int k=0; k < splitted.Length; k++)
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
        raw = raw.Replace("<","");
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

    private void CompressFiles()
    {
        string filePath1;
        string filePath2;

        string zipPath;

        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            filePath1 = savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json";
            filePath2 = savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua";
            zipPath = savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.zip";
        }
        else
        {
            filePath1 = savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json";
            filePath2 = savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua";
            zipPath = savePath + "/" + CleanName(content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.zip";
        }

        using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                // Agregar primer archivo
                string entryName1 = Path.GetFileName(filePath1);
                archive.CreateEntryFromFile(filePath1, entryName1);

                // Agregar segundo archivo
                string entryName2 = Path.GetFileName(filePath2);
                archive.CreateEntryFromFile(filePath2, entryName2);
            }
        }
    }
}
