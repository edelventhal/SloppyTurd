// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sharpen" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SourceColor( "Source Color", Color ) = ( 0.0, 0.0, 0.0, 1.0 )
		_TargetColor( "Target Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
		_ExcludeAmount( "Exclude Amount", Float ) = 0.3
		_AlphaMultiple( "Alpha Multiple", Float ) = 0.25
		_AlphaThreshold( "Alpha Threshold", Float ) = 0.1
		_DarknessThreshold1( "Darkness Threshold 1", Float ) = 0.25
		_DarknessMultiple1( "Darkness Multiple 1", Float ) = 1.75
		_DarknessThreshold2( "Darkness Threshold 2", Float ) = 0.5
		_DarknessMultiple2( "Darkness Multiple 2", Float ) = 2.0
		_DarknessMinimum( "Darkness Minimum", Float ) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		//LOD 200
		
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc" 
			#pragma vertex vert
			#pragma fragment sharpen
			#pragma target 3.0
			#pragma glsl

			uniform sampler2D _MainTex;
			uniform float4 _SourceColor;
			uniform float4 _TargetColor;
			uniform float _ExcludeAmount;
			uniform float _AlphaMultiple;
			uniform float _AlphaThreshold;
			uniform float _DarknessThreshold1;
			uniform float _DarknessMultiple1;
			uniform float _DarknessThreshold2;
			uniform float _DarknessMultiple2;
			uniform float _DarknessMinimum;

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			half4 sharpen ( v2f i ) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv.xy);
				half3 colorrgb = c.rgb;
				
				if ( abs( colorrgb.r - _SourceColor.r ) + abs( colorrgb.g - _SourceColor.g ) + abs ( colorrgb.b - _SourceColor.b ) <= _ExcludeAmount )
				{
					half colorMult = 1.0;
					half alpha = c.a;
					
					if ( c.a > _AlphaThreshold )
					{
						alpha *= _AlphaMultiple; 
					}
					
					if ( c.a <= _DarknessThreshold1 )
					{
						colorMult = c.a * _DarknessMultiple1;
					}
					
					if ( c.a <= _DarknessThreshold2 && c.a > _DarknessThreshold1 )
					{
						colorMult = c.a * _DarknessMultiple2;
					}
					
					if ( colorMult < _DarknessMinimum )
					{
						colorMult = _DarknessMinimum;
					}
					
					return half4( _TargetColor.r * colorMult, _TargetColor.g * colorMult, _TargetColor.b * colorMult, alpha );
				}
				
				return half4( c.rgb, 0 );
			}
			
			v2f vert( appdata_img v ) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos( v.vertex );
				o.uv = v.texcoord.xy;
				return o;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
