using UnityEngine;
using SimpleFileBrowser;
using TMPro;
using UnityEngine.UI;

public class PM_import : MonoBehaviour
{

	public GameObject[] content;
	private int index;

	public void OpenFileBrowser()
    {
		FileBrowser.SetFilters(true, new FileBrowser.Filter("Mod files", ".lua"));
		FileBrowser.SetDefaultFilter(".lua");
		FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".json");
		FileBrowser.AddQuickLink("Users", "C:\\Users", null);
		FileBrowser.ShowLoadDialog((paths) => { ImportSelectedFile(paths[0]); }, () => { Debug.Log( "Canceled" ); }, FileBrowser.PickMode.Files, false, null, null, "Select Mod File", "Select" );
	}

	private void ImportSelectedFile(string path)
	{
		string allContent = System.IO.File.ReadAllText(path);

		string[] perLines = allContent.Split("\n");

		for (int i = 1; i < content.Length; i++)
		{
			for (int j = 0; j < content[i].transform.childCount; j++)
			{
				//We check which LUA line contains the specific trigger we are populating
				for (int k = 0; k < perLines.Length; k++)
				{
					index = 0;
					if (perLines[k].Contains(content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().trigger.text))
					{
						index = k;
						break;
					}
				}

				//From there, we start to retrieve data (only if index !=0, which means we had a match)
				if (index != 0)
				{
					content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().lines.text = PrepareLinesBot(perLines[index]);
					content[i].transform.GetChild(j).GetComponentInChildren<PM_InputElement>().answers.text = PrepareLinesYou(perLines[index]);
				}
			}
		}

		//The rest of parameters are hardcoded
		for (int k = 0; k < perLines.Length; k++)
		{
			if (perLines[k].Contains(".Name"))
			{
				content[0].transform.GetChild(0).GetComponentInChildren<TMP_InputField>().text = perLines[k].Split("=")[1];
			}
			else if (perLines[k].Contains(".Description"))
			{
				content[0].transform.GetChild(1).GetComponentInChildren<TMP_InputField>().text = perLines[k].Split("=")[1];
			}
			else if (perLines[k].Contains(".Price"))
			{
				content[0].transform.GetChild(2).GetComponentInChildren<TMP_InputField>().text = perLines[k].Split("=")[1];
			}
			else if (perLines[k].Contains(".IsIllegal"))
            {
                if (perLines[k].Contains("true"))
                {
					content[0].transform.GetChild(3).GetComponentInChildren<Toggle>().isOn = true;
				}
                else
                {
					content[0].transform.GetChild(3).GetComponentInChildren<Toggle>().isOn = false;
				}
			}
		}
	}

	private string PrepareLinesBot(string raw)
    {
		string prepared = "";

		raw.Replace("#end", "");

		string[] preSplit = raw.Split("', CurrentModGuid");

		string[] segmented = preSplit[0].Split("\\n");

        for (int i=0; i<segmented.Length; i++)
        {
            if (segmented[i].Contains("Bot:"))
            {
				prepared += segmented[i].Replace("Bot:", "") + "\n";
            }
        }

		return prepared;
    }


	private string PrepareLinesYou(string raw)
    {
		string prepared = "";

		raw.Replace("#end", "");

		string[] preSplit = raw.Split("', CurrentModGuid");

		string[] segmented = preSplit[0].Split("\\n");

		for (int i = 0; i < segmented.Length; i++)
		{
			if (segmented[i].Contains("You:"))
			{
				prepared += segmented[i].Replace("You:", "") + "\n";
			}
		}

		return prepared;
	}
}
