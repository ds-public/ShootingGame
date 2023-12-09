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
	public partial class Battle
	{
		// ステート：バトル
		private async Task<States> State_Combat( States _ )
		{
			// プレイヤー破壊通知トークンソースを生成する
			CreateCombatFinishedTokenSource() ;

			//----------------------------------------------------------

			// 生存時間の計測開始
			m_SurvivalTimer.Reset() ;

			// コンバット中のオーディオ管理を開始する
			m_CombatAudio.StartBgm() ;

			// バトル画面のＵＩを表示する
			_HUD.ShowLayerCombat() ;

			//----------------------------------------------------------
			// プレイヤーの能力値を初期化する

			m_PlayerPower	= 0 ;
			m_PlayerShield	= 0 ;
			m_PlayerBombStocks.Clear() ;

			if( m_IsSpecialCommandSuccessful == true )
			{
				// 最強状態
				m_PlayerPower	= m_PlayerPowerMax ;
				m_PlayerShield	= m_PlayerShieldMax ;

				int b = ExMath.GetRandomRange( 0, 2 ) ;

				int i, l = m_PlayerBombMax ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_PlayerBombStocks.Add( ( BombTypes )b ) ;

					b = ( b + 1 ) % 3 ; 
				}
			}

			//----------------------------------

			// スコアを初期化する
			m_Score = 0 ;
			_HUD.SetScoreValue( m_Score, false ) ;

			// 命中率の初期化する
			m_HitCount		= 0 ;
			m_HitMaxCount	= 0 ;
			_HUD.SetHitRateValue( m_HitCount, m_HitMaxCount ) ;

			// 撃破率の初期化する
			m_CrashCount	= 0 ;
			m_CrashMaxCount	= 0 ;
			_HUD.SetCrashRateValue( m_CrashCount, m_CrashMaxCount ) ;

			// プレイヤーの準備を行う
			_Player.SetActive( true ) ;
			_Player.Start
			(
				OnPlayerAttack,
				OnPlayerDamage,
				OnPlayerShieldActive,
				OnPlayerBombCooldown,
				OnPlayerOption,
				this
			) ;

			// オーラ表示を反映させる(Start実行時は無効になっているため)
			if( m_PlayerPower >= m_PlayerPowerTop )
			{
				_Player.SetAura( true ) ;
			}

			// 生成オプション数をクリアする
			m_PlayerOptions.Clear() ;

			// ショットスピートゲージの表示設定
			_HUD.SetShotSpeed( PlayerShotSpeedEnabled, PlayerShotSpeedRate ) ;

			// シールドゲージの表示設定
			_HUD.SetShieldPod( m_PlayerShield, GetPlayerShieldActive() / m_PlayerShieldActiveMax ) ;

			// ボムストックの表示設定
			_HUD.SetBombStock( m_PlayerBombStocks, m_PlayerBombCursor, 0 ) ;

			//----------------------------------

			// バトル前のハイスコアを保存しておく
			m_HiScore_Before = m_HiScore ;

			//----------------------------------

			// コンバット中を設定する
			m_IsCombatProcessing = true ;

			// バトル処理を行う
			await ProcessEnemyGroup() ;

			// コンバット中を解除する
			m_IsCombatProcessing = false ;

			// 生存時間確定
			m_SurvivalTime = m_SurvivalTimer.Value ;

			//----------------------------------
			// ハイスコアをストレージに記録

			SaveHiScore() ;

			//----------------------------------

			// バトル画面のＵＩを隠蔽する
			_HUD.HideLayerCombat() ;

			// バトルＢＧＭ停止
			m_CombatAudio.StopBgm( 0.5f ) ;

			// 少し待つ
			await WaitForSeconds( 2 ) ;

			// デフィートへ
			return States.Defeat ;
		}
	}
}
