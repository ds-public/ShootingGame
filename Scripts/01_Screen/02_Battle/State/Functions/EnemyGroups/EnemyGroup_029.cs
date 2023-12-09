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
		/// エネミーグループ(種別 029)
		/// </summary>
		public class EnemyGroup_029 : EnemyGroupBase
		{
			/// <summary>
			/// 画面上部下部の位置にランダム出現し前進しつつプレイヤーにＸ軸を合わせてレーザーを撃つ
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

				int[] position_weights =
				{
					100,				// 上
					( level / 2 )		// 下
				} ;

				int positionType = ExMath.GetRandomIndex( position_weights ) ;
//				positionType = 1 ;	// デバッグ

				float ys0, ys1 ;

				if( positionType == 0 )
				{
					// 上から

					ys0 = -0.45f ;
					ys1 = -0.35f ;
				}
				else
				{
					// 下から

					ys0 = +0.35f ;
					ys1 = +0.45f ;
				}


				int[] attack_weights =
				{
					100,			// 1 way
					level / 4,		// 3 way
//					level / 8,		// 5 way
				} ;

				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 3 ;	// デバッグ

				//---------------------------------
				// ダブル表示にするかどうか

				int[] double_weights =
				{
					100,
					( level / 16 )
				} ;

				bool isDouble = ExMath.GetRandomIndex( double_weights ) != 0 ;
//				isDouble = true ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
				if( level >= 60 )
				{
					isReflectorBullet  = true ;
				}

				//---------------------------------

				int i, l = 0 ;

				l = ExMath.GetRandomRange(  8, 12 ) ;

				int amount ;

				if( isDouble == false )
				{
					amount = l ;
				}
				else
				{
					amount = l * 2 ;
				}

				// 出現数が確定した時点でカウンターを更新する
				var enemyGroupCounter = new EnemyGroupCounter( this )
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = amount
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield( 2, 4, level ) ;

				float xp, yp ;

				if( isDouble == false )
				{
					// シングル

					for( i  = 0 ; i <  l ; i ++ )
					{
						xp = ExMath.GetRandomRange( -0.4f, +0.4f ) ;
						yp = ExMath.GetRandomRange(   ys0,   ys1 ) ;

						// 設定値の生成(共通)
						var settings = new Settings()
						{
							StartRatioPosition	= new Vector2( xp, yp ),
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_029, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.3f ) ;
					}
				}
				else
				{
					// ダブル


					for( i  = 0 ; i <  l ; i ++ )
					{
						xp = ExMath.GetRandomRange( -0.4f, +0.4f ) ;
						yp = ExMath.GetRandomRange(   ys0,   ys1 ) ;

						// 設定値の生成(共通)
						var settings_0 = new Settings()
						{
							StartRatioPosition	= new Vector2( xp, yp ),
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						xp = ExMath.GetRandomRange( -0.4f, +0.4f ) ;
						yp = ExMath.GetRandomRange( - ys1, - ys0 ) ;

						// 設定値の生成(共通)
						var settings_1 = new Settings()
						{
							StartRatioPosition	= new Vector2( xp, yp ),
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						//-------------------------------

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_029, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_029, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 0.3f ) ;
					}
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
				public int		AttackType ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// 開始位置
				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;


				//---------------------------------

				// 基本移動方向
				Vector2 direction ;

				if( startRatioPosition.Y <  0 )
				{
					// 上から
					direction = new Vector2(  0, +1 ) ;
				}
				else
				{
					// 下から
					direction = new Vector2(  0, -1 ) ;
				}

				// 攻撃タイプ
				var attackType = settings.AttackType ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				Vector2 velocity = Vector2.Zero ;

				// 初期の回転角度を設定
				enemy.SetAngle( direction ) ;

				enemy.Alpha = 0 ;
				enemy.SetCollisionEnabled( false ) ;


				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// フェードイン

						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						enemy.Alpha = factor ;
						enemy.Scale = new Vector2( factor, 32.0f - ( 31.0f * factor ) ) ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;

							// コリジョン有効化
							enemy.SetCollisionEnabled( true ) ;

							if( m_Owner.IsPlayerDestroyed == false )
							{
								velocity = m_Owner.Player.Position - enemy.Position ;
							}
							else
							{
								velocity = Vector2.Zero ;
							}

							if( velocity.X != 0 )
							{
								velocity.Y = 0 ;

								velocity = velocity.Normalized() ;
								velocity *= 100.0f ;
							}

							//------------------------------

							if(  m_Owner.IsPlayerDestroyed == false )
							{
								if( attackType >= 1 )
								{
									m_Owner.CreateEnemyBulletMulti
									(
										EnemyBulletShapeTypes.LaserSlim, enemy.Position, direction, 600.0f, 4, 0, 
										1, 30.0f,
										correction: 64
									) ;
								}
							}
						}
					}
					else
					if( phase == 1 )
					{
						// 移動
						duration = 0.5f ;	// ０．５秒で最高速度に到達

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						enemy.Position += ( direction * 800.0f * factor * delta ) ;

						if( velocity.X != 0 )
						{
							// 横移動あり
							enemy.Position += velocity * delta ;
						}

						//-------------------------------------------------------

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
