Shader "XR/StereoEyeIndexColor"
{
   Properties
   {
       _Color1("Color1", COLOR) = (0,1,0,1)
       _Color2("Color2", COLOR) = (1,0,0,1)
       _Color3("Color3", COLOR) = (0,0,1,1)
   }

   SubShader
   {
      Tags { "RenderType" = "Opaque" }

      Pass
      {
         CGPROGRAM

         #pragma vertex vert
         #pragma fragment frag

			// 启用 Instancing 和 Stereo Multiview
			#pragma multi_compile_instancing
			#pragma multi_compile _ UNITY_STEREO_INSTANCING_ENABLED
			#pragma multi_compile _ UNITY_STEREO_MULTIVIEW_ENABLED

         float4 _Color1;
         float4 _Color2;
         float4 _Color3;

         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;

            UNITY_VERTEX_INPUT_INSTANCE_ID
         };

         struct v2f
         {
            float4 vertex : SV_POSITION;

            UNITY_VERTEX_INPUT_INSTANCE_ID 
            UNITY_VERTEX_OUTPUT_STEREO
         };

         v2f vert (appdata v)
         {
            v2f o;

            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.vertex = UnityObjectToClipPos(v.vertex);

            return o;
         }

         fixed4 frag (v2f i) : SV_Target
         {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                #ifdef UNITY_STEREO_MULTIVIEW_ENABLED
            return lerp(_Color1, _Color2, unity_StereoEyeIndex);
                #else
                  #ifdef UNITY_STEREO_INSTANCING_ENABLED
            return lerp(_Color2, _Color3, unity_StereoEyeIndex);
                  #else
            return lerp(_Color3, _Color1, unity_StereoEyeIndex);
                  #endif
                #endif
         }
         ENDCG
      }
   }
}
