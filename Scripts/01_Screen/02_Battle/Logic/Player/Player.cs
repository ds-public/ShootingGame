using Godot ;
using ExGodot ;
using System ;

using InputHelper ;

namespace Sample_001
{
	/// <summary>
	/// プレイヤーの制御クラス
	/// </summary>
	public partial class Player : CombatUnit
	{
		[Export]
		private AnimatedSprite2D		_AuraEffect ;

		[Export]
		private AnimationPlayer			_AuraEffectAnimation ;

		[Export]
		private ShieldEffect			_ShieldEffect ;

		[Export]
		private TouchCircle				_TouchCircle ;

		//-----------------------------------

		// 移動速度
		private float					m_Speed = 400.0f ;

		//-------------------------------------------------------------------------------------------

		private Action<int,Vector2>		m_OnAttack ;
		private Action<Vector2,int>		m_OnDamage ;
		private Action<float>			m_OnShieldActive ;
		private Action<float>			m_OnBombCooldown ;
		private Action<Vector2>			m_OnOption ;

		private bool					m_IsPressing ;
		private float					m_AttackInterval ;
		private SimpleTimer				m_ShotTimer ;


		private bool					m_IsPointerControllAvailable ;

		// マウス操作時の位置のオフセット
		private Vector2					m_Offset ;

		//-----------------------------------

		private bool					m_IsShieldActive ;

		/// <summary>
		/// 現在被ダメージ後の無敵状態中かどうか
		/// </summary>
		public	bool					IsShieldActive => m_IsShieldActive ;

		private float					m_ShieldActiveDuration ;
		private SimpleTimer				m_ShieldActiveTimer ;

		//-----

		/// <summary>
		/// 現在ボム使用後のクールダウン中かどうか
		/// </summary>
		public	bool					IsIsBombCooldown => m_IsBombCooldown  ;

		private bool					m_IsBombCooldown ;

		private float					m_BombCooldownDuration = 5.0f ;
		private SimpleTimer				m_BombCooldownTimer ;

		private int						m_BombActionState ;
		private float					m_BombTriggerDuration = 0.75f ;
		private float					m_BombReplaceDuration = 0.5f ;
		private SimpleTimer				m_BombActionStateTimer ;

		//---------------

		private bool					m_IsAreaEntered ;
		private Area2D					m_CollisionTargetArea ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready(){}

