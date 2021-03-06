﻿using System;
using System.Collections.Generic;
using System.Linq;
using ummisco.gama.unity.utils;
using UnityEditor;
using UnityEngine;

namespace Nextzen.Unity
{
    public class SceneGraph
    {
        // Merged mesh data for each game object group
        private Dictionary<GameObject, MeshData> gameObjectMeshData;

        // The root game object
        private GameObject regionMap;

        // Group options for grouping
        private SceneGroupType groupOptions;

        // The leaf group of the hierarchy
        private SceneGroupType leafGroup;

        // The game object options (physics, static, ...)
        private GameObjectOptions gameObjectOptions;

        // The list of feature mesh generated by the tile task
        private List<FeatureMesh> features;

        public SceneGraph(GameObject regionMap, SceneGroupType groupOptions, GameObjectOptions gameObjectOptions, List<FeatureMesh> features)
        {
            this.gameObjectMeshData = new Dictionary<GameObject, MeshData>();
            this.regionMap = regionMap;
            this.groupOptions = groupOptions;
            this.gameObjectOptions = gameObjectOptions;
            this.features = features;
            this.leafGroup = groupOptions.GetLeaf();
        }

        private GameObject AddGameObjectGroup(SceneGroupType groupType, GameObject parentGameObject, FeatureMesh featureMesh)
        {
            GameObject gameObject = null;

            string name = featureMesh.GetName(groupType);

            // No name for this group in the feature, use the parent name
            if (name.Length == 0)
            {
                name = parentGameObject.name;
            }

            Transform transform = parentGameObject.transform.Find(name);

            if (transform == null)
            {
                // No children for this name, create a new game object
                gameObject = new GameObject(name);
                gameObject.transform.parent = parentGameObject.transform;
            }
            else
            {
                // Reuse the game object found the the group name in the hierarchy
                gameObject = transform.gameObject;
            }


            return gameObject;
        }

        private void MergeMeshData(GameObject gameObject, FeatureMesh featureMesh)
        {
            // Merge the mesh data from the feature for the game object
            if (gameObjectMeshData.ContainsKey(gameObject))
            {
                gameObjectMeshData[gameObject].Merge(featureMesh.Mesh);
            }
            else
            {
                MeshData data = new MeshData();
                data.Merge(featureMesh.Mesh, true);
                gameObjectMeshData.Add(gameObject, data);

                Debug.Log("Game Object Created: " + gameObject.name);
                Debug.Log("         Its Vertices: " + data.MeshDataVerticesToString());
                Debug.Log("         Its UVs: " + data.MeshDataUVsToString());
                Debug.Log("         Its Submeshes: " + data.MeshDataSubmeshesToString());
            }
        }

        public void Generate()
        {
            // 1. Generate the game object hierarchy in the scene graph
            if (groupOptions == SceneGroupType.Nothing)
            {
                // Merge every game object created to the 'root' element being the map region
                foreach (var featureMesh in features)
                {
                    MergeMeshData(regionMap, featureMesh);

                }
            }
            else
            {
                GameObject currentGroup;

                // Generate all game object with the appropriate hiarchy
                foreach (var featureMesh in features)
                {
                    currentGroup = regionMap;

                    foreach (SceneGroupType group in Enum.GetValues(typeof(SceneGroupType)))
                    {
                        // Exclude 'nothing' and 'everything' group
                        if (group == SceneGroupType.Nothing || group == SceneGroupType.Everything)
                        {
                            continue;
                        }

                        if (groupOptions.Includes(group))
                        {
                            // Use currentGroup as the parentGroup for the current generation
                            var parentGroup = currentGroup;
                            var newGroup = AddGameObjectGroup(group, parentGroup, featureMesh);

                            // Top down of the hierarchy, merge the game objects
                            if (group == leafGroup)
                            {
                                MergeMeshData(newGroup, featureMesh);
                            }

                            currentGroup = newGroup;
                        }
                    }
                }
            }

            // 2. Initialize game objects and associate their components (physics, rendering)
            foreach (var pair in gameObjectMeshData)
            {
                var meshData = pair.Value;
                var root = pair.Key;

                // Create one game object per mesh object 'bucket', each bucket is ensured to
                // have less that 65535 vertices (valid under Unity mesh max vertex count).
                for (int i = 0; i < meshData.Meshes.Count; ++i)
                {
                    var meshBucket = meshData.Meshes[i];
                    GameObject gameObject;

                    if (meshData.Meshes.Count > 1)
                    {
                        gameObject = new GameObject(root.name + "_Part" + i);
                        gameObject.transform.parent = root.transform;
                    }
                    else
                    {
                        gameObject = root.gameObject;
                    }

                    gameObject.isStatic = gameObjectOptions.IsStatic;

                    var mesh = new Mesh();
                    mesh.Clear();
                    mesh.SetVertices(meshBucket.Vertices);
                    mesh.SetUVs(0, meshBucket.UVs);
                    mesh.subMeshCount = meshBucket.Submeshes.Count;
                    for (int s = 0; s < meshBucket.Submeshes.Count; s++)
                    {
                        mesh.SetTriangles(meshBucket.Submeshes[s].Indices, s);
                    }

                    //Unwrapping.GenerateSecondaryUVSet(mesh);

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    //MeshUtility.Optimize(mesh);

                    // Associate the mesh filter and mesh renderer components with this game object

                    var materials = meshBucket.Submeshes.Select(s => s.Material).ToArray();
                    var meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                    var meshRendererComponent = gameObject.AddComponent<MeshRenderer>();

                    meshRendererComponent.materials = materials;
                    meshFilterComponent.mesh = mesh;

                    if (gameObjectOptions.GeneratePhysicMeshCollider)
                    {
                        var meshColliderComponent = gameObject.AddComponent<MeshCollider>();
                        meshColliderComponent.material = gameObjectOptions.PhysicMaterial;
                        meshColliderComponent.sharedMesh = mesh;
                    }


                }
            }
        }

