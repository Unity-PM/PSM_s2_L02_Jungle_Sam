Shader "JungleSam/Tree Creator Bark URP"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)

        _BumpMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0,2)) = 0.5

        _Smoothness ("Smoothness", Range(0,1)) = 0.1
        _Metallic ("Metallic", Range(0,1)) = 0

        // Legacy Tree Creator compatibility.
        _TranslucencyColor ("Translucency Color", Color) = (0,0,0,1)
        _TranslucencyViewDependency ("Trans. View Dependency", Range(0,1)) = 0
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _Color;
                float _NormalStrength;
                float _Smoothness;
                float _Metallic;
                float4 _TranslucencyColor;
                float _TranslucencyViewDependency;
                float _ShadowStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            float3 UnpackNormalMap(float4 packedNormal)
            {
                float3 n;
                n.xy = packedNormal.xy * 2.0 - 1.0;
                n.xy *= _NormalStrength;
                n.z = sqrt(saturate(1.0 - dot(n.xy, n.xy)));
                return normalize(n);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs norm = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = normalize(norm.normalWS);
                output.tangentWS = float4(normalize(norm.tangentWS), input.tangentOS.w);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 albedoSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;

                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS.xyz);
                float tangentSign = input.tangentWS.w * GetOddNegativeScale();
                float3 bitangentWS = normalize(cross(normalWS, tangentWS) * tangentSign);

                float2 normalUV = TRANSFORM_TEX(input.uv, _BumpMap);
                float3 normalTS = UnpackNormalMap(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, normalUV));

                float3 finalNormalWS = normalize(
                    tangentWS * normalTS.x +
                    bitangentWS * normalTS.y +
                    normalWS * normalTS.z
                );

                Light mainLight = GetMainLight();

                float ndotl = saturate(dot(finalNormalWS, mainLight.direction));
                float shadowFactor = lerp(1.0, ndotl, _ShadowStrength);

                float3 color = albedoSample.rgb;
                color *= (0.3 + ndotl * 0.7) * mainLight.color;
                color *= shadowFactor;

                color = MixFog(color, input.fogFactor);

                return half4(color, 1);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }

    FallBack Off
}