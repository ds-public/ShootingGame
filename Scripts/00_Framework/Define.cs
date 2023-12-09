using Godot ;
using System ;

namespace Sample_001
{
	/// <summary>
	/// 各種定義クラス
	/// </summary>
	public partial class Define
	{
		/// <summary>
		/// 各種情報の暗号化を有効化するか
		/// </summary>
		public static bool SecurityEnabled
		{
			get
			{
#if TOOLS
				return false ;
#else
				return true ;
#endif
			}
		}

		/// <summary>
		/// 暗号化キー
		/// </summary>
		public const string CryptoKey		= "lkirwf897+22#bbtrm8814z5qq=498j5" ;

		/// <summary>
		/// 暗号化ベクター
		/// </summary>
		public const string CryptoVector	= "741952hheeyy66#cs!9hjv887mxx7@8y" ;
	}
}