        public void DrawFromGama()
        {
            // 1. Generate the game object hierarchy in the scene graph
            if (groupOptions == SceneGroupType.Nothing)
            {
                // Merge every game object created to the 'root' element being the map region
                foreach (var featureMesh in features)
                {
                    MergeMeshData(regionMap, featureMesh);

                }
            }
            else
            {
                GameObject currentGroup;

                // Generate all game object with the appropriate hiarchy
                foreach (var featureMesh in features)
                {
                    currentGroup = regionMap;

                    foreach (SceneGroupType group in Enum.GetValues(typeof(SceneGroupType)))
                    {
                        // Exclude 'nothing' and 'everything' group
                        if (group == SceneGroupType.Nothing || group == SceneGroupType.Everything)
                        {
                            continue;
                        }

                        if (groupOptions.Includes(group))
                        {
                            // Use currentGroup as the parentGroup for the current generation
                            var parentGroup = currentGroup;
                            var newGroup = AddGameObjectGroup(group, parentGroup, featureMesh);

                            // Top down of the hierarchy, merge the game objects
                            if (group == leafGroup)
                            {
                                MergeMeshData(newGroup, featureMesh);
                            }

                            currentGroup = newGroup;
                        }
                    }
                }
            }

            // 2. Initialize game objects and associate their components (physics, rendering)
            foreach (var pair in gameObjectMeshData)
            {
                var meshData = pair.Value;
                var root = pair.Key;

                // Create one game object per mesh object 'bucket', each bucket is ensured to
                // have less that 65535 vertices (valid under Unity mesh max vertex count).
                for (int i = 0; i < meshData.Meshes.Count; ++i)
                {
                    var meshBucket = meshData.Meshes[i];
                    GameObject gameObject;

                    if (meshData.Meshes.Count > 1)
                    {
                        gameObject = new GameObject(root.name + "_Part" + i);
                        gameObject.transform.parent = root.transform;
                    }
                    else
                    {
                        gameObject = root.gameObject;
                    }

                    gameObject.isStatic = gameObjectOptions.IsStatic;

                    var mesh = new Mesh();

                    mesh.Clear();
                    //meshBucket.meshGeometry = "LineString";
                    //-----------------------------------
                    Debug.Log("Geometry ++------> " + meshBucket.gamaAgent.geometry);
                    Debug.Log("game Object Name >    " + gameObject.name);

                    if (meshBucket.gamaAgent.geometry.Equals("LineString"))
                    {
                        gameObject.AddComponent<LineRenderer>();
                        LineRenderer line = (LineRenderer)gameObject.GetComponent(typeof(LineRenderer));
                        line.positionCount = meshBucket.Vertices.Count;
                        line.SetPositions(meshBucket.Vertices.ToArray());
                        line.positionCount = meshBucket.Vertices.Count / 2;
                        Material mat = Utils.getMaterialByName("Green");
                        if (mat != null)
                        {
                            line.material = mat;
                        }

                        line.material = new Material(Shader.Find("Particles/Additive"));
                        Color c1 = Color.green;
                        Color c2 = new Color(1, 1, 1, 0);
                        line.startColor = c1;
                        line.endColor = c1;
                        line.startWidth = 5.0f;
                        line.endWidth = 5.0f;







                    }
                    else if (meshBucket.gamaAgent.geometry.Equals("Point"))
                    {
                        Transform tr = gameObject.transform.parent;
                        string name = gameObject.name;
                        gameObject.name = gameObject.name + "Old";

                        gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        gameObject.name = name;
                        gameObject.transform.parent = tr;
                        gameObject.transform.position = meshBucket.Vertices[0];
                        gameObject.transform.localScale = new Vector3(10, 10, 10);
                        //gameObject.AddComponent<LineRenderer>();
                        Color col = Utils.getColorFromGamaColor(meshBucket.gamaAgent.color);
                        
                         Renderer rend = gameObject.GetComponent<Renderer>();
                        rend.material.color = UnityEngine.Random.ColorHSV();// Color.green; //("green");//col;
                       
                        //col.a = 0.5f;
                        //rend.material.color = col;

                         Debug.Log(" =========>>>>>> the color is : " + col);
                    }
                    else if (meshBucket.gamaAgent.geometry.Equals("Polygon"))
                    {
                        mesh.SetVertices(meshBucket.Vertices);
                        mesh.SetUVs(0, meshBucket.UVs);
                        mesh.subMeshCount = meshBucket.Submeshes.Count;
                        for (int s = 0; s < meshBucket.Submeshes.Count; s++)
                        {
                            mesh.SetTriangles(meshBucket.Submeshes[s].Indices, s);

                        }
                        // Automatic Uvs Calculator
                        // meshBucket.setUvs();

                        Unwrapping.GenerateSecondaryUVSet(mesh);

                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        MeshUtility.Optimize(mesh);

                        // Associate the mesh filter and mesh renderer components with this game object



                        var materials = meshBucket.Submeshes.Select(s => s.Material).ToArray();
                        var meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                        var meshRendererComponent = gameObject.AddComponent<MeshRenderer>();


                        if(meshFilterComponent == null){
                            meshFilterComponent = gameObject.GetComponent<MeshFilter>();
                        }
                        if(meshRendererComponent == null) {
                            meshRendererComponent = gameObject.GetComponent<MeshRenderer>();
                        }

                        meshRendererComponent.materials = materials;
                        meshFilterComponent.mesh = mesh;




                        if (gameObjectOptions.GeneratePhysicMeshCollider)
                        {
                            var meshColliderComponent = gameObject.AddComponent<MeshCollider>();
                            meshColliderComponent.material = gameObjectOptions.PhysicMaterial;
                            meshColliderComponent.sharedMesh = mesh;
                        }
 
                        
                        Renderer rend = gameObject.GetComponent<Renderer>();
                        
                        //Set the main Color of the Material to green
                        rend.material.shader = Shader.Find("_Color");
                        rend.material.SetColor("_Color", Color.green);

                        //Find the Specular shader and change its Color to red
                        rend.material.shader = Shader.Find("Specular");
                        rend.material.SetColor("_SpecColor", Color.red);
                        

                        Material material = new Material(Shader.Find("Standard"));
                        material.color = Color.blue;
                        material.color = UnityEngine.Random.ColorHSV();

                        // assign the material to the renderer
                        gameObject.GetComponent<Renderer>().material = material;
                        
                    }


                }
            }
        }

        private void Destroy(GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        private Mesh DrawLineMesh(List<Vector3> points)
        {
            Mesh m = new Mesh();
            m.Clear();
            Vector3[] verticies = new Vector3[points.Count];

            for (int i = 0; i < verticies.Length; i++)
            {
                verticies[i] = points[i];
            }

            int[] triangles = new int[((points.Count / 2) - 1) * 6];
            //Works on linear patterns tn = bn+c
            int position = 6;
            for (int i = 0; i < (triangles.Length / 6); i++)
            {
                Debug.Log("Triangles are: " + i);
                triangles[i * position] = 2 * i;
                triangles[i * position + 3] = 2 * i;

                triangles[i * position + 1] = 2 * i + 3;
                triangles[i * position + 4] = (2 * i + 3) - 1;

                triangles[i * position + 2] = 2 * i + 1;
                triangles[i * position + 5] = (2 * i + 1) + 2;
            }

            m.vertices = verticies;
            m.triangles = triangles;
            Unwrapping.GenerateSecondaryUVSet(m);
            m.RecalculateNormals();
            m.RecalculateBounds();
            MeshUtility.Optimize(m);
            Debug.Log("Triangles are: " + triangles.ToString());
            return m;
        }
    }
}