/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System;

/// <summary>
/// This script sets up the background shader effect and contains the logic
/// to capture longer touch "drag" events that distort the video background. 
/// It also checks for OpenGL ES 2.0 support.
/// The background texture access sample does not support OpenGL ES 1.x
/// </summary>
[RequireComponent(typeof(VideoTextureBehaviour))]
[RequireComponent(typeof(GLErrorHandler))]
public class NegativeGrayscaleEffect : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES
    // time of last press down event
    private bool mErrorOccurred = false;
    private const string ERROR_TEXT = "The BackgroundTextureAccess sample requires OpenGL ES 2.0 or higher";
    private const string CHECK_STRING = "OpenGL ES";
    #endregion // PRIVATE_MEMBER_VARIABLES

    public void InitEffect()
    {
        // This sample requires OpenGL ES 2.0 otherwise it won't work.
        mErrorOccurred = !IsOpenGLES2();

        if (mErrorOccurred)
        {
            Debug.LogError(ERROR_TEXT);

            // Show a dialog box with an error message.
            GLErrorHandler.SetError(ERROR_TEXT);

            // Turn off renderer to make sure the unsupported shader is not used.
            renderer.enabled = false;

            TrackableBehaviour[] tbs = (TrackableBehaviour[])FindObjectsOfType(typeof(TrackableBehaviour));
            if (tbs != null)
            {
                for (int i = 0; i < tbs.Length; ++i)
                {
                    tbs[i].enabled = false;
                }
            }
        }
    }
    
    public void UpdateEffect()
    {
        float touchX = 2.0F;
        float touchY = 2.0F;
        if(Input.GetMouseButton(0))
        {
            Vector2 touchPos = Input.mousePosition;
            // Adjust the touch point for the current orientation
            if (QCARRuntimeUtilities.ScreenOrientation == ScreenOrientation.Landscape)
            {
                touchX = (touchPos.x/Screen.width) - 0.5F;
                touchY = (touchPos.y/Screen.height) - 0.5F;
            }
            else if (QCARRuntimeUtilities.ScreenOrientation == ScreenOrientation.Portrait)
            {
                touchX = ((touchPos.y/Screen.height) - 0.5F)*-1;
                touchY = (touchPos.x/Screen.width) - 0.5F;
            }
            else if (QCARRuntimeUtilities.ScreenOrientation == ScreenOrientation.LandscapeRight)
            {
                touchX = ((touchPos.x/Screen.width) - 0.5F)*-1;
                touchY = ((touchPos.y/Screen.height) - 0.5F)*-1;
            }
            else if (QCARRuntimeUtilities.ScreenOrientation == ScreenOrientation.PortraitUpsideDown)
            {
                touchX = (touchPos.y/Screen.height) - 0.5F;
                touchY = ((touchPos.x/Screen.width) - 0.5F)*-1;
            }

        }
        
        renderer.material.SetFloat("_TouchX", touchX);
        renderer.material.SetFloat("_TouchY", touchY);
    }
    
    #region PRIVATE_METHODS
    /// <summary>
    /// This method checks if we are using OpenGL ES 2.0 or later.
    /// </summary>
    private bool IsOpenGLES2()
    {
        // in play mode on a desktop machine, always return true
        if (QCARRuntimeUtilities.IsPlayMode()) return true;

        string graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;

        Debug.Log("Sample using " + graphicsDeviceVersion);

        int oglStringIdx = graphicsDeviceVersion.IndexOf(CHECK_STRING, StringComparison.Ordinal);
        if (oglStringIdx >= 0)
        {
            // it's open gl es, parse the version number
            float esVersion;
            if (float.TryParse(graphicsDeviceVersion.Substring(oglStringIdx + CHECK_STRING.Length + 1, 3), out esVersion))
            {
                return esVersion >= 2.0f;
            }
        }
        return false;
    }

    #endregion // PRIVATE_METHODS
}
