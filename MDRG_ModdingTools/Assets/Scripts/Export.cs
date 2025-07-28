using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class Export : MonoBehaviour
{

    private string JSON;
    private string LUA;
    private int count;

    public GameObject[] content;

    public GameObject popUp;
    public TMP_Text path;

    public void ExportMod()
    {
        count = 0;
        path.text = "";
        createJSON();
        createLUA();
        popUp.SetActive(true);
    }

    private void createJSON()
    {
        JSON = "";
        JSON = "{\n" +
        "\"name\": \"" + content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text + "\",\n" +
        "\"description\": \"" + content[0].transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text + "\",\n" +
        "\"OnGameStart\": {\n" +
                            "\"luaFiles\": [\n" +
                    "\"script.lua\"\n" +
            "]\n" +
        "},\n" +
        "\"targetVersion\": \"0.90.15\"\n," +
        "\"doNotChangeVariablesBelowThis\": {\n" +
                "\"timeCreated\": 638842586268180000,\n" +
            "\"guid\": {\n" +
                    "\"serializedGuid\": \"4207c48d-b072-4a93-b95e-ce3e048c4226\"\n" +
            "}\n" +
        "}\n" +
        "}";

        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/mod.json"))
            {
                sw.WriteLine(JSON);
                path.text += System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/mod.json\n &\n";
            }
        }
        else
        {
            System.IO.File.WriteAllText(Application.persistentDataPath + "/mod.json", JSON);
            path.text += Application.persistentDataPath + "/mod.json\n &\n";
        }
    }

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
                if (!content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().hasAnswer.isOn)
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text, "Bot") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                else if (content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().hasAnswer.isOn)
                {
                    LUA += "personality.PrepareContainer('" + CleanNextLines(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text) + "').AddBranch(StoryBotDialogueBranch.__new('#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text, "Bot") + "\\n#end\\n#r" + ParseLuaSegment(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text, "You") + "', CurrentModGuid," + count.ToString() + "))\n";
                }
                
            }
        }

        LUA += "\n\n" +
            "itemprefab0.TurnIntoPersonalityModule(itemgameid0, personality)\n\n" +
            "end";


        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/script.lua"))
            {
                sw.WriteLine(LUA);
                path.text += System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/script.lua";
            }
        }
        else
        {
            System.IO.File.WriteAllText(Application.persistentDataPath + "/script.lua", LUA);
            path.text += Application.persistentDataPath + "/script.lua";
        }
    }


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

    private string ScapeText(string raw)
    {
        return raw.Replace("'", "\\'");
    }

    private string CleanNextLines(string raw)
    {
        return raw.Replace("\n", "");
    }
}
