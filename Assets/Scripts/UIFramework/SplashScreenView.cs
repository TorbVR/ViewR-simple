/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// Displays splash image with appropriate size for different device resolutions
/// </summary>
public class SplashScreenView : UIView
{
    #region PRIVATE_MEMBER_VARIABLES
    private Texture mAndroidPotrait;
    #endregion PRIVATE_MEMBER_VARIABLES
    
    #region UIView implementation
    public void LoadView ()
    {
        mAndroidPotrait = Resources.Load("SplashScreen/AndroidPotrait") as Texture;
    }

    public void UnLoadView ()
    {
        Resources.UnloadAsset(mAndroidPotrait);
    }

    public void UpdateUI (bool tf)
    {
        if(!tf)return;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mAndroidPotrait, ScaleMode.ScaleAndCrop);
    }
    #endregion UIView implementation
}

