Shader "K1/RoleRim" {
	Properties {
		_Diffuse("DiffuseColor",Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
		_RimColor ("Rim Color", Color) = (0.12,0.31,0.47,1.0)
		_RimRange ("Rim Range", Range(0.5,20.0)) = 3.0
		_RimPower("RimPower",Range(0,20)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Lighting off
		
		CGPROGRAM
		#pragma surface surf CustomBlinnPhong exclude_path:prepass 
		
		fixed4 _Diffuse;
		sampler2D _MainTex;
		fixed4 _RimColor;
		half _RimRange;
		half _RimPower;
		
		inline fixed4 LightingCustomBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 viewDir) 		
		{
	   		fixed3 diffuse = s.Albedo * _Diffuse;  
			fixed4 c;
			c.rgb = diffuse;
			c.a = s.Alpha;
			return c;
		}
		
	
		struct Input {
			float2 uv_MainTex;
			half3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb*_Diffuse;
			o.Alpha = c.a;
			fixed rim = 1.0 - abs(dot (normalize(IN.viewDir), o.Normal));  
		    o.Emission = _RimColor.rgb * pow(rim, _RimRange)*_RimPower;  
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
