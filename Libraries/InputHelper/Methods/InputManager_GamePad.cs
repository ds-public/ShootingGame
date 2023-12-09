using Godot ;


namespace InputHelper
{
	public partial class InputManager
	{
		// GamePad 関係

		//-------------------------------------------------------------------------------------------
		// 互換メソッド

		/// <summary>
		/// 接続中のゲームパッドの数
		/// </summary>
		public static int NumberOfGamePads
			=> GamePad.NumberOfGamePads ;

		/// <summary>
		/// 接続中のゲームパッドの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetJoystickNames()
			=> GamePad.GetJoystickNames() ;

		//---------------
		// ボタン関連

		/// <summary>
		/// ボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		public static bool GetButton( string buttonName )
			=> GamePad.GetButton( buttonName ) ;

		/// <summary>
		/// ボタンが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		public static bool GetButtonDown( string buttonName )
			=> GamePad.GetButtonDown( buttonName ) ;

		/// <summary>
		/// ボタンが離されたかどうかの判定
		/// </summary>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		public static bool GetButtonUp( string buttonName )
			=> GamePad.GetButtonUp( buttonName ) ;

		//---------------
		// アクシス関連

		/// <summary>
		/// アクシスの状態を取得
		/// </summary>
		/// <param name="axisName"></param>
		/// <returns></returns>
		public static float GetAxis( string axisName )
			=> GamePad.GetAxis( axisName ) ;

		//-----------------------------------------------------------
		// 独自メソッド

		//---------------
		// ボタン関連

		/// <summary>
		/// 全てのボタンが押されているかどうか判定する
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static int GetButtonAll( int playerNumber = -1 )
			=> GamePad.GetButtonAll( playerNumber ) ;

		/// <summary>
		/// ボタンが押されているかどうか判定する
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonIdentity, int playerNumber = -1 )
			=> GamePad.GetButton( buttonIdentity, playerNumber ) ;

		/// <summary>
		/// ボタンが押されたかどうか判定する
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButtonDown( int buttonIdentity, int playerNumber = -1, bool isFixed = false )
			=> GamePad.GetButtonDown( buttonIdentity, playerNumber, isFixed ) ;

