using Godot ;
using System ;


namespace uGUI.Compatible
{
	/// <summary>
	/// RectTransform ヘルパー(struct ではダメ、絶対)
	/// </summary>
	public class RectTransform
	{
		/// <summary>
		/// 位置
		/// </summary>
		public Vector2 Position
		{
			get
			{
				return m_Position ;
			}
			set
			{
				m_Position = value ;
			}
		}

		private Vector2 m_Position = Vector2.Zero ;

		/// <summary>
		/// 大きさ
		/// </summary>
		public Vector2 Size
		{
			get
			{
				return m_Size ;
			}
			set
			{
				m_Size = value ;
			}
		}

		private Vector2 m_Size = Vector2.Zero ;

		/// <summary>
		/// Anchor 最小値
		/// </summary>
		public Vector2 AnchorMin
		{
			get
			{
				return m_AnchorMin ;
			}
			set
			{
				m_AnchorMin = value ;
			}
		}

		private Vector2 m_AnchorMin = Vector2.Zero ;

		/// <summary>
		/// Anchor 最大値
		/// </summary>
		public Vector2 AnchorMax
		{
			get
			{
				return m_AnchorMax ;
			}
			set
			{
				m_AnchorMax = value ;
			}
		}

		private Vector2 m_AnchorMax = Vector2.Zero ;

		/// <summary>
		/// ピボット(基準点)
		/// </summary>
		public Vector2 Pivot
		{
			get
			{
				return m_Pivot ;
			}
			set
			{
				m_Pivot = value ;
			}
		}

		private Vector2 m_Pivot = Vector2.Zero ;

		/// <summary>
		/// ピボットを固定するかどうか
		/// </summary>
		public bool IsPivotFixed
		{
			get
			{
				return m_IsPivotFixed ;
			}
			set
			{
				m_IsPivotFixed = value ;
			}
		}

		private bool	m_IsPivotFixed = true ;

		/// <summary>
		/// ローテーション
		/// </summary>
		public float Rotation
		{
			get
			{
				return m_Rotation ;
			}
			set
			{
				m_Rotation = value ;
			}
		}

		private float m_Rotation = 0 ;

