#if TOOLS
using Godot ;
using System ;


namespace uGUI.Compatible
{
	/// <summary>
	/// RectTransform のプラグイン
	/// </summary>
	[Tool]
	public partial class RectTransformPlugin : EditorPlugin
	{
		private RectTransformInspector m_RectTransformInspector ;

		/// <summary>
		/// プラグインの処理の追加が必要な際に呼び出される
		/// </summary>
		public override void _EnterTree()
		{
			m_RectTransformInspector = new RectTransformInspector() ;
			AddInspectorPlugin( m_RectTransformInspector ) ;
		}

		/// <summary>
		/// プラグインの処理の削除が必要な際に呼び出される
		/// </summary>
		public override void _ExitTree()
		{
			if( m_RectTransformInspector != null )
			{
				RemoveInspectorPlugin( m_RectTransformInspector ) ;
			}
		}
	}
}
#endif
