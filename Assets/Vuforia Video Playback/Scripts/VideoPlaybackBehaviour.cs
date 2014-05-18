/*==============================================================================
/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * Confidential and Proprietary – Qualcomm Connected Experiences, Inc. 
 * ==============================================================================*/

using UnityEngine;

/// <summary>
/// The VideoPlaybackBehaviour manages the appearance of a video that can be superimposed on a target.
/// Playback controls are shown on top of it to control the video. 
/// </summary>
public class VideoPlaybackBehaviour : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// URL of the video, either a path to a local file or a remote address
    /// </summary>
    public string m_path = null;

    /// <summary>
    /// Texture for the play icon
    /// </summary>
    public Texture m_playTexture = null;

    /// <summary>
    /// Texture for the busy icon
    /// </summary>
    public Texture m_busyTexture = null;

    /// <summary>
    /// Texture for the error icon
    /// </summary>
    public Texture m_errorTexture = null;

    /// <summary>
    /// Define whether video should automatically start
    /// </summary>
    public bool m_autoPlay = false;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private VideoPlayerHelper mVideoPlayer = null;
    private bool mIsInited = false;
    private bool mIsPrepared = false;

    private Texture2D mVideoTexture = null;

    [SerializeField]
    [HideInInspector]
    private Texture mKeyframeTexture = null;

    private VideoPlayerHelper.MediaType mMediaType =
            VideoPlayerHelper.MediaType.ON_TEXTURE_FULLSCREEN;

    private VideoPlayerHelper.MediaState mCurrentState =
            VideoPlayerHelper.MediaState.NOT_READY;

    private float mSeekPosition = 0.0f;

    private bool isPlayableOnTexture;

    private GameObject mIconPlane = null;
    private bool mIconPlaneActive = false;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    /// <summary>
    /// Returns the video player
    /// </summary>
    public VideoPlayerHelper VideoPlayer
    {
        get { return mVideoPlayer; }
    }

    /// <summary>
    /// Returns the current playback state
    /// </summary>
    public VideoPlayerHelper.MediaState CurrentState
    {
        get { return mCurrentState; }
    }

    /// <summary>
    /// Type of playback (on-texture only, fullscreen only, or both)
    /// </summary>
    public VideoPlayerHelper.MediaType MediaType
    {
        get { return mMediaType; }
        set { mMediaType = value; }
    }

    /// <summary>
    /// Texture displayed before video playback begins
    /// </summary>
    public Texture KeyframeTexture
    {
        get { return mKeyframeTexture; }
        set { mKeyframeTexture = value; }
    }


    /// <summary>
    /// Returns whether the video should automatically start
    /// </summary>
    public bool AutoPlay
    {
        get { return m_autoPlay; }
    }

    #endregion // PROPERTIES



    #region UNITY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        // Find the icon plane (child of this object)
        mIconPlane = transform.Find("Icon").gameObject;

        // A filename or url must be set in the inspector
        if (m_path == null || m_path.Length == 0)
        {
            Debug.Log("Please set a video url in the Inspector");
            HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
            mCurrentState = VideoPlayerHelper.MediaState.ERROR;
            this.enabled = false;
        }
        else
        {
            // Set the current state to Not Ready
            HandleStateChange(VideoPlayerHelper.MediaState.NOT_READY);
            mCurrentState = VideoPlayerHelper.MediaState.NOT_READY;
        }
        // Create the video player and set the filename
        mVideoPlayer = new VideoPlayerHelper();
        mVideoPlayer.SetFilename(m_path);

        // Flip the plane as the video texture is mirrored on the horizontal
        transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x),
                transform.localScale.y, transform.localScale.z);

        // Scale the icon
        ScaleIcon();
    }

    void OnRenderObject()
    {
        if (!mIsInited)
        {
            // Initialize the video player
            if (mVideoPlayer.Init() == false)
            {
                Debug.Log("Could not initialize video player");
                HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
                this.enabled = false;
                return;
            }

            // Initialize the video texture
            InitVideoTexture();

            // Load the video
            if (mVideoPlayer.Load(m_path, mMediaType, false, 0) == false)
            {
                Debug.Log("Could not load video '" + m_path + "' for media type " + mMediaType);
                HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
                this.enabled = false;
                return;
            }

            // Successfully initialized
            mIsInited = true;
        }
        else if (!mIsPrepared)
        {
            // Get the video player status
            VideoPlayerHelper.MediaState state = mVideoPlayer.GetStatus();

            if (state == VideoPlayerHelper.MediaState.ERROR)
            {
                Debug.Log("Could not load video '" + m_path + "' for media type " + mMediaType);
                HandleStateChange(VideoPlayerHelper.MediaState.ERROR);
                this.enabled = false;
            }
            else if (state < VideoPlayerHelper.MediaState.NOT_READY)
            {
                // Video player is ready

                // Can we play this video on a texture?
                isPlayableOnTexture = mVideoPlayer.IsPlayableOnTexture();

                if (isPlayableOnTexture)
                {
                    // Pass the video texture id to the video player
                    int nativeTextureID = mVideoTexture.GetNativeTextureID();
                    mVideoPlayer.SetVideoTextureID(nativeTextureID);

                    // Get the video width and height
                    int videoWidth = mVideoPlayer.GetVideoWidth();
                    int videoHeight = mVideoPlayer.GetVideoHeight();

                    if (videoWidth > 0 && videoHeight > 0)
                    {
                        // Scale the video plane to match the video aspect ratio
                        float aspect = videoHeight / (float) videoWidth;

                        // Flip the plane as the video texture is mirrored on the horizontal
                        transform.localScale = new Vector3(-0.1f, 0.1f, 0.1f * aspect);
                    }

                    // Seek ahead if necessary
                    if (mSeekPosition > 0)
                    {
                        mVideoPlayer.SeekTo(mSeekPosition);
                    }
                }
                else
                {
                    // Handle the state change
                    state = mVideoPlayer.GetStatus();
                    HandleStateChange(state);
                    mCurrentState = state;
                }

                // Scale the icon
                ScaleIcon();

                // Video is prepared, ready for playback
                mIsPrepared = true;
            }
        }
        else
        {
            if (isPlayableOnTexture)
            {
                // Update the video texture with the latest video frame
                VideoPlayerHelper.MediaState state = mVideoPlayer.UpdateVideoData();

                // Check for playback state change
                if (state != mCurrentState)
                {
                    HandleStateChange(state);
                    mCurrentState = state;
                }
            }
            else
            {
                // Get the current status
                VideoPlayerHelper.MediaState state = mVideoPlayer.GetStatus();
                
                // Check for playback state change
                if (state != mCurrentState)
                {
                    HandleStateChange(state);
                    mCurrentState = state;
                }
            }
        }

        CheckIconPlaneVisibility();
    }


    void OnApplicationPause(bool pause)
    {
        if (!mIsInited)
            return;

        if (pause)
        {
            // Handle pause event natively
            mVideoPlayer.OnPause();

            // Store the playback position for later
            mSeekPosition = mVideoPlayer.GetCurrentPosition();

            // Deinit the video
            mVideoPlayer.Deinit();

            // Reset initialization parameters
            mIsInited = false;
            mIsPrepared = false;

            // Set the current state to Not Ready
            HandleStateChange(VideoPlayerHelper.MediaState.NOT_READY);
            mCurrentState = VideoPlayerHelper.MediaState.NOT_READY;
        }
    }


    void OnDestroy()
    {
        // Deinit the video
        mVideoPlayer.Deinit();
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region PUBLIC_METHODS

    /// <summary>
    /// Displays the busy icon on top of the video
    /// </summary>
    public void ShowBusyIcon()
    {
        mIconPlane.renderer.material.mainTexture = m_busyTexture;
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Initialize the video texture
    private void InitVideoTexture()
    {
        // Create texture of size 0 that will be updated in the plugin (we allocate buffers in native code)
        mVideoTexture = new Texture2D(0, 0, TextureFormat.RGB565, false);
        mVideoTexture.filterMode = FilterMode.Bilinear;
        mVideoTexture.wrapMode = TextureWrapMode.Clamp;
    }


    // Handle video playback state changes
    private void HandleStateChange(VideoPlayerHelper.MediaState newState)
    {
        // If the movie is playing or paused render the video texture
        // Otherwise render the keyframe
        if (newState == VideoPlayerHelper.MediaState.PLAYING ||
            newState == VideoPlayerHelper.MediaState.PAUSED)
        {
            renderer.material.mainTexture = mVideoTexture;
            renderer.material.mainTextureScale = new Vector2(1, 1);
        }
        else
        {
            if (mKeyframeTexture != null)
            {
                renderer.material.mainTexture = mKeyframeTexture;
                renderer.material.mainTextureScale = new Vector2(1, -1);
            }
        }

        // Display the appropriate icon, or disable if not needed
        switch (newState)
        {
            case VideoPlayerHelper.MediaState.READY:
            case VideoPlayerHelper.MediaState.REACHED_END:
            case VideoPlayerHelper.MediaState.PAUSED:
            case VideoPlayerHelper.MediaState.STOPPED:
                mIconPlane.renderer.material.mainTexture = m_playTexture;
                mIconPlaneActive = true;
                break;

            case VideoPlayerHelper.MediaState.NOT_READY:
            case VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN:
                mIconPlane.renderer.material.mainTexture = m_busyTexture;
                mIconPlaneActive = true;
                break;

            case VideoPlayerHelper.MediaState.ERROR:
                mIconPlane.renderer.material.mainTexture = m_errorTexture;
                mIconPlaneActive = true;
                break;

            default:
                mIconPlaneActive = false;
                break;
        }

        if (newState == VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN)
        {
            // Switching to full screen, disable QCARBehaviour (only applicable for iOS)
            QCARBehaviour qcarBehaviour = (QCARBehaviour) FindObjectOfType(typeof(QCARBehaviour));
            qcarBehaviour.enabled = false;
        }
        else if (mCurrentState == VideoPlayerHelper.MediaState.PLAYING_FULLSCREEN)
        {
            // Switching away from full screen, enable QCARBehaviour (only applicable for iOS)
            QCARBehaviour qcarBehaviour = (QCARBehaviour) FindObjectOfType(typeof(QCARBehaviour));
            qcarBehaviour.enabled = true;
        }
    }


    private void ScaleIcon()
    {
        // Icon should fill 50% of the narrowest side of the video

        float videoWidth = Mathf.Abs(transform.localScale.x);
        float videoHeight = Mathf.Abs(transform.localScale.z);
        float iconWidth, iconHeight;

        if (videoWidth > videoHeight)
        {
            iconWidth = 0.5f * videoHeight / videoWidth;
            iconHeight = 0.5f;
        }
        else
        {
            iconWidth = 0.5f;
            iconHeight = 0.5f * videoWidth / videoHeight;
        }

        mIconPlane.transform.localScale = new Vector3(-iconWidth, 1.0f, iconHeight);
    }


    private void CheckIconPlaneVisibility()
    {
        // If the video object renderer is currently enabled, we might need to toggle the icon plane visibility
        if (renderer.enabled)
        {
            // Check if the icon plane renderer has to be disabled explicitly in case it was enabled by another script (e.g. TrackableEventHandler)
            if (mIconPlane.renderer.enabled != mIconPlaneActive)
                mIconPlane.renderer.enabled = mIconPlaneActive;
        }
    }

    #endregion // PRIVATE_METHODS
}
