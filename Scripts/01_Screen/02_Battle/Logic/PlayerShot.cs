using Godot ;
using System ;

using EaseHelper ;

namespace Sample_001
{
	/// <summary>
	/// プレイヤーの弾の制御クラス
	/// </summary>
	public partial class PlayerShot : CombatUnit
	{
		// 方向
		private Vector2						m_Direction ;

		/// <summary>
		/// 速度
		/// </summary>
		public	float						  Speed => m_Speed ;
		
		// 速度
		private float						m_Speed ;


		// 生存期間
		private float						m_Duration ;

		// 生存期間が有効な場合の挙動
		private EaseTypes					m_EaseType ;

		// 生存期間が有効な場合の初期位置
		private Vector2						m_StartPosition ;

		// デフォルトの計測時間
		private double						m_DefaultTime ;


		//-----------------------------------

		/// <summary>
		/// 与えるダメージ
		/// </summary>
		public	int							  Damage => m_Damage ;

		// ダメージ
		private int							m_Damage ;


		private Action<PlayerShot,Vector2,PlayerShotDestroyedReasonTypes>	m_OnDestroy ;

		/// <summary>
		/// 貫通するかどうか(デフォルトでは貫通しない)
		/// </summary>
		public	bool						IsPenetrating = false ;

		/// <summary>
		/// 処理タイプ
		/// </summary>
		public	int							  ProcessingType => m_ProcessingType ;

		// 処理タイプ
		private int							m_ProcessingType ;


		//-----------------------------------

		/// <summary>
		/// 画面外とみなす画面外の幅
		/// </summary>
		public	float						ScreenMargin = 16.0f ;

		//-----------------------------------

		private bool						m_IsAreaEntered ;
		private Area2D						m_CollisionTargetArea ;


		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready(){}

		/// <summary>
		/// 動作を開始させる
		/// </summary>
		/// <param name="position"></param>
		public void Start
		(
			Vector2 position,
			Vector2 direction,
			float speed,		// 生存期間が有効な場合は距離
			float duration,		// 生存期間(０以下で無限で無効)
			EaseTypes easeType,	// 生存期間が有効な場合の動き
			int damage,
			PlayerShotCollisionTypes collisionType,
			bool isHitCheck,	// コリジョンのヒット対象をチェックするか
			Action<PlayerShot,Vector2,PlayerShotDestroyedReasonTypes> onDestroy,
			Battle owner,
			bool isFlip,		// デフォルト左右反転
			int processingType	// [予備]処理タイプの識別値
		)
		{
			// 基底クラスを初期化する
			Initialize( owner, isFlip, false, false ) ;

			//----------------------------------

			// 位置
			Position			= position ;

			// 初期方向
			m_Direction			= direction ;

			// 方向に応じて角度をつける
			Rotation			= ExMath.GetRativeAngle( new Vector2(  0, -1 ), direction ) ;

			// 速度
			m_Speed				= speed ;

			// 生存期間
			m_Duration			= duration ;
			
			// 生存期間が有効な場合の位置タイプ
			m_EaseType			= easeType ;

			// 生存期間が有効な場合の初期位置
			m_StartPosition		= position ;

			// デフォルトの計測時間を初期化
			m_DefaultTime		= 0 ;

			//--------------

			// エネミーが接触した際に与えるダメージを保存する
			m_Damage			= damage ;

			//----------------------------------
			// コリジョンタイプを設定する

			switch( collisionType )
			{
				// 無効
				case PlayerShotCollisionTypes.None :
					SetCollisionEnabled( false ) ;
				break ;

				// 通常弾
				case PlayerShotCollisionTypes.Shot :
					SetCollisionEnabled( true ) ;
					CollisionLayer	= 0x02 ;	// PlayerShot

					if( isHitCheck == true )
					{
						CollisionMask	= 0x04 ;	// Enemy
					}
					else
					{
						CollisionMask	= 0x00 ;	// チェックを無効化して高速化
					}
				break ;

				// 特殊弾(ボム)
				case PlayerShotCollisionTypes.Bomb :
					SetCollisionEnabled( true ) ;
					CollisionLayer	= 0x20 ;	// PlayerBomb
					
					if( isHitCheck == true )
					{
						CollisionMask	= 0x0C ;	// Enemy | EnemyBullet
					}
					else
					{
						CollisionMask	= 0x00 ;	// チェックを無効化して高速化
					}
				break ;

			}

			//----------------------------------

			m_OnDestroy			= onDestroy ;

			// 処理タイプ
			m_ProcessingType	= processingType ;

			// スクリーンマージンのデフォルト値
			ScreenMargin		= 16.0f ;

			//----------------------------------

			// コリジョンの接触コールバック設定
			this.AreaEntered += OnAreaEntered ;
			m_IsAreaEntered = false ;

			//----------------------------------

			Visible = true ;
			SetProcess( true ) ;
		}

