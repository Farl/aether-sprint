Shader "UniStorm/Celestial/Stars" {
Properties {
	_Color ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_Starmap ("Star Map", 2D) = "white" {}
	_StarSpeed ("Rotation Speed", Float) = 2.0
	_LoY ("Opaque Y", Float) = 0
    _HiY ("Transparent Y", Float) = 10
}

Category {
	Tags{ "Queue" = "Transparent-400" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
	Blend SrcAlpha One
	Lighting Off 
	ZWrite Off

	SubShader 
	{
		Pass 
		{
            Stencil {
                Ref 1
                Comp NotEqual
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma multi_compile _STEREO_INSTANCING_ON

			#include "UnityCG.cginc"

			sampler2D _Starmap;
			fixed4 _Color;
			half _LoY;
      		half _HiY;
			uniform float3 _uWorldSpaceCameraPos;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
            	UNITY_VERTEX_INPUT_INSTANCE_ID 
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float2 rotateUV(float2 uv, float degrees)
            {
               const float Rads = (UNITY_PI * 2.0) / 360.0;
 
               float ConvertedRadians = degrees * Rads;
               float _sin = sin(ConvertedRadians);
               float _cos = cos(ConvertedRadians);
 
                float2x2 R_Matrix = float2x2( _cos, -_sin, _sin, _cos);
 
                uv -= 0.5;
                uv = mul(R_Matrix, uv);
                uv += 0.5;
 
                return uv;
            }
			
			float4 _Starmap_ST;
			float _StarSpeed;
			float _Rotation;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f,o);
    			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float s = _ProjectionParams.z;
				int eyeIndex = unity_StereoEyeIndex;

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

            	//o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _Starmap);
				o.color = v.color;

				_Rotation = _Time.x*_StarSpeed*10;

				o.texcoord1.xy = TRANSFORM_TEX(rotateUV(v.texcoord, _Rotation), _Starmap);

				float4 worldV = mul (unity_ObjectToWorld, v.vertex);
		        o.color.a = 1 - saturate(((_uWorldSpaceCameraPos.y - worldV.y) - _LoY) / (_HiY - _LoY));

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
				//fixed4 col = lerp(fixed4(1,0,0,1), fixed4(0,1,0,1), unity_StereoEyeIndex);
				fixed4 col = 1.0f * i.color * _Color * (tex2D(_Starmap, i.texcoord1.xy));
				return col;
			}
			ENDCG 
			}
		}	
	}
}
