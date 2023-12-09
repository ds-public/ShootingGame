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
		/// エネミーグループ(種別 003)
		/// </summary>
		public class EnemyGroup_003 : EnemyGroupBase
		{
			/// <summary>
			/// 高速に特攻かゆっくり誘導
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
					100,					// 特攻
					 50 + ( level / 2 ),	// 誘導
				} ;

				int variationType = ExMath.GetRandomIndex( weights ) ;
//				variationType = 1 ;	// デバッグ

				//---------------------------------------------------------

				float speed = 0 ;

				// 左右

				float xr = 0 ;

				//-------------
				// 上下

				float yr ;

				if( variationType == 0 )
				{
					// 特攻

					// 上

					yr = -0.6f ;

					speed = 800.0f ;
				}
				else
				{
					// 誘導

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

					speed = 300.0f ;
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

				if( variationType == 0 )
				{
					// 特攻
					l = ExMath.GetRandomRange( 10, 20 ) ;
				}
				else
				{
					// 誘導
					l = ExMath.GetRandomRange(  5,  8 ) ;
				}

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

				for( i  = 0 ; i <  l ; i ++ )
				{
					xr = ExMath.GetRandomRange( -0.4f, + 0.4f ) ;

					// 設定値の生成
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xr, yr ),
						Speed				= speed,
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・Ｘ初期位置・Ｙ初期位置・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_003, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

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
				public float	Speed ;
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

				// 速度を取得する
				float speed = settings.Speed ;

				var variationType = settings.VariationType ;


				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 velocity ;	// １秒あたりの移動量

				float limit_yr = 0 ;

				if( startRatioPosition.Y <  0 )
				{
					// 上から下
					limit_yr = +0.6f ;
				}
				else
				if( startRatioPosition.Y >  0 )
				{
					// 下から上
					limit_yr = -0.6f ;
				}

				if( limit_yr == 0 )
				{
					// 異常
					enemy.OutOfScreen() ;
					return ;
				}

				//---------------------------------

//				int phase = 0 ;
//				bool autoRotation = false ;

				float time = 0 ;
				float duration = 0.2f ;

				// 最初の方向
				var direction = ( m_Owner._Player.Position - enemy.Position ).Normalized() ;
				velocity = direction * speed ;

				// 初期の回転角度を設定
				enemy.SetAngle( velocity ) ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					// エネミーを移動させる

					float delta = enemy.Delta ;
					enemy.Position += ( velocity * delta ) ;

					if( variationType == 1 && m_Owner.IsPlayerDestroyed == false )
					{
						// 誘導

						time += delta ;

						if( time >= duration )
						{
							time = 0 ;

							// 方向調整
							direction = ( m_Owner._Player.Position - enemy.Position ).Normalized() ;
							velocity = direction * speed ;

							// 初期の回転角度を設定
							enemy.SetAngle( velocity ) ;
						}
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
