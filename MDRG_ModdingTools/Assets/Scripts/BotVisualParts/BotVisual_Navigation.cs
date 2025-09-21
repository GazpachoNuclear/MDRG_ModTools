using UnityEngine;

public class BotVisual_Navigation : MonoBehaviour
{

    private int[] zoom = new int[] {10, 5, 3 , 2 , 1};
    private int zoomIndex = 1;

    public Camera mainCamera;

    public void ChangeZoom(int i)
    {
        if ((zoomIndex + i) >= 0 && (zoomIndex + i) < zoom.Length)
        {
            zoomIndex += i;
            mainCamera.orthographicSize = zoom[zoomIndex];
        }
    }

    public void ResetCamera()
    {
        mainCamera.transform.position = new Vector3(0, 0, -10);
        zoomIndex = 1;
        mainCamera.orthographicSize = zoom[zoomIndex];
    }


    //MISSING POSITIONAL NAVIGATION!
}