		/// <summary>
		/// 終了
		/// </summary>
		public void End()
		{
			Visible = false ;
			SetProcess( false ) ;

			//----------------------------------

			// コリジョンヒットのコールバックを解除する
			this.AreaEntered -= OnAreaEntered ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( m_Owner != null && m_Owner.IsPausing == true )
			{
				// ポーズ中は操作できない
				return ;
			}

			//----------------------------------------------------------

			// カスタムシェーダーにモジュレートカラーを設定する
//			SetModulateColor() ;

			//----------------------------------

			if( m_IsAreaEntered == true )
			{
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------

			// ダメージエフェクト処理
//			ProcessDamageEffectColor() ;

			//----------------------------------------------------------

			float x = Position.X ;
			float y = Position.Y ;

			float bx0 = x - ScreenMargin ;
			float by0 = y - ScreenMargin ;
			float bx1 = x + ScreenMargin ;
			float by1 = y + ScreenMargin ;

			float sx0 = 0 ;
			float sy0 = 0 ;
			float sx1 = ScreenSize.X ;
			float sy1 = ScreenSize.Y ;

			if( bx1 <  sx0 || by1 <  sy0 || bx0 >  sx1 || by0 >  sy1 )
			{
				this.AreaEntered -= OnAreaEntered ;	// 実際は意味が無い

				m_OnDestroy?.Invoke( this, Position, PlayerShotDestroyedReasonTypes.OutOfScreen ) ;

				return ;
			}

			//----------------------------------

			if( m_Duration <= 0 )
			{
				// 位置を更新する
				Position += m_Direction * ( m_Speed * ( float )delta ) ;
			}
			else
			{
				m_DefaultTime += delta ;

				if( m_DefaultTime >  m_Duration )
				{
					m_DefaultTime  = m_Duration ;
				}

				float factor = ( float )( m_DefaultTime / m_Duration ) ;

				factor = Ease.GetValue( factor, m_EaseType ) ;

				Position = m_StartPosition + ( m_Direction * m_Speed * factor ) ;

				if( factor >= 1 )
				{
					// 到達
					m_OnDestroy?.Invoke( this, Position, PlayerShotDestroyedReasonTypes.Self ) ;
				}
			}
		}

		//-----------------------------------

		// コリジョンに接触した際に呼び出される
		// 注意：コリジョン接触中のコールバック内で新規の Area2D 追加等を行うとエラーとなるため
		// 　　　コールバック発生直後の _Process で処理を実行する
		private void OnAreaEntered( Area2D area )
		{
			m_IsAreaEntered = true ;
			m_CollisionTargetArea = area ;
		}

		// 実際のコリジョンヒット処理(_Processから実行する)
		private void OnAreaHit( Area2D area )
		{
			if( IsPenetrating == false )
			{
				// 何かに当たれば消える(可能性がある)
				m_OnDestroy?.Invoke( this, Position, PlayerShotDestroyedReasonTypes.Enemy ) ;
			}
		}

		/// <summary>
		/// 自爆
		/// </summary>
		public void SelfDestroy()
		{
			// 何かに当たれば消える(可能性がある)
			m_OnDestroy?.Invoke( this, Position, PlayerShotDestroyedReasonTypes.Self ) ;
		}

		/// <summary>
		/// 画面外扱いという事で消去する
		/// </summary>
		public void OutOfScreen()
		{
			// 画面外で消滅
			m_OnDestroy?.Invoke( this, Position, PlayerShotDestroyedReasonTypes.OutOfScreen ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 移動を行う(１秒あたりの移動量を設定する)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="autoRotation"></param>
		public void Move( Vector2 velocity, bool autoRotation = true )
		{
			if( m_Timer != null )
			{
				Position += velocity * m_Timer.Value ;
				m_Timer.Reset() ;
			}

			if( autoRotation == true && velocity.LengthSquared() >  0 )
			{
				Rotation = ExMath.GetRativeAngle( new Vector2(  0,  1 ), velocity.Normalized() ) ;
			}
		}

		/// <summary>
		/// 方向指定
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public void SetAngle( Vector2 direction )
		{
			direction = direction.Normalized() ;

			Rotation = ExMath.GetRativeAngle( new Vector2(  0, -1 ), direction ) ;
		}
	}
}
