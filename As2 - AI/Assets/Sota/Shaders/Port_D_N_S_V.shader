Shader "Portalarium/Diffuse Bump Spec Vert" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}
		_SpecTex ("Spec (RGB) Shininess (A)", 2D) = "white" {}
		_CompositeNormal ("Normal(GA)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags { "RenderType" = "Opaque" }
		LOD 400
	
		CGPROGRAM
		#pragma surface surf ColoredSpecular

		sampler2D _MainTex;
		sampler2D _SpecTex;
		sampler2D _CompositeNormal;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_SpecTex;
			float2 uv_CompositeNormal;
			float4 color : COLOR;
		};

		struct MySurfaceOutput 
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Specular;
			half3 GlossColor;
			half Alpha;
		};

		inline half4 LightingColoredSpecular (MySurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
		  half3 h = normalize (lightDir + viewDir);
		  half diff = max (0, dot (s.Normal, lightDir));

		  float nh = max (0, dot (s.Normal, h));
		  float spec = pow (nh, 32.0 * s.Specular);
		  half3 specCol = spec * s.GlossColor;

		  half4 c;
		  c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * specCol) * (atten * 2);
		  c.a = s.Alpha;
		  return c;
		}

 

		inline half4 LightingColoredSpecular_PrePass (MySurfaceOutput s, half4 light)
		{
			half3 spec = light.a * s.GlossColor;

			half4 c;
			c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
			c.a = s.Alpha + spec * _SpecColor.a;
			return c;
		}


		void surf (Input IN, inout MySurfaceOutput o) 
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 spec = tex2D(_SpecTex, IN.uv_SpecTex);
			fixed4 norm = tex2D(_CompositeNormal, IN.uv_CompositeNormal);
	
			o.Albedo.rgb = tex.rgb * IN.color.rgb * _Color.rgb;	
			o.Alpha = tex.a;
			o.Specular = spec.a;
			o.GlossColor = spec.rgb;
			o.Normal = UnpackNormal(norm);
		}
		ENDCG
	}

	FallBack "Transparent/VertexLit"
}