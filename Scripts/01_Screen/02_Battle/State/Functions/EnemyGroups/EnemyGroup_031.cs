using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;
using EaseHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		/// <summary>
		/// エネミーグループ(種別 031)
		/// </summary>
		public class EnemyGroup_031 : EnemyGroupBase
		{
			/// <summary>
			/// エルメス(本体)
			/// </summary>
			/// <param name="owner"></param>
			/// <param name="level"></param>
			/// <param name="groupId"></param>
			/// <param name="combatFinishedToken"></param>
			/// <returns></returns>
			public override float Run( Battle owner, int level, int groupId, CancellationToken combatFinishedToken )
			{
				// 開始設定を行う(重要)
				Startup( owner, combatFinishedToken ) ;

				//---------------------------------

				// 先行してしてカウンターを null で登録しておく
				owner.EnemyGroupCounters.Add( groupId, null ) ;

				// 出現処理を実行する
				_ = Process( owner, level, groupId ) ;

				// 次のグループ処理までの最低待ち時間を返す(グループ全滅待ちの場合は０を返す)
				return IntervalTime ;
			}

			// グループ処理を行う
			private async Task Process( Battle owner, int level, int groupId )
			{
				// 現在処理中
				IsProcerssing = true ;

				//---------------------------------------------------------

				if( IntervalTime <= 0 )
				{
					// ＢＧＭの切り替えを行う
					m_Owner.CombatAudio.PauseByBoss() ;

					// ５秒のＢＧＭのフェードアウトを待つ
					await Wait( 5.0f ) ;

					// ボスＢＧＭを再生する
					m_Owner.CombatAudio.PlayBossBgm() ;
				}

				//---------------------------------------------------------

				int[] bossAmount_weights =
				{
					100,				// １体
					( level / 20 )		// ２体
				} ;

				int bossAmount = 1 + ExMath.GetRandomIndex( bossAmount_weights ) ;
//				bossAmount = 2 ;	// デバッグ

				// ボス１体につきユニットは４体固定
				int[] unitAmount_weights =
				{
					100,				// ４体
					( level / 4 ),		// ６体
					( level / 8 )		// ８体
				} ;

				int unitAmount = 4 + 2 * ExMath.GetRandomIndex( unitAmount_weights ) ;
//				unitAmount = 8 ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
//				if( level >= 60 )
//				{
//					isReflectorBullet  = true ;
//				}

				//---------------------------------

				int i, l = bossAmount ;

				// 出現数が確定した時点でカウンターを更新する
				var enemyGroupCounter = new EnemyGroupCounter( this )
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = l
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield(  80, 160, level ) ;
//				shield = 1 ;	// デバッグ

				float distance = 0.36f ;

				float xs = - ( l * distance * 0.5f ) + ( distance * 0.5f ) ;
				float ys = -0.65f ;
				
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(共通)
					var bossSettings = new BossSettings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						UnitAmount			= unitAmount,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var bossEnemy = owner.CreateEnemy( EnemyShapeTypes.No_031, 2, shield, 6000, groupId, OnBossEnemyUpdate, OnBossEnemyDestroyed, bossSettings, level ) ;

					// ボス用のコリジョン設定
					bossEnemy.SetCollisionType( EnemyCollisionTypes.Boss ) ;

					// 爆発演出補正
					bossEnemy.ExplosionScale = 3.0f ;
					bossEnemy.ExplosionTimes = 10 ;

					xs += distance ;
				}

				// 少し待つ
				await Wait( 0 ) ;

				//---------------------------------------------------------

				// 処理終了
				IsProcerssing = false ;
			}

			/// <summary>
			/// 任意データ(個別に設定したい場合は個体毎に new が必要)
			/// </summary>
			public class BossSettings
			{
				public Vector2	StartRatioPosition ;
				public int		AttackType ;
				public int		UnitAmount ;
				public bool		IsReflectorBullet ;

				/// <summary>
				/// 従属エネミー(ザコ)
				/// </summary>
				public List<Enemy>	UnitEnemies = new () ;
			}


			// エネミーの動作を処理する
			private async Task OnBossEnemyUpdate( Enemy bossEnemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var bossSettings = bossEnemy.Settings as BossSettings ;

				// 初期位置
				var startRatioPosition	= bossSettings.StartRatioPosition ;

				// 初期位置を設定する
				bossEnemy.RatioPosition = startRatioPosition ;

				// 攻撃開始位置
				var attackRatioPosition	= new Vector2( startRatioPosition.X, -0.2f ) ;

				//---------------------------------

				// 初期方向
				Vector2 velocity = attackRatioPosition - startRatioPosition ;

				// 初期方向
				bossEnemy.SetAngle( velocity ) ;

				//---------------------------------

				int phase = 0 ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				Vector2 baseRatioPosition = Vector2.Zero ;
				Vector2 moveRatioPosition = Vector2.Zero ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = bossEnemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 出現

						duration = 1.6f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						velocity = attackRatioPosition - startRatioPosition ;

						bossEnemy.RatioPosition = velocity * factor + startRatioPosition ;

						if( factor >= 1 )
						{
							// ファンネル大量生成へ
							phase = 1 ;
							time = 0 ;
						}
					}
					else
					if( phase == 1 )
					{
						// ファンネル大量生成

						// 少し待つ
						duration = 0.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// アルファフェードで隠れるへ
							phase = 10 ;
							time = 0 ;

							// コリジョン無効
							bossEnemy.SetCollisionEnabled( false ) ;

							//------------------------------

							// ファンネル大量生成
							CreateUnitEnemies( bossEnemy, bossSettings.UnitAmount ) ;
						}
					}
					else
					if( phase == 10 )
					{
						// アルファフェードで隠れる
						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						bossEnemy.Alpha = 1 - factor ;

						if( factor >= 1 )
						{
							// 一定時間待機へ
							phase = 11 ;
							time = 0 ;
						}
					}
					else
					if( phase == 11 )
					{
						// 一定時間待機
						duration = 5.0f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						// ザコが全滅していなければ現れて攻撃してまた消える
						// ザコが全滅していれば本気モードへ

						if( factor >= 1 )
						{
							if( bossSettings.UnitEnemies.Count >  0 )
							{
								// ユニットエネミーがまだ残っている

								// 通常モードのアルファフェードで現れるへ
								phase = 12 ;
								time = 0 ;

								// 位置をランダム設定

								float xn = ExMath.GetRandomRange( -0.35f, +0.35f ) ;
								float yn = ExMath.GetRandomRange( -0.35f, +0.35f ) ;

								bossEnemy.RatioPosition = new Vector2( xn, yn ) ;
							}
							else
							{
								// ユニットエネミーが全滅している

								// 本気モードのアルファフェードで現れるへ
								phase = 20 ;
								time = 0 ;

								// 位置は最初の攻撃位置
								bossEnemy.RatioPosition = attackRatioPosition ;
							}
						}
					}
					else
					if( phase == 12 )
					{
						// アルファフェードで現れる
						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						bossEnemy.Alpha = factor ;

						if( factor >= 1 )
						{
							// 攻撃実行へ
							phase = 13 ;
							time = 0 ;

							// コリジョン有効
							bossEnemy.SetCollisionEnabled( true ) ;
						}
					}
					else
					if( phase == 13 )
					{
						// 攻撃実行

						// 少し待つ
						duration = 0.1f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 少し待機へ
							phase = 14 ;
							time = 0 ;

							// 攻撃実行

							m_Owner.CreateEnemyBulletMulti
							(
								EnemyBulletShapeTypes.LaserTiny, bossEnemy.Position, 400.0f, 2, 0,
								3, 90.0f,
								0.01f, 10.0f, 0.5f,
								correction:16
							) ;
						}
					}
					else
					if( phase == 14 )
					{
						// 少し待機
						duration = 1f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// アルファフィードへ隠れるへ
							phase = 10 ;
							time = 0 ;

							// コリジョン無効
							bossEnemy.SetCollisionEnabled( false ) ;
						}
					}
					else
					if( phase == 20 )
					{
						// 本気モード : アルファフェードで現れる
						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						bossEnemy.Alpha = factor ;

						if( factor >= 1 )
						{
							// 最初の移動実行へ
							phase = 21 ;
							time = 0 ;

							// コリジョン有効
							bossEnemy.SetCollisionEnabled( true ) ;
						}
					}
					else
					if( phase == 21 )
					{
						// 最初の移動先決定

						duration = 0.2f ;

						// 少し待つ
						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;


						if( factor >= 1 )
						{
							// 移動へ
							phase = 22 ;
							time = 0 ;


							//----------

							baseRatioPosition = bossEnemy.RatioPosition ;

							float xn, yn ;

							if( bossEnemy.RatioPosition.X == 0 )
							{
								// １体の場合は画面の中心にいるのでどちらに移動するかはランダム
								if( ExMath.GetRandomRange(  0, 99 ) <  50 )
								{
									// 左へ移動
									xn = -0.35f ;
								}
								else
								{
									// 右へ移動
									xn = +0.35f ;
								}
							}
							else
							{
								// 近い方
								xn = 0.35f * ExMath.Sign( bossEnemy.RatioPosition.X ) ;
							}

							yn = -0.35f ;

							// 移動先
							moveRatioPosition = new Vector2( xn, yn ) ;
						}
					}
					else
					if( phase == 22 )
					{
						// 移動

						duration = 0.8f ;

						// 少し待つ
						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.EaseOutQuad ) ;

						velocity = moveRatioPosition - baseRatioPosition ;

						bossEnemy.RatioPosition = velocity * factor + baseRatioPosition ;

						if( factor >= 1 )
						{
							// 攻撃へ

							phase = 23 ;
							time = 0 ;
						}
					}
					else
					if( phase == 23 )
					{
						// 攻撃
						duration = 0.2f ;

						// 少し待つ
						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 次の移動先設定へ

							phase = 24 ;
							time = 0 ;

							//----------

							// 攻撃
							m_Owner.CreateEnemyBulletMulti
							(
								EnemyBulletShapeTypes.LaserSlim, bossEnemy.Position, 400.0f, 4, 0,
								3, 90.0f,
								0.01f, 10.0f, 0.5f,
								correction:16
							) ;
						}
					}
					else
					if( phase == 24 )
					{
						// 次の移動先設定

						duration = 0.2f ;

						// 少し待つ
						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 移動へ
							phase = 22 ;
							time = 0 ;

							//----------

							baseRatioPosition = moveRatioPosition ;

							if( baseRatioPosition.Y <  0 )
							{
								// 前に移動する(Ｘ座標は変化しない)

								moveRatioPosition.X =   baseRatioPosition.X ;
								moveRatioPosition.Y = - baseRatioPosition.Y ;
							}
							else
							{
								// 後に移動する(Ｘ座標が変化する)
								moveRatioPosition.X = - baseRatioPosition.X ;
								moveRatioPosition.Y = - baseRatioPosition.Y ;
							}
						}
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( bossEnemy ) == true )
					{
						// 保険
						break ;
					}
				}

				// このエネミーは画面外に出たので破棄して良い
				bossEnemy.OutOfScreen() ;
			}

			// エネミーが破壊される際に呼び出される
			private bool OnBossEnemyDestroyed( Enemy bossEnemy, EnemyDestroyedReasonTypes destroyedReasonType )
			{
				if( destroyedReasonType == EnemyDestroyedReasonTypes.PlayerShot )
				{
					// 設定情報を取り出す
					var bossSettings = bossEnemy.Settings as BossSettings ;

					// ボスに返し弾は無し
#if false
					if( bossSettings.IsReflectorBullet == true )
					{
						int avarage = bossEnemy.Level - 60 ;
						if( avarage <  0 )
						{
							avarage  = 1 ;
						}

						if( ExMath.GetRandomRange(  0, 99 ) <  avarage )
						{
							// 返し弾発射
							m_Owner?.CreateEnemyBullet( 0, bossEnemy.Position, 200.0f, 1 ) ;
						}
					}
#endif

					//--------------------------------

					if( bossSettings.UnitEnemies.Count >  0 )
					{
						if( bossSettings.UnitEnemies.Count == bossSettings.UnitAmount )
						{
							// ユニットを１体も破壊せずにボスを倒したのでボーナス
							int bonusScore = 30000 ;
							m_Owner.AddScore( bonusScore ) ;
							m_Owner._HUD.ShowBonus( bonusScore ) ;
						}

						//-------------------------------

						// ユニットエネミーを全て破壊する

						foreach( var unitEnemy in bossSettings.UnitEnemies )
						{
							// 別のノードの強制破棄は出来ないので破棄リクエストを出して自身で破棄させる必要がある
							unitEnemy.RequestDestroy() ;
						}
					}
				}

				// 実際に破壊してよい
				return true ;
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// 任意データ(個別に設定したい場合は個体毎に new が必要)
			/// </summary>
			public class UnitSettings
			{
				public Vector2	StartRatioPosition ;
				public Vector2	AttackRatioPosition ;
				public int		AttackType ;
				public bool		IsReflectorBullet ;
			}

			// 従属エネミー群を生成する
			private void CreateUnitEnemies( Enemy bossEnemy, int unitAmount )
			{
				// ボスの現在位置
				var bossRatioPosition = bossEnemy.RatioPosition ;

				// ボスの設定(インスタンスと同義)
				var bossSettings = bossEnemy.Settings as BossSettings ;

				//---------------------------------

				int groupId = -1 ;	// グループ全滅に関係しないエネミーグループ

				int level = bossEnemy.Level ;

				// レベルによるシールド値の補正
				int shield = GetShield(  10,  20, level ) ;

				//---------------------------------

				int i, l = unitAmount ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// 開始位置の周囲ランダムに最初の攻撃位置を設定する
					float xa = ExMath.GetRandomRange( -0.4f, +0.4f ) + bossRatioPosition.X ;
					if( xa <  -0.45f )
					{
						xa  = -0.45f ;
					}
					else
					if( xa >  +0.45f )
					{
						xa  = +0.45f ;
					}

					float ya = ExMath.GetRandomRange( -0.2f, +0.2f ) + bossRatioPosition.Y ;
					if( ya <  -0.45f )
					{
						ya  = -0.45f ;
					}
					else
					if( ya >  +0.2f )
					{
						ya  = +0.2f ;
					}

					var unitSettings = new UnitSettings()
					{
						StartRatioPosition	= bossRatioPosition,
						AttackRatioPosition	= new Vector2( xa, ya )
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var unitEnemy = m_Owner.CreateEnemy( EnemyShapeTypes.No_032, 2, shield, 100, groupId, OnUnitEnemyUpdate, OnUnitEnemyDestroyed, unitSettings, level ) ;

					unitEnemy.SetCollisionType( EnemyCollisionTypes.Unit ) ;

					// ボスの設定を保持させる
					unitEnemy.SetBossSettings( bossSettings ) ;

					// ボスの設定にユニットエネミーの参照を保持させる
					bossSettings.UnitEnemies.Add( unitEnemy ) ;
				}
			}

			// エネミーの動作を処理する
			private async Task OnUnitEnemyUpdate( Enemy unitEnemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var unitSettings = unitEnemy.Settings as UnitSettings ;

				// 初期位置
				var startRatioPosition	= unitSettings.StartRatioPosition ;

				// 初期位置を設定する
				unitEnemy.RatioPosition = startRatioPosition ;

				// 最初の攻撃開始位置
				var attackRatioPosition	= unitSettings.AttackRatioPosition ;

				//---------------------------------

				// 初期方向
				Vector2 velocity ;

				// 初期方向
				unitEnemy.SetAngle( GetPlayerDirection( unitEnemy ) ) ;

				//---------------------------------
				// 移動量と画面外判定情報

				//---------------------------------

				int phase = 0 ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = unitEnemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 攻撃位置へ移動する

						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.EaseOutQuad ) ;

						velocity = attackRatioPosition - startRatioPosition ;

						unitEnemy.RatioPosition = velocity * factor + startRatioPosition ;

						if( m_Owner.IsPlayerDestroyed == false )
						{
							// 常にプレイヤーの方向を向いて移動する
							unitEnemy.SetAngle( GetPlayerDirection( unitEnemy ) ) ;
						}

						if( factor >= 1 )
						{
							// 攻撃へ
							phase = 1 ;
							time = 0 ;
						}
					}
					else
					if( phase == 1 )
					{
						// 攻撃

						// 少し待つ
						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						// 常にプレイヤーの方向を向く
						unitEnemy.SetAngle( GetPlayerDirection( unitEnemy ) ) ;

						if( factor >= 1 )
						{
							// 移動へ
							phase = 2 ;
							time = 0 ;

							if( m_Owner.IsPlayerDestroyed == false )
							{
								// プレイヤーの方向に攻撃する
								m_Owner.CreateEnemyBullet
								(
									EnemyBulletShapeTypes.LaserSlim, unitEnemy.Position, 600.0f, 4, correction:16
								) ;
							}
						}
					}
					else
					if( phase == 2 )
					{
						// 移動場所決定へ

						// 少し待つ
						duration = 0.3f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 移動へ
							phase = 0 ;
							time = 0 ;

							startRatioPosition = attackRatioPosition ;

							// 移動先はランダム

							// 開始位置の周囲ランダムに最初の攻撃位置を設定する
							float xa = ExMath.GetRandomRange( -0.4f, +0.4f ) + startRatioPosition.X ;
							if( xa <  -0.45f )
							{
								xa  = -0.45f ;
							}
							else
							if( xa >  +0.45f )
							{
								xa  = +0.45f ;
							}

							float ya = ExMath.GetRandomRange( -0.2f, +0.2f ) + startRatioPosition.Y ;
							if( ya <  -0.45f )
							{
								ya  = -0.45f ;
							}
							else
							if( ya >  +0.2f )
							{
								ya  = +0.2f ;
							}

							attackRatioPosition = new Vector2( xa, ya ) ;
						}
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( unitEnemy ) == true )
					{
						// 保険
						break ;
					}

					//--------------------------------

