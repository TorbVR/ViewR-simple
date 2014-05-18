/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class UICheckButton : UIElement
{
    public event System.Action<bool> TappedOn;
    
    /// <summary>
    /// Constructs a UIElement with selected and unselected states 
    /// param rect[] takes an array of UIRect dimentions for drawing of all options
    /// param path[] takes an array of path names to load appropriate assets from Resources
    /// param selected specifies whether the element is in selected or unselected state
    /// </param>
    public UICheckButton(UIRect rect, bool selected, string[] path)
    {
        this.mRect = rect;
        mSelected = selected;
        mStyle = new GUIStyle();
        mStyle.normal.background = Resources.Load(path[0]) as Texture2D;
        mStyle.active.background = Resources.Load(path[1]) as Texture2D;
        mStyle.onNormal.background = Resources.Load(path[1]) as Texture2D;
    }
    
    public void Enable(bool tf)
    {
        mSelected = tf;
    }
    
    public bool IsEnabled
    {
        get {
            return mSelected;
        }
    }
    
    public void Draw()
    {
        mTappedOn = GUI.Toggle(mRect.rect, mSelected, "", mStyle);
        if(mTappedOn && !mSelected)
        {
            mSelected = true;
            if(this.TappedOn != null)
            {
                this.TappedOn(true);
            }
        }
        if(!mTappedOn && mSelected)
        {
            mSelected = false;
            if(this.TappedOn != null)
            {
                this.TappedOn(false);
            }
        }
    }
    
    private UIRect mRect;
    private bool mTappedOn;
    private bool mSelected;
    private GUIStyle mStyle;
}
