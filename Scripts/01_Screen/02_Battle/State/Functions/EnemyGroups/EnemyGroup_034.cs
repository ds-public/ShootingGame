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
		/// エネミーグループ(種別 034)
		/// </summary>
		public class EnemyGroup_034 : EnemyGroupBase
		{
			/// <summary>
			/// 弾を当てると拡大していく
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

				int[] attack_weights =
				{
					100,				// 無し
					( level / 8 ),		// ホーミング弾
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

				bool isReflectorBullet = false ;	// 無し
//				if( level >= 60 )
//				{
//					isReflectorBullet  = true ;
//				}

				//---------------------------------

				int i, l = 0 ;

				l = ExMath.GetRandomRange(  8, 10 ) ;

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
				int shield = GetShield( 10, 20, level ) ;

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
						owner.CreateEnemy( EnemyShapeTypes.No_034, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

						// 少し待つ
						await Wait( 2.4f ) ;
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
						owner.CreateEnemy( EnemyShapeTypes.No_034, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level ) ;

						// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
						owner.CreateEnemy( EnemyShapeTypes.No_034, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level ) ;

						// 少し待つ
						await Wait( 2.4f ) ;
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

				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				//---------------------------------
				// 移動量と画面外判定情報

				Vector2 direction ;	// 基本移動方向

				if( startRatioPosition.Y <  0 )
				{
					// 上から
					direction = new Vector2(  0, +1 ) ;
				}
				else
				{
					// 下から
					direction = new Vector2(  0, -1 ) ;
				}

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				// 初期の回転角度を設定(固定)
				enemy.SetAngle( new Vector2(  0, +1 ) ) ;

				float speed = 180.0f ;
				float scale ;

				float radius = 32.0f ;
				float xs, xa ;

				// 初期の横位置
				xs = enemy.Position.X ;

				float signX ;
				if( ExMath.GetRandomRange(  0, 99 ) <  0 )
				{
					signX = -1 ;
				}
				else
				{
					signX = + 1 ;
				}

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 移動のみ

						// 軽く蛇行
						duration = 4.0f ;
						if( time >  duration )
						{
							time  = duration ; 
						}
						factor = time / duration ;

						xa = radius * Mathf.Sin( 2.0f * Mathf.Pi * factor ) ;

						enemy.Position += ( direction * speed * delta ) ;
						enemy.SetPositionX( xs + ( xa * signX ) ) ;

						//-------------------------------

						if( enemy.ShieldRatio <  1.0f && enemy.ShieldRatio >  0.0f )
						{
							// ダメージに応じて拡大していく
							
							scale = 1.0f + 1.5f * ( 1.0f - enemy.ShieldRatio ) ;
							enemy.SetScale( scale ) ;
						}

						if( factor >= 1 )
						{
							time = 0 ;
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

					//--------------------------------------------------------
					// プレイヤーショットによって破壊された場合のみ
					
					if( ExMath.GetRandomRange(  0, 99 ) <  18 )
					{
						// アイテム
						m_Owner.CreateItem( m_Owner.PickupItem(), enemy.Position ) ;
					}
					else
					{
						// アタック
						var item = m_Owner.CreateItem( m_Owner.PickupItem(), enemy.Position, 180.0f ) ;
						item.SetFake( true ) ;

						if( settings.AttackType >= 1 )
						{
							// 大きいエネルキーホーミング
							m_Owner.CreateEnemyBullet
							(
								EnemyBulletShapeTypes.BulletLarge, enemy.Position, 300.0f,
								3, 0,
								0.01f, 10.0f, 1.8f
							) ;
						}
					}

					//--------------------------------------------------------
				}
	
				// 実際に破壊してよい
				return true ;
			}
		}
	}
}
