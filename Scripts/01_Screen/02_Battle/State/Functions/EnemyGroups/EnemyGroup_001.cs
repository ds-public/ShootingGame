using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		/// <summary>
		/// エネミーグループ(種別 001)
		/// </summary>
		public class EnemyGroup_001 : EnemyGroupBase
		{
			/// <summary>
			/// 画面の上・左右のいずれかから出現し直線するのみ
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

				// 初期値
				int r ;
				
				//---------------------------------
				// 左右

				r = ExMath.GetRandomRange( 0, 99 ) ;

				float xr ;

				if( r <  50 )
				{
					// 左

					xr = -0.4f ;
				}
				else
				{
					// 右

					xr = +0.4f ;
				}

				//-------------
				// 上下

				float yr ;

				int[] y_weights =
				{
					150,
					level
				} ;

				int yp = ExMath.GetRandomIndex( y_weights ) ;

				if( yp == 0 )
				{
					// 上

					yr = -0.6f ;
				}
				else
				{
					// 下

					yr = +0.6f ;
				}

				//---------------------------------
				// 直進・斜めに曲がる・Ｙ軸合わせで向かってくる

				// レベルが上がるほど後のものの選出率が上がる

				int[] weights =
				{
					100,
					 50 + ( level / 2 ),
					 50 + ( level / 2 ),
					 10 +   level
				} ;

				int variationType = ExMath.GetRandomIndex( weights ) ;
//				variationType = 2 ;

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
				if( level >= 60 )
				{
					isReflectorBullet  = true ;
				}

				//---------------------------------------------------------

				int i, l = ExMath.GetRandomRange( 4, 6 ) ;

				// 出現数が確定した時点でカウンターを更新する
				var enemyGroupCounter = new EnemyGroupCounter( this )
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = l
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield( 1, 2, level ) ;

				// 設定値の生成
				var settings = new Settings()
				{
					StartRatioPosition	= new Vector2( xr, yr ),
					VariationType		= variationType,
					IsReflectorBullet	= isReflectorBullet,
				} ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// エネミーを生成する：外観・ダメージ値・耐久値・Ｘ初期位置・Ｙ初期位置・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_001, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 少し待つ
					await Wait( 0.25f ) ;
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
				public int		VariationType ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				var startRatioPosition = settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				//---------------------------------
				// 移動量と画面外判定情報

				var velocity = Vector2.Zero ;	// １秒あたりの移動量

				float direction = 0 ;
				float limit_yr = 0 ;

				if( startRatioPosition.Y <  0 )
				{
					// 上から下
					velocity = new Vector2(    0, +220 ) ;

					direction = +1 ;
					limit_yr = +0.6f ;
				}
				else
				if( startRatioPosition.Y >  0 )
				{
					// 下から上
					velocity = new Vector2(    0, -220 ) ;

					direction = -1 ;
					limit_yr = -0.6f ;
				}

				if( direction == 0 )
				{
					// 異常
					enemy.OutOfScreen() ;
					return ;
				}

				//---------------------------------

				int phase = 0 ;
				bool autoRotation = true ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					if( settings.VariationType == 1 )
					{
						// 途中で斜めに移動する
						if( phase == 0 )
						{
							if( ( startRatioPosition.Y <  0 && enemy.RatioPosition.Y >   0.1f ) || ( startRatioPosition.Y >  0 && enemy.RatioPosition.Y <  -0.1f ) )
							{
								velocity = ExMath.GetRotatedVector
								(
									velocity,
									60.0f * ExMath.Sign( startRatioPosition.Y ) * ( - ExMath.Sign( startRatioPosition.X ) ) 
								) ;

								// 少し速度アップ
								velocity *= 2.0f ;
								autoRotation = false ;

								phase = 1 ;
							}
						}
					}
					else
					if( settings.VariationType == 2 )
					{
						// 途中で斜めに移動する
						if( phase == 0 )
						{
							if( ( startRatioPosition.Y <  0 && enemy.RatioPosition.Y >   0.3f ) || ( startRatioPosition.Y >  0 && enemy.RatioPosition.Y <  -0.3f ) )
							{
								velocity = ExMath.GetRotatedVector
								(
									velocity,
									135.0f * ExMath.Sign( startRatioPosition.Y ) * ( - ExMath.Sign( startRatioPosition.X ) ) 
								) ;

								// 少し速度アップ
								velocity *= 2.0f ;
								autoRotation = true ;

								phase = 1 ;
							}
						}
					}
					else
					if( settings.VariationType == 3 )
					{
						// プレイヤーにＸ軸があったら向かってくる(ただし内側のみ)
						if( phase == 0 && m_Owner.IsPlayerDestroyed == false )
						{
							var player = m_Owner._Player ;

							float y = Mathf.Abs( player.Position.Y - enemy.Position.Y ) ;
							if( y <  64 )
							{
								velocity = ExMath.GetRotatedVector
								(
									velocity,
									90.0f * ExMath.Sign( startRatioPosition.Y ) * ( - ExMath.Sign( startRatioPosition.X ) ) 
								) ;

								// 少し速度アップ
								velocity *= 2.0f ;
								autoRotation = true ;

								phase = 1 ;
							}
						}
					}

					//--------------------------------

					// エネミーを移動させる(内部でデルタタイム換算で処理される)
					enemy.Move( velocity, autoRotation ) ;

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					// 上から下へ
					if( direction >  0 && enemy.RatioPosition.Y >  limit_yr )
					{
						// 画面外
						break ;
					}

					// 下から上へ
					if( direction <  0 && enemy.RatioPosition.Y <  limit_yr )
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
