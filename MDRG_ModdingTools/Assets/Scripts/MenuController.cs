using UnityEngine;

public class MenuController : MonoBehaviour
{

    public GameObject[] panels;
    public GameObject[] PersonalityModulesSections;
    public GameObject popUp;

    public void GoToPanel(int index)
    {
        for (int i=0; i<panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        panels[index].SetActive(true);
    }


    public void GoToSectionPM(int index)
    {
        for (int i = 0; i < PersonalityModulesSections.Length; i++)
        {
            PersonalityModulesSections[i].SetActive(false);
        }
        PersonalityModulesSections[index].SetActive(true);
    }


    public void ClosePopUp()
    {
        popUp.SetActive(false);
    }


    public void Exit()
    {
        Application.Quit();
    }

}
