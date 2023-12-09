using System ;


namespace InputHelper
{
	/// <summary>
	/// キーボード制御
	/// </summary>
	public partial class Keyboard
	{
		//-------------------------------------------------------------------------------------------------------------------

		private static InputManager m_Owner ;

		/// <summary>
		/// 初期化を行う
		/// </summary>
		public static void Initialize( InputManager owner )
		{
			m_Owner = owner ;

			m_Implementation = new Implementation() ;

			//----------------------------------
			// 固有処理

			m_Implementation.Initialize() ;
		}

		/// <summary>
		/// 毎フレーム呼び出される
		/// </summary>
		public static void Update()
		{
			m_Implementation .Update() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 実装インターフェース
		/// </summary>
		public interface IImplementation
		{
			/// <summary>
			/// キーが押されているかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKey( KeyCodes keyCode ) ;

			/// <summary>
			/// キーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKeyDown( KeyCodes keyCode ) ;

			/// <summary>
			/// キーが離されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKeyUp( KeyCodes keyCode ) ;

			//----------------------------------------------------------
			// 固有処理

			void Initialize() ;
			void Update() ;
		}

		// 実装のインスタンス
		private static IImplementation m_Implementation ;

		//-------------------------------------------------------------------------------------------------------------------
		// 公開メソッド

		/// <summary>
		/// キーが押されているかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKey( KeyCodes keyCode )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}
			return m_Implementation.GetKey( keyCode ) ;
		}

		/// <summary>
		/// キーが押されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyDown( KeyCodes keyCode )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}
			return m_Implementation.GetKeyDown( keyCode ) ;
		}

		/// <summary>
		/// キーが離されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyUp( KeyCodes keyCode )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}
			return m_Implementation.GetKeyUp( keyCode ) ;
		}
	}
}
