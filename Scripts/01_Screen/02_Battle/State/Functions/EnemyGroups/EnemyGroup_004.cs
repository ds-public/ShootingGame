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
		/// エネミーグループ(種別 004)
		/// </summary>
		public class EnemyGroup_004 : EnemyGroupBase
		{
			/// <summary>
			/// 画面中央に集まって弾を撃つ
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

				int[] weights =
				{
					100,					// 発射
					 50 + ( level / 2 ),	// 特攻
				} ;

				int variationType = ExMath.GetRandomIndex( weights ) ;
//				variationType = 1 ;	// デバッグ

				//---------------------------------------------------------

				//-------------
				// 上下

				float yr0 ;
				float yr1 ;

				int[] yr_weights =
				{
					100,	// 上から
					level,	// 下から
				} ;

				int yr_index = ExMath.GetRandomIndex( yr_weights ) ;

				if( yr_index == 0 )
				{
					// 上

					yr0 = -0.6f ;
					yr1 = -0.2f ;
				}
				else
				{
					// 下

					yr0 = +0.6f ;
					yr1 = +0.2f ;
				}

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
				int shield = GetShield( 1, 3, level ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					float xr0 = ExMath.GetRandomRange( -0.4f, + 0.4f ) ;
					float xr1 = ExMath.GetRandomRange( -0.4f, + 0.4f ) ;

					// 設定値の生成
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xr0, yr0 ),
						EndRatioPosition	= new Vector2( xr1, yr1 + ExMath.GetRandomRange( -0.05f, +0.05f ) ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_004, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 少し待つ
					await Wait( 0.1f ) ;
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
				public Vector2	EndRatioPosition ;
				public int		VariationType ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// バリエーションタイプ
				var variationType = settings.VariationType ;

				var startRatioPosition	= settings.StartRatioPosition ;
				var endRatioPosition	= settings.EndRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;



				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 velocity ;	// １秒あたりの移動量

				float limit_yr = 0 ;

				Vector2 attackDirection = new ( 0, +1 ) ;

				if( startRatioPosition.Y <  0 )
				{
					// 上から下
					limit_yr = +0.6f ;

					attackDirection = new Vector2( 0, +1 ) ;
				}
				else
				if( startRatioPosition.Y >  0 )
				{
					// 下から上
					limit_yr = -0.6f ;

					attackDirection = new Vector2( 0, -1 ) ;
				}

				if( limit_yr == 0 )
				{
					// 異常
					enemy.OutOfScreen() ;
					return ;
				}

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				// 最初の方向
				var direction = ( endRatioPosition - startRatioPosition ).Normalized() ;
				velocity = direction ;

				// 初期の回転角度を設定
				enemy.SetAngle( velocity ) ;

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

						factor = Ease.GetValue( factor, EaseTypes.EaseOutQuad ) ;

						velocity = endRatioPosition - startRatioPosition ;

						enemy.RatioPosition = velocity * factor + startRatioPosition ;

						if( factor >= 1 )
						{
							// 攻撃時の回転角度を設定
							enemy.SetAngle( attackDirection ) ;

							phase = 1 ;
							time = 0 ;
						}
					}
					else
					if( phase == 1 )
					{
						// 少し待つ
						duration = 0.3f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							if( variationType == 0 )
							{
								// 弾発射
								m_Owner.CreateEnemyBullet( 0, enemy.Position, 200.0f, 1 ) ;
							}

							phase = 2 ;
							time = 0 ;
						}
					}
					else
					if( phase == 2 )
					{
						// 少し待つ
						duration = 0.3f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							if( variationType == 0 )
							{
								// 戻り
								velocity = startRatioPosition - endRatioPosition ;
								enemy.SetAngle( velocity ) ;

								phase = 3 ;
								time = 0 ;
							}
							else
							{
								// 特攻
								velocity = ( ( m_Owner.Player.Position - enemy.Position ).Normalized() ) * 600.0f ;
								enemy.SetAngle( velocity ) ;

								phase = 4 ;
								time = 0 ;
							}
						}
					}
					else
					if( phase == 3 )
					{
						// 戻る
						
						duration = 0.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.EaseInQuad ) ;

						velocity = startRatioPosition - endRatioPosition ;

						enemy.RatioPosition = velocity * factor + endRatioPosition ;

						if( factor >= 1 )
						{
							phase = 9 ;
							time = 0 ;
						}
					}
					else
					if( phase == 4 )
					{
						// 特攻

						duration = 1.0f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = 1.0f - ( Ease.GetValue( factor, EaseTypes.EaseInQuad ) * 0.5f ) ;				
						enemy.Position += velocity * factor * delta ;
					}
					else
					if( phase == 9 )
					{
						// 終了
						break ;
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					// 上から下へ
					if( limit_yr >  0 && enemy.RatioPosition.Y >  limit_yr )
					{
						// 画面外
						break ;
					}

					// 下から上へ
					if( limit_yr <  0 && enemy.RatioPosition.Y <  limit_yr )
					{
						// 画面外
						break ;
					}

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
