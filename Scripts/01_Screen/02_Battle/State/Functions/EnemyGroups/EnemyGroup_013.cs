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
		/// エネミーグループ(種別 013)
		/// </summary>
		public class EnemyGroup_013 : EnemyGroupBase
		{
			/// <summary>
			/// 逆矢印のような軌跡
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
					  100,		//  上から
					  level,	//  下から
				} ;

				int variationType = ExMath.GetRandomIndex( variation_weights ) ;
//				variationType = 1 ;	// デバッグ

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
				if( level >= 50 )
				{
					isReflectorBullet  = true ;
				}

				//---------------------------------

				int i, l = ExMath.GetRandomRange( 4, 6 ) ;

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
				int shield = GetShield( 2, 4, level ) ;

				bool isFlip = ( variationType == 1 ) ;


				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(左)
					var settings_0 = new Settings()
					{
						StartRatioPosition	= new Vector2( -0.55f, 0.0f ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_013, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_0, level,  isFlip ) ;

					// 設定値の生成(右)
					var settings_1 = new Settings()
					{
						StartRatioPosition	= new Vector2( +0.55f, 0.0f ),
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( EnemyShapeTypes.No_013, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings_1, level, !isFlip ) ;

					// 少し待つ
					await Wait( 0.2f ) ;
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

				//---------------------

				Vector2 turnRatioPosition ;
				Vector2 endRatioPosition ;

				float signX = ExMath.Sign( startRatioPosition.X ) ;

				if( variationType == 0 )
				{
					// 上から

					turnRatioPosition	= new Vector2( 0.036f * signX, -0.45f ) ;
					endRatioPosition	= new Vector2( turnRatioPosition.X, +0.6f ) ;
				}
				else
				{
					// 下から

					turnRatioPosition = new Vector2( 0.036f * signX, +0.45f ) ;
					endRatioPosition	= new Vector2( turnRatioPosition.X, -0.6f ) ;
				}

				Vector2 velocity = turnRatioPosition - startRatioPosition ;

				// 初期の向き
				enemy.SetAngle( velocity ) ;
				enemy.SetFlip( true ) ;

				//---------------------------------

				int phase = 0 ;
//				bool autoRotation = false ;

				float delta ;
				float time = 0 ;
				float duration ;
				float factor ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;
					time += delta ;

					if( phase == 0 )
					{
						// 頂点への移動

						duration = 0.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor= Ease.GetValue( factor, EaseTypes.Linear ) ;

						enemy.RatioPosition = velocity * factor + startRatioPosition ;

						if( factor >= 1 )
						{
							phase = 1 ;
							time = 0 ;

							velocity = endRatioPosition -  turnRatioPosition ;

							enemy.SetAngle( velocity ) ;
							enemy.SetFlip( false ) ;
						}
					}
					else
					if( phase == 1 )
					{
						// 少し待つ
						duration = 1.8f ;

						if( time >  duration )
						{
							time  = duration ;
						}
						factor = time / duration ;

						factor= Ease.GetValue( factor, EaseTypes.EaseInBack ) ;

						enemy.RatioPosition = velocity * factor + turnRatioPosition ;

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
