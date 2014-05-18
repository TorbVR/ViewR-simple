/*==============================================================================
Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc.
All Rights Reserved.

This  Vuforia(TM) sample application in source code form ("Sample Code") for the
Vuforia Software Development Kit and/or Vuforia Extension for Unity
(collectively, the "Vuforia SDK") may in all cases only be used in conjunction
with use of the Vuforia SDK, and is subject in all respects to all of the terms
and conditions of the Vuforia SDK License Agreement, which may be found at
https://developer.vuforia.com/legal/license.

By retaining or using the Sample Code in any manner, you confirm your agreement
to all the terms and conditions of the Vuforia SDK License Agreement.  If you do
not agree to all the terms and conditions of the Vuforia SDK License Agreement,
then you may not retain or use any of the Sample Code in any manner.
==============================================================================*/

using UnityEditor;
using UnityEngine;

/// <summary>
/// This editor renders the inspector for the VideoPlaybackBehaviour
/// </summary>
[CustomEditor(typeof(VideoPlaybackBehaviour))]
public class VideoPlaybackEditor : Editor
{
    #region NESTED

    public const string REFERENCE_MATERIAL_PATH =
            "Assets/Vuforia Video Playback/Materials/VideoMaterial.mat";

    #endregion // NESTED



    #region UNITY_EDITOR_METHODS

    public override void OnInspectorGUI()
    {
        // Get the video playback behaviour that is being edited
        VideoPlaybackBehaviour vpb = (VideoPlaybackBehaviour) target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add an inspector field for the keyframe texture
        vpb.KeyframeTexture = (Texture) EditorGUILayout.ObjectField(
            "Keyframe Texture", vpb.KeyframeTexture, typeof(Texture), false);

        // If the keyframe texture field changed, update the material
        if (GUI.changed)
        {
            UpdateMaterial(vpb);

            EditorUtility.SetDirty(vpb);
        }
    }

    #endregion // UNITY_EDITOR_METHODS



    #region PRIVATE_METHODS

    public static void UpdateMaterial(VideoPlaybackBehaviour vpb)
    {
        // Load the reference material
        Material referenceMaterial =
                (Material)AssetDatabase.LoadAssetAtPath(REFERENCE_MATERIAL_PATH,
                                                    typeof(Material));

        if (referenceMaterial == null)
        {
            Debug.LogError("Could not find reference material at " +
                           REFERENCE_MATERIAL_PATH +
                           " please reimport Unity package.");
            return;
        }

        if (vpb.KeyframeTexture == null)
        {
            // Reset to reference material if keyframe texture is null
            vpb.renderer.sharedMaterial = referenceMaterial;
        }
        else
        {
            // Create a new material that is based on the reference material and
            // uses the selected keyframe texture
            Material material = new Material(referenceMaterial);

            material.mainTexture = vpb.KeyframeTexture;
            material.name = vpb.KeyframeTexture.name + "Material";
            material.mainTextureScale = new Vector2(-1, -1);
            material.renderQueue = referenceMaterial.renderQueue + 1;

            vpb.renderer.sharedMaterial = material;
        }

        // Cleanup assets that have been created temporarily
        EditorUtility.UnloadUnusedAssets();
    }

    #endregion // PRIVATE_METHODS
}
