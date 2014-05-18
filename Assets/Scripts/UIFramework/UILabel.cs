/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class UILabel : UIElement 
{
    /// <summary>
    /// Initializes a new instance for label
    /// <param name='rect'> specifies label size
    /// <param name='path'> specifies path for assets to load from Resources
    public UILabel(UIRect rect, string path)
    {
        this.mRect = rect;
        mStyle = new GUIStyle();
        mStyle.normal.background = Resources.Load(path) as Texture2D;
    }
    
    public void Draw()
    {
        GUI.Label(mRect.rect, "", mStyle);
    }
    
    private UIRect mRect;
    private GUIStyle mStyle;
}

