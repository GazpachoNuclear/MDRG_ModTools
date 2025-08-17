using UnityEngine;

public class CheckNegatives : MonoBehaviour
{

    public GameObject cautionPoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Instantiate(cautionPoint, contact.point, Quaternion.identity) ;
        }
    }

}
