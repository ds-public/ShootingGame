#if TOOLS
using Godot ;
using System ;


namespace uGUI.Compatible
{
	/// <summary>
	/// インスペクターの処理
	/// </summary>
	[Tool]
	public partial class RectTransformPanel : Panel
	{
		[Export]
		private PanelContainer			_BaseContainer ;

		[Export]
		private Label					_AnchorTypeX ;

		[Export]
		private Label					_AnchorTypeY ;

		[Export]
		private TextureButton			_AnchorPresetButton ;

		[Export]
		private AnchorPresetSelector	_AnchorPresetSelector ;

		[Export]
		private TextureButton			_AnchorToggleButton ;


		//----------------

		[Export]
		private SpinBox				_PositionX ;

		[Export]
		private SpinBox				_PositionY ;

		[Export]
		private SpinBox				_SizeX ;

		[Export]
		private SpinBox				_SizeY ;


		//----------------

		[Export]
		private HBoxContainer		_AnchorMin_Base ;

		[Export]
		private SpinBox				_AnchorMinX ;

		[Export]
		private SpinBox				_AnchorMinY ;


		[Export]
		private HBoxContainer		_AnchorMax_Base ;

		[Export]
		private SpinBox				_AnchorMaxX ;

		[Export]
		private SpinBox				_AnchorMaxY ;

		//----------------

		[Export]
		private SpinBox				_PivotX ;

		[Export]
		private SpinBox				_PivotY ;

		[Export]
		private HBoxContainer		_RotationZ_Base ;

		[Export]
		private SpinBox				_RotationZ ;

		[Export]
		private SpinBox				_ScaleX ;

		[Export]
		private SpinBox				_ScaleY ;
	
		//----------------

		[Export]
		private Label				_Label_L ;

		[Export]
		private Label				_Label_R ;

		[Export]
		private Label				_Label_T ;

		[Export]
		private Label				_Label_B ;

		//----------------

		[Export]
		private Texture2D			_AnchorSwitch_O ;

		[Export]
		private Texture2D			_AnchorSwitch_C ;


		//----------------

		[Export]
		private Texture2D[]			_AnchorPresetImages = new Texture2D[ 25 ] ;


		[Export]
		private Panel				_InvalidMask ;

		//------------------------------------------------------------

		// RectTransform 化した状態の Control の Layout Transform
		private RectTransform		m_RectTransform ;

		// コールバックの登録状態
		private bool				m_IsCallbackEnabled ;

		// ローテーションのスライダータイプ
		private EditorSpinSlider	m_RotationZ_Slider ;

		// アンカーの詳細の表示状態
		private bool				m_IsAnchorOpened ;

		//------------------------------------------------------------

