Shader "UniStorm/Clouds/Cloud Computing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
        Tags{ "Queue" = "Transparent-400" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100
        Blend One OneMinusSrcAlpha
        ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID 
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float s = _ProjectionParams.z;
				// 获取 VR 兼容的视图和投影矩阵
				float4x4 viewMatrix = UNITY_MATRIX_V;	// or UNITY_MATRIX_I_V??
				float4x4 projMatrix = UNITY_MATRIX_P;

                float4x4 mvNoTranslation =
                    float4x4(
                        float4(viewMatrix[0].xyz, 0.0f),
                        float4(viewMatrix[1].xyz, 0.0f),
                        float4(viewMatrix[2].xyz, 0.0f),
                        float4(0, 0, 0, 1.1)
                    );
                    
                
                o.vertex = mul(mul(projMatrix, mvNoTranslation), v.vertex * float4(s, s, s, 1));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
            	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
