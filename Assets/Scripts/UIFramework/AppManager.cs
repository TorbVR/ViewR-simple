/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class manages different views in the scene like AboutPage, SplashPage and ARCameraView.
/// All of its Init, Update and Draw calls take place via SceneManager's Monobehaviour calls to ensure proper sync across all updates
/// </summary>
public class AppManager : MonoBehaviour
{

		public GameObject go;
    
    #region PUBLIC_MEMBER_VARIABLES
		public string TitleForAboutPage = "About";
		public UIEventHandler m_UIEventHandler;
    #endregion PUBLIC_MEMBER_VARIABLES
    
    #region PROTECTED_MEMBER_VARIABLES
		public static ViewType mActiveViewType;
		public enum ViewType
		{
				SPLASHVIEW,
				SNDSPLASHVIEW,
				ABOUTVIEW,
				UIVIEW,
				ARCAMERAVIEW}
		;
    #endregion PROTECTED_MEMBER_VARIABLES
    
    #region PRIVATE_MEMBER_VARIABLES
		private SplashScreenView mSplashView;
		private SecondSplashScreenView mSecondSplashView;
		private AboutScreenView mAboutView;
		private float mSecondsVisible = 2.0f;
    #endregion PRIVATE_MEMBER_VARIABLES

		private static AppManager instance;
    
		//This gets called from SceneManager's Start() 
		public virtual void InitManager ()
		{
				instance = this;
				mSplashView = new SplashScreenView ();
				mSecondSplashView = new SecondSplashScreenView ();
				mAboutView = new AboutScreenView ();
				mAboutView.SetTitle (TitleForAboutPage);
				mAboutView.OnStartButtonTapped += OnAboutStartButtonTapped;
				m_UIEventHandler.CloseView += OnTappedOnCloseButton;
				m_UIEventHandler.GoToAboutPage += OnTappedOnGoToAboutPage;
				InputController.SingleTapped += OnSingleTapped;
				InputController.DoubleTapped += OnDoubleTapped;
				InputController.BackButtonTapped += OnBackButtonTapped;
        
				mSplashView.LoadView ();
				StartCoroutine (LoadAboutPageForFirstTime ());
				mActiveViewType = ViewType.SPLASHVIEW;
				m_UIEventHandler.Bind ();
		}
    
		public static AppManager getInstance ()
		{
				return instance;
		}

		public virtual void UpdateManager ()
		{
				//Does nothing but anyone extending AppManager can run their update calls here
		}
    
		public virtual void Draw ()
		{
				m_UIEventHandler.UpdateView (false);
				switch (mActiveViewType) {

				case ViewType.SNDSPLASHVIEW:
						mSecondSplashView.UpdateUI(true);
						break;
				
				case ViewType.SPLASHVIEW:
						mSplashView.UpdateUI (true);
						break;
            
				case ViewType.ABOUTVIEW:
						mAboutView.UpdateUI (true);
						break;
            
				case ViewType.UIVIEW:
						m_UIEventHandler.UpdateView (true);
						break;
            
				case ViewType.ARCAMERAVIEW:
						break;
				}
		}
    
    #region UNITY_MONOBEHAVIOUR_METHODS
    
		void OnApplicationPause (bool tf)
		{
				//On hitting the home button, the app tends to turn off the flash
				//So, setting the UI to reflect that
				m_UIEventHandler.SetToDefault (tf);
		}
    
    #endregion UNITY_MONOBEHAVIOUR_METHODS
    
    #region PRIVATE_METHODS
    
		public void OnSingleTapped ()
		{
				if (mActiveViewType == ViewType.ARCAMERAVIEW) {
						// trigger focus once
						m_UIEventHandler.TriggerAutoFocus ();
				}
		}

		public void OnSingleTappedBack ()
		{
				if (mActiveViewType == ViewType.ARCAMERAVIEW) {
						// trigger focus once
						m_UIEventHandler.TriggerAutoFocus ();
				}
		}
    
		private void OnDoubleTapped ()
		{
				if (mActiveViewType == ViewType.ARCAMERAVIEW) {
						mActiveViewType = ViewType.UIVIEW;
				}
		}
    
		private void OnTappedOnGoToAboutPage ()
		{
				mActiveViewType = ViewType.ABOUTVIEW;   
		}
    
		private void OnBackButtonTapped ()
		{
				if (mActiveViewType == ViewType.ABOUTVIEW) {
						Application.Quit ();
				} else if (mActiveViewType == ViewType.UIVIEW) { //Hide UIMenu and Show ARCameraView
						mActiveViewType = ViewType.ARCAMERAVIEW;
				} else if (mActiveViewType == ViewType.ARCAMERAVIEW) { //if it's in ARCameraView
						mActiveViewType = ViewType.ABOUTVIEW;
				}
        
		}
    
		private void OnTappedOnCloseButton ()
		{
				mActiveViewType = ViewType.ARCAMERAVIEW;
		}
    
		private void OnAboutStartButtonTapped ()
		{
				mActiveViewType = ViewType.ARCAMERAVIEW;
		}
    
		private IEnumerator LoadAboutPageForFirstTime ()
		{
				yield return new WaitForSeconds (mSecondsVisible);
				mSplashView.UnLoadView ();
				//mActiveViewType = ViewType.SNDSPLASHVIEW;
				//mSecondSplashView.LoadView ();
				//yield return new WaitForSeconds (mSecondsVisible);
				//mSecondSplashView.UnLoadView ();
				mActiveViewType = ViewType.ABOUTVIEW;
				mAboutView.LoadView ();
				yield return null;
		}
    #endregion PRIVATE_METHODS
    
}
