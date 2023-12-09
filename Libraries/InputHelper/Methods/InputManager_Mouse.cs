using Godot ;


namespace InputHelper
{
	public partial class InputManager
	{
		// Mouse 関係

		//-------------------------------------------------------------------------------------------
		// 互換メソッド

		/// <summary>
		/// ポインターの位置
		/// </summary>
		public static Vector2 MousePosition
			=> Mouse.Position ;

		/// <summary>
		/// ボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButton( int buttonNumber )
			=> Mouse.GetButton( buttonNumber ) ;

		/// <summary>
		/// ボタンが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButtonDown( int buttonNumber )
			=> Mouse.GetButtonDown( buttonNumber ) ;

		/// <summary>
		/// ボタンが離されたどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButtonUp( int buttonNumber )
			=> Mouse.GetButtonUp( buttonNumber ) ;

		/// <summary>
		/// ホイールの移動量
		/// </summary>
		public static Vector2 MouseScrollDelta
			=> Mouse.ScrollDelta ;
	}
}
