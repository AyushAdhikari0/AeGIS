using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
// using System.Numerics;
using System.Security.Cryptography;

// using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using UnityEngine.UIElements;

// https://www.engineeringtoolbox.com/elevation-speed-sound-air-d_1534.html
// http://www.braeunig.us/space/atmos.htm

public class FinDimensionInputManager : MonoBehaviour
{

    public List<Vector3> finCoords = new List<Vector3> { };

    // Start is called before the first frame update
    public InputField dimensionInputField;

    public float finThickness;

    public Material carbonFibreMaterial;

    public Material plyWoodMaterial;

    public Material aluminiumMaterial;

    public String finMaterial;

    public Material skybox;

    public float altitude;

    public float divergenceSpeed;

    private float kappa = 1.4f;
    public float pressure_0 = 101.35f * 1_000;

    public float temperature;

    public float speedOfSound;

    public float pressure;

    public float shearModulus;

    public float denominator_constant;

    public float lambda;

    public float finAspectRatio;

    public float finThicknessRatio;

    public float epsilon;

    public float finVolume;

    public GameObject selectDimensionMenuUI;

    public Dictionary<String, float> shearModulusDict = new Dictionary<String, float>
        {
            {"plywood", 613_633_000},
            {"carbon", 4_136_854_000},
            {"fibreglass", 4_136_854_000},
            {"aluminium", 26_200_078_000},
            {"titanium", 42_747_495},
            {"steel", 82_737_087},
            {"custom", 100_000_000}
        };


    void Start()
    {
        dimensionInputField = GetComponent<InputField>();

        finThickness = 60f;

        // dimensionInputField.onSubmit.AddListener(verifyInput);

        finCoords.Add(new Vector3(0, 0, finThickness / 20));


        finCoords.Add(new Vector3(25, 8, finThickness / 20));
        finCoords.Add(new Vector3(30, 5, finThickness / 20));
        // finCoords.Add(new Vector3(32, 2, finThickness / 20));
        finCoords.Add(new Vector3(30, 0, finThickness / 20));
        generateFinMesh();


        // finCoords.Add(new Vector3(0, 0, finThickness));
        // finCoords.Add(new Vector3(5, 5, finThickness));
        // finCoords.Add(new Vector3(10, 0, finThickness));

        // finMaterial = "plywood";
        // finMaterial = "carbon";
        // finCoords.Add(new Vector3(300, 0, finThickness));

        // map vertices from 
    }

    // Update is called once per frame

    public void Update()
    {
        //normalise the altitude from 0-1 to cause the skybox shader to change 
        skybox.SetFloat("_CubemapTransition", altitude / 30000);
        // setAirProperties();
    }

