using Godot ;


namespace InputHelper
{
	/// <summary>
	/// インプット管理マネージャ Version 2023/11/23 0
	/// </summary>
	public partial class InputManager : Node2D
	{
		// マネージャのインスタンス(シングルトン)
		private static InputManager m_Instance = null ; 

		/// <summary>
		/// マネージャのインスタンス(シングルトン)
		/// </summary>
		public static InputManager Instance => m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マネージャを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static InputManager Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var inputManager = new InputManager()
			{
				Name = "InputManager"
			} ;

			if( parent == null )
			{
				inputManager.GetTree().Root.AddChild( inputManager ) ;
			}
			else
			{
				parent.AddChild( inputManager ) ;
			}

			return inputManager ;
		}

		/// <summary>
		/// マネージャを破棄する
		/// </summary>
		public static void Delete()
		{
			if( m_Instance != null )
			{
				m_Instance?.QueueFree() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			if( m_Instance == null )
			{
				m_Instance = this ;

				//----------------------------------

				Initialize() ;
			}
		}

		/// <summary>
		/// インスタンスがツリーから除外される際に呼び出される
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree() ;

			if( m_Instance == this )
			{
				Terminate() ;

				m_Instance  = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// 基本入力モジュールをセットアップする
		private void Initialize()
		{
			// プロセスの実行順を高くしておく
			ProcessPriority = 10 ;

			//----------------------------------

			// Keyboard の実装を生成する
			Keyboard.Initialize( this ) ;

			// Moue の実装を生成する
			Mouse.Initialize( this ) ;

			// Pointer の実装を生成する
			Pointer.Initialize( this ) ;

			// GamePad の実装を生成する
			GamePad.Initialize( this ) ;
		}

		// 後始末を行う
		private static void Terminate()
		{
			// ポインターが非表示になっている可能性があるので念のため表示しておく
			Input.MouseMode = Input.MouseModeEnum.Visible ;
		}

		//------------------------------------------------------------------------------------------

		//-----------------------------------

		// フォーカスを得ている状態かどうか
		private bool m_IsFocus = true ;

		/// <summary>
		/// フォーカスを得ているか状態かどうか
		/// </summary>
		public static bool IsFocus
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_IsFocus ;
			}
		}

//		internal void OnApplicationFocus( bool focus )
//		{
//			m_IsFocus = focus ;
//		}

		//---------------------------------------------------------------------------

		// m_Enabled という名前は MonoBehaviour で定義されているので使ってはいけない
		public bool ControlEnabled = true ;

