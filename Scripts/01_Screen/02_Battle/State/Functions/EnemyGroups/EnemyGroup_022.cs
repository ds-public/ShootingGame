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
		/// エネミーグループ(種別 022)
		/// </summary>
		public class EnemyGroup_022 : EnemyGroupBase
		{
			/// <summary>
			/// [ボス]左右に無限大の軌道でレーザー攻撃をしてくる
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

				// 出現数
				int[] amount_weights =
				{
					  100,	//  1
					level,	//  2
				} ;
				int amount = ExMath.GetRandomIndex( amount_weights ) + 1 ;
//				amount = 2 ;		// デバッグ

				int[] bullet_weights =
				{
					   70,				//  弾
					   30 + level,		//  レーザー
				} ;
				int bulletType = ExMath.GetRandomIndex( bullet_weights ) ;
//				bulletType = 1 ;	// デバッグ

				int[] bulletAmount_weights =
				{
					   100,				//  ３方向
					   level * 2,		//  ５方向
					   level,			//  ５方向
				} ;
				int bulletAmount = ExMath.GetRandomIndex( bulletAmount_weights ) * 2 + 3 ;
//				bulletAmount = 7 ;

				int[] aiming_weights =
				{
					   70,		//  真っ直ぐ発射
					   30,		//  プレイヤーの方向を狙う
				} ;
				int aimingType = ExMath.GetRandomIndex( aiming_weights ) ;
//				aimingType = 1 ;	// デバッグ


				int[] amplitude_weights =
				{
					   60,		//  直線
					   40,		//  無限大
				} ;
				int amplitudeType = ExMath.GetRandomIndex( amplitude_weights ) ;
//				amplitudeType = 0 ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
//				if( level >= 50 )
//				{
//					isReflectorBullet  = true ;
//				}

				//---------------------------------

				int i, l = amount ;

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
				int shield = GetShield(  60, 120, level ) ;
//				shield = 3 ;	// デバッグ

				float distance = 0.36f ;

				float xr = ( - ( distance * amount * 0.5f ) ) + ( distance * 0.5f ) ; 
				float yr = -0.6f ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(全て同じ動き)
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xr, yr ),
						BulletType			= bulletType,
						BulletAmount		= bulletAmount,
						AimingType			= aimingType,
						AmplitudeType		= amplitudeType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var bossEnemy = owner.CreateEnemy( EnemyShapeTypes.No_022, 2, shield, 3000, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level, isBoss:true ) ;

					// ボス用のコリジョン設定
					bossEnemy.SetCollisionType( EnemyCollisionTypes.Boss ) ;

					// 爆発演出補正
					bossEnemy.ExplosionScale = 3.0f ;
					bossEnemy.ExplosionTimes = 10 ;

					xr += distance ;
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
				public Vector2	StartRatioPosition ;
				public int		BulletType ;
				public int		BulletAmount ;
				public int		AimingType ;
				public int		AmplitudeType ;
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

				// 攻撃開始位置
				var attackRatioPosition	= new Vector2( startRatioPosition.X, -0.2f ) ;

				// 引き上げる場合
//				var endRatioPosition	= new Vector2( startRatioPosition.X, -0.6f ) ;

				// 小さい弾(デフォルト)
				EnemyBulletShapeTypes bulletType = EnemyBulletShapeTypes.BulletSmall ;
				if( settings.BulletType != 0 )
				{
					// 細いレーザー
					bulletType			= EnemyBulletShapeTypes.LaserSlim ;
				}

				int bulletAmount		= settings.BulletAmount ;
				int aimingType			= settings.AimingType ;
				int amplitudeType		= settings.AimingType ;

				// 最初に移動する方向
				float signX ;
				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 左
					signX = -1 ;
				}
				else
				{
					// 右
					signX = +1 ;
				}

				// 最初に移動する方向
				float signY ;
				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 上
					signY = -1 ;
				}
				else
				{
					// 下
					signY = +1 ;
				}

				float amplitudeY = 0.05f * amplitudeType ;

				// 縦は Sin
				// 横は Con

				//---------------------------------

				// 初期方向
				Vector2 velocity = attackRatioPosition - startRatioPosition ;

				// 初期方向
				enemy.SetAngle( velocity ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				int attackCount = 0 ;
				var attackIntervalTimer = new SimpleTimer() ;

				float attackDuration = 0.8f ;

				Vector2 attackDirection ;
				Vector2 bulletDirection ;

				float	bulletSpeed		= ( bulletType == 0 ? 300.0f : 500.0f ) ;
				int		bulletDamage	= ( bulletType == 0 ? 1 : 4 ) ;

				float a, d ;
				int i, l ;

				float radian ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

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

						enemy.RatioPosition = velocity * factor + startRatioPosition ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;

							// タイマーをリセット
							attackIntervalTimer.Reset() ;
						}
					}
					else
					if( phase == 1 )
					{
						// 攻撃と移動

						if( attackCount == 0 || attackIntervalTimer.IsFinished( attackDuration ) == true )
						{
							// 攻撃実行

							if( aimingType == 0 || m_Owner.IsPlayerDestroyed == true )
							{
								attackDirection = new Vector2(  0, +1 ) ;
							}
							else
							{
								attackDirection = ( m_Owner._Player.Position - enemy.Position ).Normalized() ;
							}

							// 角度差は２０度とする
							d = 20.0f ;

							a = - ( bulletAmount * d * 0.5f ) + ( d * 0.5f ) ;

							// 後で時間差弾ばらまきを考えてここでは展開処理を書いておく
							l = bulletAmount ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								bulletDirection = ExMath.GetRotatedVector( attackDirection, a ) ;

								m_Owner.CreateEnemyBullet
								(
									bulletType, enemy.Position, bulletDirection, bulletSpeed, bulletDamage
								) ;

								a += d ;
							}

							attackCount ++ ;

							// タイマーをリセット
							attackIntervalTimer.Reset() ;
						}

						//-------------------------------
						// 移動

						duration = 4.0f ;

						if( time >  duration )
						{
							time -= duration ;
						}
						factor = time / duration ;


						radian = 2.0f * Mathf.Pi * factor ;

						float x = attackRatioPosition.X +   Mathf.Sin( radian        ) * 0.38f      * signX ;
						float y = attackRatioPosition.Y + ( Mathf.Sin( radian * 2.0f ) * amplitudeY * signY ) ;

						enemy.RatioPosition = new Vector2( x, y ) ;
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
			private bool OnEnemyDestroyed( Enemy bossEnemy, EnemyDestroyedReasonTypes destroyedReasonType )
			{
				// ボスに返し弾は無し
#if false
				if( destroyedReasonType == EnemyDestroyedReasonTypes.PlayerShot )
				{
					// 設定情報を取り出す
					var settings = bossEnemy.Settings as Settings ;

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
#endif
				// 実際に破壊してよい
				return true ;
			}
		}
	}
}
