using UnityEngine;
using SimpleFileBrowser;
using TMPro;
using UnityEngine.UI;

public class ITEM_Import : MonoBehaviour
{

    public GameObject prefab;
    public GameObject content;

	public void OpenFileBrowser()
	{
		FileBrowser.SetFilters(true, new FileBrowser.Filter("Mod files", ".lua"));
		FileBrowser.SetDefaultFilter(".lua");
		FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".json");
		FileBrowser.AddQuickLink("Users", "C:\\Users", null);
		FileBrowser.ShowLoadDialog((paths) => { ImportSelectedFile(paths[0]); }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Files, false, null, null, "Select Mod File", "Select");
	}


	private void CleanPreviousItems()
	{
		for (int i = 2; i < content.transform.childCount; i++)
		{
			Destroy(content.transform.GetChild(i).gameObject);
		}
	}


	private void ImportSelectedFile(string path)
	{
		CleanPreviousItems();

		
		string allContent = System.IO.File.ReadAllText(path);

		allContent = allContent.Split("local items = {")[1];
		allContent = allContent.Split("for _, item in ipairs(items) do")[0];
		string[] splitted = allContent.Split("	{");

        for (int i=1; i < splitted.Length; i++)	//i=0 is always blank
        {
			GameObject instance = Instantiate(prefab, content.transform);

			string[] lines = splitted[i].Split(",");

			instance.GetComponent<ITEM_UsableItemElement>().itemName.text = (lines[0].Replace("\"", "")).Split("=")[1];
			instance.GetComponent<ITEM_UsableItemElement>().itemDescription.text = (lines[1].Replace("\"", "")).Split("=")[1];
			instance.GetComponent<ITEM_UsableItemElement>().itemPrice.text = lines[2].Split("=")[1];

            switch (lines[3].Split("ItemCategory.")[1])
            {
				case "Food":
					instance.GetComponent<ITEM_UsableItemElement>().itemCategory.value = 0;
					break;
				case "Meds":
					instance.GetComponent<ITEM_UsableItemElement>().itemCategory.value = 1;
					break;
				case "Fish":
					instance.GetComponent<ITEM_UsableItemElement>().itemCategory.value = 2;
					break;
				case "FishingTrash":
					instance.GetComponent<ITEM_UsableItemElement>().itemCategory.value = 3;
					break;
				case "AnonEquip":
					instance.GetComponent<ITEM_UsableItemElement>().itemCategory.value = 4;
					break;
				case "Other":
					instance.GetComponent<ITEM_UsableItemElement>().itemCategory.value = 5;
					break;
            }

            if (lines[4].Contains("true"))
            {
				instance.GetComponent<ITEM_UsableItemElement>().itemStackable.isOn = true;
			}
            else
            {
				instance.GetComponent<ITEM_UsableItemElement>().itemStackable.isOn = false;
			}


            for (int j=0; j < instance.GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.childCount; j++)
            {
                for (int k=5; k < lines.Length; k++)
                {
                    if (lines[k].Contains(instance.GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.GetChild(j).GetComponentInChildren<TMP_Text>().text))
                    {
						instance.GetComponent<ITEM_UsableItemElement>().parametersContainer.transform.GetChild(j).GetComponentInChildren<TMP_InputField>().text = (lines[k].Replace("effects = {","")).Split("=")[1];
					}
                }
            }


            for (int q = lines.Length - 3; q < lines.Length; q++)
            {
                if (lines[q].Contains("shop"))
                {
					switch (lines[q].Split("'")[1])
					{
						case "ladyparts.ic":
							instance.GetComponent<ITEM_UsableItemElement>().itemShop.value = 0;
							break;
						case "clothier":
							instance.GetComponent<ITEM_UsableItemElement>().itemShop.value = 1;
							break;
						case "pharmacy":
							instance.GetComponent<ITEM_UsableItemElement>().itemShop.value = 2;
							break;
						case "grocery":
							instance.GetComponent<ITEM_UsableItemElement>().itemShop.value = 3;
							break;
					}
				}
            }
		}
	}

}
