#include <OpenGLES/ES1/gl.h>
#include <OpenGLES/ES1/glext.h>
#include <OpenGLES/ES2/gl.h>
#include <OpenGLES/ES2/glext.h>

#include <stdio.h>

#include "GlesHelper_Kamcord.h"
#include "iPhone_Profiler.h"

#if KAMCORD_UNITY_VERSION >= 410
#include "iPhone_View.h"
#include "EAGLContextHelper.h"
#endif

#import <Kamcord/KamcordRecorder.h>

extern 	bool			_supportsDiscard;
extern 	bool			_supportsMSAA;

void	UnityCaptureScreenshot();
bool	UnityIsCaptureScreenshotRequested();

void    UnityBlitToSystemFB(unsigned tex, unsigned w, unsigned h, unsigned sysw, unsigned sysh);

#if KAMCORD_UNITY_VERSION < 430
extern void UnityPause(bool pause);
#else
extern "C" void UnityPause(bool pause);
#endif


#ifdef __cplusplus
extern "C" {
#endif
    
    void UnityPauseWrapper(bool pause)
    {
        UnityPause(pause);
    }
    
#ifdef __cplusplus
}
#endif


#if KAMCORD_UNITY_VERSION >= 350 && KAMCORD_UNITY_VERSION < 410

extern EAGLSurfaceDesc _surface;

int 	UnityGetDesiredMSAASampleCount(int defaultSampleCount);
bool    UnityUse32bitDisplayBuffer();

extern "C" void InitEAGLLayer(void* eaglLayer, bool use32bitColor);
extern "C" bool AllocateRenderBufferStorageFromEAGLLayer(void* eaglLayer);
extern "C" void DeallocateRenderBufferStorageFromEAGLLayer();

extern UIViewController* UnityGetGLViewController();
extern EAGLContext * _context;

#if KAMCORD_UNITY_VERSION < 400
enum ScreenOrientation
{
    kScreenOrientationUnknown,
    portrait,
    portraitUpsideDown,
    landscapeLeft,
    landscapeRight,
    autorotation,
    kScreenOrientationCount
};
#endif

#if KAMCORD_UNITY_VERSION >= 400
void 	UnityGetRenderingResolution(unsigned* w, unsigned* h);  // Unity
void    DestroyRenderingSurfaceGLES_Kamcord(EAGLSurfaceDesc* surface);  // Kamcord specific
#endif

#endif

#if KAMCORD_UNITY_VERSION < 430
enum EnabledOrientation
{
    kAutorotateToPortrait = 1,
    kAutorotateToPortraitUpsideDown = 2,
    kAutorotateToLandscapeLeft = 4,
    kAutorotateToLandscapeRight = 8
};
#endif
 
void ForceOrientationCheck()
{
    extern ScreenOrientation ConvertToUnityScreenOrientation(int hwOrient, EnabledOrientation* outAutorotOrient);
    extern void UnitySetScreenOrientation(ScreenOrientation orientation);
    
#if KAMCORD_UNITY_VERSION >= 400
    extern void RequestNativeOrientation(ScreenOrientation targetOrient);
#elif KAMCORD_UNITY_VERSION < 400
    extern void RequestNativeOrientation(int targetOrient);
#endif
    
    UIInterfaceOrientation orientIOS = UnityGetGLViewController().interfaceOrientation;
    ScreenOrientation orientation = ConvertToUnityScreenOrientation(orientIOS, 0);
    RequestNativeOrientation(orientation);
}


/*
 * The default framebuffer
 */
extern "C" GLint gDefaultFBO;


/*
 * Other Kamcord functionalities
 */

void KamcordDisable()
{
}


/*
 *
 * Unity 4.1+
 *
 */
#if KAMCORD_UNITY_VERSION >= 410

#import "DisplayManager.h"

/*
 * Initializes KamcordRecorder and sets the parentViewController
 */
