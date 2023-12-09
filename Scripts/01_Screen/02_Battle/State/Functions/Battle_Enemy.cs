#pragma warning disable CA1069

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
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// エネミーを生成する
		/// </summary>
		/// <param name="enemyShapeType"></param>
		/// <param name="damage"></param>
		/// <param name="shield"></param>
		/// <param name="xr"></param>
		/// <param name="yr"></param>
		/// <param name="enemyGroupId"></param>
		/// <param name="onUpdate"></param>
		public Enemy CreateEnemy
		(
			EnemyShapeTypes shapeType,
			int damage,
			int shield,
			int score,
			int groupId,
			Func<Enemy,CancellationToken,Task>			onUpdate,
			Func<Enemy,EnemyDestroyedReasonTypes,bool>	onDestroyed,
			System.Object settings,
			int level,
			bool isFlip = false,
			bool isBoss = false
		)
		{
			var enemy = AddChild<Enemy>( _Enemies[ ( int )shapeType ], _Screen ) ;
			m_Entities.Add( enemy ) ;

			enemy.Start
			(
				damage,
				shield,
				score,
				groupId,
				onUpdate,
				onDestroyed,
				settings,
				this,
				level,
				isFlip,
				m_CombatFinishedTokenSource.Token
			) ;

			// 念のためコリジョン設定
			if( isBoss == false )
			{
				// ボス無効：プレイヤーの弾とボムに当たる
				enemy.CollisionMask = 0x00000022 ;
			}
			else
			{
				// ボス有効：プレイヤーの弾のみに当たる
				enemy.CollisionMask = 0x00000002 ;
			}

			return enemy ;
		}

		//-----------------------------------

		// エネミーを破棄する際に呼び出される
		public void OnEnemyDestroyed( Enemy enemy, Vector2 position, EnemyDestroyedReasonTypes destroyedReasonType )
		{
			//----------------------------------

			// 破壊理由がプレイヤー由来かどうか
			bool fromPlayer = false ;
			if( destroyedReasonType == EnemyDestroyedReasonTypes.PlayerShot || destroyedReasonType == EnemyDestroyedReasonTypes.PlayerBomb )
			{
				fromPlayer = true ;
			}

			if( fromPlayer == true )
			{
				// プレイヤーのショットまたはボムがヒットした

				if( enemy.ExplosionScale >  0 )
				{
					// 爆発を生成する
					CreateExplosionMulti( position, enemy.ExplosionTimes, enemy.ExplosionScale ) ;
				}

				//----------------------------------

				if( enemy.Score >  0 )
				{
					// スコア更新
					AddScore( enemy.Score ) ;
				}
				
				// 命中率を更新する(後で削除予定)
				m_HitCount ++ ;
				_HUD.SetHitRateValue( m_HitCount, m_HitMaxCount ) ;

				// 撃破率を更新する
				m_CrashCount ++ ;
			}

			//----------------------------------------------------------
			// エネミーグループの消滅判定

			int groupId = enemy.GroupId ;

			if( m_EnemyGroupCounters.ContainsKey( groupId ) == true )
			{
				// グループカウンター取得
				var enemyGroupCounter = m_EnemyGroupCounters[ groupId ] ;
				
				if( fromPlayer == true )
				{
					// プレイヤーによって破壊された
					enemyGroupCounter.CountHit ++ ;
				}
				enemyGroupCounter.CountNow ++ ;

				// グループ情報
				var enemyGroup = enemyGroupCounter.EnemyGroup ;

				//---------------------------------
				// 個体のアイテム出現

				if( fromPlayer == true )
				{
					if( enemyGroup.ItemAmount >  0 )
					{
						// 必ずパワーアップアイテムが出現する

						if( enemyGroup.ItemAmount == 1 )
						{
							// 破壊したエネミーの位置に出現する
							CreateItem( PickupItem(), position ) ;
						}
						else
						{
							// ２つ以上の場合はエネミーの位置を中心とて周囲のランダム位置に出現する
							CreateItems( PickupItems( enemyGroup.ItemAmount ), position ) ;
						}
					}
				}

				//---------------------------------
				// グループの消滅・殲滅の判定

				if( enemyGroupCounter.IsFinished == true )
				{
					// グループは消滅した
					if( enemyGroupCounter.IsCompleted == true )
					{
						// グループを殲滅させた

						if( enemyGroup.ItemAmount == 0 )	// マイナス値指定でアイテムは出現しない
						{
							// グループのエネミー数が多い程アイテムの出現率が上がる
							int ratioMax = enemyGroup.ItemBaseAvarage * enemyGroupCounter.CountMax ;
							int ratioNow = ExMath.GetRandomRange(  0, 99 ) ;

							if( ratioNow <  ratioMax )
							{
								// アイテム出現
								CreateItem( PickupItem(), position ) ;
							}
						}
					}

					// このグループのエネミーは全て消滅
					m_EnemyGroupCounters.Remove( groupId ) ;
				}
			}

			//----------------------------------------------------------

			// エネミーが消えるタイミングで出現率をカウントする
			m_CrashMaxCount ++ ;

			// 撃破率はエネミーが画面外に消えたタイミングでも更新する
			_HUD.SetCrashRateValue( m_CrashCount, m_CrashMaxCount ) ;

			//----------------------------------------------------------

			// エネミーを実際に破棄する
			m_Entities.Remove( enemy ) ;
			enemy.QueueFree() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アイテムの出現比率の重み
		/// </summary>
		private static int[] m_ItemWeights =
		{
			20,
			20,
			10, 10, 10,
		} ;

		// 入手アイテムの選出を行う
		private ItemShapeTypes[] PickupItems( int amount )
		{
			if( amount <= 0 )
			{
				return null ;
			}

			var items = new ItemShapeTypes[ amount ] ; 

			int i, l = amount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				items[ i ] = PickupItem() ;
			}

			return items ;
		}

		// 入手アイテムの選出を行う
		private ItemShapeTypes PickupItem()
		{
			ItemShapeTypes shapeType = ItemShapeTypes.Power ;

			int itemIndex = ExMath.GetRandomIndex( m_ItemWeights ) ;
			
			switch( itemIndex )
			{
				case 0 : shapeType = ItemShapeTypes.Power				; break ;
				case 1 : shapeType = ItemShapeTypes.Shield				; break ;
				case 2 : shapeType = ItemShapeTypes.Bomb_Compression	; break ;
				case 3 : shapeType = ItemShapeTypes.Bomb_Diffusion		; break ;
				case 4 : shapeType = ItemShapeTypes.Bomb_Emission		; break ;
			}

			return shapeType ;
		}
	}
}
