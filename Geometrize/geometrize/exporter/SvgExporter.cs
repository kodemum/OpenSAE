// Generated by Haxe 4.3.1

#pragma warning disable 109, 114, 219, 429, 168, 162
namespace geometrize.exporter {
	public class SvgExporter : global::haxe.lang.HxObject {
		
		static SvgExporter() {
			global::geometrize.exporter.SvgExporter.SVG_STYLE_HOOK = "::svg_style_hook::";
		}
		
		
		public SvgExporter(global::haxe.lang.EmptyObject empty) {
		}
		
		
		public SvgExporter() {
			global::geometrize.exporter.SvgExporter.__hx_ctor_geometrize_exporter_SvgExporter(this);
		}
		
		
		protected static void __hx_ctor_geometrize_exporter_SvgExporter(global::geometrize.exporter.SvgExporter __hx_this) {
		}
		
		
		public static string SVG_STYLE_HOOK;
		
		public static string export(global::HaxeArray<object> shapes, int width, int height) {
			string results = global::geometrize.exporter.SvgExporter.getSvgPrelude();
			results = global::haxe.lang.Runtime.concat(results, global::geometrize.exporter.SvgExporter.getSvgNodeOpen(width, height));
			results = global::haxe.lang.Runtime.concat(results, global::geometrize.exporter.SvgExporter.exportShapes(shapes));
			results = global::haxe.lang.Runtime.concat(results, global::geometrize.exporter.SvgExporter.getSvgNodeClose());
			return results;
		}
		
		
		public static string exportShapes(global::HaxeArray<object> shapes) {
			unchecked {
				string results = "";
				{
					int _g = 0;
					int _g1 = shapes.length;
					while (( _g < _g1 )) {
						int i = _g++;
						results = global::haxe.lang.Runtime.concat(results, global::geometrize.exporter.SvgExporter.exportShape(shapes[i]));
						if (( i != ( shapes.length - 1 ) )) {
							results = global::haxe.lang.Runtime.concat(results, "\n");
						}
						
					}
					
				}
				
				return results;
			}
		}
		
		
		public static string exportShape(object shape) {
			return global::StringTools.replace(((global::geometrize.shape.Shape) (global::haxe.lang.Runtime.getField(shape, "shape", 2082267937, true)) ).getSvgShapeData(), global::geometrize.exporter.SvgExporter.SVG_STYLE_HOOK, global::geometrize.exporter.SvgExporter.stylesForShape(shape));
		}
		
		
		public static string getSvgPrelude() {
			return "<?xml version=\"1.0\" standalone=\"no\"?>\n";
		}
		
		
		public static string getSvgNodeOpen(int width, int height) {
			return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.2\" baseProfile=\"tiny\" width=\"", global::haxe.lang.Runtime.toString(width)), "\" height=\""), global::haxe.lang.Runtime.toString(height)), "\">\n");
		}
		
		
		public static string getSvgNodeClose() {
			return "</svg>";
		}
		
		
		public static string stylesForShape(object shape) {
			unchecked {
				if (( ((global::geometrize.shape.Shape) (global::haxe.lang.Runtime.getField(shape, "shape", 2082267937, true)) ).getType() == 6 )) {
					return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::geometrize.exporter.SvgExporter.strokeForColor(((int) (global::haxe.lang.Runtime.getField_f(shape, "color", 1247572323, true)) )), " stroke-width=\"1\" fill=\"none\" "), global::geometrize.exporter.SvgExporter.strokeOpacityForAlpha(((double) (( ((int) (global::haxe.lang.Runtime.getField_f(shape, "color", 1247572323, true)) ) & 255 )) )));
				}
				else {
					return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::geometrize.exporter.SvgExporter.fillForColor(((int) (global::haxe.lang.Runtime.getField_f(shape, "color", 1247572323, true)) )), " "), global::geometrize.exporter.SvgExporter.fillOpacityForAlpha(((double) (( ((int) (global::haxe.lang.Runtime.getField_f(shape, "color", 1247572323, true)) ) & 255 )) )));
				}
				
			}
		}
		
		
		public static string rgbForColor(int color) {
			unchecked {
				return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("rgb(", global::haxe.lang.Runtime.toString((( ( ((int) (color) ) >> 24 ) & 255 )))), ","), global::haxe.lang.Runtime.toString((( ( ((int) (color) ) >> 16 ) & 255 )))), ","), global::haxe.lang.Runtime.toString((( ( ((int) (color) ) >> 8 ) & 255 )))), ")");
			}
		}
		
		
		public static string strokeForColor(int color) {
			return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("stroke=\"", global::geometrize.exporter.SvgExporter.rgbForColor(color)), "\"");
		}
		
		
		public static string fillForColor(int color) {
			return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("fill=\"", global::geometrize.exporter.SvgExporter.rgbForColor(color)), "\"");
		}
		
		
		public static string fillOpacityForAlpha(double alpha) {
			return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("fill-opacity=\"", global::haxe.lang.Runtime.toString(( alpha / 255.0 ))), "\"");
		}
		
		
		public static string strokeOpacityForAlpha(double alpha) {
			return global::haxe.lang.Runtime.concat(global::haxe.lang.Runtime.concat("stroke-opacity=\"", global::haxe.lang.Runtime.toString(( alpha / 255.0 ))), "\"");
		}
		
		
	}
}

