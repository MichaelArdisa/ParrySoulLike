/*
One Mesh by ACE STUDIO X
Copyright (c) 2024 ACE STUDIO X
All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace AceStudioX.OneMat
{
    public class OneMatTool : EditorWindow
    {
        [SerializeField]
        private List<SkinnedMeshRenderer> smrList = new List<SkinnedMeshRenderer>();
        private string savePath = "Assets/One Mesh Materials/";
        private int anisoLvl = 1;
        private int maxTexSize = 4096;
        private readonly int[] texSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

        private SerializedObject so;
        private SerializedProperty smrProp;

        private readonly GUIContent smrContent = new GUIContent("Skinned Mesh Renderers", "List of Skinned Mesh Renderers to combine materials for.");
        private readonly GUIContent savePathContent = new GUIContent("Save Path", "Path to save the combined materials.");
        private readonly GUIContent anisoContent = new GUIContent("Anisotropic Filtering Level", "Anisotropic filtering level for textures.");
        private readonly GUIContent maxTexSizeContent = new GUIContent("Maximum Texture Size", "Maximum size for texture atlases.");

        [MenuItem("Tools/One Mesh/One Material")]
        public static void ShowWindow()
        {
            GetWindow<OneMatTool>("One Material");
        }

        private void OnEnable()
        {
            so = new SerializedObject(this);
            smrProp = so.FindProperty("smrList");
        }

        private void OnGUI()
        {
            so.Update();

            DrawHeader();
            DrawSettings();
            DrawCombineButtons();
            DrawSplitButton();

            so.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            GUILayout.Label("One Material - Ace Studio X", EditorStyles.boldLabel);
        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Skinned Mesh Renderers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(smrProp, smrContent, true);
            GUILayout.Space(10);

            GUILayout.Label("Save Path", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField(savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Save Folder", savePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    savePath = FileUtil.GetProjectRelativePath(selectedPath);
                    if (string.IsNullOrEmpty(savePath))
                    {
                        savePath = "Assets/One Mesh Materials";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.Label("Texture Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(anisoContent, GUILayout.Width(200));
            anisoLvl = EditorGUILayout.IntSlider(anisoLvl, 1, 16);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(maxTexSizeContent, GUILayout.Width(200));
            maxTexSize = EditorGUILayout.IntPopup(maxTexSize, Array.ConvertAll(texSizes, x => x.ToString()), texSizes);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawCombineButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Combine Actions", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent("Combine All Materials (Standard)", "Combine materials for selected skinned mesh renderers using Standard shader.")))
            {
                if (ValidateSmrList())
                {
                    try
                    {
                        CombineAllSubMaterials();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error during all skinned mesh material combination: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Combine All Materials (URP)", "Combine materials for selected skinned mesh renderers using URP shader.")))
            {
                if (ValidateSmrList())
                {
                    try
                    {
                        CombineMaterialsURP();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error during URP material combination: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Combine All Materials (HDRP)", "Combine materials for selected skinned mesh renderers using HDRP shader.")))
            {
                if (ValidateSmrList())
                {
                    try
                    {
                        CombineMaterialsHDRP();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error during HDRP material combination: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            EditorGUILayout.EndVertical();
        } // Ace Studio X Joe

        private void DrawSplitButton()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Split Actions", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent("Split Sub-Meshes", "Split sub-meshes into unique meshes with their own materials.")))
            {
                if (ValidateSmrList())
                {
                    try
                    {
                        SplitSubMeshes();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error during sub-mesh splitting: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private bool ValidateSmrList()
        {
            if (smrList.Count == 0 || smrList[0] == null)
            {
                EditorGUILayout.HelpBox("Please assign at least one Skinned Mesh Renderer.", MessageType.Error);
                return false;
            }
            return true;
        }

        private void CombineAllSubMaterials()
        {
            foreach (var smr in smrList)
            {
                if (smr != null)
                {
                    SubMaterialCombiner.CombineSubMaterials(smr, savePath, anisoLvl, maxTexSize);
                }
            }
        }

        private void CombineMaterialsURP()
        {
            foreach (var smr in smrList)
            {
                if (smr != null)
                {
                    URPMaterialCombiner.CombineSubMaterials(smr, savePath, anisoLvl, maxTexSize);
                }
            }
        }

        private void CombineMaterialsHDRP()
        {
            foreach (var smr in smrList)
            {
                if (smr != null)
                {
                    HDRPMaterialCombiner.CombineSubMaterials(smr, savePath, anisoLvl, maxTexSize);
                }
            }
        }

        private void SplitSubMeshes()
        {
            foreach (var smr in smrList)
            {
                if (smr != null)
                {
                    List<GameObject> newObjects = SplitUtility.SplitSubMeshes(smr, savePath);
                    foreach (var obj in newObjects)
                    {
                        Debug.Log($"Created new GameObject: {obj.name}");
                    }
                }
            }
        }
    }

    public static class SplitUtility
    {
        public static List<GameObject> SplitSubMeshes(SkinnedMeshRenderer smr, string path)
        {
            if (smr == null)
            {
                Debug.LogError("No Skinned Mesh Renderer provided for splitting sub-meshes.");
                return null;
            }

            List<GameObject> newObjects = new List<GameObject>();
            Material[] materials = smr.sharedMaterials;
            Mesh originalMesh = smr.sharedMesh;

            for (int i = 0; i < originalMesh.subMeshCount; i++)
            {
                Mesh newMesh = new Mesh();
                newMesh.name = $"{originalMesh.name}_SubMesh_{i}";

                newMesh.vertices = originalMesh.vertices;
                newMesh.normals = originalMesh.normals;
                newMesh.uv = originalMesh.uv;
                newMesh.boneWeights = originalMesh.boneWeights;
                newMesh.bindposes = originalMesh.bindposes;

                newMesh.SetTriangles(originalMesh.GetTriangles(i), 0);

                if (SubMeshBlendCheck(originalMesh, i))
                {
                    CopyBlendShapes(originalMesh, newMesh);
                }

                string meshPath = $"{path}/{newMesh.name}.asset";
                EnsureDir(Path.GetDirectoryName(meshPath));
                AssetDatabase.CreateAsset(newMesh, meshPath);
                AssetDatabase.SaveAssets();
                newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

                GameObject newObject = new GameObject($"{smr.gameObject.name}_SubMesh_{i}");
                newObject.transform.SetParent(smr.transform);
                newObject.transform.localPosition = Vector3.zero;
                newObject.transform.localRotation = Quaternion.identity;
                newObject.transform.localScale = Vector3.one;

                SkinnedMeshRenderer newSmr = newObject.AddComponent<SkinnedMeshRenderer>();
                newSmr.sharedMesh = newMesh;
                newSmr.sharedMaterials = new Material[] { materials[i] };
                newSmr.rootBone = smr.rootBone;
                newSmr.bones = smr.bones;
                newSmr.updateWhenOffscreen = true;

                newObjects.Add(newObject);
            }

            smr.enabled = false;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return newObjects;
        }

        private static bool SubMeshBlendCheck(Mesh originalMesh, int subMeshIndex)
        {
            for (int shapeIndex = 0; shapeIndex < originalMesh.blendShapeCount; shapeIndex++)
            {
                int frameCount = originalMesh.GetBlendShapeFrameCount(shapeIndex);
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    Vector3[] deltaVertices = new Vector3[originalMesh.vertexCount];
                    originalMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, null, null);
                    int[] triangles = originalMesh.GetTriangles(subMeshIndex);
                    foreach (int vertexIndex in triangles)
                    {
                        if (deltaVertices[vertexIndex] != Vector3.zero)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void CopyBlendShapes(Mesh originalMesh, Mesh newMesh)
        {
            for (int shapeIndex = 0; shapeIndex < originalMesh.blendShapeCount; shapeIndex++)
            {
                string shapeName = originalMesh.GetBlendShapeName(shapeIndex);
                int frameCount = originalMesh.GetBlendShapeFrameCount(shapeIndex);

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    float frameWeight = originalMesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    Vector3[] deltaVertices = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaTangents = new Vector3[originalMesh.vertexCount];

                    originalMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    newMesh.AddBlendShapeFrame(shapeName, frameWeight, deltaVertices, deltaNormals, deltaTangents);
                }
            }
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    public static class TexExtensions
    {
        public static byte[] ToTGA(this Texture2D tex)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write((byte)0); 
                bw.Write((byte)0);
                bw.Write((byte)2);
                bw.Write((short)0);
                bw.Write((short)0);
                bw.Write((byte)0);
                bw.Write((short)0);
                bw.Write((short)0);
                bw.Write((short)tex.width);
                bw.Write((short)tex.height);
                bw.Write((byte)32);
                bw.Write((byte)0);

                var pixels = tex.GetPixels32();
                foreach (var p in pixels)
                {
                    bw.Write(p.b);
                    bw.Write(p.g);
                    bw.Write(p.r);
                    bw.Write(p.a);
                }

                bw.Flush();
                return ms.ToArray();
            }
        }
    }
}