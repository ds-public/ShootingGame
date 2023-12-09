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
		// ステート：タイトル
		private async Task<States> State_Title( States previous )
		{
			//----------------------------------------------------------

			// タイトルＢＧＭ再生
			await BGM.PlayMainAsync( BGM.Title ) ;

			//----------------------------------------------------------

			// バックグラウンドの横位置を中央に戻す
			SetBackgroundPositionX( 0 ) ;

			// バージョンを読み出す
			var version = Resources.Load<string>( "Version.txt" ) ;
			version = version.TrimEnd( '\n' ) ;	// 最後に改行が入っていたら削る	

			// タイトル画面固有ＵＩ表示
			_HUD.ShowLayerTitle( version ) ;

			if( previous == States.Unknown )
			{
				// 別スクリーンシーンから遷移

				// フェードを解除する
				await Scene.WaitForFading( true ) ;
			}
			else
			{
				// 同スクリーンシーンでの遷移

				Fade.Mask( true ) ;

				await WaitForSeconds( 1.0f ) ;

				await Fade.In( 0.5f ) ;
			}

			//----------------------------------------------------------

			// 隠しコマンドが成功したかどうか
			bool isSpecialCommandSuccessful = false ;

			while( true )
			{
				if( IsSpacialCommandReady() == false )
				{
					if
					(
						GamePad.GetButtonDown( GamePad.B1 ) == true ||
						GamePad.GetButtonDown( GamePad.B2 ) == true ||
						GamePad.GetButtonDown( GamePad.O1 ) == true ||
						Pointer.GetButtonDown( 0 ) == true
					)
					{
						break ;
					}
				}

				// 隠しコマンドの入力チェック
				if( CheckSpacialCommand() == true )
				{
					// 隠しコマンドの入力成功
					isSpecialCommandSuccessful = true ;
					break ;
				}

				if( Keyboard.GetKeyDown( KeyCodes.F12 ) == true )
				{
					// ハイスコアリセット
					m_HiScore = 0 ;
					_HUD.SetHiScoreValue( m_HiScore, false ) ;
				}

				// １フレーム待つ
				await Yield() ;
			}

			//----------------------------------------------------------

			// 無敵
			bool isNoDeathSuccessful = false ;

			if
			(
				GamePad.GetButton( GamePad.L1 ) == true &&
				GamePad.GetButton( GamePad.R1 ) == true &&
				GamePad.GetButton( GamePad.L2 ) == true &&
				GamePad.GetButton( GamePad.R2 ) == true
			)
			{
				isNoDeathSuccessful = true ;
				if( isSpecialCommandSuccessful == false )
				{
					// 無敵コマンド成功
					SE.Play( SE.NoDeath ) ;
					await WaitForSeconds( 1f ) ;
				}
			}

			if( isSpecialCommandSuccessful == true )
			{
				// 特殊コマンド成功
				SE.Play( SE.SpacialCommand ) ;
				await WaitForSeconds( 1f ) ;
			}

			// タイトル画面固有ＵＩ消去
			_HUD.HideLayerTitle() ;

			// タイトルＢＧＭ停止
			BGM.StopMain() ;

			// 仮
			m_IsNoDeathSuccessful			= isNoDeathSuccessful ;
			m_IsSpecialCommandSuccessful	= isSpecialCommandSuccessful ;


			//----------------------------------
			// １フレームだけ待つ(でないとスタートボタンの押し継続で即時ボーズがかかってしまう)

			await Yield() ;

			//----------------------------------

			// バトルへ
			return States.Combat ;
		}
	}
}