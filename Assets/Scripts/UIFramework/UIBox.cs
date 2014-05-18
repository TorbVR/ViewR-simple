/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class UIBox : UIElement
{
    /// <summary>
    /// Initializes a new instance for a box UI
    /// <param name='rect'> specifies box size
    /// <param name='path'> specifies path for assets to load from Resources
    public UIBox(Rect rect, string path)
    {
        this.mRect = rect;
        mStyle = new GUIStyle();
        mStyle.normal.background = Resources.Load(path) as Texture2D;
    }
    
    public void Draw()
    {
        GUI.Box(mRect, "", mStyle);
    }
    
    private Rect mRect;
    private GUIStyle mStyle;
}