    public void generateFinMesh()
    {

        // update fin Thickness in finCoords (this is called after changes to the thickness slider)

        for (int i = 0; i < finCoords.Count; i++)
        {
            finCoords[i] = new Vector3(finCoords[i].x, finCoords[i].y, finThickness / 20);
        }

        // first destroy existing fin mesh objects

        GameObject[] existingFinMeshes = GameObject.FindGameObjectsWithTag("Fin Mesh");

        foreach (GameObject finMesh in existingFinMeshes)
        {
            Destroy(finMesh);
        }



        List<Vector3> finCoords_negative = new List<Vector3>();

        foreach (Vector3 coord in finCoords)
        {
            finCoords_negative.Add(new Vector3(coord.x, coord.y, -coord.z));
        }

        List<(Vector3, Vector3, Vector3)> triangles_negative = getTrianglesFromVertices(finCoords_negative);

        List<(Vector3, Vector3, Vector3)> triangles = getTrianglesFromVertices(finCoords, true);

        calculateDivergenceSpeed(triangles);


        // triangles.AddRange(triangles_negative);

        List<Vector3> finCoords3D = new List<Vector3>();

        finCoords3D.AddRange(finCoords);
        finCoords3D.AddRange(finCoords_negative);

        triangles.AddRange(triangles_negative);

        // add uvs to the vertices

        Vector2[] uvs = new Vector2[finCoords3D.Count];

        float minX = finCoords3D.Min(p => p.y);
        float maxX = finCoords3D.Max(p => p.y);
        float minY = finCoords3D.Min(p => p.x);
        float maxY = finCoords3D.Max(p => p.x);

        for (int i = 0; i < finCoords3D.Count; i++)
        {
            Vector3 v = finCoords3D[i];
            float u = (v.y - minX) / (maxX - minX);
            float vCoord = (v.x - minY) / (maxY - minY);
            uvs[i] = new Vector2(u, vCoord);
        }

        // add prism faces

        for (int i = 0; i < finCoords.Count - 1; i++)
        {
            Vector3 positiveVertex = finCoords[i];
            Vector3 negativeVertex = finCoords_negative[i];

            triangles.Add((positiveVertex, finCoords_negative[i + 1], negativeVertex));
            triangles.Add((positiveVertex, finCoords[i + 1], finCoords_negative[i + 1]));
        }
        triangles.Add((finCoords[0], finCoords_negative[0], finCoords_negative[finCoords_negative.Count - 1]));
        triangles.Add((finCoords[0], finCoords_negative[finCoords_negative.Count - 1], finCoords[finCoords.Count - 1]));

        int counter = 0;

        // convert triangles to indexes in vertices

        Dictionary<Vector3, int> vertexDict = new Dictionary<Vector3, int>();

        foreach (Vector3 vertex in finCoords3D)
        {
            vertexDict.Add(vertex, counter);
            counter++;
        }

        int[] triangleIndices = new int[3 * triangles.Count];

        for (int i = 0; i < triangles.Count; i++)
        {
            triangleIndices[3 * i] = vertexDict[triangles[i].Item1];
            triangleIndices[3 * i + 1] = vertexDict[triangles[i].Item2];
            triangleIndices[3 * i + 2] = vertexDict[triangles[i].Item3];
        }

        Mesh mesh = new Mesh();
        mesh.vertices = finCoords3D.ToArray();
        mesh.triangles = triangleIndices.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject meshObject = new GameObject("GeneratedMesh");
        meshObject.AddComponent<MeshFilter>().mesh = mesh;
        meshObject.tag = "Fin Mesh";
        meshObject.transform.parent = selectDimensionMenuUI.transform;

        switch (finMaterial)
        {
            case "plywood":

                meshObject.AddComponent<MeshRenderer>().material = plyWoodMaterial;
                break;

            case "carbon":
                meshObject.AddComponent<MeshRenderer>().material = carbonFibreMaterial;
                break;

            case "aluminium":
                meshObject.AddComponent<MeshRenderer>().material = aluminiumMaterial;
                break;

            default:
                meshObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                break;
        }
    }
    // public void readInputString(string inputString)
    // {

    //     string[] parts = inputString.Split(',');

    //     if (parts.Length != 2)
    //     {
    //         Debug.Log("Invalid Input: Use a single comma to separate coordinates");
    //         return;
    //     }

    //     // try to parse the input string

    //     float[] coords = new float[2];

    //     for (int i = 0; i < 2; i++)
    //     {
    //         string s = parts[i];

    //         if (float.TryParse(s.Trim(), out float result))
    //         {
    //             coords[i] = result;

    //             Debug.LogWarning($"Good numbers {string.Join(", ", coords)}");

    //             // add current coordinate to the list of fin coords

    //         }
    //         else
    //         {
    //             Debug.LogWarning($"Invalid number: {s}");
    //             return;
    //         }
    //     }
    //     finCoords.Add(new Vector3(coords[0], coords[1], finThickness / 20));

    // }

    // private void getEdges(List<Vector3> finCoords)
    // {

    //     List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>();

    //     Vector3 lastNode = finCoords[-1];
    //     Vector3 firstNode = finCoords[0];

    //     //check root chord is straight

