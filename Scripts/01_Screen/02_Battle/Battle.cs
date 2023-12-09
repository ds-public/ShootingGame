using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;

namespace Sample_001
{
	public partial class Battle : ExNode
	{
		/// <summary>
		/// スクリーン
		/// </summary>
		[Export]
		private Node2D			_Screen ;

		/// <summary>
		/// バックグラウンド
		/// </summary>
		[Export]
		private Background		_Background ;

		/// <summary>
		/// プレイヤー
		/// </summary>
		[Export]
		private Player			_Player ;

		/// <summary>
		/// 外部クラス参照用プレイヤー
		/// </summary>
		public	Player			Player => _Player ; 

		/// <summary>
		/// プレイヤー弾
		/// </summary>
		[Export]
		private PackedScene[]	_PlayerShots = new PackedScene[ 4 ] ;

		/// <summary>
		/// プレイヤーオプション
		/// </summary>
		[Export]
		private PackedScene[]	_PlayerOptions = new PackedScene[ 1 ] ;

		/// <summary>
		/// エネミー
		/// </summary>
		[Export]
		private PackedScene[]	_Enemies = new PackedScene[ 32 ] ;

		/// <summary>
		/// エネミー弾
		/// </summary>
		[Export]
		private PackedScene[]	_EnemyBullets = new PackedScene[ 7 ] ;

		/// <summary>
		/// アイテム
		/// </summary>
		[Export]
		private PackedScene		_Item ;

		/// <summary>
		/// ボム
		/// </summary>
		[Export]
		private PackedScene		_Bomb ;

		/// <summary>
		/// 爆発演出
		/// </summary>
		[Export]
		private PackedScene		_Explosion ;

		/// <summary>
		/// ２ＤのＨＵＤ
		/// </summary>
		[Export]
		private HUD				_HUD ;


		//-------------------------------------------------------------------------------------------

		// 画面サイズ
		public Vector2		ScreenSize ; // Size of the game window.

		/// <summary>
		/// 画面のアスペクト比(X/Y)
		/// </summary>
		public float		ScreenAspectXY
		{
			get
			{
				return ScreenSize.X / ScreenSize.Y ;
			}
		}

		/// <summary>
		/// 画面のアスペクト比(Y/X)
		/// </summary>
		public float		ScreenAspectYX
		{
			get
			{
				return ScreenSize.Y / ScreenSize.X ;
			}
		}

		// スコア
		private int			m_Score ;

		// ハイスコア
		private int			m_HiScore ;
		private int			m_HiScore_Before ;

		// 命中率
		private int			m_HitCount ;
		private int			m_HitMaxCount ;

		// 撃破率
		private int			m_CrashCount ;
		private int			m_CrashMaxCount ;


		// 生存時間の時間計測用タイマー
		private SimpleTimer	m_SurvivalTimer ;

		// 生存時間(確定)
		private float		m_SurvivalTime ;

		/// <summary>
		/// 戦闘中のＢＧＭ管理
		/// </summary>
		public	AudioController		  CombatAudio => m_CombatAudio ;
		private	AudioController		m_CombatAudio ;

		//-----------------------------------

		// コンバット中かどうか
		private bool				m_IsCombatProcessing ;

		// ポーズ中かどうか
		private bool				m_IsPausing = false ;

		/// <summary>
		/// ポーズ中かどうか
		/// </summary>
		public	bool				  IsPausing	=> m_IsPausing ;

		//-----------------------------------

		/// <summary>
		/// プレイヤーが破壊されたかどうか
		/// </summary>
		public	bool		IsPlayerDestroyed	=> ( !_Player.Visible ) ;

		// プレイヤー破壊通知トークンソース
		private CancellationTokenSource	m_CombatFinishedTokenSource ;


		//-----------------------------------
		// プレイヤーの能力

		// パワー
		private int			m_PlayerPower ;

		// シールド(最高)
		private const int	m_PlayerPowerTop = 10 ;

		// パワー(最大)
		private const int	m_PlayerPowerMax = 20 ;

