using Godot ;
using System.Collections.Generic ;


namespace InputHelper
{
	/// <summary>
	/// キーボード制御
	/// </summary>
	public partial class Keyboard
	{
		// 新版
		public class Implementation : IImplementation
		{
			private static readonly Dictionary<KeyCodes, Key> m_KeyCodeMapper = new ()
			{
				{ KeyCodes.Backspace,		Key.Backspace		},
				{ KeyCodes.Delete,			Key.Delete			},
				{ KeyCodes.Tab,				Key.Tab				},
				{ KeyCodes.Clear,			Key.Clear			},
				{ KeyCodes.Return,			Key.Enter			},
				{ KeyCodes.Pause,			Key.Pause			},
				{ KeyCodes.Escape,			Key.Escape			},
				{ KeyCodes.Space,			Key.Space			},

				{ KeyCodes.Keypad0,			Key.Kp0				},
				{ KeyCodes.Keypad1,			Key.Kp1				},
				{ KeyCodes.Keypad2,			Key.Kp2				},
				{ KeyCodes.Keypad3,			Key.Kp3				},
				{ KeyCodes.Keypad4,			Key.Kp4				},
				{ KeyCodes.Keypad5,			Key.Kp5				},
				{ KeyCodes.Keypad6,			Key.Kp6				},
				{ KeyCodes.Keypad7,			Key.Kp7				},
				{ KeyCodes.Keypad8,			Key.Kp8				},
				{ KeyCodes.Keypad9,			Key.Kp9				},

				{ KeyCodes.KeypadPeriod,	Key.KpPeriod		},
				{ KeyCodes.KeypadDivide,	Key.KpDivide		},
				{ KeyCodes.KeypadMultiply,	Key.KpMultiply		},
				{ KeyCodes.KeypadMinus,		Key.KpSubtract		},
				{ KeyCodes.KeypadPlus,		Key.KpAdd			},
				{ KeyCodes.KeypadEnter,		Key.KpEnter			},
				{ KeyCodes.KeypadEquals,	Key.KpEnter			},	// 未対応

				{ KeyCodes.UpArrow,			Key.Up				},
				{ KeyCodes.DownArrow,		Key.Down			},
				{ KeyCodes.RightArrow,		Key.Right			},
				{ KeyCodes.LeftArrow,		Key.Left			},

				{ KeyCodes.Insert,			Key.Insert			},
				{ KeyCodes.Home,			Key.Home			},
				{ KeyCodes.End,				Key.End				},
				{ KeyCodes.PageUp,			Key.Pageup			},
				{ KeyCodes.PageDown,		Key.Pagedown		},
				
				{ KeyCodes.F1,				Key.F1				},
				{ KeyCodes.F2,				Key.F2				},
				{ KeyCodes.F3,				Key.F3				},
				{ KeyCodes.F4,				Key.F4				},
				{ KeyCodes.F5,				Key.F5				},
				{ KeyCodes.F6,				Key.F6				},
				{ KeyCodes.F7,				Key.F7				},
				{ KeyCodes.F8,				Key.F8				},
				{ KeyCodes.F9,				Key.F9				},
				{ KeyCodes.F10,				Key.F10				},
				{ KeyCodes.F11,				Key.F11				},
				{ KeyCodes.F12,				Key.F12				},
				{ KeyCodes.F13,				Key.F13				},
				{ KeyCodes.F14,				Key.F14				},
				{ KeyCodes.F15,				Key.F15				},

				{ KeyCodes.Alpha0,			Key.Key0			},
				{ KeyCodes.Alpha1,			Key.Key1			},
				{ KeyCodes.Alpha2,			Key.Key2			},
				{ KeyCodes.Alpha3,			Key.Key3			},
				{ KeyCodes.Alpha4,			Key.Key4			},
				{ KeyCodes.Alpha5,			Key.Key5			},
				{ KeyCodes.Alpha6,			Key.Key6			},
				{ KeyCodes.Alpha7,			Key.Key7			},
				{ KeyCodes.Alpha8,			Key.Key8			},
				{ KeyCodes.Alpha9,			Key.Key9			},

				{ KeyCodes.Exclaim,			Key.Exclam			},
				{ KeyCodes.DoubleQuote,		Key.Quotedbl		},
				{ KeyCodes.Hash,			Key.Numbersign		},
				{ KeyCodes.Dollar,			Key.Dollar			},
				{ KeyCodes.Percent,			Key.Percent			},
				{ KeyCodes.Ampersand,		Key.Ampersand		},
				{ KeyCodes.Quote,			Key.Quoteleft		},
				{ KeyCodes.LeftParen,		Key.Parenleft		},
				{ KeyCodes.RightParen,		Key.Parenleft		},
				{ KeyCodes.Asterisk,		Key.Asterisk		},
				{ KeyCodes.Plus,			Key.Plus			},
				{ KeyCodes.Comma,			Key.Comma			},
				{ KeyCodes.Minus,			Key.Minus			},
				{ KeyCodes.Period,			Key.Period			},
				{ KeyCodes.Slash,			Key.Slash			},
				{ KeyCodes.Colon,			Key.Colon			},
				{ KeyCodes.Semicolon,		Key.Semicolon		},
				{ KeyCodes.Less,			Key.Less			},
				{ KeyCodes.Equals,			Key.Equal			},
				{ KeyCodes.Greater,			Key.Greater			},
				{ KeyCodes.Question,		Key.Question		},
				{ KeyCodes.At,				Key.At				},
				{ KeyCodes.LeftBracket,		Key.Bracketleft		},
				{ KeyCodes.Backslash,		Key.Backslash		},
				{ KeyCodes.RightBracket,	Key.Bracketright	},
				{ KeyCodes.Caret,			Key.Asciicircum		},
				{ KeyCodes.Underscore,		Key.Underscore		},
				{ KeyCodes.BackQuote,		Key.Quoteleft		},

				{ KeyCodes.A,				Key.A	},
				{ KeyCodes.B,				Key.B	},
				{ KeyCodes.C,				Key.C	},
				{ KeyCodes.D,				Key.D	},
				{ KeyCodes.E,				Key.E	},
				{ KeyCodes.F,				Key.F	},
				{ KeyCodes.G,				Key.G	},
				{ KeyCodes.H,				Key.H	},
				{ KeyCodes.I,				Key.I	},
				{ KeyCodes.J,				Key.J	},
				{ KeyCodes.K,				Key.K	},
				{ KeyCodes.L,				Key.L	},
				{ KeyCodes.M,				Key.M	},
				{ KeyCodes.N,				Key.N	},
				{ KeyCodes.O,				Key.O	},
				{ KeyCodes.P,				Key.P	},
				{ KeyCodes.Q,				Key.Q	},
				{ KeyCodes.R,				Key.R	},
				{ KeyCodes.S,				Key.S	},
				{ KeyCodes.T,				Key.T	},
				{ KeyCodes.U,				Key.U	},
				{ KeyCodes.V,				Key.V	},
				{ KeyCodes.W,				Key.W	},
				{ KeyCodes.X,				Key.X	},
				{ KeyCodes.Y,				Key.Y	},
				{ KeyCodes.Z,				Key.Z	},

				{ KeyCodes.LeftCurlyBracket,	Key.Braceleft			},
				{ KeyCodes.Pipe,				Key.Bar					},
				{ KeyCodes.RightCurlyBracket,	Key.Braceright			},
				{ KeyCodes.Tilde,				Key.Asciitilde			},
				{ KeyCodes.Numlock,				Key.Numlock				},
				{ KeyCodes.CapsLock,			Key.Capslock			},
				{ KeyCodes.ScrollLock,			Key.Scrolllock			},
				{ KeyCodes.RightShift,			Key.Shift				},
				{ KeyCodes.LeftShift,			Key.Shift				},	// 未対応
				{ KeyCodes.RightControl,		Key.Ctrl				},
				{ KeyCodes.LeftControl,			Key.Ctrl				},	// 未対応
				{ KeyCodes.RightAlt,			Key.Alt					},
				{ KeyCodes.LeftAlt,				Key.Alt					},	// 未対応
				{ KeyCodes.LeftMeta,			Key.Meta				},
				{ KeyCodes.LeftCommand,			Key.Section				},	// 未対応
				{ KeyCodes.LeftApple,			Key.Unknown				},	// 未対応
				{ KeyCodes.LeftWindows,			Key.Unknown				},	// 未対応
				{ KeyCodes.RightMeta,			Key.Meta				},
				{ KeyCodes.RightCommand,		Key.Section				},
				{ KeyCodes.RightApple,			Key.Unknown				},
				{ KeyCodes.RightWindows,		Key.Unknown				},
				{ KeyCodes.AltGr,				Key.Alt					},	// 未対応
				{ KeyCodes.Help,				Key.Help				},
				{ KeyCodes.Print,				Key.Print				},
				{ KeyCodes.SysReq,				Key.Sysreq				},
				{ KeyCodes.Break,				Key.Pause				},
				{ KeyCodes.Menu,				Key.Menu				},
			} ;

