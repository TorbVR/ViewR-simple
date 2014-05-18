/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class UIRadioButton : UIElement 
{
    public event System.Action<int> TappedOnOption;
    
    /// <summary>
    /// Constructs a UIElement with series of options to choose from; 
    /// Only one option is selected at a time. param index specifies which option is set to true
    /// param rect[] takes an array of UIRect dimentions for drawing of all options
    /// param path[,] takes an array of path names to load appropriate assets from Resources
    /// </param>
    public UIRadioButton(UIRect[] rect, int index, string[,] path)
    {
        if(index > rect.Length)
        {
            return; 
        }
        
        this.mRect = rect;
        mStyle = new GUIStyle[rect.Length];
        
        for(int i = 0; i < mStyle.Length; i++)
        {
            mStyle[i] = new GUIStyle();
            mStyle[i].normal.background = Resources.Load(path[i,0]) as Texture2D;
            mStyle[i].active.background = Resources.Load(path[i,1]) as Texture2D;
            mStyle[i].onNormal.background = Resources.Load(path[i,1]) as Texture2D;
        }
    
        mOptionsTapped = new bool[rect.Length];
        mOptionsSelected = new bool[rect.Length];
        
        mOptionsSelected[index] = true;
    }
    
    public void EnableIndex(int index)
    {
        if(index < mOptionsSelected.Length) {
            mOptionsSelected[index] = SetToTrue();
        }
    }
    
    private bool SetToTrue()
    {
        for(int i = 0 ; i < mOptionsSelected.Length; i++)
        {
            mOptionsSelected[i] = false;
        }
        return true;
    }
    
    public void Draw()
    {
        for(int i = 0 ; i < mRect.Length; i++)
        {
            mOptionsTapped[i] = GUI.Toggle(mRect[i].rect, mOptionsSelected[i], "", mStyle[i]);
            if(mOptionsTapped[i] && !mOptionsSelected[i])
            {
                mOptionsSelected[i] = SetToTrue();  
                if(this.TappedOnOption != null)
                {
                    this.TappedOnOption(i);
                }
            }
        }
    }
    
    private bool[] mOptionsTapped;
    private bool[] mOptionsSelected;
    private UIRect[] mRect;
    private bool mTappedOn;
    private bool mSelected;
    private GUIStyle[] mStyle;

}