		/// <summary>
		/// ショットスピードの増加が有効になっているか
		/// </summary>
		public bool			PlayerShotSpeedEnabled
		{
			get
			{
				return ( m_PlayerPower >  m_PlayerPowerTop ) ;
			}
		}

		/// <summary>
		/// ショットスピードのゲージ量
		/// </summary>
		public float		PlayerShotSpeedRate
		{
			get
			{
				if( m_PlayerPower <= m_PlayerPowerTop )
				{
					return 0 ;
				}

				return ( float )( m_PlayerPower - m_PlayerPowerTop ) / ( float )( m_PlayerPowerMax - m_PlayerPowerTop ) ;
			}
		}

		//-----------------------------------

		// シールド
		private int			m_PlayerShield ;

		// シールド(最高)
		private const int	m_PlayerShieldTop = 10 ;

		// シールド(最大)
		private const int	m_PlayerShieldMax = 20 ; 

		// 無敵時間(最小)
		private const float	m_PlayerShieldActiveMin = 0.8f ;

		// 無敵時間(最大)
		private const float	m_PlayerShieldActiveMax = 1.6f ;

		/// <summary>
		/// 現在のシールドレベルに応じた無敵時間を取得する
		/// </summary>
		/// <returns></returns>
		public float GetPlayerShieldActive()
		{
			if( m_PlayerShield == 0 )
			{
				return 0 ;
			}

			if( m_PlayerShield <= m_PlayerShieldTop )
			{
				return m_PlayerShieldActiveMin ;
			}

			return
			(
				( m_PlayerShieldActiveMax - m_PlayerShieldActiveMin ) * 
				( float )( m_PlayerShield - m_PlayerShieldTop ) / ( float )( m_PlayerShieldMax - m_PlayerShieldTop )
			) + m_PlayerShieldActiveMin ;
		}

		// 無敵時間(現在)
		private float	m_PlayerShieldActiveNow = m_PlayerShieldActiveMin ;

		//-----------------------------------

		/// <summary>
		/// プレイヤーのボム所持数
		/// </summary>
		public	int			  PlayerBomb => m_PlayerBombStocks.Count ;	

		// ボムのストック状態
		private List<BombTypes>	m_PlayerBombStocks = new () ;

		/// <summary>
		/// ボムのストック状態
		/// </summary>
		public	List<BombTypes>	  PlayerBombStocks => m_PlayerBombStocks ;

		// ボム(最大)
		private const int	m_PlayerBombMax	= 10 ;

		// ボムの選択カーソル
		private int			m_PlayerBombCursor = 0 ;

		// ボムのダメージ
		private int			m_PlayerBombDamage	= 1000 ;

		//-----------------------------------

		// プレイヤーのオプション生成数

		private List<PlayerOption>	m_PlayerOptions = new () ;

		/// <summary>
		/// プレイヤーのオプション
		/// </summary>
		public	List<PlayerOption>	  PlayerOptions => m_PlayerOptions ;

		//-----------------------------------

		/// <summary>
		/// エネミーグループ情報
		/// </summary>
		public class EnemyGroupCounter
		{
			public EnemyGroupBase	EnemyGroup ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public EnemyGroupCounter( EnemyGroupBase enemyGroup )
			{
				EnemyGroup = enemyGroup ;
			}

			public int CountHit ;
			public int CountNow ;
			public int CountMax ;

			/// <summary>
			/// エネミーグループの全てのエネミーが消滅したか
			/// </summary>
			public bool IsFinished => ( CountNow == CountMax ) ;

			/// <summary>
			/// エネミーグループの全てのエネミーを殲滅したか
			/// </summary>
			public bool IsCompleted => ( CountHit == CountMax ) ;
		}

		// エネミーのグループ管理用
		private readonly Dictionary<int,EnemyGroupCounter>	m_EnemyGroupCounters = new () ;

		/// <summary>
		/// エネミーグループの生存状態の管理
		/// </summary>
		public	Dictionary<int,EnemyGroupCounter> EnemyGroupCounters => m_EnemyGroupCounters ;

