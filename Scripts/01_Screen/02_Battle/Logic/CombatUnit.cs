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
	public partial class CombatUnit : CombatEntity
	{
		//-----------------------------------
		// 単色表示関係

		protected	bool					m_MonochromaticMode ;
		protected	float					m_MonochromaticRatio ;
		protected	Color					m_MonochromaticColor ;

		//-----------------------------------
		// ダメージ中の色変化関係

		// ダメージエフェクト表示中
		protected	bool					m_IsDamageEffectEnabled ;
		protected	float					m_DamageEffectDuration ;

		protected	uint					m_DefaultDamageEffectColor = 0xFFBFBFBF ;

		protected	SimpleTimer				m_DamageEffectTimer ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 初期化する
		/// </summary>
		/// <param name="timerEnabled"></param>
		/// <param name="damageEffectEnabled"></param>
		protected void Initialize( Battle owner, bool isFlip, bool timerEnabled = true, bool damageEffectEnabled = false )
		{
			// 基底クラスの初期化を呼ぶ
			base.Initialize( owner, isFlip, timerEnabled ) ;

			if( damageEffectEnabled == true )
			{
				// ダメージエフェクト使用
				m_DamageEffectTimer = new() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 方向指定
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public void SetAngle( Vector2 direction, bool isEnemy )
		{
			direction = direction.Normalized() ;

			if( isEnemy == false )
			{
				// プレイヤー
				Rotation = ExMath.GetRativeAngle( new Vector2(  0, -1 ), direction ) ;
			}
			else
			{
				// エネミー
				Rotation = ExMath.GetRativeAngle( new Vector2(  0, +1 ), direction ) ;
			}
		}

		//---------------

		/// <summary>
		/// 単色モードの設定を行う
		/// </summary>
		/// <param name="state"></param>
		/// <param name="Color"></param>
		public void SetMonochromaticMode( bool state, float ratio = 1.0f, uint color = 0xFFFFFFFF )
		{
			if( state == true )
			{
				// 単色モード有効
				m_MonochromaticMode	= true ;

				m_MonochromaticRatio = ratio ;

				float r = ( ( color >> 16 ) & 0xFF ) / 255.0f ;
				float g = ( ( color >>  8 ) & 0xFF ) / 255.0f ;
				float b = ( ( color       ) & 0xFF ) / 255.0f ;
				float a = ( ( color >> 24 ) & 0xFF ) / 255.0f ;

				m_MonochromaticColor = new ( r, g, b, a ) ;

				if( m_IsDamageEffectEnabled == false )
				{
					SetInterpolation( m_MonochromaticRatio, m_MonochromaticColor ) ;
				}
			}
			else
			{
				// 単色モード無効
				m_MonochromaticMode = false ;

				if( m_IsDamageEffectEnabled == false )
				{
					ResetInterpolation() ;
				}
			}
		}

		/// <summary>
		/// デフォルトのダメージエフェクトカラーを設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetDefaultDamageEffectColor( uint color )
		{
			m_DefaultDamageEffectColor = color ;
		}

		/// <summary>
		/// ダメージエフェクトを表示する
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="value"></param>
		/// <param name="color"></param>
		public void SetDamageEffect( float duration = 0.05f, float value = 0.5f, uint color = 0 )
		{
			if( color == 0 )
			{
				color  = m_DefaultDamageEffectColor ; 
			}

			float r = ( ( color >> 16 ) & 0xFF ) / 255.0f ;
			float g = ( ( color >>  8 ) & 0xFF ) / 255.0f ;
			float b = ( ( color       ) & 0xFF ) / 255.0f ;
			float a = ( ( color >> 24 ) & 0xFF ) / 255.0f ;

			var interpolationColor = new Color( r, g, b, a ) ;

			//----------------------------------

			if( duration <  0.01f )
			{
				duration  = 0.01f ;
			}
			
			m_IsDamageEffectEnabled = true ;
			m_DamageEffectDuration = duration ;
			m_DamageEffectTimer.Reset() ;

			SetInterpolation( value, interpolationColor ) ;
		}

		//---------------

		/// <summary>
		/// ダメージエフェクトの色を処理する(毎フレーム呼び出す必要がある)
		/// </summary>
		protected void ProcessDamageEffectColor()
		{
			// ダメージエフェクト処理
			if( m_IsDamageEffectEnabled == true )
			{
				if( m_DamageEffectTimer.IsFinished( m_DamageEffectDuration ) == true )
				{
					if( m_MonochromaticMode == false )
					{
						ResetInterpolation() ;
					}
					else
					{
						SetInterpolation( m_MonochromaticRatio, m_MonochromaticColor ) ;
					}
				}
			}
		}
	}
}
