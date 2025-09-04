using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VertexCoordinateSetter : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject selectedVertex;

    public Color selectedVertexColour;

    public Color defaultVertexColour;

    public float vertexXCoord;
    public float vertexYCoord;
    public GameObject displayCoordsObject;

    public bool coordinateVisibility = true;
    void Start()
    {
        selectedVertex = null;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setSelectedVertex(GameObject vertexSphere)
    {
        if (selectedVertex != null)
        {
            // reset previous selected vertex colour
            selectedVertex.GetComponent<Renderer>().material.color = defaultVertexColour;
        }

        // set new selected vertex
        selectedVertex = vertexSphere;

        //change colour of the selected vertex
        selectedVertex.GetComponent<Renderer>().material.color = selectedVertexColour;
    }

    public void updateStoredXCoord(string inputString)
    {
        if (float.TryParse(inputString, out float result))
        {
            vertexXCoord = result;
        }
    }
    public void updateStoredYCoord(string inputString)
    {
        if (float.TryParse(inputString, out float result))
        {
            vertexYCoord = result;
        }
    }

    public void setXCoord()
    {
        if (selectedVertex != null)
        {
            if (!selectedVertex.GetComponent<FinVertexSphereScript>().yConstrained)
            {
                selectedVertex.transform.position = new Vector3(vertexXCoord, selectedVertex.transform.position.y, selectedVertex.transform.position.z);
            }
        }
    }

    public void setYCoord()
    {
        if (selectedVertex != null)
        {
            if (!selectedVertex.GetComponent<FinVertexSphereScript>().xConstrained) {
            selectedVertex.transform.position = new Vector3(selectedVertex.transform.position.x, vertexYCoord, selectedVertex.transform.position.z);
        }
    }
    }

    public void toggleCoordinateLabelVisibility()
    {
        coordinateVisibility = !coordinateVisibility;
    }
}
