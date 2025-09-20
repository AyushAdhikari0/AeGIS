using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// using System.Numerics;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Play.Publisher.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCoordinates : MonoBehaviour
{
    public GameObject mainCam;              // camera object

    public List<Vector3> temp_finCoords;    // stores unsaved fin coordinates in the fin designer

    public float vertexSphereRadius = 4;

    public GameObject vertexSpherePrefab;

    public GameObject finCylinderPrefab;

    public GameObject lastVertex;           // store first and last vertex in fin geometry
    public GameObject firstVertex;

    public Button saveButton;               // save button for saving current design
    public TextMeshProUGUI errorTextBlock;  // text block that displays error messages

    void LateUpdate()
    {
        // make save button interactible if fin geometry is valid
        saveButton.interactable = finGeometryIsValid();
    }

    void OnEnable()
    {
        // destroy all fin vertices that exist
        foreach (GameObject v in GameObject.FindGameObjectsWithTag("Fin Vertex Sphere"))
        {
            Destroy(v);
        }

        // initialise new vertices and cylinders
        initialiseVertices();
        refreshEdgeCylindersAndFinCoords();
    }

    public void initialiseVertices()
    {

        // get saved fin coordinates from the main camera
        List<Vector3> finCoords = mainCam.GetComponent<FinDimensionInputManager>().finCoords;

        // stores previous vertex
        GameObject prev = null;

        for (int i = 0; i < finCoords.Count; i++)
        {
            // these booleans constrain the vertex's position domains
            bool xConstrained;      // vertex can only move along x axis if true
            bool yConstrained;      // vertex can only move along y axis if true

            // constrain the first vertex in x and y, constrain the last vertex in x
            xConstrained = ((i == 0) || (i == finCoords.Count - 1));
            yConstrained = (i == 0);

            GameObject currentFinVertex = generateVertexSphere(finCoords[i], xConstrained, yConstrained);

            if (i == 0)
            {
                // store first vertex
                firstVertex = currentFinVertex;

            }
            else
            {
                // assign 'previous vertex' field for current vertex
                currentFinVertex.GetComponent<FinVertexSphereScript>().setPreviousVertex(prev);

                // assign the 'next vertex' field for previous vertex 
                prev.GetComponent<FinVertexSphereScript>().setNextVertex(currentFinVertex);

                if (i == finCoords.Count - 1)
                {
                    // store last vertex
                    lastVertex = currentFinVertex;
                }
            }

            // store previous vertex for iteration
            prev = currentFinVertex;
            
        }
    }

    public void refreshEdgeCylindersAndFinCoords()
    {

        //destroy all existing fin edge cylinders

        List<GameObject> toDestroy = new List<GameObject>();
        toDestroy.AddRange(GameObject.FindGameObjectsWithTag("Fin Edge Cylinder"));

        foreach (GameObject g in toDestroy)
        {
            Destroy(g);
        }

        // clear temporary coords 
        temp_finCoords.Clear();

        // using the first vertex, and the linked structure of vertices, reconstruct the fin edges
        GameObject currentVertex = firstVertex;
        GameObject previousVertex = lastVertex;

        while (true)
        {
            // construct a fin edge
            generateEdgeCylinder(previousVertex.transform.position, currentVertex.transform.position);

            // store current vertex to temporary coordinate list
            temp_finCoords.Add(currentVertex.transform.position);

            // quit if final vertex reached
            if (currentVertex == lastVertex)
            {
                break;
            }

            // update previous and current cylinder using linked list structure.
            previousVertex = currentVertex;
            currentVertex = currentVertex.GetComponent<FinVertexSphereScript>().getNextVertex();
        }

    }

    GameObject generateVertexSphere(Vector3 coordinate, bool xConstrained, bool yConstrained)
    {
        // initialise new vertex
        // also encode the constraints in the vertex's rotation for troubleshooting
        GameObject vertexSphere = Instantiate(vertexSpherePrefab, coordinate, Quaternion.Euler(Convert.ToInt32(xConstrained), Convert.ToInt32(yConstrained), 0f), transform);

        // initialise adjacent vertices as null
        vertexSphere.GetComponent<FinVertexSphereScript>().setNextVertex(null);
        vertexSphere.GetComponent<FinVertexSphereScript>().setPreviousVertex(null);

        return vertexSphere;
    }

    void generateEdgeCylinder(Vector3 start, Vector3 end)
    {
        GameObject finCylinder = Instantiate(finCylinderPrefab);

        // position cylinder
        finCylinder.transform.position = (start + end) / 2;

        // scale and rotate cylinder

        Vector3 direction = end - start;
        finCylinder.transform.up = direction.normalized;

        finCylinder.transform.localScale = new Vector3(1, 1, 1) * 0.005f * Vector3.Distance(mainCam.transform.position, finCylinder.transform.position);
        finCylinder.transform.localScale = new Vector3(finCylinder.transform.localScale.x, direction.magnitude / 2, finCylinder.transform.localScale.z);

        // add tags
        finCylinder.tag = "Fin Edge Cylinder";
        finCylinder.transform.parent = transform;
        
    }

    public void createNewFinVertex()
    {
        // adds a new vertex between the last and second last vertex

        // get second last vertex
        GameObject secondLastVertex = lastVertex.GetComponent<FinVertexSphereScript>().previousVertex;

        // find middle position between last and second last vertex
        Vector3 secondLastSpot = 0.5f * (lastVertex.transform.position + secondLastVertex.transform.position);

        GameObject newSphere = generateVertexSphere(secondLastSpot, false, false);

        // alter the last vertex links
        newSphere.GetComponent<FinVertexSphereScript>().setNextVertex(lastVertex);
        lastVertex.GetComponent<FinVertexSphereScript>().setPreviousVertex(newSphere);

        // alter the second last vertex links
        newSphere.GetComponent<FinVertexSphereScript>().setPreviousVertex(secondLastVertex);
        secondLastVertex.GetComponent<FinVertexSphereScript>().setNextVertex(newSphere);
    }

    public bool finGeometryIsValid()
    {
        // checks if the current fin geometry (stored in temp_finCoords) is valid

        // this string stores all the error messages
        String statusString = "";

        //first, check self intersecting points

        HashSet<Vector3> pointSet = new HashSet<Vector3>();
        foreach (Vector3 coord in temp_finCoords)
        {
            if (pointSet.Contains(coord))
            {
                statusString += "Self Intersecting Points!\n";
                break;
            }

            pointSet.Add(coord);
        }

        // check none of the fin edge cylinders are coloured red (intersecting)
        // (the cylinder's script colours edges red if they intersect)

        bool hasIntersectingEdges = false;
        int numRootChordEdges = 0;

        foreach (GameObject cylinder in GameObject.FindGameObjectsWithTag("Fin Edge Cylinder"))
        {
            // count number of root chord edges as well
            if ((cylinder.transform.position.y < 0.01) && ((cylinder.transform.eulerAngles.z == -90) || (cylinder.transform.eulerAngles.z == 90)))
            {
                numRootChordEdges += 1;
            }

            // red cylinders are intersecting
            if (cylinder.GetComponent<Renderer>().material.color == Color.red)
            {
                hasIntersectingEdges = true;
            }
        }

        if (hasIntersectingEdges) {
            statusString += "Self Intersecting Edges or Corners Too Sharp!\n";
        }

        if (numRootChordEdges > 1) {
            statusString += "Too many edges on the Root Chord!\n";           
        }

        // check that the fin geometry is clockwise by checking the relative position of the first and last vertex
        // this works because they are both x-constrained and linked
        if (lastVertex.transform.position.x <= firstVertex.transform.position.x)
        {
            statusString += "Vertex winding order must be clockwise!";
        }

        //display error message
        errorTextBlock.text = statusString;

        // returns true if there are no errors
        return statusString.Length == 0;
    }
}