		/// <summary>
		/// 開始
		/// </summary>
		/// <param name="onAttack"></param>
		/// <param name="onDestroy"></param>
		public void Start
		(
			Action<int,Vector2> onAttack, 
			Action<Vector2,int> onDamage,
			Action<float> onShieldActive,
			Action<float> onBombCooldown,
			Action<Vector2> onOption,
			Battle owner
		)
		{
			// 基底クラスを初期化する
			Initialize( owner, false, false, false ) ;

			//----------------------------------

			m_OnAttack			= onAttack ;
			m_OnDamage			= onDamage ;
			m_OnShieldActive	= onShieldActive ;
			m_OnBombCooldown	= onBombCooldown ;
			m_OnOption			= onOption ;

			//----------------------------------

			// 初期位置設定
			Position = new Vector2( ScreenSize.X * 0.5f, ScreenSize.Y - 128 ) ;

			// 表示優先順位は高めに設定する
			ZIndex = 20 ;

			// 連射制限タイマー
			m_ShotTimer = new SimpleTimer() ;

			// オーラエフェクトは非表示にしておく
			_AuraEffect.Visible = false ;
			_AuraEffectAnimation.Stop() ;

			// シールドエフェクトは非表示にしておく
			_ShieldEffect.Visible = false ;

			// タッチサークルは非表示にしておく
			_TouchCircle.Visible = false ;

			// ポインターによる操作の有効化状態
			m_IsPointerControllAvailable = false ;

			// シールド
			m_IsShieldActive = false ;
			m_ShieldActiveTimer = new () ;

			// ボム
			m_IsBombCooldown = false ;
			m_BombCooldownTimer = new () ;

			// ボムの発射判定
			m_BombActionState = 0 ;
			m_BombActionStateTimer = new () ;

			//----------------------------------

			// コリジョン有効化
			SetCollisionEnabled( true ) ;

			// コリジョンヒットのコールバックを設定する
			this.AreaEntered += OnAreaEntered ;
			m_IsAreaEntered  = false ;

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

			// コリジョン無効化
			SetCollisionEnabled( false ) ;
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

			if( m_IsAreaEntered == true )
			{
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------------------------------

			var velocity = Vector2.Zero ; // The player's movement vector.

			// 方向ボタン
			var axis = GamePad.GetAxis( 0 ) ;
			if( axis.X >  0 )
			{
				velocity.X += 1 ;
			}
			if( axis.X <  0 )
			{
				velocity.X -= 1 ;
			}
			if( axis.Y >  0 )
			{
				velocity.Y += 1 ;
			}
			if( axis.Y <  0 )
			{
				velocity.Y -= 1 ;
			}

			if( velocity.X != 0 || velocity.Y != 0 )
			{
				// 確定
				velocity = velocity.Normalized() ;
			}
			else
			{
				// 左スティック
				axis = GamePad.GetAxis( 1 ) ;

				velocity.X = axis.X ;
				velocity.Y = axis.Y ;
			}

			if( velocity.X != 0 || velocity.Y != 0 )
			{
				Position += velocity * ( float )( m_Speed * delta ) ;

				Position = new Vector2
				(
					x: Mathf.Clamp( Position.X, 0, ScreenSize.X ),
					y: Mathf.Clamp( Position.Y, 0, ScreenSize.Y )
				) ;
			}
			else
			{
				// ポインターを見る
				var pointerPosition = Pointer.Position ;

				// タッチサークルの有効範囲(半径)
				float radius = _TouchCircle.Radius ;

				float radiusSquared = radius * radius ;

				if( Pointer.GetButtonDown( 0 ) == true )
				{
					// 画面が押された

					float distanceSquared = ( pointerPosition - Position ).LengthSquared() ;	// 距離の２乗値
					if( distanceSquared <  radiusSquared )
					{
						// 有効
						Pointer.Visible = false ;
						m_IsPointerControllAvailable = true ;
						m_Offset = Position - pointerPosition ;
					}
					else
					{
						// 無効
						_TouchCircle.Visible = true ;
					}
				}

				if( Pointer.GetButton( 0 ) == true )
				{
					// 画面が押されている
					if( m_IsPointerControllAvailable == true )
					{
						Position = pointerPosition + m_Offset ;

						Position = new Vector2
						(
							x: Mathf.Clamp( Position.X, 0, ScreenSize.X ),
							y: Mathf.Clamp( Position.Y, 0, ScreenSize.Y )
						) ;
					}
				}

				if( Pointer.GetButtonUp( 0 ) == true )
				{
					// 画面が離された
					m_IsPointerControllAvailable = false ;

					Pointer.Visible = true ;

					_TouchCircle.Visible = false ;
				}
			}

			//----------------------------------
			// アニメーション

			string asName = "default" ;

			if( velocity.X == 0 )
			{
				asName = "default" ;
			}
			else
			if( velocity.X >  0 )
			{
				asName = "move_r" ;
			}
			else
			if( velocity.X <  0 )
			{
				asName = "move_l" ;
			}

			_Sprite?.Play( asName ) ;

			if( _AuraEffect.Visible == true )
			{
				_AuraEffect?.Play( asName ) ;
			}

			//----------------------------------
			// 攻撃(弾の発射)

			bool isShot = false ;

			if
			(
				GamePad.GetButton( GamePad.B1 ) == true ||
				GamePad.GetButton( GamePad.B2 ) == true ||
				Pointer.GetButton( 0 ) == true
			)
			{
				// 自動の場合は秒間最大４発まで発射する
				if( m_ShotTimer.IsFinished( 0.25f ) == true )
				{
					isShot = true ;
					m_ShotTimer.Reset() ;
				}
			}

			//--------------

			if
			(
				GamePad.GetButtonDown( GamePad.B1 ) == true ||
				GamePad.GetButtonDown( GamePad.B2 ) == true ||
				Pointer.GetButtonDown( 0 ) == true
			)
			{
				// 手動の場合は秒間最大２０発まで発射する
				if( m_ShotTimer.IsFinished( 0.05f ) == true )
				{
					isShot = true ;
					m_ShotTimer.Reset() ;
				}
			}

			if( isShot == true )
			{
				// 弾を発射する
				m_OnAttack?.Invoke( 1, Position ) ;
			}

			//----------------------------------
			// 攻撃(ボムの発射)

			if
			(
				GamePad.GetButton( GamePad.B3 ) == true ||
				GamePad.GetButton( GamePad.B4 ) == true ||
				GamePad.GetButton( GamePad.R2 ) == true ||
				GamePad.GetButton( GamePad.L2 ) == true ||
				Pointer.GetButton( 1 ) == true
			)
			{
				// 押した

				if( m_Owner.PlayerBombStocks.Count >  0 && m_BombActionState == 0 )
				{
					// 判定開始
					m_BombActionState = 1 ;
					m_BombActionStateTimer.Reset() ;
				}
				else
				if( m_Owner.PlayerBombStocks.Count >  1 && m_BombActionState == 1 )
				{
					// 押しっぱなし
					if( m_BombActionStateTimer.IsFinished( m_BombTriggerDuration ) == true )
					{
						// ボムを発射する
						m_OnAttack?.Invoke( 3, Position ) ;

						//-----------

						// カーソル移動へ切り替える
						m_BombActionState = 2 ;

						m_BombActionStateTimer.Reset() ;
					}
				}
				else
				if( m_BombActionState == 2 )
				{
					// 押しっぱなし
					if( m_BombActionStateTimer.IsFinished( m_BombReplaceDuration ) == true )
					{
						// ボムを発射する
						m_OnAttack?.Invoke( 3, Position ) ;

						//-----------

						m_BombActionStateTimer.Reset() ;
					}
				}
			}
			else
			{
				// 離した

				if( m_Owner.PlayerBombStocks.Count >  0 && m_BombActionState == 1 )
				{
					if( m_BombActionStateTimer.IsRunning( m_BombTriggerDuration ) == true )
					{
						// 時間以内にボム発射ボタンを離した

						if( m_IsBombCooldown == false )
						{
							// クールダウンは行われていない

							// ボムを発射する
							m_OnAttack?.Invoke( 2, Position ) ;
						}
					}
				}

				// 初期状態へ
				m_BombActionState = 0 ;
			}

			//----------------------------------
			// 後で削除する
#if false
			if( InputManager.GetKey( KeyCodes.H ) == true )
			{
				// 画面中央(マウス座標を画面中央に強制移動)
				Input.MouseMode = Input.MouseModeEnum.Captured ;	// 消える
			}
			if( InputManager.GetKey( KeyCodes.J ) == true )
			{
				Input.MouseMode = Input.MouseModeEnum.Confined ;
			}
			if( InputManager.GetKey( KeyCodes.K ) == true )
			{
				// 指定位置(マウス座標を現在位置に強制移動)
				Input.MouseMode = Input.MouseModeEnum.ConfinedHidden ;	// 消える
			}
#endif

			//----------------------------------

			if ( m_IsShieldActive == true )
			{
				m_OnShieldActive?.Invoke( ShieldActiveRate ) ;

				//---------------------------------

				// 現在無敵モード中なので時間経過を確認する
				if( m_ShieldActiveTimer.IsFinished( m_ShieldActiveDuration ) == true )
				{
					// 時間経過で無敵終了
					m_IsShieldActive = false ;

					// 自機の表示設定
					_ShieldEffect.Visible = false ;
					_Sprite.SelfModulate = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
				}
			}

			if( m_IsBombCooldown == true )
			{
				m_OnBombCooldown?.Invoke( BombCooldownRate ) ;

				//---------------------------------

				// ボム使用後のクールダウン中
				if( m_BombCooldownTimer.IsFinished( m_BombCooldownDuration ) == true )
				{
					// 時間経過でクールダウン終了
					m_IsBombCooldown = false ;
				}
			}

			//----------------------------------
			// オプションの生成・破棄

			m_OnOption?.Invoke( Position ) ;

			//----------------------------------

			// スクロール位置の反映
			m_Owner.SetBackgroundPositionX( Position.X - ( ScreenSize.X * 0.5f ) ) ;

			//----------------------------------

			// 無敵コマンドを解除して自爆する
			if
			(
				GamePad.GetButton( GamePad.L1 ) == true &&
				GamePad.GetButton( GamePad.R1 ) == true &&
				GamePad.GetButton( GamePad.L2 ) == true &&
				GamePad.GetButton( GamePad.R2 ) == true &&
				GamePad.GetButton( GamePad.L3 ) == true &&
				GamePad.GetButton( GamePad.R3 ) == true
			)
			{
				m_Owner.ClearNoDeathSuccessful() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// コリジョンに接触した際に呼び出される
		public void OnAreaEntered( Area2D area )
		{
			m_IsAreaEntered = true ;
			m_CollisionTargetArea = area ;
		}

		// 実際のコリジョンヒット処理(_Processから実行する)
		private void OnAreaHit( Area2D area )
		{
			if( m_IsShieldActive == true )
			{
				// 無敵中は無効
				return ;
			}

			//----------------------------------

			int damage = 0 ;
			if( area is Enemy enemy)
			{
				// エネミー本体に接触
				damage = enemy.Damage ;
			}
			else
			if( area is EnemyBullet enemyBullet )
			{
				// エネミー弾に接触
				damage = enemyBullet.Damage ;
			}

			if( damage >  0 )
			{
				// 実際にダメージがある場合のみコールバックを呼ぶ
				m_OnDamage?.Invoke( Position, damage ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// オーラエフェクトを設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetAura( bool state )
		{
			if( state == true )
			{
				_AuraEffect.Visible = true ;
				_AuraEffectAnimation.Play() ;
			}
			else
			{
				_AuraEffect.Visible = false ;
				_AuraEffectAnimation.Stop() ;
			}
		}

		/// <summary>
		/// シールドの無敵時間状態
		/// </summary>
		public float ShieldActiveRate
		{
			get
			{
				if( m_IsShieldActive == false )
				{
					return 1 ;
				}

				float timer = m_ShieldActiveTimer.Value ;
				if( timer >  m_ShieldActiveDuration )
				{
					timer  = m_ShieldActiveDuration ;
				}
				return 1 - ( timer / m_ShieldActiveDuration ) ;
			}
		}

		/// <summary>
		/// ボムのクールダウン状態
		/// </summary>
		public float BombCooldownRate
		{
			get
			{
				if( IsIsBombCooldown == false )
				{
					return 0 ;
				}

				float timer = m_BombCooldownTimer.Value ;
				if( timer >  m_BombCooldownDuration )
				{
					timer  = m_BombCooldownDuration ;
				}
				return  1 - ( timer / m_BombCooldownDuration ) ;
			}
		}

		/// <summary>
		/// 無敵モードを有効化する
		/// </summary>
		/// <param name="duration"></param>
		public void SetShieldActive( float duration )
		{
			// 無敵モード
			m_IsShieldActive		= true ;
			m_ShieldActiveDuration	= duration ;
			m_ShieldActiveTimer.Reset() ;

			//----------------------------------

			// コリジョンは無効化しない(エネミーの弾は無敵中に当たったら消去する)

			// 自機の表示設定
			_Sprite.SelfModulate = new Color( 1.0f, 1.0f, 1.0f, 0.5f ) ;
			_ShieldEffect.Visible = true ;
		}

		/// <summary>
		/// 無敵モードを有効化する
		/// </summary>
		/// <param name="duration"></param>
		public void SetBombCooldown( float duration )
		{
			// ボム使用後のクールダウン
			m_IsBombCooldown		= true ;
			m_BombCooldownDuration	= duration ;
			m_BombCooldownTimer.Reset() ;
		}

		//-------------------------------------------------------------------------------------------
	}
}
