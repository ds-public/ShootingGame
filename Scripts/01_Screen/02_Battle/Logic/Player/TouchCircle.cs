using Godot ;
using ExGodot ;
using System ;

namespace Sample_001
{
	public partial class TouchCircle : ExNode2D
	{
		/// <summary>
		/// タッチサークルの半径
		/// </summary>
		[Export]
		public float Radius = 48 ;

		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready(){}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{}

		/// <summary>
		/// カスタム描画(Unity の OnWillRenderObject 的な)
		/// </summary>
		public override void _Draw()
		{
			base._Draw() ;

//			DrawCircle( Position, Radius, new Color( 0xFF0000BF ) ) ;

			// 注意：円弧の場合は半径値は２倍の値で表示されるので半分の値を設定する事(32→16)
			DrawArc( Position, Radius * 0.5f,   0, 360, 36, new Color( 0xFF1F00BF ), 2 ) ;
		}
	}
}
