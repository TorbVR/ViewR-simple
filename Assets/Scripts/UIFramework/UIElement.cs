/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// Custom UIElement that encapsulates Unity GUI Elements and runs a custom Draw() call.
/// All UIElements for this application must implement this interface
/// </summary>
public interface UIElement 
{
    void Draw();
}

