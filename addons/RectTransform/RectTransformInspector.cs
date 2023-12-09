#if TOOLS
using Godot ;
using System ;


namespace uGUI.Compatible
{
	/// <summary>
	/// RectTransform の Inspector 拡張
	/// </summary>
	[Tool]
	public partial class RectTransformInspector : EditorInspectorPlugin
	{
		/// <summary>
		/// Inspector 対象クラスかどうか判定が必要な際に呼び出される
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public override bool _CanHandle( GodotObject @object )
		{
			if( @object is Control )
			{
				// 対象クラス
				return true ;
			}
			else
			{
				return false ;
			}
		}

		/// <summary>
		/// Inspector の最初にカスタムコントロールを追加可能なタイミングで呼び出される
		/// </summary>
		/// <param name="object"></param>
		public override void _ParseBegin( GodotObject @object )
		{
			// パネルのシーンをロードする
			var packedScene = ( PackedScene )GD.Load( "res://addons/RectTransform/RectTransformPanel.tscn" ) ;

			// パネルのインスタンスを生成する
			var control = packedScene.Instantiate<RectTransformPanel>() ;

			// 注意：RectTransformPanel のインスタンスはフィールドに保存してはいけない
			// 　　　RectTransformInspector のインスタンスは生きているが
			// 　　　RectTransformPanel のインスタンスは非表示時に破棄されてしまっているため
			// 　　　コードのリビルド時に
			// 　　　RectTransformInspector のインスタンスをシリアライズ化して保存を実行するが
			// 　　　RectTransformPanel のインスタンスが破棄済みであるためシリアライズ化出来ずエラーが発生する
			// 　　　原則
			// 　　　EditorInspectorPlugin 継承クラスではフィールドを定義してはいけない

			control.SetControl( @object as Control ) ;

			AddCustomControl( control ) ;
		}

		/// <summary>
		/// Inspector の最後にカスタムコントロールを追加可能なタイミングで呼び出される
		/// </summary>
		/// <param name="object"></param>
		public override void _ParseEnd( GodotObject @object )
		{
		}
	}
}
#endif
