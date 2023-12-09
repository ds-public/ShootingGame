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
		// プレイヤーのオプションを生成する
		private PlayerOption CreatePlayerOption
		(
			Vector2 position, Vector2 direction, float speed,
			int damage,
			float correction = 0
		)
		{
			// 生成
			var playerOption = AddChild<PlayerOption>( _PlayerOptions[ 0 ], _Screen ) ;

			// 保持
			m_Entities.Add( playerOption ) ;

			if( correction != 0 )
			{
				// 位置補正をかける
				position += direction * correction ;
			}

			// 開始
			playerOption.Start
			(
				position,
				direction,
				speed,
				damage,
				OnPlayerOptionDestroy,
				this
			) ;

			// インスタンスを返す
			return playerOption ;
		}

		// プレイヤーのオプションが破棄された際に呼び出される
		private void OnPlayerOptionDestroy( PlayerOption node, Vector2 position )
		{
			// プレイヤーのオプションを実際に破棄する
			m_Entities.Remove( node ) ;
			node.QueueFree() ;
		}
	}
}
