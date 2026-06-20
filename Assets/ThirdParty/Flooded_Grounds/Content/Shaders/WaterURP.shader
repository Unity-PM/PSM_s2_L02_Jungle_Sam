Shader "JungleSam/Water URP Simple"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _NormalA ("Normal Map A", 2D) = "bump" {}
        _NormalB ("Normal Map B", 2D) = "bump" {}
        _NoiseMap ("Noise / Height Map", 2D) = "gray" {}

        _BaseColor ("Water Color", Color) = (0.05, 0.32, 0.38, 0.65)
        _Alpha ("Alpha", Range(0, 1)) = 0.65

        _BaseTiling ("Base Tiling", Float) = 25
        _NormalATiling ("Normal A Tiling", Float) = 50
        _NormalBTiling ("Normal B Tiling", Float) = 10
        _NoiseTiling ("Noise Tiling", Float) = 10

        _NormalASpeed ("Normal A Speed", Vector) = (0.04, 0.02, 0, 0)
        _NormalBSpeed ("Normal B Speed", Vector) = (-0.02, 0.03, 0, 0)
        _NoiseSpeed ("Noise Speed", Vector) = (0.01, -0.015, 0, 0)

        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.45
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 4
        _FresnelStrength ("Fresnel Strength", Range(0, 2)) = 0.25
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.7
        _NoiseColorStrength ("Noise Color Strength", Range(0, 1)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardWater"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalA);
            SAMPLER(sampler_NormalA);

            TEXTURE2D(_NormalB);
            SAMPLER(sampler_NormalB);

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Alpha;

                float _BaseTiling;
                float _NormalATiling;
                float _NormalBTiling;
                float _NoiseTiling;

                float4 _NormalASpeed;
                float4 _NormalBSpeed;
                float4 _NoiseSpeed;

                float _NormalStrength;
                float _FresnelPower;
                float _FresnelStrength;
                float _Smoothness;
                float _SpecularStrength;
                float _NoiseColorStrength;
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

            float3 UnpackNormalSimple(float4 packedNormal)
            {
                float3 normal;
                normal.xy = packedNormal.xy * 2.0 - 1.0;
                normal.z = sqrt(saturate(1.0 - dot(normal.xy, normal.xy)));
                return normal;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.tangentWS = float4(normalize(normalInputs.tangentWS), input.tangentOS.w);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;

                float2 baseUV = input.uv * _BaseTiling;
                float2 normalAUV = input.uv * _NormalATiling + time * _NormalASpeed.xy;
                float2 normalBUV = input.uv * _NormalBTiling + time * _NormalBSpeed.xy;
                float2 noiseUV = input.uv * _NoiseTiling + time * _NoiseSpeed.xy;

                float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
                float4 normalASample = SAMPLE_TEXTURE2D(_NormalA, sampler_NormalA, normalAUV);
                float4 normalBSample = SAMPLE_TEXTURE2D(_NormalB, sampler_NormalB, normalBUV);
                float noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, noiseUV).r;

                float3 normalA = UnpackNormalSimple(normalASample);
                float3 normalB = UnpackNormalSimple(normalBSample);

                float3 normalTS = normalize(float3(
                    (normalA.xy + normalB.xy) * _NormalStrength,
                    normalA.z * normalB.z
                ));

                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS.xyz);
                float tangentSign = input.tangentWS.w * GetOddNegativeScale();
                float3 bitangentWS = normalize(cross(normalWS, tangentWS) * tangentSign);

                float3 finalNormalWS = normalize(
                    tangentWS * normalTS.x +
                    bitangentWS * normalTS.y +
                    normalWS * normalTS.z
                );

                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));

                Light mainLight = GetMainLight();

                float ndotl = saturate(dot(finalNormalWS, mainLight.direction));
                float3 halfDir = normalize(mainLight.direction + viewDirWS);
                float specular = pow(saturate(dot(finalNormalWS, halfDir)), 64.0) * _SpecularStrength * _Smoothness;

                float fresnel = pow(1.0 - saturate(dot(finalNormalWS, viewDirWS)), _FresnelPower) * _FresnelStrength;

                float3 waterBase = baseSample.rgb * _BaseColor.rgb;
                waterBase += noise * _NoiseColorStrength;

                float3 litColor = waterBase * (0.35 + ndotl * 0.65) * mainLight.color;
                litColor += fresnel.xxx;
                litColor += specular.xxx;

                litColor = MixFog(litColor, input.fogFactor);

                float alpha = saturate(_Alpha * _BaseColor.a);

                return half4(litColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}