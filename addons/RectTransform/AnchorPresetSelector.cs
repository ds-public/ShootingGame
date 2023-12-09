#if TOOLS
using Godot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;


namespace uGUI.Compatible
{
	/// <summary>
	/// アンカーのプリセット選択
	/// </summary>
	[Tool]
	public partial class AnchorPresetSelector : Panel
	{
		// 選択中のアンカープリセットタイプのカーソル
		[Export]
		protected ReferenceRect			_Cursor ;
	
		// 選択中のアンカープリセットタイプのＸ方向の種別
		[Export]
		protected ReferenceRect			_CursorX ;

		// 選択中のアンカープリセットタイプのＹ方向の種別
		[Export]
		protected ReferenceRect			_CursorY ;

		// 選択中のアンカープリセットタイプ全種ボタン
		[Export]
		protected TextureButton[]		_AnchroPresetButtons = new TextureButton[ 25 ] ;

		// 閉じるボタン
		[Export]
		protected TextureButton			_CloseButton ;

		//------------------------------------------------------------

		private Action<AnchorPresetTypes>	m_OnAnchorPresetSelected ;

		private bool						m_IsCallbackEnabled ;

		//------------------------------------------------------------

		/// <summary>
		/// コールバック管理クラス
		/// </summary>
		public class AnchorPresetTypeCallback
		{
			public bool							Enabled ;
			public AnchorPresetTypes			AnchorPresetType ;

			public void Gateway()
			{
				Method?.Invoke( AnchorPresetType ) ;
			}

			public Action<AnchorPresetTypes>	Method ;
		}

		// コールバック情報を保持する
		private AnchorPresetTypeCallback[]		m_Callbacks ;

		// 選択中のアンカープリセットタイプ
		private	AnchorPresetTypes				m_ActiveAnchorPresetType ;

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// ヒエラルキーに追加された際に呼び出される
		/// </summary>
		public override void _EnterTree()
		{
			_Cursor.Visible = false ;
			_CursorX.Visible = false ;
			_CursorY.Visible = false ;

			//----------------------------------

			AddCallback() ;
		}

		/// <summary>
		/// 準備が整った際に呼び出される
		/// </summary>
		public override void _Ready()
		{
		}

		/// <summary>
		/// 毎フレーム呼び出される
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
		}

