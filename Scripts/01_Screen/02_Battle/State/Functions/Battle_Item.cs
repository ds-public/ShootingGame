using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		// 複数のアイテムを生成する
		private Item[] CreateItems( ItemShapeTypes[] shapeTypes, Vector2 position, float angle = 0 )
		{
			if( shapeTypes == null || shapeTypes.Length == 0 )
			{
				// 生成しない
				return null ;
			}

			Item[] items = new Item[ shapeTypes.Length ] ;

			if( shapeTypes.Length == 1 )
			{
				// 単数生成
				items[ 0 ] = CreateItem( shapeTypes[ 0 ], position, angle ) ;
			}
			else
			{
				// 複数生成

				// ２つ以上の場合は指定位置を中心として周囲のランダム位置に出現する
				int i, l = shapeTypes.Length ;
				float a = 16.0f + l * 16.0f ;	// 数が多いほどランダムの範囲が広がる
				float xp, yp ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					xp = ExMath.GetRandomRange( -a, +a ) ;
					yp = ExMath.GetRandomRange( -a, +a ) ;

					items[ i ] = CreateItem( shapeTypes[ i ], position + new Vector2( xp, yp ), angle ) ;
				}
			}

			return items ;
		}

		// アイテムを生成する
		private Item CreateItem( ItemShapeTypes shapeType, Vector2 position, float angle = 0 )
		{
			var item = AddChild<Item>( _Item, _Screen ) ;

			m_Entities.Add( item ) ;

			item.Start( shapeType, position, angle, OnItemDestroy, this ) ;

			return item ;
		}

		//-----------------------------------

		// アイテムの種類ごとに入手時の効果音を変える
		private static string[] m_ItemGetSeNames = { SE.Item_P, SE.Item_S, SE.Item_B, SE.Item_B, SE.Item_B } ;

		// アイテムが破棄(取得)された際に呼び出される
		private void OnItemDestroy( Item item, Vector2 position, bool isPlayerHit )
		{
			if( isPlayerHit == true )
			{
				if( item.IsFake == false )
				{
					// 本物

					var shapeType = item.ShapeType ;

					// 入手ＳＥを鳴らす
					m_CombatAudio.PlaySe( m_ItemGetSeNames[ ( int )shapeType ], pan: item.RatioPosition.X ) ;

					switch( shapeType )
					{
						// パワー
						case ItemShapeTypes.Power :
							if( m_PlayerPower <  m_PlayerPowerMax )
							{
								m_PlayerPower ++ ;

								// ショットスピートゲージの表示設定
								_HUD.SetShotSpeed( PlayerShotSpeedEnabled, PlayerShotSpeedRate ) ;

								if( m_PlayerPower >= m_PlayerPowerTop )
								{
									// オーラエフェクト有効化
									_Player.SetAura( true ) ;
								}
							}
							else
							{
								// 余剰分はスコアになる
								AddScore( 500 ) ;
							}
						break ;

						// シールド
						case ItemShapeTypes.Shield :
							if( m_PlayerShield <  m_PlayerShieldMax )
							{
								m_PlayerShield ++ ;
							
	//							float shiledActiveRate ;
								if( _Player.IsShieldActive == false )
								{
									// 現在被ダメージ後の無敵状態でなければ最後の無敵時間は更新しておく
									m_PlayerShieldActiveNow = GetPlayerShieldActive() ;
	//
	//								shiledActiveRate = 1.0f ;
								}
	//							else
	//							{
	//								// 変動中    
	//								shiledActiveRate = _Player.ShieldActiveRate ;
	//							}

								_HUD.SetShieldPod
								(
									m_PlayerShield,
									_Player.ShieldActiveRate * m_PlayerShieldActiveNow / m_PlayerShieldActiveMax
								) ;
							}
							else
							{
								// 余剰分はスコアになる
								AddScore( 500 ) ;
							}
						break ;

						// ボム
						case ItemShapeTypes.Bomb_Compression :
						case ItemShapeTypes.Bomb_Diffusion :
						case ItemShapeTypes.Bomb_Emission :
							if( m_PlayerBombStocks.Count >= m_PlayerBombMax )
							{
								// 余剰分は捨てられる
								m_PlayerBombStocks.RemoveAt( 0 ) ;
							}

							//-------------------------------

							BombTypes bombType = BombTypes.Compression ;

							switch( shapeType )
							{
								case ItemShapeTypes.Bomb_Compression	: bombType = BombTypes.Compression	; break ;
								case ItemShapeTypes.Bomb_Diffusion		: bombType = BombTypes.Diffusion	; break ;
								case ItemShapeTypes.Bomb_Emission		: bombType = BombTypes.Emission		; break ;
							}
							
							m_PlayerBombStocks.Add( bombType ) ;

							_HUD.SetBombStock( m_PlayerBombStocks, m_PlayerBombCursor, _Player.BombCooldownRate ) ;
						break ;
					}
				}
				else
				{
					// 偽物
					OnPlayerDamage( position, 1 ) ;

					// 爆発演出
					CreateExplosion( position, 1 ) ;
				}
			}

			//----------------------------------

			// アイテムを実際に破棄する
			m_Entities.Remove( item ) ;
			item.QueueFree() ;
		}
	}
}
