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
		/// エネミーグループ(種別 024)
		/// </summary>
		public class EnemyGroup_024 : EnemyGroupBase
		{
			/// <summary>
			/// 上下の左右から出現し高速に画面反対側への斜め移動を繰り返す
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
//				positionType = 1 ;		// デバッグ

				float yr ;

				if( positionType == 0 )
				{
					// 上から出る
					yr = -0.6f ;
				}
				else
				{
					// 下から出る
					yr = +0.6f ;
				}

				float xr ;

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 左から出る
					xr = -0.6f ;
				}
				else
				{
					// 右から出る
					xr = +0.6f ;
				}

				//---------------------------------
				// ダブル表示にするかどうか(対角線は無し)

				int[] double_weights =
				{
					100,
					( level / 4 )
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

				l = ExMath.GetRandomRange( 10, 12 ) ;

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
				int shield = GetShield( 1, 2, level ) ;

				if( isDouble == false )
				{
					// シングル

					// 設定値の生成(共通)
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xr, yr ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_024, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.2f ) ;
					}
				}
				else
				{
					// ダブル

					// 設定値の生成(共通)
					var settings_0 = new Settings()
					{
						StartRatioPosition	= new Vector2( xr, yr ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					float xs, ys ;

					// ミラーか対角線か
					int doubleType = ExMath.GetRandomRange( 0, 1 ) ;
					if(  doubleType == 0 )
					{
						// ミラーＸ
						xs = -xr ;
						ys =  yr ;
					}
					else
					{
						// ミラーＹ
						xs =  xr ;
						ys = -yr ;
					}

					// 設定値の生成(共通)
					var settings_1 = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_024, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_024, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 0.2f ) ;
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

				float signX = ExMath.Sign( startRatioPosition.X ) ;
				float signY = ExMath.Sign( startRatioPosition.Y ) ;

				// 最初の折返し位置
				var turnRatioPosition = new Vector2( -0.45f * signX, startRatioPosition.Y - 0.1f * signY ) ; ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				Vector2 velocity = Vector2.Zero ;

				Vector2 baseRatioPosition = startRatioPosition ;
				Vector2 previousRatioPosition = startRatioPosition ;


				int moveCount = 0 ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 前身

						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.EaseInOutQuad ) ;


						velocity = turnRatioPosition - baseRatioPosition ;

						enemy.RatioPosition = velocity * factor + baseRatioPosition ;


						//-------------------------------

						if( factor >= 1 )
						{
							moveCount ++ ;

							if( moveCount <  6 )
							{
								// 斜め移動継続

								time = 0 ;

								//------------------------------

								float xt = - turnRatioPosition.X ;
								float yt = turnRatioPosition.Y - 0.2f * signY ;

								baseRatioPosition = turnRatioPosition ;
								turnRatioPosition = new Vector2( xt, yt ) ;
							}
							else
							{
								phase = 1 ;
								time = 0 ;


								//-----------------------------
								// プレイヤーがいればプレイヤーの方向に向かう

								previousRatioPosition = enemy.RatioPosition ;

								if( m_Owner.IsPlayerDestroyed == false )
								{
									velocity = ( m_Owner.Player.Position - enemy.Position ).Normalized() ;
									velocity *= 400.0f ;
								}
								else
								{
									float pxs = ExMath.Sign( enemy.Position.X ) ;
									velocity = new Vector2( - pxs * 400.0f, 0 ) ;
								}
							}
						}
					}
					else
					if( phase == 1 )
					{
						// 退却

						enemy.Position += velocity * delta ;
					}

					//-------------------------------
					// 表示の方向設定

					direction = enemy.RatioPosition - previousRatioPosition ;
					previousRatioPosition = enemy.RatioPosition ;

					if( direction.Y >  0 )
					{
						// 下に進む
						enemy.SetAngle( new Vector2( 0, +1 ) ) ;

						if( direction.X >  0 )
						{
							enemy.SetFlip( true ) ;
						}
						else
						{
							enemy.SetFlip( false ) ;
						}
					}
					else
					{
						// 上に進む
						enemy.SetAngle( new Vector2( 0, -1 ) ) ;

						if( direction.X <  0 )
						{
							enemy.SetFlip( true ) ;
						}
						else
						{
							enemy.SetFlip( false ) ;
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
