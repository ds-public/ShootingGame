using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;

using AudioHelper ;

namespace Sample_001
{
	/// <summary>
	/// ＢＧＭファサードクラス(シングルトン)
	/// </summary>
	public partial class BGM : ExNode
	{
		// インスタンス(シングルトン)
		private static BGM m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static BGM Instance	=> m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static BGM Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var node = new BGM()
			{
				Name = "BGM"
			} ;

			if( parent == null )
			{
				node.GetTree().Root.AddChild( node ) ;
			}
			else
			{
				parent.AddChild( node ) ;
			}

			return node ;
		}

		/// <summary>
		/// マネージャを破棄する
		/// </summary>
		public static void Delete()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.QueueFree() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			m_Instance = this ;
		}

		/// <summary>
		/// インスタンスがツリーから除外される際に呼び出される
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree() ;

			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
//		public override void _Process( double delta ){}

		//-----------------------------------------------------------

		// ルートフォルダ名
		public const string m_Root				= "AssetBundle/Audio/BGM/" ;

		public const string None				= null ;

		public const string Title				= "001_Title.ogg" ;
		public const string Battle				= "010_Stage_01.ogg" ;
		public const string Defeat				= "003_Defeat.ogg" ;
		public const string Victory				= "004_Victory.ogg" ;

		public const string Stage_01			= "011_Stage_01.ogg" ;
		public const string Stage_02			= "012_Stage_02.ogg" ;
		public const string Stage_03			= "013_Stage_03.ogg" ;
		public const string Stage_04			= "014_Stage_04.ogg" ;
		public const string Stage_05			= "015_Stage_05.ogg" ;
		public const string Stage_06			= "016_Stage_06.ogg" ;

		public const string Boss				= "040_Boss.ogg" ;

		//-----------------------------------------------------------

		/// <summary>
		/// カテゴリー単位での処理用のタグ名
		/// </summary>
		public const string	TagName				= "BGM" ;

		/// <summary>
		/// 最大チャンネル数
		/// </summary>
		public const int	MaxChannels			=    4 ;

		//-----------------------------------------------------------

		// メインＢＧＭのオーディオチャンネルインスタンスを保持する
		private static string	m_MainBGM_RequestPath		= string.Empty ;
		private static string	m_MainBGM_Path				= string.Empty ;
		private static float	m_MainBGM_Volume			=  1 ;
		private static float	m_MainBGM_Pan				=  0 ;

		private static int		m_MainBGM_PlayId			= -1 ;

		private static string	m_MainBGM_Reserved_Path		=  string.Empty ;
		private static float	m_MainBGM_Reserved_Volume	= 1 ;
		private static float	m_MainBGM_Reserved_Pan		= 0 ;

		//-----------------------------------------------------------

		/// <summary>
		/// ワーク変数初期化
		/// </summary>
		public static void Initialize()
		{
			// 最大チャンネル数を設定する
			AudioManager.SetMaxChannels( TagName, MaxChannels ) ;

			//----------------------------------

			m_MainBGM_RequestPath		= string.Empty ;
			m_MainBGM_Path				= string.Empty ;

			m_MainBGM_Volume			=  1 ;
			m_MainBGM_Pan				=  0 ;

			m_MainBGM_PlayId			= -1 ;

			m_MainBGM_Reserved_Path		=  string.Empty ;
			m_MainBGM_Reserved_Volume	= 1 ;
			m_MainBGM_Reserved_Pan		= 0 ;
		}

		/// <summary>
		/// 現在再生中のＢＧＭのパスを取得する
		/// </summary>
		/// <returns></returns>
		public static string GetCurrent()
		{
			return m_MainBGM_Path ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// タイトル前から必要なアセットをロードする
		/// </summary>
		/// <returns></returns>
		public static async Task LoadInternalAsync()
		{
			// アセットバンドルの強制ダウンロードを行う(失敗してもダイアログは出さない)
//			if( Asset.Exists( BGM.Title ) == false )
//			{
//				await Asset.DownloadAssetBundleAsync( BGM.Title, true ) ;
//			}

			await m_Instance.Yield() ;
		}

		// パスの保険
		private static ( string, float ) Correct( string path )
		{
			float volume = 1.0f ;

//			var record = MasterCache.Current.Audio.FindByName( path ) ;
//
//			// マスターデータに該当するレコードが存在する
//			if( record != null )
//			{
//				path	= record.Path ;
//				volume	= record.Volume ;
//
//				if( path.IndexOf( m_InternalPath ) <  0 )
//				{
//					if( path.IndexOf( m_Path ) <  0 )
//					{
//						path = m_Root + path ;
//					}
//				}
//			}

			path = m_Root + path ;

			return ( path, volume ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMain( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, bool restart = false, float offset = 0.0f )
		{
			string originPath = path ;

			if( m_MainBGM_RequestPath == originPath )
			{
				return true ;	// 既に再生リクエスト中
			}

			m_MainBGM_RequestPath = originPath ;

			//----------------------------------

			if( restart == false )
			{
				// 既に同じ曲が鳴っていたらスルーする
				if( originPath == m_MainBGM_Path )
				{
					m_MainBGM_RequestPath = string.Empty ;
					return true ;
				}
			}

			//----------------------------------

			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;

			path	= correctedPath ;
			volume *= correctedVolume ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			var audioClip = Asset.Load<AudioStream>( path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				return false ;
			}

			//----------------------------------------------------------

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, offset, TagName ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, offset, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				m_MainBGM_RequestPath = string.Empty ;
				return false ;
			}

			m_MainBGM_Path		= originPath ;
			m_MainBGM_Volume	= volume ;
			m_MainBGM_Pan		= pan ;

			m_MainBGM_PlayId	= playId ;

			m_MainBGM_RequestPath = string.Empty ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async Task<int> PlayMainAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, bool restart = false, float offset = 0.0f )
		{
			string originPath = path ;

			if( m_MainBGM_RequestPath == originPath )
			{
				return -1 ;	// 既に再生リクエスト中
			}

			m_MainBGM_RequestPath = originPath ;

			//----------------------------------

			if( restart == false )
			{
				// 既に同じ曲が鳴っていたらスルーする
				if( originPath == m_MainBGM_Path )
				{
					m_MainBGM_RequestPath = string.Empty ;
					return m_MainBGM_PlayId ;
				}
			}

			//----------------------------------

			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			var audioClip = await Asset.LoadAsync<AudioStream>( path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			await m_Instance.Yield() ;

			//----------------------------------------------------------

			// ＢＧＭを再生する
			int playId ;
			
			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, offset, TagName ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, offset, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				m_MainBGM_RequestPath = string.Empty ;
				return -1 ;
			}

			m_MainBGM_Path		= originPath ;
			m_MainBGM_Volume	= volume ;
			m_MainBGM_Pan		= pan ;

			m_MainBGM_PlayId	= playId ;

			m_MainBGM_RequestPath = string.Empty ;

			// 成功
			return m_MainBGM_PlayId ;
		}

		/// <summary>
		/// メインＢＧＭを退避する
		/// </summary>
		/// <returns></returns>
		public static bool ReserveMainBGM()
		{
			if( m_MainBGM_PlayId <  0 )
			{
				Debug.Log( "<color=#FFFF00>退避するBGMは無い</color>" ) ;

				m_MainBGM_Reserved_Path		= string.Empty ;
				m_MainBGM_Reserved_Volume	= 1 ;
				m_MainBGM_Reserved_Pan		= 0 ;

				return false ;
			}

			m_MainBGM_Reserved_Path		= m_MainBGM_Path ;
			m_MainBGM_Reserved_Volume	= m_MainBGM_Volume ;
			m_MainBGM_Reserved_Pan		= m_MainBGM_Pan ;

			return true ;
		}

		/// <summary>
		/// 退避中のメインＢＧＭを復帰させる
		/// </summary>
		/// <returns></returns>
		public static async Task<bool> RestoreMainBGM( float fadeTime = 0 )
		{
			if( string.IsNullOrEmpty( m_MainBGM_Reserved_Path ) == true )
			{
				Debug.Log( "<color=#FFFF00>復帰するBGMは無い</color>" ) ;
				return false ;
			}

			int playId = await PlayMainAsync( m_MainBGM_Reserved_Path, fadeTime, m_MainBGM_Reserved_Volume, m_MainBGM_Reserved_Pan, true ) ;

			ClearReservedMainBGM() ;

			return ( playId != -1 ) ;
		}

		/// <summary>
		/// 退避中のメインＢＧＭ情報を消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearReservedMainBGM()
		{
			if( string.IsNullOrEmpty( m_MainBGM_Reserved_Path ) == true )
			{
				return false ;
			}

			m_MainBGM_Reserved_Path		= null ;
			m_MainBGM_Reserved_Volume	= 1 ;
			m_MainBGM_Reserved_Pan		= 0 ;

			return true ;
		}

		/// <summary>
		/// 現在再生中のメインＢＧＭと退避中のメインＢＧＭが同じか判定する
		/// </summary>
		/// <returns></returns>
		public static bool IsSameReservedMainBGM()
		{
			return m_MainBGM_Path == m_MainBGM_Reserved_Path ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭを停止する
		/// </summary>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopMain( float fade = 0 )
		{
			m_MainBGM_RequestPath = string.Empty ;

			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;
				return false ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
			}

			m_MainBGM_Path		= string.Empty ;
			m_MainBGM_PlayId	= -1 ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="fade"></param>
		/// <returns></returns>
		public static async Task<int> StopMainAsync( float fade = 0 )
		{
			m_MainBGM_RequestPath = string.Empty ;

			if( m_MainBGM_PlayId <  0 )
			{
				return -1 ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;
				return -1 ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;

				int mainBGM_PlayId	= m_MainBGM_PlayId ;
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;

				return mainBGM_PlayId ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
				
				await m_Instance.WaitWhile( () => AudioManager.IsPlaying( m_MainBGM_PlayId ) == true ) ;

				int mainBGM_PlayId	= m_MainBGM_PlayId ;
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;

				return mainBGM_PlayId ;
			}
		}

		/// <summary>
		/// メインＢＧＭを一時停止する
		/// </summary>
		/// <returns></returns>
		public static bool PauseMain()
		{
			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			return AudioManager.Pause( m_MainBGM_PlayId ) ;
		}

		/// <summary>
		/// メインＢＧＭを再開する
		/// </summary>
		/// <returns></returns>
		public static bool UnpauseMain()
		{
			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			return AudioManager.Unpause( m_MainBGM_PlayId ) ;
		}

		/// <summary>
		/// メインＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="audioClipName ">曲の名前(再生中の曲の種類を限定したい場合は指定する)</param>
		/// <returns>再生状況(true=再生中・false=停止中)</returns>
		public static bool IsPlayingMain( string path = null )
		{
			// 何かの曲は再生中か
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				// 何も再生されていない
				return false ;
			}

			//----------------------------------------------------------
			
			// 何かしらの曲は鳴っている
			if( string.IsNullOrEmpty( path ) == false )
			{
				// 曲の名前の指定がある

				if( m_MainBGM_Path != path )
				{
					// 指定した曲は鳴っていない
					return false ;
				}
			}

			return true ;
		}

#if false
		/// <summary>
		/// 再生中のメインＢＧＭの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetMainName()
		{
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId )== false )
			{
				return string.Empty ;
			}

			return AudioManager.GetName( m_MainBGM_PlayId ) ;
		}
