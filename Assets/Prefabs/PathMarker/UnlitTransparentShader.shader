// borrowed from https://www.benjaminoutram.com/blog/2019/8/20/simple-single-colour-shader-in-unity-with-transparency
Shader "Unlit/Transparent Colored" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        
        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
            Color [_Color]
        }
    }
}
