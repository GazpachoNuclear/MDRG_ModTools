using UnityEngine;

public class test : MonoBehaviour
{

    public Texture2D texture;
    public GameObject prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rect rect = new Rect(5, 5, 237, 212);   //This comes from JSONs data "x", "y", "width" & "height"!

        Sprite testSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        GameObject instance = Instantiate(prefab, this.gameObject.transform);
        instance.GetComponent<SpriteRenderer>().sprite = testSprite;
        Debug.Log("test done");
    }
}
