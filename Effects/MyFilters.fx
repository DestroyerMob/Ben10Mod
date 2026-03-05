sampler uImage0 : register(s0);

float strength = 1.0f;

float4 MainPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, uv);

    float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float3 grayscale = float3(gray, gray, gray);

    color.rgb = lerp(color.rgb, grayscale, strength);
    return color;
}

technique Technique1
{
    pass Grayscale
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}