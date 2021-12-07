using Coordinates;
using Geometry;
using SimpleJSONOptimized;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;
using MeshFilter = UnityEngine.MeshFilter;
using MeshRenderer = UnityEngine.MeshRenderer;


namespace Imports {
    public class Import : MonoBehaviour {
        private static ArrayBuffer<Vector3> vecBuffer = new ArrayBuffer<Vector3>();
        private static ArrayBuffer<int> triBuffer = new ArrayBuffer<int>();

        public Material material;
        public TextAsset file;
        private string _jsonString;
        public string type;
        public float version;
        public string coordinateSystem;
        public bool goodCoord;
        public bool centered;
        private Vector3 _origin = Vector3.zero;
        private bool _existing = false;

        private void Awake() {
            _jsonString = file.text;



            MeshFilter mf = this.GetComponent<MeshFilter>();
            if (mf != null && mf.mesh != null) {
                Destroy(mf.mesh);
                mf.mesh = null; // not sure if needed, but also no harm
            }


            double startTime = Time.realtimeSinceStartupAsDouble;
            Generate();
            Debug.Log("Execute Time: " + ((Time.realtimeSinceStartupAsDouble - startTime) * 1000) + " ms");

        }

        void MakeGameObject_array(Vector3[] vertices, int[] triangles, int vertcount, int triCount) {
            // Debug.Log($"there are {vertices.Length} points in the list and {triangles.Length / 3} triangles in the list");

            // settings for the mesh

            MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = this.GetComponent<MeshFilter>();

            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

            _existing = true;
            meshRenderer.sharedMaterial = material;


            // set all the right values to the mesh
            Mesh mesh = new Mesh {
                indexFormat = IndexFormat.UInt32,
                // vertices = vertices
                // triangles = triangles
            };
            mesh.SetVertices(vertices, 0, vertcount);
            mesh.SetTriangles(triangles, 0, triCount, 0);


            mesh.RecalculateNormals();

            //make the mesh
            meshFilter.mesh = mesh;
            meshFilter.transform.position = _origin;
            Quaternion empty = new Quaternion(0, 0, 0, 0);
            // meshFilter.transform.SetPositionAndRotation(_origin, empty);
        }

