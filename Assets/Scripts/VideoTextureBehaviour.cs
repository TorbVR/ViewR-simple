/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using System;
using UnityEngine;

/// <summary>
/// This Behaviour renders the video background from the camera in Unity.
/// </summary>
public class VideoTextureBehaviour : MonoBehaviour, IVideoBackgroundEventHandler
{
    #region PUBLIC_MEMBER_VARIABLES
    public Camera m_Camera = null;
    public int m_NumDivisions = 2;
    public event Action TextureChanged;
    #endregion // PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES
    private Texture2D mTexture = null;
    private QCARRenderer.VideoTextureInfo mTextureInfo;
    private ScreenOrientation mScreenOrientation;
    private int mScreenWidth = 0;
    private int mScreenHeight = 0;
    private bool mVideoBgConfigChanged = false;
    #endregion // PRIVATE_MEMBER_VARIABLES

    #region PUBLIC_METHODS

    /// <summary>
    /// Get the texture object containing the video
    /// </summary>
    public Texture GetTexture()
    {
        return mTexture;
    }

    /// <summary>
    ///  Get the scale of the image width inside the texture
    /// </summary>
    public float GetScaleFactorX()
    {
        return (float)mTextureInfo.imageSize.x / (float)mTextureInfo.textureSize.x;
    }

    /// <summary>
    /// Get the scale of the image height inside the texture
    /// </summary>
    public float GetScaleFactorY()
    {
        return (float)mTextureInfo.imageSize.y / (float)mTextureInfo.textureSize.y;
    }

    public void InitBehaviour()
    {
        // register for the OnVideoBackgroundConfigChanged event at the QCARBehaviour
        QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterVideoBgEventHandler(this);
        }

        // Use the main camera if one wasn't set in the Inspector
        if (m_Camera == null)
        {
            m_Camera = Camera.main;
        }

