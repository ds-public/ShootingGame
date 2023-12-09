using System ;
using System.Collections.Generic ;


namespace InputHelper
{
	public partial class InputManager
	{
		/// <summary>
		/// 状態に変化があった際にコールバックで通知するための定義クラス
		/// </summary>
		public class Updater
		{
			public Action<System.Object>	Action = null ;
			public System.Object			Option = null ;

			public Updater( Action<System.Object> action, System.Object option )
			{
				Action = action ;
				Option = option ;
			}
		}

		private readonly Stack<Updater>	m_Updater = new () ;

		//-----------------------------------------------------------

		/// <summary>
		/// GamePadUpdater を登録する
		/// </summary>
		/// <param name="action"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static bool AddUpdater( Action<System.Object> action, System.Object option = null )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.AddUpdater_Private( action, option ) ;
		}
		
		private bool AddUpdater_Private( Action<System.Object> action, System.Object option )
		{
			if( action == null )
			{
				return false ;
			}

			m_Updater.Push( new Updater( action, option ) ) ;

			return true ;
		}

		/// <summary>
		/// GamePadUpdater を除去する
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public static bool RemoveUpdater( Action<System.Object> action = null )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.RemoveUpdater_Private( action ) ;
		}

		private bool RemoveUpdater_Private( Action<System.Object> action )
		{
			if( m_Updater.Count == 0 )
			{
				return false ;	// キューには何も積まれていない
			}

			if( action != null )
			{
				// 間違った GamePadUpdater を除去しないように確認する

				Updater stateUpdater = m_Updater.Peek() ;
				if( stateUpdater.Action != action )
				{
					// 違う！
					return false ;
				}
			}
			
			m_Updater.Pop() ;

			return true ;
		}

		//-----------------------------------------------------------

		private Action<InputTypes>	m_OnInputTypeChanged = null ;

		/// <summary>
		/// 入力タイプが切り替わった際に通知するデリケート
		/// </summary>
		/// <param name="inputType"></param>
		public delegate void OnInputTypeChangedDelegate( InputTypes inputType ) ;

		private OnInputTypeChangedDelegate	m_OnInputTypeChangedDelegate ;

		/// <summary>
		/// モード切替を通知するコールバックを設定する
		/// </summary>
		/// <param name="onInputTypeChanged"></param>
		/// <param name="call"></param>
		public static void SetOnInputTypeChanged( Action<InputTypes> onInputTypeChanged, bool call = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.SetOnInputTypeChanged_Private( onInputTypeChanged, call ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void SetOnInputTypeChanged_Private( Action<InputTypes> onInputTypeChanged, bool call )
		{
			m_OnInputTypeChanged = onInputTypeChanged ;
			
			// セットした直後に現在のモードでコールバックを呼ぶ
			if( call == true && m_OnInputTypeChanged != null )
			{
				m_OnInputTypeChanged( m_InputType ) ;
			}
		}

		/// <summary>
		/// モード切替を通知するコールバックを設定する
		/// </summary>
		/// <param name="onInputTypeChangedDelegate"></param>
		/// <param name="call"></param>
		public static void AddOnInputTypeChanged( OnInputTypeChangedDelegate onInputTypeChangedDelegate, bool call = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.AddOnInputTypeChanged_Private( onInputTypeChangedDelegate, call ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void AddOnInputTypeChanged_Private( OnInputTypeChangedDelegate onInputTypeChangedDelegate, bool call )
		{
			// 念のため既存の登録があったら削除する(おそらく必要は無いと思うが)
			m_OnInputTypeChangedDelegate -= onInputTypeChangedDelegate ;

			m_OnInputTypeChangedDelegate += onInputTypeChangedDelegate ;
			
			// セットした直後に現在のモードでコールバックを呼ぶ
			if( call == true )
			{
				m_OnInputTypeChangedDelegate( m_InputType ) ;
			}
		}

		/// <summary>
		/// モード切替を通知するコールバックを解除する
		/// </summary>
		/// <param name="onInputTypeChangedDelegate"></param>
		public static void RemoveOnInputTypeChanged( OnInputTypeChangedDelegate onInputTypeChangedDelegate )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.RemoveOnInputTypeChanged_Private( onInputTypeChangedDelegate ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void RemoveOnInputTypeChanged_Private( OnInputTypeChangedDelegate onInputTypeChangedDelegate )
		{
			m_OnInputTypeChangedDelegate -= onInputTypeChangedDelegate ;
		}
	}
}
