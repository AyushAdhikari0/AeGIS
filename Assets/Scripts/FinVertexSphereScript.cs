using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;

// using System.Numerics;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class FinVertexSphereScript : MonoBehaviour
{
    // Start is called before the first frame update

    public int index;
    public bool xConstrained;
    public bool yConstrained;

    private Vector3 mousePos;

    public Vector3 labelOffset = new Vector3(0, 1f, 0);

    private GameObject labelInstance;
    public TextMeshPro labelText;

    public GameObject previousVertex;

    public GameObject nextVertex;

    private GameObject mainCam;

    public float scaleFactor;

    public GameObject vertexCoordinateManager;
    void Start()
    {
        // transform.localScale = Vector3.one * radius * 2;
        gameObject.tag = "Fin Vertex Sphere";
        index = (int)transform.eulerAngles.x;
        xConstrained = (bool)(transform.eulerAngles.x == 1);
        yConstrained = transform.eulerAngles.y != 0;


        // labelInstance = transform.Find("Canvas/CoordLabel").gameObject;
        // // Parent to this sphere

        // // Set local position to place above/below the sphere
        // labelInstance.transform.localPosition = labelOffset;
        // labelInstance.transform.localRotation = Quaternion.identity;

        // Get reference to the text component

        vertexCoordinateManager = GameObject.FindGameObjectWithTag("Vertex Coordinate Setter");
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");



    }

    void Update()
    {
        // depending on initial load timings, the object may not find the camera in Start()

        if (mainCam == null)
        {
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        }

        if (vertexCoordinateManager == null)
        {
            vertexCoordinateManager = GameObject.FindGameObjectWithTag("Vertex Coordinate Setter");
        }

        //left click to drag and drop

        //left double click to manually input coordinate

        //right click to delete

        //click new vertex to add a vertex

        xConstrained = transform.eulerAngles.x != 0;
        yConstrained = transform.eulerAngles.y != 0;

        if (labelText != null)
        {
            if (GameObject.FindGameObjectWithTag("Vertex Coordinate Setter").GetComponent<VertexCoordinateSetter>().coordinateVisibility == true)
            {
                Vector3 pos = transform.position;
                labelText.text = $"({pos.x:F2}, {pos.y:F2})";
            }

            else
            {
                labelText.text = "";
            }
        }


        if (transform.position.y < 0)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }

        // Keep size consistent on screen by scaling based on distance
        float distance = Vector3.Distance(mainCam.transform.position, transform.position);
        transform.localScale = Vector3.one * distance * scaleFactor;

        // check for right click as there is no native handler for OnRightMouseDown


    }

    public void OnMouseDown()
    {
        mousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if ((xConstrained == false) && (transform.parent.GetComponent<DisplayCoordinates>().temp_finCoords.Count > 3))
            {
                deleteItself();

            }

        }
    }

    public void OnMouseOver()
    {
        mousePos = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

        if (Input.GetMouseButtonDown(1))
        {
            vertexCoordinateManager.GetComponent<VertexCoordinateSetter>().setSelectedVertex(gameObject);
        }
    }

    public void OnMouseDrag()
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition - mousePos);

        float xMax = newPos.x;

        if (yConstrained)
        {
            xMax = 0;
        }

        if (!xConstrained)
        {
            transform.position = new Vector3(xMax, Math.Max(newPos.y, 0), transform.position.z);
        }
        else
        {
            transform.position = new Vector3(xMax, 0, transform.position.z);
        }
        transform.parent.GetComponent<DisplayCoordinates>().refreshEdgeCylindersAndFinCoords();
    }

    public void deleteItself()
    {
        // updateSpheresAbove();

        previousVertex.GetComponent<FinVertexSphereScript>().setNextVertex(nextVertex);
        nextVertex.GetComponent<FinVertexSphereScript>().setPreviousVertex(previousVertex);
        transform.parent.GetComponent<DisplayCoordinates>().refreshEdgeCylindersAndFinCoords();
        Destroy(gameObject);

    }

    public void setPreviousVertex(GameObject v)
    {
        previousVertex = v;
    }
    public GameObject getPreviousVertex()
    {
        return previousVertex;
    }

    public void setNextVertex(GameObject v)
    {
        nextVertex = v;
    }
    public GameObject getNextVertex()
    {
        return nextVertex;
    }
}
