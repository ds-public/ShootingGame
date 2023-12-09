using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;


namespace Sample_001
{
	public partial class Enemy : CombatUnit
	{
		/// <summary>
		/// 接触ダメージ値
		/// </summary>
		public	int							  Damage => m_Damage ;
		private int							m_Damage ;

		/// <summary>
		/// 耐久値
		/// </summary>
		public	int							  Shield => m_ShieldNow ;
		private int							m_ShieldNow ;

		/// <summary>
		/// 初期の耐久値
		/// </summary>
		private	int							m_ShieldMax ;

		/// <summary>
		/// シールドの割合
		/// </summary>
		public float						  ShieldRatio
		{
			get
			{
				if( m_ShieldMax <= 0 )
				{
					return 0 ;
				}

				return ( float )m_ShieldNow / ( float )m_ShieldMax ;
			}
		}

		/// <summary>
		/// スコア
		/// </summary>
		public	int							  Score => m_Score ;
		private	int							m_Score ;


		/// <summary>
		/// エネミーのグループ識別子
		/// </summary>
		public	int							  GroupId => m_GroupId ;
		private int							m_GroupId ;


		// 処理のコールバック(通常時)
		private Func<Enemy, CancellationToken, Task>				m_OnUpdate ;

		// 処理のコールバック(破壊時)
		private Func<Enemy,EnemyDestroyedReasonTypes,bool>			m_OnDestroyed ;

		// 任意設定値
		public	System.Object				  Settings => m_Settings ;
		private System.Object				m_Settings ;

		/// <summary>
		/// レベル
		/// </summary>
		public	int							  Level => m_Level ; 
		private int							m_Level ;


		//-----------------------------------

		// 処理のコールバック(破壊時・ボス従属)
		private Action<Enemy,EnemyDestroyedReasonTypes,object>		m_OnDestroyedToBoss ;

		// ボスの任意設定値(ボスそのものでもある)
		public	System.Object				  BossSettings => m_BossSettings ;
		private System.Object				m_BossSettings ;

		// 他からの自爆要求
		private	bool						m_IsDestroyRequested = false ;

		//-----------------------------------

		/// <summary>
		/// 爆発のスケール係数
		/// </summary>
		public	float						ExplosionScale = 1 ;

		/// <summary>
		/// 爆発の表示回数
		/// </summary>
		public	int							ExplosionTimes = 1 ;

		//-----------------------------------

		// 破壊通知用のトークンソース
		private CancellationTokenSource		m_EnemyDestroyedTokenSource ;


		private bool						m_IsAreaEntered ;
		private Area2D						m_CollisionTargetArea ;

		//-----------------------------------------------------------

		// エネミー破壊通知トークンソースを生成する
		private void CreateEnemyDestroyedTokenSource( CancellationToken combatFinishedTocken )
		{
			DeleteEnemyDestroyedTokenSource() ;

			m_EnemyDestroyedTokenSource = CancellationTokenSource.CreateLinkedTokenSource( combatFinishedTocken ) ;
		}

		// エネミー破壊通知トークンソースを破棄する
		private void DeleteEnemyDestroyedTokenSource()
		{
			if( m_EnemyDestroyedTokenSource != null )
			{
				if( m_EnemyDestroyedTokenSource.IsCancellationRequested == false )
				{
					m_EnemyDestroyedTokenSource.Cancel() ;
				}
				m_EnemyDestroyedTokenSource.Dispose() ;
				m_EnemyDestroyedTokenSource = null ;
			}
		}

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready(){}


		/// <summary>
		/// 動作を開始させる(バージョン２)
		/// </summary>
		/// <param name="position"></param>
		/// <param name="onAttak"></param>
		/// <param name="onDestroy"></param>
		public void Start
		(
			int damage,
			int shield,
			int score,
			int groupId,
			Func<Enemy,CancellationToken,Task> onUpdate,
			Func<Enemy,EnemyDestroyedReasonTypes,bool> onDestroyed, 
			System.Object settings,
			Battle owner,
			int level,
			bool isFlip,
			CancellationToken combatFinishedTocken
		)
		{
			// 基底クラスを初期化する
			Initialize( owner, isFlip, true, true ) ;

			//----------------------------------

			// 接触ダメージ値を保存する
			m_Damage		= damage ;

			// 耐久値を保存する
			m_ShieldNow		= shield ;
			m_ShieldMax		= shield ;

			// スコアを保存する
			m_Score			= score ;

			//----------------------------------

			// グループ識別子を保存する
			m_GroupId		= groupId ;

			//----------------------------------
			
			// 通常時コールバックを保存する
			m_OnUpdate		= onUpdate ;

			// 破壊時コールバックを保存する
			m_OnDestroyed	= onDestroyed ;

			// 任意設定値を保存する
			m_Settings		= settings ;

			// レベルを保持する
			m_Level			= level ;

			//----

			// エネミー破壊通知トークンソースを生成する
			CreateEnemyDestroyedTokenSource( combatFinishedTocken ) ;

			//----------------------------------

			// コリジョンの接触コールバック設定
			this.AreaEntered += OnAreaEntered ;
			m_IsAreaEntered = false ;

			//----------------------------------

			Visible = true ;
			SetProcess( true ) ;

			//----------------------------------------------------------

			// 他からの自爆要求をクリアする
			m_IsDestroyRequested = false ;

			// 動作の処理を実行する
			if( m_OnUpdate != null )
			{
				_ = m_OnUpdate( this, m_EnemyDestroyedTokenSource.Token ) ;
			}
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

			// コリジョンヒットをバイパスするためだけに使用する(コリジョンヒットのコールバック内の処理は様々な制限がるため)
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

			if( m_IsDestroyRequested == true )
			{
				// 自爆要求が出されていたら自爆する
				m_IsDestroyRequested  = false ;

				SelfDestroy() ;
			}
		}

