Shader "Custom/GoalPoint"{
  Properties{
    [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    [MainTexture] _BaseMap("Base Map", 2D) = "white"
  }

  SubShader{
    Tags { "RenderType"="Transparent" "Queue"="Transparent" }

    Pass{
      Blend SrcAlpha OneMinusSrcAlpha

      HLSLPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      struct Attributes
      {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct Varyings
      {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 position : TEXCOORD1;
      };

      TEXTURE2D(_BaseMap);
      SAMPLER(sampler_BaseMap);

      CBUFFER_START(UnityPerMaterial)
        half4 _BaseColor;
        float4 _BaseMap_ST;
      CBUFFER_END

      Varyings vert(Attributes IN){
        Varyings OUT;
        OUT.position = IN.positionOS.xyz / IN.positionOS.w;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
        return OUT;
      }

      // Code reused from my old code at:
      // https://www.shadertoy.com/view/wcScDw
      // ３つの関数から疑似乱数を生成
      float rand3d(float3 pos){
        float A = dot(pos, float3(32.4, 43.76, 87.54));
        return frac(A + 898.3213 * sin(84.43 * A + 9842.43242 * frac(dot(pos, float3(42.24, 85.42, 65.45)))));
      }

      // 3次元ノイズ
      float noise3d(float3 pos){
        float3 base = floor(pos);
        float3 dec  = frac(pos);
        float3 smo = float3(
          smoothstep(0.0, 1.0, dec.x),
          smoothstep(0.0, 1.0, dec.y),
          smoothstep(0.0, 1.0, dec.z)
        );

        // 左下前
        float ldf = rand3d(base);
        // 右下前
        float rdf = rand3d(base + float3(1.0, 0.0, 0.0));
        // 左上前
        float luf = rand3d(base + float3(0.0, 1.0, 0.0));
        // 右上前
        float ruf = rand3d(base + float3(1.0, 1.0, 0.0));
        // 左下奥
        float ldb = rand3d(base + float3(0.0, 0.0, 1.0));
        // 右下奥
        float rdb = rand3d(base + float3(1.0, 0.0, 1.0));
        // 左上奥
        float lub = rand3d(base + float3(0.0, 1.0, 1.0));
        // 右上奥
        float rub = rand3d(base + float3(1.0, 1.0, 1.0));

        float df = ldf + smo.x * (rdf - ldf);
        float uf = luf + smo.x * (ruf - luf);
        float db = ldb + smo.x * (rdb - ldb);
        float ub = lub + smo.x * (rub - lub);

        float f = df + smo.y * (uf - df);
        float b = db + smo.y * (ub - db);

        return f + smo.z * (b - f);
      }

      //Fractal Brownian Motion
      float FBM3d(float3 pos, int passes){
        float val = 0.0;
        float passesF = float(passes);
        for(int i = 0; i < passes; i++){
          val += noise3d(pos) / passesF;
          pos *= 1.8;
        }
        return val;
      }

      half4 frag(Varyings IN) : SV_Target{
        half3 color1 = half3(0, 0, 0);
        float strengthOfFirstColor = 0.3;
        float initialNoise = FBM3d(3 * IN.position + float3(0, -_Time.y * 0.2, 0), 5);
        float pulse = FBM3d(6 * initialNoise + 3 * IN.position + float3(0, -_Time.y * 0.5, 0), 3);
        return half4(color1, min(pulse * 1.6, 1));
      }
      ENDHLSL
    }
  }
}
