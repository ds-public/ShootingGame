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
		/// エネミーグループ(種別 018)
		/// </summary>
		public class EnemyGroup_018 : EnemyGroupBase
		{
			/// <summary>
			/// 画面中央で７方向に弾を１０回発射する
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

				int[] attack_weights =
				{
					100,			// ８方向
					level,			// １２方向
					level / 2,		// １６方向
				} ;
				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 1 ;

				int[] weapon_weights =
				{
					100,			// 大きい弾
					 ( level / 4 ),	// 太いレーザー
				} ;

				int weaponType = ExMath.GetRandomIndex( weapon_weights ) ;
//				weaponType = 1 ;	// デバッグ

				int[] amount_weights =
				{
					100,			// １体
					 ( level / 4 ),	// ２体
				} ;

				int amount = 1 + ExMath.GetRandomIndex( amount_weights ) ;
//				amount = 2 ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
//				if( level >= 60 )
//				{
//					isReflectorBullet  = true ;
//				}

				//---------------------------------

				int i, l = amount ;

				// 出現数が確定した時点でカウンターを更新する
				var enemyGroupCounter = new EnemyGroupCounter( this)
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = l
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield( 24, 48, level ) ;

				float distance = 0.4f ;
				float xs = ( distance * l ) * -0.5f + ( distance * 0.5f ) ;
				float ys = -0.1f ;


				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成
					var settings = new Settings()
					{
						AttackType			= attackType,
						WeaponType			= weaponType,
						StartRatioPosition	= new Vector2( xs, ys ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var enemy = owner.CreateEnemy( EnemyShapeTypes.No_018, 2, shield, 1000, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 爆発演出補正
					enemy.ExplosionScale = 1.5f ;
					enemy.ExplosionTimes = 5 ;

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
			public class Settings
			{
				public int		AttackType ;
				public int		WeaponType ;
				public Vector2	StartRatioPosition ;
				public Vector2	EndRatioPosition ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// 攻撃方向
				var attackType = settings.AttackType ;

				// 武器
				var weaponType = settings.WeaponType ;

				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 非表示＆無敵
				enemy.Alpha = 0 ;
				enemy.SetCollisionEnabled( false ) ;

				//---------------------------------
				// 移動量と画面外判定情報

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				int attackCount = 0 ;
				var attackTimer = new SimpleTimer() ;
				float attackDurection = 0.5f ;
				var attackDirection = new Vector2(  0, +1 ) ;
				float attackAngleSign ;

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					attackAngleSign = +1 ;
				}
				else
				{
					attackAngleSign = -1 ;
				}


				// 初期の回転角度を設定
				enemy.SetAngle( attackDirection ) ;




				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 出現

						duration = 0.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						enemy.Alpha = factor ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;

							// コリジョン有効化
							enemy.SetCollisionEnabled( true ) ;
						}
					}
					else
					if( phase == 1 )
					{
						// 何もしないで少し待つ
						duration = 0.2f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							phase = 2 ;
							time = 0 ;

							attackCount = 0 ;
							attackTimer.Reset() ;
						}
					}
					else
					if( phase == 2 )
					{
						// 攻撃フェーズ

						duration = 10.0f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						float scale = 1.0f + ( float )( Mathf.Sin( Mathf.Pi * factor * 8 ) * 0.8 ) ;

						enemy.Scale = new Vector2( scale , scale ) ;

						//-------------------------------

						int way = 8 + attackType * 4 ;

						if( attackCount == 0 || attackTimer.IsFinished( attackDurection ) == true )
						{
							// ８～１６方向攻撃

							int i, l = way ;

							float angle ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								angle = 360.0f * i / l ;
								var weaponDirection = ExMath.GetRotatedVector( attackDirection, angle ) ;

								if( i != 0 )
								{
									if( weaponType == 0 )
									{
										// 小さい弾
										m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.BulletSmall, enemy.Position, weaponDirection, 200.0f, 1 ) ;
									}
									else
									{
										// 細いレーザー
										m_Owner.CreateEnemyBullet
										(
											EnemyBulletShapeTypes.LaserSlim, enemy.Position, weaponDirection, 400.0f, 4,
											correction: 48
										) ;
									}
								}
							}

							// 方向調整
							angle = ( 360.0f / ( 4 * way ) ) * attackAngleSign ;
							attackDirection = ExMath.GetRotatedVector( attackDirection, angle ) ;

							attackCount ++ ;
							attackTimer.Reset() ;
						}

						if( factor >= 1 )
						{
							// 退却へ
							phase = 3 ;
							time = 0 ;
						}
					}
					else
					if( phase == 3 )
					{
						// 何もしないで少し待つ
						duration = 0.3f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							phase = 4 ;
							time = 0 ;

							// コリジョン無効化
							enemy.SetCollisionEnabled( false ) ;
						}
					}
					else
					if( phase == 4 )
					{
						// 退却
						duration = 0.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						enemy.Alpha = 1 - factor ;

						if( factor >= 1 )
						{
							break ;
						}
					}
					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( enemy ) == true )
					{
						// 保険
						break ;
					}
				}

				// このエネミーは画面外に出たので破棄して良い
				enemy.OutOfScreen() ;
			}

			// エネミーが破壊された際に呼び出される
			private bool OnEnemyDestroyed( Enemy enemy, EnemyDestroyedReasonTypes destroyedReasonType )
			{
				if( destroyedReasonType == EnemyDestroyedReasonTypes.PlayerShot )
				{
					// 設定情報を取り出す
					var settings = enemy.Settings as Settings ;

					if( settings.IsReflectorBullet == true )
					{
						int avarage = enemy.Level - 60 ;
						if( avarage <  0 )
						{
							avarage  = 1 ;
						}

						if( ExMath.GetRandomRange(  0, 99 ) <  avarage )
						{
							// 返し弾発射
							m_Owner?.CreateEnemyBullet( 0, enemy.Position, 200.0f, 1 ) ;
						}
					}
				}

				// 実際に破壊してよい
				return true ;
			}
		}
	}
}
