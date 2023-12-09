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
		/// エネミーグループ(種別 006)
		/// </summary>
		public class EnemyGroup_006 : EnemyGroupBase
		{
			/// <summary>
			/// ジグザクに移動する
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
					100,					// 進む
					 50 + ( level / 2 ),	// 戻る
				} ;

				int variationType = ExMath.GetRandomIndex( weights ) ;
//				variationType = 1 ;	// デバッグ

				//---------------------------------------------------------

				//-------------
				// 上下

				float ys ;

				int[] yr_weights =
				{
					100,	// 上から
					level,	// 下から
				} ;

				int yr_index = ExMath.GetRandomIndex( yr_weights ) ;
//				yr_index = 1 ;	// デバッグ

				if( yr_index == 0 )
				{
					// 上

					ys = -0.6f ;
				}
				else
				{
					// 下

					ys = +0.6f ;
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

				l = ExMath.GetRandomRange(  8, 10 ) ;

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
				int shield = GetShield( 2, 4, level ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					float xs = ExMath.GetRandomRange( -0.4f, +0.4f ) ;

					// 設定値の生成
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_006, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 少し待つ
					await Wait( 0.2f ) ;
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

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				float yr = 0 ;
				float yf = 0 ;

				bool isFire = false ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 y_velocity = Vector2.Zero ;
				Vector2 r_velocity = Vector2.Zero ;

				if( startRatioPosition.Y <  0 )
				{
					// 上から下
					yr = +0.4f ;
					yf = -0.1f ;

					y_velocity = new Vector2( 0, +1 ) ;
					r_velocity = new Vector2( 0, -1 ) ;
				}
				else
				if( startRatioPosition.Y >  0 )
				{
					// 下から上
					yr = -0.4f ;
					yf = +0.1f ;

					y_velocity = new Vector2( 0, -1 ) ;
					r_velocity = new Vector2( 0, +1 ) ;
				}

				//---------------------------------

				y_velocity *= 300 ;
				r_velocity *= 500 ;

				Vector2 x_velocity ;

				// 最初の横方向
				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					x_velocity = new Vector2( -1,  0 ) ;
				}
				else
				{
					x_velocity = new Vector2( +1,  0 ) ;
				}

				x_velocity *= 300 ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				// 最初の方向


				int velocityType = 0 ;

				// 初期の回転角度を設定
				enemy.SetAngle( y_velocity ) ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 出現

						duration = 0.25f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( velocityType == 0 )
						{
							// 縦
							enemy.Position += y_velocity * delta ;
						}
						else
						{
							// 横
							enemy.Position += x_velocity * delta ;
						}

						if( factor >= 1 )
						{
							time = 0 ;

							// 方向切替
							if( velocityType == 1 )
							{
								// 横方向の向き反転
								x_velocity = - x_velocity ;
							}

							velocityType = 1 - velocityType ;

							if( velocityType == 0 )
							{
								// 縦
								enemy.SetAngle( y_velocity ) ;
							}
							else
							{
								// 横
								enemy.SetAngle( x_velocity ) ;
							}

							//-------------------------------

							// 弾撃ち判定
							if( isFire == false )
							{
								if( ( startRatioPosition.Y <  0 && enemy.RatioPosition.Y >= yf ) || ( startRatioPosition.Y >  0 && enemy.RatioPosition.Y <= yf ) )
								{
									m_Owner.CreateEnemyBullet( 0, enemy.Position, 300, 1 ) ;
									isFire = true ;
								}
							}

							// 戻りポイント
							if( variationType == 1 )
							{
								if( ( startRatioPosition.Y <  0 && enemy.RatioPosition.Y >= yr ) || ( startRatioPosition.Y >  0 && enemy.RatioPosition.Y <= yr ) )
								{
									phase = 1 ;
									
									enemy.SetAngle( r_velocity ) ;
								}
							}
						}
					}
					else
					if( phase == 1 )
					{
						// 戻る
						enemy.Position += r_velocity * delta ;
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