#endif
		//-----------------------------------------------------------

		/// <summary>
		/// ＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子</returns>
		public static int Play( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, float offset = 0.0f )
		{
			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			//----------------------------------

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			var audioClip = Asset.Load<AudioStream>( path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			//----------------------------------

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, offset, TagName ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, offset, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			return playId ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async Task<int> PlayAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, float offset = 0.0f )
		{
			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			//----------------------------------

			int playId ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			var audioClip = await Asset.LoadAsync<AudioStream>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			await m_Instance.Yield() ;

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, offset, TagName ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, offset, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			// 成功
			return playId ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＢＧＭを停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int playId, float fade = 0 )
		{
			if( fade <= 0 )
			{
				// フェードなし停止
				return AudioManager.Stop( playId ) ;
			}
			else
			{
				// フェードあり停止
				return AudioManager.StopFade( playId, fade ) ;
			}
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade"></param>
		/// <returns></returns>
		public static async Task<bool> StopAsync( int playId, float fade = 0 )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId ) == false )
			{
				return false ;	// 識別子が不正か元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( playId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( playId, fade ) ;
				
				await m_Instance.WaitWhile( () => AudioManager.IsPlaying( playId ) == true ) ;
			}

			return true ;
		}

		/// <summary>
		/// ＢＧＭを一時停止する
		/// </summary>
		/// <returns></returns>
		public static bool Pause( int playId )
		{
			if( playId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			return AudioManager.Pause( playId ) ;
		}

		/// <summary>
		/// ＢＧＭを再開する
		/// </summary>
		/// <returns></returns>
		public static bool Unpause( int playId )
		{
			if( playId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			return AudioManager.Unpause( playId ) ;
		}

		/// <summary>
		/// 識別子で指定するＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId )== false )
			{
				return false ;  // 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// カクツキ防止のためにキャッシュにロードしておく
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <returns>列挙子</returns>
		public static async Task<bool> LoadAsync( string path )
		{
			//----------------------------------

			// テーブルで補正する
			( string correctedPath, float _ ) = Correct( path ) ;
			path	= correctedPath ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
//			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( path, Asset.CachingTypes.ResourceOnly ) ;
			AudioStream audioClip = GD.Load<AudioStream>( path ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return false ;
			}

			await m_Instance.Yield() ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭのボリュームを設定する(オプションからリアルタイムに変更するケースで使用する value = GetVolume() )
		/// </summary>
		public static float OptionalVolume
		{
			set
			{
				if( m_MainBGM_PlayId >= 0 )
				{
					AudioManager.SetVolume( m_MainBGM_PlayId, m_MainBGM_Volume * value ) ;
				}
			}
		}
		
		/// <summary>
		/// メインＢＧＭのボリュームを設定する
		/// </summary>
		public static float Volume
		{
			set
			{
				if( m_MainBGM_PlayId >= 0 )
				{
					m_MainBGM_Volume = value ;
					AudioManager.SetVolume( m_MainBGM_PlayId, m_MainBGM_Volume ) ;
				}
			}
		}
		
		/// <summary>
		/// ベースボリュームを設定する
		/// </summary>
		/// <param name="baseVolume"></param>
		public static void SetBaseVolume( float baseVolume )
		{
			AudioManager.SetBaseVolume( TagName, baseVolume ) ;
		}
	}
}
