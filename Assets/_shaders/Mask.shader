Shader "Custom/Mask"
{
    SubShader
    {
        Tags{"Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Blend Zero One
        }
    }
} 