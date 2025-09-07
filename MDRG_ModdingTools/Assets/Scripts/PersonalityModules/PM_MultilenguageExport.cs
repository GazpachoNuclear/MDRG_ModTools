using UnityEngine;
using System.IO;
using System;
using SimpleFileBrowser;
using TMPro;
using UnityEngine.UI;

public class PM_MultilenguageExport : MonoBehaviour
{

    //private string JSON;
    private string LUA;
    private int count;

    private Guid guid;

    private string savePath;
    private string staticData;

    public Export exportManager;

    public GameObject popUp;
    public TMP_Text path;

    public GameObject[] content;

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


    public void OpenSaveFileBrowser()
    {
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.ShowSaveDialog((paths) => { savePath = paths[0]; ExportMultilenguage(); }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Folders, false, null, null, "Select Save Folder", "Select");
    }


    public void ExportMultilenguage()
    {
        //Create a general folder
        var folder = Directory.CreateDirectory(savePath + "/MachineTranslations");
        

        //Iterate to all lenguages
        for (int i = 3; i < myStructuredData.rows[0].parameter.Length - 1; i++) //Column index for the lenguage
        {
            guid = Guid.NewGuid();

            count = 0;

            //Create a folder for lenguage specific
            var folder2 = Directory.CreateDirectory(savePath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[i]);


            createJSON(i);
            createLUA(i);
            string[] filePaths = new string[2];
            filePaths[0] = savePath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[i] + "/" + "mod.json";
            filePaths[1] = savePath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[i] + "/" + "script.lua";
            exportManager.ExportCompressedMod(filePaths, savePath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[i] + "/" + "mod.zip");
        }

        path.text = savePath + "/MachineTranslations";
        popUp.SetActive(true);
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
        exportManager.ExportFile(savePath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "mod.json", json);
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

        exportManager.ExportFile(savePath + "/MachineTranslations/" + myStructuredData.rows[1].parameter[lenguage] + "/" + "script.lua", LUA);
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


    public void CopyTranslationTemplate()
    {
        Application.OpenURL("https://drive.google.com/drive/folders/1TS5ZKvJfehe_3M03BtViK9b6TkRhLLj6?usp=sharing");


        //Let's prepare the data for copy-paste on the template
        string triggers = "";
        string speakers = "";
        string isAnswer = "";
        string rawLines = "";

        triggers += "Name\nMod_description\nDescription\nPrice\nIsIllegal\nStore\n";
        speakers += "\n\n\n\n\n\n";
        isAnswer += "\n\n\n\n\n\n";
        rawLines += content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text + "\n";
        rawLines += content[0].transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text + "\n";
        rawLines += content[0].transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text + "\n";
        rawLines += (content[0].transform.GetChild(2).GetComponentInChildren<TMP_InputField>().text).ToString() + "\n";
        if (content[0].transform.GetChild(3).GetComponentInChildren<Toggle>().isOn)
        {
            rawLines += "true\n";
        }
        else
        {
            rawLines += "false\n";
        }
        switch (content[0].transform.GetChild(3).GetComponentInChildren<TMP_Dropdown>().value)
        {
            case 0:
                rawLines += "ladyparts.ic\n";
                break;
            case 1:
                rawLines += "clothier\n";
                break;
            case 2:
                rawLines += "pharmacy\n";
                break;
            case 3:
                rawLines += "grocery\n";
                break;
        }

        for (int i = 1; i < content.Length; i++)
        {
            for (int j = 0; j < content[i].transform.childCount; j++)
            {
                if (!string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text) || !string.IsNullOrEmpty(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text))
                {
                    //For Bot:
                    triggers += "***" + content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text + "***" + "\n";
                    speakers += "Bot\n";
                    if (content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().hasAnswer.isOn)
                    {
                        isAnswer += "Y\n";
                    }
                    else
                    {
                        isAnswer += "N\n";
                    }
                    rawLines += ParseForTranslationSheet(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text);

                    //For you:
                    triggers += "***" + content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text + "***" + "\n";
                    speakers += "You\n";
                    if (content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().hasAnswer.isOn)
                    {
                        isAnswer += "Y\n";
                    }
                    else
                    {
                        isAnswer += "N\n";
                    }
                    rawLines += ParseForTranslationSheet(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text);
                }
            }
        }

        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Application.persistentDataPath;  //Generic, in case the user is not triggering a save previous some commands.
        }

        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(savePath + "/triggersColumn.txt"))
            {
                sw.WriteLine(triggers);
            }
            using (StreamWriter sw = File.CreateText(savePath + "/speakerColumn.txt"))
            {
                sw.WriteLine(speakers);
            }
            using (StreamWriter sw = File.CreateText(savePath + "/answerColumn.txt"))
            {
                sw.WriteLine(isAnswer);
            }
            using (StreamWriter sw = File.CreateText(savePath + "/linesColumn.txt"))
            {
                sw.WriteLine(rawLines);
            }
        }
        else
        {
            System.IO.File.WriteAllText(savePath + "/triggersColumn.txt", triggers);
            System.IO.File.WriteAllText(savePath + "/speakerColumn.txt", speakers);
            System.IO.File.WriteAllText(savePath + "/answerColumn.txt", isAnswer);
            System.IO.File.WriteAllText(savePath + "/linesColumn.txt", rawLines);
        }
    }


    private string ParseForTranslationSheet(string raw)
    {
        raw.Replace("\n", "***");
        return raw;
    }
}
