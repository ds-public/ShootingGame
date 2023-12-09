using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		/// <summary>
		/// 戦闘中のＢＧＭを管理するクラス
		/// </summary>
		public class AudioController
		{
			/// <summary>
			/// 処理が既に開始されているかどうか
			/// </summary>
			public	bool	  IsStarted => m_IsStarted ;
			private	bool	m_IsStarted ;

			private float	m_SurvivalTime ;

			//--------------

			// １ステージの時間
			private float	m_StageIntervalTime ;

			private float	m_StageFadeOutDuration ;
			private float	m_BossFadeInDuration ;
			private float	m_BossFadeOutDuration ;

			//----

			private int		m_StageBgmNumber ;

			private readonly SimpleTimer m_StageFadeTimer ;
			private float	m_StageFadeDuration ;
			private int		m_StageFadeType ;
			private float	m_StageFadeVolume ;
			private int		m_StageFadeNumber ;

			private int		m_BossBgmPlayId ;

			private readonly SimpleTimer m_BossFadeTimer ;
			private float	m_BossFadeDuration ;
			private int		m_BossFadeType ;
			private float	m_BossFadeVolume ;

			//----------------------------------------------------------
			// ＳＥ関係の管理

			private readonly Dictionary<string,double>	m_SeStartingTimes ;


			//----------------------------------------------------------

			// ＢＧＭの名前群
			private static readonly string[] m_BgmNames =
			{
				BGM.Stage_01, BGM.Stage_02, BGM.Stage_03, BGM.Stage_04, BGM.Stage_05, BGM.Stage_06
			} ;


			/// <summary>
			/// コンストラクタ
			/// </summary>
			public AudioController()
			{
				m_IsStarted				= false ;
				m_SurvivalTime			= -1 ;

				m_StageFadeTimer		= new SimpleTimer() ;
				m_BossFadeTimer			= new SimpleTimer() ;

				m_SeStartingTimes		= new () ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// ステージＢＧＭのフェード中かどうか
			/// </summary>
			public bool IsFading
			{
				get
				{
					return ( m_StageFadeDuration != 0 ) ;
				}
			}

			//----------------------------------------------------------

			/// <summary>
			/// 処理を開始する
			/// </summary>
			public void StartBgm( float survivalTime = 0 )
			{
				m_IsStarted				= true ;

				//-------------

				m_StageBgmNumber		= -1 ;

				m_StageFadeDuration		=  0 ;
				m_StageFadeType			=  0 ;
				m_StageFadeVolume		=  1 ;
				m_StageFadeNumber		= -1 ;

				m_BossBgmPlayId			= -1 ;

				m_BossFadeDuration		=  0 ;
				m_BossFadeType			=  0 ;
				m_BossFadeVolume		=  1 ;

				m_StageIntervalTime		= 150.0f ;	// １ステージの時間(秒)
//				m_StageIntervalTime		=  15.0f ;	// １ステージの時間(秒)　デバッグ
				m_StageFadeOutDuration	=   6.0f ;	// １ステージの終わりのフェードアウト時間(秒)
				m_BossFadeInDuration	=   5.0f ;	// ボス出現時のフェードアウト時間(秒)
				m_BossFadeOutDuration	=   2.0f ;	// ボス撃破後のフェードイン時間(秒)

				//---------------------------------

				m_SeStartingTimes.Clear() ;

				//---------------------------------

				Update( survivalTime ) ;
			}

			/// <summary>
			/// 処理を停止する
			/// </summary>
			public void StopBgm( float fadeTime )
			{
				m_IsStarted			= false ;
				m_SurvivalTime		= -1 ;

				BGM.StopMain( fadeTime ) ;

				if( m_BossBgmPlayId >= 0 )
				{
					BGM.Stop( m_BossBgmPlayId, fadeTime ) ;
					m_BossBgmPlayId  = -1 ;
				}

				//---------------------------------

				m_SeStartingTimes.Clear() ;
			}

			/// <summary>
			/// 毎フレームＢＧＭの管理を実行する
			/// </summary>
			/// <param name="survivalTime"></param>
			public void Update( float survivalTime )
			{
				if( m_IsStarted == false )
				{
					// 処理は開始されていない
					return ;
				}

				//---------------------------------

				// ボスＢＧＭで処理するので保存しておく
				m_SurvivalTime = survivalTime ;

				//---------------------------------

				// メインＢＧＭにフェードがかかっている
				if( m_StageFadeDuration >  0 || m_BossFadeDuration >  0 )
				{
					// Stage
					float stageFadeFactor = 0 ;
					if( m_StageFadeDuration >  0 )
					{
						stageFadeFactor = m_StageFadeTimer.Value / m_StageFadeDuration ;
						if( stageFadeFactor >  1 )
						{
							stageFadeFactor  = 1 ;
						}

						if( m_StageFadeType >  0 )
						{
							// イン
							m_StageFadeVolume = stageFadeFactor ;
						}
						else
						if( m_StageFadeType <  0 )
						{
							// アウト
							m_StageFadeVolume = 1 - stageFadeFactor ;

//							GD.Print( "フェードアウト中 : " + m_StageFadeVolume ) ;
						}
					}

					// Boss
					float bossFadeFactor = 0 ;
					if( m_BossFadeDuration >  0 )
					{
						bossFadeFactor = m_BossFadeTimer.Value / m_BossFadeDuration ;
						if( bossFadeFactor >  1 )
						{
							bossFadeFactor  = 1 ;
						}

						if( m_BossFadeType >  0 )
						{
							// イン
							m_BossFadeVolume = bossFadeFactor ;
						}
						else
						if( m_BossFadeType <  0 )
						{
							// アウト
							m_BossFadeVolume = 1 - bossFadeFactor ;
						}
					}

					//--------------------------------

					// ボリューム設定
					BGM.Volume = m_StageFadeVolume * m_BossFadeVolume ;

					//--------------------------------

					if( stageFadeFactor >= 1 )
					{
						// 終了

						if( m_StageFadeType <  0 )
						{
							// フェードアウトの場合は最後にステージＢＧＭを停止させる
							BGM.StopMain() ;

							m_StageFadeVolume = 1 ;	// １に戻す

//							GD.Print( "ステージＢＧＭを停止させました : " + m_StageBgmNumber ) ;
						}

						m_StageFadeDuration	=  0 ;
						m_StageFadeType		=  0 ;
					}

					if( bossFadeFactor >= 1 )
					{
						// 終了
						m_BossFadeDuration	=  0 ;
						m_BossFadeType		=  0 ;
					}
				}

				//---------------------------------

				if( m_SurvivalTime >= 0 )
				{
					int stageBgmNumber = ( int )( m_SurvivalTime / m_StageIntervalTime ) % m_BgmNames.Length ;
					float offset = m_SurvivalTime % m_StageIntervalTime ;

					// バトルＢＧＭ再生
					if( BGM.IsPlayingMain() == false )
					{
						// ＢＧＭは鳴っていない

						// ボスによって中断されたのではなければ最初から再生する

						//-------------------------------
						// 再生する

						if( m_StageBgmNumber != stageBgmNumber )
						{
							m_StageBgmNumber  = stageBgmNumber ;

//							GD.Print( "ステージＢＧＭを再生させます : " + m_StageBgmNumber ) ;

							// ステージＢＧＭを再生する
							var bgmName = m_BgmNames[ m_StageBgmNumber ] ;
							_ = BGM.PlayMainAsync( bgmName, volume: m_StageFadeVolume * m_BossFadeVolume ) ;
						}
					}

					// ステージ終了チェック
					if( offset >= ( m_StageIntervalTime - m_StageFadeOutDuration ) )
					{
						if( m_StageFadeNumber != m_StageBgmNumber && m_StageFadeDuration == 0 )
						{
							m_StageFadeNumber  = m_StageBgmNumber ;

							m_StageFadeDuration	= m_StageFadeOutDuration - 1.0f ;
							m_StageFadeType		= -1 ;	// フェードアウト
							m_StageFadeVolume	=  1 ;
							m_StageFadeTimer.Reset() ;

//							GD.Print( "ステージＢＧＭのフェードアウト開始 : " + m_StageBgmNumber + " " + m_StageFadeDuration ) ;
						}
					}
				}
			}

			//----------------------------------------------------------

			/// <summary>
			/// ボスによってＢＧＭが中断される
			/// </summary>
			public void PauseByBoss()
			{
				if( m_IsStarted == false || m_SurvivalTime <  0 )
				{
					// 処理不可
					return ;
				}

				//---------------------------------

				m_BossFadeDuration	= m_BossFadeInDuration ;
				m_BossFadeType		= -1 ;
				m_BossFadeVolume	=  1 ;
				m_BossFadeTimer.Reset() ;
			}

			/// <summary>
			/// ボスＢＧＭを再生する
			/// </summary>
			public void PlayBossBgm()
			{
				if( m_IsStarted == false || m_SurvivalTime <  0 )
				{
					// 処理不可
					return ;
				}

				//---------------------------------

				// ボスＢＧＭを再生させる
				m_BossBgmPlayId = BGM.Play( BGM.Boss ) ;
			}

			/// <summary>
			/// ボスＢＧＭを停止する
			/// </summary>
			public void StopBossBgm()
			{
				if( m_IsStarted == false || m_SurvivalTime <  0 )
				{
					// 処理不可
					return ;
				}

				//---------------------------------

				if( m_BossBgmPlayId != -1 )
				{
					// ボスＢＧＭをフェードアウトで停止させる
					BGM.Stop( m_BossBgmPlayId, m_BossFadeOutDuration ) ;
					m_BossBgmPlayId = -1 ;
				}

				//---------------------------------

				// ステージＢＧＭをフェードインさせる
				m_BossFadeDuration	= m_BossFadeOutDuration ;
				m_BossFadeType		= +1 ;		// フェードイン
				m_BossFadeVolume	=  0 ;
				m_BossFadeTimer.Reset() ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// 再生中のＢＧＭを一時停止する
			/// </summary>
			public void Pause()
			{
				BGM.PauseMain() ;

				if( m_BossBgmPlayId >= 0 )
				{
					BGM.Pause( m_BossBgmPlayId ) ;
				}
			}

			/// <summary>
			/// ＢＧＭを再開する
			/// </summary>
			public void Unpause()
			{
				BGM.UnpauseMain() ;

				if( m_BossBgmPlayId >= 0 )
				{
					BGM.Unpause( m_BossBgmPlayId ) ;
				}
			}

			//------------------------------------------------------------------------------------------
			// ＳＥ関係

			/// <summary>
			/// 近い時間での再生を抑制したＳＥ再生
			/// </summary>
			/// <param name="seName"></param>
			/// <param name="pan"></param>
			public void PlaySe( string seName, float pan = 0, float volume = 1 )
			{
				var masterTime = ApplicationManager.MasterTime ;

				if( m_SeStartingTimes.ContainsKey( seName ) == true )
				{
					// 既に１度再生した事のあるＳＥ

					var seStaringTime = m_SeStartingTimes[ seName ] ;

					if( masterTime <  ( seStaringTime + 0.1f ) )
					{
						// 近すぎるので再生不可
						return ;
					}
				}

				//---------------------------------

				// ＳＥ再生
				SE.Play( seName, pan: pan,  volume:volume ) ;

				if( m_SeStartingTimes.ContainsKey( seName ) == false )
				{
					// 初めて再生するＳＥ
					m_SeStartingTimes.Add( seName, masterTime ) ;
				}
				else
				{
					// ２回目以降の再生のＳＥ
					m_SeStartingTimes[ seName ] = masterTime ;
				}
			}
		}
	}
}
