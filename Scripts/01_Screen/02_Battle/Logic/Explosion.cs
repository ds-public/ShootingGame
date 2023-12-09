using Godot ;
using System ;


namespace Sample_001
{
	/// <summary>
	/// 爆発制御クラス
	/// </summary>
	public partial class Explosion : CombatEntity
	{
		//-----------------------------------------------------------

		private float							m_Delay ;

		private Action<Explosion,Vector2>		m_OnDestroy ;

		private double							m_DelayTimer ;

		//-----------------------------------

		private bool							m_IsAreaEntered ;
		private Area2D							m_CollisionTargetArea ;

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
			float scale,
			float delay,
			Action<Explosion,Vector2> onDestroy,
			Battle owner,
			bool isFlip = false
		)
		{
			// 基底クラスの初期化を実行する
			Initialize( owner, isFlip, true ) ;

			//----------------------------------

			Position		= position ;

			Scale			= new Vector2( scale, scale ) ;

			m_Delay			= delay ;
			m_DelayTimer	= 0 ;

			m_OnDestroy		= onDestroy ;

			//----------------------------------

			ZIndex = 10 ;

			// コリジョンは無効
			SetCollisionEnabled( false ) ;

			//----------------------------------

			_Sprite.AnimationFinished += OnAnimationFinished ;

			if( m_Delay <= 0 )
			{
				// 再生する(ディレイ無し)
				Play() ;
			}

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

			// アニメーションを開始する
			_Sprite.AnimationFinished -= OnAnimationFinished ;
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
				// エネミーの撃破数カウントのため必要
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------

			// ダメージエフェクト処理
//			ProcessDamageEffectColor() ;

			//----------------------------------------------------------

			if( m_Delay >  0 )
			{
				m_DelayTimer += delta ;

				if( m_DelayTimer >  m_Delay )
				{
					m_Delay  = 0 ;

					// 再生
					Play() ;
				}
			}
		}

		//-----------------------------------------------------------

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
			// 何かに当たれば対象を消滅させる
			m_OnDestroy?.Invoke( this, Position ) ;
		}

		//-------------------------------------------------------------------------------------------

		// アニメーションを再生する
		private void Play()
		{
			// 効果音を発生させる(ディレイがあるためここで再生するしかない)
			m_Owner.CombatAudio.PlaySe( SE.Explosion, pan: RatioPosition.X ) ;

			// アニメーションを開始する
			_Sprite.Play( "default" ) ;
		}

		// アニメーションが終了した際に呼び出される
		private void OnAnimationFinished()
		{
//			GD.Print( "爆発アニメーションが終わりました : " + animName ) ;

			m_OnDestroy?.Invoke( this, Position ) ;
		}
	}
}
