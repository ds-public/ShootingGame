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
		/// エネミーグループ(種別 032)
		/// </summary>
		public class EnemyGroup_032 : EnemyGroupBase
		{
			/// <summary>
			/// 円移動
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

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 左から
					xs = -0.45f ;
				}
				else
				{
					// 右から
					xs = +0.45f ;
				}

				//---------------------------------
				// ダブル表示にするかどうか

				int[] double_weights =
				{
					100,
					( level / 2 )
				} ;

				bool isDouble = ExMath.GetRandomIndex( double_weights ) != 0 ;
				isDouble = true ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
				if( level >= 60 )
				{
					isReflectorBullet  = true ;
				}

				//---------------------------------

				int i, l = 0 ;

				l = ExMath.GetRandomRange(  5,  5 ) ;

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

					// 設定値の生成(共通)
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_008, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.12f ) ;
					}
				}
				else
				{
					// ダブル(対角のみ)

					// 設定値の生成(共通)
					var settings_0 = new Settings()
					{
						StartRatioPosition	= new Vector2(  xs,  ys ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// 設定値の生成(共通)
					var settings_1 = new Settings()
					{
						StartRatioPosition	= new Vector2( -xs, - ys ),
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_008, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_008, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 0.12f ) ;
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
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

				Vector2 baseRatioPosition ;

				float radius = 0.4f ;

				if( startRatioPosition.Y <  0 )
				{
					// 上から
					baseRatioPosition = new Vector2(  0, - radius ) ;
				}
				else
				{
					// 下から
					baseRatioPosition = new Vector2(  0, + radius ) ;
				}

				Vector2 previousRatioPosition = startRatioPosition ;

				// 終了位置
				var endRatioPosition = new Vector2( - startRatioPosition.X, startRatioPosition.Y ) ;

				// 色味を設定する
				enemy.SetColor( new Color( 0, 0, 1, 1 ) ) ;


				float signX = - ExMath.Sign( startRatioPosition.X ) ;
				float signY =   ExMath.Sign( startRatioPosition.Y ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;
				float factorX ;
				float factorY ;

				Vector2 velocity ;

				float xp, yp ;
				float cv, sv ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 出現

						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factorX = Ease.GetValue( factor, EaseTypes.Linear ) ;
						factorY = Ease.GetValue( factor, EaseTypes.EaseInOutQuad ) ;


						velocity = baseRatioPosition - startRatioPosition ;

						xp = velocity.X * factorX ;
						yp = velocity.Y * factorY ;

						enemy.RatioPosition = new Vector2( xp, yp ) + startRatioPosition ;
						
						//-----------

						direction = ( enemy.RatioPosition - previousRatioPosition ).Normalized() ;
						
						// 回転角度を設定
						enemy.SetAngle( direction ) ;

						previousRatioPosition = enemy.RatioPosition ;

						//-------------------------------------------------------

						if( factor >= 1 )
						{
							// 一回転へ
							phase = 1 ;
							time = 0 ;
						}
					}
					else
					if( phase == 1 )
					{
						// 一回転

						duration = 1.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						cv = Mathf.Cos( 2.0f * Mathf.Pi * factor ) ;
						sv = Mathf.Sin( 2.0f * Mathf.Pi * factor ) ;

						xp = radius * sv * signX ;
						yp = radius * cv * signY ;

						enemy.RatioPosition = new Vector2( xp, yp ) ;

						//-----------

						direction = ( enemy.RatioPosition - previousRatioPosition ).Normalized() ;
						
						// 回転角度を設定
						enemy.SetAngle( direction ) ;

						previousRatioPosition = enemy.RatioPosition ;

						if( factor >= 1 )
						{
							// 退却へ
							phase = 2 ;
							time = 0 ;

							baseRatioPosition = enemy.RatioPosition ;
						}
					}
					else
					{
						// 退却

						duration = 0.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factorX = Ease.GetValue( factor, EaseTypes.Linear ) ;
						factorY = Ease.GetValue( factor, EaseTypes.EaseInOutQuad ) ;


						velocity = endRatioPosition -  baseRatioPosition ;

						xp = velocity.X * factorX ;
						yp = velocity.Y * factorY ;

						enemy.RatioPosition = new Vector2( xp, yp ) + baseRatioPosition ;
						
						//-----------

						direction = ( enemy.RatioPosition - previousRatioPosition ).Normalized() ;
						
						// 回転角度を設定
						enemy.SetAngle( direction ) ;

						previousRatioPosition = enemy.RatioPosition ;

						//-------------------------------------------------------

						if( factor >= 1 )
						{
							// 終了
							break ;
						}
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( enemy, 0.8f ) == true )
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
