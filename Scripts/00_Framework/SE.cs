using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;

using AudioHelper ;

namespace Sample_001
{
	/// <summary>
	/// ＳＥファサードクラス(シングルトン)
	/// </summary>
	public partial class SE : ExNode
	{
		// インスタンス(シングルトン)
		private static SE m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static SE Instance	=> m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static SE Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var node = new SE()
			{
				Name = "SE"
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
		public override void _Process( double delta ){}

		//-----------------------------------------------------------

		// ルートフォルダ名
		public const string m_Root				= "AssetBundle/Audio/SE/" ;

		public const string None				= null ;

		// System
		public const string Start				= "System/001_Start.wav" ;
		public const string Decision			= "System/002_Decision.wav" ;
		public const string Cancel				= "System/003_Cancel.wav" ;
		public const string Selection			= "System/004_Selection.wav" ;

		public const string SpacialCommand		= "System/573_SpacialCommand.mp3" ;

		public const string NoDeath				= "System/999_NoDeath.wav" ;

		// Battle
		public const string Shoot				= "Battle/001_Shoot.mp3" ;
		public const string Explosion			= "Battle/002_Explosion.mp3" ;
		public const string Bomb				= "Battle/003_Bomb.mp3" ;

		public const string Hit					= "Battle/005_Hit.wav" ;

		public const string Item_P				= "Battle/006_Item_P.wav" ;
		public const string Item_B				= "Battle/007_Item_B.wav" ;
		public const string Item_S				= "Battle/008_Item_S.wav" ;

		public const string Bonus				= "Battle/009_Bonus.mp3" ;

		public const string Victory				= "Battle/012_Victory.mp3" ;

		public const string PauseOn				= "Battle/016_Pause.wav" ;

		//-----------------------------------------------------------

		/// <summary>
		/// カテゴリー単位での処理用のタグ名
		/// </summary>
		public const string TagName				= "SE" ;

		/// <summary>
		/// 最大チャンネル数
		/// </summary>
		public const int	MaxChannels			=   16 ;

		//-----------------------------------------------------------

		/// <summary>
		/// ワーク変数初期化
		/// </summary>
		public static void Initialize()
		{
			// 最大チャンネル数を設定する
			AudioManager.SetMaxChannels( TagName, MaxChannels ) ;
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

		/// <summary>
		/// 常駐させておきたいＳＥをロードしておく
		/// </summary>
		/// <returns></returns>
		public static async Task LoadExternalAsync()
		{
			string path ;

			string[] seNames =
			{
				Start, Decision, Cancel, Selection, SpacialCommand, NoDeath,
				Shoot, Explosion, Bomb, Hit, Item_P, Item_B, Item_S, Bonus, Victory, PauseOn
			} ;

			// Ｃ＃の方には preload は存在しない
			// GD.Load と ResourceLoader.Load は同じもの

			// 将来的には AssetBundleManager Asset クラスを用意する

			foreach( string seName in seNames )
			{
				( string correctedPath, float correctedVolume  ) = Correct( seName ) ;
				path	= correctedPath ;

				await Asset.LoadAsync<AudioStream>( path, Asset.CachingTypes.Same ) ;
			}

			// 以下は後で適切な保持方法に変える(ゲーム全般を通して保持しておく必要は無い)

//			path = m_Root + "SE/Character" ;
//			await Asset.LoadAssetBundleAsync( path, true ) ;	// シーンが変わっても破棄しない

//			path = m_Root + "SE/Weapon" ;
//			await Asset.LoadAssetBundleAsync( path, true ) ;	// シーンが変わっても破棄しない
		}

		//-------------------------------------------------------------------------------------------

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
		/// 再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子(-1で失敗)</returns>
		public static int Play( string path, float volume = 1.0f, float pan = 0, float pitch = 0.0f, bool loop = false )
		{
			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			//----------------------------------

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			var audioClip = Asset.Load<AudioStream>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				return -1 ;
			}

			// 再生する
			return AudioManager.Play( audioClip, loop, volume, pan, pitch, 0.0f, TagName ) ;
		}

		/// <summary>
		/// 再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async Task<int> PlayAsync( string path, float volume = 1.0f, float pan = 0, float pitch = 0.0f, bool loop = false )
		{
			string originalPath = path ;

			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			//----------------------------------

			int playId ;

			if( Asset.Exists( originalPath ) == true )
			{
				// 既にあるなら同期で高速再生
				playId = Play( originalPath, volume, pan, pitch, loop ) ;
				return playId ;
			}

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			var audioClip = await Asset.LoadAsync<AudioStream>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			// 再生する
			playId = AudioManager.Play( audioClip, loop, volume, pan, pitch, 0.0f, TagName ) ;
			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			// 成功
			return playId ;
		}

		/// <summary>
		/// ３Ｄ空間想定で再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="position">音源の位置(ワールド座標系)</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		public static int Play3D( string path, Vector3 position, Transform3D? listener = null, float scale = 1, float volume = 1.0f, float pitch = 0.0f, bool loop = false )
		{
			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			//----------------------------------

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			var audioClip = Asset.Load<AudioStream>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				return -1 ;
			}
			
			// 再生する
			return AudioManager.Play3D( audioClip, position, listener, scale, loop, volume, pitch, 0.0f, TagName ) ;
		}
		
		/// <summary>
		/// ３Ｄ空間想定で再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="position">音源の位置(ワールド座標系)</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <returns>列挙子</returns>
		public static async Task<int> Play3DAsync( string path, Vector3 position, Transform3D? listener = null, float scale = 1, float volume = 1.0f, float pitch = 0.0f, bool loop = false )
		{
			// テーブルで補正する
			( string correctedPath, float correctedVolume  ) = Correct( path ) ;
			path	= correctedPath ;
			volume *= correctedVolume ;

			//----------------------------------

			int playId ;

			if( Asset.Exists( path ) == true )
			{
				// 既にあるなら高速再生
				return Play3D( path, position, listener, scale, volume, pitch, loop ) ;
			}

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioStream audioClip = await Asset.LoadAsync<AudioStream>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			// 再生する
			playId = AudioManager.Play3D( audioClip, position, listener, scale, loop, volume, pitch, 0.0f, TagName ) ;
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
		/// 停止する
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
		/// 停止するまで待つ
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
		/// 識別子で指定する発音が再生中か確認する(一時停止中は停止扱いになる)
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId )== false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 再生中のチャンネル数を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetPlayingCount()
		{
			return AudioManager.IsPlaying( TagName ) ;
		}

		/// <summary>
		/// 一時停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			return AudioManager.Pause( playId ) ;
		}

		/// <summary>
		/// 再開する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			return AudioManager.Unpause( playId ) ;
		}

		/// <summary>
		/// 識別子で指定する発音が再生中か確認する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( playId <  0 || AudioManager.IsPausing( playId )== false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 全発音を完全停止する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return AudioManager.StopAll( TagName ) ;
		}

		/// <summary>
		/// 全発音を一時停止する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return AudioManager.PauseAll( TagName ) ;
		}

		/// <summary>
		/// 全発音を再開する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return AudioManager.UnpauseAll( TagName ) ;
		}

		//-------------------------------------------------------------------------------------------

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
