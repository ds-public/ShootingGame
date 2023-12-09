using Godot ;
using System ;

namespace InputHelper
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
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

		//-------------------------------------------------------------------------------------------------------------------

		private static InputManager m_Owner ;

		/// <summary>
		/// 初期化を行う
		/// </summary>
		public static void Initialize( InputManager owner )
		{
			m_Owner = owner ;

			// 新版の実装を採用
			m_Implementation = new Implementation() ;

			//----------------------------------
			// 固有処理

			m_Implementation.Initialize() ;
		}

		/// <summary>
		/// 毎フレーム呼び出される
		/// </summary>
		public static void Update( Vector2 mousePosition )
		{
			m_Implementation.Update( mousePosition ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 実装インターフェース
		/// </summary>
		public interface IImplementation
		{
			/// <summary>
			/// ポインターの位置
			/// </summary>
			Vector2 Position{ get ; }

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButton( int buttonNumber ) ;

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButtonDown( int buttonNumber ) ;

			/// <summary>
			/// ボタンが離されたどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButtonUp( int buttonNumber ) ;

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			Vector2 ScrollDelta{ get ; }

			//----------------------------------------------------------
			// 固有処理

			void Initialize() ;
			void Update( Vector2 mousePosition ) ;
		}

		// 実装のインスタンス
		private static IImplementation m_Implementation ;

		//-------------------------------------------------------------------------------------------------------------------
		// 公開メソッド

		/// <summary>
		/// ポインターの位置
		/// </summary>
		public static Vector2 Position
		{
			get
			{
				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}
				return m_Implementation.Position ;
			}
		}

		/// <summary>
		/// ボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonNumber )
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
			return m_Implementation.GetButton( buttonNumber ) ;
		}

		/// <summary>
		/// ボタンが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonDown( int buttonNumber )
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
			return m_Implementation.GetButtonDown( buttonNumber ) ;
		}

		/// <summary>
		/// ボタンが離されたどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonNumber )
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
			return m_Implementation.GetButtonUp( buttonNumber ) ;
		}

		/// <summary>
		/// ホイールの移動量
		/// </summary>
		public static Vector2 ScrollDelta
		{
			get
			{
				// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
				if( m_Owner == null || m_Owner.ControlEnabled == false )
				{
					// 無効
					return Vector2.Zero ;
				}

				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}
				return m_Implementation.ScrollDelta ;
			}
		}
	}
}
