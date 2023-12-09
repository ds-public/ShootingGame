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
		/// エネミーの弾を生成する(プレイヤーの方向に向ける)
		/// </summary>
		/// <param name="shapeType"></param>
		/// <param name="position"></param>
		/// <param name="speed"></param>
		/// <param name="damage"></param>
		public void CreateEnemyBullet
		(
			EnemyBulletShapeTypes shapeType, Vector2 position, float speed,
			int damage,
			int shield = 0,
			float rotationIntervalTime = 0, float rotationLimitAngle = 0, float homingLimitTime = 0,
			bool isFlip = false, float correction = 0
		)
		{
			if( IsPlayerDestroyed == true )
			{
				// 念のため確認
				return ;
			}

			//----------------------------------

			var v0 = position ;
			var v1 = _Player.Position ;

			// プレイヤーの方向
			var direction = ( v1 - v0 ).Normalized() ;

			CreateEnemyBullet( shapeType, position, direction, speed, damage, shield, rotationIntervalTime, rotationLimitAngle, homingLimitTime, isFlip, correction ) ;
		}

		/// <summary>
		/// エネミーの弾を複数同時に生成する(プレイヤーの方向に向ける)
		/// </summary>
		/// <param name="shapeType"></param>
		/// <param name="position"></param>
		/// <param name="speed"></param>
		/// <param name="damage"></param>
		/// <param name="way"></param>
		/// <param name="angle"></param>
		public void CreateEnemyBulletMulti
		(
			EnemyBulletShapeTypes shapeType, Vector2 position, float speed,
			int damage,
			int shield,
			int way, float angle,
			float rotationIntervalTime = 0, float rotationLimitAngle = 0, float homingLimitTime = 0,
			bool isFlip = false, float correction = 0
		)
		{
			if( IsPlayerDestroyed == true )
			{
				// 念のため確認
				return ;
			}

			//----------------------------------

			var v0 = position ;
			var v1 = Player.Position ;

			// プレイヤーの方向
			var direction = ( v1 - v0 ).Normalized() ;

			CreateEnemyBulletMulti
			(
				shapeType, position, direction, speed, damage, shield,
				way, angle,				
				rotationIntervalTime, rotationLimitAngle, homingLimitTime,
				isFlip, correction
			) ;
		}

		/// <summary>
		/// エネミーの弾を複数同時に生成する
		/// </summary>
		/// <param name="shapeType"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="speed"></param>
		/// <param name="damage"></param>
		/// <param name="way"></param>
		/// <param name="angle"></param>
		public void CreateEnemyBulletMulti
		(
			EnemyBulletShapeTypes shapeType, Vector2 position, Vector2 direction, float speed,
			int damage,
			int shield,
			int way, float angle,
			float rotationIntervalTime = 0, float rotationLimitAngle = 0, float homingLimitTime = 0,
			bool isFlip = false, float correction = 0
		)
		{
			if( ( way & 1 ) != 0 )
			{
				// 奇数

				CreateEnemyBullet( shapeType, position, direction, speed, damage, shield, rotationIntervalTime, rotationLimitAngle, homingLimitTime, isFlip, correction ) ;

				if( way <= 1 )
				{
					return ;
				}

				int i, l = way / 2 ;
				float a = angle ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					CreateEnemyBullet( shapeType, position, ExMath.GetRotatedVector( direction, +a ), speed, damage, shield, rotationIntervalTime, rotationLimitAngle, homingLimitTime, isFlip, correction ) ;
					CreateEnemyBullet( shapeType, position, ExMath.GetRotatedVector( direction, -a ), speed, damage, shield, rotationIntervalTime, rotationLimitAngle, homingLimitTime, isFlip, correction ) ;

					a += angle ;
				}
			}
			else
			{
				// 偶数

				int i, l = way / 2 ;
				float a = angle * 0.5f ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					CreateEnemyBullet( shapeType, position, ExMath.GetRotatedVector( direction, +a ), speed, damage, shield, rotationIntervalTime, rotationLimitAngle, homingLimitTime, isFlip, correction ) ;
					CreateEnemyBullet( shapeType, position, ExMath.GetRotatedVector( direction, -a ), speed, damage, shield, rotationIntervalTime, rotationLimitAngle, homingLimitTime, isFlip, correction ) ;

					a += angle ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// エネミーの弾を生成する
		/// </summary>
		/// <param name="shapeType"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="speed"></param>
		/// <param name="damage"></param>
		/// <param name="shield"></param>
		/// <param name="isFlip"></param>
		/// <param name="correction"></param>
		public void CreateEnemyBullet
		(
			EnemyBulletShapeTypes shapeType, Vector2 position, Vector2 direction, float speed,
			int damage,
			int shield = 0,
			float rotationIntervalTime = 0, float rotationLimitAngle = 0, float homingLimitTime = 0,
			bool isFlip = false, float correction = 0
		)
		{
			if( IsPlayerDestroyed == true )
			{
				// 念のため確認
				return ;
			}

			//----------------------------------

			// 生成
			var enemyBullet = AddChild<EnemyBullet>( _EnemyBullets[ ( int )shapeType ], _Screen ) ;

			// 保持
			m_Entities.Add( enemyBullet ) ;

			// 補正
			if( correction >  0 )
			{
				// 位置補正を行う
				position += direction * correction ;
			}

			// 開始
			enemyBullet.Start
			(
				position,
				direction,
				speed,
				rotationIntervalTime, rotationLimitAngle, homingLimitTime,	// 追尾
				damage,
				shield,
				OnEnemyBulletDestroyed,
				this,
				isFlip
			) ;
		}

		//-----------------------------------------------------------

		// エネミーの弾が破棄された際に呼び出される
		private void OnEnemyBulletDestroyed( EnemyBullet enemyBullet, Vector2 position, bool fromPlayerAttacked )
		{
			if( enemyBullet.IsBreakable == true && fromPlayerAttacked == true )
			{
				// プレイヤーの行動によって破壊された場合は爆発を生成する
				CreateExplosion( position ) ;
			}

			//----------------------------------

			// エネミーの弾を実際に破棄する
			m_Entities.Remove( enemyBullet ) ;
			enemyBullet.QueueFree() ;
		}
	}
}
