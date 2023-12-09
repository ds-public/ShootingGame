using Godot ;
using ExGodot ;
using System ;

using InputHelper ;
using AudioHelper ;
using StorageHelper ;
using SceneHelper ;

namespace Sample_001
{
	/// <summary>
	/// アプリケーションの全体管理クラス
	/// </summary>
	public partial class ApplicationManager : ExNode
	{
		// シングルトン
		private static ApplicationManager	m_Instance ;
		
		//-----------------------------------------------------------

		// ゲーム全体の時間経過
		private double	m_MasterTime ;

		// ゲーム全体の時間経過(差分)
		private double	m_MasterTimeDelta ;

		// マスタータイムがボーズ中かどうか
		private bool	m_IsMasterTimePausing ;

		//-----------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			// シングルトンインスタンスを保持する
			m_Instance = this ;

			//----------------------------------

			// マスタータイム(ゲーム全体の時間経過)　※ポーズを行いたいので自前で時間は管理する
			m_MasterTime = 0 ;

			// ポーズ処理で Engine.TimeScale は使用しない(止まっては困るものもあるため)

			//----------------------------------

			InputManager.Create( parent: this ) ;
			AudioManager.Create( parent: this ) ;
			StorageManager.Create( parent: this ) ;
			SceneManager.Create( parent: this ) ;

			InputManager.SetInputProcessingType( InputProcessingTypes.Parallel ) ;
			InputManager.SetCursorProcessing( true ) ;

			//----------------------------------

			// 描画フレームレートを設定
			Engine.MaxFps = 60 ;

			// 垂直同期を設定(無効化)
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled ) ;
			
			//----------------------------------

			Asset.Create( parent: this ) ;

			Preference.Create( parent: this ) ;

			Scene.Create( parent: this ) ;

			BGM.Create( parent: this ) ;
			SE.Create( parent: this ) ;

			//----------------------------------
			// 各種初期化

			BGM.Initialize() ;
			SE.Initialize() ;

			BGM.SetBaseVolume( 0.8f ) ;
			SE.SetBaseVolume( 0.8f ) ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( m_IsMasterTimePausing == false )
			{
				// タイマー増加
				m_MasterTimeDelta = delta ;
				m_MasterTime += delta ;
			}
			else
			{
				m_MasterTimeDelta = 0 ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マスタータイム
		/// </summary>
		public static double MasterTime
		{
			get
			{
				if( m_Instance == null )
				{
					// まだ準備が出来ていない
					return 0 ;
				}
				return m_Instance.m_MasterTime ;
			}
		}

		/// <summary>
		/// マスタータイム
		/// </summary>
		public static double MasterTimeDelta
		{
			get
			{
				if( m_Instance == null )
				{
					// まだ準備が出来ていない
					return 0 ;
				}
				return m_Instance.m_MasterTimeDelta ;
			}
		}

		/// <summary>
		/// マスタータイムを一時停止させる
		/// </summary>
		public static void PauseTime()
		{
			if( m_Instance == null )
			{
				return ;
			}
			
			m_Instance.m_IsMasterTimePausing = true ;
		}

		/// <summary>
		/// マスタータイムの一時停止を解除する
		/// </summary>
		public static void UnpauseTime()
		{
			if( m_Instance == null )
			{
				return ;
			}
			
			m_Instance.m_IsMasterTimePausing = false ;
		}
	}
}

