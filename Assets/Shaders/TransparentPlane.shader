// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TransparentPlane"
{
    Properties{

    }
    SubShader{
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" }
        LOD 100

        Blend Zero One

        Pass {

        }
    }
}
