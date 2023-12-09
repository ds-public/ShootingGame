using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;


namespace Sample_001
{
	/// <summary>
	/// 戦闘ユニットの基底クラス
	/// </summary>
	public partial class CombatEntity : ExArea2D
	{
		[Export]
		protected AnimatedSprite2D			_Sprite ;

		[Export]
		protected CollisionShape2D			_Collision ;

		//-----------------------------------------------------------

		// オーナー
		protected	Battle					m_Owner ;

		// 画面サイズのショートカット
		public		Vector2					ScreenSize ;

		//-----------------------------------
		// 左右反転関係

		protected	bool					m_MasterFlip ;
		protected	bool					m_ActiveFlip ;

		//-----------------------------------
		// 汎用タイマー関係

		protected	SimpleTimer				m_Timer ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 初期化する
		/// </summary>
		/// <param name="timerEnabled"></param>
		/// <param name="damageEffectEnabled"></param>
		protected void Initialize( Battle owner, bool isFlip, bool timerEnabled = true )
		{
			m_Owner = owner ;

			// 画面サイズを取得しておく
			ScreenSize = GetViewportRect().Size ;

			//----------------------------------
			// 左右反転初期化

			m_MasterFlip = isFlip ;
			m_ActiveFlip = false ;

			// 左右反転
			_Sprite.FlipH = m_MasterFlip ;

			//----------------------------------

			if( timerEnabled == true )
			{
				// 汎用タイマー使用
				m_Timer = new() ;
			}
		}


		//-----------------------------------------------------------
		// タイマー関係

		/// <summary>
		/// デルタタイムを取得する
		/// </summary>
		public float Delta
		{
			get
			{
				if( m_Timer != null )
				{
					float value = m_Timer.Value ;
					m_Timer.Reset() ;
					return value ;
				}
				else
				{
					return 0 ;
				}
			}
		}

		/// <summary>
		/// 汎用タイマーをリセットする
		/// </summary>
		public void ResetTimer()
		{
			if( m_Timer != null )
			{
				m_Timer .Reset() ;
			}
		}

		//-----------------------------------------------------------
		// 座標関係

		/// <summary>
		/// 実値で横位置を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetPositionX( float x )
		{
			Position = new Vector2( x, Position.Y ) ;
		}

		/// <summary>
		/// 実値で縦位置を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetPositionY( float y )
		{
			Position = new Vector2( Position.X, y ) ;
		}

		/// 画面比率で位置を取得する
		/// </summary>
		public Vector2 GetRatioPosition( Vector2 position )
		{
			// 画面サイズを取得しておく
			var viewportSize = GetViewportRect().Size ;

			float x = position.X ;
			x = ( x / viewportSize.X ) - 0.5f ;

			float y = position.Y ;
			y = ( y / viewportSize.Y ) - 0.5f ;

			// 画面中心が ( 0, 0 )
			return new Vector2( x, y ) ;
		}

		/// <summary>
		/// 画面比率で位置を取得する
		/// </summary>
		public Vector2 RatioPosition
		{
			get
			{
				// 画面サイズを取得しておく
				var viewportSize = GetViewportRect().Size ;

				float x = Position.X ;
				x = ( x / viewportSize.X ) - 0.5f ;

				float y = Position.Y ;
				y = ( y / viewportSize.Y ) - 0.5f ;

				// 画面中心が ( 0, 0 )
				return new Vector2( x, y ) ;
			}
			set
			{
				// 画面サイズを取得しておく
				var viewportSize = GetViewportRect().Size ;

				float x = value.X ;
				x = ( x + 0.5f ) * viewportSize.X ;

				float y = value.Y ;
				y = ( y + 0.5f ) * viewportSize.Y ;

				// 画面左上が ( 0, 0 )
				Position = new Vector2( x, y ) ;
			}
		}

		/// <summary>
		/// 比率で横位置を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetRatioPositionX( float x )
		{
			// 画面サイズを取得しておく
			var viewportSize = GetViewportRect().Size ;

			x = ( x + 0.5f ) * viewportSize.X ;

			Position = new Vector2( x, Position.Y ) ;
		}

