using Godot ;
using System ;


namespace InputHelper
{
	/// <summary>
	/// ゲームパッド制御
	/// </summary>
	public partial class GamePad
	{
		//-------------------------------------------------------------------------------------------
		// 新版

		/// <summary>
		/// 新版の実装
		/// </summary>
		public partial class Implementation : IImplementation
		{
			//------------------------------------------------------------------------------------------
			// 独自メソッド

			/// <summary>
			/// 全てのボタンが押されているかどうか判定する
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public int GetButtonAll( int playerNumber = -1 )
			{
				int buttonFlags = 0 ;

				//----------------------------------

				// 接続しているしているプレイヤー(最大４)
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToButtonEnabled == true )
					{
						// キーボードのキーのボタンへの割り当て(ループでは回さない)
						if( GetButtonByMappingKey( GamePad.B1 ) == true ){ buttonFlags |= B1 ; }
						if( GetButtonByMappingKey( GamePad.B2 ) == true ){ buttonFlags |= B2 ; }
						if( GetButtonByMappingKey( GamePad.B3 ) == true ){ buttonFlags |= B3 ; }
						if( GetButtonByMappingKey( GamePad.B4 ) == true ){ buttonFlags |= B4 ; }

						if( GetButtonByMappingKey( GamePad.R1 ) == true ){ buttonFlags |= R1 ; }
						if( GetButtonByMappingKey( GamePad.L1 ) == true ){ buttonFlags |= L1 ; }
						if( GetButtonByMappingKey( GamePad.R2 ) == true ){ buttonFlags |= R2 ; }
						if( GetButtonByMappingKey( GamePad.L2 ) == true ){ buttonFlags |= L2 ; }
						if( GetButtonByMappingKey( GamePad.R3 ) == true ){ buttonFlags |= R3 ; }
						if( GetButtonByMappingKey( GamePad.L3 ) == true ){ buttonFlags |= L3 ; }

						if( GetButtonByMappingKey( GamePad.O1 ) == true ){ buttonFlags |= O1 ; }
						if( GetButtonByMappingKey( GamePad.O2 ) == true ){ buttonFlags |= O2 ; }
						if( GetButtonByMappingKey( GamePad.O3 ) == true ){ buttonFlags |= O3 ; }
						if( GetButtonByMappingKey( GamePad.O4 ) == true ){ buttonFlags |= O4 ; }
					}
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return buttonFlags ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					if( SwapB1toB2 == false )
					{
						// ボタン１とボタン２の入れ替え：なし
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  0 ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  1 ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}
					else
					{
						// ボタン１とボタン２の入れ替え：あり
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  1 ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  0 ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}

					if( SwapB3toB4 == false )
					{
						// ボタン３とボタン４の入れ替え：なし
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  2 ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  3 ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}
					else
					{
						// ボタン３とボタン４の入れ替え：あり
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  3 ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  2 ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}