void KamcordInitUnity()
{
    const UnityRenderingSurface * surface = &([[DisplayManager Instance] mainDisplay]->surface);
    if (surface)
    {
        [KamcordRecorder initWithEAGLContext:surface->context layer:surface->layer];
        [KamcordRecorder createFramebuffers:(surface->targetFB ? surface->targetFB : surface->systemFB)
                            msaaFramebuffer:(_supportsMSAA ? surface->msaaFB : 0)
                                primarySize:CGSizeMake(surface->targetW, surface->targetH)];
        [KamcordRecorder setParentViewController:UnityGetGLViewController()];
    }
}

void CreateUnityRenderBuffers_Kamcord(UnityRenderingSurface* surface)
{
	CreateUnityRenderBuffers(surface);
    
    [KamcordRecorder initWithEAGLContext:surface->context layer:surface->layer];
    [KamcordRecorder createFramebuffers:(surface->targetFB ? surface->targetFB : surface->systemFB)
                        msaaFramebuffer:(_supportsMSAA ? surface->msaaFB : 0)
                            primarySize:CGSizeMake(surface->targetW, surface->targetH)];
}

void DestroyUnityRenderBuffers_Kamcord(UnityRenderingSurface* surface)
{
	DestroyUnityRenderBuffers(surface);
    
    [KamcordRecorder deleteFramebuffers];
}

void PreparePresentRenderingSurface_Kamcord(UnityRenderingSurface* surface, EAGLContext* mainContext)
{
    // Use systemFB if no MSAA since we don't blit from target to system here
    if (![KamcordRecorder beforePresentRenderbuffer:surface->systemFB])
    {
        PreparePresentRenderingSurface(surface, mainContext);
    }
}

void SetupUnityDefaultFBO_Kamcord(UnityRenderingSurface* surface)
{
    extern GLint gDefaultFBO;
    
    if ([KamcordRecorder isRecording])
    {
        gDefaultFBO = [KamcordRecorder activeFramebuffer];
    }
    else
    {
        if (surface->msaaFB)            gDefaultFBO = surface->msaaFB;
        else if (surface->targetFB)     gDefaultFBO = surface->targetFB;
        else                            gDefaultFBO = surface->systemFB;
    }
    GLES_CHK(glBindFramebufferOES(GL_FRAMEBUFFER_OES, gDefaultFBO));
}

/*
 *
 * Unity 3.5 and 4.0
 *
 */
#elif KAMCORD_UNITY_VERSION >= 350

void KamcordInitUnity()
{
#if KAMCORD_UNITY_VERSION >= 400
    CGSize surfaceSize = CGSizeMake(_surface.targetW, _surface.targetH);
    CreateRenderingSurfaceGLES(&_surface);
    
    // Must do this after the above method, which may create the targetFramebuffer
    GLint framebuffer = (_surface.targetFramebuffer ? _surface.targetFramebuffer : _surface.systemFramebuffer);
#elif KAMCORD_UNITY_VERSION >= 350
    CGSize surfaceSize = CGSizeMake(_surface.w, _surface.h);
    CreateSurfaceMultisampleBuffersGLES(&_surface);
    
    GLint framebuffer = _surface.framebuffer;
#endif
    if ([KamcordRecorder initWithEAGLContext:_context layer:(CAEAGLLayer *)_surface.eaglLayer])
    {
        [KamcordRecorder createFramebuffers:framebuffer
                            msaaFramebuffer:(_supportsMSAA && _surface.msaaSamples > 1 ? _surface.msaaFramebuffer : 0)
                                primarySize:surfaceSize];
        [KamcordRecorder setParentViewController:UnityGetGLViewController()];
    }
    
    gDefaultFBO = [KamcordRecorder activeFramebuffer];
}

void CreateSurfaceGLES_Kamcord(EAGLSurfaceDesc * surface)
{
	GLuint oldRenderbuffer;
	GLES_CHK( glGetIntegerv(GL_RENDERBUFFER_BINDING_OES, (GLint*)&oldRenderbuffer) );
    
	DestroySurfaceGLES(surface);
    
	InitEAGLLayer(surface->eaglLayer, surface->use32bitColor);
    
    GLuint renderbuffer;
	GLES_CHK( glGenRenderbuffersOES(1, &renderbuffer) );
	GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, renderbuffer) );
    
	if( !AllocateRenderBufferStorageFromEAGLLayer(surface->eaglLayer) )
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &renderbuffer) );
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_BINDING_OES, oldRenderbuffer) );
        
		printf_console("FAILED allocating render buffer storage from gles context\n");
		return;
	}
    
    GLuint framebuffer;
	GLES_CHK( glGenFramebuffersOES(1, &framebuffer) );
	GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, framebuffer) );
	GLES_CHK( glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_COLOR_ATTACHMENT0_OES, GL_RENDERBUFFER_OES, renderbuffer) );