		public override void _ExitTree()
		{
			// エネミー破壊通知トークンソースを破棄する
			DeleteEnemyDestroyedTokenSource() ;
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
			EnemyDestroyedReasonTypes destroyedReasonType = EnemyDestroyedReasonTypes.OutOfScreen ;

			int damage = 0 ;
			
			if( area is PlayerShot playerShot )
			{
				damage = playerShot.Damage ;
				destroyedReasonType = EnemyDestroyedReasonTypes.PlayerShot ;
			}
			else
			if( area is PlayerBomb playerBomb )
			{
				damage = playerBomb.Damage ;
				destroyedReasonType = EnemyDestroyedReasonTypes.PlayerBomb ;
			}

			m_ShieldNow -= damage ;

			if( m_ShieldNow >  0 )
			{
				m_Owner.CombatAudio.PlaySe( SE.Hit, RatioPosition.X ) ;

				SetDamageEffect() ;
			}
			else
			{
				// エネミー破壊(個別)
				if( m_OnDestroyed?.Invoke( this, destroyedReasonType ) == true )
				{
					// 少し早めだがタスクキャンセルを発行しておく
					m_EnemyDestroyedTokenSource?.Cancel() ;

					//----------------------------------

					// ボス従属であればボスに通知
					m_OnDestroyedToBoss?.Invoke( this, destroyedReasonType, m_BossSettings ) ;

					//----------------------------------

					// エネミー破壊(全体)
					m_Owner?.OnEnemyDestroyed( this, Position, destroyedReasonType ) ;
				}
			}
		}

		/// <summary>
		/// 自身で自爆
		/// </summary>
		public void SelfDestroy()
		{
			EnemyDestroyedReasonTypes destroyedReasonType = EnemyDestroyedReasonTypes.Self ;

			// エネミー破壊(個別)
			if( m_OnDestroyed?.Invoke( this, destroyedReasonType ) == true )
			{
				// 少し早めだがタスクキャンセルを発行しておく
				m_EnemyDestroyedTokenSource?.Cancel() ;

				//----------------------------------

				// ボス従属であればボスに通知
				m_OnDestroyedToBoss?.Invoke( this, destroyedReasonType, m_BossSettings ) ;

				//----------------------------------

				// エネミー破壊(全体)
				m_Owner?.OnEnemyDestroyed( this, Position, destroyedReasonType ) ;
			}
		}

		/// <summary>
		/// 他からの自爆要求
		/// </summary>
		public void RequestDestroy()
		{
			// 破棄要求
			m_IsDestroyRequested = true ;
		}

		/// <summary>
		/// 画面外扱いという事で消去する
		/// </summary>
		public void OutOfScreen()
		{
			EnemyDestroyedReasonTypes destroyedReasonType = EnemyDestroyedReasonTypes.OutOfScreen ;

			//----------------------------------

			// ボス従属であればボスに通知
			m_OnDestroyedToBoss?.Invoke( this, destroyedReasonType, m_BossSettings ) ;

			//----------------------------------

			// 画面外で消滅
			m_Owner?.OnEnemyDestroyed( this, Position, destroyedReasonType ) ;
		}

		//-------------------------------------------------------------------------------------------
		// ボス従属関連

		/// <summary>
		/// ボスの設定を保持させる
		/// </summary>
		/// <param name="bossSettings"></param>
		public void SetBossSettings( System.Object bossSettings )
		{
			m_BossSettings		= bossSettings ;
		}

		/// <summary>
		/// ボス従属である場合の通知コールバックを設定する(将来的にはフェーズ変化などにも対応するかも
		/// </summary>
		/// <param name="bossSettings"></param>
		/// <param name="onDestroyedToBoss"></param>
		public void SetOnBossCallbacks( System.Object bossSettings, Action<Enemy,EnemyDestroyedReasonTypes,System.Object> onDestroyedToBoss )
		{
			m_BossSettings		= bossSettings ;
			m_OnDestroyedToBoss	= onDestroyedToBoss ;
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
	

		/// <summary>
		/// コリジョンタイプを設定する
		/// </summary>
		/// <param name="collisionType"></param>
		public void SetCollisionType( EnemyCollisionTypes collisionType )
		{
			CollisionLayer = 0x04 ;

			switch( collisionType )
			{
				case EnemyCollisionTypes.None :
					CollisionMask = 0x00 ;
				break ;

				// Shot と Bomb にヒットする
				case EnemyCollisionTypes.Unit :
					CollisionMask = 0x22 ;
				break ;

				// Shot のみにヒットする
				case EnemyCollisionTypes.Boss :
					CollisionMask = 0x02 ;
				break ;
			}
		}
	}
}

