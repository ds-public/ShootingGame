using Godot ;


namespace InputHelper
{
	public partial class InputManager
	{
		// 現在の入力の処理タイプ
		public InputProcessingTypes InputProcessingType = InputProcessingTypes.Parallel ;

		/// <summary>
		/// 入力の処理タイプを設定する
		/// </summary>
		/// <returns></returns>
		public static bool SetInputProcessingType( InputProcessingTypes inputProcessingType )
		{
			if( m_Instance == null )
			{
				// 失敗
				return false ;
			}

			m_Instance.SetInputProcessingType_Private( inputProcessingType ) ;
			return true ;
		}

		// 入力の処理タイプを設定する
		private void SetInputProcessingType_Private( InputProcessingTypes inputProcessingType )
		{
			if( InputProcessingType == inputProcessingType )
			{
				// 現在と同じなら何も処理しない
				return ;
			}

			//----------------------------------------------------------

			InputProcessingType = inputProcessingType ;

			if( InputProcessingType == InputProcessingTypes.Switching )
			{
				// シングルにする場合は初期状態はポインターとする

				m_InputType = InputTypes.Pointer ;
				m_InputHold = false ;

				Input.MouseMode = Input.MouseModeEnum.Visible ;
			}
			else
			if( InputProcessingType == InputProcessingTypes.Parallel )
			{
				// デュアルにする場合は念のためポインターを表示する(シングルのゲームパッド状態からの移行)

				Input.MouseMode = Input.MouseModeEnum.Visible ;
			}
		}

		/// <summary>
		/// 現在の入力の処理タイプ
		/// </summary>
		public static InputProcessingTypes GetInputProcessingType()
		{
			if( m_Instance == null )
			{
				return InputProcessingTypes.Unknown ;
			}

			return m_Instance.InputProcessingType ;
		}

		//-------------------------------------------------------------------------------------------

		// 現在の入力タイプ
		private InputTypes	m_InputType	= InputTypes.Pointer ;	// デフォルトはポインターモード
		private bool		m_InputHold	= false ;

		/// <summary>
		/// 現在の入力タイプ
		/// </summary>
		public static InputTypes InputType
		{
			get
			{
				if( m_Instance == null )
				{
					return InputTypes.Unknown ;
				}

				return m_Instance.m_InputType ;
			}
		}

		/// <summary>
		/// 現在のモードが実際に有効かどうか
		/// </summary>
		public static bool InputEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return ! m_Instance.m_InputHold ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// カーソルの制御状態
		public bool CursorProcessing = true ;

		/// <summary>
		/// カーソルの制御の有無を設定する
		/// </summary>
		/// <returns></returns>
		public static bool SetCursorProcessing( bool state )
		{
			if( m_Instance == null )
			{
				// 失敗
				return false ;
			}

			m_Instance.CursorProcessing = state ;

			return true ;
		}

		/// <summary>
		/// カーソルの制御の有無
		/// </summary>
		public static bool GetCursorProcessing()
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.CursorProcessing ;
		}

		//---------------

		// カーソルの表示状態
		private bool m_ActiveCursorVisible = true ;

		// システム制御のカーソルの表示状態
		private bool m_SystemCursorVisible = true ;

		// カーソルの表示状態
		public bool CursorVisible = true ;

		/// <summary>
		/// カーソルの表示状態のを設定する
		/// </summary>
		/// <returns></returns>
		public static bool SetCursorVisible( bool state )
		{
			if( m_Instance == null )
			{
				// 失敗
				return false ;
			}

			m_Instance.CursorVisible = state ;

			return true ;
		}

		/// <summary>
		/// カーソルの表示状態を取得する
		/// </summary>
		public static bool GetCursorVisible()
		{
			if( m_Instance == null )
			{
				return true ;
			}
			return m_Instance.CursorVisible ;
		}
	}
}