#if KAMCORD_UNITY_VERSION >= 400
    surface->systemFramebuffer  = framebuffer;
    surface->systemRenderbuffer = renderbuffer;
#elif KAMCORD_UNITY_VERSION >= 350
    surface->framebuffer    = framebuffer;
    surface->renderbuffer   = renderbuffer;
#endif
    
    gDefaultFBO = framebuffer;
    
    KamcordInitUnity();
}

void DestroySurfaceGLES_Kamcord(EAGLSurfaceDesc* surface)
{
#if KAMCORD_UNITY_VERSION >= 400
    if( surface->systemRenderbuffer )
	{
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->systemRenderbuffer) );
		DeallocateRenderBufferStorageFromEAGLLayer();
        
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, 0) );
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->systemRenderbuffer) );
        
		surface->systemRenderbuffer = 0;
	}
    
	if( surface->systemFramebuffer )
	{
		GLES_CHK( glDeleteFramebuffersOES(1, &surface->systemFramebuffer) );
		surface->systemFramebuffer = 0;
	}
    
	DestroyRenderingSurfaceGLES(surface);
    
	if(surface->depthbuffer)
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->depthbuffer) );
		surface->depthbuffer = 0;
	}
#elif KAMCORD_UNITY_VERSION >= 350
	if( surface->renderbuffer )
	{
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->renderbuffer) );
		DeallocateRenderBufferStorageFromEAGLLayer();
        
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, 0) );
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->renderbuffer) );
        
		surface->renderbuffer = 0;
	}
    
	if( surface->framebuffer )
	{
		GLES_CHK( glDeleteFramebuffersOES(1, &surface->framebuffer) );
		surface->framebuffer = 0;
	}
    
	DestroySurfaceMultisampleBuffersGLES(surface);
#endif
}

#if KAMCORD_UNITY_VERSION >= 400
void CreateRenderingSurfaceGLES_Kamcord(EAGLSurfaceDesc* surface)
{
    gDefaultFBO = (GLint)[KamcordRecorder activeFramebuffer];
    
	GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->systemFramebuffer) );
	GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->systemRenderbuffer) );
    
	DestroyRenderingSurfaceGLES_Kamcord(surface);
    
	GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->systemFramebuffer) );
	GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->systemRenderbuffer) );
    
	if( surface->targetW != surface->systemW || surface->targetH != surface->systemH )
	{
		GLES_CHK( glGenTextures(1, &surface->targetRT) );
		GLES_CHK( glBindTexture(GL_TEXTURE_2D, surface->targetRT) );
        GLES_CHK( glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GLES_UPSCALE_FILTER) );
        GLES_CHK( glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GLES_UPSCALE_FILTER) );
        GLES_CHK( glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE) );
        GLES_CHK( glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE) );
        
        GLenum fmt  = surface->use32bitColor ? GL_RGBA : GL_RGB;
        GLenum type = surface->use32bitColor ? GL_UNSIGNED_BYTE : GL_UNSIGNED_SHORT_5_6_5;
        GLES_CHK( glTexImage2D(GL_TEXTURE_2D, 0, fmt, surface->targetW, surface->targetH, 0, fmt, type, 0) );
        
		GLES_CHK( glGenFramebuffersOES(1, &surface->targetFramebuffer) );
		GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->targetFramebuffer) );
		GLES_CHK( glFramebufferTexture2DOES(GL_FRAMEBUFFER_OES, GL_COLOR_ATTACHMENT0_OES, GL_TEXTURE_2D, surface->targetRT, 0) );
        
		GLES_CHK( glBindTexture(GL_TEXTURE_2D, 0) );
		gDefaultFBO = surface->targetFramebuffer;
	}
    
