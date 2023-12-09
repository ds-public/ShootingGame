using Godot ;
using System ;

namespace uGUI.Compatible
{
	/// <summary>
	/// アンカーのプリセットタイプ(型をきちんと指定しないと数値キャストが出来ない)
	/// </summary>
	public enum AnchorPresetTypes : int
	{
		AA =  0,

		LA =  1,
		CA =  2,
		RA =  3,

		SA =  4,

		AT =  5,

		LT =  6,
		CT =  7,
		RT =  8,

		ST =  9,

		AM = 10,

		LM = 11,
		CM = 12,
		RM = 13,

		SM = 14,

		AB = 15,

		LB = 16,
		CB = 17,
		RB = 18,

		SB = 19,

		AS = 20,

		LS = 21,
		CS = 22,
		RS = 23,

		SS = 24,
	}
}

