using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.UI;
using SimpleFileBrowser;

public class ITEM_Export : MonoBehaviour
{

    private string LUA;
    private int count;

    public GameObject content;

    public GameObject popUp;
    public TMP_Text path;

    public Export exportManager;

    private string savePath;

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

        path.text = "";

        //Create a folder
        var folder = Directory.CreateDirectory(savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text));


        createJSON();
        createLUA();
        string[] filePaths = new string[2];
        string[] relativePaths = new string[2];
        filePaths[0] = savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "mod.json";
        filePaths[1] = savePath + "/" + CleanName(content.transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text) + "/" + "script.lua";
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
        LUA = "local function CreateItem(name, description, price, category, isStackable, effects, shop)\n" +
        "    local itemprefab = ModUtilities.CreateItemPrefab()\n" +
        "    itemprefab.Name = name\n" +
        "    itemprefab.Description = description\n" +
        "    itemprefab.Price = price\n" +
        "    itemprefab.Category = category\n" +
        "    itemprefab.IsStackable = isStackable\n\n" +
        "    function OnUse(item)\n" +
        "        for stat, value in pairs(effects) do\n" +
        "            if type(value) == \"number\" then\n" +
        "                gv[stat] = gv[stat] + value\n" +
        "            end\n" +
        "        end\n" +
        "        return true\n" +
        "    end\n\n" +
        "    itemprefab.OnUse = OnUse\n" +
        "    local newItem = ModUtilities.CreateNewItemAutoAssignId(CurrentModGuid, itemprefab)\n" +
        "    ModUtilities.AddGenericItemToShop(shop, newItem)\n" +
        "end\n\n" +
        "local items = {\n";


        //Loop through all user defined parameters for new items
        for (int i=2; i < content.transform.childCount; i++)
        {
            LUA += "	{\n" +
                "		name = \"" + ScapeText(content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemName.text) + "\",\n" +
                "		description = \"" + ScapeText(content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemDescription.text) + "\",\n" +
                "		price = " + content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemPrice.text + ",\n" +
                "		category = ItemCategory." + content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemCategory.options[content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemCategory.value].text + ",\n";
            
            if (content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemStackable.isOn)
            {
                LUA += "		isStackable = true,\n";
            }
            else
            {
                LUA += "		isStackable = false,\n";
            }

            LUA += "		effects = {\n";


            //Sub loop on all parameters
            for (int j=0; j < content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.childCount; j++)
            {
                if (!string.IsNullOrEmpty(content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.GetChild(j).GetComponentInChildren<TMP_InputField>().text))
                {
                    LUA += "			" + content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.GetChild(j).GetComponentInChildren<TMP_Text>().text + " = " + content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.GetChild(j).GetComponentInChildren<TMP_InputField>().text.Replace(",",".");
                    
                    if (j == content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.childCount - 1)
                    {
                        LUA += "\n";
                    }
                    else
                    {
                        LUA += ",\n";
                    }
                }
            }


            LUA += "		},\n" +
                "		shop = '" + content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemShop.options[content.transform.GetChild(i).GetComponent<ITEM_UsableItemElement>().itemShop.value].text + "'\n";

            if (i == content.transform.childCount -1)
            {
                LUA += "	}\n";
            }
            else
            {
                LUA += "	},\n";
            }
        }


        LUA += "}\n\n" +
            "for _, item in ipairs(items) do\n" +
            "    CreateItem(item.name, item.description, item.price, item.category, item.isStackable, item.effects, item.shop)\n" +
            "end";

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

}

