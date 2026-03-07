sampler uImage0 : register(s0); // The contents of the screen.
sampler uImage1 : register(s1); // Up to three extra textures you can use for various purposes (for instance as an overlay).
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition; // The position of the camera.
float2 uTargetPosition; // The "target" of the shader, what this actually means tends to vary per shader.
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect; // Doesn't seem to be used, but included for parity.
float2 uZoom;

float strength = 1.0f;

float4 Grayscale(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, uv);

    float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float3 grayscale = float3(gray, gray, gray);

    color.rgb = lerp(color.rgb, grayscale, strength);
    return color;
}

float4 FrostyScreen(float4 sampleColor: COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, uv);
    float luminosity = (color.r + color.g + color.b) / 3;
    return color * sampleColor;
}

technique Technique1
{
    pass Grayscale
    {
        PixelShader = compile ps_2_0 Grayscale();
    }
    pass Bluescale
    {
        PixelShader = compile ps_2_0 FrostyScreen();
    }
}