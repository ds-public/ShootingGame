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
		/// エネミーグループ(種別 010)
		/// </summary>
		public class EnemyGroup_010 : EnemyGroupBase
		{
			/// <summary>
			/// 横から出てきてＸ軸が合うとレーザーを撃つか特攻
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
//				variationType = 0 ;	// デバッグ

				//---------------------------------------------------------

				//-------------
				// 上下

				float xs ;
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

					ys = -0.3f ;
				}
				else
				{
					// 下

					ys = +0.3f ;
				}

				// 左右
				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 左
					xs = -0.6f ;
				}
				else
				{
					// 右
					xs = +0.6f ;
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

				l = ExMath.GetRandomRange( 4, 8 ) ;

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
					// 設定値の生成
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys + ExMath.GetRandomRange( -0.15f, +0.15f ) ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_010, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 少し待つ
					await Wait( 0.5f ) ;
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
				var endRatioPosition	= new Vector2( - settings.StartRatioPosition.X, settings.StartRatioPosition.Y ) ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 attackDirection ;

				if( startRatioPosition.Y <  0 )
				{
					// 上
					attackDirection = new Vector2(  0, +1 ) ;
				}
				else
				{
					// 下
					attackDirection = new Vector2(  0, -1 ) ;
				}

				// 向きは固定
				enemy.SetAngle( attackDirection ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;

				bool isAttacked = false ;

				// 最初の方向
				var direction = ( endRatioPosition - startRatioPosition ).Normalized() ;

				// 左右反転
				enemy.SetFlip( ( ExMath.Sign( direction.X ) * ExMath.Sign( attackDirection.Y ) >  0 ) ) ;


				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 横移動

						enemy.Position += direction * 200.0f * delta ;

						if( m_Owner.IsPlayerDestroyed == false )
						{
							float distance = Mathf.Abs( m_Owner._Player.Position.X - enemy.Position.X ) ;
							float limit ;

							if( variationType == 0 )
							{
								limit = 16 ;
							}
							else
							{
								limit = 32 ;
							}


							if( distance <  limit )
							{
								// 攻撃または特攻

								if( variationType == 0 )
								{
									// 攻撃
									if( isAttacked == false )
									{
										isAttacked  = true ;

										// 細いレーザー
										m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.LaserSlim, enemy.Position, attackDirection, 600, 4 ) ;
									}
								}
								else
								{
									// 特攻
									phase = 1 ;
								}
							}
						}
					}
					else
					if( phase == 1 )
					{
						// 特攻

						enemy.Position += attackDirection * 400 * delta ;
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