//					if( m_Owner.IsPlayerDestroyed == true )
//					{
//						// 自己破壊
//						unitEnemy.SelfDestroy() ;
//						return ;
//					}
				}

				// このエネミーは画面外に出たので破棄して良い
				unitEnemy.OutOfScreen() ;
			}

			// エネミーが破壊された際に呼び出される
			private bool OnUnitEnemyDestroyed( Enemy unitEnemy, EnemyDestroyedReasonTypes destroyedReasonType )
			{
				if( destroyedReasonType == EnemyDestroyedReasonTypes.PlayerShot )
				{
					// 設定情報を取り出す
					var unitSettings = unitEnemy.Settings as UnitSettings ;

					if( unitSettings.IsReflectorBullet == true )
					{
						int avarage = unitEnemy.Level - 60 ;
						if( avarage <  0 )
						{
							avarage  = 1 ;
						}

						if( ExMath.GetRandomRange(  0, 99 ) <  avarage )
						{
							// 返し弾発射
							m_Owner?.CreateEnemyBullet( 0, unitEnemy.Position, 200.0f, 1 ) ;
						}
					}
				}
	
				//---------------------------------------------------------

				// ボスのユニット管理から参照を削除する
				var bossSettings = unitEnemy.BossSettings as BossSettings ;

				if( bossSettings.UnitEnemies.Contains( unitEnemy ) == true )
				{
					bossSettings.UnitEnemies.Remove( unitEnemy ) ;
				}

				//---------------------------------

				if( destroyedReasonType == EnemyDestroyedReasonTypes.Self )
				{
					// ボス情報から自身の参照を削除する
					// ただしセルフ以外(セルフはボスによって破壊された)

					//--------------------------------
					// セルフの場合は爆発エフェクトが出ないので手動で出す

					if( unitEnemy.ExplosionScale >  0 )
					{
						// 爆発を生成する
						m_Owner.CreateExplosionMulti( unitEnemy.Position, unitEnemy.ExplosionTimes, unitEnemy.ExplosionScale ) ;
					}
				}

				//---------------------------------

				// 実際に破壊してよい
				return true ;
			}
		}
	}
}
