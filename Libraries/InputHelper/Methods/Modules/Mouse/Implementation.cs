using Godot ;
using System.Collections.Generic ;


namespace InputHelper
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
	{
		// 新版
		public class Implementation : IImplementation
		{
			/// ポインターの位置
			/// </summary>
			public Vector2 Position
			{
				get
				{
					return m_MousePosition ;
				}
			}

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonNumber )
			{
				return Input.IsMouseButtonPressed( m_MouseButtons[ buttonNumber ] ) ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonDown( int buttonNumber )
			{
				return m_MouseButtonStates[ buttonNumber ].IsPressed ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonUp( int buttonNumber )
			{
				return m_MouseButtonStates[ buttonNumber ].IsReleased ;
			}

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			public Vector2 ScrollDelta
			{
				get
				{
					return m_MouseScrollDelta ;
				}
			}

			//------------------------------------------------------------------------------------------

			private Vector2 m_MousePosition ;

			private Vector2	m_MouseScrollDelta ;

			/// <summary>
			/// フレームで押した離したの状態格納
			/// </summary>
			public class MouseButtonState
			{
				public bool		IsPressing ;

				public bool		IsPressed ;
				public bool		IsReleased ;
			}

			private Dictionary<int,MouseButtonState> m_MouseButtonStates ;

			/// <summary>
			/// 初期化する
			/// </summary>
			public void Initialize()
			{
				m_MouseButtonStates = new Dictionary<int,MouseButtonState>() ;

				for( int buttonNumber = 0 ; buttonNumber <= 2 ; buttonNumber ++ )
				{
					m_MouseButtonStates.Add( buttonNumber, new MouseButtonState() ) ;
				}
			}

			private static readonly MouseButton[] m_MouseButtons =
			{
				MouseButton.Left, MouseButton.Right, MouseButton.Middle,
			} ;

			/// <summary>
			/// アップデートする
			/// </summary>
			/// <param name="position"></param>
			public void Update( Vector2 position )
			{
				m_MousePosition = position ;

				//---------------------------------

				for( int buttonNumber = 0 ; buttonNumber <= 2 ; buttonNumber ++ )
				{
					var mouseButtonState = m_MouseButtonStates[ buttonNumber ] ;

					if( Input.IsMouseButtonPressed( m_MouseButtons[ buttonNumber ] ) == true )
					{
						// 押している
						if( mouseButtonState.IsPressing == false )
						{
							// 前フレームでは離している
							mouseButtonState.IsPressed = true ;		// 現在フレームで押した判定する
							mouseButtonState.IsPressing = true ;
						}
						else
						{
							// 前フレームでは押している
							mouseButtonState.IsPressed = false ;
						}
					}
					else
					{
						// 離している
						if( mouseButtonState.IsPressing == true )
						{
							// 前フレームでは押している
							mouseButtonState.IsReleased = true ;	// 現在フレームで離した判定する
							mouseButtonState.IsPressing = false ;
						}
						else
						{
							// 前フレームでは離している
							mouseButtonState.IsReleased = false ;
						}
					}
				}

				//---------------------------------
				// ホイール

				float x = 0 ;
				if( Input.IsMouseButtonPressed( MouseButton.WheelRight ) == true )
				{
					x += 1 ;
				}
				if( Input.IsMouseButtonPressed( MouseButton.WheelLeft ) == true )
				{
					x -= 1 ;
				}

				float y = 0 ;
				if( Input.IsMouseButtonPressed( MouseButton.WheelUp ) == true )
				{
					y += 1 ;
				}
				if( Input.IsMouseButtonPressed( MouseButton.WheelDown ) == true )
				{
					y -= 1 ;
				}

				m_MouseScrollDelta = new Vector2( x, y ) ;
			}
		}
	}
}