			/// <summary>
			/// キーが押されているかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKey( KeyCodes keyCode )
			{
				return Input.IsKeyPressed( m_KeyCodeMapper[ keyCode ] ) ;
			}

			/// <summary>
			/// キーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyDown( KeyCodes keyCode )
			{
				return m_KeyStates[ keyCode ].IsPressed ;
			}

			/// <summary>
			/// キーが離されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyUp( KeyCodes keyCode )
			{
				return m_KeyStates[ keyCode ].IsReleased ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// フレームで押した離したの状態格納
			/// </summary>
			public class KeyState
			{
				public bool		IsPressing ;

				public bool		IsPressed ;
				public bool		IsReleased ;
			}

			private Dictionary<KeyCodes,KeyState> m_KeyStates ;

			/// <summary>
			/// 初期化する
			/// </summary>
			public void Initialize()
			{
				m_KeyStates = new Dictionary<KeyCodes,KeyState>() ;

				foreach( var keyMapper in m_KeyCodeMapper )
				{
					if( keyMapper.Value != Key.Unknown )
					{
						m_KeyStates.Add( keyMapper.Key, new KeyState() ) ;
					}
				}
			}

			/// <summary>
			/// イベントによる状態アップデート
			/// </summary>
			public void Update()
			{
				foreach( var keyMapper in m_KeyCodeMapper )
				{
					if( keyMapper.Value != Key.Unknown )
					{
						var keyState = m_KeyStates[ keyMapper.Key ] ;

						if( Input.IsKeyPressed( keyMapper.Value ) == true )
						{
							// 押している
							if( keyState.IsPressing == false )
							{
								// 前フレームでは離している
								keyState.IsPressed = true ;		// 現在フレームで押した判定する
								keyState.IsPressing = true ;
							}
							else
							{
								// 前フレームでは押している
								keyState.IsPressed = false ;
							}
						}
						else
						{
							// 離している
							if( keyState.IsPressing == true )
							{
								// 前フレームでは押している
								keyState.IsReleased = true ;	// 現在フレームで離した判定する
								keyState.IsPressing = false ;
							}
							else
							{
								// 前フレームでは離している
								keyState.IsReleased = false ;
							}
						}
					}
				}
			}
		}
	}
}
