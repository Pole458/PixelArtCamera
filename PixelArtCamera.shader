Shader "Pole/PixelArtCamera"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BackgroundColor("Background Color", Color) = (0,0,0,0)
        _Scale("Scale", float) = 1
        _GameRes("Game Res", vector) = (0,0,0,0)
        _Rect("Rect", vector) = (0,0,0,0)
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            sampler2D _MainTex;
            float _Scale;
            fixed4 _BackgroundColor;
            float4 _Rect;
            float4 _GameRes;

            float4 frag(v2f_img input) : COLOR
            {
                if(input.pos.x < _Rect.x || input.pos.y < _Rect.y) return _BackgroundColor;
                if(input.pos.x > _Rect.z || input.pos.y > _Rect.w) return _BackgroundColor;

                float x = (input.pos.x - _Rect.x) / _Scale / _GameRes.x;
                float y = (input.pos.y - _Rect.y) / _Scale / _GameRes.y;

                float2 coord = float2(x, _ProjectionParams.x > 0 ? 1-y : y);
                
                return tex2D(_MainTex, coord);
            }
            ENDCG
        }
    }
}