        // Ask the renderer to stop drawing the videobackground.
        QCARRenderer.Instance.DrawVideoBackground = false;
    }

    public void UpdateBehaviour()
    {
        if (mVideoBgConfigChanged && QCARRenderer.Instance.IsVideoBackgroundInfoAvailable())
        {
            // reset the video texture:
            if (mTexture != null)
                Destroy(mTexture);

            CreateAndSetVideoTexture();

            QCARRenderer.VideoTextureInfo texInfo = QCARRenderer.Instance.GetVideoTextureInfo();

            // Cache the info:
            mTextureInfo = QCARRenderer.Instance.GetVideoTextureInfo(); ;

            Debug.Log("VideoTextureInfo " + texInfo.textureSize.x + " " +
                texInfo.textureSize.y + " " + texInfo.imageSize.x + " " + texInfo.imageSize.y);

            // Create the video mesh
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            meshFilter.mesh = CreateVideoMesh(m_NumDivisions, m_NumDivisions);

            // Position the video mesh
            PositionVideoMesh();

            if (TextureChanged != null)
                TextureChanged();

            mVideoBgConfigChanged = false;
        }
    }

    public void CleanUp()
    {
        // unregister for the OnVideoBackgroundConfigChanged event at the QCARBehaviour
        QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.UnregisterVideoBgEventHandler(this);
        }

        // Revert to default video background rendering
        QCARRenderer.Instance.DrawVideoBackground = true;

        // remove texture pointer
        QCARRenderer.Instance.SetVideoBackgroundTexture(null);
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS
    /// <summary>
    /// This creates an emtpy texture and passes its texture id to native so that the video background can be rendered into it.
    /// </summary>
    private void CreateAndSetVideoTexture()
    {
        // Create texture of size 0 that will be updated in the plugin (we allocate buffers in native code)
		mTexture = new Texture2D(0, 0, TextureFormat.RGB565, false);

        mTexture.filterMode = FilterMode.Bilinear;
        mTexture.wrapMode = TextureWrapMode.Clamp;

        // Assign texture to the renderer
        renderer.material.mainTexture = mTexture;

        // Set the texture to render into:
        if (!QCARRenderer.Instance.SetVideoBackgroundTexture(mTexture))
        {
            Debug.Log("Failed to setVideoBackgroundTexture " + mTexture.GetNativeTextureID());
        }
        else
        {
            Debug.Log("Successfully setVideoBackgroundTexture " + +mTexture.GetNativeTextureID());
        }
    }

    // Create a video mesh with the given number of rows and columns
    // Minimum two rows and two columns
    private Mesh CreateVideoMesh(int numRows, int numCols)
    {
        Mesh mesh = new Mesh();

        // Build mesh:
        mesh.vertices = new Vector3[numRows * numCols];
        Vector3[] vertices = mesh.vertices;

        for (int r = 0; r < numRows; ++r)
        {
            for (int c = 0; c < numCols; ++c)
            {
                float x = (((float)c) / (float)(numCols - 1)) - 0.5F;
                float z = (1.0F - ((float)r) / (float)(numRows - 1)) - 0.5F;

                vertices[r * numCols + c].x = x * 2.0F;
                vertices[r * numCols + c].y = 0.0F;
                vertices[r * numCols + c].z = z * 2.0F;
            }
        }
        mesh.vertices = vertices;

        // Builds triangles:
        mesh.triangles = new int[numRows * numCols * 2 * 3];
        int triangleIndex = 0;

        // Setup UVs to match texture info:
        float scaleFactorX = (float)mTextureInfo.imageSize.x / (float)mTextureInfo.textureSize.x;
        float scaleFactorY = (float)mTextureInfo.imageSize.y / (float)mTextureInfo.textureSize.y;

        mesh.uv = new Vector2[numRows * numCols];

        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;

        QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));

        for (int r = 0; r < numRows - 1; ++r)
        {
            for (int c = 0; c < numCols - 1; ++c)
            {
                // p0-p3
                // |\ |
                // p2-p1

                int p0Index = r * numCols + c;
                int p1Index = r * numCols + c + numCols + 1;
                int p2Index = r * numCols + c + numCols;
                int p3Index = r * numCols + c + 1;

                triangles[triangleIndex++] = p0Index;
                triangles[triangleIndex++] = p1Index;
                triangles[triangleIndex++] = p2Index;

                triangles[triangleIndex++] = p1Index;
                triangles[triangleIndex++] = p0Index;
                triangles[triangleIndex++] = p3Index;

                uvs[p0Index] = new Vector2(((float)c) / ((float)(numCols - 1)) * scaleFactorX,
                                                ((float)r) / ((float)(numRows - 1)) * scaleFactorY);

                uvs[p1Index] = new Vector2(((float)(c + 1)) / ((float)(numCols - 1)) * scaleFactorX,
                                ((float)(r + 1)) / ((float)(numRows - 1)) * scaleFactorY);

                uvs[p2Index] = new Vector2(((float)c) / ((float)(numCols - 1)) * scaleFactorX,
                            ((float)(r + 1)) / ((float)(numRows - 1)) * scaleFactorY);

                uvs[p3Index] = new Vector2(((float)(c + 1)) / ((float)(numCols - 1)) * scaleFactorX,
                            ((float)r) / ((float)(numRows - 1)) * scaleFactorY);

                // mirror UV coordinates if necessary
                if (qcarBehaviour.VideoBackGroundMirrored)
                {
                    uvs[p0Index] = new Vector2(scaleFactorX - uvs[p0Index].x, uvs[p0Index].y);
                    uvs[p1Index] = new Vector2(scaleFactorX - uvs[p1Index].x, uvs[p1Index].y);
                    uvs[p2Index] = new Vector2(scaleFactorX - uvs[p2Index].x, uvs[p2Index].y);
                    uvs[p3Index] = new Vector2(scaleFactorX - uvs[p3Index].x, uvs[p3Index].y);
                }
            }
        }

        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.normals = new Vector3[mesh.vertices.Length];
        mesh.RecalculateNormals();

        return mesh;
    }

    // Scale and position the video mesh to fill the screen
    private void PositionVideoMesh()
    {
        // Cache the screen orientation and size
        mScreenOrientation = QCARRuntimeUtilities.ScreenOrientation;
        mScreenWidth = Screen.width;
        mScreenHeight = Screen.height;

        // Reset the rotation so the mesh faces the camera
        gameObject.transform.localRotation = Quaternion.AngleAxis(270.0f, Vector3.right);

        // Adjust the rotation for the current orientation
        if (mScreenOrientation == ScreenOrientation.Landscape)
        {
            gameObject.transform.localRotation *= Quaternion.identity;
        }
        else if (mScreenOrientation == ScreenOrientation.Portrait)
        {
            gameObject.transform.localRotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
        }
        else if (mScreenOrientation == ScreenOrientation.LandscapeRight)
        {
            gameObject.transform.localRotation *= Quaternion.AngleAxis(180.0f, Vector3.up);
        }
        else if (mScreenOrientation == ScreenOrientation.PortraitUpsideDown)
        {
            gameObject.transform.localRotation *= Quaternion.AngleAxis(270.0f, Vector3.up);
        }

        // Scale game object for full screen video image:
        gameObject.transform.localScale = new Vector3(1, 1, 1 * (float)mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x);

        // Set the scale of the orthographic camera to match the screen size:
        m_Camera.orthographic = true;

        // Visible portion of the image:
        float visibleHeight;
        if (ShouldFitWidth())
        {
            // should fit width is true, so we have to adjust the horizontal autographic size so that
            // the viewport covers the whole texture WIDTH.
            if (QCARRuntimeUtilities.IsPortraitOrientation)
            {
                // in portrait mode, the background is rotated by 90 degrees. It's actual height is
                // therefore 1, so we have to set the visible height so that the visible width results in 1.
                visibleHeight = (mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x) *
                                ((float)mScreenHeight / (float)mScreenWidth);
            }
            else
            {
                // in landscape mode, we have to set the visible height to the screen ratio to
                // end up with a visible width of 1.
                visibleHeight = (float)mScreenHeight / (float)mScreenWidth;
            }
        }
        else
        {
            // should fit width is true, so we have to adjust the horizontal autographic size so that
            // the viewport covers the whole texture HEIGHT.
            if (QCARRuntimeUtilities.IsPortraitOrientation)
            {
                // in portrait mode, texture height is 1
                visibleHeight = 1.0f;
            }
            else
            {
                // in landscape mode, the texture height will be this value (see above)
                visibleHeight = mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x;
            }
        }

        m_Camera.orthographicSize = visibleHeight;
    }

    // Returns true if the video mesh should be scaled to match the width of the screen
    // Returns false if the video mesh should be scaled to match the height of the screen
    private bool ShouldFitWidth()
    {
        float screenAspect = mScreenWidth / (float)mScreenHeight;
        float cameraAspect;
        if (QCARRuntimeUtilities.IsPortraitOrientation)
            cameraAspect = mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x;
        else
            cameraAspect = mTextureInfo.imageSize.x / (float)mTextureInfo.imageSize.y;

        return (screenAspect >= cameraAspect);
    }

    #endregion // PRIVATE_METHODS



    #region IVideoBackgroundEventHandler_IMPLEMENTATION

    /// <summary>
    /// reset the video background
    /// </summary>
    public void OnVideoBackgroundConfigChanged()
    {
        mVideoBgConfigChanged = true;
    }

    #endregion // IVideoBackgroundEventHandler_IMPLEMENTATION
}
