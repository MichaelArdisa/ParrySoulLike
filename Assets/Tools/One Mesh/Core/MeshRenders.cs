/*
One Mesh by ACE STUDIO X
Copyright (c) 2024 ACE STUDIO X
All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AceStudioX.OneMesh
{
    public static class MeshRendererCombiner
    {
        public static void CombineMeshRenderers(GameObject rootObj, string savePath)
        {
            if (rootObj == null)
            {
                Debug.LogError("Root object is null.");
                return;
            }

            var meshRenders = rootObj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenders.Length == 0)
            {
                Debug.LogWarning($"No MeshRenderers found in the root object {rootObj.name}.");
                return;
            }

            var matToCombInst = new Dictionary<Material, List<CombineInstance>>();
            foreach (var meshRender in meshRenders)
            {
                var meshFilt = meshRender.GetComponent<MeshFilter>();
                if (meshFilt == null || meshFilt.sharedMesh == null)
                {
                    continue;
                }

                var mesh = meshFilt.sharedMesh;
                for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; subMeshIdx++)
                {
                    if (subMeshIdx >= meshRender.sharedMaterials.Length)
                    {
                        Debug.LogWarning($"SubMesh index {subMeshIdx} is out of range for materials on {meshRender.name}.");
                        continue;
                    }

                    var mat = meshRender.sharedMaterials[subMeshIdx];
                    if (!matToCombInst.ContainsKey(mat))
                    {
                        matToCombInst[mat] = new List<CombineInstance>();
                    }

                    var combInst = new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = subMeshIdx,
                        transform = rootObj.transform.worldToLocalMatrix * meshRender.transform.localToWorldMatrix
                    };

                    matToCombInst[mat].Add(combInst);
                }
            }

            if (!CreateCombMesh(matToCombInst, rootObj.name, savePath, out var combMesh, out var combMats))
            {
                return;
            }

            CreateCombMeshRender(rootObj, combMesh, combMats.ToArray());

            DisableOrigMeshRenders(meshRenders);

            Debug.Log($"Mesh renderers combined successfully for {rootObj.name}.");
        }

        private static bool CreateCombMesh(Dictionary<Material, List<CombineInstance>> matToCombInst, string objName, string savePath, out Mesh combMesh, out List<Material> combMats)
        {
            combMesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            combMats = new List<Material>();

            var combSubMeshes = new List<CombineInstance>();
            foreach (var kvp in matToCombInst)
            {
                var mat = kvp.Key;
                var combInstForMat = kvp.Value;

                var subMesh = new Mesh();
                subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                subMesh.CombineMeshes(combInstForMat.ToArray(), true, true);

                combSubMeshes.Add(new CombineInstance
                {
                    mesh = subMesh,
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity
                });

                combMats.Add(mat);
            }

            combMesh.CombineMeshes(combSubMeshes.ToArray(), false, false);

            if (!SaveCombMeshAsAsset(combMesh, objName, savePath))
            {
                return false;
            }

            
            foreach (var combInst in combSubMeshes)
            {
                Object.DestroyImmediate(combInst.mesh);
            }

            return true;
        }

        private static bool SaveCombMeshAsAsset(Mesh combMesh, string objName, string savePath)
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(savePath))
                {
                    var parentFolder = Path.GetDirectoryName(savePath);
                    var newFolder = Path.GetFileName(savePath);
                    AssetDatabase.CreateFolder(parentFolder, newFolder);
                }

                var assetPath = $"{savePath}/{objName}_CombinedMesh.asset";
                AssetDatabase.CreateAsset(combMesh, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Saved combined mesh as asset at: {assetPath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save combined mesh asset: {ex.Message}");
                return false;
            }
        }

        private static GameObject CreateCombMeshRender(GameObject rootObj, Mesh combMesh, Material[] mats)
        {
            var combObj = new GameObject($"CombinedMesh_{rootObj.name}")
            {
                transform =
                {
                    parent = rootObj.transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };

            var combMeshFilt = combObj.AddComponent<MeshFilter>(); // Ace Studio X Joe
            combMeshFilt.mesh = combMesh;

            var combMeshRender = combObj.AddComponent<MeshRenderer>();
            combMeshRender.materials = mats;

            Debug.Log($"Created combined MeshRenderer for {rootObj.name} with {combMesh.vertexCount} vertices and {combMesh.subMeshCount} submeshes.");

            return combObj;
        }

        private static void DisableOrigMeshRenders(MeshRenderer[] meshRenders)
        {
            foreach (var meshRender in meshRenders)
            {
                meshRender.enabled = false;
            }
        }
    }
}
