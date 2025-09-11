using UnityEngine;

public class ITEM_CreateNewItemTemplate : MonoBehaviour
{

    public GameObject usableItemPrefab;
    public GameObject container;

    public void InstantiatePrefab()
    {
        Instantiate(usableItemPrefab, container.transform);
    }

}