					if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  4 ] ) == true )
					{
						buttonFlags |= R1 ;
					}
					if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  5 ] ) == true )
					{
						buttonFlags |= L1 ;
					}

					if( profile.ButtonNumbers[  6 ] >= 0 )
					{
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  6 ] ) == true )
						{
							buttonFlags |= R2 ;
						}
					}
					else
					{
						float axis = GetGamePadAxis( gamePadId, profile.AxisNumbers[  6 ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= R2 ;
						}
					}

					if( profile.ButtonNumbers[  7 ] >= 0 )
					{
						if( GetGamePadButton( gamePadId, profile.ButtonNumbers[ 7 ] ) == true )
						{
							buttonFlags |= L2 ;
						}
					}
					else
					{
						float axis = GetGamePadAxis( gamePadId, profile.AxisNumbers[  7 ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= L2 ;
						}
					}

					if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  8 ] ) == true )
					{
						buttonFlags |= R3 ;
					}
					if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  9 ] ) == true )
					{
						buttonFlags |= L3 ;
					}
					if( GetGamePadButton( gamePadId, profile.ButtonNumbers[ 10 ] ) == true )
					{
						buttonFlags |= O1 ;
					}
					if( GetGamePadButton( gamePadId, profile.ButtonNumbers[ 11 ] ) == true )
					{
						buttonFlags |= O2 ;
					}
				}

				//---------------------------------------------------------

				return buttonFlags ;
			}

			/// <summary>
			/// ボタンが押されているかどうか判定する
			/// </summary>
			/// <param name="buttonIdentity"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonIdentity, int playerNumber = -1 )
			{
				// 接続しているしているプレイヤー(最大４)
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToButtonEnabled == true )
					{
						// キーボードのキーのボタンへの割り当て(ループでは回さない)
						if( GetButtonByMappingKey( buttonIdentity ) == true )
						{
							return true ;
						}
					}
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					switch( buttonIdentity )
					{
						case B1 :
							if( SwapB1toB2 == false )
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  0 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  1 ] ) == true ){ return true ; }
							}
						break ;

						case B2 :
							if( SwapB1toB2 == false )
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  1 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  0 ] ) == true ){ return true ; }
							}
						break ;

						case B3 :
							if( SwapB3toB4 == false )
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  2 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  3 ] ) == true ){ return true ; }
							}
						break ;

						case B4 :
							if( SwapB3toB4 == false )
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  3 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  2 ] ) == true ){ return true ; }
							}
						break ;

						case R1 :
							if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  4 ] ) == true ){ return true ; }
						break ;

						case L1 :
							if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  5 ] ) == true ){ return true ; }
						break ;

						case R2 :
							if( profile.ButtonNumbers[  6 ] >= 0 )
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  6 ] ) == true ){ return true ; }
							}
							else
							{
								float axis = GetGamePadAxis( gamePadId, profile.AxisNumbers[  6 ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case L2 :
							if( profile.ButtonNumbers[  7 ] >= 0 )
							{
								if( GetGamePadButton( gamePadId, profile.ButtonNumbers[ 7 ] ) == true ){ return true ; }
							}
							else
							{
								float axis = GetGamePadAxis( gamePadId, profile.AxisNumbers[  7 ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case R3 :
							if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  8 ] ) == true ){ return true ; }
						break ;

						case L3 :
							if( GetGamePadButton( gamePadId, profile.ButtonNumbers[  9 ] ) == true ){ return true ; }
						break ;

						case O1 :
							if( GetGamePadButton( gamePadId, profile.ButtonNumbers[ 10 ] ) == true ){ return true ; }
						break ;

						case O2 :
							if( GetGamePadButton( gamePadId, profile.ButtonNumbers[ 11 ] ) == true ){ return true ; }
						break ;
					}
				}

				return false ;
			}

			/// <summary>
			/// アクシスが押されているかどうか判定する
			/// </summary>
			/// <param name="axisIdentity"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public Vector2 GetAxis( int axisIdentity, int playerNumber = -1 )
			{
				float oAxisX = 0 ;
				float oAxisY = 0 ;

				//----------------------------------

				// 接続しているしているプレイヤー(最大４)
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}
			
				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToAxisEnabled == true )
					{
						// キーボードのキーのアクシスへの割り当て

						// ＷＡＳＤ
						GetAxisByMappingKey_WASD( axisIdentity, ref oAxisX, ref oAxisY ) ;

						// カーソル
						GetAxisByMappingKey_Cursor( axisIdentity, ref oAxisX, ref oAxisY ) ;

						// ナンバー
						GetAxisByMappingKey_Number( axisIdentity, ref oAxisX, ref oAxisY ) ;
					}
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					// 縦軸の符号反転
					if( m_Owner.Invert == true && axisIdentity != TB )
					{
						oAxisY = - oAxisY ;
					}

					return new Vector2( oAxisX, oAxisY ) ; ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				var gamePadIds = Input.GetConnectedJoypads() ;
				
				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					float axisX, axisY ;

					switch( axisIdentity )
					{
						case DP :
							// SCX
							axisX = GetGamePadAxis( gamePadId, profile.AxisNumbers[  0 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SCY
							axisY = GetGamePadAxis( gamePadId, profile.AxisNumbers[  1 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case LS :
							// SLX
							axisX = GetGamePadAxis( gamePadId, profile.AxisNumbers[  2 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SLY
							axisY = GetGamePadAxis( gamePadId, profile.AxisNumbers[  3 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case RS :
							// SRX
							axisX = GetGamePadAxis( gamePadId, profile.AxisNumbers[  4 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SRY
							axisY = GetGamePadAxis( gamePadId, profile.AxisNumbers[  5 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case TB :
							// R2
							axisX = GetGamePadAxis( gamePadId, profile.AxisNumbers[  6 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
								if( oAxisX <  0 )
								{
									oAxisX  = - oAxisX ;
								}
							}

							// L2
							axisY = GetGamePadAxis( gamePadId, profile.AxisNumbers[  7 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
								if( oAxisY <  0 )
								{
									oAxisY  = - oAxisY ;
								}
							}
						break ;
					}
				}

				// 縦軸の符号反転
				if( m_Owner.Invert == true && axisIdentity != TB )
				{
					oAxisY = - oAxisY ;
				}

				return new Vector2( oAxisX, oAxisY ) ;
			}

			//----------------------------------------------------------

			private static bool GetGamePadButton( int gamePadId, int buttonNumber )
			{
				bool state = false ;

				switch( buttonNumber )
				{
					case  0 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.A )				; break ;
					case  1 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.B )				; break ;
					case  2 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.X )				; break ;
					case  3 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.Y )				; break ;
					case  4 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.RightShoulder )	; break ;
					case  5 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.LeftShoulder )		; break ;
					case  6 : state = ( Input.GetJoyAxis( gamePadId, JoyAxis.TriggerRight ) >  0.75f )	; break ;
					case  7 : state = ( Input.GetJoyAxis( gamePadId, JoyAxis.TriggerLeft  ) >  0.75f )	; break ;
					case  8 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.RightStick )		; break ;
					case  9 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.LeftStick )		; break ;
					case 10 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.Start )			; break ;
					case 11 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.Back )				; break ;
					case 12 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.Touchpad )			; break ;
					case 13 : state = Input.IsJoyButtonPressed( gamePadId, JoyButton.Guide )			; break ;
				}

				return state ;
			}

			private static float GetGamePadAxis( int gamePadId, int axisNumber )
			{
				float value = 0 ;

				switch( axisNumber )
				{
					case  0 : value = ( Input.IsJoyButtonPressed( gamePadId, JoyButton.DpadLeft ) == true ? -1 : 0 ) + ( Input.IsJoyButtonPressed( gamePadId, JoyButton.DpadRight ) == true ? +1 : 0 ) ; break ;
					case  1 : value = ( Input.IsJoyButtonPressed( gamePadId, JoyButton.DpadDown ) == true ? -1 : 0 ) + ( Input.IsJoyButtonPressed( gamePadId, JoyButton.DpadUp    ) == true ? +1 : 0 ) ; break ;
					case  2 : value =   Input.GetJoyAxis( gamePadId, JoyAxis.LeftX )		; break ;
					case  3 : value = - Input.GetJoyAxis( gamePadId, JoyAxis.LeftY )		; break ;
					case  4 : value =   Input.GetJoyAxis( gamePadId, JoyAxis.RightX )		; break ;
					case  5 : value = - Input.GetJoyAxis( gamePadId, JoyAxis.RightY )		; break ;
					case  6 : value =   Input.GetJoyAxis( gamePadId, JoyAxis.TriggerRight )	; break ;
					case  7 : value =   Input.GetJoyAxis( gamePadId, JoyAxis.TriggerLeft  )	; break ;
				}

				// Ｙ値は一旦、上を＋・下を－、に揃える(左手系)

				//---------------------------------

				float sign = Mathf.Sign( value ) ;
				value = Mathf.Abs( value ) ;
				if( value <  m_AxisLowerThreshold )
				{
					// チャタリング防止
					value = 0 ;
				}
				else
				{
					if( value >  m_AxisUpperThreshold )
					{
						// 最大補正
						value = 1 ;
					}
					else
					{
						// フィッティング
						value = ( value - m_AxisLowerThreshold ) / ( m_AxisUpperThreshold - m_AxisLowerThreshold ) ;
					}
					value *= sign ;
				}

				//---------------------------------

				return value ;
			}

			//----------------------------------------------------------
			// 振動関係

			/// <summary>
			/// 振動を開始させる(範囲は 0～1)
			/// </summary>
			/// <param name="lowerSpeed"></param>
			/// <param name="upperSpeed"></param>
			/// <param name="duration"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool SetMotorSpeeds( float lowerSpeed, float upperSpeed, float duration = 1.0f, int playerNumber = -1 )
			{
				// 接続しているしているプレイヤー(最大４)
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					m_Players[ p ].SetMotorSpeeds( gamePadId, lowerSpeed, upperSpeed, duration ) ;
				}

				return true ;
			}

			/// <summary>
			/// 振動を停止させる
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool StopMotor( int playerNumber = -1 )
			{
				// 接続しているしているプレイヤー(最大４)
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					m_Players[ p ].StopMotor( gamePadId ) ;
				}

				return true ;
			}

			/// <summary>
			/// 振動を一時停止させる
			/// </summary>
			/// <returns></returns>
			public bool PauseHaptics()
			{
				// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;
				
				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				int p, ps, pe ;

				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					m_Players[ p ].PauseHaptics( gamePadId ) ;
				}

				return true ;
			}

			/// <summary>
			/// 振動を再開させる
			/// </summary>
			/// <returns></returns>
			public bool ResumeHaptics()
			{
				// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;
				
				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				int p, ps, pe ;

				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					m_Players[ p ].ResumeHaptics( gamePadId ) ;
				}

				return true ;
			}

			/// <summary>
			/// 振動を停止させる(パラメータもリセットされる)
			/// </summary>
			/// <returns></returns>
			public bool ResetHaptics()
			{
				// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
				int max = Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ) ;
				
				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				int p, ps, pe ;

				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;

				var gamePadIds = Input.GetConnectedJoypads() ;

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamePadId = gamePadIds[ p ] ;

					m_Players[ p ].ResetHaptics( gamePadId ) ;
				}

				return true ;
			}
		}
	}
}
