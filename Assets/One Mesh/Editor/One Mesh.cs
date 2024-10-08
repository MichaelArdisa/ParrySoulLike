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
    public class OneMeshTool : EditorWindow
    {
        [SerializeField]
        private List<GameObject> targetObjs = new List<GameObject>();
        [SerializeField]
        private Transform rootBone;
        [SerializeField]
        private bool combineBlendshapes = false;
        private string savePath = "Assets/One Mesh Assets";

        private SerializedObject so;
        private SerializedProperty targetObjsProp;
        private SerializedProperty rootBoneProp;
        private SerializedProperty combineBlendshapesProp;

        [MenuItem("Tools/One Mesh/One Mesh 2.1")]
        public static void ShowWindow()
        {
            GetWindow<OneMeshTool>("One Mesh 2.1");
        }

        private void OnEnable()
        {
            so = new SerializedObject(this);
            targetObjsProp = so.FindProperty("targetObjs");
            rootBoneProp = so.FindProperty("rootBone");
            combineBlendshapesProp = so.FindProperty("combineBlendshapes");
        }

        private void OnGUI()
        {
            so.Update();

            EditorGUILayout.LabelField("One Mesh 2.1 - Ace Studio X", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawTargetObjsSection();
            DrawRootBoneSection();
            DrawBlendshapeToggle();
            DrawSavePathSection();
            DrawActionsSection();

            so.ApplyModifiedProperties();
        }

        private void DrawTargetObjsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Target Objects", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(targetObjsProp, new GUIContent("Objects", "The objects for which combined skinned meshes will be generated"), true);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawRootBoneSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Root Bone", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rootBoneProp, new GUIContent("Assigned Root Bone", "The root bone to be used for the combined mesh"));
            if (GUILayout.Button(new GUIContent("Remove Root Bone", "Remove the currently assigned root bone")))
            {
                rootBone = null;
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawBlendshapeToggle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Blendshapes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(combineBlendshapesProp, new GUIContent("Combine Blendshapes", "Enable blendshape support"));
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawSavePathSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Save Folder", EditorStyles.boldLabel);
            savePath = EditorGUILayout.TextField(new GUIContent("Save Folder Path", "Path to save the combined meshes"), savePath);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Browse", "Browse for the save folder"), GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Save Folder", savePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    savePath = FileUtil.GetProjectRelativePath(selectedPath);
                    if (string.IsNullOrEmpty(savePath))
                    {
                        savePath = "Assets/OneMesh";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawActionsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Combine Skinned Meshes", "Combine skinned meshes for the selected target objects"), GUILayout.MinWidth(150)))
            {
                if (combineBlendshapes)
                {
                    CombineAllSkinnedWithBlendshapes();
                }
                else
                {
                    CombineAllSkinned();
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Combine Mesh Renderers", "Combine mesh renderers for the selected target objects"), GUILayout.MinWidth(150)))
            {
                CombineAllMesh();
            }
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Enable All Skinned Mesh Renderers", "Enable all skinned mesh renderers for the selected target objects"), GUILayout.MinWidth(150)))
            {
                EnableAllSkinned();
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Enable All Mesh Renderers", "Enable all mesh renderers for the selected target objects"), GUILayout.MinWidth(150)))
            {
                EnableAllMesh();
            }
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Remove Combined Mesh", "Remove the combined mesh from the selected target objects"), GUILayout.Height(30))) //Ace Studio X Joe
            {
                RemoveAllCombined();
            }

            EditorGUILayout.EndVertical();
        }

        private void CombineAllSkinned()
        {
            foreach (var targetObj in targetObjs)
            {
                if (targetObj != null)
                {
                    CombineSkinned(targetObj);
                }
                else
                {
                    ShowError("Please assign all target objects.");
                    return;
                }
            }
        }

        private void CombineAllSkinnedWithBlendshapes()
        {
            foreach (var targetObj in targetObjs)
            {
                if (targetObj != null)
                {
                    AceStudioX.OneMesh.BlendSupport.Combine(targetObj, rootBone, savePath);
                }
                else
                {
                    ShowError("Please assign all target objects.");
                    return;
                }
            }
        }

        private void CombineSkinned(GameObject rootObj)
        {
            ApplyTransforms(rootObj);

            var smRenderers = rootObj.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

            if (smRenderers.Count == 0)
            {
                Debug.LogWarning($"No SkinnedMeshRenderers found in {rootObj.name}.");
                return;
            }

            var combinedBones = new List<Transform>();
            var combinedBindPoses = new List<Matrix4x4>();
            var combinedMats = new List<Material>();

            var matToCombineInstances = new Dictionary<Material, List<CombineInstance>>();
            var matToBoneWeights = new Dictionary<Material, List<BoneWeight>>();
            var combinedBoneWeights = new List<BoneWeight>();

            Transform root = rootBone != null ? rootBone : FindRoot(rootObj.transform);
            Matrix4x4 rootMatrix = root?.localToWorldMatrix ?? Matrix4x4.identity;

            int boneOffset = 0;

            foreach (var smr in smRenderers)
            {
                var mesh = smr.sharedMesh;

                if (!ValidateMesh(smr, mesh))
                    continue;

                if (mesh.vertexCount > 65535)
                {
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }

                Matrix4x4 matrix = rootMatrix * smr.transform.localToWorldMatrix;
                ProcessSubMeshes(smr, mesh, ref boneOffset, matToCombineInstances, matToBoneWeights, matrix);

                boneOffset += smr.bones.Length;
                combinedBones.AddRange(smr.bones);
                combinedBindPoses.AddRange(mesh.bindposes);
            }

            CombineAndSave(rootObj, smRenderers, combinedBones, root, combinedMats, combinedBindPoses, matToCombineInstances, matToBoneWeights);
        }

        private void CombineAllMesh()
        {
            foreach (var targetObj in targetObjs)
            {
                if (targetObj != null)
                {
                    MeshRendererCombiner.CombineMeshRenderers(targetObj, savePath);
                }
                else
                {
                    ShowError("Please assign all target objects.");
                    return;
                }
            }
        }

        private Transform FindRoot(Transform root)
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

        private bool ValidateMesh(SkinnedMeshRenderer smr, Mesh mesh)
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

            if (mesh.boneWeights.Length == 0 || mesh.bindposes.Length == 0)
            {
                Debug.LogWarning($"Mesh on {smr.name} is missing bone weights or bind poses.");
                return false;
            }

            
            if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Position) ||
                !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal) ||
                !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent) ||
                !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0))
            {
                Debug.LogWarning($"Mesh on {smr.name} is missing required vertex attributes.");
                return false;
            }

            return true;
        }

        private void ProcessSubMeshes(SkinnedMeshRenderer smr, Mesh mesh, ref int boneOffset, Dictionary<Material, List<CombineInstance>> matToCombineInstances, Dictionary<Material, List<BoneWeight>> matToBoneWeights, Matrix4x4 matrix)
        {
            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                if (subMeshIndex >= smr.sharedMaterials.Length)
                {
                    Debug.LogWarning($"SubMesh index {subMeshIndex} is out of range for materials on {smr.name}.");
                    continue;
                }

                var mat = smr.sharedMaterials[subMeshIndex];

                if (!matToCombineInstances.ContainsKey(mat))
                {
                    matToCombineInstances[mat] = new List<CombineInstance>();
                    matToBoneWeights[mat] = new List<BoneWeight>();
                }

                matToCombineInstances[mat].Add(new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = subMeshIndex,
                    transform = matrix
                });

                var currentBoneWeights = mesh.boneWeights;
                if (currentBoneWeights.Length != mesh.vertexCount)
                {
                    Debug.LogWarning($"Bone weights count does not match vertex count on {smr.name}. Skipping subMesh {subMeshIndex}.");
                    continue;
                }

                for (int i = 0; i < currentBoneWeights.Length; i++)
                {
                    var bw = currentBoneWeights[i];
                    bw.boneIndex0 += boneOffset;
                    bw.boneIndex1 += boneOffset;
                    bw.boneIndex2 += boneOffset;
                    bw.boneIndex3 += boneOffset;
                    matToBoneWeights[mat].Add(bw);
                }
            }
        }

        private void CombineAndSave(GameObject rootObj, List<SkinnedMeshRenderer> smRenderers, List<Transform> bones, Transform root, List<Material> mats, List<Matrix4x4> bindPoses, Dictionary<Material, List<CombineInstance>> matToCombineInstances, Dictionary<Material, List<BoneWeight>> matToBoneWeights)
        {
            var finalCombineInstances = new List<CombineInstance>();
            var finalBoneWeights = new List<BoneWeight>();
            var finalBindPoses = new List<Matrix4x4>();

            foreach (var kvp in matToCombineInstances)
            {
                var mat = kvp.Key;
                var combineInstancesForMat = kvp.Value;

                var combinedMesh = new Mesh();
                combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combinedMesh.CombineMeshes(combineInstancesForMat.ToArray(), true, false);

                var boneWeights = matToBoneWeights[mat];
                if (boneWeights.Count != combinedMesh.vertexCount)
                {
                    Debug.LogWarning($"Mismatch between bone weights and vertex count for material {mat.name}. Bone weights count: {boneWeights.Count}, Vertex count: {combinedMesh.vertexCount}. Adjusting bone weights.");
                    boneWeights = AdjustWeights(boneWeights, combinedMesh.vertexCount);
                }

                combinedMesh.boneWeights = boneWeights.ToArray();
                combinedMesh.bindposes = bindPoses.ToArray();

                finalCombineInstances.Add(new CombineInstance
                {
                    mesh = combinedMesh,
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity
                });

                mats.Add(mat);
                finalBoneWeights.AddRange(boneWeights);
            }

            var finalCombinedMesh = new Mesh();
            finalCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalCombinedMesh.CombineMeshes(finalCombineInstances.ToArray(), false, false);
            finalCombinedMesh.boneWeights = finalBoneWeights.ToArray();
            finalCombinedMesh.bindposes = bindPoses.ToArray();
            finalCombinedMesh.subMeshCount = mats.Count;

            if (finalCombinedMesh.boneWeights.Length != finalCombinedMesh.vertexCount)
            {
                Debug.LogWarning($"Final combined mesh has a mismatch between bone weights and vertex count. Bone weights count: {finalCombinedMesh.boneWeights.Length}, Vertex count: {finalCombinedMesh.vertexCount}");
            }

            EditorApplication.delayCall += () => SaveMesh(finalCombinedMesh, rootObj.name);

            CreateSkinnedRenderer(rootObj, finalCombinedMesh, bones, root, mats.ToArray());

            DisableOriginal(smRenderers);

            Debug.Log($"Meshes combined successfully for {rootObj.name}.");
        }

        private List<BoneWeight> AdjustWeights(List<BoneWeight> boneWeights, int vertexCount)
        {
            List<BoneWeight> adjustedBoneWeights = new List<BoneWeight>(new BoneWeight[vertexCount]);
            for (int i = 0; i < vertexCount && i < boneWeights.Count; i++)
            {
                adjustedBoneWeights[i] = boneWeights[i];
            }
            return adjustedBoneWeights;
        }

        private void SaveMesh(Mesh combinedMesh, string objName)
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
            AssetDatabase.CreateAsset(combinedMesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Saved combined mesh as asset at: {assetPath}");
        }
        // Ace Studio X Joe
        private Mesh MakeMeshReadable(Mesh originalMesh)
        {
            if (!originalMesh.isReadable)
            {
                Mesh readableMesh = Object.Instantiate(originalMesh);
                readableMesh.name = originalMesh.name + "_readable";
                return readableMesh;
            }
            return originalMesh;
        }

        private void CreateSkinnedRenderer(GameObject rootObj, Mesh combinedMesh, List<Transform> bones, Transform root, Material[] mats)
        {
            GameObject combinedObj = new GameObject("CombinedSkinnedMesh_" + rootObj.name);
            combinedObj.transform.SetParent(rootObj.transform);

            SkinnedMeshRenderer combinedSkinnedRenderer = combinedObj.AddComponent<SkinnedMeshRenderer>();

            combinedSkinnedRenderer.sharedMesh = combinedMesh;
            combinedSkinnedRenderer.bones = bones.ToArray();
            combinedSkinnedRenderer.rootBone = root;
            combinedSkinnedRenderer.materials = mats;

            combinedObj.transform.localPosition = Vector3.zero;
            combinedObj.transform.localRotation = Quaternion.identity;
            combinedObj.transform.localScale = Vector3.one;

            combinedSkinnedRenderer.updateWhenOffscreen = true;

            Debug.Log($"Created combined SkinnedMeshRenderer for {rootObj.name} with {combinedMesh.vertexCount} vertices, {combinedMesh.subMeshCount} submeshes, and {bones.Count} bones.");
        }

        private void DisableOriginal(List<SkinnedMeshRenderer> smRenderers)
        {
            foreach (var smr in smRenderers)
            {
                smr.enabled = false;
            }
        }

        private void RemoveAllCombined()
        {
            foreach (var targetObj in targetObjs)
            {
                if (targetObj != null) // Ace Studio X Joe
                {
                    RemoveCombined(targetObj);
                }
            }
        }

        private void RemoveCombined(GameObject rootObj)
        {
            Transform combinedSkinnedMeshTransform = rootObj.transform.Find("CombinedSkinnedMesh_" + rootObj.name);
            if (combinedSkinnedMeshTransform != null)
            {
                DestroyImmediate(combinedSkinnedMeshTransform.gameObject);
                Debug.Log($"Combined skinned mesh removed from {rootObj.name}.");
            }
            else // Ace Studio X Joe
            {
                Debug.LogWarning($"No combined skinned mesh found on {rootObj.name}.");
            }

            Transform combinedMeshTransform = rootObj.transform.Find("CombinedMesh_" + rootObj.name);
            if (combinedMeshTransform != null)
            {
                DestroyImmediate(combinedMeshTransform.gameObject);
                Debug.Log($"Combined mesh removed from {rootObj.name}.");
            }
            else
            {
                Debug.LogWarning($"No combined mesh found on {rootObj.name}.");
            }
        }

        private void EnableAllSkinned()
        {
            foreach (var targetObj in targetObjs)
            {
                if (targetObj != null)
                {
                    EnableSkinned(targetObj);
                }
            }
        }

        private void EnableSkinned(GameObject rootObj)
        {
            SkinnedMeshRenderer[] smRenderers = rootObj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smRenderers)
            {
                smr.enabled = true;
            }
            Debug.Log($"Enabled all SkinnedMeshRenderers on {rootObj.name}.");
        }

        private void EnableAllMesh()
        {
            foreach (var targetObj in targetObjs)
            {
                if (targetObj != null)
                {
                    EnableMesh(targetObj);
                }
            }
        }

        private void EnableMesh(GameObject rootObj)
        {
            MeshRenderer[] meshRenderers = rootObj.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in meshRenderers)
            {
                mr.enabled = true;
            }
            Debug.Log($"Enabled all MeshRenderers on {rootObj.name}.");
        }

        private void ShowError(string msg)
        {
            Debug.LogError(msg);
            EditorUtility.DisplayDialog("Error", msg, "OK");
        }

        private void ApplyTransforms(GameObject rootObj)
        {
            foreach (var smr in rootObj.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.sharedMesh.RecalculateBounds();
                smr.sharedMesh.RecalculateNormals();
                smr.sharedMesh.RecalculateTangents();
            }
        }

        private Mesh CleanupMesh(Mesh mesh)
        {
            
            Mesh cleanedMesh = new Mesh();

           
            HashSet<Vector3> uniqueVertices = new HashSet<Vector3>();
            List<Vector3> cleanedVertices = new List<Vector3>();
            List<int> cleanedTriangles = new List<int>();

            
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 vertex = mesh.vertices[i];
                if (!uniqueVertices.Contains(vertex))
                {
                    uniqueVertices.Add(vertex);
                    cleanedVertices.Add(vertex);
                }
            }

            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                int triangle = mesh.triangles[i];
                if (uniqueVertices.Contains(mesh.vertices[triangle]))
                {
                    cleanedTriangles.Add(cleanedVertices.IndexOf(mesh.vertices[triangle]));
                }
            }

            
            cleanedMesh.SetVertices(cleanedVertices);
            cleanedMesh.SetTriangles(cleanedTriangles, 0);

            
            cleanedMesh.SetNormals(mesh.normals);
            cleanedMesh.SetUVs(0, mesh.uv);
            cleanedMesh.SetTangents(mesh.tangents);
            cleanedMesh.SetColors(mesh.colors);
            cleanedMesh.boneWeights = mesh.boneWeights;
            cleanedMesh.bindposes = mesh.bindposes;

            return cleanedMesh;
        }

        private void CorrectUVMapping(Mesh mesh)
        {
            Vector2[] uvs = mesh.uv;
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(uvs[i].x, 1 - uvs[i].y);
            }
            mesh.uv = uvs;
        }
    }
}
