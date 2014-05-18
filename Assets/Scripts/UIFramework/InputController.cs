/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This is a static class that dispatches events for all kinds of user-inputs like singletap, doubletap and backbutton
/// No need to instantiate it. Simply run its UpdateInput method from a different class and register for all its events
/// </summary>
public class InputController {
    
    #region PUBLIC_EVENTS
    public static System.Action DoubleTapped;
    public static System.Action SingleTapped;
    public static System.Action BackButtonTapped;
    #endregion PUBLIC_EVENTS
    
    #region PRIVATE_STATIC_VARIABLES
    static private float timeSinceBackEventDispatched;
    static private bool backButtonEventDispached;
    static private bool tapEventDispatched;
    static private float mMillisecondsSinceLastTap;
    static private int MAX_TAP_MILLISEC = 500;
    static private float MAX_TAP_DISTANCE_SCREEN_SPACE = 0.1f;
    static private bool mWaitingForSecondTap;
    static private Vector3 mFirstTapPosition;
    static private DateTime mFirstTapTime;
    #endregion PRIVATE_STATIC_VARIABLES
    
    #region PUBLIC_STATIC_METHODS
    
    /// <summary>
    /// Captures user inputs and dispatches events for singletap, doubletap and backbuttontap
    /// </summary>
    public static void UpdateInput()
    {
        //We need to limit it in a way that the event gets dispatched only once even when the application might register the input multiple times within a short time frame
        if(Time.time - timeSinceBackEventDispatched > 1.0f) {
            backButtonEventDispached = false;
        }
        
        if(Input.GetKey(KeyCode.Escape) && !backButtonEventDispached)
        {
            //User tapped on back button
            if(InputController.BackButtonTapped != null)
            {
                InputController.BackButtonTapped();
            }
            backButtonEventDispached = true;
            timeSinceBackEventDispatched = Time.time;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            tapEventDispatched = false;
            if (mWaitingForSecondTap)
            {
                // check if time and position match:
                int smallerScreenDimension = Screen.width < Screen.height ? Screen.width : Screen.height;
                if (DateTime.Now - mFirstTapTime < TimeSpan.FromMilliseconds(MAX_TAP_MILLISEC) &&
                    Vector4.Distance(Input.mousePosition, mFirstTapPosition) < smallerScreenDimension*MAX_TAP_DISTANCE_SCREEN_SPACE)
                {
                    // it's a double tap
                    if(InputController.DoubleTapped != null)
                    {
                        InputController.DoubleTapped();
                    }
                    //Debug.Log ("Double Tap Registered");
                    tapEventDispatched = true;
                }
                else
                {
                    // too late/far to be a double tap, treat it as first tap:
                    mFirstTapPosition = Input.mousePosition;
                    mFirstTapTime = DateTime.Now;
                }
            }
            else
            {
                // it's the first tap
                mWaitingForSecondTap = true;
                mFirstTapPosition = Input.mousePosition;
                mFirstTapTime = DateTime.Now;
            }
        }
        else
        {
            // time window for second tap has passed, trigger single tap
            if (!tapEventDispatched && mWaitingForSecondTap && DateTime.Now - mFirstTapTime > TimeSpan.FromMilliseconds(MAX_TAP_MILLISEC))
            {
                if(InputController.SingleTapped != null)
                {
                    InputController.SingleTapped();
                }
               // Debug.Log ("Single Tap Registered");
                tapEventDispatched = true;
                mFirstTapPosition = Input.mousePosition;
                mFirstTapTime = DateTime.Now;
                mWaitingForSecondTap = false;
            }
        }
        
    }
    
    #endregion PUBLIC_STATIC_METHODS
}
