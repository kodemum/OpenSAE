// Generated by Haxe 4.3.1

#pragma warning disable 109, 114, 219, 429, 168, 162
using System.Collections.Generic;
using System.Linq;

namespace geometrize.rasterizer {
	public class Scanline : global::haxe.lang.HxObject {
		
		public Scanline(global::haxe.lang.EmptyObject empty) {
		}
		
		
		public Scanline(int y, int x1, int x2) {
			global::geometrize.rasterizer.Scanline.__hx_ctor_geometrize_rasterizer_Scanline(this, y, x1, x2);
		}
		
		
		protected static void __hx_ctor_geometrize_rasterizer_Scanline(global::geometrize.rasterizer.Scanline __hx_this, int y, int x1, int x2) {
			__hx_this.y = y;
			__hx_this.x1 = x1;
			__hx_this.x2 = x2;
		}
		
		
		public static List<Scanline> trim(List<Scanline> scanlines, int w, int h) {
			return scanlines.Where(x => trimHelper(x, w, h)).ToList();
		}
		
		
		public static bool trimHelper(Scanline line, int w, int h) {
			unchecked {
				if (( ( ( ( line.y < 0 ) || ( line.y >= h ) ) || ( line.x1 >= w ) ) || ( line.x2 < 0 ) )) {
					return false;
				}
				
				int @value = line.x1;
				int max = ( w - 1 );
				if (( 0 > max )) {
					throw ((global::System.Exception) (global::haxe.Exception.thrown("FAIL: min <= max")) );
				}
				
				line.x1 = ( (( @value < 0 )) ? (0) : (( (( @value > max )) ? (max) : (@value) )) );
				int value1 = line.x2;
				int max1 = ( w - 1 );
				if (( 0 > max1 )) {
					throw ((global::System.Exception) (global::haxe.Exception.thrown("FAIL: min <= max")) );
				}
				
				line.x2 = ( (( value1 < 0 )) ? (0) : (( (( value1 > max1 )) ? (max1) : (value1) )) );
				return ( line.x1 <= line.x2 );
			}
		}
		
		
		public int y;
		
		public int x1;
		
		public int x2;
		
		public override double __hx_setField_f(string field, int hash, double @value, bool handleProperties) {
			unchecked {
				switch (hash) {
					case 26810:
					{
						this.x2 = ((int) (@value) );
						return @value;
					}
					
					
					case 26809:
					{
						this.x1 = ((int) (@value) );
						return @value;
					}
					
					
					case 121:
					{
						this.y = ((int) (@value) );
						return @value;
					}
					
					
					default:
					{
						return base.__hx_setField_f(field, hash, @value, handleProperties);
					}
					
				}
				
			}
		}
		
		
		public override object __hx_setField(string field, int hash, object @value, bool handleProperties) {
			unchecked {
				switch (hash) {
					case 26810:
					{
						this.x2 = ((int) (global::haxe.lang.Runtime.toInt(@value)) );
						return @value;
					}
					
					
					case 26809:
					{
						this.x1 = ((int) (global::haxe.lang.Runtime.toInt(@value)) );
						return @value;
					}
					
					
					case 121:
					{
						this.y = ((int) (global::haxe.lang.Runtime.toInt(@value)) );
						return @value;
					}
					
					
					default:
					{
						return base.__hx_setField(field, hash, @value, handleProperties);
					}
					
				}
				
			}
		}
		
		
		public override object __hx_getField(string field, int hash, bool throwErrors, bool isCheck, bool handleProperties) {
			unchecked {
				switch (hash) {
					case 26810:
					{
						return this.x2;
					}
					
					
					case 26809:
					{
						return this.x1;
					}
					
					
					case 121:
					{
						return this.y;
					}
					
					
					default:
					{
						return base.__hx_getField(field, hash, throwErrors, isCheck, handleProperties);
					}
					
				}
				
			}
		}
		
		
		public override double __hx_getField_f(string field, int hash, bool throwErrors, bool handleProperties) {
			unchecked {
				switch (hash) {
					case 26810:
					{
						return ((double) (this.x2) );
					}
					
					
					case 26809:
					{
						return ((double) (this.x1) );
					}
					
					
					case 121:
					{
						return ((double) (this.y) );
					}
					
					
					default:
					{
						return base.__hx_getField_f(field, hash, throwErrors, handleProperties);
					}
					
				}
				
			}
		}
		
		
		public override void __hx_getFields(global::HaxeArray<string> baseArr) {
			baseArr.push("x2");
			baseArr.push("x1");
			baseArr.push("y");
			base.__hx_getFields(baseArr);
		}
		
		
	}
}



#pragma warning disable 109, 114, 219, 429, 168, 162
namespace geometrize.rasterizer {
	public class Scanline_trim_44__Fun : global::haxe.lang.Function {
		
		public Scanline_trim_44__Fun(int w1, int h1) : base(1, 0) {
			this.w1 = w1;
			this.h1 = h1;
		}
		
		
		public override object __hx_invoke1_o(double __fn_float1, object __fn_dyn1) {
			unchecked {
				global::geometrize.rasterizer.Scanline line = ( (( __fn_dyn1 == global::haxe.lang.Runtime.undefined )) ? (((global::geometrize.rasterizer.Scanline) (((object) (__fn_float1) )) )) : (((global::geometrize.rasterizer.Scanline) (__fn_dyn1) )) );
				if (( ( ( ( line.y < 0 ) || ( line.y >= this.h1 ) ) || ( line.x1 >= this.w1 ) ) || ( line.x2 < 0 ) )) {
					return false;
				}
				else {
					int @value = line.x1;
					int max = ( this.w1 - 1 );
					if (( 0 > max )) {
						throw ((global::System.Exception) (global::haxe.Exception.thrown("FAIL: min <= max")) );
					}
					
					line.x1 = ( (( @value < 0 )) ? (0) : (( (( @value > max )) ? (max) : (@value) )) );
					int value1 = line.x2;
					int max1 = ( this.w1 - 1 );
					if (( 0 > max1 )) {
						throw ((global::System.Exception) (global::haxe.Exception.thrown("FAIL: min <= max")) );
					}
					
					line.x2 = ( (( value1 < 0 )) ? (0) : (( (( value1 > max1 )) ? (max1) : (value1) )) );
					return ( line.x1 <= line.x2 );
				}
				
			}
		}
		
		
		public int w1;
		
		public int h1;
		
	}
}