		/// <summary>
		/// 比率で縦位置を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetRatioPositionY( float y )
		{
			// 画面サイズを取得しておく
			var viewportSize = GetViewportRect().Size ;

			y = ( y + 0.5f ) * viewportSize.Y ;

			Position = new Vector2( Position.X, y ) ;
		}

		/// <summary>
		/// スケールを設定する
		/// </summary>
		/// <param name="scale"></param>
		public void SetScale( float scale )
		{
			Scale = new Vector2( scale, scale ) ;
		}

		//-----------------------------------
		// アスペクト比率

		/// <summary>
		/// Ｘ／Ｙ
		/// </summary>
		public float AspectXY
		{
			get
			{
				var size = GetViewportRect().Size ;

				return size.X / size.Y ;
			}
		}

		/// <summary>
		/// Ｙ／Ｘ
		/// </summary>
		public float AspectYX
		{
			get
			{
				var size = GetViewportRect().Size ;

				return size.Y / size.X ;
			}
		}

		//-----------------------------------------------------------
		// コリジョン関係

		/// <summary>
		/// コリジョンの状態を設定する
		/// </summary>
		/// <param name="enabled"></param>
		public void SetCollisionEnabled( bool enabled )
		{
			if( enabled == true )
			{
				// コリジョン有効化
				_Collision.Disabled = false ;
				_Collision.SetDeferred( CollisionShape2D.PropertyName.Disabled, false ) ;
			}
			else
			{
				// コリジョン無効化
				_Collision.SetDeferred( CollisionShape2D.PropertyName.Disabled, true ) ;
				_Collision.Disabled = false ;
			}
		}


		//-----------------------------------------------------------
		// 表示関係

		/// <summary>
		/// 色設定
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			SelfModulate = color ;
		}

		/// <summary>
		/// アルファ値を設定する
		/// </summary>
		public float Alpha
		{
			get
			{
				return Modulate.A ;
			}
			set
			{
				Modulate = new Color( Modulate.R, Modulate.G, Modulate.B, value ) ;
			}
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		public void PlayAnimation( string animationName )
		{
			_Sprite.Play( animationName ) ;
		}


		//---------------

		/// <summary>
		/// 左右反転を設定する
		/// </summary>
		/// <param name="isFlip"></param>
		public void SetFlip( bool isFlip )
		{
			m_ActiveFlip = isFlip ;

			_Sprite.FlipH = m_MasterFlip ^ m_ActiveFlip ;
		}

		//-----------------------------------------------------------

		// カスタムシェーダーにモジュレートカラーを設定する
		protected void SetModulateColor()
		{
			if( _Sprite.Material == null )
			{
				return ;
			}

			if( _Sprite.Material is ShaderMaterial material )
			{
				material.SetShaderParameter( "modulate_color", SelfModulate * Modulate ) ;
			}
		}

		//---------------

		/// <summary>
		/// 補完色設定を行う
		/// </summary>
		/// <param name="value"></param>
		/// <param name="color"></param>
		public void SetInterpolation( float value, Color color )
		{
			if( _Sprite.Material == null )
			{
				return ;
			}

			if( _Sprite.Material is ShaderMaterial material )
			{
				material.SetShaderParameter( "interpolation_value", value ) ;
				material.SetShaderParameter( "interpolation_color", color ) ;
			}
		}

		/// <summary>
		/// 補完色設定を行う
		/// </summary>
		/// <param name="value"></param>
		/// <param name="color"></param>
		public void SetInterpolation( float value )
		{
			if( _Sprite.Material == null )
			{
				return ;
			}

			if( _Sprite.Material is ShaderMaterial material )
			{
				material.SetShaderParameter( "interpolation_value", value ) ;
			}
		}

		/// <summary>
		/// 補完色設定を行う
		/// </summary>
		/// <param name="value"></param>
		/// <param name="color"></param>
		public void ResetInterpolation()
		{
			if( _Sprite.Material == null )
			{
				return ;
			}

			if( _Sprite.Material is ShaderMaterial material )
			{
				material.SetShaderParameter( "interpolation_value", 0 ) ;
			}
		}
	}
}
