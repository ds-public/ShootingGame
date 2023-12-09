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
		/// エネミーグループ(種別 005)
		/// </summary>
		public class EnemyGroup_005 : EnemyGroupBase
		{
			/// <summary>
			/// Ｓ字で中央で弾を撃つ
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
					   50,		// １ウェイ
					level,		// ３ウェイ
					level / 2	// ５ウェイ
				} ;

				int variationType = ExMath.GetRandomIndex( weights ) ;
//				variationType = 1 ;	// デバッグ

				//---------------------------------------------------------

				// 上下

				float ys ;
				float yc ;
				float ye ;

				if( ExMath.GetRandomRange(  0, 99 ) <  20 )
				{
					// 上から下

					ys = -0.6f ;
					yc = -0.1f ;

				}
				else
				{
					// 下から下

					ys = +0.6f ;
					yc = -0.1f ;
				}

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					ye = - ys ;
				}
				else
				{
					ye =   ys ;
				}

				// 左右

				float xs ;
				float xc  ;
				float xe ;

				if( ExMath.GetRandomRange(  0, 99 ) <  50 )
				{
					// 左から右
					xs = -0.4f ;
					xc =  0.0f ;
					xe = +0.4f ;
				}
				else
				{
					// 右から左
					xs = +0.4f ;
					xc =  0.0f ;
					xe = -0.4f ;
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

				l = ExMath.GetRandomRange( 3, 6 ) ;

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
				int shield = GetShield( 1, 3, level ) ;

				// 設定値の生成(全て同じ動き)
				var settings = new Settings()
				{
					StartRatioPosition	= new Vector2( xs, ys ),
					CenterRatioPosition	= new Vector2( xc, yc ),
					EndRatioPosition	= new Vector2( xe, ye ),
					VariationType		= variationType,
					IsReflectorBullet	= isReflectorBullet,
				} ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_005, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 少し待つ
					await Wait( 0.8f ) ;
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
				public Vector2	CenterRatioPosition ;
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
				var centerRatioPosition	= settings.CenterRatioPosition ;
				var endRatioPosition	= settings.EndRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 向きは終始下向き
				enemy.SetAngle( new Vector2(  0, +1 ) ) ;


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

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				// 最初の方向
				var direction = ( endRatioPosition - startRatioPosition ).Normalized() ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					// エネミーを移動させる

					if( phase == 0 )
					{
						// 出現

						duration = 1.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						velocity = centerRatioPosition - startRatioPosition ;

						velocity.X *= Ease.GetValue( factor, EaseTypes.Linear ) ;
						velocity.Y *= Ease.GetValue( factor, EaseTypes.EaseOutQuad ) ;

						enemy.RatioPosition = startRatioPosition + velocity ;

						if( factor >= 1 )
						{
							// 弾発射
							int way = 1 ;
							if( variationType == 1 )
							{
								way = 3 ;
							}
							else
							if( variationType == 2 )
							{
								way = 5 ;
							}
							m_Owner.CreateEnemyBulletMulti
							(
								EnemyBulletShapeTypes.BulletSmall, enemy.Position, 200.0f, 1, 0, way, 20.0f
							) ;

							phase = 2 ;
							time = 0 ;
						}
					}
					else
					if( phase == 2 )
					{
						// 戻る
						
						duration = 1.5f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						velocity = endRatioPosition - centerRatioPosition ;

						velocity.X *= Ease.GetValue( factor, EaseTypes.Linear ) ;
						velocity.Y *= Ease.GetValue( factor, EaseTypes.EaseInQuad ) ;

						enemy.RatioPosition = centerRatioPosition + velocity ;

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
