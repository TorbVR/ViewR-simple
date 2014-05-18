/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System.Collections;

/// <summary>
/// Custom rectangle for drawing Unity GUI elements. It scales everything to device screen width.
/// </summary>
public class UIRect
{
    public Rect rect
    {
        get {
            return new Rect(mX * Screen.width, mY * Screen.width, mWidth * Screen.width, mHeight * Screen.width);
        }
    }
    
    public UIRect(float x, float y, float W, float H)
    {
        mX = x;
        mY = y;
        mWidth = W;
        mHeight = H;
    }
    
    private float mX;
    private float mY;
    private float mWidth;
    private float mHeight;
    
}
