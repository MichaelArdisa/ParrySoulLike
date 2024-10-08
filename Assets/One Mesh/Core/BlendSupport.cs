/*
One Mesh by ACE STUDIO X
Copyright (c) 2024 ACE STUDIO X
All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AceStudioX.OneMesh
{
    public static class BlendSupport
    {
        private static Dictionary<Mesh, Mesh> readableMeshCache = new Dictionary<Mesh, Mesh>();

        public static void Combine(GameObject rootObj, Transform finalRootBone, string savePath)
        {
            try
            {
                var smRenderers = rootObj.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smRenderers.Length == 0)
                {
                    Debug.LogWarning($"No SkinnedMeshRenderers found in {rootObj.name}.");
                    return;
                }

                var combBones = new List<Transform>();
                var combBindPoses = new List<Matrix4x4>();
                var combBlendShapeData = new Dictionary<string, BlendShapeData>();
                var allSubmeshes = new List<SubmeshData>();

                int vertexOffset = 0;
                int boneOffset = 0;
                int totalVertexCount = 0;

                foreach (var smr in smRenderers)
                {
                    var mesh = smr.sharedMesh;
                    if (!ValidateMesh(smr, mesh))
                    {
                        Debug.LogError($"Validation failed for mesh on {smr.name}. Aborting combination.");
                        return;
                    }

                    totalVertexCount += mesh.vertexCount;

                    for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; subMeshIdx++)
                    {
                        allSubmeshes.Add(new SubmeshData
                        {
                            Mesh = mesh,
                            SubMeshIndex = subMeshIdx,
                            Material = GetMatForSubmesh(smr, subMeshIdx),
                            Transform = smr.transform.localToWorldMatrix,
                            VertexOffset = vertexOffset,
                            BoneOffset = boneOffset
                        });
                    }

                    for (int i = 0; i < mesh.blendShapeCount; i++)
                    {
                        string blendShapeName = mesh.GetBlendShapeName(i);
                        if (!combBlendShapeData.ContainsKey(blendShapeName))
                        {
                            combBlendShapeData[blendShapeName] = new BlendShapeData(totalVertexCount);
                        }
                    }

                    if (!ProcBlendShapes(smr, mesh, ref combBlendShapeData, vertexOffset))
                    {
                        Debug.LogError($"Failed to process blend shapes for {smr.name}. Aborting combination.");
                        return;
                    }

                    vertexOffset += mesh.vertexCount;
                    boneOffset += smr.bones.Length;
                    combBones.AddRange(smr.bones);
                    combBindPoses.AddRange(mesh.bindposes);
                }

                UniqueBlendshapeWeights(ref combBlendShapeData);

                CombAndSave(rootObj, smRenderers, combBones, finalRootBone, allSubmeshes, combBindPoses, combBlendShapeData, savePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in Combine: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static bool ValidateMesh(SkinnedMeshRenderer smr, Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogWarning($"Mesh on {smr.name} is null.");
                return false;
            }

            if (!mesh.isReadable)
            {
                mesh = MakeMeshReadable(mesh);
            }

            if (mesh == null)
            {
                Debug.LogWarning($"Mesh on {smr.name} could not be made readable.");
                return false;
            }

            bool missingAttributes = false;

            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Position))
            {
                Debug.LogWarning($"Mesh on {smr.name} is missing Position attribute.");
                missingAttributes = true;
            }

            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal))
            {
                Debug.LogWarning($"Mesh on {smr.name} is missing Normal attribute. Adding default normals.");
                mesh.RecalculateNormals();
            }

            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))
            {
                mesh.RecalculateTangents();
            }

            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0))
            {
                Debug.LogWarning($"Mesh on {smr.name} is missing TexCoord0 attribute.");
                missingAttributes = true;
            }

            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color))
            {
                VertexColors(mesh);
            }

            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1))
            {
                AddDefaultUV2(mesh);
            }

            return !missingAttributes;
        }

        private static Mesh MakeMeshReadable(Mesh originalMesh)
        {
            if (readableMeshCache.TryGetValue(originalMesh, out var readableMesh))
            {
                return readableMesh;
            }

            if (!originalMesh.isReadable) //Ace Studio X Joe
            {
                readableMesh = Object.Instantiate(originalMesh);
                readableMesh.name = originalMesh.name + "_readable";
                readableMeshCache[originalMesh] = readableMesh;
                return readableMesh;
            }

            return originalMesh;
        }

        private static void VertexColors(Mesh mesh)
        {
            Color[] defaultColors = new Color[mesh.vertexCount];
            for (int i = 0; i < defaultColors.Length; i++)
            {
                defaultColors[i] = Color.white;
            }
            mesh.colors = defaultColors;
        }

        private static void AddDefaultUV2(Mesh mesh)
        {
            Vector2[] defaultUV2 = new Vector2[mesh.vertexCount];
            for (int i = 0; i < defaultUV2.Length; i++)
            {
                defaultUV2[i] = Vector2.zero;
            }
            mesh.uv2 = defaultUV2;
        }

        private static bool ProcBlendShapes(SkinnedMeshRenderer smr, Mesh mesh, ref Dictionary<string, BlendShapeData> combBlendShapeData, int vertexOffset)
        {
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string blendShapeName = mesh.GetBlendShapeName(i);
                if (!combBlendShapeData.TryGetValue(blendShapeName, out var blendShapeData))
                {
                    blendShapeData = new BlendShapeData(mesh.vertexCount);
                    combBlendShapeData[blendShapeName] = blendShapeData;
                }

                for (int frame = 0; frame < mesh.GetBlendShapeFrameCount(i); frame++)
                {
                    float frameWeight = mesh.GetBlendShapeFrameWeight(i, frame);
                    Vector3[] vertices = new Vector3[mesh.vertexCount];
                    Vector3[] normals = new Vector3[mesh.vertexCount];
                    Vector3[] tangents = new Vector3[mesh.vertexCount];
                    mesh.GetBlendShapeFrameVertices(i, frame, vertices, normals, tangents);
                    blendShapeData.AddFrameData(frameWeight, vertices, normals, tangents, vertexOffset, mesh.vertexCount);
                }
            }
            return true;
        }

        private static void UniqueBlendshapeWeights(ref Dictionary<string, BlendShapeData> combBlendShapeData)
        {
            foreach (var blendShape in combBlendShapeData.Values)
            {
                float lastWeight = -1f;
                int adjustmentCount = 0;
                foreach (var frame in blendShape.frames)
                {
                    if (frame.weight <= lastWeight)
                    {
                        frame.weight = lastWeight + 0.001f;
                        adjustmentCount++;
                    }
                    lastWeight = frame.weight;
                }

                if (adjustmentCount > blendShape.frames.Count * 0.5f)
                {
                    Debug.LogWarning($"Too many blend shape weight adjustments for {blendShape}. Consider reviewing the blend shapes manually.");
                }
            }
        }

        private static void TransformVertices(Mesh mesh, Transform meshTransform, Transform rootTransform)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;

            Matrix4x4 localToWorldMatrix = meshTransform.localToWorldMatrix;
            Matrix4x4 worldToRootLocalMatrix = rootTransform.worldToLocalMatrix;
            Matrix4x4 transformMatrix = worldToRootLocalMatrix * localToWorldMatrix;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transformMatrix.MultiplyPoint3x4(vertices[i]);
                normals[i] = transformMatrix.MultiplyVector(normals[i]).normalized;
                Vector3 tangent = transformMatrix.MultiplyVector(tangents[i]);
                tangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, tangents[i].w);
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;

            mesh.RecalculateBounds();
        }

        private static void CombAndSave(GameObject rootObj, SkinnedMeshRenderer[] smRenderers, List<Transform> bones, Transform finalRootBone, List<SubmeshData> allSubmeshes, List<Matrix4x4> bindPoses, Dictionary<string, BlendShapeData> blendShapeData, string savePath)
        {
            try
            {
                var finalCombMesh = new Mesh();
                finalCombMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                var combineInstances = new List<CombineInstance>();
                var finalMaterials = new List<Material>();
                var finalBoneWeights = new List<BoneWeight>();
                var finalBindPoses = new List<Matrix4x4>();

                int boneIndexOffset = 0;
                foreach (var smr in smRenderers)
                {
                    Mesh mesh = Object.Instantiate(smr.sharedMesh);
                    TransformVertices(mesh, smr.transform, rootObj.transform);

                    for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
                    {
                        combineInstances.Add(new CombineInstance
                        {
                            mesh = mesh,
                            subMeshIndex = subMeshIndex,
                            transform = Matrix4x4.identity
                        });

                        finalMaterials.Add(smr.sharedMaterials[subMeshIndex]);
                    }

                    BoneWeight[] boneWeights = mesh.boneWeights;
                    for (int i = 0; i < boneWeights.Length; i++)
                    {
                        boneWeights[i].boneIndex0 += boneIndexOffset;
                        boneWeights[i].boneIndex1 += boneIndexOffset;
                        boneWeights[i].boneIndex2 += boneIndexOffset;
                        boneWeights[i].boneIndex3 += boneIndexOffset;
                    }
                    finalBoneWeights.AddRange(boneWeights);

                    Matrix4x4 rootLocalToWorld = rootObj.transform.localToWorldMatrix;
                    for (int i = 0; i < smr.bones.Length; i++)
                    {
                        Matrix4x4 boneLocalToWorld = smr.bones[i].localToWorldMatrix;
                        Matrix4x4 boneWorldToRootLocal = rootObj.transform.worldToLocalMatrix * boneLocalToWorld;
                        finalBindPoses.Add(boneWorldToRootLocal.inverse);
                    }

                    boneIndexOffset += smr.bones.Length;
                }

                finalCombMesh.CombineMeshes(combineInstances.ToArray(), false, true);
                finalCombMesh.boneWeights = finalBoneWeights.ToArray();
                finalCombMesh.bindposes = finalBindPoses.ToArray();

                foreach (var kvp in blendShapeData)
                {
                    string blendShapeName = kvp.Key;
                    var data = kvp.Value;
                    data.frames.Sort((a, b) => a.weight.CompareTo(b.weight));

                    foreach (var frame in data.frames)
                    {
                        if (frame.vertices.Length != finalCombMesh.vertexCount)
                        {
                            frame.AdjustFrame(finalCombMesh.vertexCount);
                        }

                        finalCombMesh.AddBlendShapeFrame(blendShapeName, frame.weight, frame.vertices, frame.normals, frame.tangents);
                    }
                }

                SaveMesh(finalCombMesh, rootObj.name, savePath);

                if (finalRootBone == null)
                {
                    finalRootBone = FindRoot(rootObj.transform);
                }

                CreateSkinnedRenderer(rootObj, finalCombMesh, bones, finalRootBone, finalMaterials.ToArray());

                DisableOriginal(smRenderers);

                Debug.Log($"Meshes combined successfully for {rootObj.name}.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in CombAndSave: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void CreateSkinnedRenderer(GameObject rootObj, Mesh combMesh, List<Transform> bones, Transform root, Material[] mats)
        {
            try
            {
                GameObject combObj = new GameObject("CombinedSkinnedMesh_" + rootObj.name);
                combObj.transform.SetParent(rootObj.transform);
                combObj.transform.localPosition = Vector3.zero;
                combObj.transform.localRotation = Quaternion.identity;
                combObj.transform.localScale = Vector3.one;

                SkinnedMeshRenderer combSkinnedRenderer = combObj.AddComponent<SkinnedMeshRenderer>();

                combSkinnedRenderer.sharedMesh = combMesh;
                combSkinnedRenderer.bones = bones.ToArray();
                combSkinnedRenderer.rootBone = root;
                combSkinnedRenderer.materials = mats;

                combSkinnedRenderer.localBounds = combMesh.bounds;

                combSkinnedRenderer.updateWhenOffscreen = true;

                Debug.Log($"Created combined SkinnedMeshRenderer for {rootObj.name} with {combMesh.vertexCount} vertices, {combMesh.subMeshCount} submeshes, and {bones.Count} bones.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in CreateSkinnedRenderer: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static Material GetMatForSubmesh(SkinnedMeshRenderer smr, int subMeshIdx)
        {
            if (subMeshIdx < smr.sharedMaterials.Length)
            {
                Material material = smr.sharedMaterials[subMeshIdx];
                if (material.shader.name != "Standard" && material.shader.name != "URP/Lit" && material.shader.name != "HDRP/Lit")
                {
                    return new Material(Shader.Find("Standard"));
                }
                return material;
            }
            else
            {
                return new Material(Shader.Find("Standard"));
            }
        }

        private static void SaveMesh(Mesh combMesh, string objName, string savePath)
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(savePath))
                {
                    string parentFolder = Path.GetDirectoryName(savePath);
                    if (!AssetDatabase.IsValidFolder(parentFolder))
                    {
                        string[] folders = savePath.Split('/');
                        string currentPath = folders[0];
                        for (int i = 1; i < folders.Length; i++)
                        {
                            string folderPath = currentPath + "/" + folders[i];
                            if (!AssetDatabase.IsValidFolder(folderPath))
                            {
                                AssetDatabase.CreateFolder(currentPath, folders[i]);
                            }
                            currentPath = folderPath;
                        }
                    }
                    else
                    {
                        string newFolder = Path.GetFileName(savePath);
                        AssetDatabase.CreateFolder(parentFolder, newFolder);
                    }
                }

                string assetPath = $"{savePath}/{objName}_CombinedMesh.asset";
                AssetDatabase.CreateAsset(combMesh, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in SaveMesh: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void DisableOriginal(SkinnedMeshRenderer[] smRenderers)
        {
            foreach (var smr in smRenderers)
            {
                smr.enabled = false;
            }
        }

        private static Transform FindRoot(Transform root)
        {
            foreach (var bone in root.GetComponentsInChildren<Transform>())
            {
                if (bone.name.ToLower().Contains("hip") || bone.name.ToLower().Contains("pelvis"))
                {
                    return bone;
                }
            }
            return root;
        }
    }

    public class BlendShapeData
    {
        public List<FrameData> frames = new List<FrameData>();
        private int vertexCount;

        public BlendShapeData(int totalVertexCount)
        {
            vertexCount = totalVertexCount;
        }

        public void AddFrameData(float weight, Vector3[] vertices, Vector3[] normals, Vector3[] tangents, int vertexOffset, int submeshVertexCount)
        {
            Vector3[] adjVerts = new Vector3[vertexCount];
            Vector3[] adjNorms = new Vector3[vertexCount];
            Vector3[] adjTangs = new Vector3[vertexCount];

            for (int i = 0; i < submeshVertexCount; i++)
            {
                adjVerts[i + vertexOffset] = vertices[i];
                adjNorms[i + vertexOffset] = normals[i];
                adjTangs[i + vertexOffset] = tangents[i];
            }

            frames.Add(new FrameData
            {
                weight = weight,
                vertices = adjVerts,
                normals = adjNorms,
                tangents = adjTangs
            });
        }
    }

    public class FrameData
    {
        public float weight;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector3[] tangents;

        public void AdjustFrame(int expectedVertexCount)
        {
            if (vertices.Length != expectedVertexCount)
            {
                var newVertices = new Vector3[expectedVertexCount];
                var newNormals = new Vector3[expectedVertexCount];
                var newTangents = new Vector3[expectedVertexCount];

                System.Array.Copy(vertices, newVertices, Mathf.Min(vertices.Length, expectedVertexCount));
                System.Array.Copy(normals, newNormals, Mathf.Min(normals.Length, expectedVertexCount));
                System.Array.Copy(tangents, newTangents, Mathf.Min(tangents.Length, expectedVertexCount));

                for (int i = vertices.Length; i < expectedVertexCount; i++)
                {
                    newVertices[i] = Vector3.zero;
                    newNormals[i] = Vector3.zero;
                    newTangents[i] = Vector3.zero;
                }

                vertices = newVertices;
                normals = newNormals; //Ace Studio X Joe
                tangents = newTangents;
            }
        }
    }

    public class SubmeshData
    {
        public Mesh Mesh;
        public int SubMeshIndex;
        public Material Material;
        public Matrix4x4 Transform;
        public int VertexOffset;
        public int BoneOffset;
    }
}