		/// <summary>
		/// ボタンが離されたかどうか判定する
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonIdentity, int playerNumber = -1, bool isFixed = false )
			=> GamePad.GetButtonUp( buttonIdentity, playerNumber, isFixed ) ;

		/// <summary>
		/// ボタンが押されているかどうか判定する(リピート有効)
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButtonRepeat( int buttonIdentity, int playerNumber = -1, bool isFixed = false )
			=> GamePad.GetButtonRepeat( buttonIdentity, playerNumber, isFixed ) ;

		//---------------
		// アクシス関連

		/// <summary>
		/// アクシスが押されているかどうか判定する
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxis( int axisIdentity, int playerNumber = -1 )
			=> GamePad.GetAxis ( axisIdentity, playerNumber ) ;

		/// <summary>
		/// アクシス(デジタル扱い)が押されたかどうか判定する
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisDown( int axisIdentity, int playerNumber = -1, bool isFixed = false )
			=> GamePad.GetAxisDown( axisIdentity, playerNumber, isFixed ) ;

		/// <summary>
		/// アクシス(デジタル扱い)が離されたかどうか判定する
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisUp( int axisIdentity, int playerNumber = -1, bool isFixed = false )
			=> GamePad.GetAxisUp( axisIdentity, playerNumber, isFixed ) ;

		/// <summary>
		/// アクシス(デジタル扱い)が押されているかどうか判定する(リピート有効)
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisRepeat( int axisIdentity, int playerNumber = -1, bool isFixed = false )
			=> GamePad.GetAxisRepeat( axisIdentity, playerNumber, isFixed ) ;

		//---------------
		// 振動関連

		/// <summary>
		/// 振動を開始させる(範囲は 0～1)
		/// </summary>
		/// <param name="lowerSpeed"></param>
		/// <param name="upperSpeed"></param>
		/// <param name="duration"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool SetMotorSpeeds( float lowerSpeed, float upperSpeed, float duration = 1.0f, int playerNumber = -1 )
			=> GamePad.SetMotorSpeeds( lowerSpeed, upperSpeed, duration, playerNumber ) ;

		/// <summary>
		/// 振動を停止させる
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool StopMotor( int playerNumber = -1 )
			=> GamePad.StopMotor( playerNumber ) ;

		/// <summary>
		/// 振動を一時停止させる
		/// </summary>
		/// <returns></returns>
		public static bool PauseHaptics()
			=> GamePad.PauseHaptics() ;

		/// <summary>
		/// 振動を再開させる
		/// </summary>
		/// <returns></returns>
		public static bool ResumeHaptics()
			=> !GamePad.ResumeHaptics() ;

		/// <summary>
		/// 振動を停止させる(パラメータもリセットされる)
		/// </summary>
		/// <returns></returns>
		public static bool ResetHaptics()
			=> GamePad.ResetHaptics() ;

		//---------------
		// その他

		/// <summary>
		/// 全てゲームパッドの有効状況
		/// </summary>
		public static bool	GamePadEnabled
		{
			get{ return GamePad.Enabled ; }
			set{ GamePad.Enabled = value ; }
		}


		/// <summary>
		/// アクシスのしきい値(上限)
		/// </summary>
		public static float AxisUpperThreshold
		{
			get{ return GamePad.AxisUpperThreshold ; }
			set{ GamePad.AxisUpperThreshold = value ; }
		}

		/// <summary>
		/// アクシスのしきい値(下限)
		/// </summary>
		public static float AxisLowerThreshold
		{
			get{ return GamePad.AxisLowerThreshold ; }
			set{ GamePad.AxisLowerThreshold = value ; }
		}

		/// リピートを開始するまでの時間(秒)
		/// </summary>
		public static float RepeatStartingTime
		{
			get{ return GamePad.RepeatStartingTime ; }
			set{ GamePad.RepeatStartingTime = value ; }
		}

		/// <summary>
		/// リピートを繰り返す間隔の時間(秒)
		/// </summary>
		public static float RepeatIntervalTime
		{
			get{ return GamePad.RepeatIntervalTime ; }
			set{ GamePad.RepeatIntervalTime = value ; }
		}

		/// <summary>
		/// 完全アナログ値をデジタルと認識するしきい値
		/// </summary>
		public static float AnalogToDigitalThreshold
		{
			get{ return GamePad.AnalogToDigitalThreshold ; }
			set{ GamePad.AnalogToDigitalThreshold = value ; }
		}
		
		/// <summary>
		/// ボタン１とボタン２の入れ替え
		/// </summary>
		public static bool SwapB1toB2
		{
			get{ return GamePad.SwapB1toB2 ; }
			set{ GamePad.SwapB1toB2 = value ; }
		}

		/// <summary>
		/// ボタン３とボタン４の入れ替え
		/// </summary>
		public static bool SwapB3toB4
		{
			get{ return GamePad.SwapB3toB4 ; }
			set{ GamePad.SwapB3toB4 = value ; }
		}

		/// <summary>
		/// キーボードのボタンマッピング
		/// </summary>
		public static bool MappingKeyboardToButtonEnabled
		{
			get{ return GamePad.MappingKeyboardToButtonEnabled ; }
			set{ GamePad.MappingKeyboardToButtonEnabled = value ; }
		}

		/// <summary>
		/// キーボードのアクシスマッピング
		/// </summary>
		public static bool MappingKeyboardToAxisEnabled
		{
			get{ return GamePad.MappingKeyboardToAxisEnabled ; }
			set{ GamePad.MappingKeyboardToAxisEnabled = value ; }
		}

		/// <summary>
		/// プロファィル情報を追加する
		/// </summary>
		/// <param name="profileNumber">0～</param>
		/// <param name="buttonNumbers"></param>
		/// <param name="axisNumbers"></param>
		/// <param name="analogButtonCorrection"></param>
		/// <param name="analogButtonThreshold"></param>
		/// <returns></returns>
		public static bool AddProfile
		(
			int profileNumber,
			int[] buttonNumbers, int[] axisNumbers,
			bool analogButtonCorrection, float analogButtonThreshold
		)
			=>
			GamePad.AddProfile
			(
				profileNumber,
				buttonNumbers, axisNumbers,
				analogButtonCorrection,	analogButtonThreshold
			) ;

		/// <summary>
		/// プロファィル情報を追加する
		/// </summary>
		/// <param name="profileNumber">0～</param>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static bool AddProfile( int profileNumber, GamePad.Profile profile )
			=> GamePad.AddProfile( profileNumber, profile ) ;

		/// <summary>
		/// プロファイル情報を削除する
		/// </summary>
		/// <param name="profileNumber">0～</param>
		/// <returns></returns>
		public static bool RemoveProfile( int profileNumber )
			=> GamePad.RemoveProfile( profileNumber ) ;

		/// <summary>
		/// プレイヤーごとのプロフィール番号を設定する(初期はデフォルト＝－１が設定されている)
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static bool SetProfileNumber( int playerNumber, int profileNumber )
			=> GamePad.SetProfileNumber( playerNumber, profileNumber ) ;

		/// <summary>
		/// ボタンへの任意のキー群のマッピングを行う
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static bool SetMappingKeyboardToButton( int buttonIdentity, params KeyCodes[] keyCodes )
			=> GamePad.SetMappingKeyboardToButton( buttonIdentity, keyCodes ) ;

		/// <summary>
		/// ＷＡＳＤキーのアクシスへの割り当てを設定する
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static void SetMappingKeyboardToAxis_WASD( params int[] axisIdentities )
			=> GamePad.SetMappingKeyboardToAxis_WASD( axisIdentities ) ;

		/// <summary>
		/// カーソルキーのアクシスへの割り当てを設定する
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static void SetMappingKeyboardToAxis_Cursor( params int[] axisIdentities )
			=> GamePad.SetMappingKeyboardToAxis_Cursor( axisIdentities ) ;

		/// <summary>
		/// ナンバーキーのアクシスへの割り当てを設定する
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static void SetMappingKeyboardToAxis_Number( params int[] axisIdentities )
			=> GamePad.SetMappingKeyboardToAxis_Number( axisIdentities ) ;

		/// <summary>
		/// 接続中のゲームパッドの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetNames()
			=> GamePad.GetNames() ;
	}
}
