using Godot ;


namespace InputHelper
{
	/// <summary>
	/// ポインター制御
	/// </summary>
	public static class Pointer
	{
		/// <summary>
		/// 左ボタン番号
		/// </summary>
		public const int		LB = 0 ;

		/// <summary>
		/// 右ボタン番号
		/// </summary>
		public const int		RB = 1 ;

		/// <summary>
		/// 中ボタン番号
		/// </summary>
		public const int		MB = 2 ;

		/// <summary>
		/// ボタンの数
		/// </summary>
		public const int		NumberOfButtons = 3 ;

		/// <summary>
		/// リピート開始までの時間(秒)
		/// </summary>
		public static float		RepeatStartingTime { get ; set ; } = 0.5f ;

		/// <summary>
		/// リピートする間隔の時間(秒)
		/// </summary>
		public static float		RepeatIntervalTime { get ; set ; } = 0.05f ;

		//-----------------------------------

		/// <summary>
		/// ポインターの表示状態
		/// </summary>
		public static bool Visible
		{
			get
			{
				return ( Input.MouseMode == Input.MouseModeEnum.Visible || Input.MouseMode == Input.MouseModeEnum.Confined ) ;
			}
			set
			{
				if( m_Owner.CursorProcessing == true )
				{
					// InputManager でのカーソル制御は有効

					m_Owner.CursorVisible = value ;
				}
				else
				{
					// InputManager でのカーソル制御は無効

					Input.MouseMode = ( value == true ? Input.MouseModeEnum.Visible : Input.MouseMode = Input.MouseModeEnum.Hidden ) ;
				}
			}
		}

		//-----------------------------------

		/// <summary>
		/// ボタン用状態
		/// </summary>
		public class ButtonState
		{
			public bool		RepeatKeepFlag ;
			public float	RepeatWakeTime ;
			public float	RepeatLoopTime ;
			public bool		IsRepeat ;
			public bool		IsDown ;
			public bool		IsUp ;
		}

		private static ButtonState[,] m_ButtonStates ;

		//-----------------------------------------------------------

		private static InputManager m_Owner ;

		/// <summary>
		/// 初期化を行う
		/// </summary>
		public static void Initialize( InputManager owner )
		{
			m_Owner = owner ;

			int length ;

			// ボタン
			length = NumberOfButtons ;
			m_ButtonStates = new ButtonState[ length, 2 ] ;
			for( int buttonIndex  = 0 ; buttonIndex <  length ; buttonIndex ++ )
			{
				m_ButtonStates[ buttonIndex, 0 ] = new ButtonState() ;
				m_ButtonStates[ buttonIndex, 1 ] = new ButtonState() ;
			}
		}

		// 時間を取得する
		private static float GetTime()
		{
			return Time.GetTicksMsec() / 1000.0f ;
		}

		/// <summary>
		/// 毎フレーム更新
		/// </summary>
		public static void Update( bool isFixed )
		{
			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			int buttonIndex ;

			for( buttonIndex  = 0 ; buttonIndex <  NumberOfButtons ; buttonIndex ++ )
			{
				ButtonState state = m_ButtonStates[ buttonIndex, slotNumber ] ;

				//---------------------------------

				state.IsRepeat	= false ;
				state.IsDown	= false ;
				state.IsUp		= false ;

				if( Mouse.GetButton( buttonIndex ) == true )
				{
					if( state.RepeatKeepFlag == false )
					{
						// リピート開始
						state.IsRepeat	= true ;

						state.RepeatKeepFlag = true ;
						state.RepeatWakeTime = GetTime() ;
						state.RepeatLoopTime = GetTime() ;

						state.IsDown = true ;
					}
					else
					{
						// リピート最中
						if( ( GetTime() - state.RepeatWakeTime ) >= RepeatStartingTime )
						{
							// リピート中
							if( ( GetTime() - state.RepeatLoopTime ) >= RepeatIntervalTime )
							{
								state.RepeatLoopTime = GetTime() ;

								state.IsRepeat = true ;
							}
						}
					}
				}
				else
				{
					// リピート解除
					if( state.RepeatKeepFlag == true )
					{
						state.IsUp = true ;

						state.RepeatKeepFlag  = false ;
					}
				}
			}
		}

		/// <summary>
		/// ポインターの位置
		/// </summary>
		public static Vector2 Position
			=> Mouse.Position ;

		/// <summary>
		/// ポインターが押されているかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonIndex )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( buttonIndex <  0 || buttonIndex >= NumberOfButtons )
			{
				return false ;
			}

			return Mouse.GetButton( buttonIndex ) ;
		}

		/// <summary>
		/// ポインターが押されたかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButtonDown( int buttonIndex, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( buttonIndex <  0 || buttonIndex >= NumberOfButtons )
			{
				return false ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			return m_ButtonStates[ buttonIndex, slotNumber ].IsDown ;
		}

		/// <summary>
		/// ポインターが離されたかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonIndex, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( buttonIndex <  0 || buttonIndex >= NumberOfButtons )
			{
				return false ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			return m_ButtonStates[ buttonIndex, slotNumber ].IsUp ;
		}

		/// <summary>
		/// ポインターが押されているかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonIndex, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( buttonIndex <  0 || buttonIndex >= NumberOfButtons )
			{
				return false ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			return m_ButtonStates[ buttonIndex, slotNumber ].IsRepeat ;
		}

		/// <summary>
		/// リピート付きでポインターの状態を取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButtonRepeat( int buttonIndex, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( buttonIndex <  0 || buttonIndex >= NumberOfButtons )
			{
				return false ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			return m_ButtonStates[ buttonIndex, slotNumber ].IsRepeat ;
		}
	}
}
