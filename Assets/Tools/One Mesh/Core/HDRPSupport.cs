/*
One Mesh by ACE STUDIO X
Copyright (c) 2024 ACE STUDIO X
All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AceStudioX.OneMat
{
    public static class HDRPMaterialCombiner
    {
        public static void CombineSubMaterials(SkinnedMeshRenderer smr, string path, int aniso, int maxTex)
        {
            if (smr == null)
            {
                Debug.LogError("No Skinned Mesh Renderer provided for sub-material combination.");
                return;
            }

            Material[] subMats = smr.sharedMaterials;
            if (subMats.Length <= 1)
            {
                Debug.LogWarning("No sub-materials to combine or only one sub-material present.");
                return;
            }

            var texDict = InitTexDict();
            var matToTex = new Dictionary<Material, Dictionary<string, Texture2D>>();

            foreach (var mat in subMats)
            {
                if (mat == null) continue;
                matToTex[mat] = AddTex(mat, texDict);
            }

            EnsureDir($"{path}/SubMaterials");

            var atlases = SaveAtlases(smr.name, texDict, path, aniso, maxTex, out var rects);

            Material combinedMat = CreateMat(atlases);

            string matPath = $"{path}/SubMaterials/{smr.name}_Combined_Sub_HDRP.mat";
            SaveAsset(combinedMat, matPath);

            combinedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Mesh combinedMesh = CombineMesh(smr.sharedMesh, subMats, rects);

            string meshPath = $"{path}/SubMaterials/{smr.name}_CombinedMesh_HDRP.asset";
            SaveAsset(combinedMesh, meshPath);

            combinedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

            Transform rootBone = smr.rootBone;
            Transform[] bones = smr.bones;

            smr.sharedMesh = combinedMesh;
            smr.sharedMaterials = new Material[] { combinedMat };
            smr.rootBone = rootBone;
            smr.bones = bones;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, List<Texture2D>> InitTexDict()
        {
            return new Dictionary<string, List<Texture2D>>
            {
                { "_BaseColorMap", new List<Texture2D>() },
                { "_MaskMap", new List<Texture2D>() },
                { "_NormalMap", new List<Texture2D>() }
            };
        }

        private static Dictionary<string, Texture2D> AddTex(Material mat, Dictionary<string, List<Texture2D>> texDict)
        {
            var texMap = new Dictionary<string, Texture2D>();

            foreach (var texType in texDict.Keys)
            {
                texMap[texType] = GetTex(mat, texType, texDict[texType]);
            }

            return texMap;
        }

        private static Texture2D GetTex(Material mat, string texName, List<Texture2D> texList)
        {
            Texture2D tex = mat.HasProperty(texName) ? mat.GetTexture(texName) as Texture2D : null;
            if (tex != null)
            {
                EnsureReadable(tex, texName == "_NormalMap");
                texList.Add(tex);
                return tex;
            }
            texList.Add(Texture2D.whiteTexture);
            return Texture2D.whiteTexture; // Ace Studio X Joe
        }

        private static void EnsureReadable(Texture2D tex, bool isNormalMap)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;

                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                    changed = true;
                }

                if (isNormalMap && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }
                else if (!isNormalMap && importer.textureType == TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.Default;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
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

        private static Dictionary<string, Texture2D> SaveAtlases(string name, Dictionary<string, List<Texture2D>> texDict, string path, int aniso, int maxTex, out Dictionary<string, Rect[]> rects)
        {
            var atlases = new Dictionary<string, Texture2D>();
            rects = new Dictionary<string, Rect[]>();

            foreach (var kvp in texDict)
            {
                if (kvp.Value.Count == 0) continue;

                var atlas = CreateAtlas(kvp.Value, out var rect, maxTex);
                if (kvp.Key == "_NormalMap")
                {
                    atlas = NewAtlas(atlas);
                } // Ace Studio X Joe
                string atlasPath = $"{path}/SubMaterials/{name}_Combined_{kvp.Key}_Atlas.tga";
                EnsureDir(Path.GetDirectoryName(atlasPath));
                SaveAtlas(atlas, atlasPath, kvp.Key == "_NormalMap", aniso, maxTex);
                atlases[kvp.Key] = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                rects[kvp.Key] = rect;
            }

            return atlases;
        }

        private static Texture2D CreateAtlas(List<Texture2D> texList, out Rect[] rects, int maxTex)
        {
            int size = CalcAtlasSize(texList.Count);
            var atlas = new Texture2D(size, size, TextureFormat.RGBA32, true);
            rects = atlas.PackTextures(texList.ToArray(), 0, maxTex, false);
            atlas.Apply(true);
            return atlas;
        }

        private static Texture2D NewAtlas(Texture2D src)
        {
            int width = src.width, height = src.height;
            Texture2D dest = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color[] pixels = src.GetPixels();
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Color color = pixels[j * width + i];
                    float red = Mathf.Sqrt(1.0f - Mathf.Pow((color.g * 2) - 1, 2) - Mathf.Pow((color.b * 2) - 1, 2));
                    float green = Mathf.Clamp01(((color.g * 2) - 1) * 0.5f + 0.5f);
                    float blue = Mathf.Clamp01(((color.b * 2) - 1) * 0.5f + 0.5f);
                    dest.SetPixel(i, j, new Color(green, blue, red, color.a));
                }
            }

            dest.Apply();
            return dest;
        }

        private static int CalcAtlasSize(int count)
        {
            int size = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(count)) * 256);
            return Mathf.Clamp(size, 256, 4096);
        }

        private static void SaveAtlas(Texture2D atlas, string path, bool isNormalMap, int aniso, int maxTex)
        {
            EnsureDir(Path.GetDirectoryName(path));

            Texture2D uncompressed = new Texture2D(atlas.width, atlas.height, TextureFormat.RGBA32, false);
            uncompressed.SetPixels(atlas.GetPixels());
            uncompressed.Apply();

            byte[] bytes = uncompressed.EncodeToTGA();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = isNormalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.anisoLevel = aniso;
                importer.maxTextureSize = maxTex;
                importer.sRGBTexture = !isNormalMap;
                importer.SaveAndReimport();
            }
        }

        private static void SaveAsset(UnityEngine.Object asset, string path)
        {
            EnsureDir(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material CreateMat(Dictionary<string, Texture2D> atlases)
        {
            var mat = new Material(Shader.Find("HDRP/Lit"));
            foreach (var kvp in atlases)
            {
                mat.SetTexture(kvp.Key, kvp.Value);
            }

            mat.SetFloat("_Smoothness", 0.5f);
            mat.SetFloat("_Metallic", 0.5f);

            return mat;
        }

        private static Mesh CombineMesh(Mesh originalMesh, Material[] mats, Dictionary<string, Rect[]> rects)
        {
            if (originalMesh == null || mats == null || rects == null)
            {
                Debug.LogError("CombineMesh received a null parameter.");
                return null;
            }

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var norms = new List<Vector3>();
            var tris = new List<int>();
            var boneWeights = new List<BoneWeight>();
            var bindposes = originalMesh.bindposes;
            var blendShapes = new Dictionary<string, List<Vector3>>();
            var blendShapesNormals = new Dictionary<string, List<Vector3>>();
            var blendShapesTangents = new Dictionary<string, List<Vector3>>();

            int vertOffset = 0;

            for (int subMeshIdx = 0; subMeshIdx < originalMesh.subMeshCount; subMeshIdx++)
            {
                var subVerts = originalMesh.vertices;
                var subUVs = originalMesh.uv;
                var subNorms = originalMesh.normals;
                var subTris = originalMesh.GetTriangles(subMeshIdx);
                var subBoneWeights = originalMesh.boneWeights;

                if (!rects.ContainsKey("_BaseColorMap") || subMeshIdx >= rects["_BaseColorMap"].Length)
                {
                    Debug.LogError($"Missing UV rect for subMeshIdx {subMeshIdx} in _BaseColorMap.");
                    continue;
                }

                var uvRect = rects["_BaseColorMap"][subMeshIdx];

                for (int i = 0; i < subVerts.Length; i++)
                {
                    verts.Add(subVerts[i]);
                    uvs.Add(new Vector2(
                        Mathf.Lerp(uvRect.xMin, uvRect.xMax, subUVs[i].x),
                        Mathf.Lerp(uvRect.yMin, uvRect.yMax, subUVs[i].y)
                    ));
                    norms.Add(subNorms[i]);
                    boneWeights.Add(subBoneWeights[i]);
                }

                for (int i = 0; i < subTris.Length; i++)
                {
                    tris.Add(subTris[i] + vertOffset);
                }

                
                for (int shapeIndex = 0; shapeIndex < originalMesh.blendShapeCount; shapeIndex++)
                {
                    string blendShapeName = originalMesh.GetBlendShapeName(shapeIndex);
                    if (!blendShapes.ContainsKey(blendShapeName))
                    {
                        blendShapes[blendShapeName] = new List<Vector3>();
                        blendShapesNormals[blendShapeName] = new List<Vector3>();
                        blendShapesTangents[blendShapeName] = new List<Vector3>();
                    }

                    Vector3[] deltaVertices = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaTangents = new Vector3[originalMesh.vertexCount];

                    originalMesh.GetBlendShapeFrameVertices(shapeIndex, 0, deltaVertices, deltaNormals, deltaTangents);
                    for (int i = 0; i < deltaVertices.Length; i++)
                    {
                        blendShapes[blendShapeName].Add(deltaVertices[i]);
                        blendShapesNormals[blendShapeName].Add(deltaNormals[i]);
                        blendShapesTangents[blendShapeName].Add(deltaTangents[i]);
                    }
                }

                vertOffset += subVerts.Length;
            }

            var combined = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            combined.SetVertices(verts);
            combined.SetUVs(0, uvs);
            combined.SetNormals(norms);
            combined.SetTriangles(tris, 0);
            combined.boneWeights = boneWeights.ToArray();
            combined.bindposes = bindposes;

            
            foreach (var kvp in blendShapes)
            {
                string blendShapeName = kvp.Key;
                List<Vector3> deltaVertices = kvp.Value;
                List<Vector3> deltaNormals = blendShapesNormals[blendShapeName];
                List<Vector3> deltaTangents = blendShapesTangents[blendShapeName];

                if (deltaVertices.Count == combined.vertexCount)
                {
                    combined.AddBlendShapeFrame(blendShapeName, 100f, deltaVertices.ToArray(), deltaNormals.ToArray(), deltaTangents.ToArray());
                }
            }

            return combined;
        }
    }
}
