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
		// ステート：デフィート
		private async Task<States> State_Defeat( States _ )
		{
			bool isHiScoreUpdated = false ;
			if( m_HiScore >  m_HiScore_Before )
			{
				isHiScoreUpdated = true ;
			}
//			isHiScoreUpdated = true ;	// デバッグ

			//----------------------------------
			// 生存時間をセット

			_HUD.SetSurvivalTime( m_SurvivalTime ) ;

			//----------------------------------------------------------

			if( isHiScoreUpdated == false )
			{
				// デフィートＢＧＭ再生
				await BGM.PlayMainAsync( BGM.Defeat ) ;
			}

			//----------------------------------

			States existing ;

			_HUD.ShowLayerDefeat() ;

			float duration = 10.0f ;

			//----------------------------------
			// ハイスコアを更新していたら通知を表示する

			int playId = -1 ;

			if( isHiScoreUpdated == true )
			{
				playId = await SE.PlayAsync( SE.Victory ) ;

				// ハイスコア更新
				_HUD.ShowCongratulations() ;

				duration = 300.0f ;
			}

			//----------------------------------

			var timer = new SimpleTimer() ;

			while( true )
			{
				if( isHiScoreUpdated == true && playId >= 0 )
				{
					if( SE.IsPlaying( playId ) == false )
					{
						playId = -1 ;
						await BGM.PlayMainAsync( BGM.Victory ) ;
					}
				}

				//---------------------------------

				if
				(
					timer.IsFinished( duration ) == true		||
					GamePad.GetButtonDown( GamePad.B3 ) == true ||
					GamePad.GetButtonDown( GamePad.B4 ) == true ||
					GamePad.GetButtonDown( GamePad.O2 ) == true ||
					Pointer.GetButtonDown( 1 ) == true
				)
				{
					// タイトルへ
					existing = States.Title ;
					break ;
				}

				if
				(
					GamePad.GetButtonDown( GamePad.B1 ) == true ||
					GamePad.GetButtonDown( GamePad.B2 ) == true ||
					GamePad.GetButtonDown( GamePad.O1 ) == true ||
					Pointer.GetButtonDown( 0 ) == true
				)
				{
					// バトルへ
					existing = States.Combat ;
					break ;
				}

				// １フレーム待つ
				await Yield() ;
			}

			//----------------------------------------------------------

			_HUD.HideLayerDefeat() ;

			// プレイヤー破壊通知トークンソースを破棄する
			DeleteCombatFinishedTokenSource() ;

			// 残っているエンティティを全て強制的に破棄する
			DestroyAllEntities() ;

			// １フレーム待たないと完全にエンティティが破棄されない(エネミーから弾が飛んでくる可能性がある)
			await Yield() ;

			if( existing == States.Title )
			{
				// タイトルへ

				// デフィトートＢＧＭ停止
				BGM.StopMain( 0.5f ) ;

				await Fade.Out( 0.5f ) ;
			}
			else
			{
				// バトルへ

				// デフィトートＢＧＭ停止
				BGM.StopMain() ;

				SE.StopAll() ;
			}

			// スコアのカウントアップが行われている可能性があるためスコアの強制更新を行う
			_HUD.SetScoreValue( m_Score, false ) ;
			_HUD.SetHiScoreValue( m_HiScore, false ) ;

			return existing ;
		}
	}
}