#if GL_APPLE_framebuffer_multisample
	if(_supportsMSAA && surface->msaaSamples > 1)
	{
		GLES_CHK( glGenRenderbuffersOES(1, &surface->msaaRenderbuffer) );
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->msaaRenderbuffer) );
        
		GLES_CHK( glGenFramebuffersOES(1, &surface->msaaFramebuffer) );
		GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->msaaFramebuffer) );
        
		GLES_CHK( glRenderbufferStorageMultisampleAPPLE(GL_RENDERBUFFER_OES, surface->msaaSamples, surface->format, surface->targetW, surface->targetH) );
		GLES_CHK( glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_COLOR_ATTACHMENT0_OES, GL_RENDERBUFFER_OES, surface->msaaRenderbuffer) );
        
		gDefaultFBO = surface->msaaFramebuffer;
	}
#endif
    
	GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, gDefaultFBO) );
	if(surface->depthFormat != 0)
	{
		GLES_CHK( glGenRenderbuffersOES(1, &surface->depthbuffer) );
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->depthbuffer) );
        
		bool needMSAA = GL_APPLE_framebuffer_multisample && (surface->msaaSamples > 1);
        
#if GL_APPLE_framebuffer_multisample
		if(needMSAA)
			GLES_CHK( glRenderbufferStorageMultisampleAPPLE(GL_RENDERBUFFER_OES, surface->msaaSamples, surface->depthFormat, surface->targetW, surface->targetH) );
#endif
        
		if(!needMSAA)
			GLES_CHK( glRenderbufferStorageOES(GL_RENDERBUFFER_OES, surface->depthFormat, surface->targetW, surface->targetH) );
        
		GLES_CHK( glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_DEPTH_ATTACHMENT_OES, GL_RENDERBUFFER_OES, surface->depthbuffer) );
	}
}

void DestroyRenderingSurfaceGLES_Kamcord(EAGLSurfaceDesc* surface)
{
    if( (surface->msaaFramebuffer || surface->targetFramebuffer) && surface->depthbuffer )
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->depthbuffer) );
		surface->depthbuffer = 0;
	}
    
	if(surface->targetRT)
	{
		GLES_CHK( glDeleteTextures(1, &surface->targetRT) );
		surface->targetRT = 0;
	}
    
	if(surface->targetFramebuffer)
	{
		GLES_CHK( glDeleteFramebuffersOES(1, &surface->targetFramebuffer) );
		surface->targetFramebuffer = 0;
	}
    
	if(surface->msaaRenderbuffer)
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->msaaRenderbuffer) );
		surface->msaaRenderbuffer = 0;
	}
    
	if(surface->msaaFramebuffer)
	{
		GLES_CHK( glDeleteFramebuffersOES(1, &surface->msaaFramebuffer) );
		surface->msaaFramebuffer = 0;
	}
    
    [KamcordRecorder deleteFramebuffers];
}

#elif KAMCORD_UNITY_VERSION == 350

void CreateSurfaceMultisampleBuffersGLES_Kamcord(EAGLSurfaceDesc* surface)
{
	UNITY_DBG_LOG ("CreateSurfaceMultisampleBuffers: samples=%i\n", surface->msaaSamples);
	GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->renderbuffer) );
    
	UNITY_DBG_LOG ("glBindFramebuffer(GL_FRAMEBUFFER, %d) :: AppCtrl\n", surface->framebuffer);
	GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->framebuffer) );
    
    gDefaultFBO = (GLint)[KamcordRecorder activeFramebuffer];
    
	DestroySurfaceMultisampleBuffersGLES(surface);
    
	GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->renderbuffer) );
    
	UNITY_DBG_LOG ("glBindFramebuffer(GL_FRAMEBUFFER, %d) :: AppCtrl\n", surface->framebuffer);
	GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->framebuffer) );
    