		/// <summary>
		/// 有効にする
		/// </summary>
		public static void Enable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ControlEnabled = true ;
		}

		/// <summary>
		/// 無効にする
		/// </summary>
		public static void Disable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ControlEnabled = false ;
		}

		/// <summary>
		/// 有効無効状態
		/// </summary>
		public static bool IsControlEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.ControlEnabled ;
			}
			set
			{
				if( m_Instance == null )
				{
					return  ;
				}

				m_Instance.ControlEnabled = value ;
			}
		}

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public bool Invert = true ;

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public static bool IsInvert
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.Invert ;
			}
			set
			{
				m_Instance.Invert = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 描画アップデート
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			// 毎フレームの処理
			ProcessUpdate( delta ) ;
		}

		/// <summary>
		/// 物理アップデート
		/// </summary>
		/// <param name="delta"></param>
		public override void _PhysicsProcess( double delta )
		{
			// 毎フレームの処理
			ProcessFixedUpdate( delta ) ;
		}

		//-------------------------------------------------------------------------------------------
		// 毎フレームの処理

		private float	m_Tick = 0 ;
		private Vector2 m_Position = Vector2.Zero ;

		// 毎フレーム呼び出される(描画)
		private void ProcessUpdate( double delta )
		{
			if( ControlEnabled == false )
			{
				return ;
			}

			//----------------------------------------------------------
			// 固有処理

			Keyboard.Update() ;
			Mouse.Update( GetGlobalMousePosition() ) ;

			//----------------------------------

			if( InputProcessingType == InputProcessingTypes.Switching )
			{
				// いずれか片方の入力のみ可能

				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード
					if( m_InputHold == true )
					{
						// 切り替えた直後は一度全開放しないと入力できない
						if( GetMouseButton( 0 ) == false && GetMouseButton( 1 ) == false && GetMouseButton( 2 ) == false )
						{
							m_InputHold = false ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する
							m_Tick += ( float )delta ;
							if( m_Tick >= 1 )
							{
								m_InputHold = false ;
							}
						}
					}
					else
					{
						// Pointer モード有効中
						Pointer.Update( false ) ;

						Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
						Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
						Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

						if( GamePad.GetButtonAll() != 0 || axis_0.X != 0 || axis_0.Y != 0 || axis_1.X != 0 || axis_1.Y != 0 || axis_2.X != 0 || axis_2.Y != 0 )
						{
							// GamePad モードへ移行
							
							m_SystemCursorVisible = false ;

							m_InputType = InputTypes.GamePad ;
							m_InputHold = true ;
							m_Tick = 0 ;

							m_OnInputTypeChanged?.Invoke( m_InputType ) ;
							m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
						}
					}
				}
				else
				{
					// 現在は GamePad モード
					if( m_InputHold == true )
					{
						// 切り替えた直後は一度全開放しないと入力できない
						Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
						Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
						Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

						if( GamePad.GetButtonAll() == 0 && axis_0.X == 0 && axis_0.Y == 0 && axis_1.X == 0 && axis_1.Y == 0 && axis_2.X == 0 && axis_2.Y == 0 )
						{
							m_InputHold = false ;

							m_Position = MousePosition ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する(DualShock4を繋いている時にXbox互換のプロファイルに切り替えると右スティックが-1,-1で入りっぱなし扱いになってしまうため)
							m_Tick += ( float )delta ;
							if( m_Tick >= 1 )
							{
								m_InputHold = false ;
							}
						}
					}
					else
					{
						// GamePad モード有効中
						GamePad.Update( false ) ;

						if( m_Position.Equals( MousePosition ) == false || GetMouseButton( 0 ) == true || GetMouseButton( 1 ) == true || GetMouseButton( 2 ) == true )
						{
							// Pointer モードへ移行

							m_SystemCursorVisible = true ;

							m_InputType = InputTypes.Pointer ;
							m_InputHold = true ;

							m_OnInputTypeChanged?.Invoke( m_InputType ) ;
							m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
						}
					}
				}
			}
			else
			{
				// 両方の入力が同時に可能

				// 最後に入力された方を現在のモードとする
				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード扱い

					Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
					Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
					Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

					if( GamePad.GetButtonAll() != 0 || axis_0.X != 0 || axis_0.Y != 0 || axis_1.X != 0 || axis_1.Y != 0 || axis_2.X != 0 || axis_2.Y != 0 )
					{
						// GamePad モードへ移行

						// GamePad モード解除判定用に現在の Pointer の位置を記録する
						m_Position = MousePosition ;

						m_SystemCursorVisible = false ;

						m_InputType = InputTypes.GamePad ;

						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
					}
				}
				else
				{
					// 現在は GamePad モード扱い

					if( m_Position.Equals( MousePosition ) == false || GetMouseButton( 0 ) == true || GetMouseButton( 1 ) == true || GetMouseButton( 2 ) == true )
					{
						// Pointer モードへ移行

						m_SystemCursorVisible = true ;

						m_InputType = InputTypes.Pointer ;

						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
					}
				}

				//---------------------------------

				// Pointer
				Pointer.Update( false ) ;

				// GamePad
				GamePad.Update( false ) ;
			}

			//----------------------------------------------------------
			// カーソルの表示制御

			if( CursorProcessing == true )
			{
				// カーソルの表示制御が有効になっている
				bool isVisible = m_SystemCursorVisible & CursorVisible ;
                if( isVisible != m_ActiveCursorVisible )
                {
					// カーソルの表示状態が変化する
					m_ActiveCursorVisible = isVisible ;

					if( m_ActiveCursorVisible == true )
					{
						// カーソルは表示
						Input.MouseMode = Input.MouseModeEnum.Visible ;
					}
					else
					{
						// カーソルは隠蔽
						Input.MouseMode = Input.MouseModeEnum.Hidden ;
					}
                }
            }

			//----------------------------------------------------------

			if( m_Updater.Count >  0 )
			{
				Updater updater = m_Updater.Peek() ;
				updater.Action( updater.Option ) ;
			}
		}

		// 毎フレーム呼び出される(物理)
		private void ProcessFixedUpdate( double _ )
		{
			if( ControlEnabled == false )
			{
				return ;
			}

			//----------------------------------------------------------

			// Pointer
			Pointer.Update( true ) ;

			// GamePad
			GamePad.Update( true ) ;
		}
	}
}

