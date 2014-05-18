//
//  KamcordUnityImport.h
//  Unity-iPhone
//
//  Created by Kevin Wang on 11/12/13.
//
//

#ifndef Unity_iPhone_KamcordUnityImport_h
#define Unity_iPhone_KamcordUnityImport_h

#ifdef UNITY_VERSION
#define KAMCORD_UNITY_VERSION UNITY_VERSION
#endif

#if KAMCORD_UNITY_VERSION >= 410

#define CreateUnityRenderBuffers CreateUnityRenderBuffers_Kamcord
#define DestroyUnityRenderBuffers DestroyUnityRenderBuffers_Kamcord
#define PreparePresentRenderingSurface PreparePresentRenderingSurface_Kamcord
#define SetupUnityDefaultFBO SetupUnityDefaultFBO_Kamcord

#elif KAMCORD_UNITY_VERSION >= 350

#define CreateSurfaceGLES CreateSurfaceGLES_Kamcord
#define DestroySurfaceGLES DestroySurfaceGLES_Kamcord
#define PreparePresentSurfaceGLES PreparePresentSurfaceGLES_Kamcord
#define AfterPresentSurfaceGLES AfterPresentSurfaceGLES_Kamcord

#if KAMCORD_UNITY_VERSION < 400

#define CreateSurfaceMultisampleBuffersGLES CreateSurfaceMultisampleBuffersGLES_Kamcord
#define DestroySurfaceMultisampleBuffersGLES DestroySurfaceMultisampleBuffersGLES_Kamcord

#endif

#endif

#endif
