using Godot ;
using System ;

namespace Sample_001
{
	/// <summary>
	/// 算術のヘルパー
	/// </summary>
	public partial class ExMath
	{
		/// <summary>
		/// 外積値を取得する
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float Cross( Vector2 a, Vector2 b )
		{
			return a.Cross( b ) ;
		}

		/// <summary>
		/// Ａに対するＢの方向を取得する
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float Angle( Vector2 a, Vector2 b )
		{
			return Mathf.Asin( Cross( a, b ) ) ;
		}

		/// <summary>
		/// 指定のベクトルを回転させたベクトルを取得する(正＝右回転・負＝左回転)
		/// </summary>
		/// <param name="main"></param>
		/// <param name="degree"></param>
		/// <returns></returns>
		public static Vector2 GetRotatedVector( Vector2 direction, float degree )
		{
			float radian = Mathf.Pi * degree / 180.0f ;

			float x = direction.X ;
			float y = direction.Y ;

			float cv = Mathf.Cos( radian ) ;
			float sv = Mathf.Sin( radian ) ;

			direction.X = x * cv - y * sv ;
			direction.Y = x * sv + y * cv ;

			return direction ;
		}

		/// <summary>
		/// １つ目のベクトルに対して２つ目のベクトルの相対的な角度をラジアン(-PAI～+PAI)で返す
		/// </summary>
		/// <param name="normal"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static float GetRativeAngle( Vector2 normal, Vector2 direction, bool isDegree = false )
		{
			var cross	= normal.Cross( direction ) ;
			var dot		= normal.Dot( direction ) ;

			float rotation ;

			if( cross >= 0 )
			{
				// 右回転(cross  0 → +1 →  0)

				if( dot >= 0 )
				{
					// +  0度～+ 90度(dot +1 →  0)
					rotation = MathF.Asin( cross ) ;
				}
				else
				{
					// + 90度～+180度(dot  0 → -1)
					rotation =   Mathf.Pi - MathF.Asin( cross ) ;
				}
			}
			else
			{
				// 左回転(cross 0 → -1 → 0)

				if( dot >= 0 )
				{
					// +  0度～+ 90度(dot +1 →  0)
					rotation = MathF.Asin( cross ) ;
				}
				else
				{
					// + 90度～+180度(dot  0 → -1)
					rotation = - Mathf.Pi - MathF.Asin( cross ) ;
				}
			}

			if( isDegree == true )
			{
				// 角度に変更する
				rotation = 180.0f * rotation / Mathf.Pi ;
			}

			return rotation ;
		}

		/// <summary>
		/// ウェイト値配列を元にインデックス値をランダムで選択する
		/// </summary>
		/// <param name="weights"></param>
		/// <returns></returns>
		public static int GetRandomIndex( params int[] weights )
		{
			if( weights == null || weights.Length == 0 )
			{
				return 0 ;
			}

			int totalWeight = 0 ;

			foreach( int weight in weights )
			{
				totalWeight += weight ;
			}

			int weightValue = ( int )( GD.Randi() % totalWeight ) ;

			int i, l = weights.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( weightValue <  weights[ i ] )
				{
					// 確定
					return  i ;
				}
				else
				{
					weightValue -= weights[ i ] ;
				}
			}

			// ここに来る事はありえない
			return 0 ;
		}

		/// <summary>
		/// 範囲の乱数値を取得する
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int GetRandomRange( int min, int max )
		{
			if( min >  max )
			{
				int swap = min ;
				min = max ;
				min = swap ;
			}

			return ( int )( GD.Randi() % ( max - min + 1 ) ) + min ;
		}

		/// <summary>
		/// 範囲の乱数値を取得する
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float GetRandomRange( float min, float max )
		{
			if( min >  max )
			{
				float swap = min ;
				min = max ;
				min = swap ;
			}

			return ( GD.Randf() * ( max - min ) ) + min ;
		}

		/// <summary>
		/// 符号を取得する
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float Sign( float v )
		{
			if( v <  0 )
			{
				return -1 ;
			}
			else
			if( v >  0 )
			{
				return +1 ;
			}
			else
			{
				return 0 ;
			}
		}
	}
}
