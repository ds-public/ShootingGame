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
		/// エネミーグループ(種別 027)
		/// </summary>
		public class EnemyGroup_027 : EnemyGroupBase
		{
			/// <summary>
			/// 画面中央にブロックが集まってその後プレイヤーに突っ込んでくる
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

				// バリエーション選別
				int[] variation_weights =
				{
					100,				//  前方
					( level / 2 ),		//  左右
					( level / 8 ),		//  四方
				} ;
				int variationType = ExMath.GetRandomIndex( variation_weights ) ;
//				variationType = 2 ;	// デバッグ

				//---------------------------------

				if( variationType == 0 )
				{
					// 前方

					int[] size_weights =
					{
						100,				//  7 x  5
						( level / 2 ),		//  9 x  7
						( level/  4 ),		// 11 x  9
						( level/  8 )		// 13 x 11
					} ;

					int sizeType = ExMath.GetRandomIndex( size_weights ) ;
	//				sizeType = 3 ;	// デバッグ

					int xc =  5 ;
					int yc =  3 ;

					switch( sizeType )
					{
						case 0 : xc =  7 ; yc =  5 ; break ;
						case 1 : xc =  9 ; yc =  7 ; break ;
						case 2 : xc = 11 ; yc =  9 ; break ;
						case 3 : xc = 13 ; yc = 11 ; break ;
					}

					// 開始位置
					// 集合位置
					// 開始ディレイ(0.0～1.0)
					// 維持(５秒＋ szieType * 2)
					// 終了ディレイ(0.0～1.0)
					// 突っ込み(プレイヤーが破壊されていたら↓に移動するただけ)
					// ※突っ込みは一定速度まで時間経過ごとに加速させる

					Settings[,] allSettings = new Settings[ yc, xc ] ;


					// ブロック群の全体の中心
					var center = new Vector2( 0.0f, -0.1f ) ;

					// １つあたりのサイズ(横を基準にする)　※正方形にするためアスペクト比補正を行う
					float bdx = 0.08f ;
					float bdy = bdx * m_Owner.ScreenAspectXY ;
				
					// 左上
					float xs = - ( bdx * xc * 0.5f ) + ( bdx * 0.5f ) + center.X ;
					float ys = - ( bdy * yc * 0.5f ) + ( bdy * 0.5f ) + center.Y ;

					int lx, ly ;
					float cxp, cyp ;

					float idleTime = 5.0f + sizeType * 2 ;

					//---------------------------------
					// 返し弾

					bool isReflectorBullet = false ;
					if( level >= 60 )
					{
						isReflectorBullet  = true ;
					}

					//---------------------------------

					for( ly  = 0 ; ly <  yc ; ly ++ )
					{
						for( lx  = 0 ; lx <  xc ; lx ++ )
						{
							var settings = new Settings() ;

							cxp = xs + bdx * lx ;
							cyp = ys + bdy * ly ;

							settings.CombineRatioPosition = new Vector2( cxp, cyp ) ;

							// 集合と逆ベクトル(開始位置を算出しやすくするため)
							var direction = ( settings.CombineRatioPosition - center ).Normalized() ;

							// 方向のゆらぎ
							float ra = ExMath.GetRandomRange( -20.0f, +20.0f ) ;
							direction = ExMath.GetRotatedVector( direction, ra ) ;

							// 距離のゆらぎ
							float rd = ExMath.GetRandomRange( 0.2f, 0.4f ) ;
							settings.StartRatioPosition = settings.CombineRatioPosition + direction * rd ;
						
							// 開始時間のゆらぎ
							settings.StartDelayTime = ExMath.GetRandomRange( 0.0f, 1.0f ) ;
							settings.IdleTime = idleTime ;
							settings.AttackDelayTime = ExMath.GetRandomRange( 0.0f, 2.0f ) ;

							settings.IsReflectorBullet = isReflectorBullet ;

							allSettings[ ly, lx ] = settings ;
						}
					}

					//---------------------------------

					int l = xc * yc ;

					// 出現数が確定した時点でカウンターを更新する
					var enemyGroupCounter = new EnemyGroupCounter( this )
					{
						CountHit = 0,
						CountNow = 0,
						CountMax = l
					} ;
					owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

					//---------------------------------

					// レベルによるシールド値の補正
					int shield = GetShield( 2, 5, level ) ;

					for( ly  = 0 ; ly <  yc ; ly ++ )
					{
						for( lx  = 0 ; lx <  xc ; lx ++ )
						{
							var settings = allSettings[ ly, lx ] ;

							// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
							owner.CreateEnemy( EnemyShapeTypes.No_027, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;
						}
					}
				}
				else
				if( variationType == 1 )
				{
					// 左右

					int[] size_weights =
					{
						100,				//  3 x 13
						( level /  2 ),		//  4 x 15
						( level /  4 ),		//  5 x 17
						( level /  8 )		//  6 x 19
					} ;

					int sizeType = ExMath.GetRandomIndex( size_weights ) ;
//					sizeType = 3 ;	// デバッグ

					int xc =  3 ;
					int yc = 15 ;

					switch( sizeType )
					{
						case 0 : xc =  3 ; yc = 13 ; break ;
						case 1 : xc =  4 ; yc = 15 ; break ;
						case 2 : xc =  5 ; yc = 17 ; break ;
						case 3 : xc =  6 ; yc = 19 ; break ;
					}

					// 開始位置
					// 集合位置
					// 開始ディレイ(0.0～1.0)
					// 維持(５秒＋ szieType * 2)
					// 終了ディレイ(0.0～1.0)
					// 突っ込み(プレイヤーが破壊されていたら↓に移動するただけ)
					// ※突っ込みは一定速度まで時間経過ごとに加速させる

					Settings[,,] allSettings = new Settings[ yc, xc, 2 ] ;


					// ブロック群の全体の中心(横方向は端)
					var center = new Vector2( 0.0f, 0.0f ) ;

					// １つあたりのサイズ(横を基準にする)　※正方形にするためアスペクト比補正を行う
					float bdx = 0.08f ;
					float bdy = bdx * m_Owner.ScreenAspectXY ;
				
					// 左上
					float xs = -0.48f ;
					float ys = - ( bdy * yc * 0.5f ) + ( bdy * 0.5f ) ;

					int lx, ly ;
					float cxp, cyp ;

					float idleTime = 5.0f + sizeType * 2 ;

					//---------------------------------
					// 返し弾

					bool isReflectorBullet = false ;
					if( level >= 60 )
					{
						isReflectorBullet  = true ;
					}

					//---------------------------------

					Settings settings ;
					Vector2 direction ;
					float ra, rd ;

					for( ly  = 0 ; ly <  yc ; ly ++ )
					{
						for( lx  = 0 ; lx <  xc ; lx ++ )
						{
							cxp = xs + bdx * lx ;
							cyp = ys + bdy * ly ;

							//----------
							// 左側

							settings = new ()
							{
								// 順位置
								CombineRatioPosition = new Vector2( +cxp, cyp )
							} ;

							// 集合と逆ベクトル(開始位置を算出しやすくするため)
							direction = ( settings.CombineRatioPosition - center ).Normalized() ;

							// 方向のゆらぎ
							ra = ExMath.GetRandomRange( -20.0f, +20.0f ) ;
							direction = ExMath.GetRotatedVector( direction, ra ) ;

							// 距離のゆらぎ
							rd = ExMath.GetRandomRange( 0.2f, 0.4f ) ;
							settings.StartRatioPosition = settings.CombineRatioPosition + direction * rd ;
						
							// 開始時間のゆらぎ
							settings.StartDelayTime = ExMath.GetRandomRange( 0.0f, 1.0f ) ;
							settings.IdleTime = idleTime ;
							settings.AttackDelayTime = ExMath.GetRandomRange( 0.0f, 2.0f ) ;

							settings.IsReflectorBullet = isReflectorBullet ;

							allSettings[ ly, lx, 0 ] = settings ;

							//------------------------------

							// 右側
							settings = new ()
							{
								// 逆位置
								CombineRatioPosition = new Vector2( -cxp, cyp )
							} ;

							// 集合と逆ベクトル(開始位置を算出しやすくするため)
							direction = ( settings.CombineRatioPosition - center ).Normalized() ;

							// 方向のゆらぎ
							ra = ExMath.GetRandomRange( -20.0f, +20.0f ) ;
							direction = ExMath.GetRotatedVector( direction, ra ) ;

							// 距離のゆらぎ
							rd = ExMath.GetRandomRange( 0.2f, 0.4f ) ;
							settings.StartRatioPosition = settings.CombineRatioPosition + direction * rd ;
						
							// 開始時間のゆらぎ
							settings.StartDelayTime = ExMath.GetRandomRange( 0.0f, 1.0f ) ;
							settings.IdleTime = idleTime ;
							settings.AttackDelayTime = ExMath.GetRandomRange( 0.0f, 2.0f ) ;

							settings.IsReflectorBullet = isReflectorBullet ;

							allSettings[ ly, lx, 1 ] = settings ;
						}
					}

					//---------------------------------

					int l = xc * yc * 2 ;

					// 出現数が確定した時点でカウンターを更新する
					var enemyGroupCounter = new EnemyGroupCounter( this )
					{
						CountHit = 0,
						CountNow = 0,
						CountMax = l
					} ;
					owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

					//---------------------------------

					// レベルによるシールド値の補正
					int shield = GetShield( 2, 5, level ) ;

					for( ly  = 0 ; ly <  yc ; ly ++ )
					{
						for( lx  = 0 ; lx <  xc ; lx ++ )
						{
							settings = allSettings[ ly, lx, 0 ] ;

							// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
							owner.CreateEnemy( EnemyShapeTypes.No_027, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

							settings = allSettings[ ly, lx, 1 ] ;

							// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
							owner.CreateEnemy( EnemyShapeTypes.No_027, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;
						}
					}
				}
				else
				{
					// 四方

					int[] size_weights =
					{
						100,				//  9 x 15
						( level /  2 ),		//  7 x 13
						( level /  4 ),		//  5 x 11
						( level /  8 )		//  3 x  9
					} ;

					int sizeType = ExMath.GetRandomIndex( size_weights ) ;
//					sizeType = 3 ;	// デバッグ

					int xc = 13 ;
					int yc = 19 ;

					int scx =  9 ;
					int scy = 13 ;

					switch( sizeType )
					{
						case 0 : scx =  9 ; scy = 15 ; break ;
						case 1 : scx =  7 ; scy = 13 ; break ;
						case 2 : scx =  5 ; scy = 11 ; break ;
						case 3 : scx =  3 ; scy =  9 ; break ;
					}

					int sx0 = ( xc / 2 ) - ( scx / 2 ) ;
					int sx1 = ( xc / 2 ) + ( scx / 2 ) ;
					int sy0 = ( yc / 2 ) - ( scy / 2 ) ;
					int sy1 = ( yc / 2 ) + ( scy / 2 ) ;


					// 開始位置
					// 集合位置
					// 開始ディレイ(0.0～1.0)
					// 維持(５秒＋ szieType * 2)
					// 終了ディレイ(0.0～1.0)
					// 突っ込み(プレイヤーが破壊されていたら↓に移動するただけ)
					// ※突っ込みは一定速度まで時間経過ごとに加速させる

					Settings[,] allSettings = new Settings[ yc, xc ] ;

					// ブロック群の全体の中心
					var center = new Vector2( 0.0f, 0.0f ) ;

					// １つあたりのサイズ(横を基準にする)　※正方形にするためアスペクト比補正を行う
					float bdx = 0.08f ;
					float bdy = bdx * m_Owner.ScreenAspectXY ;
				
					// 左上
					float xs = - ( bdx * xc * 0.5f ) + ( bdx * 0.5f ) + center.X ;
					float ys = - ( bdy * yc * 0.5f ) + ( bdy * 0.5f ) + center.Y ;

					int lx, ly ;
					float cxp, cyp ;

					float idleTime = 5.0f + sizeType * 2 ;

					//---------------------------------
					// 返し弾

					bool isReflectorBullet = false ;
					if( level >= 60 )
					{
						isReflectorBullet  = true ;
					}

					//---------------------------------

					for( ly  = 0 ; ly <  yc ; ly ++ )
					{
						for( lx  = 0 ; lx <  xc ; lx ++ )
						{
							// くり抜いた内側は除外する
							if( ( lx >= sx0 && lx <= sx1 && ly >= sy0 && ly <= sy1 ) == false )
							{
								var settings = new Settings() ;

								cxp = xs + bdx * lx ;
								cyp = ys + bdy * ly ;

								settings.CombineRatioPosition = new Vector2( cxp, cyp ) ;

								// 集合と逆ベクトル(開始位置を算出しやすくするため)
								var direction = ( settings.CombineRatioPosition - center ).Normalized() ;

								// 方向のゆらぎ
								float ra = ExMath.GetRandomRange( -20.0f, +20.0f ) ;
								direction = ExMath.GetRotatedVector( direction, ra ) ;

								// 距離のゆらぎ
								float rd = ExMath.GetRandomRange( 0.2f, 0.4f ) ;
								settings.StartRatioPosition = settings.CombineRatioPosition + direction * rd ;
						
								// 開始時間のゆらぎ
								settings.StartDelayTime = ExMath.GetRandomRange( 0.0f, 1.0f ) ;
								settings.IdleTime = idleTime ;
								settings.AttackDelayTime = ExMath.GetRandomRange( 0.0f, 2.0f ) ;

								settings.IsReflectorBullet = isReflectorBullet ;

								allSettings[ ly, lx ] = settings ;
							}
						}
					}

					//---------------------------------

					int l = ( xc * yc ) - ( scx * scy ) ;

					// 出現数が確定した時点でカウンターを更新する
					var enemyGroupCounter = new EnemyGroupCounter( this )
					{
						CountHit = 0,
						CountNow = 0,
						CountMax = l
					} ;
					owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

					//---------------------------------

					// レベルによるシールド値の補正
					int shield = GetShield( 2, 5, level ) ;

					for( ly  = 0 ; ly <  yc ; ly ++ )
					{
						for( lx  = 0 ; lx <  xc ; lx ++ )
						{
							// くり抜いた内側は除外する
							if( ( lx >= sx0 && lx <= sx1 && ly >= sy0 && ly <= sy1 ) == false )
							{
								var settings = allSettings[ ly, lx ] ;

								// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
								owner.CreateEnemy( EnemyShapeTypes.No_027, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;
							}
						}
					}
				}

				// 少し待つ
				await Wait( 0 ) ;

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
				public Vector2	CombineRatioPosition ;
				public float	StartDelayTime ;
				public float	IdleTime ;
				public float	AttackDelayTime ;
				public bool		IsReflectorBullet ;
			}
			// 開始位置
			// 集合位置
			// 開始ディレイ(0.0～1.0)
			// 維持(５秒＋ szieType * 2)
			// 終了ディレイ(0.0～1.0)
			// 突っ込み(プレイヤーが破壊されていたら↓に移動するただけ)
			// ※突っ込みは一定速度まで時間経過ごとに加速させる


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				// 初期位置
				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 合体位置
				var combineRatioPosition = settings.CombineRatioPosition ;

				// 開始ディレイ時間
				var startDelayTime = settings.StartDelayTime ;

				// 維持時間
				var idleTime = settings.IdleTime ;

				// 攻撃ディレイ時間
				var attackDelayTime = settings.AttackDelayTime ;

				//---------------------------------
				// 移動量と画面外判定情報

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				float factorP ;
				float factorA ;

				var velocity = new Vector2(  0, +1 ) ;

				// 初期の方向は全て同じ
				enemy.SetAngle( new Vector2(  0, +1 ) ) ;

				// コリジョン無効
				enemy.SetCollisionEnabled( false ) ;

				// 最初は見えない
				enemy.SetMonochromaticMode( true, 1, 0xFF000000 ) ;
				enemy.Alpha = 0 ;

				float rotationAngle = 0 ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 開始ディレイ

						duration = startDelayTime ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 合体開始へ
							phase = 1 ;
							time = 0 ;
						}
					}
					else
					if( phase == 1 )
					{
						// 合体開始
						duration = 1f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factorP = Ease.GetValue( factor, EaseTypes.EaseInOutQuad ) ;
						factorA = Ease.GetValue( factor, EaseTypes.Linear ) ;

						velocity = combineRatioPosition - startRatioPosition ;

						enemy.RatioPosition = velocity * factorP + startRatioPosition ;
						enemy.Alpha = factorA ;
						enemy.SetInterpolation( 1 - factorA ) ;

						if( factor >= 1 )
						{
							// 一定時間維持へ
							phase = 2 ;
							time = 0 ;

							// コリジョン有効
							enemy.SetCollisionEnabled( true ) ;

							enemy.SetMonochromaticMode( false ) ;
							enemy.Alpha = 1 ;
						}
					}
					else
					if( phase == 2 )
					{
						// 一定時間維持
						duration = idleTime ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 攻撃ディレイへ
							phase = 3 ;
							time = 0 ;
						}
					}
					else
					if( phase == 3 )
					{
						// 攻撃ディレイ
						duration = attackDelayTime ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						if( factor >= 1 )
						{
							// 攻撃へ
							phase = 4 ;
							time = 0 ;

							if( m_Owner.IsPlayerDestroyed == false )
							{
								velocity = ( m_Owner.Player.Position - enemy.Position ).Normalized() ;
							}
							else
							{
								velocity = new Vector2(  0, +1 ) ;
							}

							velocity *= 600.0f ;	// 最終的には結構速い

							// 回転方向をランダムで決める
							if( ExMath.GetRandomRange(  0, 99 ) <  50 )
							{
								// 右
								rotationAngle = +10 ;
							}
							else
							{
								// 左
								rotationAngle = -10 ;
							}
						}
					}
					else
					if( phase == 4 )
					{
						// 攻撃

						duration = 1.0f ;	// １秒で本来の最高速度に到達

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						enemy.Position += ( velocity * factor * delta ) ;

						enemy.Rotation += ( rotationAngle * factor * delta ) ;	// 回転アタック
					}

					//--------------------------------

					// １フレーム待つ(この間にエネミーが破壊されたかコンバットが終了したらタスクキャンセルされる)
					await Yield( cancellationToken: linkedToken ) ;

					//------------

					if( IsOutOfScreen( enemy, 1.2f ) == true )
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