        private void Generate() {
            JSONNode input = JSON.Parse(_jsonString, false);

            // Knowledge so you know what you deal with... not perse needed
            type = input["type"].Value;
            version = input["version"].AsFloat;
            if (string.IsNullOrEmpty(coordinateSystem)) {
                if (!(string.IsNullOrEmpty(input["metadata"].ToString()))) {
                    JSONNode metadata = input["metadata"];
                    if (!(string.IsNullOrEmpty(metadata["referenceSystem"]))) {
                        string rName = metadata["referenceSystem"];
                        if (rName.Contains("EPSG")) {
                            coordinateSystem = "EPSG ";
                        }

                        for (int i = 0; i < rName.Length; i++) {
                            if (Char.IsDigit(rName[i])) {
                                coordinateSystem += rName[i];
                            }
                        }
                    }
                }
            }

            //Get the vertices
            JSONNode vertices = input["vertices"];
            Vector3[] vArray = new Vector3[vertices.Count];

            if (string.IsNullOrEmpty(coordinateSystem) || !goodCoord) {
                Debug.Log(
                    "no coordinate sytem is found so the json will be projected unscaled on the wrong place");
                if (centered) {
                    //change y and z axis
                    for (int i = 0; i < vertices.Count; i++) {
                        // Make the first vertice (0,0,0) so it doesn't dissapear in the infinity
                        float x = (vertices[i][0].AsFloat - vertices[0][0]);
                        float y = (vertices[i][2].AsFloat - vertices[0][2]);
                        float z = (vertices[i][1].AsFloat - vertices[0][1]);
                        vArray[i] = new Vector3(x, y, z);
                    }
                } else {
                    //change y and z axis
                    for (int i = 0; i < vertices.Count; i++) {
                        float x = (vertices[i][0].AsFloat);
                        float y = (vertices[i][2].AsFloat);
                        float z = (vertices[i][1].AsFloat);
                        vArray[i] = new Vector3(x, y, z);
                    }
                }

            } else {
                // goodplace is based on coordinate system
                JSONNode scale = input["transform"]["scale"];
                JSONNode trans = input["transform"]["translate"];
                Vector3 zero = Vector3.zero;

                // RD or RD with NAP height
                if (coordinateSystem == "EPSG 7415" || coordinateSystem == "EPSG 28992") {
                    for (int i = 0; i < vertices.Count; i++) {
                        double x = (vertices[i][0].AsDouble);
                        double y = (vertices[i][2].AsDouble);
                        double z = (vertices[i][1].AsDouble);
                        if (scale != null) {
                            x *= scale[0];
                            y *= scale[2];
                            z *= scale[1];
                        }

                        if (trans != null) {
                            x += trans[0];
                            y += trans[2];
                            z += trans[1];
                        }

                        Vector2d ptToWgs = Coord.RDtoWGS84(x, z);
                        Vector2d newCoord = Coord.WGS84toGoogleBing(ptToWgs.x, ptToWgs.y);

                        x = newCoord.x;
                        z = newCoord.y;
                        Vector3d loc = Coord.sm_loc(new Vector3d(x, y, z));
                        loc.y = loc.y * Coord.Meter_to_WM_source / 100;
                        vArray[i] = loc.ToVector3();
                        if (i == 0 && centered) {
                            _origin = zero;
                            zero = vArray[i];
                        } else if (i == 0) {
                            _origin = vArray[i];
                            zero = vArray[i];
                        }

                        vArray[i] -= zero;
                    }
                } else if (coordinateSystem == "WGS 84") {
                    for (int i = 0; i < vertices.Count; i++) {
                        double x = (vertices[i][0].AsDouble);
                        double y = (vertices[i][2].AsDouble);
                        double z = (vertices[i][1].AsDouble);
                        if (scale != null) {
                            x *= scale[0];
                            y *= scale[2];
                            z *= scale[1];
                        }

                        if (trans != null) {
                            x += trans[0];
                            y += trans[2];
                            z += trans[1];
                        }

                        Vector2d newCoord = Coord.WGS84toGoogleBing(x, y);

                        x = newCoord.x;
                        z = newCoord.y;
                        Vector3d loc = Coord.sm_loc(new Vector3d(x, y, z));
                        loc.y = loc.y * Coord.Meter_to_WM_source / 100;
                        vArray[i] = loc.ToVector3();
                        if (i == 0 && centered) {
                            _origin = zero;
                            zero = vArray[i];
                        } else if (i == 0) {
                            _origin = vArray[i];
                        }

                        vArray[i] -= zero;
                    }
                } else {
                    Debug.Log("your coordinate reference system is not suported");
                    if (centered) {
                        for (int i = 0; i < vertices.Count; i++) {
                            // Make the first vertice (0,0,0) so it doesn't dissapear in the infinity
                            float x = (vertices[i][0].AsFloat - vertices[0][0]);
                            float y = (vertices[i][2].AsFloat - vertices[0][2]);
                            float z = (vertices[i][1].AsFloat - vertices[0][1]);
                            vArray[i] = new Vector3(x, y, z);
                        }
                    } else {
                        for (int i = 0; i < vertices.Count; i++) {
                            float x = vertices[i][0].AsFloat;
                            float y = vertices[i][2].AsFloat;
                            float z = vertices[i][1].AsFloat;
                            vArray[i] = new Vector3(x, y, z);
                        }
                    }
                }
            }


            // get the city object
            JSONNode cityObjects = input["CityObjects"];
            Debug.Log($"Number of cityObjects {cityObjects.Count}"); // for now to see what format we deal with

            GEOM.preProcessedEarcut.Clear();
            GEOM.veccount = 0;
            GEOM.tricount = 0;

            // Perprocess loop
            int maxObject = cityObjects.Count;
            for (int i = 0; i < maxObject; i++) {
                // unpack a city object           
                JSONNode cityObject = cityObjects[i];
                JSONNode geometries = cityObject["geometry"];

                // Get size of veclist and trilist
                GEOM.Count(geometries, vArray);
            }

            //ARRAY STUFF
            //make empty arrays
            int[] triarray = triBuffer.GetBuffer(GEOM.tricount * 3);
            Vector3[] vecarray = vecBuffer.GetBuffer(GEOM.veccount);

            for (int i = 0; i < maxObject; i++) {
                // unpack a city object           
                JSONNode cityObject = cityObjects[i];
                string objectType = cityObject["type"].Value;
                JSONNode geometries = cityObject["geometry"];

                // get the vertices and triangles in the build class
                GEOM.buildarray(geometries, vArray, vecarray, triarray);
            }


            MakeGameObject_array(vecarray, triarray, GEOM.veccount, GEOM.tricount * 3);
        }
    }
}