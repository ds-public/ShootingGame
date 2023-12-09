using Godot ;
using System ;

namespace Sample_001
{
	/// <summary>
	/// 星を多数表示して３重スクロールさせるクラス
	/// </summary>
	public partial class Background : Node2D
	{
		public Vector2 ScreenSize ; // Size of the game window.

		[Export]
		private Node2D[]			_Layers = new Node2D[ 3 ] ;

		[Export]
		private AnimatedSprite2D[]	_Stars	= new AnimatedSprite2D[ 3 ] ;

		//-----------------------------------------------------------

		private float				m_PositionX ;

		private bool				m_IsPausing ;

		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			ScreenSize = GetViewportRect().Size ;

			int i, l = _Stars.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				_Stars[ i ].Visible = false ;
			}

			var colors = new Color[ 3 ]
			{
				new ( 1.00f, 1.00f, 1.00f, 0.5f ),
				new ( 0.75f, 0.75f, 0.75f, 0.5f ),
				new ( 0.50f, 0.50f, 0.50f, 0.5f ),
			} ;

			float xMin =               -128.0f ;
			float xMax = ScreenSize.X + 128.0f ;

			int layerIndex ;
			for( layerIndex  = 0 ; layerIndex <  _Layers.Length ; layerIndex ++ )
			{
				var layer = _Layers[ layerIndex ] ;

				layer.ZIndex = - 10 - layerIndex ;

				for( i  = 0 ; i <  256 ; i ++ )
				{
					int starIndex = ( int )( GD.Randi() % _Stars.Length ) ;
					var star = _Stars[ starIndex ] ;

					float x = ExMath.GetRandomRange( xMin, xMax ) ;
					float y = ExMath.GetRandomRange( 0, ScreenSize.Y ) ;

					AnimatedSprite2D star_Clone ;

					// １つ目

					star_Clone = star.Duplicate() as AnimatedSprite2D ;

					layer.AddChild( star_Clone ) ;

					star_Clone.Position = new Vector2( x, y ) ;
					star_Clone.Modulate = colors[ layerIndex ] ;
					star_Clone.Visible = true ;
					star_Clone.Play( "default" ) ;

					// ２つ目

					star_Clone = star.Duplicate() as AnimatedSprite2D ;

					layer.AddChild( star_Clone ) ;

					star_Clone.Position = new Vector2( x, y - ScreenSize.Y ) ;
					star_Clone.Modulate = colors[ layerIndex ] ;
					star_Clone.Visible = true ;
					star_Clone.Play( "default" ) ;
				}
			}

			//----------------------------------

			m_PositionX = 0 ;

			m_IsPausing = false ;
		}

		// スクロールスピートは手前のレイヤーの方が早い
		private static readonly float[] m_ScrollSpeed = new float[ 3 ]{ 200.0f, 100.0f,  50.0f } ;

		// スクロールスピートは手前のレイヤーの方が早い
		private static readonly float[] m_ScrollRatio = new float[ 3 ]{ 1.00f, 0.50f, 0.25f } ;

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( m_IsPausing == true )
			{
				// ポーズ中は操作できない
				return ;
			}

			//----------------------------------------------------------

			float x = m_PositionX * -0.25f ;

			int layerIndex ;
			for( layerIndex  = 0 ; layerIndex <  _Layers.Length ; layerIndex ++ )
			{
				var layer = _Layers[ layerIndex ] ;

				float y = layer.Position.Y ;
				y = ( y + ( m_ScrollSpeed[ layerIndex ] * ( float )delta ) ) % ScreenSize.Y ;

				layer.Position = new Vector2( x * m_ScrollRatio[ layerIndex ], y ) ;
			}
		}

		/// <summary>
		/// バックグラウンドの横位置を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetPositionX( float x )
		{
			m_PositionX = x ;
		}

		/// <summary>
		/// ポーズ状態を設定する
		/// </summary>
		/// <param name="isPausing"></param>
		public void SetPause( bool isPausing )
		{
			m_IsPausing= isPausing ;
		}
	}
}
