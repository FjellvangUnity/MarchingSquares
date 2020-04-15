// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/My First Shader"
{

    Properties{
        //Proerties til editoren
        _Tint ("Tint", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    
    }
    SubShader{
        Pass{
            CGPROGRAM
                #pragma vertex MyVertexProgram
                #pragma fragment MyFragmentProgram

                #include "UnityCG.cginc"

                float4 _Tint; //prop mapping
                sampler2D _MainTex; //Texture.
                float4 _MainTex_ST; //Textures scale and translation.

                struct Interpolators{
                    float4 position : SV_POSITION;
                    //float3 localPosition : TEXCOORD0;
                    float2 uv : TEXCOORD0;
                };

                struct VertexData {
                    float4 position : POSITION;
                    float2 uv : TEXCOORD0;
                };

                Interpolators MyVertexProgram(VertexData v
                )  { //System value position
                    Interpolators i;
                    //i.localPosition = v.position.xyz;
                    i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    //i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                    i.position = UnityObjectToClipPos(v.position); //Samme som gamle mul(UNITY_MATRIX_MVP,position)
                    return i;
                    //return 0; // Samme som float4(0,0,0,0)
                }
                //output af vertex bruges som input til fragment
                float4 MyFragmentProgram(Interpolators i) : SV_TARGET { //Default frame buffer,
                    return tex2D(_MainTex, i.uv) * _Tint;
                    //return float4(i.uv, 1, 1);// * _Tint;
                    ///return _Tint;
                }
            ENDCG
        }
    }
}
