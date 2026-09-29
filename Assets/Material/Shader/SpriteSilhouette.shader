// 스프라이트의 모양(알파)만 살리고 색은 SpriteRenderer.color 한 가지로 칠한다 — 보스 금빛 테두리(BossOutline)용.
//  기본 스프라이트 셰이더는 텍스처 색에 곱하므로 어두운 픽셀이 금색이 되지 않는다.
//  뼈대는 URP 의 Sprite-Unlit-Default 그대로다: Unity 6 은 flipX·렌더러 색을 버텍스 색이 아니라
//  unity_SpriteProps·unity_SpriteColor 로 넘기므로(처음 버전은 이걸 몰라 뒤집히지 않은 흰 실루엣이 삐져나왔다)
//  같은 함수로 받아야 본체와 겹친다. LightMode 태그가 없는 패스라 2D 렌더러가 조명 없이 그린다.
Shader "RE001/SpriteSilhouette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex SilhouetteVertex
            #pragma fragment SilhouetteFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            Varyings SilhouetteVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * unity_SpriteColor;
                return o;
            }

            half4 SilhouetteFragment(Varyings input) : SV_Target
            {
                half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                return half4(input.color.rgb, input.color.a * a);
            }
            ENDHLSL
        }
    }
}
