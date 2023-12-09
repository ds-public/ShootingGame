using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;

using EaseHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		// プレイヤーの弾を生成する
		private PlayerShot CreatePlayerBullet
		(
			PlayerShotShapeTypes shapeType,
			Vector2 position, Vector2 direction, float speed,
			float duration = 0,
			EaseTypes easeType = EaseTypes.Linear,
			int damage = 1,
			PlayerShotCollisionTypes collisionType = PlayerShotCollisionTypes.Shot,
			bool isHitCheck = true,
			float correction = 0,
			bool isFlip = false,
			int processingType = 0
		)
		{
			// 生成
			var playerShot = AddChild<PlayerShot>( _PlayerShots[ ( int )shapeType ], _Screen ) ;

			// 保持
			m_Entities.Add( playerShot ) ;

			if( correction != 0 )
			{
				// 位置補正をかける
				position += direction * correction ;
			}

			// 開始
			playerShot.Start
			(
				position,
				direction,
				speed,
				duration,
				easeType,
				damage,
				collisionType,
				isHitCheck,
				OnPlayerBulletDestroy,
				this,
				isFlip,
				processingType
			) ;

			return playerShot ;
		}

		// プレイヤーの弾が破棄された際に呼び出される
		private void OnPlayerBulletDestroy( PlayerShot playerShot, Vector2 position, PlayerShotDestroyedReasonTypes destroyedReasonType )
		{
			if( playerShot.ProcessingType == 1 && destroyedReasonType == PlayerShotDestroyedReasonTypes.Self )
			{
				// グラビティボール

				// 拡散タイプのボム発生
				CreatePlayerBomb( BombTypes.Diffusion, position, 0.5f, 1.2f, m_PlayerBombDamage, false ) ;
			}

			//----------------------------------
			// いずれ削除予定

			if( playerShot.ProcessingType == 0 && destroyedReasonType == PlayerShotDestroyedReasonTypes.OutOfScreen )
			{
				// エネミーにヒットしなかった場合に命中率が下がるので更新が必要

				// 命中率を更新する
				_HUD.SetHitRateValue( m_HitCount, m_HitMaxCount ) ;
			}

			//----------------------------------------------------------

			// プレイヤーの弾を実際に破棄する
			m_Entities.Remove( playerShot ) ;
			playerShot.QueueFree() ;
		}
	}
}
