/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// About screen manager.
/// 
/// Draws the UI for the About Screen handling different
/// screen sizes and dpis
/// </summary>
public class AboutScreenView : UIView
{
    #region PUBLIC_MEMBER_VARIABLES
    public TextAsset m_AboutText;
    public System.Action OnStartButtonTapped;
    #endregion PUBLIC_MEMBER_VARIABLES
    
    #region PRIVATE_MEMBER_VARIABLES
    GUIStyle mAboutTitleBgStyle;
    GUIStyle mOKButtonBgStyle;
    
    private string mTitle;
    private const float ABOUT_TEXT_MARGIN = 20.0f;
    private const float START_BUTTON_VERTICAL_MARGIN = 10.0f;
    private GUISkin mUISkin;
    private Dictionary<string, GUIStyle> mButtonGUIStyles;
    private Vector2 mScrollPosition;
    private float mStartButtonAreaHeight = 80.0f;
    private float mAboutTitleHeight = 80.0f;
    private Vector2 mLastTouchPosition;
    public UIBox mBox;
    private static float DeviceDependentScale
    {
         get { if ( Screen.width > Screen.height)
                return Screen.height / 480f;
              else 
                return Screen.width / 480f; 
        }
    }
    #endregion PRIVATE_MEMBER_VARIABLES
    
    public void SetTitle(string title)
    {
        mTitle = title; 
    }
    
    #region UIView implementation
    public void LoadView ()
    {
        m_AboutText = Resources.Load("Vuforia_About") as TextAsset;
        mBox = new UIBox(UIConstants.BoxRect, UIConstants.MainBackground);
        mAboutTitleBgStyle = new GUIStyle();
        mOKButtonBgStyle = new GUIStyle();
        mAboutTitleBgStyle.normal.background = Resources.Load ("UserInterface/grayTexture") as Texture2D;
        mOKButtonBgStyle.normal.background = Resources.Load ("UserInterface/capture_button_normal_XHigh") as Texture2D;
        
        mAboutTitleBgStyle.font = Resources.Load("SourceSansPro-Regular_big_xhdpi") as Font;
        mOKButtonBgStyle.font = Resources.Load("SourceSansPro-Regular_big_xhdpi") as Font;
        
        if(Screen.dpi > 300 ){
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkinsXHDPI") as GUISkin;
            mUISkin.label.font = Resources.Load("SourceSansPro-Regular") as Font;
            mAboutTitleBgStyle.font = Resources.Load("SourceSansPro-Regular_big_xhdpi") as Font;
            mOKButtonBgStyle.font = Resources.Load("SourceSansPro-Regular_big_xhdpi") as Font;
         
        } else if(Screen.dpi > 260 ){
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkins") as GUISkin;
            mUISkin.label.font = Resources.Load("SourceSansPro-Regular") as Font;
            mAboutTitleBgStyle.font = Resources.Load("SourceSansPro-Regular_big_xhdpi") as Font;
            mOKButtonBgStyle.font = Resources.Load("SourceSansPro-Regular_big_xhdpi") as Font;
            
        }else{
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkinsSmall") as GUISkin;
            mUISkin.label.font = Resources.Load("SourceSansPro-Regular_Small") as Font;
            mAboutTitleBgStyle.font = Resources.Load("SourceSansPro-Regular") as Font;
            mOKButtonBgStyle.font = Resources.Load("SourceSansPro-Regular") as Font;
        }
        
        #if UNITY_IPHONE
        if(Screen.height > 1500 ){
            // Loads the XHDPI sources for the iPAd 3
            mUISkin = Resources.Load("UserInterface/ButtonSkinsiPad3") as GUISkin;
            mUISkin.label.font = Resources.Load("SourceSansPro-Regular_big_iPad3") as Font;
            mAboutTitleBgStyle.font = Resources.Load("SourceSansPro-Regular_big_iPad3") as Font;
            mOKButtonBgStyle.font = Resources.Load("SourceSansPro-Regular_big_iPad3") as Font;
        }

        #endif
        
        mOKButtonBgStyle.normal.textColor = Color.white;
        mAboutTitleBgStyle.alignment = TextAnchor.MiddleLeft;
        mOKButtonBgStyle.alignment = TextAnchor.MiddleCenter;
    }
    
    public void UpdateUI (bool tf)
    {
        if(!tf)
            return;
        float scale = 1*DeviceDependentScale;
        mAboutTitleHeight = 80.0f* scale;
        mBox.Draw();
        GUI.Box(new Rect(0,0,Screen.width,mAboutTitleHeight),string.Empty,mAboutTitleBgStyle);
        GUI.Box(new Rect(ABOUT_TEXT_MARGIN * DeviceDependentScale,0,Screen.width,mAboutTitleHeight),mTitle,mAboutTitleBgStyle);
        float width = Screen.width / 1.5f;
        //float height = startButtonStyle.normal.background.height * scale;
        float height = mOKButtonBgStyle.normal.background.height * scale;
        
        mStartButtonAreaHeight = height + 2*(START_BUTTON_VERTICAL_MARGIN * scale);
        float left = Screen.width/2 - width/2;
        float top = Screen.height - mStartButtonAreaHeight + START_BUTTON_VERTICAL_MARGIN * scale;
        
        GUI.skin = mUISkin;
        
        GUILayout.BeginArea(new Rect(ABOUT_TEXT_MARGIN * DeviceDependentScale,
                                 mAboutTitleHeight + 5 * DeviceDependentScale,
                                 Screen.width - (ABOUT_TEXT_MARGIN * DeviceDependentScale),
                                 Screen.height - ( mStartButtonAreaHeight) - mAboutTitleHeight - 5 * DeviceDependentScale));
        
        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, false, false, GUILayout.Width(Screen.width - (ABOUT_TEXT_MARGIN * DeviceDependentScale)), 
            GUILayout.Height (Screen.height - mStartButtonAreaHeight - mAboutTitleHeight));
    
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUILayout.Label(m_AboutText.text);
    
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.EndScrollView();
    
        GUILayout.EndArea();
    
        // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
        if (GUI.Button(new Rect(left, top, width, height), "OK" ,mOKButtonBgStyle))
        {
            if(this.OnStartButtonTapped != null)
            {
                this.OnStartButtonTapped();
            }
        }
    }
    
    public void UnLoadView ()
    {
    }
    
    #endregion UIView Implementation
    
}
