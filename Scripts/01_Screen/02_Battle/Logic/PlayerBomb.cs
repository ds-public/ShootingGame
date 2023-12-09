using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading.Tasks ;

using EaseHelper ;


namespace Sample_001
{
	public partial class PlayerBomb : CombatUnit
	{
		// 基本のスケール
		private const float					m_BaseSclae = 3.0f ;

		//-----------------------------------------------------------

		// 生存期間
		private float						m_Duration ;
		
		// 変化状態
		private int							m_Step ;

		/// <summary>
		/// 調整スケール
		/// </summary>
		private float						m_CorrectionScale = 1.0f ;

		//-----------------------------------


		/// <summary>
		/// 与えるダメージ
		/// </summary>
		public	int							  Damage => m_Damage ;

		// ダメージ
		private int							m_Damage ;

		// 撃破時のコールバック
		private Action<PlayerBomb,Vector2,Node>	m_OnAttack ;

		// 破棄時のコールバック
		private Action<PlayerBomb,Vector2>		m_OnDestroy ;


		/// <summary>
		/// 処理タイプ
		/// </summary>
		public	int							  ProcessingType => m_ProcessingType ;

		// 処理タイプ
		private int							m_ProcessingType ;

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
		/// <param name="posiition"></param>
		/// <param name="direction"></param>
		public void Start
		(
			Vector2 position,
			float scale,
			float duration,
			int damage,
			bool isHitCheck,
			Action<PlayerBomb,Vector2,Node> onAttack,
			Action<PlayerBomb,Vector2> onDestroy,
			Battle owner,
			bool isFlip,		// デフォルト左右反転
			int processingType	// [予備]処理タイプの識別値
		)
		{
			// 基底クラスを初期化する
			Initialize( owner, isFlip, true, false ) ;

			//----------------------------------

			// 位置
			Position			= position ;

			// 補正スケール
			m_CorrectionScale	= scale ;

			// 生存期間
			m_Duration			= duration ;
			
			//----------------------------------

			// スケールを初期化
			Scale = Vector2.Zero ;

			// 状態を初期化
			m_Step = 0 ;

			//--------------

			// エネミーが接触した際に与えるダメージを保存する
			m_Damage			= damage ;

			// コリジョンの設定
			CollisionLayer	= 0x20 ;

			if( isHitCheck == true )
			{
				// コリジョンヒットが検出される(ボム自体は無敵なので検出されても現状処理するものが無い)
				CollisionMask	= 0xC0 ;
			}
			else
			{
				// コリジョンヒットの検出を無効化して高速化
				CollisionMask	= 0x00 ;
			}

			//----------------------------------

			m_OnAttack			= onAttack ;
			m_OnDestroy			= onDestroy ;

			// 処理タイプ
			m_ProcessingType	= processingType ;

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
				// エネミーの撃破数カウントのため必要
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------

			// ダメージエフェクト処理
//			ProcessDamageEffectColor() ;

			//----------------------------------------------------------

			if( m_Step == 0 )
			{
				// 広がる

				float duration = m_Duration * 0.2f ;	// 全体の時間の２０％

				float time = m_Timer.Value ;

				if( time >  duration )
				{
					time  = duration ;   
				}

				float factor = Ease.GetValue( time / duration, EaseTypes.EaseOutQuad ) ;

				float scale = m_BaseSclae * m_CorrectionScale * factor ;
				Scale = new Vector2( scale, scale ) ;

				if( time >= duration )
				{
					m_Step = 1 ;
					m_Timer.Reset() ;
				}
			}
			else
			if( m_Step == 1 )
			{
				// 少し維持

				float duration = m_Duration * 0.6f ;	// 全体の時間の６０％

				float time = m_Timer.Value ;

				if( time >  duration )
				{
					time  = duration ;   
				}

				float factor = time / duration ;
				float radian = Mathf.Pi * factor * 2.0f ; 
				factor = ( Mathf.Abs( Mathf.Cos( radian ) ) * 0.05f + 0.95f ) ;

				float scale = m_BaseSclae * m_CorrectionScale * factor ;
				Scale = new Vector2( scale, scale ) ;

				factor *= factor ;
				Modulate = new Color( factor, factor, factor, factor ) ;

				if( time >= duration )
				{
					m_Step = 2 ;
					m_Timer.Reset() ;
				}
			}
			else
			if( m_Step == 2 )
			{
				// 狭まる

				float duration = m_Duration * 0.2f ;	// 全体の時間の２０％

				float time = m_Timer.Value ;

				if( time >  duration )
				{
					time  = duration ;   
				}

				float factor = 1 - Ease.GetValue( time / duration, EaseTypes.EaseInQuad ) ;

				float scale = m_BaseSclae * m_CorrectionScale * factor ;
				Scale = new Vector2( scale, scale ) ;

				if( time >= duration )
				{
					m_Step = 3 ;
					m_Timer.Reset() ;
				}
			}
			else
			if( m_Step == 3 )
			{
				m_Step = 4 ;

				// 破棄
				m_OnDestroy?.Invoke( this, Position ) ;
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
			m_OnAttack?.Invoke( this, Position, area ) ;
		}

		/// <summary>
		/// 自爆
		/// </summary>
		public void SelfDestroy()
		{
			// 何かに当たれば消える(可能性がある)
			m_OnDestroy?.Invoke( this, Position ) ;
		}

		/// <summary>
		/// 画面外扱いという事で消去する
		/// </summary>
		public void OutOfScreen()
		{
			// 画面外で消滅
			m_OnDestroy?.Invoke( this, Position ) ;
		}

		//-------------------------------------------------------------------------------------------

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

