/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class VideoPlaybackAppManager : AppManager {
    
    public override void InitManager ()
    {
        base.InitManager ();
        InputController.SingleTapped += HandleTap;
        InputController.DoubleTapped += HandleDoubleTap;
    }
    
    public override void UpdateManager ()
    {
        base.UpdateManager ();
        
        // special handling in play mode:
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (PickVideo(Input.mousePosition) != null)
                    Debug.LogWarning("Playing videos is currently not supported in Play Mode.");
            }
        }
    }
    
     #region PRIVATE_METHODS

    /// <summary>
    /// Handle single tap event
    /// </summary>
    private void HandleTap()
    {
        if(mActiveViewType == AppManager.ViewType.ARCAMERAVIEW) {
        // Find out which video was tapped, if any
        VideoPlaybackBehaviour video = PickVideo(Input.mousePosition);

        if (video != null)
        {
            if (video.VideoPlayer.IsPlayableOnTexture())
            {
                // This video is playable on a texture, toggle playing/paused

                VideoPlayerHelper.MediaState state = video.VideoPlayer.GetStatus();
                if (state == VideoPlayerHelper.MediaState.PAUSED ||
                    state == VideoPlayerHelper.MediaState.READY ||
                    state == VideoPlayerHelper.MediaState.STOPPED)
                {
                    // Pause other videos before playing this one
                    PauseOtherVideos(video);

                    // Play this video on texture where it left off
                    video.VideoPlayer.Play(false, video.VideoPlayer.GetCurrentPosition());
                }
                else if (state == VideoPlayerHelper.MediaState.REACHED_END)
                {
                    // Pause other videos before playing this one
                    PauseOtherVideos(video);

                    // Play this video from the beginning
                    video.VideoPlayer.Play(false, 0);
                }
                else if (state == VideoPlayerHelper.MediaState.PLAYING)
                {
                    // Video is already playing, pause it
                    video.VideoPlayer.Pause();
                }
            }
            else
            {
                // Display the busy icon
                video.ShowBusyIcon();
                
                // This video cannot be played on a texture, play it full screen
                video.VideoPlayer.Play(true, 0);
            }
        }
        }
    }


    /// <summary>
    /// Handle double tap event
    /// </summary>
    private void HandleDoubleTap()
    {
        // Find out which video was tapped, if any
        VideoPlaybackBehaviour video = PickVideo(Input.mousePosition);

        if (video != null)
        {
            AppManager.mActiveViewType = AppManager.ViewType.ARCAMERAVIEW;
            if (video.VideoPlayer.IsPlayableFullscreen())
            {
                // Pause the video if it is currently playing
                video.VideoPlayer.Pause();

                // Seek the video to the beginning();
                video.VideoPlayer.SeekTo(0.0f);

                // Display the busy icon
                video.ShowBusyIcon();

                // Play the video full screen
                video.VideoPlayer.Play(true, 0);
                
                UpdateFlashSettingsInUIView();
            }
        }
    }
    
    //Flash turns off automatically on fullscreen videoplayback mode, so we need to update the UI accordingly
    private void UpdateFlashSettingsInUIView()
    {
        VideoPlaybackUIEventHandler handler = GameObject.FindObjectOfType(typeof(VideoPlaybackUIEventHandler)) as VideoPlaybackUIEventHandler;
        handler.View.mCameraFlashSettings.Enable(false);
    }


    /// <summary>
    /// Find the video object under the screen point
    /// </summary>
    private VideoPlaybackBehaviour PickVideo(Vector3 screenPoint)
    {
        VideoPlaybackBehaviour[] videos = (VideoPlaybackBehaviour[])
                FindObjectsOfType(typeof(VideoPlaybackBehaviour));

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit = new RaycastHit();

        foreach (VideoPlaybackBehaviour video in videos)
        {
            if (video.collider.Raycast(ray, out hit, 10000))
            {
                return video;
            }
        }

        return null;
    }


    /// <summary>
    /// Pause all videos except this one
    /// </summary>
    private void PauseOtherVideos(VideoPlaybackBehaviour currentVideo)
    {
        VideoPlaybackBehaviour[] videos = (VideoPlaybackBehaviour[])
                FindObjectsOfType(typeof(VideoPlaybackBehaviour));

        foreach (VideoPlaybackBehaviour video in videos)
        {
            if (video != currentVideo)
            {
                if (video.CurrentState == VideoPlayerHelper.MediaState.PLAYING)
                {
                    video.VideoPlayer.Pause();
                }
            }
        }
    }

    #endregion // PRIVATE_METHODS


}
