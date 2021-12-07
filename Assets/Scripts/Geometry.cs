using System.Collections.Generic;
using UnityEngine;
using meshData;
using SimpleJSONOptimized;

// this.transform.position = objectPosition.ToVector3();

namespace Geometry

{
    public static class GEOM
    {
        const bool debugging = false;

        public static int veccount = 0;
        public static int tricount = 0;
        public static Dictionary<JSONNode, MeshData> preProcessedEarcut = new Dictionary<JSONNode, MeshData>();

        private static int emptyvec = 0;
        private static int emptytri = 0;

        private static List<Vector3> outside = new List<Vector3>();
        private static List<List<Vector3>> holes = new List<List<Vector3>>();
        private static List<Vector3> newHole = new List<Vector3>();


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //count function ///////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void Count(JSONNode geometries, Vector3[] vArray)
        {
            emptyvec = 0;
            emptytri = 0;
            //head function it tells it where to go and what to do
            for (int l = 0; l < geometries.Count; l++)
            {
                var geometry = geometries[l];

                // do we need settings like this? maybe we can set them as atributes or someting like that
                var geometryType = geometry["type"].Value;
                var geometryLod = geometry["lod"].Value;
                var geometryBoundaries = geometry["boundaries"];

                // if (geometryLod != "2.2")
                // {
                //     continue;
                // }

                // Special object implement later -> Not in BAG
                if (geometryType == "MultiPoint")
                {
                    Debug.LogError("not yet implemented");
                }

                // Special object implement later -> Not in BAG
                else if (geometryType == "MultiLineString")
                {
                    Debug.LogError("not yet implemented");
                }

                else if (geometryType == "MultiSurface" | geometryType == "CompositeSurface")
                {
                    CountSurface(geometryBoundaries, vArray);
                }

                else if (geometryType == "Solid")
                {
                    CountSolid(geometryBoundaries, vArray);
                }
                else if (geometryType == "MultiSolid" | geometryType == "CompositeSolid")
                {
                    CountMSolid(geometryBoundaries, vArray);
                }

                // Special object implement later --> Not in BAG
                else if (geometryType == "GeometryInstance")
                {
                    Debug.LogError("not yet implemented");
                }
            }
        }

        private static void CountSurface(JSONNode surfaces, Vector3[] vArray)
        {
            // the real count function
            for (int i = 0; i < surfaces.Count; i++)
            {
                // get the surfaces
                var surface = surfaces[i];
                var exteriorBoundary = surface[0];
                
                // if only triangle
                if (exteriorBoundary.Count == 3 && surface.Count == 1)
                {
                    veccount += 3;
                    tricount += 1;
                    Debug.Log($"there are in this geom {veccount} points in the list and {tricount} triangles in the list");
                }

                //if only quad
                else if (exteriorBoundary.Count == 4 && surface.Count == 1)
                {
                    veccount += 4;
                    tricount += 2;
                }

                // all other forms
                else
                {
                    MeshData surfaceMeshData = GenerateSurfaceMeshData(surface, vArray);
                    preProcessedEarcut.Add(surface, surfaceMeshData);
                    veccount += surfaceMeshData.Vertices.Count;
                    for (int j = 0; j < surfaceMeshData.Triangles.Count; j++)
                    {
                        tricount += (surfaceMeshData.Triangles[j].Count / 3);
                    }
                }
            }
        }

        private static void CountSolid(JSONNode solid, Vector3[] vArray)
        {
            // get the surfaces of a solid and put them in the build surface function
            for (int i = 0; i < solid.Count; i++)
            {
                CountSurface(solid[i], vArray);
            }
        }

