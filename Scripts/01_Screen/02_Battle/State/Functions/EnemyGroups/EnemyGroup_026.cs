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
		/// エネミーグループ(種別 026)
		/// </summary>
		public class EnemyGroup_026 : EnemyGroupBase
		{
			/// <summary>
			/// 機雷(爆発時に弾を撒き散らす)
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
					100,		// 上から
					level		// 下から
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
					50,					// 　４
					( level / 2 ),		// 　８
					( level / 4 ),		// １２
					( level / 8 )		// １６
				} ;

				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 3 ;	// デバッグ

				//---------------------------------
				// ダブル表示にするかどうか

				int[] double_weights =
				{
					200,
					( level / 8 )
				} ;

				bool isDouble = ExMath.GetRandomIndex( double_weights ) != 0 ;
//				isDouble = true ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
//				if( level >= 60 )
//				{
//					isReflectorBullet  = true ;
//				}

				//---------------------------------

				int i, l = 0 ;

				l = ExMath.GetRandomRange( 12, 16 ) ;

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
				int shield = GetShield( 2, 4, level ) ;

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
						owner.CreateEnemy( EnemyShapeTypes.No_026, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.6f ) ;
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
						owner.CreateEnemy( EnemyShapeTypes.No_026, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_026, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 0.6f ) ;
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

				// 初期位置
				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				//---------------------------------

				Vector2 velocity ;

				float signY = ExMath.Sign( startRatioPosition.Y ) ;

				velocity = new Vector2( 0, -200.0f * signY ) ;

				float limitY = ( - ExMath.GetRandomRange( 0.15f, 0.45f ) * signY ) ;

				//---------------------------------

//				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration = 1.0f ;
				float factor ;

				float radian ;
				float scale ;

				// 初期の回転角度を設定
				enemy.SetAngle( velocity ) ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					enemy.Position += velocity * delta ;

					// スケールを調整する
					factor = ( time % duration ) / duration ;
					radian = Mathf.Cos( 2.0f * Mathf.Pi * factor ) ;

					scale = radian * 0.2f + 0.8f ;

					enemy.Scale = new Vector2( scale, scale ) ;

					//--------------------------------

					if( m_Owner.IsPlayerDestroyed == false )
					{
						// 爆発判定
						if( startRatioPosition.Y <  0 )
						{
							// 上から
							if( enemy.RatioPosition.Y >  limitY )
							{
								// 爆発実行
								enemy.SelfDestroy() ;
							}
						}
						else
						{
							// 下から
							if( enemy.RatioPosition.Y <  limitY )
							{
								// 爆発実行
								enemy.SelfDestroy() ;
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
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;
	

				//---------------------------------

				if( destroyedReasonType == EnemyDestroyedReasonTypes.Self )
				{
					// 自爆の場合は弾をばらまく

					// 爆発を生成する
					m_Owner.CreateExplosion( enemy.Position ) ;

					//--------------------------------------------------------

					var attackType = settings.AttackType ;

					// 弾数
					int amount = 4 + 4 * attackType ;

					// 基準方向をランダムに設定

					float baseRotation = ExMath.GetRandomRange( -180.0f, +180.0f ) ;
					Vector2 baseDirection = ExMath.GetRotatedVector( new Vector2(  0, +1 ), baseRotation ) ;
					Vector2 direction ;

					float angle = 0 ;
					float additionalAngle = 360.0f / amount ;
				
					float speed ;

					int i, l = amount ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						direction = ExMath.GetRotatedVector( baseDirection, angle ) ;
						speed = ExMath.GetRandomRange( 200.0f, 360.0f ) ;

						m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.BulletSmall, enemy.Position, direction, speed, 1 ) ;

						angle += additionalAngle ;
					}
				}

				//---------------------------------

				// 実際に破壊してよい
				return true ;
			}
		}
	}
}
