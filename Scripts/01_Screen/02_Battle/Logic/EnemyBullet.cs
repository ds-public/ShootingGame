using Godot ;
using System ;


namespace Sample_001
{
	/// <summary>
	/// エネミーの弾の制御クラス
	/// </summary>
	public partial class EnemyBullet : CombatUnit
	{
		// 方向
		private Vector2						m_Direction ;

		// 速度(１秒間あたり)
		private float						m_Speed ;

		//-----------------------------------
		// ホーミング(追尾)用情報

		// 角度調整間隔(０より大きい場合追尾有効)
		private float						m_RotationIntervalTime ;

		// 最大調整角度(０より大きい場合追尾有効)
		private float						m_MaxRotationAngle ;

		// 最大追尾時間(０で無限追尾)
		private float						m_MaxHomingTime ;

		//-----------------------------------

		/// <summary>
		/// 与えるダメージ
		/// </summary>
		public	int							  Damage => m_Damage ;
		private	int							m_Damage ;

		// 耐久値
		private int							m_Shield ;

		/// <summary>
		/// 壊せるかどうか
		/// </summary>
		public	bool						IsBreakable ;

		//-----

		private Action<EnemyBullet,Vector2,bool>	m_OnDestroyed ;

		//---------------
		// 固有タイマー

		private SimpleTimer					m_AngleCorrectionTimer ;
		private SimpleTimer					m_HomingLimitTimer ;

		//---------------

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
		/// <param name="posiition"></param>
		/// <param name="direction"></param>
		public void Start
		(
			Vector2 posiition,
			Vector2 direction,
			float speed,
			float rotationIntervalTime,	// 歩行補正間隔
			float maxRotationAngle,		// 最大補正角度
			float maxHomingTime,		// 最大追尾時間
			int damage,
			int shield,					// ０で破壊不能
			Action<EnemyBullet,Vector2,bool> onDestroyed,
			Battle owner,
			bool isFlip
		)
		{
			// 基底クラスを初期化する
			Initialize( owner, isFlip, true, true ) ;

			//----------------------------------

			Position		= posiition ;

			m_Direction		= direction ;

			// 方向に応じて角度をつける
			Rotation	= ExMath.GetRativeAngle( new Vector2(  0, +1 ), direction ) ;

			m_Speed			= speed ;

			// ホーミング(追尾)関係
			m_RotationIntervalTime		= rotationIntervalTime ;
			m_MaxRotationAngle			= maxRotationAngle ;
			m_MaxHomingTime				= maxHomingTime ;

			//----------------------------------

			// プレイヤーが接触した際に受けるダメージを保存する
			m_Damage		= damage ;

			// 耐久値を保存する
			m_Shield		= shield ;

			// 耐久値ありなしでコリジョンを設定

			if( m_Shield <= 0 )
			{
				// 耐久値無し＝破壊不可
				IsBreakable		= false ;
				CollisionMask	= 0x00000021 ;	// プレイヤーとプレイヤーのボムにのみヒットする
			}
			else
			{
				// 耐久値有り＝破壊可能
				IsBreakable		= true ;
				CollisionMask	= 0x00000023 ;	// プレイヤーとプレイヤーの弾とボムにヒットする
			}

			//----

			m_OnDestroyed	= onDestroyed ;

			//----------------------------------------------------------

			// 表示優先順位は高めに設定する(アイテムに隠れて見えなかったりするため)
			ZIndex = 10 ;

			//----------------------------------

			// コリジョンの接触コールバック設定
			this.AreaEntered += OnAreaEntered ;
			m_IsAreaEntered = false ;

			//----------------------------------

			// 方向補正タイマー
			m_AngleCorrectionTimer = new () ;

			// 追尾限界タイマー
			m_HomingLimitTimer = new () ;

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
			SetModulateColor() ;

			//----------------------------------

			if( m_IsAreaEntered == true )
			{
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------

			// ダメージエフェクト処理
			ProcessDamageEffectColor() ;

			//----------------------------------------------------------

			float x = Position.X ;
			float y = Position.Y ;

			float bx0 = x - 32 ;
			float by0 = y - 32 ;
			float bx1 = x + 32 ;
			float by1 = y + 32 ;

			float sx0 = 0 ;
			float sy0 = 0 ;
			float sx1 = ScreenSize.X ;
			float sy1 = ScreenSize.Y ;

			if( bx1 <  sx0 || by1 <  sy0 || bx0 >  sx1 || by0 >  sy1 )
			{
//				GD.Print( $"エネミーの弾削除: {x},{y}" ) ;

				this.AreaEntered -= OnAreaEntered ;		// 実際は意味が無い

				// 画面外で消失
				m_OnDestroyed?.Invoke( this, Position, false ) ;

				return ;
			}

			//------------------------------------------------------------------------------------------

			// 位置を更新する
			var position = Position ;

			if( m_RotationIntervalTime >  0 && m_MaxRotationAngle >  0 )
			{
				// 追尾あり

				if( m_Owner.IsPlayerDestroyed == false && m_AngleCorrectionTimer.IsFinished( m_RotationIntervalTime ) == true )
				{
					// 方向補正

					m_AngleCorrectionTimer.Reset() ;

					bool isHomingEnabled = true ;
					if( m_MaxHomingTime >  0 && m_HomingLimitTimer.IsFinished( m_MaxHomingTime ) == true )
					{
						// 追尾限界に到達

						isHomingEnabled = false ;
					}

					if( isHomingEnabled == true )
					{
						// 方向補正処理

						Vector2 targetDirection = ( m_Owner.Player.Position - position ).Normalized() ;

						float angle = ExMath.GetRativeAngle( m_Direction, targetDirection, true ) ;

						float sign = Mathf.Sign( angle ) ;
						angle = Mathf.Abs( angle ) ;

						if( angle >  m_MaxRotationAngle )
						{
							angle  = m_MaxRotationAngle ;
						}

						angle *= sign ;

						m_Direction = ExMath.GetRotatedVector( m_Direction, angle ) ;

						// 表示向き補正
						SetAngle( m_Direction ) ;
					}
				}
			}

			// 進行方向に進む
			Position = position + ( m_Direction * ( float )( m_Speed * delta ) ) ;
		}

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
			int damage = 0 ;

			if( area is Player )
			{
				damage = 10000 ;
			}
			else
			if( area is PlayerShot playerShot )
			{
				damage = playerShot.Damage ;
			}
			else
			if( area is PlayerBomb playerBomb )
			{
				damage = playerBomb.Damage ;
			}

			m_Shield -= damage ;

			if( m_Shield >  0 )
			{
				// 耐久値はまだ残っている

				m_Owner.CombatAudio.PlaySe( SE.Hit, RatioPosition.X ) ;

				SetDamageEffect() ;
			}
			else
			{
				// 消滅する
				m_OnDestroyed?.Invoke( this, Position, true ) ;
			}
		}

		/// <summary>
		/// 自爆
		/// </summary>
		public void SelfDestroy()
		{
			// 実際に使用する場合は自爆と画面外を区別する値が必要になる
			m_OnDestroyed?.Invoke( this, Position, false ) ;
		}

		/// <summary>
		/// 画面外扱いという事で消去する
		/// </summary>
		public void OutOfScreen()
		{
			// 画面外で消滅
			m_OnDestroyed?.Invoke( this, Position, false ) ;
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

			Rotation = ExMath.GetRativeAngle( new Vector2( 0, 1 ), direction ) ;
		}

	}
}
