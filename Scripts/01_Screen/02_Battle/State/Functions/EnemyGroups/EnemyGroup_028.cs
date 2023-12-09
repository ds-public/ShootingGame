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
		/// エネミーグループ(種別 028)
		/// </summary>
		public class EnemyGroup_028 : EnemyGroupBase
		{
			/// <summary>
			/// 画面の左右に常駐する移動砲台
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

				int[] amount_weights =
				{
					50,				// １体
					level / 2,		// ２体
					level / 4,		// ３体
					level / 8,		// ４体
				} ;
				int amount = 1 + ExMath.GetRandomIndex( amount_weights ) ;
//				amount = 3 ;	// デバッグ

				int[] indices = new int[]{ 0, 1, 2, 3 } ;

				int i, l = indices.Length, r ;
				int swap ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					r = ExMath.GetRandomRange( 0, 3 ) ;
					swap = indices[ r ] ;
					indices[ r ] = indices[ i ] ;
					indices[ i ] = swap ;
				}

				int[] attack_weights =
				{
					50,			// １方向
					level,		// ３方向
				} ;
				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 1 ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
				if( level >= 60 )
				{
					isReflectorBullet  = true ;
				}

				//---------------------------------

				l= amount ;

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
				int shield = GetShield( 15, 30, level ) ;

				int p ;
				float xs, ys, xa ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(共通)

					p = indices[ i ] ;

					if( ( p & 1 ) == 0 )
					{
						// 左
						xs = -0.6f ;
						xa = -0.36f ;
					}
					else
					{
						// 右
						xs = +0.6f ;
						xa = +0.36f ;
					}

					if( ( p & 2 ) == 0 )
					{
						// 上
						ys = ExMath.GetRandomRange( -0.43f, -0.1f ) ;
					}
					else
					{
						// 下
						ys = ExMath.GetRandomRange( +0.43f, +0.1f ) ;
					}

					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						AttackRatioPosition	= new Vector2( xa, ys ),
						AttackType			= attackType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var enemy = owner.CreateEnemy( EnemyShapeTypes.No_028, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 爆発演出補正
					enemy.ExplosionScale = 1.5f ;
					enemy.ExplosionTimes = 5 ;

					// 少し待つ
					await Wait( 0.5f ) ;
				}

				//---------------------------------------------------------

				// 処理終了
				IsProcerssing = false ;
			}

			/// <summary>
			/// 任意データ(個別に設定したい場合は個体毎に new が必要)
			/// </summary>
			public class Settings
			{
				public Vector2	StartRatioPosition ;
				public Vector2	AttackRatioPosition ;
				public int		AttackType ;
				public bool		IsReflectorBullet ;
			}

			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// 初期位置
				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 攻撃位置
				var attackRatioPosition = settings.AttackRatioPosition ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

				var attackType = settings.AttackType ;

				// 表示アニメーション選別
				if( attackType == 0 )
				{
					enemy.PlayAnimation( "way_1" ) ;
				}
				else
				{
					enemy.PlayAnimation( "way_3" ) ;
				}

				//---------------------------------

				Vector2 attackDirection ;

				if( startRatioPosition.X <  0 )
				{
					// 左側
					attackDirection = new Vector2( +1,  0 ) ;
				}
				else
				{
					// 右側
					attackDirection = new Vector2( -1,  0 ) ;
				}

				// 初期の回転角度を設定
				enemy.SetAngle( attackDirection ) ;


				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;


				Vector2 velocity ;

				Vector2 targetDirection = Vector2.Zero ;

				float rotationAngle = 0 ;
				int attackCount = 0 ;
				int movingCount = 0 ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 出現
						duration = 2f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						velocity = attackRatioPosition - startRatioPosition ;

						enemy.RatioPosition = velocity * factor + startRatioPosition ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;
						}
					}
					else
					if( phase == 1 )
					{
						// 少し待機から方向調整
						duration = 0.2f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							if( m_Owner.IsPlayerDestroyed == false )
							{
								// プレイヤーの方向へ
								targetDirection = ( m_Owner.Player.Position - enemy.Position ).Normalized() ;

								rotationAngle = ExMath.GetRativeAngle( attackDirection, targetDirection, isDegree:true ) ;

								phase = 2 ;
								time = 0 ;
							}
							else
							{
								// 退却へ
								phase = 6 ;
								time = 0 ;
							}
						}
					}
					else
					if( phase == 2 )
					{
						// プレイヤーの方向を向く

						duration = 1.0f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						direction = ExMath.GetRotatedVector( attackDirection, rotationAngle * factor ) ;

						// 初期の回転角度を設定
						enemy.SetAngle( direction ) ;

						if( factor >= 1 )
						{
							// 攻撃する
							phase = 3 ;
							time = 0 ;

							attackCount = 0 ;

							// 方向更新
							attackDirection = targetDirection ;
						}
					}
					else
					if( phase == 3 )
					{
						// 攻撃する

						if( attackType == 0 )
						{
							// １方向
							m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.LaserSlim, enemy.Position, attackDirection, 400.0f, 4, correction:  64 ) ;
						}
						else
						{
							// ３方向
							m_Owner.CreateEnemyBulletMulti
							(
								EnemyBulletShapeTypes.LaserSlim, enemy.Position, attackDirection, 400.0f, 4, 0,
								3, 45.0f,
								correction: 64
							) ;
						}

						attackCount ++ ;

						if( attackCount <  3 )
						{
							// 攻撃間隔調整へ
							phase = 4 ;
							time = 0 ;
						}
						else
						{
							// 一旦休息へ
							phase = 5 ;
							time = 0 ;
						}
					}
					else
					if( phase == 4 )
					{
						// 攻撃間隔調整

						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 次の攻撃へ
							phase = 3 ;
							time = 0 ;
						}
					}
					else
					if( phase == 5 )
					{
						// 一旦休息

						duration = 1.0f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							movingCount ++ ;

							if( movingCount <  3 )
							{
								// また方向確認へ
								phase = 1 ;
								time = 0 ;

							}
							else
							{
								// 退却へ
								phase = 6 ;
								time = 0 ;
							}
						}
					}
					else
					{
						duration = 2f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						velocity = startRatioPosition - attackRatioPosition ;

						enemy.RatioPosition = velocity * factor + attackRatioPosition ;

						if( factor >= 1 )
						{
							// 終了
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
