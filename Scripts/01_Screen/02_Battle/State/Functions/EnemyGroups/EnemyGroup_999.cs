using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		/// <summary>
		/// エネミーグループ(種別 999)
		/// </summary>
		public class EnemyGroup_999 : EnemyGroupBase
		{
			/// <summary>
			/// ボーナス隠しキャラ
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

				float xr = ExMath.GetRandomRange( -0.45f, +0.45f ) ;
				float yr ;

				if( ExMath.GetRandomRange(  0, 99 ) <  70 )
				{
					// 上から
					yr = -0.6f ;
				}
				else
				{
					// 下から
					yr = +0.6f ;
				}

				//---------------------------------------------------------

				int i, l = 1 ;	// 必ず１つだけ

				// 出現数が確定した時点でカウンターを更新する
				var enemyGroupCounter = new EnemyGroupCounter( this )
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = l
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				//---------------------------------------------------------

				// レベルによるシールド値の補正
				int shield = GetShield( 16, 32, level ) ;

				// 設定値の生成
				var settings = new Settings()
				{
					StartRatioPosition	= new Vector2( xr, yr ),
				} ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// エネミーを生成する：外観・ダメージ値・耐久値・Ｘ初期位置・Ｙ初期位置・グループ識別子
					var bonusEnemy = owner.CreateEnemy( EnemyShapeTypes.No_999, 0, shield, 10000, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level, isBoss:true ) ;	// ボスにするとボム無効になる

					// ユニット用のコリジョン設定
					bonusEnemy.SetCollisionType( EnemyCollisionTypes.Unit ) ;
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
				public bool		IsCompleted ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				var startRatioPosition = settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 移動方向と移動量
				Vector2 velocity ;
				
				if( startRatioPosition.Y <  0 )
				{
					// 上から下へ
					velocity = new Vector2(   0,  180.0f ) ;
				}
				else
				{
					// 下から上へ
					velocity = new Vector2(   0, -180.0f ) ;
				}

				// 初期の向き(常に下向き)
				enemy.SetAngle( new Vector2(  0, +1 ) ) ;

				// 単色化
				enemy.SetMonochromaticMode( true, 1, 0x04000000 ) ;
//				enemy.SetMonochromaticMode( true, 1, 0xFF000000 ) ;	// デバッグ

				//---------------------------------

				int phase = 0 ;
				float time = 0 ;
				float factor ;
				float duration ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					if( settings.IsCompleted == false )
					{
						// 移動
						enemy.Position += velocity * enemy.Delta ;
					}
					else
					{
						// 待機と消去
						time += enemy.Delta ;

						if( phase == 0 )
						{
							// 待機
							duration = 2.2f ;
							if( time >  duration )
							{
								time  = duration ;
							}
							factor = time / duration ;

							if( factor >= 1 )
							{
								phase = 1 ;
								time = 0 ;
							}
						}
						else
						if( phase == 1 )
						{
							// 消去
							duration = 0.5f ;
							if( time >  duration )
							{
								time  = duration ;
							}
							factor = time / duration ;

							enemy.Alpha = 1 - factor ;

							if( factor >= 1 )
							{
								break ;
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
				if( destroyedReasonType == EnemyDestroyedReasonTypes.PlayerShot || destroyedReasonType == EnemyDestroyedReasonTypes.PlayerBomb )
				{
					// 設定情報を取り出す
					var settings = enemy.Settings as Settings ;

					// ボーナスゲット
					settings.IsCompleted = true ;

					//---------------------------------------------------------

					// スコア加算
					m_Owner.AddScore( enemy.Score ) ;

					// 画面にボーナス表示を出す
					m_Owner._HUD.ShowBonus( enemy.Score ) ;

					//---------------------------------

					// 原色化
					enemy.SetMonochromaticMode( false ) ;

					// コリジョン無効化
					enemy.SetCollisionEnabled( false ) ;
				}

				// 破壊はしない(画面外に出て消去する)
				return false ;
			}
		}
	}
}
