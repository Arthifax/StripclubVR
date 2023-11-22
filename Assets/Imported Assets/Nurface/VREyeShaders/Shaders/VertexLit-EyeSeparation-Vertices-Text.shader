Shader "EyeSeparation/VertexLit_EyeSeparation_Text" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white" {}
		_TargetEye("Target eye (left:-1,right:1,both:0)", Range(-1.0,1.0)) = 1.0
	}

		SubShader{
			Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 200

Pass {

		CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag 
			#pragma target 2.0
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _EyeFloatFlag;
			fixed _TargetEye;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				//Collapsing vertices if eyes mismatch
				o.vertex = lerp(UnityObjectToClipPos(v.vertex),float4(0,0,0,0), saturate(abs(_TargetEye - _EyeFloatFlag) - 1));

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.color = v.color * _Color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = i.color;
				col.a *= tex2D(_MainTex, i.texcoord).a;
				return col;
			}
		ENDCG
	}
		}

}
