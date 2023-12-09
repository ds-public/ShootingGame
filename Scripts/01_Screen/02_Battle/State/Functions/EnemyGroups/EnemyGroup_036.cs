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
		/// エネミーグループ(種別 036)
		/// </summary>
		public class EnemyGroup_036 : EnemyGroupBase
		{
			/// <summary>
			/// リザーブ
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

				float xs ;
				float ys ;

				if( positionType == 0 )
				{
					// 上から

					ys = -0.6f ;
				}
				else
				{
					// 下から

					ys = +0.6f ;
				}


				int[] attack_weights =
				{
					20,					// 1 way
					( level / 2 ),		// 3 way
					( level / 4 ),		// ホーミング弾
				} ;

				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 3 ;	// デバッグ

				//---------------------------------
				// ダブル表示にするかどうか

				int[] double_weights =
				{
					100,
					( level / 2 )
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

				l = ExMath.GetRandomRange(  8, 10 ) ;

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
				var enemyGroupCounter = new EnemyGroupCounter( this)
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = amount
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield( 2, 5, level ) ;

				if( isDouble == false )
				{
					// シングル

					for( i  = 0 ; i <  l ; i ++ )
					{
						xs = ExMath.GetRandomRange( -0.45f, +0.45f ) ;

						// 設定値の生成(共通)
						var settings = new Settings()
						{
							StartRatioPosition	= new Vector2( xs, ys ),
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_030, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.25f ) ;
					}
				}
				else
				{
					// ダブル

					for( i  = 0 ; i <  l ; i ++ )
					{
						xs = ExMath.GetRandomRange( -0.45f, +0.45f ) ;

						// 設定値の生成(共通)
						var settings_0 = new Settings()
						{
							StartRatioPosition	= new Vector2( xs,   ys ),
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						xs = ExMath.GetRandomRange( -0.45f, +0.45f ) ;

						// 設定値の生成(共通)
						var settings_1 = new Settings()
						{
							StartRatioPosition	= new Vector2( xs, - ys ),
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_030, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_030, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 0.25f ) ;
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

				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

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

				Vector2 sideDirection ;

				if( startRatioPosition .X <  0 )
				{
					// 右へ
					sideDirection = new Vector2( +1,  0 ) ;
				}
				else
				{
					// 左へ
					sideDirection = new Vector2( -1,  0 ) ;
				}

				var attackType = settings.AttackType ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				float attackInterval = 0.0f ;
				float attackDuration = 3.0f ;


				Vector2 velocity = Vector2.Zero ;

				// 初期の回転角度を設定
				enemy.SetAngle( direction ) ;

				// ホッピング前の位置
				Vector2 baseRatioPosition = startRatioPosition ;
				Vector2 moveRatioPosition ;

				float xh, yh ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
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
							+ ( 0.24f * sideDirection * xh )
							- ( 0.12f * direction * yh ) ;

						enemy.RatioPosition = moveRatioPosition ;

						//-------------------------------------------------------
						// 攻撃

						attackInterval += delta ;

						if( attackInterval >= attackDuration )
						{
							attackInterval  = 0 ;

							if( m_Owner.IsPlayerDestroyed == false )
							{
								if( attackType <= 1 )
								{
									m_Owner.CreateEnemyBulletMulti
									(
										EnemyBulletShapeTypes.BulletSmall, enemy.Position, 220.0f, 1, 0,
										1 + attackType * 2, 30.0f
									) ;
								}
								else
								{
									// ホーミング
									m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.BulletLarge, enemy.Position, 220.0f, 1, 0, 0.01f, 30.0f, 1.5f ) ;
								}
							}
						}

						//-------------------------------------------------------

						if( factor >= 1 )
						{
							//------------------------------

							bool isReturn = false ;

							if( startRatioPosition.Y <  0 )
							{
								// 上から
								if( enemy.RatioPosition.Y >= +0.35f )
								{
									// 戻る
									isReturn = true ;
								}
							}
							else
							{
								// 下から
								if( enemy.RatioPosition.Y <= -0.35f )
								{
									// 戻る
									isReturn = true ;
								}
							}

							if( isReturn == false )
							{
								// ホッピング継続

								phase = 0 ;
								time = 0 ;

								sideDirection = - sideDirection ;

								baseRatioPosition = moveRatioPosition ;
							}
							else
							{
								// 帰る

								phase = 1 ;
								time = 0 ;

								velocity = ( - direction ) * 400.0f ;
							}
						}
					}
					else
					if( phase == 1 )
					{
						// 後退

						enemy.Position += velocity * delta ;
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( enemy, 0.8f ) == true )
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
