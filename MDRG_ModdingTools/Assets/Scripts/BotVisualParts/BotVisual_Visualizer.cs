using UnityEngine;

public class BotVisual_Visualizer : MonoBehaviour
{

    public GameObject[] botAssemblies;
    public GameObject[] botOverlays;

    public GameObject container;
    public GameObject partPrefab;
    public GameObject overlayPrefab;

    private int position;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = 0;
        ChangePosition();
    }


    public void ChangePosition()
    {
        for (int i=0; i < botAssemblies.Length; i++)
        {
            botAssemblies[i].SetActive(false);
        }
        botAssemblies[position].SetActive(true);

        UpdateList();

    }

    public void UpdateList()
    {
        for (int j = 0; j < container.transform.childCount; j++)
        {
            Destroy(container.transform.GetChild(j).gameObject);
        }

        for (int k = 0; k < botAssemblies[position].transform.childCount; k++)
        {
            GameObject instance = Instantiate(partPrefab, container.transform);
            instance.GetComponent<BotVisual_PartElement>().partName.text = botAssemblies[position].transform.GetChild(k).name;
            instance.GetComponent<BotVisual_PartElement>().targetGameObject = botAssemblies[position].transform.GetChild(k).gameObject;

            for (int i=0; i < botOverlays[position].transform.GetChild(k).transform.childCount; i++)
            {
                GameObject instance2 = Instantiate(overlayPrefab, container.transform);
                instance2.GetComponent<BotVisual_PartElement>().partName.text = botOverlays[position].transform.GetChild(k).transform.GetChild(i).name;
                instance2.GetComponent<BotVisual_PartElement>().targetGameObject = botOverlays[position].transform.GetChild(k).transform.GetChild(i).gameObject;
            }
        }
    }
}