		/// <summary>
		/// スケール
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return m_Scale ;
			}
			set
			{
				m_Scale = value ;
			}
		}

		private Vector2 m_Scale = Vector2.One ;


		//------------------------------------
		// オリジナル情報

		private readonly Control	m_Control ;

		private Vector2				m_Original_Position ;
		private Vector2				m_Original_Size ;

		private Vector2				m_Original_AnchorMin ;
		private Vector2				m_Original_AnchorMax ;

		private Vector2				m_Original_AnchorOffsetMin ;
		private Vector2				m_Original_AnchorOffsetMax ;	

		private Vector2				m_Original_PivotOffset ;

		private float				m_Original_Rotation ;

		private Vector2				m_Original_Scale ;

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="control"></param>
		public RectTransform( Control control )
		{
			m_Control = control ;

			m_Original_Position			= new Vector2( control.Position.X, control.Position.Y )  ;
			m_Original_Size				= new Vector2( control.Size.X, control.Size.Y ) ;

			m_Original_AnchorMin		= new Vector2( control.AnchorLeft,	control.AnchorTop		) ;
			m_Original_AnchorMax		= new Vector2( control.AnchorRight,	control.AnchorBottom	) ;

			m_Original_AnchorOffsetMin	= new Vector2( control.OffsetLeft,	control.OffsetTop		) ;
			m_Original_AnchorOffsetMax	= new Vector2( control.OffsetRight,	control.OffsetBottom	) ;

			m_Original_PivotOffset		= new Vector2( control.PivotOffset.X, control.PivotOffset.Y ) ;

			m_Original_Rotation			= control.Rotation ;

			m_Original_Scale			= new Vector2( control.Scale.X, control.Scale.Y ) ;

			//-----------------------------------

			Update( false, isPivotFixed:false ) ;
		}

		/// <summary>
		/// Inspector からの反映用
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <param name="anchorMin"></param>
		/// <param name="anchorMax"></param>
		/// <param name="pivot"></param>
		/// <param name="rotation"></param>
		/// <param name="scale"></param>
		public void Apply
		(
			Vector2	anchorMin,
			Vector2	anchorMax
		)
		{
			Apply
			(
				m_Position,
				m_Size,
				anchorMin,
				anchorMax,
				m_Pivot,
				m_Rotation,
				m_Scale
			) ;
		}

		/// <summary>
		/// Inspector からの反映用
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <param name="anchorMin"></param>
		/// <param name="anchorMax"></param>
		/// <param name="pivot"></param>
		/// <param name="rotation"></param>
		/// <param name="scale"></param>
		public void Apply
		(
			Vector2	position,
			Vector2	size,
			Vector2	anchorMin,
			Vector2	anchorMax,
			Vector2	pivot,
			float	rotation,
			Vector2	scale
		)
		{
			( float parentSizeX, float parentSizeY ) = GetParentSize() ;

			//----------------------------------------------------------
			// X

			if( parentSizeX >  0 )
			{
				// 親のサイズＸが有効

				//-----------------------------------
				// Anchor X

				if( anchorMin.X != m_AnchorMin.X || anchorMax.X != m_AnchorMax.X )
				{
					// アンカーＸに変化あり

					// Godot が Min.X > Max.X を許可していないので、この状態となっていたら補正する
					if( anchorMin.X != m_AnchorMin.X )
					{
						if( anchorMin.X >  anchorMax.X )
						{
							anchorMin.X  = anchorMax.X ;
						}
					}
					if( anchorMax.X != m_AnchorMax.X )
					{
						if( anchorMax.X <  anchorMin.X )
						{
							anchorMax.X  = anchorMin.X ;
						}
					}

					//----------------------------------

					if( anchorMin.X == anchorMax.X )
					{
						if( m_AnchorMin.X != m_AnchorMax.X )
						{
							// 比率から即値に変わる

							// 現在の表示位置(ピクセル単位)
							float x0 = parentSizeX * m_AnchorMin.X + m_Position.X ;
							float x1 = parentSizeX * m_AnchorMax.X - m_Size.X ;

							float sizeX = x1 - x0 ;

							// 変化前のピボット位置(ピクセル単位)
							float px = x0 + ( sizeX * m_Pivot.X ) ;

							// 変化後のアンカー位置(ピクセル単位)
							float ax = ( parentSizeX * anchorMin.X ) ;

							//---

							// 変更後の位置
							position.X	= px - ax ;

							// 変更後の大きさ
							size.X		= sizeX ;
						}
						else
						if( anchorMin.X != m_AnchorMin.X )
						{
							// 即値のまま別の位置に変わる

							float xo = parentSizeX * m_AnchorMin.X ;
							float xn = parentSizeX * anchorMin.X ;

							position.X = m_Position.X + xo - xn ;
						}
					}
					else
					{
						if( m_AnchorMin.X == m_AnchorMax.X )
						{
							// 即値から比率に変わる

							// 変更前のアンカー位置(ピクセル単位)
							float ax = ( parentSizeX * m_AnchorMin.X ) ;

							// 現在の表示範囲
							float x0 = ax + m_Position.X - ( m_Size.X * m_Pivot.X ) ;
							float x1 = x0 + m_Size.X ;

							// 変更後のアンカー範囲(ピクセル単位)
							float ax0 = parentSizeX * anchorMin.X ;
							float ax1 = parentSizeX * anchorMax.X ;

							//---

							// 変更後のマージン(Min)　※変更前の状態になるわうにマージンを設定する
							position.X	= x0 - ax0 ;

							// 変更後のマージン(Max)　※変更前の状態になるわうにマージンを設定する
							size.X		= ax1 - x1 ;
						}
						else
						{
							// 別の比率に変わる

							float amx0	= ( anchorMin.X - m_AnchorMin.X ) * parentSizeX ;
							float amx1	= ( anchorMax.X - m_AnchorMax.X ) * parentSizeX ;

							position.X	= m_Position.X	- amx0 ;
							size.X		= m_Size.X		+ amx1 ;
						}
					}
				}
			}
			else
			{
				// 親のサイズＸが無効

				// アンカーＸは親の中心固定
				anchorMin.X = 0.5f ;
				anchorMax.X = 0.5f ;
			}

			//---------------
			// Pivot X

			if( m_Pivot.X != pivot.X && m_Size.X == size.X )
			{
				// ピボットＸに変化あり

				if( anchorMin.X == anchorMax.X )
				{
					// アンカーは即値

					// ピボットを変えても表示位置を変えない措置
					position.X = m_Position.X + ( pivot.X - m_Pivot.X ) * size.X ;
				}
			}

			//----------------------------------------------------------
			// Y

			if( parentSizeY >  0 )
			{
				// 親のサイズＹが有効

				//-----------------------------------
				// Anchor Y

				if( anchorMin.Y != m_AnchorMin.Y || anchorMax.Y != m_AnchorMax.Y )
				{
					// アンカーＹに変化あり

					// Godot が Min.Y > Max.Y を許可していないので、この状態となっていたら補正する
					if( anchorMin.Y != m_AnchorMin.Y )
					{
						if( anchorMin.Y >  anchorMax.Y )
						{
							anchorMin.Y  = anchorMax.Y ;
						}
					}
					if( anchorMax.Y != m_AnchorMax.Y )
					{
						if( anchorMax.Y <  anchorMin.Y )
						{
							anchorMax.Y  = anchorMin.Y ;
						}
					}

					//----------------------------------

					if( anchorMin.Y == anchorMax.Y )
					{
						if( m_AnchorMin.Y != m_AnchorMax.Y )
						{
							// 比率から即値に変わる

							// 現在の表示位置(ピクセル単位)
							float y0 = parentSizeY * m_AnchorMin.Y + m_Position.Y ;
							float y1 = parentSizeY * m_AnchorMax.Y - m_Size.Y ;

							float sizeY = y1 - y0 ;

							// 変化前のピボット位置(ピクセル単位)
							float py = y0 + ( sizeY * m_Pivot.Y ) ;

							// 変化後のアンカー位置(ピクセル単位)
							float ay = ( parentSizeY * anchorMin.Y ) ;

							//---

							// 変更後の位置
							position.Y	= py - ay ;

							// 変更後の大きさ
							size.Y		= sizeY ;
						}
						else
						if( anchorMin.Y != m_AnchorMin.Y )
						{
							// 即値のまま別の位置に変わる

							float yo = parentSizeY * m_AnchorMin.Y ;
							float yn = parentSizeY * anchorMin.Y ;

							position.Y = m_Position.Y + yo - yn ;
						}
					}
					else
					{
						if( m_AnchorMin.Y == m_AnchorMax.Y )
						{
							// 即値から比率に変わる

							// 変更前のアンカー位置(ピクセル単位)
							float ay = ( parentSizeY * m_AnchorMin.Y ) ;

							// 現在の表示範囲
							float y0 = ay + m_Position.Y - ( m_Size.Y * m_Pivot.Y ) ;
							float y1 = y0 + m_Size.Y ;

							// 変更後のアンカー範囲(ピクセル単位)
							float ay0 = parentSizeY * anchorMin.Y ;
							float ay1 = parentSizeY * anchorMax.Y ;

							//---

							// 変更後のマージン(Min)　※変更前の状態になるわうにマージンを設定する
							position.Y	= y0 - ay0 ;

							// 変更後のマージン(Max)　※変更前の状態になるわうにマージンを設定する
							size.Y		= ay1 - y1 ;
						}
						else
						{
							// 別の比率に変わる

							float amy0	= ( anchorMin.Y - m_AnchorMin.Y ) * parentSizeY ;
							float amy1	= ( anchorMax.Y - m_AnchorMax.Y ) * parentSizeY ;

							position.Y	= m_Position.Y	- amy0 ;
							size.Y		= m_Size.Y		+ amy1 ;
						}
					}
				}
			}
			else
			{
				// 親のサイズＹが無効

				// アンカーＹは親の中心固定
				anchorMin.Y = 0.5f ;
				anchorMax.Y = 0.5f ;
			}

			//---------------
			// Pivot Y

			if( m_Pivot.Y != pivot.Y && m_Size.Y == size.Y )
			{
				// ピボットＹに変化あり

				if( anchorMin.Y == anchorMax.Y )
				{
					// アンカーは即値

					// ピボットを変えても表示位置を変えない措置
					position.Y = m_Position.Y + ( pivot.Y - m_Pivot.Y ) * size.Y ;
				}
			}

			//-----------------------------------

			m_Position.X	= position.X ;
			m_Position.Y	= position.Y ;
			m_Size.X		= size.X ;
			m_Size.Y		= size.Y ;
			m_AnchorMin.X	= anchorMin.X ;
			m_AnchorMin.Y	= anchorMin.Y ;
			m_AnchorMax.X	= anchorMax.X ;
			m_AnchorMax.Y	= anchorMax.Y ;
			m_Pivot.X		= pivot.X ;
			m_Pivot.Y		= pivot.Y ;
			m_Rotation		= rotation ;
			m_Scale.X		= scale.X ;
			m_Scale.Y		= scale.Y ;

			//-----------------------------------

			Apply() ;
		}


		/// <summary>
		/// RectTransform 値を Controll の Layout・Transform に反映させる
		/// </summary>
		public void Apply()
		{
			if( m_Control == null )
			{
				return ;
			}

			//-----------------------------------

			( float parentSizeX, float parentSizeY ) = GetParentSize() ;

			//-----------------------------------

			float anchorOffsetMinX ;
			float anchorOffsetMaxX ;

			float sizeX ;

			// X
			if( m_AnchorMin.X == m_AnchorMax.X )
			{
				// 即値幅

				anchorOffsetMinX = - ( m_Size.X *       m_Pivot.X   ) + m_Position.X ;
				anchorOffsetMaxX =     m_Size.X * ( 1 - m_Pivot.X )   + m_Position.X ;

				sizeX = m_Size.X ;
			}
			else
			{
				// 比率幅

				// 最終的な幅
				sizeX = parentSizeX * ( m_AnchorMax.X - m_AnchorMin.X ) - m_Position.X - m_Size.X ;

				anchorOffsetMinX =   m_Position.X ;
				anchorOffsetMaxX = - m_Size.X     ;
			}

			m_Control.AnchorLeft			= m_AnchorMin.X ;
			m_Control.AnchorRight			= m_AnchorMax.X ;

			m_Control.OffsetLeft			= anchorOffsetMinX ;
			m_Control.OffsetRight			= anchorOffsetMaxX ;

			//-----

			m_Original_AnchorMin.X			= m_Control.AnchorLeft ;
			m_Original_AnchorMax.X			= m_Control.AnchorRight ;

			m_Original_AnchorOffsetMin.X	= m_Control.OffsetLeft ;
			m_Original_AnchorOffsetMax.X	= m_Control.OffsetRight ;

			//-----
			// 重要：位置と大きさも同時に更新されているはずなので記録する
		
			m_Original_Position.X			= m_Control.Position.X ;
			m_Original_Size.X				= m_Control.Size.X ;

			//---------------

			float anchorOffsetMinY ;
			float anchorOffsetMaxY ;

			float sizeY ;

			// Y
			if( m_AnchorMin.Y == m_AnchorMax.Y )
			{
				// 即値幅

				anchorOffsetMinY = - ( m_Size.Y *       m_Pivot.Y   ) + m_Position.Y ;
				anchorOffsetMaxY =     m_Size.Y * ( 1 - m_Pivot.Y )   + m_Position.Y ;

				sizeY = m_Size.Y ;
			}
			else
			{
				// 比率幅

				// 最終的な幅
				sizeY = parentSizeY * ( m_AnchorMax.Y - m_AnchorMin.Y ) - m_Position.Y - m_Size.Y ;

				anchorOffsetMinY =   m_Position.Y ;
				anchorOffsetMaxY = - m_Size.Y     ;
			}

			m_Control.AnchorTop				= m_AnchorMin.Y ;
			m_Control.AnchorBottom			= m_AnchorMax.Y ;

			m_Control.OffsetTop				= anchorOffsetMinY ;
			m_Control.OffsetBottom			= anchorOffsetMaxY ;

			//-----

			m_Original_AnchorMin.Y			= m_Control.AnchorTop ;
			m_Original_AnchorMax.Y			= m_Control.AnchorBottom ;

			m_Original_AnchorOffsetMin.Y	= m_Control.OffsetTop ;
			m_Original_AnchorOffsetMax.Y	= m_Control.OffsetBottom ;

			//-----
			// 重要：位置と大きさも同時に更新されているはずなので記録する
		
			m_Original_Position.Y			= m_Control.Position.Y ;
			m_Original_Size.Y				= m_Control.Size.Y ;

			//-----------------------------------
			// ピボットオフセット

			m_Control.PivotOffset = new Vector2( m_Pivot.X * sizeX, m_Pivot.Y * sizeY ) ;
			m_Original_PivotOffset.X = m_Control.PivotOffset.X ;
			m_Original_PivotOffset.Y = m_Control.PivotOffset.Y ;

			//-----------------------------------
			// 以下はそのままの

			m_Control.Rotation	= m_Rotation ;
			m_Original_Rotation	= m_Control.Rotation ;

			m_Control.Scale		= new Vector2( m_Scale.X, m_Scale.Y ) ;
			m_Original_Scale.X	= m_Control.Scale.X ;
			m_Original_Scale.Y	= m_Control.Scale.Y ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンテナかどうか
		/// </summary>
		public bool IsContainer
		{
			get
			{
				if( m_Control == null )
				{
					return false ;
				}

				// 0 = Position
				// 1 = Anchors
				// 2 = Container
				// 3 = Uncontrolled

				return ( m_Control.LayoutMode == 2 ) ;
			}
		}

		/// <summary>
		/// 親はサイズが有効なノードかどうか
		/// </summary>
		public bool IsParentSizeEnabled
		{
			get
			{
				if( m_Control == null )
				{
					return false ;
				}

				var parentNode = m_Control.GetParent<Node>() ;

				if( parentNode != null )
				{
					if( parentNode is Control )
					{
						return true ;
					}
					else
					if( parentNode is CanvasLayer )
					{
						return true ;
					}
					else
					if( parentNode is SubViewport )
					{
						return true ;
					}
				}
				else
				{
					// そもそも親が存在しなければ画面を対象とする
					return true ;
				}
				
				return false ;
			}
		}

		// 親のサイズを取得する
		private ( float, float ) GetParentSize()
		{
			float parentSizeX = 0 ;
			float parentSizeY = 0 ;

			var parentNode = m_Control.GetParent<Node>() ;

			if( parentNode != null )
			{
				if( parentNode is Control parentControl )
				{
					parentSizeX = parentControl.Size.X ;
					parentSizeY = parentControl.Size.Y ;
				}
				else
				if( parentNode is CanvasLayer )
				{
					( parentSizeX, parentSizeY ) = GetViewportSize() ;
				}
				else
				if( parentNode is SubViewport subViewport )
				{
					( parentSizeX, parentSizeY ) = GetSubViewportSize( subViewport ) ;
				}
			}
			else
			{
				// そもそも親が存在しなければ画面を対象とする
				( parentSizeX, parentSizeY ) = GetViewportSize() ;
			}

			return ( parentSizeX, parentSizeY ) ;
		}

		// ビューポートのサイズを取得する
		private ( float, float ) GetViewportSize()
		{
			float viewportSizeX = 0 ;
			float viewportSizeY = 0 ;
#if TOOLS
			if( Engine.IsEditorHint() == true )
			{
				// Editor モード動作
				viewportSizeX = ( float )ProjectSettings.GetSetting( "display/window/size/viewport_width"  ) ;
				viewportSizeY = ( float )ProjectSettings.GetSetting( "display/window/size/viewport_height" ) ;
			}
			else
#endif
			{

				// Runtime モード動作
				var viewportSize = m_Control.GetViewportRect().Size ;
				viewportSizeX = viewportSize.X ;
				viewportSizeY = viewportSize.Y ;
			}

			return ( viewportSizeX, viewportSizeY ) ;
		}

		// サブビューポートのサイズを取得する
		private static ( float, float ) GetSubViewportSize( SubViewport subViewport )
		{
			float viewportSizeX = 0 ;
			float viewportSizeY = 0 ;
#if TOOLS
			if( Engine.IsEditorHint() == true )
			{
				// Editor モード動作
				viewportSizeX = ( float )ProjectSettings.GetSetting( "display/window/size/viewport_width"  ) ;
				viewportSizeY = ( float )ProjectSettings.GetSetting( "display/window/size/viewport_height" ) ;
			}
			else
#endif
			{

				// Runtime モード動作
				var viewportSize = subViewport.Size ;
				viewportSizeX = viewportSize.X ;
				viewportSizeY = viewportSize.Y ;
			}

			return ( viewportSizeX, viewportSizeY ) ;
		}


		//--------------------------------------------------------------------------------------------
		// Control to RectTransform

		/// <summary>
		/// Control の Layout Transform の値に変化があったら RectTransform の値に反映させる
		/// </summary>
		/// <returns></returns>
		public bool Update()
		{
			( var isModified, var isSizeChanged ) = IsModified ;

			if( isModified == true )
			{
				Update( isSizeChanged, isPivotFixed: m_IsPivotFixed ) ;
			}

			return isModified ;
		}

		/// <summary>
		/// オリジナルの値に変更があったか
		/// </summary>
		public ( bool, bool ) IsModified
		{
			get
			{
				if( m_Control == null )
				{
					return ( false, false ) ;
				}

				bool isModified = false ;

				//----------------------------------

//				int flag = 0 ;

				if( m_Original_Position.X != m_Control.Position.X || m_Original_Position.Y != m_Control.Position.Y )
				{
					isModified = true ;

					m_Original_Position.X  = m_Control.Position.X ;
					m_Original_Position.Y  = m_Control.Position.Y ;

//					flag |= 0x01 ;
				}

				bool isSizeChanged = false ;
				if( m_Original_Size.X != m_Control.Size.X || m_Original_Size.Y != m_Control.Size.Y )
				{
					isModified = true ;
					isSizeChanged = true ;

					m_Original_Size.X  = m_Control.Size.X ;
					m_Original_Size.Y  = m_Control.Size.Y ;

//					flag |= 0x02 ;
				}

				//----

				if( m_Original_AnchorMin.X != m_Control.AnchorLeft || m_Original_AnchorMin.Y != m_Control.AnchorTop )
				{
					isModified = true ;

					m_Original_AnchorMin.X  = m_Control.AnchorLeft ;
					m_Original_AnchorMin.Y  = m_Control.AnchorTop ;

//					flag |= 0x04 ;
				}

				if( m_Original_AnchorMax.X != m_Control.AnchorRight || m_Original_AnchorMax.Y != m_Control.AnchorBottom )
				{
					isModified = true ;

					m_Original_AnchorMax.X	= m_Control.AnchorRight ;
					m_Original_AnchorMax.Y  = m_Control.AnchorBottom ;

//					flag |= 0x08 ;
				}

				//----

				if( m_Original_AnchorOffsetMin.X != m_Control.OffsetLeft || m_Original_AnchorOffsetMin.Y != m_Control.OffsetTop )
				{
					isModified = true ;

					m_Original_AnchorOffsetMin.X  = m_Control.OffsetLeft ;
					m_Original_AnchorOffsetMin.Y  = m_Control.OffsetTop ;

//					flag |= 0x10 ;
				}

				if( m_Original_AnchorOffsetMax.X != m_Control.OffsetRight || m_Original_AnchorOffsetMax.Y != m_Control.OffsetBottom )
				{
					isModified = true ;

					m_Original_AnchorOffsetMax.X  = m_Control.OffsetRight ;
					m_Original_AnchorOffsetMax.Y  = m_Control.OffsetBottom ;

//					flag |= 0x20 ;
				}

				//----

				if( m_Original_PivotOffset.X != m_Control.PivotOffset.X || m_Original_PivotOffset.Y != m_Control.PivotOffset.Y )
				{
					isModified = true ;

					m_Original_PivotOffset.X  = m_Control.PivotOffset.X ;
					m_Original_PivotOffset.Y  = m_Control.PivotOffset.Y ;

//					flag |= 0x40 ;
				}

				//--------------

				if( m_Original_Rotation != m_Control.Rotation )
				{
					isModified = true ;

					m_Original_Rotation  = m_Control.Rotation ;

//					flag |= 0x80 ;
				}

				if( m_Original_Scale.X != m_Control.Scale.X || m_Original_Scale.Y != m_Control.Scale.Y )
				{
					isModified = true ;

					m_Original_Scale.X = m_Control.Scale.X ;
					m_Original_Scale.Y = m_Control.Scale.Y ;

//					flag |= 0x0100 ;
				}

				//----------------------------------
			
				return ( isModified, isSizeChanged ) ;
			}
		}


		// Control の Lyaout Transform の変化を RectTransform に反映させる
		private void Update( bool isSizeChanged, bool isPivotFixed )
		{
			// X
			if( m_Original_AnchorMin.X == m_Original_AnchorMax.X )
			{
				// サイズは数値指定が可能な状態
				m_Position.X	= m_Original_AnchorOffsetMin.X + m_Original_PivotOffset.X ;
				m_Size.X		= m_Original_AnchorOffsetMax.X - m_Original_AnchorOffsetMin.X ;
			}
			else
			{
				// サイズはマージン状態になっている
				m_Position.X	=   m_Original_AnchorOffsetMin.X ;
				m_Size.X		= - m_Original_AnchorOffsetMax.X ;	// 符号逆転
			}

			if( IsContainer == true )
			{
				m_Size.X = m_Original_Size.X ;
			}

			// アンカー値はそのまま対応される
			m_AnchorMin.X = m_Original_AnchorMin.X ;
			m_AnchorMax.X = m_Original_AnchorMax.X ;

			//---------------

			// Y
			if( m_Original_AnchorMin.Y == m_Original_AnchorMax.Y )
			{
				// サイズは数値指定が可能な状態
				m_Position.Y	= m_Original_AnchorOffsetMin.Y + m_Original_PivotOffset.Y ;
				m_Size.Y		= m_Original_AnchorOffsetMax.Y - m_Original_AnchorOffsetMin.Y ;
			}
			else
			{
				// サイズはマージン状態になっている
				m_Position.Y	=   m_Original_AnchorOffsetMin.Y ;
				m_Size.Y		= - m_Original_AnchorOffsetMax.Y ;	// 符号逆転
			}

			if( IsContainer == true )
			{
				m_Size.Y = m_Original_Size.Y ;
			}

			// アンカー値はそのまま対応される
			m_AnchorMin.Y = m_Original_AnchorMin.Y ;
			m_AnchorMax.Y = m_Original_AnchorMax.Y ;

			//---------------

			if( isPivotFixed == false )
			{
				// ピボットの比率は可変
				float pivotX = 0 ;
				if( m_Original_Size.X != 0 )
				{
					pivotX = m_Original_PivotOffset.X / m_Original_Size.X ;
				}

				float pivotY = 0 ;
				if( m_Original_Size.Y != 0 )
				{
					pivotY = m_Original_PivotOffset.Y / m_Original_Size.Y ;
				}

				m_Pivot.X = pivotX ;
				m_Pivot.Y = pivotY ;
			}
			else
			{
				// ピボットは比率は固定
				if( isSizeChanged == true )
				{
					// サイズの変化あり

					float pivotOffsetX = 0 ;
					if( m_Original_Size.X != 0 )
					{
						pivotOffsetX = m_Original_Size.X * m_Pivot.X ;
					}

					float pivotOffsetY = 0 ;
					if( m_Original_Size.Y != 0 )
					{
						pivotOffsetY = m_Original_Size.Y * m_Pivot.Y ;
					}

					if( m_Control != null )
					{
						 m_Control.PivotOffset = new Vector2( pivotOffsetX, pivotOffsetY ) ;
					}

					m_Original_PivotOffset.X = pivotOffsetX ;
					m_Original_PivotOffset.Y = pivotOffsetY ;
				}
				else
				{
					// サイズの変化なし

					float pivotX = 0 ;
					if( m_Original_Size.X != 0 )
					{
						pivotX = m_Original_PivotOffset.X / m_Original_Size.X ;
					}

					float pivotY = 0 ;
					if( m_Original_Size.Y != 0 )
					{
						pivotY = m_Original_PivotOffset.Y / m_Original_Size.Y ;
					}

					m_Pivot.X = pivotX ;
					m_Pivot.Y = pivotY ;
				}
			}

			m_Rotation	= m_Original_Rotation ;

			m_Scale.X	= m_Original_Scale.X ;
			m_Scale.Y	= m_Original_Scale.Y ;
		}
	}
}

