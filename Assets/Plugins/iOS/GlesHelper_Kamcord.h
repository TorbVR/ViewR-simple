#ifndef _KAMCORD_IPHONE_GLESHELPER_H_
#define _KAMCORD_IPHONE_GLESHELPER_H_

#ifdef UNITY_VERSION
#define KAMCORD_UNITY_VERSION UNITY_VERSION
#endif

#if KAMCORD_UNITY_VERSION >= 410
#include "GlesHelper.h"
#elif KAMCORD_UNITY_VERSION >= 350
#include "iPhone_GlesSupport.h"
#endif

#ifdef __cplusplus
extern "C" {
#endif
    void KamcordInitUnity();
    void ForceOrientationCheck();
    void KamcordDisable();
#ifdef __cplusplus
}
#endif

#if KAMCORD_UNITY_VERSION >= 410

void CreateUnityRenderBuffers_Kamcord(struct UnityRenderingSurface* surface);
void DestroyUnityRenderBuffers_Kamcord(struct UnityRenderingSurface* surface);
void PreparePresentRenderingSurface_Kamcord(struct UnityRenderingSurface* surface, EAGLContext* mainContext);
void SetupUnityDefaultFBO_Kamcord(struct UnityRenderingSurface* surface);

#elif KAMCORD_UNITY_VERSION >= 350

void CreateSurfaceGLES_Kamcord(struct EAGLSurfaceDesc * surface);
void DestroySurfaceGLES_Kamcord(struct EAGLSurfaceDesc* surface);
void PreparePresentSurfaceGLES_Kamcord(struct EAGLSurfaceDesc* surface);
void AfterPresentSurfaceGLES_Kamcord(struct EAGLSurfaceDesc* surface);

#if KAMCORD_UNITY_VERSION < 400

void CreateSurfaceMultisampleBuffersGLES_Kamcord(struct EAGLSurfaceDesc* surface);
void DestroySurfaceMultisampleBuffersGLES_Kamcord(struct EAGLSurfaceDesc* surface);

#endif

#endif

#endif
