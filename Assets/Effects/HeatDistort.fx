sampler2D ScreenTexture : register(s0);

float4 uColor;
float uOpacity;
float uTime;
float2 uTargetPosition; // screen-space-ish, provided by tML shader pipeline
float uIntensity;

float4 HeatDistortPass(float2 uv : TEXCOORD0) : COLOR0
{
    // Simple animated wave distortion
    float wave = sin((uv.y * 120.0) + (uTime * 3.2)) * 0.0018;
    float wave2 = cos((uv.x * 95.0) + (uTime * 2.6)) * 0.0012;

    float2 distortedUV = uv + float2(wave, wave2) * uIntensity;
    float4 col = tex2D(ScreenTexture, distortedUV);

    return col;
}

technique HeatDistortPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 HeatDistortPass();
    }
}
