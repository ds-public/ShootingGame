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
		/// エネミーグループ(種別 033)
		/// </summary>
		public class EnemyGroup_033 : EnemyGroupBase
		{
			/// <summary>
			/// ボーナスアイテムボス
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
					( level / 25 ),		// ２体
//					( level / 25 )		// ３体
				} ;

				int bossAmount = 1 + ExMath.GetRandomIndex( bossAmount_weights ) ;
//				bossAmount = 3 ;	// デバッグ

				//---------------------------------------------------------

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
				int shield = GetShield( 100, 200, level ) ;
				// shield = 1 ;	// デバッグ

				float distance = 0.36f ;

				float xs = - ( l * distance * 0.5f ) + ( distance * 0.5f ) ;
				float ys = -0.65f ;
				
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(共通)
					var bossSettings = new BossSettings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var bossEnemy = owner.CreateEnemy( EnemyShapeTypes.No_033, 2, shield, 5000, groupId, OnBossEnemyUpdate, OnBossEnemyDestroyed, bossSettings, level ) ;

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

				Vector2 sideDirection ;

				float sx = startRatioPosition .X ;
				if( sx == 0 )
				{
					sx = ExMath.GetRandomRange( -1, +1 ) ;
				}

				if( sx <  0 )
				{
					// 右へ
					sideDirection = new Vector2( +1,  0 ) ;
				}
				else
				{
					// 左へ
					sideDirection = new Vector2( -1,  0 ) ;
				}

				// 攻撃タイプ
				var attackType = bossSettings.AttackType ;

				// ホッピング方向
				var aheadDirection = new Vector2(  0, +1 ) ;

				//---------------------------------


				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				float attackInterval = 0.0f ;
				float attackDuration = 2.0f ;

				float inviteInterval = 0.0f ;
				float inviteDuration = 1.0f ;


				// ホッピング前の位置
				Vector2 baseRatioPosition = startRatioPosition ;
				Vector2 moveRatioPosition = Vector2.Zero ;


				float xh, yh ;

				while( true )	// 画面内の座標割合値で位置を判定する
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
							// ホッピング攻撃へ
							phase = 1 ;
							time = 0 ;

							baseRatioPosition = bossEnemy.RatioPosition ;
						}
					}
					else
					if( phase == 1 )
					{
						// ポッピング中

						duration = 1f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						xh = factor ;	// 0→1
						yh = ( Mathf.Sin( Mathf.Pi * ( factor * 1.25f ) ) ) ;	// 0→1→?

						moveRatioPosition = baseRatioPosition
							+ ( 0.24f * sideDirection  * xh )
							- ( 0.12f * aheadDirection * yh ) ;

						bossEnemy.RatioPosition = moveRatioPosition ;

						//-------------------------------------------------------
						// 攻撃

						attackInterval += delta ;

						if( attackInterval >= attackDuration )
						{
							attackInterval  = 0 ;

							if( m_Owner.IsPlayerDestroyed == false )
							{
								m_Owner.CreateEnemyBulletMulti
								(
									EnemyBulletShapeTypes.LaserTiny, bossEnemy.Position, 400.0f, 2, 0,
									7 + attackType * 2, 30.0f
								) ;
							}
						}

						//-------------------------------------------------------
						// 召喚

						inviteInterval += delta ;

						if( inviteInterval >= inviteDuration )
						{
							inviteInterval  = 0 ;

							if( m_Owner.IsPlayerDestroyed == false )
							{
								CreateUnitEnemy( bossEnemy ) ;
							}
						}

						//-------------------------------------------------------

						if( factor >= 1 )
						{
							baseRatioPosition = moveRatioPosition ;

							bool isSideReturn = false ;

							if( sideDirection.X <  0 )
							{
								// 左へホッピング中

								if( bossEnemy.RatioPosition.X <  -0.25f )
								{
									isSideReturn = true ;
								}
							}
							else
							{
								// 右へホッピング中
								if( bossEnemy.RatioPosition.X >  +0.25f )
								{
									isSideReturn = true ;
								}
							}

							if( isSideReturn == true )
							{
								// 逆方向にホッピングする
								sideDirection = - sideDirection ;
							}

							if( bossEnemy.RatioPosition.Y <  +0.25f )
							{
								// まだホッピグ
								phase = 1 ;
								time = 0 ;
							}
							else
							{
								// 上に上がる
								phase = 2 ;
								time = 0 ;

								moveRatioPosition.X = baseRatioPosition.X ;
								moveRatioPosition.Y = -0.3f  ;
							}
						}
					}
					else
					if( phase == 2 )
					{
						duration = 2f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						// 上に上がる

						velocity = moveRatioPosition - baseRatioPosition ;

						bossEnemy.RatioPosition = velocity * factor + baseRatioPosition ;

						if( factor >= 1 )
						{
							// ホッピングへ
							phase = 1 ;
							time = 0 ;

							baseRatioPosition = bossEnemy.RatioPosition ;

							if( bossEnemy.RatioPosition.X <  0 )
							{
								sideDirection = new Vector2( +1,  0 ) ;
							}
							else
							{
								sideDirection = new Vector2( -1,  0 ) ;
							}
						}
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( bossEnemy, 0.8f ) == true )
					{
						// 保険
						break ;
					}
				}

				// このエネミーは画面外に出たので破棄して良い
				bossEnemy.OutOfScreen() ;
			}

			// エネミーが破壊された際に呼び出される
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

					if( ExMath.GetRandomRange(  0, 99 ) <  70 )
					{
						// 報酬
						m_Owner.CreateItems( m_Owner.PickupItems( 10 ), bossEnemy.Position ) ;
					}
					else
					{
						// 攻撃
						m_Owner.CreateEnemyBulletMulti( EnemyBulletShapeTypes.BulletLarge, bossEnemy.Position, 400.0f,
							3, 0,
							16, 22.5f,
							correction: 16
						) ;
					}

					//--------------------------------

					if( bossSettings.UnitEnemies.Count >  0 )
					{
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
			private void CreateUnitEnemy( Enemy bossEnemy )
			{
				// ボスの現在位置
				var bossRatioPosition = bossEnemy.RatioPosition ;

				// ボスの設定(インスタンスと同義)
				var bossSettings = bossEnemy.Settings as BossSettings ;

				//---------------------------------

				int groupId = -1 ;	// グループ全滅に関係しないエネミーグループ

				int level = bossEnemy.Level ;

				// レベルによるシールド値の補正
				int shield = GetShield(  1,  2, level ) ;

				//---------------------------------

				// 開始位置の周囲ランダムに最初の攻撃位置を設定する
				float xa = ExMath.GetRandomRange( -0.2f, +0.2f ) + bossRatioPosition.X ;
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
				if( ya >  +0.45f )
				{
					ya  = +0.45f ;
				}

				var unitSettings = new UnitSettings()
				{
					StartRatioPosition	= bossRatioPosition,
					AttackRatioPosition	= new Vector2( xa, ya )
				} ;

				// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
				var unitEnemy = m_Owner.CreateEnemy( EnemyShapeTypes.No_034, 2, shield, 100, groupId, OnUnitEnemyUpdate, OnUnitEnemyDestroyed, unitSettings, level ) ;

				unitEnemy.SetCollisionType( EnemyCollisionTypes.Unit ) ;

				// ボスの設定を保持させる
				unitEnemy.SetBossSettings( bossSettings ) ;

				// ボスの設定にユニットエネミーの参照を保持させる
				bossSettings.UnitEnemies.Add( unitEnemy ) ;
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

				//---------------------------------

				// 初期方向(常に下向き)
				unitEnemy.SetAngle( new Vector2(  0, +1 ) ) ;

				// 初期状態ではコリジョン無効
				unitEnemy.SetCollisionEnabled( false ) ;

				//---------------------------------

				int phase = 0 ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				float factorA ;
				float factorS ;

				Vector2 attackDirection = Vector2.Zero ;
				float speed = 400.0f ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = unitEnemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						duration = 0.75f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;
						factorA = Ease.GetValue( factor, EaseTypes.Linear ) ;
						factorS = Ease.GetValue( factor, EaseTypes.EaseInOutBack ) ;

						unitEnemy.Alpha = factorA ;
						unitEnemy.SetScale( factorS ) ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;

							// コリジョン有効
							unitEnemy.SetCollisionEnabled( true ) ; 

							// プレイヤーに向けて突っ込む
							if( m_Owner.IsPlayerDestroyed == false )
							{
								attackDirection = GetPlayerDirection( unitEnemy ) ;
							}
							else
							{
								// 画面下
								attackDirection = new Vector2(  0, +1 ) ;
							}
						}
					}
					else
					if( phase == 1 )
					{
						duration = 0.5f ;	// ０．２秒で最高速に到達

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						unitEnemy.Position += ( attackDirection * speed * factor * delta ) ;
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

					// 返し弾は無し
#if false
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
#endif
					// 確率で報酬か攻撃

					if( ExMath.GetRandomRange(  0, 99 ) <  30 )
					{
						// 報酬
						m_Owner.CreateItem( m_Owner.PickupItem(), unitEnemy.Position ) ;
					}
					else
					{
						// 攻撃
						var item = m_Owner.CreateItem( m_Owner.PickupItem(), unitEnemy.Position, 180.0f ) ;
						item.SetFake( true ) ;
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
