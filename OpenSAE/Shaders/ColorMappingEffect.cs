using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace OpenSAE.Shaders
{
	public class ColorMappingEffect : ShaderEffect
	{
		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ColorMappingEffect), 0);

		public ColorMappingEffect()
		{
            PixelShader pixelShader = new PixelShader
            {
                UriSource = new Uri("/OpenSAE;component/Shaders/ColorMapping.ps", UriKind.Relative)
            };
            
			PixelShader = pixelShader;

			UpdateShaderValue(InputProperty);
		}

		public Brush Input
        {
            get => ((Brush)(GetValue(InputProperty)));
            set => SetValue(InputProperty, value);
        }
    }
}
