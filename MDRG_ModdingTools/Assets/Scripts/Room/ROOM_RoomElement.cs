using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ROOM_RoomElement : MonoBehaviour
{
    public TMP_InputField itemName;
    public TMP_InputField itemDescription;
    public TMP_InputField itemPrice;
    public TMP_Dropdown itemShop;
    public TMP_Dropdown itemType;
    public TMP_Text targetSprite;

    public void DeleteButton()
    {
        Destroy(this.gameObject);
    }
}
