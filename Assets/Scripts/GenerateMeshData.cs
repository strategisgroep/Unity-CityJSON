using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers;
using SimpleJSONOptimized;
using UnityEngine;



namespace meshData
{
    public static class DATA
    {
        private static List<Vector3> _outside = new List<Vector3>();
        private static List<List<Vector3>> _holes = new List<List<Vector3>>();
        private static List<List<Vector3>> subset = new List<List<Vector3>>(); 
        private static List<int> result;
        private static List<int> triList;
        private static List<Vector3> sub;
        
        public static MeshData GenerateMeshdata(List<Vector3> points, List<List<Vector3>> holes)
        {
            MeshData mesh = new MeshData();
            _outside.Clear();
            if (_outside.Count < points.Count)
            {
                _outside.Capacity = points.Count;
            }
            
            _holes.Clear();
            if (_holes.Count < holes.Count)
            {
                _holes.Capacity = holes.Count;
            }
            
            int numpoints = points.Count;
            int numHoles = holes.Count;

            for (int i = 0; i < numpoints; i++) {
                
                _outside.Add(points[i]);
            }
            
            for (int i = 0; i < numHoles; i++) {
                numpoints = holes[i].Count;

                _holes.Add(new List<Vector3>(numpoints));

                for (int j = 0; j < numpoints; j++) {
                    _holes[i].Add(holes[i][j]);
                }
                if (IsClockwiseV3(_holes[i])) {
                    _holes[i].Reverse();
                }
            }
            _holes = _holes.OrderBy(x => x.Count).ToList();

            int _secondCounter = 1 + _holes.Count;
            subset.Clear();
            if (subset.Count < _secondCounter)
            {
                subset.Capacity = _secondCounter;
            }
            
            Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers.Data flatData = null;
            List<int> result = null;
            List<int> triList = null;
            List<Vector3> sub = null;
            
            
            int currentIndex = 0;
            int vertCount = 0, c2 = 0;
            

            for (int i = 0; i < _secondCounter; i++) {
                if (i == 0) {
                    sub = _outside;
                } else {
                    sub = _holes[i - 1];
                }

                vertCount = mesh.Vertices.Count;
                if (IsClockwiseV3(sub) && vertCount > 0) {
                    flatData = EarcutLibrary.Flatten(subset);
                    result = EarcutLibrary.Earcut(flatData.Vertices, flatData.Holes, flatData.Dim);
                    c2 = result.Count;
                    if (triList == null) {
                        triList = new List<int>(c2);
                    } else {
                        triList.Capacity = triList.Count + c2;
                    }

                    for (int j = 0; j < c2; j++) {
                        triList.Add(result[j] + currentIndex);
                    }
                    currentIndex = vertCount;
                    subset.Clear();
                }

                subset.Add(sub);

                c2 = sub.Count;
                mesh.Vertices.Capacity = mesh.Vertices.Count + c2;
                mesh.Normals.Capacity = mesh.Normals.Count + c2;
                mesh.Edges.Capacity = mesh.Edges.Count + c2 * 2;



                for (int j = 0; j < c2; j++) {
                    mesh.Edges.Add(vertCount + ((j + 1) % c2));
                    mesh.Edges.Add(vertCount + j);
                    mesh.Vertices.Add(sub[j]);
                }
            }

            flatData = EarcutLibrary.Flatten(subset);
            result = EarcutLibrary.Earcut(flatData.Vertices, flatData.Holes, flatData.Dim);
            c2 = result.Count;
            if (triList == null) {
                triList = new List<int>(c2);
            } else {
                triList.Capacity = triList.Count + c2;
            }
            for (int i = 0; i < c2; i++) {
                triList.Add(result[i] + currentIndex);
            }

            mesh.Triangles.Add(triList);
            return mesh;
        }

        public static bool IsClockwiseV3(IList<Vector3> vertices) {
            float sum = 0.0f;
            for(int i = 0; i < vertices.Count; i++) {
                Vector3 v1 = vertices[i];
                Vector3 v2 = vertices[(i + 1) % vertices.Count]; // % is the modulo operator
                sum += (v2.x - v1.x) * (v2.z + v1.z);
            }
            return sum > 0.0f;
        }
    }
}

