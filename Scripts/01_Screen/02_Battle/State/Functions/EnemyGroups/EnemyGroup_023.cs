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
		/// エネミーグループ(種別 023)
		/// </summary>
		public class EnemyGroup_023 : EnemyGroupBase
		{
			/// <summary>
			/// 前後の左右から２次関数の動きで頂点で弾撃ち(1・3・ホーミング)
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

				float xp ;

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 左から出る
					xp = -1 ;
				}
				else
				{
					// 右から出る
					xp = +1 ;
				}

				int[] attack_weights =
				{
					100,				// １方向弾
					( level / 2 ),		// ３方向弾
					( level / 4 )		// 追尾弾
				} ;

				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 1 ;	// デバッグ

				//---------------------------------
				// ダブル表示にするかどうか

				int[] double_weights =
				{
					100,
					( level / 2 )
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

				l = ExMath.GetRandomRange(  3,  6 ) ;

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

					for( i  = 0 ; i <  l ; i ++ )
					{
						float xr = ExMath.GetRandomRange( 0.3f, 0.7f ) * xp ;
						float amplitude = ExMath.GetRandomRange( 0.35f, 0.65f ) ;

						// 設定値の生成(共通)
						var settings = new Settings()
						{
							StartRatioPosition	= new Vector2( xr, yr ),
							AttackType			= attackType,
							Amplitude			= amplitude,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_023, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.2f ) ;
					}
				}
				else
				{
					// ダブル

					for( i  = 0 ; i <  l ; i ++ )
					{
						float xr = ExMath.GetRandomRange( 0.3f, 0.7f ) * xp ;
						float amplitude = ExMath.GetRandomRange( 0.35f, 0.65f ) ;

						// 設定値の生成(共通)
						var settings_0 = new Settings()
						{
							StartRatioPosition	= new Vector2( xr, yr ),
							Amplitude			= amplitude,
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						float xs, ys ;

						// ミラーか対角線か
						int doubleType = ExMath.GetRandomRange( 0, 1 ) ;
						if(  doubleType == 0 )
						{
							// ミラー
							xs =  xr ;
							ys = -yr ;
						}
						else
						{
							// 対角線
							xs = -xr ;
							ys = -yr ;
						}

						// 設定値の生成(共通)
						var settings_1 = new Settings()
						{
							StartRatioPosition	= new Vector2( xs, ys ),
							Amplitude			= amplitude,
							AttackType			= attackType,
							IsReflectorBullet	= isReflectorBullet,
						} ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_023, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_023, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

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
				public float	Amplitude ;
				public int		AttackType ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// 攻撃タイプ
				var attackType = settings.AttackType ;

				// 初期位置
				var startRatioPosition	= settings.StartRatioPosition ;

				// 縦の振幅
				float amplitude = settings.Amplitude ;


				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

				if( startRatioPosition.Y <  0 )
				{
					direction = new Vector2(  0, +1 ) ;
				}
				else
				{
					direction = new Vector2(  0, -1 ) ;
				}

				// 初期の回転角度を設定
				enemy.SetAngle( direction ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				Vector2 previousRatioPosition = startRatioPosition ;

				float signX = - ExMath.Sign( startRatioPosition.X ) ;
				float signY = - ExMath.Sign( startRatioPosition.Y ) ;


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

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						float xa = 1 - factor ;
						float ya = amplitude - ( xa * xa * amplitude ) ;

						enemy.RatioPosition = startRatioPosition + new Vector2( ( factor * 0.5f ) * signX, ya * signY ) ;

						// 表示の向き
						direction = enemy.RatioPosition - previousRatioPosition ;
						enemy.SetAngle( direction ) ;

						previousRatioPosition = enemy.RatioPosition ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;

							//------------------------------
							// 弾を撃つ

							if( ExMath.GetRandomRange(  0,  99 ) <  50 )
							{
								if( attackType == 0 )
								{
									m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.BulletSmall, enemy.Position, 200.0f, 1 ) ;
								}
								else
								if( attackType == 1 )
								{
									m_Owner.CreateEnemyBulletMulti
									(
										EnemyBulletShapeTypes.BulletSmall, enemy.Position, 200.0f, 1, 0,
										3, 30.0f
									) ;
								}
								else
								if( attackType == 2 )
								{
									m_Owner.CreateEnemyBullet( EnemyBulletShapeTypes.Missile, enemy.Position, 400.0f, 3, 2, 0.05f, 30.0f, 2.0f ) ;
								}
							}
						}
					}
					else
					if( phase == 1 )
					{
						// 後退

						duration = 0.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						float xa = factor ;
						float ya = amplitude - ( xa * xa * amplitude ) ;

						enemy.RatioPosition = startRatioPosition + new Vector2( ( 0.5f + factor * 0.4f ) * signX, ya * signY ) ;

						// 表示の向き
						direction = enemy.RatioPosition - previousRatioPosition ;
						enemy.SetAngle( direction ) ;

						previousRatioPosition = enemy.RatioPosition ;

						if( factor >= 1 )
						{
							break ;
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
