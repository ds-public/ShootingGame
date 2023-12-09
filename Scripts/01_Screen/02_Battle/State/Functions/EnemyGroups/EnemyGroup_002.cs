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
		/// エネミーグループ(種別 002)
		/// </summary>
		public class EnemyGroup_002 : EnemyGroupBase
		{
			/// <summary>
			/// 左右大きくに蛇行
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
				uint r ;
				
				//---------------------------------
				// 左右

				// 常に中央出現
				float xr = 0 ;

				// 振幅
				float amplitude ;

				r = GD.Randi() % 100 ;
				if( r <  20 )
				{
					// 振幅なし
					amplitude = 0 ;
				}
				else
				{
					r = GD.Randi() % 100 ;

					if( r <  50 )
					{
						// 左
						amplitude = -0.3f ;
					}
					else
					{
						// 右
						amplitude = +0.3f ;
					}
				}

//				amplitude = 0.3f ;		// デバッグ

				bool fromCenter = true ;
				if( amplitude != 0 )
				{
					r = GD.Randi() % 100 ;
					if( r <  50 )
					{
						fromCenter = true ;
					}
					else
					{
						fromCenter = false ;
					}
				}

//				fromCenter = false ;	// デバッグ

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
					  0 +   level,
					  0 +   level
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

				//---------------------------------

				int i, l = ExMath.GetRandomRange(  8, 16 ) ;

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
				int shield = GetShield( 1, 2, level ) ;

				// 設定値の生成(共通)
				var settings = new Settings()
				{
					StartRatioPosition	= new Vector2( xr, yr ),
					Amplitude			= amplitude,
					FromCenter			= fromCenter,
					VariationType		= variationType,
					IsReflectorBullet	= isReflectorBullet,
				} ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// エネミーを生成する：外観・ダメージ値・耐久値・Ｘ初期位置・Ｙ初期位置・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_002, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

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
				public float	Amplitude ;
				public bool		FromCenter ;
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

				float	amplitude	= settings.Amplitude ;
				bool	fromCenter	= settings.FromCenter ;


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

				// 初期の回転角度を設定
				enemy.SetAngle( velocity ) ;

//				int phase = 0 ;
//				bool autoRotation = false ;

				float time = 0 ;
				float duration = 2.0f ;


				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					// エネミーを移動させる

					float delta = enemy.Delta ;

					// 縦

					enemy.Position += velocity * delta ;

					// 横

					if( amplitude != 0 )
					{
						time += delta ;

						float factor = time % duration ;

						float radian = 2.0f * Mathf.Pi * factor / duration ;
						if( fromCenter == true )
						{
							factor = Mathf.Cos( radian ) ;
						}
						else
						{
							factor = Mathf.Sin( radian ) ;
						}

						enemy.SetRatioPositionX( amplitude * factor ) ;
					}

					//--------------------------------

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
