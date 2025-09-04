using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexLabeller : MonoBehaviour
{
    // Start is called before the first frame update

    public float scaleFactor;

    private GameObject mainCam;
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;

        // Keep size consistent on screen by scaling based on distance
        float distance = Vector3.Distance(mainCam.transform.position, transform.position);
        // transform.localScale = Vector3.one * distance * scaleFactor;

        // transform.localPosition = new Vector3(0, 0, 0);

    }
}
