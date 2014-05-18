//Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc.
//All Rights Reserved.
//Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
Shader "Custom/Default" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}      
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface frag Lambert vertex:vert
        #pragma exclude_renderers flash

        sampler2D _MainTex;
        float _TouchX;
        float _TouchY;

        struct Input {
            float2 uv_MainTex;
        };
           
        void vert(inout appdata_full v) {
            float distance;
            float2 direction;
            float2 _Point;
            float sinDistance;
            
            _Point.x=0;
            _Point.y=0;
        
            direction =v.vertex.xz-_Point;
            distance=sqrt(direction.x*direction.x+direction.y*direction.y);
            sinDistance = (sin(distance));
            direction=direction/distance;
            v.vertex.xz+=2*(direction*(sinDistance*8));
            v.vertex.xz*=0.1;
            v.vertex.yz+=(direction*(sinDistance*8));
            v.vertex.yz*=2*0.1;
        }              

        void frag(Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	//FallBack "Diffuse"
}