		/// <summary>
		/// ターゲットを設定する
		/// </summary>
		/// <param name="control"></param>
		public void SetControl( Control control )
		{
			m_RectTransform = new RectTransform( control ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ヒエラルキーに追加された際に呼び出される
		/// </summary>
		public override void _EnterTree()
		{
			_AnchorPresetSelector.Visible = false ;

			_RotationZ.Visible = false ;

			//-----

			// Rotation の SpinSlider 版を追加する
			m_RotationZ_Slider = new EditorSpinSlider() ;
			_RotationZ_Base.AddChild( m_RotationZ_Slider ) ;

			m_RotationZ_Slider.CustomMinimumSize = new Vector2( 208,  30 ) ;
			m_RotationZ_Slider.MinValue = -360 ;
			m_RotationZ_Slider.MaxValue =  360 ;
			m_RotationZ_Slider.Step = 0.001f ;
			m_RotationZ_Slider.Value = 0 ;
			m_RotationZ_Slider.Suffix = "゜" ;
			m_RotationZ_Slider.TooltipText = "回転角度(Degree)" ;
		}

		/// <summary>
		/// ヒエラルキーから削除された際に呼び出される
		/// </summary>
		public override void _ExitTree()
		{
			// Rotation の SpinSlider 版を削除する
			if( m_RotationZ_Slider != null )
			{
				_RotationZ_Base.RemoveChild( m_RotationZ_Slider ) ;
				m_RotationZ_Slider.Free() ;
				m_RotationZ_Slider = null ;
			}

			//-----

			_RotationZ.Visible = true ;

			_AnchorPresetSelector.Visible = true ;

			//---------------

	//		RemoveCallback() ;	// 実行すべきできない
		}

		/// <summary>
		/// 準備が整った際に呼び出される
		/// </summary>
		public override void _Ready()
		{
            if( IsParentSizeEnabled == true )
            {
				// 親のサイズは有効
				_AnchorToggleButton.Visible = true ;

				var anchorPresetType = GetAnchorPresetType() ;

				int i = ( int )anchorPresetType ;
				int xType = i % 5 ;
				int yType = i / 5 ;

				// アンカー詳細表示の初期状態を設定する
				m_IsAnchorOpened = ( xType == 0 || yType == 0 ) ;
			}
			else
			{
				// 親のサイズは無効
				_AnchorToggleButton.Visible = false ;

				// アンカー詳細表示の初期状態を設定する
				m_IsAnchorOpened = false ;
			}

			//----------------------------------

			// アンカー設定部分の表示のオンオフを行う
			SetAnchorDisplay() ;

			// RectTransform のパラメータの表示更新を行う
			SetDisplay() ;

			//----------------------------------

			// コンテナである場合は編集不可
			_InvalidMask.Visible = IsContainer ;
		}

		/// <summary>
		/// 毎フレーム呼び出される
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( Engine.IsEditorHint() )
			{
				if( m_RectTransform != null )
				{
					// オリジナルの値の変化は Editor モードでは検知できない(コールバックが呼ばれない)
					// よって、毎フレームの値の変化の確認が必要
					if( m_RectTransform.Update() == true )
					{
						// RectTransform のパラメータの表示更新を行う
						SetDisplay() ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// コンテナかどうか
		private bool IsContainer
		{
			get
			{
				if( m_RectTransform == null )
				{
					return false ;
				}

				return m_RectTransform.IsContainer ;
			}
		}

		// 親のサイズが有効かどうか
		private bool IsParentSizeEnabled
		{
			get
			{
				if( m_RectTransform == null )
				{
					return false ;
				}

				return m_RectTransform.IsParentSizeEnabled ;
			}
		}

		// RectTransform のパラメータに変更があった際に呼び出される
		private void OnValueChanged( double value )
		{
			if( m_RectTransform != null )
			{
				float rotationZ = 0 ;

				if( _RotationZ.Visible == true )
				{
					rotationZ = Mathf.Pi * ( float )_RotationZ.Value / 180.0f ;
				}
				if( m_RotationZ_Slider != null )
				{
					rotationZ = Mathf.Pi * ( float )m_RotationZ_Slider.Value / 180.0f ;
				}

				// 入力値を RectTransform に反映させる
				m_RectTransform.Apply
				(
					new Vector2( ( float )_PositionX.Value,		( float )_PositionY.Value	),
					new Vector2( ( float )_SizeX.Value,			( float )_SizeY.Value		),
					new Vector2( ( float )_AnchorMinX.Value,	( float )_AnchorMinY.Value	),
					new Vector2( ( float )_AnchorMaxX.Value,	( float )_AnchorMaxY.Value	),
					new Vector2( ( float )_PivotX.Value,		( float )_PivotY.Value		),
					rotationZ,
					new Vector2( ( float )_ScaleX.Value,		( float )_ScaleY.Value		)
				) ;

				// RectTransform のパラメータの表示更新を行う
				SetDisplay() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// アンカープリセット選択ボタンが押された際に呼び出される
		private void OnAnchorPresetButton()
		{
			if( IsParentSizeEnabled == false )
			{
				// 親のサイズが無効であるためアンカープリセットは選択できない(中心固定)
				return ;
			}

			//----------------------------------

			// アンカープリセットセレクターの表示を行う
			_AnchorPresetSelector.Prepare( GetAnchorPresetType(), OnAnchorPresetSelected ) ;
			_AnchorPresetSelector.Open() ;
		}

		// アンカープリセット選択パネル(サブＵＩ)でアンカープリセットタイプが選択されると呼び出される
		private void OnAnchorPresetSelected( AnchorPresetTypes anchorPresetType )
		{
			var anchorMin = new Vector2() ;
			var anchorMax = new Vector2() ;

			switch( anchorPresetType )
			{
				case AnchorPresetTypes.LA :
					anchorMin.X = 0.0f ; anchorMax.X = 0.0f ;
					anchorMin.Y = m_RectTransform.AnchorMin.Y ;
					anchorMax.Y = m_RectTransform.AnchorMax.Y ;
				break ;

				case AnchorPresetTypes.CA :
					anchorMin.X = 0.5f ; anchorMax.X = 0.5f ;
					anchorMin.Y = m_RectTransform.AnchorMin.Y ;
					anchorMax.Y = m_RectTransform.AnchorMax.Y ;
				break ;

				case AnchorPresetTypes.RA :
					anchorMin.X = 1.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = m_RectTransform.AnchorMin.Y ;
					anchorMax.Y = m_RectTransform.AnchorMax.Y ;
				break ;

				//----

				case AnchorPresetTypes.SA :
					anchorMin.X = 0.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = m_RectTransform.AnchorMin.Y ;
					anchorMax.Y = m_RectTransform.AnchorMax.Y ;
				break ;

				//----------------------------------

				case AnchorPresetTypes.AT :
					anchorMin.X = m_RectTransform.AnchorMin.X ;
					anchorMax.X = m_RectTransform.AnchorMax.X ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 1.0f ;
				break ;

				//--------------

				case AnchorPresetTypes.LT :
					anchorMin.X = 0.0f ; anchorMax.X = 0.0f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 0.0f ;
				break ;

				case AnchorPresetTypes.CT :
					anchorMin.X = 0.5f ; anchorMax.X = 0.5f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 0.0f ;
				break ;

				case AnchorPresetTypes.RT :
					anchorMin.X = 1.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 0.0f ;
				break ;

				//----

				case AnchorPresetTypes.ST :
					anchorMin.X = 0.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 0.0f ;
				break ;

				//----------------------------------

				case AnchorPresetTypes.AM :
					anchorMin.X = m_RectTransform.AnchorMin.X ;
					anchorMax.X = m_RectTransform.AnchorMax.X ;
					anchorMin.Y = 0.5f ; anchorMax.Y = 0.5f ;
				break ;

				//--------------

				case AnchorPresetTypes.LM :
					anchorMin.X = 0.0f ; anchorMax.X = 0.0f ;
					anchorMin.Y = 0.5f ; anchorMax.Y = 0.5f ;
				break ;

				case AnchorPresetTypes.CM :
					anchorMin.X = 0.5f ; anchorMax.X = 0.5f ;
					anchorMin.Y = 0.5f ; anchorMax.Y = 0.5f ;
				break ;

				case AnchorPresetTypes.RM :
					anchorMin.X = 1.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 0.5f ; anchorMax.Y = 0.5f ;
				break ;

				//----

				case AnchorPresetTypes.SM :
					anchorMin.X = 0.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 0.5f ; anchorMax.Y = 0.5f ;
				break ;

				//----------------------------------

				case AnchorPresetTypes.AB :
					anchorMin.X = m_RectTransform.AnchorMin.X ;
					anchorMax.X = m_RectTransform.AnchorMax.X ;
					anchorMin.Y = 1.0f ; anchorMax.Y = 1.0f ;
				break ;

				//--------------

				case AnchorPresetTypes.LB :
					anchorMin.X = 0.0f ; anchorMax.X = 0.0f ;
					anchorMin.Y = 1.0f ; anchorMax.Y = 1.0f ;
				break ;

				case AnchorPresetTypes.CB :
					anchorMin.X = 0.5f ; anchorMax.X = 0.5f ;
					anchorMin.Y = 1.0f ; anchorMax.Y = 1.0f ;
				break ;

				case AnchorPresetTypes.RB :
					anchorMin.X = 1.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 1.0f ; anchorMax.Y = 1.0f ;
				break ;

				//----

				case AnchorPresetTypes.SB :
					anchorMin.X = 0.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 1.0f ; anchorMax.Y = 1.0f ;
				break ;

				//----------------------------------

				case AnchorPresetTypes.AS :
					anchorMin.X = m_RectTransform.AnchorMin.X ;
					anchorMax.X = m_RectTransform.AnchorMax.X ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 1.0f ;
				break ;

				//--------------

				case AnchorPresetTypes.LS :
					anchorMin.X = 0.0f ; anchorMax.X = 0.0f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 1.0f ;
				break ;

				case AnchorPresetTypes.CS :
					anchorMin.X = 0.5f ; anchorMax.X = 0.5f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 1.0f ;
				break ;

				case AnchorPresetTypes.RS :
					anchorMin.X = 1.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 1.0f ;
				break ;

				//----

				case AnchorPresetTypes.SS :
					anchorMin.X = 0.0f ; anchorMax.X = 1.0f ;
					anchorMin.Y = 0.0f ; anchorMax.Y = 1.0f ;
				break ;
			}

			// アンカーの変更を反映させる
			m_RectTransform.Apply
			(
				anchorMin,
				anchorMax
			) ;

			// RectTransform のパラメータの表示更新を行う
			SetDisplay() ;
		}

		// 現在のアンカーの状態から該当するアンカープリセットタイプを取得する
		private AnchorPresetTypes GetAnchorPresetType()
		{
			if( m_RectTransform == null )
			{
				return AnchorPresetTypes.AA ;
			}

			int xType = 0 ;
			if( m_RectTransform.AnchorMin.X == m_RectTransform.AnchorMax.X )
			{
				if( m_RectTransform.AnchorMin.X == 0.0f )
				{
					xType = 1 ;
				}
				else
				if( m_RectTransform.AnchorMin.X == 0.5f )
				{
					xType = 2 ;
				}
				else
				if( m_RectTransform.AnchorMin.X == 1.0f )
				{
					xType = 3 ;
				}
			}
			else
			if( m_RectTransform.AnchorMin.X == 0.0f && m_RectTransform.AnchorMax.X == 1.0f )
			{
				xType = 4 ;
			}

			int yType = 0 ;
			if( m_RectTransform.AnchorMin.Y == m_RectTransform.AnchorMax.Y )
			{
				if( m_RectTransform.AnchorMin.Y == 0.0f )
				{
					yType = 1 ;
				}
				else
				if( m_RectTransform.AnchorMin.Y == 0.5f )
				{
					yType = 2 ;
				}
				else
				if( m_RectTransform.AnchorMin.Y == 1.0f )
				{
					yType = 3 ;
				}
			}
			else
			if( m_RectTransform.AnchorMin.Y == 0.0f && m_RectTransform.AnchorMax.Y == 1.0f )
			{
				yType = 4 ;
			}

			return ( AnchorPresetTypes )( yType * 5 + xType ) ;
		}

		//-------------------------------------------------------------------------------------------

		private string[] m_AnchorTypeX_Names = { "custom", "left", "center", "right", "stretch" } ;
		private string[] m_AnchorTypeY_Names = { "custom", "top", "middle", "bottom", "stretch" } ;

		// RectTransform のパラメータの表示更新を行う
		private void SetDisplay()
		{
			if( m_RectTransform != null )
			{
				//----------------------------------------------------------
				// ＵＩのコールバック無効化

				RemoveCallback() ;

				//----------------------------------------------------------
				// アンカープリセット部分の表示

				var anchorPresetType = GetAnchorPresetType() ;

				int i = ( int )anchorPresetType ;

				SetTextureButtonTexrure( _AnchorPresetButton, _AnchorPresetImages[ i ] ) ;

				int xType = i % 5 ;
				int yType = i / 5 ;

				_AnchorTypeX.Text = m_AnchorTypeX_Names[ xType ] ;
				_AnchorTypeY.Text = m_AnchorTypeY_Names[ yType ] ;

				//----------------------------------------------------------
				// 各種パラメータ部分の表示

				_PositionX.Value			= m_RectTransform.Position.X ;
				_PositionY.Value			= m_RectTransform.Position.Y ;
				_SizeX.Value				= m_RectTransform.Size.X ;
				_SizeY.Value				= m_RectTransform.Size.Y ;
				_AnchorMinX.Value			= m_RectTransform.AnchorMin.X ;
				_AnchorMinY.Value			= m_RectTransform.AnchorMin.Y ;
				_AnchorMaxX.Value			= m_RectTransform.AnchorMax.X ;
				_AnchorMaxY.Value			= m_RectTransform.AnchorMax.Y ;
				_PivotX.Value				= m_RectTransform.Pivot.X ;
				_PivotY.Value				= m_RectTransform.Pivot.Y ;

				if( _RotationZ.Visible == true )
				{
					_RotationZ.Value	= 180.0f * m_RectTransform.Rotation / Mathf.Pi ;
				}
				if( m_RotationZ_Slider != null )
				{
					m_RotationZ_Slider.Value	= 180.0f * m_RectTransform.Rotation / Mathf.Pi ;
				}

				_ScaleX.Value				= m_RectTransform.Scale.X ;
				_ScaleY.Value				= m_RectTransform.Scale.Y ;

				if( m_RectTransform.AnchorMin.X == m_RectTransform.AnchorMax.X )
				{
					_Label_L.Text = "X" ;
					_Label_R.Text = "W" ;
				}
				else
				{
					_Label_L.Text = "L" ;
					_Label_R.Text = "R" ;
				}

				if( m_RectTransform.AnchorMin.Y == m_RectTransform.AnchorMax.Y )
				{
					_Label_T.Text = "Y" ;
					_Label_B.Text = "H" ;
				}
				else
				{
					_Label_T.Text = "T" ;
					_Label_B.Text = "B" ;
				}

				//----------------------------------------------------------
				// ＵＩのコールバック有効化

				AddCallback() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// アンカー表示オンオフのトグルボタンが押された際に呼び出される
		private void OnAnchorToggleButton()
		{
			m_IsAnchorOpened = !m_IsAnchorOpened ;

			// アンカー設定部分の表示のオンオフを行う
			SetAnchorDisplay() ;
		}

		// アンカー設定部分の表示のオンオフを行う
		private void SetAnchorDisplay()
		{
			Texture2D texture = ( m_IsAnchorOpened == true ) ? _AnchorSwitch_C : _AnchorSwitch_O ;

			SetTextureButtonTexrure( _AnchorToggleButton, texture ) ;

			//---------------

			_AnchorMin_Base.Visible = m_IsAnchorOpened ;
			_AnchorMax_Base.Visible = m_IsAnchorOpened ;

			// サイズを追従させる
//			GD.Print( "サイズ : " + _BaseContainer.Size + " / " + Size + " " + m_IsAnchorOpened ) ;

			SetSize( new Vector2( Size.X,  _BaseContainer.Size.Y ) ) ;
		}

		//-------------------------------------------------------------------------------------------

		// ボタンのテクスチャを設定する
		private static void SetTextureButtonTexrure( TextureButton button, Texture2D texture )
		{
			button.TextureNormal	= texture ;
			button.TexturePressed	= texture ;
			button.TextureHover		= texture ;
			button.TextureDisabled	= texture ;
			button.TextureFocused	= texture ;
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

			_AnchorPresetButton.ButtonDown		+= OnAnchorPresetButton ;

			_AnchorToggleButton.ButtonDown		+= OnAnchorToggleButton ;

			//-----------------------------------

			_PositionX.ValueChanged				+= OnValueChanged ;
			_PositionY.ValueChanged				+= OnValueChanged ;
			_SizeX.ValueChanged					+= OnValueChanged ;
			_SizeY.ValueChanged					+= OnValueChanged ;
			_AnchorMinX.ValueChanged			+= OnValueChanged ;
			_AnchorMinY.ValueChanged			+= OnValueChanged ;
			_AnchorMaxX.ValueChanged			+= OnValueChanged ;
			_AnchorMaxY.ValueChanged			+= OnValueChanged ;
			_PivotX.ValueChanged				+= OnValueChanged ;
			_PivotY.ValueChanged				+= OnValueChanged ;

			if( _RotationZ.Visible == true )
			{
				_RotationZ.ValueChanged			+= OnValueChanged ;
			}
			if( m_RotationZ_Slider != null )
			{
				m_RotationZ_Slider.ValueChanged	+= OnValueChanged ;
			}

			_ScaleX.ValueChanged				+= OnValueChanged ;
			_ScaleY.ValueChanged				+= OnValueChanged ;

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

			_AnchorPresetButton.ButtonDown		-= OnAnchorPresetButton ;

			_AnchorToggleButton.ButtonDown		-= OnAnchorToggleButton ;

			//-----------------------------------

			_PositionX.ValueChanged				-= OnValueChanged ;
			_PositionY.ValueChanged				-= OnValueChanged ;
			_SizeX.ValueChanged					-= OnValueChanged ;
			_SizeY.ValueChanged					-= OnValueChanged ;
			_AnchorMinX.ValueChanged			-= OnValueChanged ;
			_AnchorMinY.ValueChanged			-= OnValueChanged ;
			_AnchorMaxX.ValueChanged			-= OnValueChanged ;
			_AnchorMaxY.ValueChanged			-= OnValueChanged ;
			_PivotX.ValueChanged				-= OnValueChanged ;
			_PivotY.ValueChanged				-= OnValueChanged ;

			if( _RotationZ.Visible == true )
			{
				_RotationZ.ValueChanged			-= OnValueChanged ;
			}
			if( m_RotationZ_Slider != null )
			{
				m_RotationZ_Slider.ValueChanged	-= OnValueChanged ;
			}

			_ScaleX.ValueChanged				-= OnValueChanged ;
			_ScaleY.ValueChanged				-= OnValueChanged ;

			//-----------------------------------

			m_IsCallbackEnabled = false ;
		}
	}
}
#endif
