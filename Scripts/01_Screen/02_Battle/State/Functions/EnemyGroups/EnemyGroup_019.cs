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
		/// エネミーグループ(種別 019)
		/// </summary>
		public class EnemyGroup_019 : EnemyGroupBase
		{
			/// <summary>
			/// 画面左右にジグザク移動で最後に戻る
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

				int[] direction_weights =
				{
					100,				// 縦
					( level / 2 )		// 横
				} ;

				int directionType = ExMath.GetRandomIndex( direction_weights ) ;
//				directionType = 1 ;	// デバッグ

				float xs ;
				float ys ;

				if( directionType == 0 )
				{
					// 縦タイプ
					directionType = 0 ;

					if( ExMath.GetRandomRange(  0, 99 ) <  70 )
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

						xs = -0.40f ;
					}
					else
					{
						// 右から

						xs = +0.40f ;
					}
				}
				else
				{
					// 横タイプ
					directionType = 1 ;

					if( ExMath.GetRandomRange(  0, 99 ) <  50 )
					{
						// 左から

						xs = -0.6f ;
					}
					else
					{
						// 右から

						xs = +0.6f ;
					}

					if( ExMath.GetRandomRange(  0, 99 ) <  70 )
					{
						// 上から

						ys = -0.40f ;
					}
					else
					{
						// 下から

						ys = +0.40f ;
					}
				}

				int[] attack_weights =
				{
					20,					// 何もしない
					( level / 2 ),		// ５０％の確率で弾を１発撃つ
					( level / 4 ),		// １００％の確率で弾を１発撃つ
					( level / 8 )		// １００％の確率で弾を３発撃つ
				} ;

				int attackType = ExMath.GetRandomIndex( attack_weights ) ;
