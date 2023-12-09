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
		/// エネミーグループ(種別 000)
		/// </summary>
		public class EnemyGroup_000 : EnemyGroupBase
		{
			/// <summary>
			/// パワーアップユニット
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

				float xr = ExMath.GetRandomRange( -0.4f, + 0.4f ) ;
				float yr = -0.6f ;

				//---------------------------------

				int i, l = 1 ;	// 登場数は常に１

				// 出現数が確定した時点でカウンターを更新する
				var enemyGroupCounter = new EnemyGroupCounter( this )
				{
					CountHit = 0,
					CountNow = 0,
					CountMax = l
				} ;
				owner.EnemyGroupCounters[ groupId ] = enemyGroupCounter ;

				int shield = 1 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成(全て同じ動き)
					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xr, yr )
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					owner.CreateEnemy( 0, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;
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
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				var startRatioPosition	= settings.StartRatioPosition ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				// 移動方向と移動量
				var velocity = new Vector2(   0, 200.0f ) ;

				// 初期の向き
				enemy.SetAngle( velocity ) ;

				//---------------------------------

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					enemy.Position += velocity * enemy.Delta ;

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
			private bool OnEnemyDestroyed( Enemy enemy, EnemyDestroyedReasonTypes destroyedReasonTypes )
			{
				// 実際に破壊してよい
				return true ;
			}
		}
	}
}
