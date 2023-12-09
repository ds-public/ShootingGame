using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;
using EaseHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		/// <summary>
		/// プレイヤーの攻撃力
		/// </summary>
		public int PlayerDamage
		{
			get
			{
				if( m_PlayerPower <  m_PlayerPowerTop )
				{
					return 1 ;
				}
				else
				{
					return 2 ;
				}
			}
		}

		// 基本攻撃方向
		private static Vector2 m_BaseShotDirection = new (  0, -1 ) ;

		// プレイヤーの攻撃の際に呼び出される
		private void OnPlayerAttack( int attackType, Vector2 position )
		{
			// 通常弾
			PlayerShotShapeTypes shapeType = PlayerShotShapeTypes.ShotWeakly ;

			if( attackType == 1 )
			{
				// ショット

				// 弾生成時では効果音は鳴らさない(弾数分の効果音が鳴ってしまうため)

				m_CombatAudio.PlaySe( SE.Shoot, pan: Player.RatioPosition.X, volume: 0.8f ) ;

				//----------------------------------

				// 外観
				if( m_PlayerPower >= m_PlayerPowerTop )
				{
					// 強化弾
					shapeType = PlayerShotShapeTypes.ShotStrong ;
				}

				// ダメージ値
				int damage = PlayerDamage ;

				// 弾の速度
				float speed =  960.0f ;	// 基本速度(960/60=16)

				// 速度補正(1200)　※すり抜けが怖いので(144/60=24)
				speed +=( 480.0f * PlayerShotSpeedRate ) ;

				// 同時発射数と方向
				int shotLevel = m_PlayerPower ;
				if( shotLevel >  7 )
				{
					shotLevel  = 7 ;
				}

				switch( shotLevel )
				{
					case 0 :
						// 前方
						CreatePlayerBullet( shapeType, position, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
					break ;
					case 1 :
						// 前方
						CreatePlayerBullet( shapeType, position + new Vector2( +12, -20 ), m_BaseShotDirection, speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -12, -20 ), m_BaseShotDirection, speed, damage: damage ) ;
					break ;
					case 2 :
						// 前方
						CreatePlayerBullet( shapeType, position + new Vector2( +16, -20 ), m_BaseShotDirection, speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -16, -20 ), m_BaseShotDirection, speed, damage: damage ) ;

						// 後方
						CreatePlayerBullet( shapeType, position, - m_BaseShotDirection, speed, damage, correction:  8 ) ;
					break ;
					case 3 :
						// 前方
						CreatePlayerBullet( shapeType, position + new Vector2( +24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, +1 ), speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, -1 ), speed, damage: damage ) ;

						// 後方
						CreatePlayerBullet( shapeType, position, - m_BaseShotDirection, speed, damage: damage, correction:  8 ) ;
					break ;
					case 4 :
						// 前方
						CreatePlayerBullet( shapeType, position + new Vector2( +24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, +2 ), speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, -2 ), speed, damage: damage ) ;

						// 後方
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( - m_BaseShotDirection, +30 ), speed, damage: damage, correction: 12 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( - m_BaseShotDirection, -30 ), speed, damage: damage, correction: 12 ) ;
					break ;
					case 5 :
						// 前方
						CreatePlayerBullet( shapeType, position + new Vector2( +24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, +3 ), speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, -3 ), speed, damage: damage ) ;

						// 後方
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( m_BaseShotDirection, +90 ), speed, damage: damage, correction: 12 ) ;
						CreatePlayerBullet( shapeType, position, - m_BaseShotDirection, speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( m_BaseShotDirection, -90 ), speed, damage: damage, correction: 12 ) ;
					break ;
					case 6 :
						// 前方
						CreatePlayerBullet( shapeType, position + new Vector2( +24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, +3 ), speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, -3 ), speed, damage: damage ) ;

						// 後方および側面
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector(   m_BaseShotDirection, +90 ), speed, damage: damage, correction: 12 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( - m_BaseShotDirection, +30 ), speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, - m_BaseShotDirection, speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( - m_BaseShotDirection, -30 ), speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector(   m_BaseShotDirection, -90 ), speed, damage: damage, correction: 12 ) ;
					break ;
					case 7 :
						// 前方
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( m_BaseShotDirection, +45 ), speed, damage: damage, correction: 12 ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( +24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, +3 ), speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
						CreatePlayerBullet( shapeType, position + new Vector2( -24, -20 ), ExMath.GetRotatedVector( m_BaseShotDirection, -3 ), speed, damage: damage ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( m_BaseShotDirection, -45 ), speed, damage: damage, correction: 12 ) ;

						// 後方および側面
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector(   m_BaseShotDirection, +90 ), speed, damage: damage, correction: 12 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( - m_BaseShotDirection, +30 ), speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, - m_BaseShotDirection, speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector( - m_BaseShotDirection, -30 ), speed, damage: damage, correction:  8 ) ;
						CreatePlayerBullet( shapeType, position, ExMath.GetRotatedVector(   m_BaseShotDirection, -90 ), speed, damage: damage, correction: 12 ) ;
					break ;
				}

				//---------------------------------------------------------
				// オプション

				int i, l = m_PlayerOptions.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					var playerOption = m_PlayerOptions[ i ] ;

					var optionPosition = playerOption.Position ;

					// 前方
					CreatePlayerBullet( shapeType, optionPosition, m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;

					if( m_PlayerPower >= 9 )
					{
						// 後方にも
						CreatePlayerBullet( shapeType, optionPosition, - m_BaseShotDirection, speed, damage: damage, correction: 16 ) ;
					}
				}

				//---------------------------------------------------------

				// 命中率計算用(最終的には削除予定)
				m_HitMaxCount ++ ;
			}
			else
			if( attackType == 2 )
			{
				// ボム

				if( m_PlayerBombStocks.Count == 0 )
				{
					// 不可
					return ;
				}

				//---------------------------------

				// 使用するボムの種類
				var bombType = m_PlayerBombStocks[ m_PlayerBombCursor ] ;

				m_PlayerBombStocks.RemoveAt( m_PlayerBombCursor ) ;

				if( m_PlayerBombCursor >= m_PlayerBombStocks.Count )
				{
					m_PlayerBombCursor  = m_PlayerBombStocks.Count - 1 ;
					if( m_PlayerBombCursor <  0 )
					{
						m_PlayerBombCursor  = 0 ;
					}
				}

				_HUD.SetBombStock( m_PlayerBombStocks, m_PlayerBombCursor, 1.0f ) ;

				switch( bombType )
				{
					// 収縮タイプ
					case BombTypes.Compression :
						CreatePlayerBomb( bombType, position, 1.0f, 2.0f, m_PlayerBombDamage, false ) ;
					break ;

					// 拡散タイプ
					case BombTypes.Diffusion :
						CreateDiffusionBomb( position ) ;
					break ;

					// 放射タイプ
					case BombTypes.Emission :
						CreateEmissionBomb( position, 0 ) ;
					break ;
				}

				// クールダウンを設定する
				_Player.SetBombCooldown( 5.0f ) ;
			}
			else
			if( attackType == 3 )
			{
				// ボム選択

				if( m_PlayerBombStocks.Count >  1 )
				{
					int l =  m_PlayerBombStocks.Count ;

					m_PlayerBombCursor = ( m_PlayerBombCursor + 1 + l ) % l ;

					SE.Play( SE.Selection ) ;
					_HUD.SetBombStock( m_PlayerBombStocks, m_PlayerBombCursor, _Player.BombCooldownRate ) ;
				}
			}
		}

		// プレイヤーの被弾の際に呼び出される
		private void OnPlayerDamage( Vector2 position, int damage )
		{
			if( damage <= 0 || m_IsNoDeathSuccessful == true )
			{
				// ダメージ０以下(または無敵モード)は無視する(無敵モードだとシールドゲージが変動せず表示が若干おかしくなるので以下の処理は行わない)
				return ;
			}

			if( m_IsNoDeathSuccessful == true )
			{
				// 無敵モードの場合はダメージ０として処理する
				damage = 0 ;
			}

			//----------------------------------------------------------

			// パワーダウン

			int playerPower = m_PlayerPower ;
			if( playerPower >  m_PlayerPowerTop )
			{
				playerPower  = m_PlayerPowerTop ;
			}

			playerPower  -= damage ;
			if( playerPower <  0 )
			{
				playerPower  = 0 ;
			}

			m_PlayerPower = playerPower ;

			// ショットスピートゲージの表示設定
			_HUD.SetShotSpeed( PlayerShotSpeedEnabled, PlayerShotSpeedRate ) ;

			if( m_PlayerPower <  m_PlayerPowerTop )
			{
				// オーラエフェクト無効化
				_Player.SetAura( false ) ;
			}

			//--------------

			// シールドダウン

			// シールド減らす前の現時点での無敵時間を取得する
			m_PlayerShieldActiveNow = GetPlayerShieldActive() ;

			int playerShield = m_PlayerShield ;
			if( playerShield >  m_PlayerShieldTop )
			{
				playerShield  = m_PlayerShieldTop ;
			}

			playerShield  -= damage ;

			m_PlayerShield = playerShield ;

			// シールドゲージの表示設定
			_HUD.SetShieldPod( m_PlayerShield, m_PlayerShieldActiveNow / m_PlayerShieldActiveMax ) ;

			if( m_PlayerShield >= 0 )
			{
				// プレイヤーの処理を継続する

				// 無敵
				_Player.SetShieldActive( m_PlayerShieldActiveNow ) ;	// ひとまず少しの間無敵
			}
			else
			if( m_IsNoDeathSuccessful == false )
			{
				// 無敵モードではない

				// プレイヤーの処理を終了する
				_Player.End() ;

				// 注意：プレイヤーが破壊されてもすぐにはエネミーのタスクは停止させない
				// 　　　コンバットが終了するまではエネミーの動作はさせたいため
				// 　　　ただしプレイヤーを狙って弾を撃つような処理は行わないようにする
				// 　　　m_Owner.IsPlayerDestroyed で判定する

				// プレイヤー爆発を生成する
				CreateExplosion( position ) ;

				if( InputManager.InputType == InputTypes.GamePad )
				{
					// 振動
					GamePad.SetMotorSpeeds( 1, 1, 1 ) ;
				}
				else
				{
					// カーソルは強制的に表示する
					Pointer.Visible = true ;
				}
			}
		}

		/// <summary>
		/// シールド使用中の無敵状態時に呼び出される
		/// </summary>
		/// <param name="durationRate"></param>
		private void OnPlayerShieldActive( float rate )
		{
			// durationRate は 1 → 0 で変化する

			// シールド使用後の無敵状態をＵＩに反映させる
			_HUD.SetShieldActive
			(
				rate * m_PlayerShieldActiveNow / m_PlayerShieldActiveMax
			) ;

			if( rate == 0 )
			{
				_HUD.SetShieldPod( m_PlayerShield, GetPlayerShieldActive() / m_PlayerShieldActiveMax ) ;
			}
		}

		/// <summary>
		/// ボム使用後のクールダウン状態時に呼び出される
		/// </summary>
		/// <param name="durationRate"></param>
		private void OnPlayerBombCooldown( float rate )
		{
			// durationRate は 1 → 0 で変化する

			// ボム使用後のクールダウンの状態をＵＩに反映させる
			_HUD.SetBombCooldown( rate ) ;

			if( rate == 0 )
			{
				_HUD.SetBombStock( m_PlayerBombStocks, m_PlayerBombCursor, 0 ) ;
			}
		}

		/// <summary>
		/// プレイヤーのオプション状態をチェックして適切に更新する
		/// </summary>
		/// <param name="position"></param>
		private void OnPlayerOption( Vector2 position )
		{
			int optionAmount = 0 ;

			if( m_PlayerPower == 8 )
			{
				optionAmount = 1 ;
			}
			else
			if( m_PlayerPower >= 9 )
			{
				optionAmount = 2 ;
			}

			//----------------------------------------------------------

			// 生成済みのオプションの数が足りない場合は生成する

			if( m_PlayerOptions.Count == optionAmount )
			{
				// 必要な数のオプションが生成済みである
				return ;
			}

			//----------------------------------------------------------

			if( m_PlayerOptions.Count <  optionAmount )
			{
				// 足りないので追加で生成する

				int i, l = optionAmount - m_PlayerOptions.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					var playerOption = CreatePlayerOption( _Player.Position, new Vector2(  0, -1 ), 600.0f, 0 ) ;

					m_PlayerOptions.Add( playerOption ) ;
				}
			}
			else
			if( m_PlayerOptions.Count >  optionAmount )
			{
				// 数が多いので破棄する

				int i, l = m_PlayerOptions.Count - optionAmount ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					var playerOption = m_PlayerOptions[ ^1 ] ;
					m_PlayerOptions.Remove( playerOption ) ;

					playerOption.SelfDestroy() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// 拡散タイプのボムを生成する
		private void CreateDiffusionBomb( Vector2 position )
		{
			// ８方向にゆらぎ(方向・距離・時間)をもたせつつ拡散タイプボムのショットを放つ

			var baseDirection = new Vector2(  0, -1 ) ;
			Vector2 direction ;

			int i, l = 8 ;
			float ra, angle ;
			float distance ;
			float duration ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				ra = ExMath.GetRandomRange( -3f, +3f ) ;
				angle = i * 45f + ra ;

				direction = ExMath.GetRotatedVector( baseDirection, angle ) ;
				distance = ExMath.GetRandomRange( 200.0f, 220.0f ) ;
				duration = ExMath.GetRandomRange( 0.2f, 0.25f ) ;

				// グラビティボール
				var playerShot = CreatePlayerBullet
				(
					PlayerShotShapeTypes.GravityBall, 
					position,
					direction,
					distance,
					duration,
					EaseTypes.EaseOutQuad,
					0,
					PlayerShotCollisionTypes.None,
					false,
					12,
					processingType: 1
				) ;

				// 画面外の幅を広くする(ボムが発生する前にグラビティボールが消えてしまうのを防ぐため)
				playerShot.ScreenMargin = 256.0f ;
			}
		}

		// 放射タイプのボムを生成する
		private void CreateEmissionBomb( Vector2 position, int phase )
		{
			if( phase == 0 )
			{
				// 演出
				CreatePlayerBomb( BombTypes.Emission, position, 0.2f, 0.5f, m_PlayerBombDamage, false, processingType: 2, isSe: false ) ;
			}
			else
			if( phase == 1 )
			{
				// 効果音(実際に発射する時に出す)
				CombatAudio.PlaySe( SE.Bomb, _Player.GetRatioPosition( position ).X ) ;

				//---------------------------------

				var baseDirection = new Vector2(  0, -1 ) ;
				Vector2 direction ;

				int i, l = 8 ;
				float angle ;
				float speed ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// ゆらぎは無し
					angle = i * 45f ;

					direction = ExMath.GetRotatedVector( baseDirection, angle ) ;
					speed = 600.0f ;

					// グラビティウェーブ
					var playerShot = CreatePlayerBullet
					(
						PlayerShotShapeTypes.GravityWave, 
						position,
						direction,
						speed,
						0,
						EaseTypes.Linear,
						m_PlayerBombDamage,
						PlayerShotCollisionTypes.Bomb,
						false,	// 貫通(処理自体を無効化する)
						12,
						processingType: 0
					) ;

					// 貫通する設定にする
					playerShot.IsPenetrating = true ;
				}
			}
		}
	}
}
