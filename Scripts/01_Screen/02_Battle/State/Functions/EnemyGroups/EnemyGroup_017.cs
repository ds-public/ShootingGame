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
		/// エネミーグループ(種別 017)
		/// </summary>
		public class EnemyGroup_017 : EnemyGroupBase
		{
			/// <summary>
			/// 大量出現し直進して途中でプレイヤーに向かって弾を撃つ
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
					100,					// 弾
					( level / 4 ),			// ホーミング
				} ;

				int variationType = ExMath.GetRandomIndex( variation_weights ) ;
//				variationType = 1 ;	// デバッグ

				int[] amount_weights =
				{
					100,					// 多い
					( level / 4 ),			// 少ない
				} ;

				int amountType = ExMath.GetRandomIndex( amount_weights ) ;
//				amountType = 0 ;	// デバッグ


				//---------------------------------------------------------

				//-------------
				// 上下

				int[] position_weights =
				{
					100,			// 上から
					( level / 4 ),	// 下から
				} ;

				int positionType = ExMath.GetRandomIndex( position_weights ) ;

				float ys ;

				if( positionType == 0 )
				{
					// 上から下に

					ys = -0.6f ;
				}
				else
				{
					// 下から上に

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

				l = ExMath.GetRandomRange(  9, 18 ) ;

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
					float xs = ExMath.GetRandomRange( -0.45f, +0.45f ) ;

					// 設定値の生成
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						VariationType		= variationType,
						AmountType			= amountType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_017, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 少し待つ
					await Wait( 0.4f ) ;
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
				public int		AmountType ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// バリエーションタイプ
				var variationType	= settings.VariationType ;
				var amountType		= settings.AmountType ;

				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				Vector2 direction ;

				//---------------------------------

				if( startRatioPosition.Y <  0 )
				{
					// 上から下へ
					direction = new Vector2(  0, +1 ) ;
				}
				else
				{
					// 下から上へ
					direction = new Vector2(  0, -1 ) ;
				}

				//---------------------------------

//				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;

				bool isAttacked = false ;

				// 初期の回転角度を設定
				enemy.SetAngle( direction ) ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					enemy.Position += direction * 400.0f * delta ;

					if( isAttacked == false && m_Owner.IsPlayerDestroyed == false )
					{
						if
						(
							( startRatioPosition.Y <  0 && enemy.RatioPosition.Y >   0.1f ) ||
							( startRatioPosition.Y >  0 && enemy.RatioPosition.Y <  -0.1f )
						)
						{
							isAttacked = true ;

							if( variationType == 0 )
							{
								if( amountType == 0 )
								{
									// 小さい弾
									m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.BulletSmall, enemy.Position, 220.0f, 1 ) ;
								}
								else
								{
									// 複数同時の小さい弾
									m_Owner.CreateEnemyBulletMulti
									(
										EnemyBulletShapeTypes.BulletSmall, enemy.Position, 220.0f, 1, 0,
										3, 30.0f
									) ;
								}
							}
							else
							{
								m_Owner.CreateEnemyBullet
								(
									EnemyBulletShapeTypes.Missile, enemy.Position, 360.0f, 
									5, 1,
									0.05f, 20.0f, 1.0f,
									false
								) ;
							}
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