		/// <summary>
		/// ヒエラルキーから削除された際に呼び出される
		/// </summary>
		public override void _ExitTree()
		{
			RemoveCallback() ;
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// 表示前の準備を行う
		/// </summary>
		/// <param name="anchorPresetType"></param>
		/// <param name="onAnchorPresetSelected"></param>
		public void Prepare( AnchorPresetTypes anchorPresetType, Action<AnchorPresetTypes> onAnchorPresetSelected )
		{
			// アクティブなプリセットタイプを保存する
			m_ActiveAnchorPresetType = anchorPresetType ;

			// カーソルの表示を更新する
			SetCursorPosition( anchorPresetType ) ;

			// アンカープリセットタイプが選択された際に呼ぶコールバックを保存する
			m_OnAnchorPresetSelected = onAnchorPresetSelected ;
		}

		/// <summary>
		/// アンカープリセット選択パネルを開く
		/// </summary>
		public void Open()
		{
			Visible = true ;
			SetProcess( true ) ;
		}

		// アンカープリセットが選択された際に呼び出される
		private void OnAnchorPresetSelected( AnchorPresetTypes anchorPresetType )
		{
			int i_Old = ( int )m_ActiveAnchorPresetType ;

			int xType_Old = i_Old % 5 ;
			int yType_Old = i_Old / 5 ;

			int i_New = ( int )anchorPresetType ;

			int xType_New = i_New % 5 ;
			int yType_New = i_New / 5 ;

			if( yType_New == 0 && xType_New >= 1 )
			{
				// →カスタム選択
				if( yType_Old >= 1 )
				{
					anchorPresetType = ( AnchorPresetTypes )( yType_Old * 5 + xType_New ) ;
				}
			}
			else
			if( xType_New == 0 && yType_New >= 1 )
			{
				// ↓カスタム選択
				if( xType_Old >= 1 )
				{
					anchorPresetType = ( AnchorPresetTypes )( yType_New * 5 + xType_Old ) ;
				}
			}

			//-----------------------------------

			if( anchorPresetType == m_ActiveAnchorPresetType )
			{
				// 結果同じ場合は無視する
				return ;
			}

			//-----------------------------------

			// アクティブなプリセットタイプを更新する
			m_ActiveAnchorPresetType = anchorPresetType ;

			//-----------------------------------

			// カーソルの表示を更新する
			SetCursorPosition( anchorPresetType ) ;

			// アンカープリセットタイプが選択された際に呼ぶコールバックを実行する
			m_OnAnchorPresetSelected?.Invoke( anchorPresetType ) ;
		}

		// アンカープリセット選択パネルを閉じる
		private void Close()
		{
			SetProcess( false ) ;
			Visible = false ;
		}

		//------------------------------------------------------------

		// カーソルの表示を更新する
		private void SetCursorPosition( AnchorPresetTypes anchorPresetType )
		{
			int i = ( int )anchorPresetType ;

	//		GD.Print( "インデックス番号 : " + i ) ;

			int xType = i % 5 ;
			int yType = i / 5 ;

			if( xType == 0 && yType == 0 )
			{
				// 完全なカスタム
				_AnchroPresetButtons[ 0 ].Visible = true ;

				var button = _AnchroPresetButtons[ i ] ;

				_Cursor.SetPosition( button.Position + new Vector2( -2, -2 ) ) ;
				_Cursor.Visible = true ;

				_CursorX.Visible = false ;
				_CursorY.Visible = false ;
			}
			else
			if( yType == 0 && xType >= 1 )
			{
				// Ｙがカスタム

				_AnchroPresetButtons[ 0 ].Visible = true ;

				var button = _AnchroPresetButtons[ 0 ] ;

				_Cursor.SetPosition( button.Position + new Vector2( -2, -2 ) ) ;
				_Cursor.Visible = true ;

				var buttonX = _AnchroPresetButtons[ xType ] ;

				_CursorX.SetPosition( buttonX.Position + new Vector2( -2, -2 ) ) ;
				_CursorX.Visible = true ;

				_CursorY.Visible = false ;
			}
			else
			if( xType == 0 && yType >= 1 )
			{
				// Ｘがカスタム

				_AnchroPresetButtons[ 0 ].Visible = true ;

				var button = _AnchroPresetButtons[ 0 ] ;

				_Cursor.SetPosition( button.Position + new Vector2( -2, -2 ) ) ;
				_Cursor.Visible = true ;

				_CursorX.Visible = false ;

				var buttonY = _AnchroPresetButtons[ yType * 5 ] ;

				_CursorY.SetPosition( buttonY.Position + new Vector2( -2, -2 ) ) ;
				_CursorY.Visible = true ;
			}
			else
			if( xType >= 1 && yType >= 1 )
			{
				// 一般的なタイプ
				_AnchroPresetButtons[ 0 ].Visible = false ;

				var button = _AnchroPresetButtons[ i ] ;

				_Cursor.SetPosition( button.Position + new Vector2( -2, -2 ) ) ;
				_Cursor.Visible = true ;

				var buttonX = _AnchroPresetButtons[ xType ] ;

				_CursorX.SetPosition( buttonX.Position + new Vector2( -2, -2 ) ) ;
				_CursorX.Visible = true ;

				var buttonY = _AnchroPresetButtons[ yType * 5 ] ;

				_CursorY.SetPosition( buttonY.Position + new Vector2( -2, -2 ) ) ;
				_CursorY.Visible = true ;
			}
		}

		//------------------------------------------------------------

		/// <summary>
		/// パネル全体に対してインタラクションがあった際に呼び出される
		/// </summary>
		/// <param name="event"></param>
		public override void _GuiInput( InputEvent @event )
		{
			if( @event is InputEventMouseButton mbe && mbe.ButtonIndex == MouseButton.Left && mbe.Pressed )
			{
				Close() ;
			}
		}

		// 閉じるボタンが押された際に呼び出される
		private void OnCloseButton()
		{
			Close() ;
		}

		//--------------------------------------------------------------------------------------------

		// コールバックを追加する
		private void AddCallback()
		{
			if( m_IsCallbackEnabled == true )
			{
				// 既に追加済み
				return ;
			}

			//-----------------------------------

			int i, l = _AnchroPresetButtons.Length ;

			m_Callbacks = new AnchorPresetTypeCallback[ l ] ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				int xType = i % 5 ;
				int yType = i / 5 ;

				bool isEnabled = !( xType == 0 && yType == 0 ) ;

				m_Callbacks[ i ] = new AnchorPresetTypeCallback()
				{
					Enabled = isEnabled,
					AnchorPresetType = ( AnchorPresetTypes )i,
					Method	= OnAnchorPresetSelected
				} ;

				if( m_Callbacks[ i ].Enabled == true )
				{
					_AnchroPresetButtons[ i ].Pressed += m_Callbacks[ i ].Gateway ;
				}
			}

			_CloseButton.ButtonDown += OnCloseButton ;

			//-----------------------------------

			m_IsCallbackEnabled = true ;
		}

		// コールバックを削除する
		private void RemoveCallback()
		{
			if( m_IsCallbackEnabled == false )
			{
				// 既に削除済み
				return ;
			}

			//-----------------------------------

			_CloseButton.ButtonDown -= OnCloseButton ;

			if( m_Callbacks != null && m_Callbacks.Length >  0 )
			{
				int i, l = m_Callbacks.Length ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					if( m_Callbacks[ i ] != null )
					{
						if( m_Callbacks[ i ].Enabled == true )
						{
							_AnchroPresetButtons[ i ].Pressed -= m_Callbacks[ i ].Gateway ;
						}
						m_Callbacks[ i ]  = null ;
					}
				}
				m_Callbacks = null ;
			}

			//-----------------------------------

			m_IsCallbackEnabled = false ;
		}
	}
}
#endif
