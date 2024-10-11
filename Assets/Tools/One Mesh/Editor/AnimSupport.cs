/*
One Mesh by ACE STUDIO X
Copyright (c) 2024 ACE STUDIO X
All rights reserved.
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;

namespace AceStudioX.OneMesh
{
    public class AnimeSupport : EditorWindow
    {
        private Animator animator;
        private string bakedAnimationFolder = "Assets/OMAnimations/";

        [MenuItem("Tools/One Mesh/AnimeSupport")]
        public static void ShowWindow()
        {
            GetWindow<AnimeSupport>("Anime Support");
        }

        private void OnGUI()
        {
            GUILayout.Label("Animation Baker -  Ace Studio X", EditorStyles.boldLabel);
            
            animator = (Animator)EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);
            bakedAnimationFolder = EditorGUILayout.TextField("Baked Animation Folder", bakedAnimationFolder);

            if (GUILayout.Button("Bake All Animations"))
            {
                BakeAllAnimations();
            }
        }

        private void BakeAllAnimations()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Animator or Animator Controller is not assigned!");
                return;
            }

            AnimatorController originalController = (AnimatorController)animator.runtimeAnimatorController;
            AnimatorController newController = CreateNewAnimatorController(originalController);

            var clips = originalController.animationClips;
            foreach (var clip in clips)
            {
                AnimationClip bakedClip = BakeAnimation(clip);
                if (bakedClip != null)
                {
                    AddBakedClipToController(newController, clip, bakedClip);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            animator.runtimeAnimatorController = newController;
            Debug.Log("All animations baked and new Animator Controller assigned.");
        }

        private AnimatorController CreateNewAnimatorController(AnimatorController originalController)
        {
            EnsureFolderExists(bakedAnimationFolder);
            
            string newControllerPath = Path.Combine(bakedAnimationFolder, originalController.name + "_Baked.controller");
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(originalController), newControllerPath);

            return AssetDatabase.LoadAssetAtPath<AnimatorController>(newControllerPath);
        }

        private AnimationClip BakeAnimation(AnimationClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("Animation clip is null!");
                return null;
            }

            AnimationClip bakedClip = new AnimationClip
            {
                name = clip.name + "_Baked"
            };

            var bindingPaths = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindingPaths)
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                AnimationUtility.SetEditorCurve(bakedClip, binding, curve);
            }

            SaveAnimationClip(bakedClip);
            return bakedClip;
        }

        private void SaveAnimationClip(AnimationClip clip)
        { // Ace Studio X Joe
            EnsureFolderExists(bakedAnimationFolder);

            string path = Path.Combine(bakedAnimationFolder, clip.name + ".anim");
            AssetDatabase.CreateAsset(clip, path);
            Debug.Log("Baked animation saved at: " + path);
        }

        private void EnsureFolderExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = Path.GetDirectoryName(folderPath);
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    EnsureFolderExists(parentFolder);
                }
                string newFolderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        private void AddBakedClipToController(AnimatorController controller, AnimationClip originalClip, AnimationClip bakedClip)
        {
            var layers = controller.layers;
            foreach (var layer in layers)
            {
                var stateMachine = layer.stateMachine;
                foreach (var state in stateMachine.states)
                {
                    if (state.state.motion == originalClip)
                    {
                        state.state.motion = bakedClip;
                    }
                }
            }
        }
    }
}
