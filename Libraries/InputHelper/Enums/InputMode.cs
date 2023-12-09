namespace InputHelper
{
	/// <summary>
	/// 現在の入力の処理タイプ
	/// </summary>
	public enum InputProcessingTypes
	{
		/// <summary>
		/// 不明
		/// </summary>
		Unknown	= 0 ,

		/// <summary>
		/// どちらか片方の入力のみ出来る
		/// </summary>
		Switching	= 1,

		/// <summary>
		/// 同時に両方の入力が出来る
		/// </summary>
		Parallel	= 2,
	}

	/// <summary>
	/// 現在の入力タイプ
	/// </summary>
	public enum InputTypes
	{
		/// <summary>
		/// 不明
		/// </summary>
		Unknown =  0,

		/// <summary>
		/// ポインター(マウス・タッチ)　※nputProcessingType が Single
		/// </summary>
		Pointer	=  1,

		/// <summary>
		/// ゲームパッド　※InputProcessingType が Single
		/// </summary>
		GamePad	=  2,

		/// <summary>
		/// 両方　※InputProcessingType が Dual
		/// </summary>
		All		=  3,
	}
}