		//-----------------------------------

		// 動的生成されたエンティティ
		private readonly List<Node>	m_Entities = new () ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			ScreenSize = _Screen.GetViewportRect().Size ;

			// 最初のスコアの初期化
			m_Score = 0 ;
			_HUD.SetScoreValue( m_Score, false ) ;

			// 最初のハイスコアの初期化
			LoadHiScore() ;
			_HUD.SetHiScoreValue( m_HiScore, false ) ;

			// 最初の命中率の初期化
			m_HitCount		= 0 ;
			m_HitMaxCount	= 0 ;
			_HUD.SetHitRateValue( m_HitCount, m_HitMaxCount ) ;

			// 最初の撃破率の初期化
			m_CrashCount	= 0 ;
			m_CrashMaxCount	= 0 ;
			_HUD.SetCrashRateValue( m_CrashCount, m_CrashMaxCount ) ;

			// プレイヤーは無効化
			_Player.SetActive( false ) ;

			//----------------------------------

			// 生存時間計測タイマー生成
			m_SurvivalTimer = new () ;

			// コンバット中のオーディオの管理を行う
			m_CombatAudio = new () ;

			// コンバット中ではない状態で初期化する
			m_IsCombatProcessing = false ;

			// ポーズ中ではない状態で初期化する
			m_IsPausing = false ;

			//----------------------------------

			// メインループ実行
			_ = MainLoop( States.Title ) ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( m_IsCombatProcessing == true )
			{
				// コンバット中のみポーズ可能
				if( GamePad.GetButtonDown( GamePad.O1 ) == true || Pointer.GetButtonDown( 2 ) == true )
				{
					if( m_IsPausing == false )
					{
						// ポーズ状態を有効化する
						SE.Play( SE.PauseOn ) ;

						ApplicationManager.PauseTime() ;

						m_CombatAudio.Pause() ;

						_Background.SetPause( true ) ;

						_HUD.SetPause( true ) ;

						m_IsPausing = true ;
					}
					else
					{
						// ポーズ状態を無効化する

						m_IsPausing = false ;

						_HUD.SetPause( false ) ;

						_Background.SetPause( false ) ;

						m_CombatAudio.Unpause() ;

						ApplicationManager.UnpauseTime() ;
					}
				}
			}

			if( m_CombatAudio != null && m_SurvivalTimer != null )
			{
				// 戦闘ＢＧＭの毎フレームの処理を行う
				m_CombatAudio.Update( m_SurvivalTimer.Value ) ;
			}
		}

		public override void _ExitTree()
		{
			// プレイヤー破壊通知トークンを破棄する
			DeleteCombatFinishedTokenSource() ;
		}


		//-----------------------------------------------------------

		// プレイヤー破壊通知トークンソースを生成する
		private void CreateCombatFinishedTokenSource()
		{
			DeleteCombatFinishedTokenSource() ;

			m_CombatFinishedTokenSource = new CancellationTokenSource() ;
		}

		// プレイヤー破壊通知トークンソースを破棄する
		private void DeleteCombatFinishedTokenSource()
		{
			if( m_CombatFinishedTokenSource != null )
			{
				if( m_CombatFinishedTokenSource.IsCancellationRequested == false )
				{
					m_CombatFinishedTokenSource.Cancel() ;
				}
				m_CombatFinishedTokenSource.Dispose() ;
				m_CombatFinishedTokenSource = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// メインループ
		private async Task MainLoop( States state )
		{
			States existing = state ;
			States previous = States.Unknown ;
			States next		= States.Unknown ;

			// ステート分岐
			while( existing != States.Unknown )
			{
				switch( existing )
				{
					case States.Title	: next = await State_Title( previous )			; break ;
					case States.Combat	: next = await State_Combat( previous )			; break ;
					case States.Defeat	: next = await State_Defeat( previous )			; break ;
				}

				previous = existing ;
				existing = next ;
			}
		}
	}
}
