using UnityEngine;

public class FinEdgeCylinderScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Color colour;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Fin Edge Cylinder"))
        {
            turnRed();
        }
    }

        void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Fin Edge Cylinder"))
        {
            turnRed();
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Fin Edge Cylinder"))
        {
            turnWhite();
        }
    }

    void turnRed()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }

    void turnWhite()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
}
