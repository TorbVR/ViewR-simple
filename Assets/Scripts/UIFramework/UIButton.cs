/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class UIButton : UIElement
{
    public event System.Action TappedOn;
    
    /// <summary>
    /// Initializes a new instance button UI
    /// <param name='rect'> specifies button size
    /// <param name='path'> specifies path for assets to load from Resources
    public UIButton(Rect rect, string[] path)
    {
        this.mRect = rect;
        mStyle = new GUIStyle();
        mStyle.normal.background = Resources.Load(path[0]) as Texture2D;
        mStyle.active.background = Resources.Load(path[1]) as Texture2D;
        mStyle.onNormal.background = Resources.Load(path[1]) as Texture2D;
    }
    
    public void Draw()
    {
        if(GUI.Button(mRect, "", mStyle))
        {
            if(this.TappedOn != null){
                this.TappedOn();
            }
        }
    }
    
    private Rect mRect;
    private GUIStyle mStyle;
}