    private List<(Vector3, Vector3, Vector3)> getTrianglesFromVertices(List<Vector3> finCoords, bool flipNormal = false)
    {

        // use ear clipping method to get triangles from list of verticies

        // assume vertices are ordered in clockwise direction

        List<(Vector3, Vector3, Vector3)> trianglesList = new List<(Vector3, Vector3, Vector3)>();

        var finCoords_temp = new List<Vector3>(finCoords);
        int vertexIndex = 0;


        while (finCoords_temp.Count > 3)
        {
            bool foundEar = true;

            // get three adjacent points along the polygon
            int n = finCoords_temp.Count;
            Vector3 a = finCoords_temp[(vertexIndex - 1 + n) % n];
            Vector3 b = finCoords_temp[vertexIndex % n];
            Vector3 c = finCoords_temp[(vertexIndex + 1) % n];


            // other point inside triangle check
            foreach (Vector3 point in finCoords_temp)
            {
                if ((point != a) && (point != b) && (point != c))
                {
                    if (isPointInTriangleXY(point, a, b, c))
                    {
                        foundEar = false;
                    }
                }
            }

            // convexity check
            if (!isConvex(a, b, c))
            {
                foundEar = false;
            }


            // if ear is not found, then increment vertex index
            if (foundEar == false)
            {
                vertexIndex++;
                continue;
            }
            else
            {
                if (flipNormal)
                {
                    trianglesList.Add((a, c, b));
                }
                else
                {
                    trianglesList.Add((a, b, c));
                }

                vertexIndex = vertexIndex % finCoords_temp.Count;
                finCoords_temp.RemoveAt(vertexIndex);

            }

        }

        if (finCoords_temp.Count == 3)
        {
            if (flipNormal)
            {
                trianglesList.Add((
finCoords_temp[0],
finCoords_temp[2],
finCoords_temp[1]
));
            }
            else
            {
                trianglesList.Add((
finCoords_temp[0],
finCoords_temp[1],
finCoords_temp[2]
));
            }

        }

        return trianglesList;

    }

