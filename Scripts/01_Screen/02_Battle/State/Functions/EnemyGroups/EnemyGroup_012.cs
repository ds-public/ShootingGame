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
		/// エネミーグループ(種別 012)
		/// </summary>
		public class EnemyGroup_012 : EnemyGroupBase
		{
			/// <summary>
			/// 外周を短い∪字移動
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

				int variationType ;

				float xs ;
				float ys ;

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 縦タイプ
					variationType = 0 ;

					if( ExMath.GetRandomRange(  0, 99 ) <  50 )
					{
						// 上から

						ys = -0.55f ;
					}
					else
					{
						// 下から

						ys = +0.55f ;
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
					variationType = 1 ;

					if( ExMath.GetRandomRange(  0, 99 ) <  50 )
					{
						// 左から

						xs = -0.55f ;
					}
					else
					{
						// 右から

						xs = +0.55f ;
					}

					if( ExMath.GetRandomRange(  0, 99 ) <  50 )
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
						StartRatioPosition	= new Vector2( xs, ys ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_012, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 0.1f ) ;
					}
				}
				else
				{
					// ダブル

					// 設定値の生成(共通)
					var settings_0 = new Settings()
					{
						StartRatioPosition	= new Vector2( xs, ys ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// ミラーか対角線か
					int doubleType = ExMath.GetRandomRange( 0, 1 ) ;
					if(  doubleType == 0 )
					{
						// ミラー
						if( variationType == 0 )
						{
							// 縦
							ys = -ys ;
						}
						else
						{
							// 横
							xs = -xs ;
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
						StartRatioPosition	= new Vector2( xs, ys ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_012, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_012, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 0.1f ) ;
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



				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 velocity ;	// １秒あたりの移動量

				float rx, ry, signX, signY, radius ;


				if( variationType == 0 )
				{
					// 縦

					if( startRatioPosition.Y <  0 )
					{
						// 上から下へ
						velocity = new Vector2(  0, +1 ) ;
					}
					else
					{
						// 下から上へ
						velocity = new Vector2(  0, +1 ) ;
					}

					radius = Mathf.Abs( startRatioPosition.X ) ;
				}
				else
				{
					// 横
					if( startRatioPosition.X <  0 )
					{
						// 左から右へ
						velocity = new Vector2( +1,  0 ) ;
					}
					else
					{
						// 右から左へ
						velocity = new Vector2( -1,  0 ) ;
					}

					radius = Mathf.Abs( startRatioPosition.Y ) ;
				}

				signX = ExMath.Sign( startRatioPosition.X ) ;
				signY = ExMath.Sign( startRatioPosition.Y ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;
				float radian ;


				// 初期の回転角度を設定
				enemy.SetAngle( velocity ) ;

				Vector2 p0 = startRatioPosition ;
				Vector2 p1 ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 半円運動
						duration = 1.4f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor = Ease.GetValue( factor, EaseTypes.Linear ) ;

						radian = Mathf.Pi * factor ;

						rx =   Mathf.Cos( radian ) ;
						ry = - Mathf.Sin( radian ) ;

						if( variationType == 0 )
						{
							// 縦

							p1 = new Vector2( rx * signX * radius, ry * signY * radius ) ;

							p1.Y += startRatioPosition.Y ;

							enemy.SetAngle( p1 - p0 ) ;

							enemy.RatioPosition = p1 ;

							p0 = p1 ;
						}
						else
						{
							// 横

							p1 = new Vector2( ry * signX * radius, rx * signY * radius ) ;

							p1.X += startRatioPosition.X ;

							enemy.SetAngle( p1 - p0 ) ;

							enemy.RatioPosition = p1 ;

							p0 = p1 ;
						}

						if( factor >= 1 )
						{
							phase = 9 ;
							time = 0 ;
						}
					}
					else
					if( phase == 9 )
					{
						// 終了
						break ;
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
