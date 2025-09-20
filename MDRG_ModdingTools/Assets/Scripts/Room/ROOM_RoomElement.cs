using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimpleFileBrowser;

public class ROOM_RoomElement : MonoBehaviour
{
    public TMP_InputField itemName;
    public TMP_InputField itemDescription;
    public TMP_InputField itemPrice;
    public TMP_Dropdown itemShop;
    public TMP_Dropdown itemType;
    public TMP_Text targetSprite;
    public Toggle itemAnimated;
    public TMP_InputField animationFrameTime;

    public void DeleteButton()
    {
        Destroy(this.gameObject);
    }

    public void OpenFileBrowser()
    {
        if (!itemAnimated.isOn)
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Sprite", ".png"));
            FileBrowser.SetDefaultFilter(".png");
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".json");
            FileBrowser.AddQuickLink("Users", "C:\\Users", null);
            FileBrowser.ShowLoadDialog((paths) => { targetSprite.text = paths[0]; }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Files, false, null, null, "Select Image File", "Select");
        }
        else if (itemAnimated.isOn)
        {
            FileBrowser.AddQuickLink("Users", "C:\\Users", null);
            FileBrowser.ShowSaveDialog((paths) => { targetSprite.text = paths[0]; }, () => { Debug.Log("Canceled"); }, FileBrowser.PickMode.Folders, false, null, null, "Select Sequence Folder", "Select");
        }
    }
}
