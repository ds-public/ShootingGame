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
		/// エネミーグループ(種別 016)
		/// </summary>
		public class EnemyGroup_016 : EnemyGroupBase
		{
			/// <summary>
			/// アステロイド
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

				float yrs, yre ;
				if( variationType == 0 )
				{
					yrs = -0.6f ;
					yre = +0.6f ;
				}
				else
				{
					yrs = +0.6f ;
					yre = -0.6f ;
				}

				//---------------------------------
				// 返し弾

				bool isReflectorBullet = false ;
//				if( level >= 50 )
//				{
//					isReflectorBullet  = true ;
//				}

				//---------------------------------

				int i, l = ExMath.GetRandomRange( 16, 20 ) ;

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
				int shield = GetShield(  8, 16, level ) ;

				float xrs, xre, speed ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					// 設定値の生成
					xrs = ExMath.GetRandomRange( -0.6f, +0.6f ) ;
					xre = ExMath.GetRandomRange( -0.6f, +0.6f ) ;

					speed = ExMath.GetRandomRange( 120.0f, 180.0f ) ;

					var settings = new Settings()
					{
						StartRatioPosition	= new Vector2( xrs, yrs ),
						EndRatioPosition	= new Vector2( xre, yre ),
						Speed				= speed,	
						VariationType		= variationType,
						IsReflectorBullet	= isReflectorBullet,
					} ;

					// エネミーを生成する：外観・ダメージ値・耐久値・グループ識別子
					var enemy = owner.CreateEnemy( EnemyShapeTypes.No_016, 2, shield, 100, groupId, OnEnemyUpdate, OnEnemyDestroyed, settings, level ) ;

					// 爆発演出補正
					enemy.ExplosionScale = 1.5f ;
					enemy.ExplosionTimes = 1 ;

					// 少し待つ
					await Wait( 1.2f ) ;
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
				public float	Speed ;
				public int		VariationType ;
				public bool		IsReflectorBullet ;
			}


			// エネミーの動作を処理する
			private async Task OnEnemyUpdate( Enemy enemy, CancellationToken linkedToken )
			{
				// 設定情報を取り出す
				var settings = enemy.Settings as Settings ;

				var startRatioPosition	= settings.StartRatioPosition ;
				var endRatioPosition	= settings.EndRatioPosition ;

				var speed				= settings.Speed ;

				// 初期位置を設定する
				enemy.RatioPosition = startRatioPosition ;

				Vector2 direction = endRatioPosition - startRatioPosition ;

				float rotationAngle = 0 ;

				if( direction.X <  0 )
				{
					rotationAngle = -1 ;
				}
				else
				if( direction.X >  0 )
				{
					rotationAngle = +1 ;
				}

				rotationAngle *= 2.0f * Mathf.Pi * ( speed * 0.1f ) / 360.0f ;

				//---------------------

				// 初期の向き
				enemy.SetAngle( direction ) ;

				// 大きさにゆらぎ

				float scale  = ExMath.GetRandomRange( 0.8f, 1.6f ) ;				
				enemy.SetScale( scale ) ;

				//---------------------------------

				float delta ;

				while( true  )	// 画面内の座標割合値で位置を判定する
				{
					//--------------------------------

					delta = enemy.Delta ;

					enemy.Position += direction * speed * delta ;
					enemy.Rotation += rotationAngle * delta ;

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