        private static void CountMSolid(JSONNode solids, Vector3[] vArray)
        {
            // get the different solids and put them in the build solid function
            for (int i = 0; i < solids.Count; i++)
            {
                CountSolid(solids[i], vArray);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //build function ///////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void buildarray(JSONNode geometries, Vector3[] vArray, Vector3[] vecList, int[] triList)
        {
            //head function it tells it where to go and what to do
            for (int l = 0; l < geometries.Count; l++)
            {
                var geometry = geometries[l];

                // do we need settings like this? maybe we can set them as atributes or someting like that
                var geometryType = geometry["type"].Value;
                var geometryLod = geometry["lod"].Value;
                var geometryBoundaries = geometry["boundaries"];
                
                // not needed for BAG
                var geometryTMatrix = geometry["transformationMatrix"];
                var geometrySemantics = geometry["Semantics"];
                
                // // not needed if you use clean version
                // if (geometryLod != "2.2")
                // {
                //     continue;
                // }

                // Special object implement later -> Not needed for BAG
                if (geometryType == "MultiPoint")
                {
                    Debug.LogError("not yet implemented");
                }

                // Special object implement later -> Not needed for BAG
                else if (geometryType == "MultiLineString")
                {
                    Debug.LogError("not yet implemented");
                }

                else if (geometryType == "MultiSurface" | geometryType == "CompositeSurface")
                {
                    buildSurfacearray(geometryBoundaries, vArray, vecList, triList);
                }

                else if (geometryType == "Solid")
                {
                    buildSolidarray(geometryBoundaries, vArray, vecList, triList);
                }
                else if (geometryType == "MultiSolid" | geometryType == "CompositeSolid")
                {
                    buildMSolidarray(geometryBoundaries, vArray, vecList, triList);
                }

                // Special object implement later -> Not needed for BAG
                else if (geometryType == "GeometryInstance")
                {
                    Debug.LogError("not yet implemented");
                }
            }
        }

        private static void buildSurfacearray(JSONNode surfaces, Vector3[] vArray, Vector3[] vecList, int[] triList)
        {
            // the real build class
            for (int i = 0; i < surfaces.Count; i++)
            {
                if (debugging) Debug.Log($"in round number {i} the number of triangles is {triList.Length / 3}");
                // get the surfaces
                var surface = surfaces[i];
                var exteriorBoundary = surface[0];

                if (exteriorBoundary.Count == 3 && surface.Count == 1)
                {
                    // continue;
                    //get number from original list
                    var vIndex0 = exteriorBoundary[0].AsInt;
                    var vIndex1 = exteriorBoundary[1].AsInt;
                    var vIndex2 = exteriorBoundary[2].AsInt;

                    //add triangle by adding first the vertices to vec list (CCW) an the reference them in triangle list
                    vecList[emptyvec] = (vArray[vIndex2]);
                    vecList[emptyvec + 1] = (vArray[vIndex1]);
                    vecList[emptyvec + 2] = (vArray[vIndex0]);
                    triList[emptytri] = (emptyvec);
                    triList[emptytri + 1] = (emptyvec + 1);
                    triList[emptytri + 2] = (emptyvec + 2);

                    emptyvec += 3;
                    emptytri += 3;
                }

                // //DELETE if I fix the rotation part and find out why normals sometimes are fucked up
                else if (exteriorBoundary.Count == 4 && surface.Count == 1)
                {
                    // continue;
                    // todo make triangle from quad surface, assumes that it doesn't have an ear --> maybe build in a check for that
                    //get number from original list
                    var vIndex0 = exteriorBoundary[0].AsInt;
                    var vIndex1 = exteriorBoundary[1].AsInt;
                    var vIndex2 = exteriorBoundary[2].AsInt;
                    var vIndex3 = exteriorBoundary[3].AsInt;

                    // //get reference length
                    int length = vecList.Length;

                    //add the vertices
                    vecList[emptyvec] = (vArray[vIndex3]);
                    vecList[emptyvec + 1] = (vArray[vIndex1]);
                    vecList[emptyvec + 2] = (vArray[vIndex0]);
                    vecList[emptyvec + 3] = (vArray[vIndex2]);


                    //add triangle by adding first the vertices to vec list (CCW) an the reference them in triangle list
                    triList[emptytri] = (emptyvec);
                    triList[emptytri + 1] = (emptyvec + 1);
                    triList[emptytri + 2] = (emptyvec + 2);

                    //add triangle by adding first the vertices to vec list (CCW) an the reference them in triangle list
                    triList[emptytri + 3] = (emptyvec + 1);
                    triList[emptytri + 4] = (emptyvec);
                    triList[emptytri + 5] = (emptyvec + 3);

                    emptyvec += 4;
                    emptytri += 6;
                }

                // for all other number and surfaces with holes
                else
                {
                    int currrentVert = emptyvec;

                    // check if earcut data is processed and retrieve it
                    if (preProcessedEarcut.ContainsKey(surface))
                    {
                        MeshData surfaceMeshData = preProcessedEarcut[surface];

                        // ad to the list
                        for (int j = 0; j < surfaceMeshData.Vertices.Count; j++)
                        {
                            vecList[emptyvec] = (surfaceMeshData.Vertices[j]);
                            emptyvec += 1;
                        }

                        for (int j = 0; j < surfaceMeshData.Triangles.Count; j++)
                        {
                            for (int k = 0; k < surfaceMeshData.Triangles[j].Count; k++)
                            {
                                triList[emptytri] = (surfaceMeshData.Triangles[j][k] + currrentVert);
                                emptytri += 1;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Surface not found");
                    }
                }
            }
        }

        private static void buildSolidarray(JSONNode solid, Vector3[] vArray, Vector3[] vecList, int[] triList)
        {
            // get the surfaces of a solid and put them in the build surface function
            for (int i = 0; i < solid.Count; i++)
            {
                // todo doesn't do anything different for interior surfaces --> Maybe make extra interior surface class where everything is clockwise instead of ccw
                buildSurfacearray(solid[i], vArray, vecList, triList);
            }
        }

        private static void buildMSolidarray(JSONNode solids, Vector3[] vArray, Vector3[] vecList, int[] triList)
        {
            // get the different solids and put them in the build solid function
            for (int i = 0; i < solids.Count; i++)
            {
                buildSolidarray(solids[i], vArray, vecList, triList);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //helper function //////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static MeshData GenerateSurfaceMeshData(JSONNode surface, Vector3[] vArray)
        {
            outside.Clear();
            if (outside.Count < surface[0].Count)
            {
                outside.Capacity = surface[0].Count;
            }

            holes.Clear();
            if (holes.Count < surface.Count - 1)
            {
                holes.Capacity = surface.Count - 1;
            }

            // outside en (later) holes hier opvullen
            for (int j = 0; j < surface.Count; j++)
            {
                var boundary = surface[j];

                // first one is the exterior boundary
                if (j == 0)
                {
                    for (int k = 0; k < boundary.Count; k++)
                    {
                        int vIndex = boundary[k].AsInt;
                        outside.Add(vArray[vIndex]);
                    }
                }
                // other wise holes to add to the hole list
                else
                {
                    newHole.Clear();
                    if (newHole.Count < boundary.Count)
                    {
                        newHole.Capacity = boundary.Count;
                    }

                    for (int k = 0; k < boundary.Count; k++)
                    {
                        int vIndex = boundary[k].AsInt;
                        newHole.Add(vArray[vIndex]);
                    }

                    if (newHole.Count > 0) holes.Add(newHole);
                }
            }

            // First the "easy" ones, are they somewehere on a same plane
            double threshold = 0.05;
            bool ytrue = true;
            for (int j = 0; j < outside.Count; j++)
            {
                if (outside[0].y - outside[j].y> threshold || outside[j].y - outside[0].y> threshold )
                {
                    ytrue = false;
                    break;
                }
            }

            MeshData surfaceMeshData;

            if (ytrue)
            {
                //without rotation
                surfaceMeshData = DATA.GenerateMeshdata(outside, holes);
            }
            else
            {
                // rotate the surface to y plane
                Vector3 n = normal(outside);
                Vector3 to = Vector3.up;

                rotate(outside, holes, n, to);

                surfaceMeshData = DATA.GenerateMeshdata(outside, holes);

                // rotate vertices back before adding to the list
                rotateBack(surfaceMeshData.Vertices, to, n);
            }

            return surfaceMeshData;
        }


        private static Vector3 normal(List<Vector3> exterior)
        {
            double maxDir = 0;
            Vector3 maxVec = Vector3.zero;
            for (int i = 1; i < exterior.Count; i++)
            {
                for (int j = 1; j < exterior.Count; j++)
                {
                    //     Pick 3 random polygon vertices
                    Vector3 a = exterior[0];
                    Vector3 b = exterior[i];
                    Vector3 c = exterior[j];

                    //     Calculate the normal of the according triangle
                    var dir = Vector3.Cross(b - a, c - a);
                    double magnitude = dir.magnitude;
                    
                    //     Choose the longest normal as the polygon's normal.
                    if (magnitude > maxDir)
                    {
                        maxDir = magnitude;
                        maxVec = dir;
                    }
                }
            }

            double threshold = 0;
            if (maxDir <= threshold)
            {
                Debug.LogWarning($"points are on a line so normal cant be calculated {maxDir.ToString()}");
                return Vector3.zero;
            }
            else
            {
                return -maxVec;
            }
        }

        private static void rotate(List<Vector3> exterior, List<List<Vector3>> holes, Vector3 from, Vector3 to)
        {
            Quaternion rotation = Quaternion.FromToRotation(from, to);
            for (int i = 0; i < exterior.Count; i++)
            {
                exterior[i] = rotation * exterior[i];
            }

            for (int i = 0; i < holes.Count; i++)
            {
                for (int j = 0; j < holes[i].Count; j++)
                {
                    holes[i][j] = rotation * holes[i][j];
                }
            }
        }

        private static void rotateBack(List<Vector3> vertices, Vector3 from, Vector3 to)
        {
            Quaternion rotation = Quaternion.FromToRotation(from, to);
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = rotation * vertices[i];
            }
        }
    }
}