#if GL_APPLE_framebuffer_multisample
	if (surface->msaaSamples > 1)
	{
		GLES_CHK( glGenRenderbuffersOES(1, &surface->msaaRenderbuffer) );
		GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->msaaRenderbuffer) );
        
		GLES_CHK( glGenFramebuffersOES(1, &surface->msaaFramebuffer) );
        
		UNITY_DBG_LOG ("glBindFramebuffer(GL_FRAMEBUFFER, %d) :: AppCtrl\n", surface->msaaFramebuffer);
		GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->msaaFramebuffer) );
        
		gDefaultFBO = surface->msaaFramebuffer;
        
		GLES_CHK( glRenderbufferStorageMultisampleAPPLE(GL_RENDERBUFFER_OES, surface->msaaSamples, surface->format, surface->w, surface->h) );
		GLES_CHK( glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_COLOR_ATTACHMENT0_OES, GL_RENDERBUFFER_OES, surface->msaaRenderbuffer) );
        
		if(surface->depthFormat)
		{
			GLES_CHK( glGenRenderbuffersOES(1, &surface->msaaDepthbuffer) );
			GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->msaaDepthbuffer) );
			GLES_CHK( glRenderbufferStorageMultisampleAPPLE(GL_RENDERBUFFER_OES, surface->msaaSamples, GL_DEPTH_COMPONENT16_OES, surface->w, surface->h) );
			GLES_CHK( glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_DEPTH_ATTACHMENT_OES, GL_RENDERBUFFER_OES, surface->msaaDepthbuffer) );
		}
	}
	else
#endif
	{
		if (surface->depthFormat)
		{
			GLES_CHK( glGenRenderbuffersOES(1, &surface->depthbuffer) );
			GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->depthbuffer) );
			GLES_CHK( glRenderbufferStorageOES(GL_RENDERBUFFER_OES, surface->depthFormat, surface->w, surface->h) );
            
			UNITY_DBG_LOG ("glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, %d) :: AppCtrl\n", surface->depthbuffer);
			GLES_CHK( glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_DEPTH_ATTACHMENT_OES, GL_RENDERBUFFER_OES, surface->depthbuffer) );
		}
	}
}

void DestroySurfaceMultisampleBuffersGLES_Kamcord(EAGLSurfaceDesc* surface)
{
	if(surface->depthbuffer)
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->depthbuffer) );
	}
	surface->depthbuffer = 0;
    
	if(surface->msaaDepthbuffer)
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->msaaDepthbuffer) );
	}
	surface->msaaDepthbuffer = 0;
    
	if(surface->msaaRenderbuffer)
	{
		GLES_CHK( glDeleteRenderbuffersOES(1, &surface->msaaRenderbuffer) );
	}
	surface->msaaRenderbuffer = 0;
    
	if (surface->msaaFramebuffer)
	{
		GLES_CHK( glDeleteFramebuffersOES(1, &surface->msaaFramebuffer) );
	}
	surface->msaaFramebuffer = 0;
    
    [KamcordRecorder deleteFramebuffers];
}

#endif

void PreparePresentSurfaceGLES_Kamcord(EAGLSurfaceDesc* surface)
{
#if KAMCORD_UNITY_VERSION >= 400
    GLint framebuffer = surface->systemFramebuffer;
#elif KAMCORD_UNITY_VERSION >= 350
    GLint framebuffer = surface->framebuffer;
#endif
    if (![KamcordRecorder beforePresentRenderbuffer:framebuffer])
    {
#if GL_APPLE_framebuffer_multisample
        if( surface->msaaSamples > 1 && _supportsMSAA )
        {
            Profiler_StartMSAAResolve();
            
            UNITY_DBG_LOG ("  ResolveMSAA: samples=%i msaaFBO=%i destFBO=%i\n", surface->msaaSamples, surface->msaaFramebuffer, kamcordInfo_->primaryOffscreenFramebuffer_);
            
            GLES_CHK( glBindFramebufferOES(GL_READ_FRAMEBUFFER_APPLE, surface->msaaFramebuffer) );
            GLES_CHK( glBindFramebufferOES(GL_DRAW_FRAMEBUFFER_APPLE, framebuffer) );
            
            GLES_CHK( glResolveMultisampleFramebufferAPPLE() );
            
            Profiler_EndMSAAResolve();
        }
#endif
        
        // update screenshot here
        if( UnityIsCaptureScreenshotRequested() )
        {
            GLint curfb = 0;
            GLES_CHK( glGetIntegerv(GL_FRAMEBUFFER_BINDING_OES, &curfb) );
            GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, framebuffer) );
            UnityCaptureScreenshot();
            GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, curfb) );
        }
        
