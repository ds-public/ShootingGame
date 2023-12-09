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
		// 爆発を複数生成する
		private void CreateExplosionMulti( Vector2 position, int times, float scale = 1 )
		{
			if( times <= 1 )
			{
				CreateExplosion( position, scale, 0 ) ;
			}
			else
			{
				float x, y ;

				float rx = 64.0f * scale ;
				float ry = 64.0f * scale ;

				float delay = 0 ;

				int i, l = times ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					x = ExMath.GetRandomRange( - rx, + rx ) ;
					y = ExMath.GetRandomRange( - ry, + ry ) ;

					CreateExplosion( position + new Vector2( x, y ), scale, delay ) ;
					delay += 0.1f ;
				}
			}
		}

		// 爆発を生成する
		private void CreateExplosion( Vector2 position, float scale = 1, float delay = 0 )
		{
			// 爆発を生成・追加する
			var explosion = AddChild<Explosion>( _Explosion, _Screen ) ;

			// 生成中エンティティに追加する(まとめて削除する時のため)
			m_Entities.Add( explosion ) ;

			//----------------------------------

			// 爆発を開始させる
			explosion.Start
			(
				position,
				scale,
				delay,
				OnExplosionDestroy,
				this
			) ;
		}

		// 爆発が破棄された際に呼び出される
		private void OnExplosionDestroy( Explosion explosion, Vector2 position )
		{
			// 爆発を実際に破棄する
			m_Entities.Remove( explosion ) ;
			explosion.QueueFree() ;
		}
	}
}
