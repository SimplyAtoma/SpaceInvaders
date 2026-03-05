Shader "Custom/SpaceBackgroundURP"
{
    Properties
    {
        _ScrollSpeed1    ("Star Layer 1 Speed",    Float)       = 0.04
        _ScrollSpeed2    ("Star Layer 2 Speed",    Float)       = 0.09
        _ScrollSpeed3    ("Star Layer 3 Speed",    Float)       = 0.18
        _StarDensity1    ("Star Density 1 (far)",  Float)       = 80.0
        _StarDensity2    ("Star Density 2 (mid)",  Float)       = 40.0
        _StarDensity3    ("Star Density 3 (near)", Float)       = 18.0
        _StarThresh1     ("Star Threshold 1",      Range(0,1))  = 0.97
        _StarThresh2     ("Star Threshold 2",      Range(0,1))  = 0.95
        _StarThresh3     ("Star Threshold 3",      Range(0,1))  = 0.92
        _NebulaScale     ("Nebula Scale",          Float)       = 3.5
        _NebulaSpeed     ("Nebula Scroll Speed",   Float)       = 0.015
        _NebulaColor1    ("Nebula Color A",        Color)       = (0.08, 0.0, 0.18, 1)
        _NebulaColor2    ("Nebula Color B",        Color)       = (0.0, 0.05, 0.2, 1)
        _NebulaStrength  ("Nebula Strength",       Range(0,1))  = 0.55
        _TwinkleSpeed    ("Twinkle Speed",         Float)       = 2.0
        _TwinkleAmount   ("Twinkle Amount",        Range(0,1))  = 0.35
        _ScanlineStr     ("Scanline Strength",     Range(0,1))  = 0.07
        _ScanlineFreq    ("Scanline Frequency",    Float)       = 400.0
    }

    SubShader
    {
        // URP tags
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Opaque"
            "Queue"          = "Background"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            // URP core includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Properties (CBUFFER required for URP SRP Batcher) ─────────
            CBUFFER_START(UnityPerMaterial)
                float  _ScrollSpeed1, _ScrollSpeed2, _ScrollSpeed3;
                float  _StarDensity1, _StarDensity2, _StarDensity3;
                float  _StarThresh1,  _StarThresh2,  _StarThresh3;
                float  _NebulaScale,  _NebulaSpeed,  _NebulaStrength;
                float4 _NebulaColor1, _NebulaColor2;
                float  _TwinkleSpeed, _TwinkleAmount;
                float  _ScanlineStr,  _ScanlineFreq;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                return OUT;
            }

            // ── Helpers ────────────────────────────────────────────────────

            float hash21(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash21(i),               hash21(i + float2(1,0)), u.x),
                    lerp(hash21(i + float2(0,1)), hash21(i + float2(1,1)), u.x),
                    u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0, a = 0.5, f = 1.0;
                for (int i = 0; i < 4; i++)
                {
                    v += a * vnoise(p * f);
                    a *= 0.5; f *= 2.1;
                }
                return v;
            }

            float starLayer(float2 uv, float density, float threshold, float timeOff)
            {
                float2 gid   = floor(uv * density);
                float2 local = frac(uv * density) - 0.5;

                float h  = hash21(gid);
                float h2 = hash21(gid + 7.3);
                float h3 = hash21(gid + 13.7);

                if (h < threshold) return 0.0;

                float2 offset = float2(h2 - 0.5, h3 - 0.5) * 0.6;
                float  dist   = length(local - offset);
                float  size   = lerp(0.015, 0.055, hash21(gid + 99.1));

                float phase   = h * 6.2831;
                float twinkle = 1.0 - _TwinkleAmount *
                                (0.5 + 0.5 * sin(_Time.y * _TwinkleSpeed + phase + timeOff));

                return smoothstep(size, 0.0, dist) * twinkle;
            }

            float3 starColor(float2 gid)
            {
                float  t    = hash21(gid + 55.5);
                float3 warm = float3(1.0,  0.92, 0.78);
                float3 cool = float3(0.78, 0.90, 1.0);
                float3 cyan = float3(0.6,  1.0,  1.0);
                if (t < 0.5) return lerp(warm, cool, t * 2.0);
                else         return lerp(cool, cyan, (t - 0.5) * 2.0);
            }

            // ── Fragment ───────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;

                // Nebula
                float2 nUV = uv * _NebulaScale + float2(0, t * _NebulaSpeed);
                float  n1  = fbm(nUV);
                float  n2  = fbm(nUV * 1.7 + float2(3.1, 1.7));
                float3 nebula = lerp(_NebulaColor1.rgb, _NebulaColor2.rgb, n1);
                nebula += _NebulaColor1.rgb * pow(n2, 2.5) * 0.4;
                nebula  = lerp(0, nebula, _NebulaStrength * smoothstep(0.3, 0.7, n1));

                float3 col = nebula;

                // Star layer 1 — far/slow
                float2 uv1 = uv + float2(0, t * _ScrollSpeed1);
                for (int a = 0; a < 4; a++)
                {
                    float2 off = float2(hash21(float2(a, 0.1)), hash21(float2(a, 0.9))) * 0.1;
                    float  b   = starLayer(uv1 + off, _StarDensity1, _StarThresh1, (float)a);
                    col += b * starColor(floor((uv1 + off) * _StarDensity1)) * 0.55;
                }

                // Star layer 2 — mid
                float2 uv2 = uv + float2(0, t * _ScrollSpeed2);
                for (int b2 = 0; b2 < 3; b2++)
                {
                    float2 off2 = float2(hash21(float2(b2+10, 0.3)), hash21(float2(b2+10, 0.7))) * 0.08;
                    float  br2  = starLayer(uv2 + off2, _StarDensity2, _StarThresh2, (float)b2 + 1.5);
                    col += br2 * starColor(floor((uv2 + off2) * _StarDensity2) + 200.0) * 0.8;
                }

                // Star layer 3 — near/bright/fast
                float2 uv3  = uv + float2(0, t * _ScrollSpeed3);
                float  br3  = starLayer(uv3, _StarDensity3, _StarThresh3, 3.0);
                float  gl3  = starLayer(uv3 * 0.995, _StarDensity3, _StarThresh3, 3.0);
                float3 sc3  = starColor(floor(uv3 * _StarDensity3) + 400.0);
                col += gl3 * sc3 * 0.25;
                col += br3 * sc3 * 1.0;

                // Vignette
                float2 vig = uv * 2.0 - 1.0;
                col *= 1.0 - dot(vig * float2(0.8, 1.0), vig * float2(0.8, 1.0)) * 0.3;

                // Scanlines
                col *= 1.0 - _ScanlineStr * (0.5 + 0.5 * sin(uv.y * _ScanlineFreq));

                return half4(max(col, 0.0), 1.0);
            }
            ENDHLSL
        }
    }
}
