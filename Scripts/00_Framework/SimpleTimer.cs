using Godot ;
using System ;


namespace Sample_001
{
	/// <summary>
	/// 時間計測用クラス
	/// </summary>
	public class SimpleTimer
	{
		// 基準時間
		private double m_BasisTime ;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SimpleTimer()
		{
			m_BasisTime = ApplicationManager.MasterTime ;
		}

		/// <summary>
		/// 現在の経過時間を取得する
		/// </summary>
		public float Value
		{
			get
			{
				double delta = ApplicationManager.MasterTime - m_BasisTime ;
				return ( float )delta ;
			}
		}

		/// <summary>
		/// 時間内であるかどうか
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool IsRunning( float duration )
		{
			return ( Value <  duration ) ;
		}

		/// <summary>
		/// 時間内外あるかどうか
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool IsFinished( float duration )
		{
			return ( Value >  duration ) ;
		}

		/// <summary>
		/// タイマーをリセットする
		/// </summary>
		public void Reset()
		{
			m_BasisTime = ApplicationManager.MasterTime ;
		}
	}
}
