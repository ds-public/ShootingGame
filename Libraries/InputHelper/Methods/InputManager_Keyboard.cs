namespace InputHelper
{
	public partial class InputManager
	{
		// Keyboard 関係

		//-------------------------------------------------------------------------------------------
		// 互換メソッド

		/// <summary>
		/// キーが押されているかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKey( KeyCodes keyCode )
			=> Keyboard.GetKey( keyCode ) ;

		/// <summary>
		/// キーが押されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyDown( KeyCodes keyCode )
			=> Keyboard.GetKeyDown( keyCode ) ;

		/// <summary>
		/// キーが離されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyUp( KeyCodes keyCode )
			=> Keyboard.GetKeyUp( keyCode ) ;
	}
}
