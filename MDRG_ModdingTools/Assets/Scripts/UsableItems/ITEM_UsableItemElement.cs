using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ITEM_UsableItemElement : MonoBehaviour
{
    public TMP_InputField itemName;
    public TMP_InputField itemDescription;
    public TMP_InputField itemPrice;
    public TMP_Dropdown itemShop;
    public TMP_Dropdown itemCategory;
    public Toggle itemStackable;
    public GameObject parametersContainer;

    public void DeleteButton()
    {
        Destroy(this.gameObject);
    }

}
