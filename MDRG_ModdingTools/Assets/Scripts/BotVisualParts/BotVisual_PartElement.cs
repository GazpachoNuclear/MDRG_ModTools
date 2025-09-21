using UnityEngine;
using TMPro;

public class BotVisual_PartElement : MonoBehaviour
{
    public TMP_Text partName;
    public GameObject[] visibilityButtons;
    public GameObject targetGameObject;
    
    public void ChangeVisibility()
    {
        if (visibilityButtons[0].activeSelf)
        {
            visibilityButtons[0].SetActive(false);
            visibilityButtons[1].SetActive(true);
            targetGameObject.SetActive(false);
        }
        else
        {
            visibilityButtons[0].SetActive(true);
            visibilityButtons[1].SetActive(false);
            targetGameObject.SetActive(true);
        }
    }

}
