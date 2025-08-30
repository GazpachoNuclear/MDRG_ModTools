using UnityEngine;
using System.IO;
using System.IO.Compression;
using System;
using SimpleFileBrowser;
using TMPro;
using UnityEditor;

public class PM_MultilenguageExport : MonoBehaviour
{

    //private string JSON;
    private string LUA;
    private int count;

    private Guid guid;

    private string staticData;

    public TMP_Text loadedCSV;

    [System.Serializable]
    public class structuredDataRow
    {
        public string[] parameter;
    }

    [System.Serializable]
    public class structuredDataList
    {
        public structuredDataRow[] rows;
    }

    public structuredDataList myStructuredData = new structuredDataList();


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


    public void OpenFileBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("translation file", ".csv"));
        FileBrowser.SetDefaultFilter(".csv");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".json", ".lua", ".ods" , ".xlsx");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.ShowLoadDialog((paths) => { readCSV(paths[0]); }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Files, false, null, null, "Select Translations File", "Select");
    }


    void readCSV(string path)
    {
        staticData = System.IO.File.ReadAllText(path);

        string[] totalRows = staticData.Split("***END***");
        int numColumns = totalRows[0].Split("|").Length;

        myStructuredData.rows = new structuredDataRow[totalRows.Length];
        for (int i = 0; i < totalRows.Length; i++)
        {
            myStructuredData.rows[i] = new structuredDataRow();
            myStructuredData.rows[i].parameter = new string[numColumns];
        }

        for (int i = 0; i < totalRows.Length - 1; i++)
        {
            for (int j = 0; j < numColumns; j++)
            {
                myStructuredData.rows[i].parameter[j] = totalRows[i].Split("|")[j];
            }
        }

        loadedCSV.text = path;
    }


    public void ExportMultilenguage()
    {
        //Create a general folder
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            var folder = Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations");
        }
        else
        {
            var folder = Directory.CreateDirectory(Application.persistentDataPath + "/MachineTranslations");
        }

        //Iterate to all lenguages
        for (int i = 3; i < myStructuredData.rows[0].parameter.Length - 1; i++) //Column index for the lenguage
        {
            guid = Guid.NewGuid();

            count = 0;

            //Create a folder for lenguage specific
            if (SystemInfo.operatingSystem.Contains("Windows"))
            {
                var folder = Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations/" + myStructuredData.rows[1].parameter[i]);
            }
            else
            {
                var folder = Directory.CreateDirectory(Application.persistentDataPath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[i]);
            }

            createJSON(i);
            createLUA(i);
            CompressFiles(i);
        }
    }


    //Parses all information to the JSON file
    private void createJSON(int lenguage)
    {
        JSON.name = myStructuredData.rows[2].parameter[3] + "_" + myStructuredData.rows[1].parameter[lenguage];
        JSON.description = ScapeText(myStructuredData.rows[3].parameter[lenguage]);
        JSON.OnGameStart.luaFiles = new string[1];
        JSON.OnGameStart.luaFiles[0] = "script.lua";
        JSON.targetVersion = "0.90.15";
        JSON.doNotChangeVariablesBelowThis.timeCreated = 638842586268180000;
        JSON.doNotChangeVariablesBelowThis.guid.serializedGuid = guid.ToString();

        string json = JsonUtility.ToJson(JSON, true);
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.json"))
            {
                sw.WriteLine(json);
            }
        }
        else
        {
            System.IO.File.WriteAllText(Application.persistentDataPath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.json", json);
        }
    }

    //Parses all information to the LUA file
    private void createLUA(int lenguage)
    {
        LUA = "";
        LUA = "do\n\n" +
        "local itemprefab0 = ModUtilities.CreateItemPrefab()\n" +
        "itemprefab0.Name = '" + ScapeText(myStructuredData.rows[2].parameter[3]) + "_" + myStructuredData.rows[1].parameter[lenguage] + "'\n" +
        "itemprefab0.Description = '" + ScapeText(myStructuredData.rows[4].parameter[lenguage]) + "'\n" +
        "itemprefab0.Price = " + ScapeText(myStructuredData.rows[5].parameter[lenguage]) + "\n" +
        "itemprefab0.PossibleEquipmentSlots = { 'PersonalityModule'}\n" +
        "itemprefab0.RequiredSlots = { }\n";

        if (myStructuredData.rows[6].parameter[lenguage] == "true")
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
        LUA += "ModUtilities.AddSingleBuyItemToShop('" + myStructuredData.rows[7].parameter[lenguage] + "', itemgameid0)\n\n";
        LUA += "local personality = ModUtilities.PrepareNewPersonalityDefinition()\n";

        for (int j = 8; j < myStructuredData.rows.Length-1; j++) //For all rows (same as for all potential dialogues) starting on dialogue rows
            {
                //If the "You" section is not meant to be an answer for the specific block, and there is text in both sections (Bot and You):
                if (myStructuredData.rows[j].parameter[2]=="N" && myStructuredData.rows[j].parameter[3] != "null" && myStructuredData.rows[j+1].parameter[3] != "null")
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(myStructuredData.rows[j].parameter[0]) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(myStructuredData.rows[j].parameter[lenguage], "Bot") + ParseLuaSegment(myStructuredData.rows[j + 1].parameter[lenguage], "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //If "You" is meant to be used as an answer on the specific block, there is text in both sections (Bot and You):
                else if (myStructuredData.rows[j].parameter[2] == "Y" && myStructuredData.rows[j].parameter[3] != "null" && myStructuredData.rows[j + 1].parameter[3] != "null")
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(myStructuredData.rows[j].parameter[0]) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(myStructuredData.rows[j].parameter[lenguage], "Bot") + "\\n#end\\n#r" + ParseLuaSegment(myStructuredData.rows[j + 1].parameter[lenguage], "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //There is only text on "Bot" (answer toogle is ignored):
                else if (myStructuredData.rows[j].parameter[3] != "null" && myStructuredData.rows[j+1].parameter[3] == "null")
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(myStructuredData.rows[j].parameter[0]) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(myStructuredData.rows[j].parameter[lenguage], "Bot") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //There is only text on "You" (answer toogle is ignored):
                else if (myStructuredData.rows[j].parameter[3] == "null" && myStructuredData.rows[j + 1].parameter[3] != "null")
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(myStructuredData.rows[j].parameter[0]) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(myStructuredData.rows[j + 1].parameter[lenguage], "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                //If none of the conditions are met, it means there is no text on the block, so it is ignored.

                j++; //So we increment two units instead of just one
            }

        LUA += "\n\n" +
            "itemprefab0.TurnIntoPersonalityModule(itemgameid0, personality)\n\n" +
            "end";


        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "script.lua"))
            {
                sw.WriteLine(LUA);
            }
        }
        else
        {
            System.IO.File.WriteAllText(Application.persistentDataPath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "script.lua", LUA);
        }
    }

    //Per each LUA entry (character line), the script parses it and sets it in a single scaped line
    private string ParseLuaSegment(string raw, string name)
    {
        string[] splitted = raw.Split("***");
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
        raw.Replace("\n", "");
        raw.Replace("<br>", "");
        raw.Replace("\r\n", "");
        raw.Replace("\r", "");
        raw.Replace(" ", "");
        return raw.Split("***")[1];
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

    private void CompressFiles(int lenguage)
    {
        string filePath1;
        string filePath2;

        string zipPath;

        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            filePath1 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.json";
            filePath2 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "script.lua";
            zipPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.zip";
        }
        else
        {
            filePath1 = Application.persistentDataPath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.json";
            filePath2 = Application.persistentDataPath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "script.lua";
            zipPath = Application.persistentDataPath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.zip";
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

    public void CopyTranslationTemplate()
    {

        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            File.Copy(Application.dataPath + "/Resources/TranslationsVolumeII_guidance.ods", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/TranslationTemplateExample.ods");
        }
        else
        {
            File.Copy(Application.dataPath + "/Resources/TranslationsVolumeII_guidance.ods", Application.persistentDataPath + " /TranslationTemplateExample.ods");
        }
    }
}
