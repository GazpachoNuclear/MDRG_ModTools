using UnityEngine;

public class BotVisual_Visualizer : MonoBehaviour
{

    public GameObject[] botAssemblies;

    public GameObject container;
    public GameObject partPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangePosition(0);
    }


    public void ChangePosition(int position)
    {
        for (int i=0; i < botAssemblies.Length; i++)
        {
            botAssemblies[i].SetActive(false);
        }
        botAssemblies[position].SetActive(true);

        for (int j=0; j < container.transform.childCount; j++)
        {
            Destroy(container.transform.GetChild(j).gameObject);
        }

        for (int k=0; k < botAssemblies[position].transform.childCount; k++)
        {
            GameObject instance = Instantiate(partPrefab, container.transform);
            instance.GetComponent<BotVisual_PartElement>().partName.text = botAssemblies[position].transform.GetChild(k).name;
            instance.GetComponent<BotVisual_PartElement>().targetGameObject = botAssemblies[position].transform.GetChild(k).gameObject;
        }

    }
}
