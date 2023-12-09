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
		// ボムを生成する
		private PlayerBomb CreatePlayerBomb
		(
			BombTypes bombType,
			Vector2 position,
			float scale,
			float duration,
			int	damage,
			bool isHitCheck,
			bool isFlip = false,
			int processingType = 0,
			bool isSe = true
		)
		{
			// 生成と追加
			var playerBomb = AddChild<PlayerBomb>( _Bomb, _Screen ) ;

			// 生成中エンティティに登録
			m_Entities.Add( playerBomb ) ;

			//----------------------------------

			// 開始設定
			playerBomb.Start
			(
				position,
				scale,
				duration,
				damage,	// 与ダメージ値
				isHitCheck,
				OnPlayerBombAttack,
				OnPlayerBombDestroy,
				this,
				isFlip,
				processingType
			) ;

			if( isSe == true )
			{
				// 効果音
				CombatAudio.PlaySe( SE.Bomb, playerBomb.RatioPosition.X ) ;
			}

			//----------------------------------

			return playerBomb ;
		}

		// ボムが対象を攻撃した際に呼び出される(基本は一撃破壊)
		private void OnPlayerBombAttack( PlayerBomb playerBomb, Vector2 position, Node target )
		{
			// カウントはエネミーの破壊コールバック側でカウントしているのでこちらでカウントする必要は無い
		}

		// ボムが破棄された際に呼び出される
		private void OnPlayerBombDestroy( PlayerBomb playerBomb, Vector2 position )
		{
			if( playerBomb.ProcessingType == 2 )
			{
				// 放射タイプのボム
				CreateEmissionBomb( position, 1 ) ;
			}

			//----------------------------------

			// 爆発を実際に破棄する
			m_Entities.Remove( playerBomb ) ;
			playerBomb.QueueFree() ;
		}
	}
}