#if KAMCORD_UNITY_VERSION >= 400
        if( surface->targetFramebuffer )
        {
            gDefaultFBO = surface->systemFramebuffer;
            GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, gDefaultFBO) );
            
            UnityBlitToSystemFB(surface->targetRT, surface->targetW, surface->targetH, surface->systemW, surface->systemH);
            
            gDefaultFBO = surface->msaaFramebuffer ? surface->msaaFramebuffer : surface->targetFramebuffer;
            GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, gDefaultFBO) );
        }
#endif
        
#if GL_EXT_discard_framebuffer
        if( _supportsDiscard )
        {
            GLenum	discardAttach[] = {GL_COLOR_ATTACHMENT0_OES, GL_DEPTH_ATTACHMENT_OES, GL_STENCIL_ATTACHMENT_OES};
            
            if ( surface->msaaFramebuffer )
            {
                GLES_CHK( glDiscardFramebufferEXT(GL_READ_FRAMEBUFFER_APPLE, 3, discardAttach) );
            }
            
#if KAMCORD_UNITY_VERSION >= 400
            if(surface->targetFramebuffer)
            {
                GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->targetFramebuffer) );
                GLES_CHK( glDiscardFramebufferEXT(GL_FRAMEBUFFER_OES, 3, discardAttach) );
            }
            
            GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->systemFramebuffer) );
            GLES_CHK( glDiscardFramebufferEXT(GL_FRAMEBUFFER_OES, 2, &discardAttach[1]) );
#elif KAMCORD_UNITY_VERSION >= 350
            GLenum target = (surface->msaaSamples > 1 && _supportsMSAA) ? GL_READ_FRAMEBUFFER_APPLE: GL_FRAMEBUFFER_OES;
            GLES_CHK( glDiscardFramebufferEXT(target, 3, discardAttach) );
#endif
            GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, gDefaultFBO) );
        }
#endif
    }
}

void AfterPresentSurfaceGLES_Kamcord(EAGLSurfaceDesc* surface)
{
    [KamcordRecorder afterPresentRenderbuffer];
    
#if KAMCORD_UNITY_VERSION >= 400
    GLuint renderbuffer  = surface->systemRenderbuffer;
#elif KAMCORD_UNITY_VERSION >= 350
    GLuint renderbuffer  = surface->renderbuffer;
#endif
    
    if (surface->use32bitColor != UnityUse32bitDisplayBuffer())
    {
        surface->use32bitColor = UnityUse32bitDisplayBuffer();
        CreateSurfaceGLES(surface);
        GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, renderbuffer) );
    }
    
#if KAMCORD_UNITY_VERSION >= 400
    if (NeedRecreateRenderingSurfaceGLES(surface))
    {
        UnityGetRenderingResolution(&surface->targetW, &surface->targetH);
        surface->msaaSamples = UnityGetDesiredMSAASampleCount(MSAA_DEFAULT_SAMPLE_COUNT);
        
        [KamcordRecorder deleteFramebuffers];
        KamcordInitUnity();
    }
#elif KAMCORD_UNITY_VERSION >= 350
#if GL_APPLE_framebuffer_multisample
    if (_supportsMSAA)
    {
        const int desiredMSAASamples = UnityGetDesiredMSAASampleCount(MSAA_DEFAULT_SAMPLE_COUNT);
        if (surface->msaaSamples != desiredMSAASamples)
        {
            surface->msaaSamples = desiredMSAASamples;
            KamcordInitUnity();
            GLES_CHK( glBindRenderbufferOES(GL_RENDERBUFFER_OES, surface->renderbuffer) );
        }
        
        if (surface->msaaSamples > 1)
        {
			GLES_CHK( glBindFramebufferOES(GL_FRAMEBUFFER_OES, surface->msaaFramebuffer) );
			gDefaultFBO = surface->msaaFramebuffer;
		}
    }
#endif
#endif
    
    gDefaultFBO = [KamcordRecorder activeFramebuffer];
}

#endif