    private bool isConvex(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // returns true if p1 is a convex point

        // get vectors from p0 to p1 and p1 to p2

        Vector3 v1 = p1 - p0;
        Vector3 v2 = p2 - p1;

        if (Vector3.Cross(v1, v2).z < 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private bool isPointInTriangleXY(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Use only x and y
        float denom = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
        if (Mathf.Abs(denom) < 1e-8f)
            return false; // Degenerate triangle

        float alpha = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / denom;
        float beta = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / denom;
        float gamma = 1.0f - alpha - beta;

        return (alpha >= 0) && (beta >= 0) && (gamma >= 0);
    }


    float Direction(Vector2 a, Vector2 b, Vector2 c)
    {
        // Cross product (b - a) x (c - a)
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    public void calculateDivergenceSpeed(List<(Vector3, Vector3, Vector3)> triangles)
    {
        setAirProperties();
        // asssumes the triangles variable contains only the Vector3 coordinates for a 2D fin shape.

        // first find the total area of the fin

        float[] areas = new float[triangles.Count];
        float[] centroids = new float[triangles.Count];
        float total_centroid = 0;
        float total_area = 0;

        for (int i = 0; i < triangles.Count; i++)
        {

            areas[i] = getTriangleAreaXY(triangles[i].Item1, triangles[i].Item2, triangles[i].Item3) / 10_000;
            centroids[i] = getTriangleXCentroidXY(triangles[i].Item1, triangles[i].Item2, triangles[i].Item3) / 100;
            total_centroid += centroids[i] * areas[i];
            total_area += areas[i];
        }

        total_centroid = total_centroid / total_area;

        finVolume = total_area * finThickness / 1000;

        // convert from m*m*m to cm*cm*cm

        finVolume = finVolume * 1_000_000;



        // first get the root chord measurement and calculate epsilon 

        float root_chord = (finCoords[finCoords.Count - 1].x - finCoords[0].x) / 100;
        epsilon = total_centroid / root_chord - 0.25f;

        // now, we calculate the batwing area to get the equivalent tip chord length and lambda

        float span = 0;

        foreach (Vector3 coord in finCoords)
        {
            if (coord.y / 100 > span)
            {
                span = coord.y / 100;
            }
        }


        //convert all values to SI units before continuing


        float tip_chord = total_area / span * 2 - root_chord;

        lambda = tip_chord / root_chord;

        finAspectRatio = span * span / total_area;
        finThicknessRatio = (finThickness / 1_000) / root_chord;

        // calculating the denominator constant

        denominator_constant = (float)(kappa * 24 * epsilon * pressure_0 / Math.PI);

        shearModulus = getShearModulus(finMaterial);

        divergenceSpeed = (float)(speedOfSound * Math.Sqrt(shearModulus / ((denominator_constant * Math.Pow(finAspectRatio, 3) / (Math.Pow(finThicknessRatio, 3) * (finAspectRatio + 2)) * ((lambda + 1) / 2) * (pressure / pressure_0)))));

        return;
    }

    private float getTriangleAreaXY(Vector3 a, Vector3 b, Vector3 c)
    {
        return (float)0.5 * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
    }

    private float getTriangleXCentroidXY(Vector3 a, Vector3 b, Vector3 c)
    {
        return (float)(a.x + b.x + c.x) / 3;
    }

    public Vector3 getFinCentre()
    {
        Vector3 centre = new Vector3(0, 0, 0);
        foreach (Vector3 coord in finCoords)
        {
            centre = centre + coord;

        }
        return centre / finCoords.Count;
    }

    private float getShearModulus(String finMaterial)
    {
        // returns shear modulus in Pa

        return shearModulusDict[finMaterial];
    }

    public void setCustomMaterial(float customShearModulus)
    {
        shearModulusDict["custom"] = customShearModulus;
        finMaterial = "custom";
    }

    private void setAirProperties()
    {
        // sets the air pressure, speed of sound and pressure given altitude

        Dictionary<String, int> columnIndices = new Dictionary<String, int>
        {
            {"altitude" , 0},
            {"temperature", 1},
            {"pressure" , 2},
            {"speedOfSound" , 6}
        };

        TextAsset csvFile = Resources.Load<TextAsset>("atmosphericData"); // omit .csv extension


        float[] previousLine = { };

        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split('\n');

            bool headingLineBool = true;

            foreach (string line in lines)
            {
                if (headingLineBool)
                {
                    headingLineBool = false;
                    continue;
                }

                string[] values = line.Split(',');
                float currentAltitude = float.Parse(values[columnIndices["altitude"]]);

                if (currentAltitude >= altitude)
                {
                    // interpolate between the two altitudes 
                    float previousAltitude = previousLine[columnIndices["altitude"]];
                    float previousTemperature = previousLine[columnIndices["temperature"]];
                    float previousPressure = previousLine[columnIndices["pressure"]];
                    float previousSpeedOfSound = previousLine[columnIndices["speedOfSound"]];

                    float fraction = (altitude - previousAltitude) / (currentAltitude - previousAltitude);

                    // float  = previousLine[columnIndices["altitude"]];
                    float currentTemperature = float.Parse(values[columnIndices["temperature"]]);
                    float currentPressure = float.Parse(values[columnIndices["pressure"]]);
                    float currentSpeedOfSound = float.Parse(values[columnIndices["speedOfSound"]]);

                    pressure = Mathf.Lerp(previousPressure, currentPressure, fraction);
                    temperature = Mathf.Lerp(previousTemperature, currentTemperature, fraction);
                    speedOfSound = Mathf.Lerp(previousSpeedOfSound, currentSpeedOfSound, fraction);

                    return;

                }

                // store previous line as floats

                previousLine = new float[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    previousLine[i] = float.Parse(values[i]);
                }

            }
            Debug.LogError("Target altitude not found in CSV directory, max altitude 30,000m");
        }
        else
        {
            Debug.LogError("CSV file not found in Resources folder.");
        }



    }

    public void handleInputMaterialData(int dropDownIndex)
    {

        switch (dropDownIndex)
        {
            case 1:
                {
                    finMaterial = "plywood";
                    Debug.Log("PLYWOOD");
                }
                break;

            case 0:
                {
                    finMaterial = "carbon";
                    Debug.Log("CARBON");
                }
                break;

            case 2:
                {
                    finMaterial = "fibreglass";
                }
                break;

            case 3:
                {
                    finMaterial = "aluminium";
                }
                break;
        }

        generateFinMesh();

    }
}