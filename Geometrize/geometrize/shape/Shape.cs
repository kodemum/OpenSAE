// Generated by Haxe 4.3.1

#pragma warning disable 109, 114, 219, 429, 168, 162
namespace geometrize.shape {
	public interface Shape : global::haxe.lang.IHxObject {
		
		global::HaxeArray<object> rasterize();
		
		void mutate();
		
		global::geometrize.shape.Shape clone();
		
		int getType();
		
		global::HaxeArray<double> getRawShapeData();
		
		string getSvgShapeData();
		
	}
}

