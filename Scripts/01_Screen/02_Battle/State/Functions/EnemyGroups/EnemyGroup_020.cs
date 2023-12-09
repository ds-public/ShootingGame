﻿using Godot ;
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
		/// エネミーグループ(種別 020)
		/// </summary>
		public class EnemyGroup_020 : EnemyGroupBase
		{
			/// <summary>
			/// 中型
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

				// 出現数
				int[] amount_weights =
				{
					   50,		//  1
					level * 2,	//  2
					level,		//  3
				} ;
				int amount = ExMath.GetRandomIndex( amount_weights ) + 1 ;
//				amount = 1 ;

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
					   level,			//  ５方向
				} ;
				int bulletAmount = ExMath.GetRandomIndex( bulletAmount_weights ) * 2 + 3 ;
//				bulletAmount = 5 ;

				int[] aiming_weights =
				{
					   70,		//  真っ直ぐ発射
					   30,		//  プレイヤーの方向を狙う
				} ;
				int aimingType = ExMath.GetRandomIndex( aiming_weights ) ;
//				aimingType = 1 ;

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
				var enemyGroupCounter = new EnemyGroupCounter( this)
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = l
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield( 30, 60, level ) ;

				float distance = 0.24f ;

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
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var enemy = owner.CreateEnemy( EnemyShapeTypes.No_020, 2, shield, 500, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 爆発演出補正
					enemy.ExplosionScale = 1.5f ;
					enemy.ExplosionTimes = 5 ;

					xr += distance ;
				}

				// 少し待つ
				await Wait( 0.1f ) ;

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
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				var startRatioPosition	= settings.StartRatioPosition ;

				var attackRatioPosition	= new Vector2( startRatioPosition.X, -0.2f ) ;

				var endRatioPosition	= new Vector2( startRatioPosition.X, +0.6f ) ;

				// 小さい弾(デフォルト)
				EnemyBulletShapeTypes bulletType = EnemyBulletShapeTypes.BulletSmall  ;
				if( settings.BulletType != 0 )
				{
					// 細いレーザー
					bulletType = EnemyBulletShapeTypes.LaserSlim ;
				}

				int bulletAmount		= settings.BulletAmount ;
				int aimingType			= settings.AimingType ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 初期の向き
				enemy.SetAngle( startRatioPosition ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				int attackCount = 0 ;
				var attackIntervalTimer = new SimpleTimer() ;

				Vector2 velocity = attackRatioPosition - startRatioPosition ;

				float attackDuration = 0.8f ;

				Vector2 attackDirection ;
				Vector2 bulletDirection ;

				float	bulletSpeed		= ( bulletType == 0 ? 300.0f : 500.0f ) ;
				int		bulletDamage	= ( bulletType == 0 ? 1 : 4 ) ;

				float a, d ;
				int i, l ;

				enemy.SetAngle( velocity ) ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					if( phase == 0 )
					{
						// 出現

						duration = 0.8f ;

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
						// 最初のウェイト
						duration = 0.3f ;

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
						}
					}
					else
					if( phase == 2 )
					{
						// 攻撃

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

								m_Owner.CreateEnemyBullet( bulletType, enemy.Position, bulletDirection, bulletSpeed, bulletDamage ) ;

								a += d ;
							}

							attackCount ++ ;

							if( attackCount >= 6 )
							{
								// 終了
								phase = 3 ;
								time = 0 ;
							}

							// タイマーをリセット
							attackIntervalTimer.Reset() ;
						}
					}
					else
					if( phase == 3 )
					{
						// 少し待つ
						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							phase = 4 ;
							time = 0 ;
						}
					}
					else
					if( phase == 4 )
					{
						// 退却
						duration = 2.4f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.EaseInQuad ) ;

						velocity = endRatioPosition - attackRatioPosition ;

						enemy.RatioPosition = velocity * factor + attackRatioPosition ;

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
