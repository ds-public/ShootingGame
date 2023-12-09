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
		/// エネミーグループ(種別 011)
		/// </summary>
		public class EnemyGroup_011 : EnemyGroupBase
		{
			/// <summary>
			/// 画面に円を描くように出現し弾を撃つか特攻
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

				int[] variation_weights =
				{
					   50,		//  特攻
					   40,		//  射撃
				} ;

				int variationType = ExMath.GetRandomIndex( variation_weights ) ;
//				variationType = 1 ;	// デバッグ

				int[] amount_weights =
				{
					   50,		//  4
					level * 2,	//  8
					level,		// 12
					level / 2	// 16
				} ;

				int amount = ( ExMath.GetRandomIndex( amount_weights ) + 1 ) * 4 ;
//				amount = 16 ;

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
				if( level >= 50 )
				{
					isReflectorBullet  = true ;
				}

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

				float radian ;
				float cv, sv ;

				// 位置計算
				Vector2[] positions = new Vector2[ l ] ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					radian = 2.0f * Mathf.Pi * i / l ;
					
					cv = Mathf.Cos( radian ) ;
					sv = Mathf.Sin( radian ) ;

					positions[ i ] = new Vector2( 0.4f * cv, 0.4f * sv ) ;
				}

				// 移動方向
				int d ;
				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					d = +1 ;
				}
				else
				{
					d = -1 ;
				}

				// レベルによるシールド値の補正
				int shield = GetShield( 1, 2, level ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(全て同じ動き)
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( positions[   i               ].X , positions[   i               ].Y ),
						EndRatioPosition	= new Vector2( positions[ ( i + d + l ) % l ].X , positions[ ( i + d + l ) % l ].Y ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_011, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;
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

				// 初期の向き
				enemy.SetAngle( - startRatioPosition ) ;

				// 初期は非表示
				enemy.Alpha = 0.0f ;
				enemy.SetCollisionEnabled( false ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				Vector2 velocity = ( endRatioPosition - startRatioPosition ).Normalized() * 300.0f ;	// １秒あたりの移動量
				
				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					if( phase == 0 )
					{
						// 出現

						duration = 1.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						enemy.Alpha = factor ;

						if( factor >= 1 )
						{
							enemy.SetCollisionEnabled( true ) ;

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
							if( variationType == 1 )
							{
								// 弾発射
								m_Owner.CreateEnemyBullet( 0, enemy.Position, 200.0f, 1 ) ;

								// 移動

							}
							else
							{
								// 特攻

								if( m_Owner.IsPlayerDestroyed == false )
								{
									velocity = ( m_Owner._Player.Position - enemy.Position ).Normalized() * 300.0f ;
								}
							}

							enemy.SetAngle( velocity ) ;

							phase = 2 ;
							time = 0 ;
						}
					}
					else
					if( phase == 2 )
					{
						enemy.Position += velocity * delta ;
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