//				attackType = 3 ;	// デバッグ

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

				l = ExMath.GetRandomRange( 10, 12 ) ;

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
				var enemyGroupCounter = new EnemyGroupCounter( this)
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
						DirectionType		= directionType,
						StartRatioPosition	= new Vector2( xs, ys ),
						AttackType			= attackType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_019, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.2f ) ;
					}
				}
				else
				{
					// ダブル

					// 設定値の生成(共通)
					var settings_0 = new Settings()
					{
						DirectionType		= directionType,
						StartRatioPosition	= new Vector2( xs, ys ),
						AttackType			= attackType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// ミラーか対角線か
					int doubleType = ExMath.GetRandomRange( 0, 1 ) ;
					if(  doubleType == 0 )
					{
						// ミラー
						if( directionType == 0 )
						{
							// 縦
							xs = -xs ;
						}
						else
						{
							// 横
							ys = -ys ;
						}
					}
					else
					{
						// 対角線
						xs = -xs ;
						ys = -ys ;
					}

					// 設定値の生成(共通)
					var settings_1 = new Settings()
					{
						DirectionType		= directionType,
						StartRatioPosition	= new Vector2( xs, ys ),
						AttackType			= attackType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_019, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_019, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

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
				public int		DirectionType ;
				public Vector2	StartRatioPosition ;
				public int		AttackType ;
				public Vector2	EndRatioPosition ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// 方向タイプ
				var directionType = settings.DirectionType ;

				var startRatioPosition	= settings.StartRatioPosition ;
				Vector2 endRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;



				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

				Vector2 turnRatioPosition ;

				float radius ;

				Vector2 amplitudeX ;
				Vector2 amplitudeY ;

				float aspect = 1 ;

				if( directionType == 0 )
				{
					// 縦

					if( startRatioPosition.Y <  0 )
					{
						// 上から下へ
						direction = new Vector2(  0, +1 ) ;
						turnRatioPosition	= new Vector2( + startRatioPosition.X, +0.45f ) ;
						endRatioPosition	= startRatioPosition ;
					}
					else
					{
						// 下から上へ
						direction = new Vector2(  0, +1 ) ;
						turnRatioPosition = new Vector2( + startRatioPosition.X, -0.45f ) ;
						endRatioPosition	= startRatioPosition ;
					}

					amplitudeX = new Vector2( ExMath.Sign( startRatioPosition.X ), 0 ) ;
					amplitudeY = Vector2.Zero ;
				}
				else
				{
					// 横
					if( startRatioPosition.X <  0 )
					{
						// 左から右へ
						direction = new Vector2( +1,  0 ) ;
						turnRatioPosition = new Vector2( +0.45f, + startRatioPosition.Y ) ;
						endRatioPosition	= startRatioPosition ;
					}
					else
					{
						// 右から左へ
						direction = new Vector2( -1,  0 ) ;
						turnRatioPosition = new Vector2( -0.45f, + startRatioPosition.Y ) ;
						endRatioPosition	= startRatioPosition ;
					}

					amplitudeY = new Vector2( 0, ExMath.Sign( startRatioPosition.Y ) ) ;
					amplitudeX = Vector2.Zero ;

//					aspect = enemy.AspectYX ;
				}

				var attackType = settings.AttackType ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				float factorA ;
				float factorS ;

				Vector2 velocity ;

				// 初期の回転角度を設定
				enemy.SetAngle( direction ) ;

				Vector2 previousRatioPosition = startRatioPosition ;
				Vector2 attackDirection ;
				float ar ;

				float amplitude = 0.05f * aspect ;	// 画面比率での振幅

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 前身

						duration = 2.4f * aspect ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factorA = Ease.GetValue( factor, EaseTypes.Linear ) ;

						velocity = turnRatioPosition - startRatioPosition ;

						factorS = Ease.GetValue( factor, EaseTypes.Linear ) ;
						factorS *= 8 ;
						
						radius = Mathf.Sin( Mathf.Pi * factorS ) ;

						enemy.RatioPosition =
							velocity * factorA + startRatioPosition +
							amplitudeX * amplitude * radius +
							amplitudeY * amplitude * radius ;

						direction = enemy.RatioPosition - previousRatioPosition ;
						enemy.SetAngle( direction ) ;

						previousRatioPosition = enemy.RatioPosition ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;


							//------------------------------
							// 弾を撃つ

							if( attackType == 1 || attackType == 2 )
							{
								if( ExMath.GetRandomRange(  0, 99 ) <  attackType * 50 )
								{
									m_Owner.CreateEnemyBullet( 0, enemy.Position, 200.0f, 1 ) ;
								}
							}
							else
							if( attackType == 3 )
							{
								attackDirection = ( m_Owner.Player.Position - enemy.Position ).Normalized() ;
								ar = ExMath.GetRandomRange( -15.0f, +15.0f ) ;
								attackDirection = ExMath.GetRotatedVector( attackDirection, ar ) ;

								m_Owner.CreateEnemyBulletMulti
								(
									EnemyBulletShapeTypes.BulletSmall, enemy.Position, attackDirection, 200.0f, 1, 0,
									3, 45.0f
								) ;
							}

							//------------------------------

							previousRatioPosition = turnRatioPosition ;
						}
					}
					else
					if( phase == 1 )
					{
						// 後退
						duration = 2.4f * aspect ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factorA = Ease.GetValue( factor, EaseTypes.Linear ) ;

						velocity = endRatioPosition - turnRatioPosition ;

						factorS = Ease.GetValue( factor, EaseTypes.Linear ) ;
						factorS *= 8 ;
						
						radius = Mathf.Sin( Mathf.Pi * factorS ) ;

						enemy.RatioPosition =
							velocity * factorA + turnRatioPosition +
							amplitudeX * amplitude * radius +
							amplitudeY * amplitude * radius ;

						direction = enemy.RatioPosition - previousRatioPosition ;
						previousRatioPosition = enemy.RatioPosition ;

						enemy.SetAngle( direction ) ;

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
