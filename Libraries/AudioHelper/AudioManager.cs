using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;


// https://zenn.dev/saitos/articles/godot-sync-audio

namespace AudioHelper
{
	/// <summary>
	/// オーディオ管理マネージャ Version 2023/11/29 0
	/// </summary>
	public partial class AudioManager : ExNode3D
	{
		// マネージャのインスタンス(シングルトン)
		private static AudioManager m_Instance = null ; 

		/// <summary>
		/// マネージャのインスタンス(シングルトン)
		/// </summary>
		public static AudioManager Instance => m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マネージャを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static AudioManager Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var audioManager = new AudioManager()
			{
				Name = "AudioManager"
			} ;

			if( parent == null )
			{
				audioManager.GetTree().Root.AddChild( audioManager ) ;
			}
			else
			{
				parent.AddChild( audioManager ) ;
			}

			return audioManager ;
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

			//----------------------------------

			// チャンネルリストをクリアする
			Channels.Clear() ;
		
			// ミュートリストをクリアする
			m_MuteList.Clear() ;
		
			// フェードプレイリストをクリアする
			m_FadePlayList.Clear() ;
		
			// フェードストップリストをクリアする
			m_FadeStopList.Clear() ;

			//----------------------------------

			// リスナーを張り付けておく
			CreateAudioListener( m_EnableListaner ) ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			// 毎フレーム処理を行う
			Process() ;
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

		//-------------------------------------------------------------------------------------------

		// リスナー（Awake 時にクリアされるのでシリアライズ化して保持する意味が無い）
		private AudioListener3D		m_Listener = null ;

		/// <summary>
		/// オーディオリスナーのインスタンス
		/// </summary>
		public  AudioListener3D		Listener{ get{ return m_Listener ; } }
		
		// ３Ｄの場合はリスナー用のカメラが１つ必要になる
		private Camera3D			m_ListenerCamera = null ;

		/// <summary>
		/// チャンネル情報
		/// </summary>
		public List<AudioChannel>	Channels = new () ;

		/// <summary>
		/// 最大チャンネル数
		/// </summary>
		public int Max = 32 ;
		
		//-----------------------------------------------------------

		// 発音毎に割り振られるユニークな識別子
		private int m_PlayId = 0 ;

		// １増加した発音毎に割り振られるユニークな識別子
		private int GetPlayId()
		{
			m_PlayId = ( m_PlayId + 1 ) & 0x7FFFFFFF ;
			return m_PlayId ;
		}

		//---------------

		/// <summary>
		/// タグ情報
		/// </summary>
		public class TagData
		{
			public float	BaseVolume ;
			public int		MaxChannels ;
		}

		// タグ情報群
		private readonly Dictionary<string, TagData> m_Tags = new () ;

		// ベースボリュームを追加する
		private void AddBaseVolume_Private( string tag, float baseVolume )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				m_Tags.Add( tag, new TagData(){ BaseVolume = baseVolume, MaxChannels = 0 } ) ;
			}
			else
			{
				m_Tags[ tag ].BaseVolume = baseVolume ;
			}
		}

		// ベースボリュームを取得する
		private float GetBaseVolume_Private( string tag )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return 1.0f ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				return 1.0f ;
			}
			else
			{
				return m_Tags[ tag ].BaseVolume ;
			}
		}

		// 最大チャンネル数を追加する
		private void AddMaxChannels_Private( string tag, int maxChannels )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				m_Tags.Add( tag, new TagData(){ BaseVolume = 1, MaxChannels = maxChannels } ) ;
			}
			else
			{
				m_Tags[ tag ].MaxChannels = maxChannels ;
			}
		}

		// タグごとの最大チャンネル数を取得する
		private int GetMaxChannels_Private( string tag )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return 0 ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				return 0 ;
			}
			else
			{
				return m_Tags[ tag ].MaxChannels ;
			}
		}

		//---------------

		/// <summary>
		/// スタートフェードとストップフェードのエフェクト
		/// </summary>
		internal protected class FadeEffect
		{
			/// <summary>
			/// フェード時間
			/// </summary>
			internal protected float Duration ;

			/// <summary>
			/// 基準時間
			/// </summary>
			internal protected float StartTime ;

			/// <summary>
			/// 開始音量
			/// </summary>
			internal protected float StartVolume ;

			/// <summary>
			/// 終了音量
			/// </summary>
			internal protected float EndVolume ;
		}
	
	
		// プレイフェード用
		private readonly Dictionary<AudioChannel,FadeEffect> m_FadePlayList = new () ;
		private readonly Dictionary<AudioChannel,FadeEffect> m_FadeStopList = new () ;
	
		/// <summary>
		/// マスターボリューム
		/// </summary>
		public float MasterVolume = 1.0f ;
	
		// ミュート
		private bool m_Mute = false ;
	
		// ミュートリスト
		private readonly List<string> m_MuteList = new () ;
	
		/// <summary>
		/// バックグラウンド再生を有効にするかどうか
		/// </summary>
		public bool m_RunInBackground = false ;

		/// <summary>
		/// バックグラウンド再生を有効にするかどうか
		/// </summary>
		public	static bool		  RunInBackground
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_RunInBackground ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_RunInBackground = value ;
			}
		}

		/// <summary>
		/// リスナーを有効にするかどうか
		/// </summary>
		private bool	m_EnableListaner = true ;

		// 初期化済みかの判定用
		private bool	m_IsInitialized ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		public delegate void OnStopped( int playId ) ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲート
		/// </summary>
		public OnStopped OnStoppedDelegate ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onStoppedDelegate">追加するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool AddOnStopped( OnStopped onStoppedDelegate )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnStoppedDelegate += onStoppedDelegate ;

			return true ;
		}
		
		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onStoppedDelegate">削除するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveOnStopped( OnStopped onStoppedDelegate )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnStoppedDelegate -= onStoppedDelegate ;

			return true ;
		}

		//-----------------------------------------------------

		// オーディオリスナーを生成する
		private bool CreateAudioListener( bool isEnabled )
		{
			if( m_Listener != null )
			{
				m_Listener.Visible = isEnabled ;
				m_Listener.SetProcess( isEnabled ) ;
				if( isEnabled == true ){ m_Listener.MakeCurrent() ; }else{ m_Listener.ClearCurrent() ; }
				return true ;	// 既に自身のオーディオリスナーを生成済みになっている
			}

			//----------------------------------

			// 存在しない(自身のオーディオリスナーを生成する)
			m_Listener = new AudioListener3D()
			{
				Name = "Listener"
			} ;
			AddChild( m_Listener ) ;

			m_ListenerCamera = new Camera3D()
			{
				Name = "ListenerCamera"
			} ;
			AddChild( m_ListenerCamera ) ;
			m_ListenerCamera.ClearCurrent() ;

//			m_Listener.Volume = 1.0f ;

			//----------------------------------

			AudioListener3D listener = FindObjectOfType<AudioListener3D>() ;

			if( listener == null )
			{
				// 存在しない

				m_Listener.Visible = isEnabled ;
				m_Listener.SetProcess( isEnabled ) ;
				if( isEnabled == true ){ m_Listener.MakeCurrent() ; }else{ m_Listener.ClearCurrent() ; }
			}
			else
			{
				// 存在する
				GD.Print( "[AudioManager] 既に AudioListener が存在します" ) ;

				m_Listener.Visible = false ;
				m_Listener.SetProcess( false ) ;		// 無効化しておく
				m_Listener.ClearCurrent() ;
			}

			//----------------------------------

			return true ;
		}

		/// <summary>
		/// アクティブなシーンに変更があった際に呼び出してもらう
		/// </summary>
		public static void OnActiveSceneChanged()
		{
			if( m_Instance == null )
			{
				return ;
			}
			m_Instance.OnActiveSceneChanged_Private() ;
		}

		// アクティブなシーンに変更があった際に呼び出してもらう
		private void OnActiveSceneChanged_Private()
		{
			bool isEnabled = true ;
			if( m_Listener != null )
			{
				isEnabled = m_Listener.IsProcessing() ;
			}

			CreateAudioListener( isEnabled ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 時間値を取得する
		private static float GetTime()
		{
			return ( float )( Time.GetTicksMsec() / 1000.0f ) ;
		}

		/// <summary>
		/// いずれかの空いているオーディオチャンネルを１つ取得する
		/// </summary>
		/// <returns>オーディオチャンネルのインスタンス</returns>
		public static AudioChannel GetChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetChannel_Private( null, null ) ;
		}
		
		// いずれかの空いているオーディオチャンネルを１つ取得する
		private AudioChannel GetChannel_Private( string tag, string path )
		{
			AudioChannel audioChannel ;

			int i, l = Channels.Count ;

			//----------------------------------

			if( string.IsNullOrEmpty( tag ) == false && m_Tags.ContainsKey( tag ) == true && m_Tags[ tag ].MaxChannels >  0 )
			{
				// タグごとのチャンネル数の制限が有る

				int count = 0 ;

				// 現在再生中のチャンネルで指定のタグに該当するものをカウントする
				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel != null && audioChannel.IsPlaying == true && audioChannel.Tag == tag )
					{
						count ++ ;
					}
				}

				if( count >= m_Tags[ tag ].MaxChannels )
				{
					// 既に最大数までチャンネルを使用している
					GD.Print( $"このタグの最大チャンネルまで使用済み: {count} /  {m_Tags[ tag ].MaxChannels} 再生出来なかったソース: {path}" ) ;
					return null ;	// 最大チャンネルまで使用済み
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				audioChannel = Channels[ i ] ;
				if( audioChannel != null && audioChannel.IsPlaying == false && audioChannel.IsPausing == false && audioChannel.IsLocking == false )
				{
//					Debug.Log( "空いているチャンネル:" + Channels[ i ].CueName + " P:" + Channels[ i ].IsPlaying + " P:" + Channels[ i ].IsPausing + " L:" + Channels[ i ].IsLocking ) ;

					// 空いているチャンネルを発見した
					return audioChannel ;
				}
			}
		
			//----------------------------------------------------------

			if( l >= Max )
			{
				GD.Print( $"最大チャンネルまで使用済み: {l} / {Max} 再生出来なかったソース: {path}" ) ;
				return null ;	// 最大チャンネルまで使用済み
			}
			
			//----------------------------------

			// 空いているソースが無いので追加する

			var source = new AudioStreamPlayer3D()
			{
				Name = "Source - " + Channels.Count.ToString( "D02" )
			} ;
			AddChild( source ) ;

			audioChannel = new AudioChannel( source ) ;
			Channels.Add( audioChannel ) ;
		
			return audioChannel ;
		}
	
		/// <summary>
		/// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		/// </summary>
		/// <returns>使用中の全てのチャンネルのインスタンス</returns>
		public static string[] GetUsedChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetUsedChannel_Private() ;
		}
	
		// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		private string[] GetUsedChannel_Private()
		{
			var list = new List<string>() ;
		
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true || Channels[ i ].IsPausing == true || Channels[ i ].IsLocking == true )
					{
						// 使用中のチャンネルを発見した
						list.Add( Channels[ i ].Name ) ;
					}
				}
			}
		
			if( list.Count == 0 )
			{
				return null ;
			}
		
			return list.ToArray() ;
		}
		
		// 指定した識別子に該当する再生中のオーディオチャンネルを取得する
		private AudioChannel GetChannelByPlayId( int playId )
		{
			AudioChannel audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( ( audioChannel.IsPlaying == true || audioChannel.IsPausing == true || audioChannel.IsLocking == true ) && audioChannel.PlayId == playId )
					{
						// 該当するチャンネルを発見した
						return audioChannel ;
					}
				}
			}
			
			// 該当するチャンネルが存在しない1
			return null ;
		}

		//---------------------------------------------------------
			
		/// <summary>
		/// サウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上)</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int Play( AudioStream audioClip, bool loop = false, float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f, float offset = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.Play_Private( audioClip, loop, volume, pan, pitch, offset, tag ) ;
		}
	
		//----------------
	
		// サウンドを再生する(オーディオクリップから)
		private int Play_Private( AudioStream audioClip, bool loop, float volume, float pan, float pitch, float offset, string tag )
		{
			string audioClipName = string.Empty ;
			if( audioClip != null )
			{
				audioClipName = audioClip.ResourceName ;
			}

			AudioChannel audioChannel = GetChannel_Private( tag, audioClipName ) ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}
		
			//-----------------------------------------------------
			
			int playId = GetPlayId() ;

			Vector3 position = Vector3.Zero ;

			if( m_Listener != null && m_Listener.IsCurrent() == true )
			{
				// マネージャのものが有効化されている
				position = m_Listener.Position ;
			}
			else
			{
				// その他のものを使用する
				AudioListener3D listener = FindObjectOfType<AudioListener3D>() ;
				if( listener != null )
				{
					position = listener.Position ;
				}
			}

			audioChannel.Play( playId, audioClip, loop, GetBaseVolume( tag ), volume, pan, position, false, pitch, offset, tag ) ;
		
			if( string.IsNullOrEmpty( tag ) == false && m_MuteList.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}
			
			// 成功したらソースのインスタンスを返す
			return playId ;
		}
	
		//---------------------------------
	
		/// <summary>
		/// フェード付きでサウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="duration">フェードの時間(秒)</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パンの位置(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int PlayFade( AudioStream audioClip, float duration = 1.0f, bool loop = false, float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f, float offset = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.PlayFade_Private( audioClip, duration, loop, volume, pan, pitch, offset, tag ) ;
		}
	
		//--------------------

		// フェード付きでサウンドを再生する(オーディオクリップから)
		private int PlayFade_Private( AudioStream audioClip, float duration, bool loop, float volume, float pan, float pitch, float offset, string tag )
		{
			string audioClipName = string.Empty ;
			if( audioClip != null )
			{
				audioClipName = audioClip.ResourceName ;
			}

			AudioChannel audioChannel = GetChannel_Private( tag, audioClipName ) ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}
		
			//-----------------------------------------------------
		
			// プレイフェードに登録する
		
			// 一旦破棄
			RemoveFadeList( audioChannel ) ;
		
			var effect = new FadeEffect()
			{
				Duration	= duration,
				StartTime   = GetTime(),
				StartVolume = 0,
				EndVolume   = 1
			} ;
		
			m_FadePlayList.Add( audioChannel, effect ) ;
		
			// フェード処理中であるというロックをかける
			audioChannel.Lock() ;
		
			//-----------------------------------------------------
			
			int playId = GetPlayId() ;

			Vector3 position = Vector3.Zero ;

			if( m_Listener != null && m_Listener.IsCurrent() == true )
			{
				// マネージャのものが有効化されている
				position = m_Listener.Position ;
			}
			else
			{
				// その他のものを使用する
				AudioListener3D listener = FindObjectOfType<AudioListener3D>() ;
				if( listener != null )
				{
					position = listener.Position ;
				}
			}

			// 再生する
			audioChannel.Play( playId, audioClip, loop, GetBaseVolume( tag ), volume, pan, position, false, pitch, offset, tag ) ;
		
			if( string.IsNullOrEmpty( tag ) == false && m_MuteList.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}
		
			// 成功したらソースのインスタンスを返す
			return playId ;
		}


		/// <summary>
		/// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		/// </summary>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="position">音源のワールド空間座標</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離の係数</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static int Play3D( AudioStream audioClip, Vector3 position, Transform3D? listenerTransform = null, float scale = 1, bool loop = false, float volume = 1.0f, float pitch = 0.0f, float offset = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.Play3D_Private( audioClip, position, listenerTransform, scale, loop, volume, pitch, offset, tag ) ;
		}

		// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		private int Play3D_Private( AudioStream audioClip, Vector3 audioSourcePosition, Transform3D? listenerTransform, float distanceScale, bool loop, float volume, float pitch, float offset, string tag )
		{
			string audioClipName = string.Empty ;
			if( audioClip != null )
			{
				audioClipName = audioClip.ResourceName ;
			}

			AudioChannel audioChannel = GetChannel_Private( tag, audioClipName ) ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}

			//----------------------------------------------------------

			int playId = GetPlayId() ;

			//----------------------------------------------------------
			// 音源位置の設定

			AudioListener3D trueListener ;

			if( m_Listener != null )
			{
				trueListener = m_Listener ;
			}
			else
			{
				trueListener = FindObjectOfType<AudioListener3D>() ;
			}

			if( trueListener == null )
			{
				GD.Print( "[AudioManager] Not found listener." ) ;
				return -1 ;
			}

			//----------------------------------

			// その他のものを使用する
			if( listenerTransform != null )
			{
				// 音源の座標を指定されたリスナーからのローカル座標系に変換する
//				audioSourcePosition = listenerTransform.Value.InverseTransformPoint( audioSourcePosition ) ;
				audioSourcePosition = listenerTransform.Value.Inverse() * audioSourcePosition ;

				// スケール調整を行う
				audioSourcePosition *= distanceScale ;

				// 音源の座標を真のリスナーのワールド座標系に変換する
//				audioSourcePosition = trueListener.Transform.TransformPoint( audioSourcePosition ) ;
				audioSourcePosition = trueListener.Transform * audioSourcePosition ;
			}
			else
			{
				// 音源の座標を真のリスナーのローカル座標系に変換する
//				audioSourcePosition = trueListener.Transform.InverseTransformPoint( audioSourcePosition ) ;
				audioSourcePosition = trueListener.GetListenerTransform().Inverse() * audioSourcePosition ;

				// スケール調整を行う
				audioSourcePosition *= distanceScale ;

				// 音源の座標を真のリスナーのワールド座標系に変換する
//				audioSourcePosition = trueListener.Transform.TransformPoint( audioSourcePosition ) ;
				audioSourcePosition = trueListener.GetListenerTransform() * audioSourcePosition ;
			}

			//------------------------------------------------------------------------------------------

			// 再生する
			audioChannel.Play( playId, audioClip, loop, GetBaseVolume( tag ), volume, 0, audioSourcePosition, true, pitch, offset, tag ) ;

			if( string.IsNullOrEmpty( tag ) == false && m_MuteList.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}

			// 成功したらソースのインスタンスを返す
			return playId ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音のオーディオクリップ名を取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static string GetName( int playId )
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetName_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private string GetName_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//----------------------------------

			return audioChannel.Name ;
		}

		//-----------------------------------------------------------
		// AudioManager 固有

		/// <summary>
		/// 再生中のオーディオクリップを取得する
		/// </summary>
		/// <param name="playId">発音識別子</param>
		/// <returns></returns>
		public static AudioStream GetClip( int playId )
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetClip_Private( playId ) ;
		}
	
		// 再生中のオーディオクリップを取得する
		private AudioStream GetClip_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//-----------------------------------------------------
			
			return audioChannel.Clip ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsPlaying_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPlaying_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.IsPlaying ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsPausing_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPausing_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == false )
			{
				return false ;
			}

			return audioChannel.IsPausing ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsUsing( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsUsing_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsUsing_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == true || audioChannel.IsPausing == true || audioChannel.IsLocking == true )
			{
				return true ;
			}

			return false ;
		}
	
		/// <summary>
		/// オーディオチャンネルごとのミュート状態を設定する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Mute( int playId, bool state )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Mute_Private( playId, state ) ;
		}
		
		// オーディオチャンネルごとのミュート状態を設定する
		private bool Mute_Private( int playId, bool state )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			audioChannel.Mute = state ;	// 完全停止
				
			return true ;	// 発見
		}
		
		/// <summary>
		/// オーディオチャンネルごとにサウンドを完全停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Stop_Private( playId ) ;
		}
		
		// オーディオチャンネルごとにサウンドを完全停止させる
		private bool Stop_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			audioChannel.Stop() ;	// 完全停止
				
			// フェードリストから除外する
			RemoveFadeList( audioChannel ) ;
					
			// 即時呼び出す
			if( audioChannel.IsUsing == true )
			{
				OnStoppedDelegate?.Invoke( playId ) ;

				// 以後は完全に発音が停止するまでチャンネルへの操作はできない
				audioChannel.Clear() ;

				audioChannel.IsUsing = false ;

//				audioChannel.callback = false ;
			}

			return true ;	// 成功
		}

		/// <summary>
		/// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="duration">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFade( int playId, float duration = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return  m_Instance.StopFade_Private( playId, duration ) ;
		}
	
		// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		private bool StopFade_Private( int playId, float duration )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のチャンネルを発見した（ただし既にフェード対象に対する多重の効果はかけられない）
				
			if( audioChannel.IsPlaying == true && m_FadeStopList.ContainsKey( audioChannel ) == false )
			{
				// 一旦破棄
				RemoveFadeList( audioChannel ) ;	// 多重実行は禁止したが保険
					
				var effect = new FadeEffect()
				{
					Duration	= duration,
					StartTime	= GetTime(),
					StartVolume	= 1,
					EndVolume   = 0
				} ;
					
				m_FadeStopList.Add( audioChannel, effect ) ;
					
				// フェード処理中なのでロックする
				audioChannel.Lock() ;
					
				return true ;   // 発見
			}
			else
			{
				// ポーズ中であれば即時停止させる
				audioChannel.Stop() ;
					
				// リストから除外する
				RemoveFadeList( audioChannel ) ;

				// 即時呼び出す
				if( audioChannel.IsUsing == true )
				{
					OnStoppedDelegate?.Invoke( playId ) ;

					// 以後は完全に発音が停止するまでチャンネルへの操作はできない
					audioChannel.Clear() ;

					audioChannel.IsUsing = false ;

//					audioChannel.callback = false ;
				}
			}
		
			return false ;
		}
		
		/// <summary>
		/// オーディオチャンネルごとにサウンドを一時停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Pause_Private( playId ) ;
		}
		
		// オーディオチャンネルごとにでサウンドを一時停止させる
		private bool Pause_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した
				
			if( audioChannel.IsPlaying == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				PauseFadeList( audioChannel ) ;
					
				audioChannel.Pause() ;	// 一時停止
			}
			
			return true ;
		}
	
		/// <summary>
		/// オーディオチャンネルごとに一時停止を解除させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Unpause_Private( playId ) ;
		}
		
		// オーディオチャンネルごとに一時停止を解除させる
		private bool Unpause_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == false )
			{
				// 再生中ではない
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した
				
			// 注意：AudioChannel では、終了中なのか中断中なのか区別がつかない
			if( audioChannel.IsPausing == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				UnpauseFadeList( audioChannel ) ;
					
				audioChannel.Unpause() ;	// 再開
			}
			else
			{
				// ポーズ中でなく普通に終わっているだけなら再生する
				audioChannel.Unpause() ;
			}
				
			return true ;	// 発見
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 何らかのチャンネルが鳴っているか確認する
		/// </summary>
		/// <returns>鳴っているチャンネル数</returns>
		public static int IsPlaying()
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.IsPlaying_Private() ;

		}

		// 何らかのチャンネルが鳴っているか確認する
		private int IsPlaying_Private()
		{
			int count = 0 ;

			AudioChannel audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true )
					{
						// 該当するチャンネルを発見した
						count ++ ;
					}
				}
			}

			return count ;
		}

		/// <summary>
		/// 指定のタグで何らかのチャンネルが鳴っているか確認する
		/// </summary>
		/// <returns>鳴っているチャンネル数</returns>
		public static int IsPlaying( string tag )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.IsPlaying_Private( tag ) ;

		}

		// 何らかのチャンネルが鳴っているか確認する
		private int IsPlaying_Private( string tag )
		{
			int count = 0 ;

			AudioChannel audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true && ( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) ) )
					{
						// 該当するチャンネルを発見した
//						Debug.Log( "<color=#FFFF00>----->鳴っている環境音:" + audioChannel.CueName + " Play = " + audioChannel.IsPlaying + "</color>" ) ;
						count ++ ;
					}
				}
			}

			return count ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 再生中の位置(秒)を取得する
		/// </summary>
		/// <param name="playId">発音識別子</param>
		/// <returns></returns>
		public static float GetTime( int playId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}
		
			return m_Instance.GetTime_Private( playId ) ;
		}
	
		// 再生中の位置(秒)を取得する
		private float GetTime_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------
			
			return audioChannel.Time ;
		}

		/// <summary>
		/// 再生中の位置(サンプル数)を取得する
		/// </summary>
		/// <param name="playId">発音識別子</param>
		/// <returns></returns>
		public static int GetTimeSamples( int playId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}
		
			return m_Instance.GetTimeSamples_Private( playId ) ;
		}
	
		// 再生中の位置(秒)を取得する
		private int GetTimeSamples_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------
			
			return audioChannel.TimeSamples ;
		}

		//-----------------------------------------------------------
#if false
		/// <summary>
		/// オーディオチャンネルごとの現在の状態を取得する
		/// </summary>
		/// <param name="audioChannel">オーディオチャンネルのインスタンス</param>
		/// <returns>現在の状態(-1=不正な引数・0=一時停止か完全停止・1=再生</returns>
		public static int GetStatus( AudioChannel audioChannel )
		{
			if( audioChannel == null )
			{
				return -1 ;
			}
		
			if( audioChannel.IsPlaying == true )
			{
				return 1 ;	// 再生中
			}
		
			return 0 ;	// 停止中か一時停止中
		}
#endif
		//---------------------------------------------------------
	
		// 個別に停止したい場合は再生時に取得したオーディオソースに対して Stop() を実行する
	
		// オーディオに関しては実装が不完全と言える
		// 音自体の再生停止は runInBackground とは別に管理しなくてはならない（別に管理出来るようにする）
		// （あまり使い道は無いかもしれないがゲームが止まっても音は流し続ける事も可能）
		// しかしフェード効果は Update で処理しているためバックグウンド時はフェード効果が正しく処理されなくなってしまう
	
		// サスペンド・レジューム
		internal void OnApplicationPause( bool state )
		{
			if( state == true )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}
	
		// フォーカス
		internal void OnApplicationFocus( bool state )
		{
			if( state == false )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}
	
		// 全サスペンド
		private bool SuspendAll_Private()
		{
			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( m_RunInBackground == true )
			{
				return true ;
			}

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						PauseFadeList( Channels[ i ] ) ;
					
						Channels[ i ].Pause() ;	// 一時停止				
						Channels[ i ].Suspend() ;
					}
				}
			}
		
			return true ;
		}
	
		// 全レジューム
		private bool ResumeAll_Private()
		{
			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( m_RunInBackground == true )
			{
				return true ;
			}

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPausing == true && Channels[ i ].IsSuspending == true )
					{
						// リプレイ状態になるのでベースタイムとベースボリュームを更新する
						UnpauseFadeList( Channels[ i ] ) ;
					
						Channels[ i ].Unpause() ;	// 一時停止解除					
						Channels[ i ].Resume() ;
					}
				}
			}
		
			return true ;
		}
		
		//---------------------------------------------------------------------------
	
		/// <summary>
		/// 全オーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteAll( bool state )
		{
			return MuteWithTag( state, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteWithTag( bool state, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.MuteWithTag_Private( state, tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルのミュート状態を設定する
		private bool MuteWithTag_Private( bool state, string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
						{
							Channels[ i ].Mute = state ;	// ミュート設定
						}
					}
				}
			}
		
			// タグ指定でミュートリストに保存する
			if( string.IsNullOrEmpty( tag ) == false )
			{
				if( state == true )
				{
					// オン
					if( m_MuteList.Contains( tag ) == false )
					{
						m_MuteList.Add( tag ) ;
					}
				}
				else
				{
					// オフ
					if( m_MuteList.Contains( tag ) == true )
					{
						m_MuteList.Remove( tag ) ;
					}
				}
			}
		
			return true ;
		}
		
		/// <summary>
		/// 全オーディオチャンネルを完全停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return StopAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを完全停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.StopAll_Private( tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルを完全停止させる
		private bool StopAll_Private( string tag )
		{
			AudioChannel audioChannel ;
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
					{
						// 再生中のチャンネルを発見した
						audioChannel.Stop() ;
					
						//-----------

						// フェードリストから除外する
						RemoveFadeList( audioChannel ) ;

						// 即時呼び出す
						if( audioChannel.IsUsing == true )
						{
							// 以後、のチャンネルに対する操作はできない
							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;

							// 以後は完全に発音が停止するまでチャンネルへの操作はできない
							audioChannel.Clear() ;

							audioChannel.IsUsing = false ;
						}

//						if( audioChannel.callback == true )
//						{
//							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;
//							audioChannel.callback = false ;
//						}
					}
				}
			}
		
			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFadeAll( float fadeDuration )
		{
			return StopFadeAll( fadeDuration, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを完全停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFadeAll( float fadeDuration, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StopFadeAll_Private( fadeDuration, tag ) ;
		}

		// タグで指定されたオーディオチャンネルを完全停止させる
		private bool StopFadeAll_Private( float fadeDuration, string tag )
		{
			if( fadeDuration <= 0 )
			{
				return StopAll_Private( tag ) ;
			}

			//----------------------------

			AudioChannel audioChannel ;
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
					{
//						Debug.Log( "<color=#FFFF00>------>フェードアウト対象1:" + audioChannel.CueName + "</color>" ) ;

						// 再生中のチャンネルを発見した
						StopFade( audioChannel.PlayId, fadeDuration ) ;
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを一時停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return PauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを一時停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.PauseAll_Private( tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルを一時停止させる
		private bool PauseAll_Private( string tag )
		{
			AudioChannel audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true && audioChannel.IsPausing == false )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
						{
							// ポーズ状態になるのでベースタイムとベースボリュームを更新する
							PauseFadeList( audioChannel ) ;

							audioChannel.Pause() ;	// 一時停止
						}
					}
				}
			}
		
			return true ;
		}
		
		/// <summary>
		/// 全オーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return UnpauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.UnpauseAll_Private( tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルの一時停止を解除させる
		private bool UnpauseAll_Private( string tag )
		{
			AudioChannel audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true && audioChannel.IsPausing == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
						{
							// ポーズ解除状態になるのでベースタイムとベースボリュームを更新する
							UnpauseFadeList( audioChannel ) ;

							audioChannel.Unpause() ;	// 再開
						}
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全にクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearAll()
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.ClearAll_Private() ;
		}
		
		// 全オーディオチャンネルを完全にクリアする
		private bool ClearAll_Private()
		{
			StopAll_Private( null ) ;
		
			int i, l = Channels.Count ;
			for( i  = l - 1 ; i >= 0 ; i -- )
			{
				Channels[ i ]?.Destroy() ;
			}
		
			m_FadeStopList.Clear() ;
		
			m_FadePlayList.Clear() ;
		
			m_MuteList.Clear() ;
		
			Channels.Clear() ;
		
			return true ;
		}
	
		// --------------------------------------------------------
	
		/// <summary>
		/// 全体共通のミュート状態を設定する
		/// </summary>
		/// <param name="state">全体共通のミュート状態(true=オン・false=オフ)</param>
		public static void Mute( bool state )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			m_Instance.m_Mute = state ;
#if false
			if( m_Instance.m_Mute == false )
			{
				AudioListener.volume = m_Instance.MasterVolume ;
			}
			else
			{
				AudioListener.volume = 0 ;
			}
#endif
		}
	
		/// <summary>
		/// マスターボリュームを設定する
		/// </summary>
		/// <param name="volume">マスターボリューム(0～1)</param>
		public static void SetMasterVolume( float volume )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			if( volume >  1 )
			{
				volume  = 1 ;
			}
			else
			if( volume <  0 )
			{
				volume  = 0 ;
			}
		
			m_Instance.MasterVolume = volume ;
#if false
			if( m_Instance.m_Mute == false )
			{
				AudioListener.volume = m_Instance.MasterVolume ;
			}
#endif
		}
	
		/// <summary>
		/// 特定チャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">発音識別子</param>
		/// <param name="volume">マスターボリューム(0～1)</param>
		public static void SetVolume( int playId, float volume )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			if( volume >  1 )
			{
				volume  = 1 ;
			}
			else
			if( volume <  0 )
			{
				volume  = 0 ;
			}
			
			AudioChannel audioChannel = m_Instance.GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return ;
			}

			audioChannel.Volume = volume ;
		}

		/// <summary>
		/// 全オーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float volume )
		{
			return SetAllVolumes( volume, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float volume, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.SetAllVolumes_Private( volume, tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルのボリュームを設定する
		private bool SetAllVolumes_Private( float volume, string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						Channels[ i ].Volume = volume ;	// 一時停止
					}
				}
			}
		
			return true ;
		}
	
		//-----------------------------------------------------------

		/// <summary>
		/// ダグで指定されたベースボリュームを設定する
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="baseVolume"></param>
		/// <returns></returns>
		public static bool SetBaseVolume( string tag, float baseVolume )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetBaseVolume_Private( tag, baseVolume ) ;
		}

		// タグで指定されたベースボリュームを設定する
		private bool SetBaseVolume_Private( string tag, float baseVolume )
		{
			AddBaseVolume_Private( tag, baseVolume ) ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
					{
						Channels[ i ].BaseVolume = baseVolume ;
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// ダグで指定されたベースボリュームを取得する
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static float GetBaseVolume( string tag )
		{
			if( m_Instance == null )
			{
				return 1 ;
			}

			return m_Instance.GetBaseVolume_Private( tag ) ;
		}

		//-----

		/// <summary>
		/// タグごとの最大チャンネル数を設定する
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="maxChannels"></param>
		/// <returns></returns>
		public static bool SetMaxChannels( string tag, int maxChannels )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetMaxChannels_Private( tag, maxChannels ) ;
		}

		// タグごとの最大チャンネル数を設定する
		private bool SetMaxChannels_Private( string tag, int maxChannels )
		{
			AddMaxChannels_Private( tag, maxChannels ) ;

			return true ;
		}

		/// <summary>
		/// タグごとの最大チャンネル数を取得する
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static int GetMaxChannels( string tag )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.GetMaxChannels_Private( tag ) ;
		}

		//---------------------------------------------------------
		
		// フェードを処理する
		private void Process()
		{
			// 現在の時間を取得する
			float currentTime = GetTime() ;
			
			int i, l ;
			AudioChannel audioChannel ;

			//-----------------------------------------------------
		
			// プレイフェードを処理する
		
			if( m_FadePlayList.Count >  0 )
			{
				l = m_FadePlayList.Count ;
				AudioChannel[] audioChannelList = new AudioChannel[ l ] ;
			
				m_FadePlayList.Keys.CopyTo( audioChannelList, 0 ) ;
			
				float deltaTime ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = audioChannelList[ i ] ;
				
					if( audioChannel.IsPlaying == true )
					{
						// 再生中のみ処理する
					
						deltaTime = currentTime - m_FadePlayList[ audioChannel ].StartTime ;
						if( deltaTime <  m_FadePlayList[ audioChannel ].Duration )
						{
							// まだ続く
							audioChannel.FadeVolume = m_FadePlayList[ audioChannel ].StartVolume + ( ( m_FadePlayList[ audioChannel ].EndVolume - m_FadePlayList[ audioChannel ].StartVolume ) * deltaTime / m_FadePlayList[ audioChannel ].Duration ) ;
						}
						else
						{
							// 終了
							audioChannel.FadeVolume = m_FadePlayList[ audioChannel ].EndVolume ;
						
							// リストから除外する
							m_FadePlayList.Remove( audioChannel ) ;
						}
					}
				}
			}
		
			//-----------------------------
		
			// ストップフェードを処理する
		
			if( m_FadeStopList.Count >  0 )
			{
				l = m_FadeStopList.Count ;
				AudioChannel[] audioChannelList = new AudioChannel[ l ] ;
			
				m_FadeStopList.Keys.CopyTo( audioChannelList, 0 ) ;
			
				float deltaTime ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = audioChannelList[ i ] ;
				
					if( audioChannel.IsPlaying == true )
					{
						// 再生中のみ処理する
					
						deltaTime = currentTime - m_FadeStopList[ audioChannel ].StartTime ;
						if( deltaTime <  m_FadeStopList[ audioChannel ].Duration )
						{
							// まだ続く
							audioChannel.FadeVolume = m_FadeStopList[ audioChannel ].StartVolume + ( ( m_FadeStopList[ audioChannel ].EndVolume - m_FadeStopList[ audioChannel ].StartVolume ) * deltaTime / m_FadeStopList[ audioChannel ].Duration ) ;
						}
						else
						{
							// 終了
						
							// 再生も停止する
							audioChannel.Stop() ;
						
							audioChannel.FadeVolume = m_FadeStopList[ audioChannel ].EndVolume ;
						
							// リストから除外する
							m_FadeStopList.Remove( audioChannel ) ;
						
				//			Debug.Log( "フェードストップが終わったので破棄:" + mFadeStopList.Count ) ;

							//------------------------------

							// 即時呼び出す(Stop()実行後に即座にStatus==Stopとはならないため、このタイミングでそのチャンネルへの操作が一切できないようにする)
							if( audioChannel.IsUsing == true )
							{
//								Debug.Log( "<color=#FFFF00>--------->チャンネルから完全に破棄:" + audioChannel.CueName + "</color>" ) ;

								// 以後、のチャンネルに対する操作はできない
								OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;

								// 以後は完全に発音が停止するまでチャンネルへの操作はできない
								audioChannel.Clear() ;

								audioChannel.IsUsing = false ;
							}
						}
					}
				}
			}

			//----------------------------------------------------------

			// 終了したチャンネルを監視してコールバックを発生させる
			l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == false )
					{
						if( audioChannel.IsUsing == true )
						{
							// ワンショット再生の自然停止待ちだとここで再生終了をチェックするしかない
//							Debug.Log( "<color=#FFFF00>--------->チャンネルから完全に破棄:" + audioChannel.CueName + "</color>" ) ;

							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;

							// 以後は完全に発音が停止するまでチャンネルへの操作はできない
							audioChannel.Clear() ;

							audioChannel.IsUsing = false ;	// 多重にコールバックが呼ばれてしまうと問題なので１回呼んだらフラグを落として２回以上呼ばれないようにする
						}

						//-------------------------------

						// 真の意味でチャンネルが解放された
						audioChannel.Busy = false ;
					}
				}
			}
		}
	
		// ポーズ状態になるのでベースタイムとベースボリュームを更新する
		private void PauseFadeList( AudioChannel audioChannel )
		{
			float currentTime = GetTime() ;
			float deltaTime ;
		
			// プレイリスト
			if( m_FadePlayList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				deltaTime = currentTime - m_FadePlayList[ audioChannel ].StartTime ;
			
				if( deltaTime <  m_FadePlayList[ audioChannel ].Duration )
				{
					// まだ続く
					m_FadePlayList[ audioChannel ].Duration   = m_FadePlayList[ audioChannel ].Duration - deltaTime ;
				
					m_FadePlayList[ audioChannel ].StartTime   = currentTime ;
					m_FadePlayList[ audioChannel ].StartVolume = audioChannel.Volume ;
				}
				else
				{
					// 終了
					audioChannel.FadeVolume = m_FadePlayList[ audioChannel ].EndVolume ;	// 最終的なボリューム
					m_FadePlayList.Remove( audioChannel ) ;	// リストから破棄
				}
			}
		
			// ストップリスト
			if( m_FadeStopList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				deltaTime = currentTime - m_FadeStopList[ audioChannel ].StartTime ;
			
				if( deltaTime <  m_FadeStopList[ audioChannel ].Duration )
				{
					// まだ続く
					m_FadeStopList[ audioChannel ].Duration   = m_FadeStopList[ audioChannel ].Duration - deltaTime ;
				
					m_FadeStopList[ audioChannel ].StartTime   = currentTime ;
					m_FadeStopList[ audioChannel ].StartVolume = audioChannel.Volume ;
				}
				else
				{
					// 終了
					audioChannel.FadeVolume = m_FadeStopList[ audioChannel ].EndVolume ;	// 最終的なボリューム（ストップの場合は０）
				
					m_FadeStopList.Remove( audioChannel ) ;
				
					// ここに来る事はあまり考えられないが念のため
					audioChannel.Stop() ;
				}
			}
		}
	
		// リプレイ状態になるのでベースタイムとベースボリュームを更新する
		private void UnpauseFadeList( AudioChannel audioChannel )
		{
			float currentTime = GetTime() ;
		
			// プレイリスト
			if( m_FadePlayList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				m_FadePlayList[ audioChannel ].StartTime   = currentTime ;
			}
		
			// ストップリスト
			if( m_FadeStopList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				m_FadeStopList[ audioChannel ].StartTime   = currentTime ;
			}
		}
		
		// フェードリストから除外する
		private void RemoveFadeList( AudioChannel audioChannel )
		{
			if( m_FadePlayList.ContainsKey( audioChannel ) == true )
			{
				m_FadePlayList.Remove( audioChannel ) ;
			}
						
			if( m_FadeStopList.ContainsKey( audioChannel ) == true )
			{
				m_FadeStopList.Remove( audioChannel ) ;
			}
		}
	}

	//-------------------------------------------------------------------------------------------

	/// <summary>
	/// オーディオチャンネルクラス
	/// </summary>
	public class AudioChannel
	{
		/// <summary>
		/// オーディオクリップの名称
		/// </summary>
		public string			Name
		{
			get
			{
				if( m_Clip == null )
				{
					return string.Empty ;
				}
				return m_Clip._GetStreamName() ;
			}
		}
		
		/// <summary>
		/// 発音中の音のユニークな識別子
		/// </summary>
		public int				PlayId ;

		/// <summary>
		/// タグ名
		/// </summary>
		public string			Tag ;	// タグ
	

		/// <summary>
		/// チャンネルを使用したかどうか
		/// </summary>
		public bool				IsUsing = false ;

		/// <summary>
		/// 内部的に使用しているか
		/// </summary>
		public bool				Busy	= false ;

		//---------------------------------
		
		// オーディオソースのインスタンス
		private AudioStreamPlayer3D		m_AudioSource ;
//		private AudioStreamPlayer		m_AudioSource ;
		
		/// <summary>
		/// ロック状態
		/// </summary>
		private bool			m_IsLocking ;

		/// <summary>
		/// サスペンド状態
		/// </summary>
		private bool			m_IsSuspending ;

		/// <summary>
		/// 再生状態
		/// </summary>
		private bool			m_IsPlaying ;

		/// <summary>
		/// ポーズ状態
		/// </summary>
		private bool			m_IsPausing ;

		/// <summary>
		/// ミュート
		/// </summary>
		private bool			m_Mute = false ;

		//-----------------------------------

		/// <summary>
		/// ベースボリーム
		/// </summary>
		private float			m_BaseVolume = 1.0f ;

		/// <summary>
		/// ボリーム
		/// </summary>
		private float			m_Volume = 1.0f ;

		/// <summary>
		/// フェードボリューム
		/// </summary>
		private float			m_FadeVolume = 1.0f ;

		//---------------------------------------------------------
	
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="audioSource">オーディオソースのインスタンス</param>
		internal protected AudioChannel( AudioStreamPlayer3D audioSource )
		{
			m_AudioSource = audioSource ;
		}
		
		/// <summary>
		/// オーディオチャンネルの破棄を行う
		/// </summary>
		internal protected void Destroy()
		{
			if( m_AudioSource != null )
			{
				m_AudioSource.Free() ;
				m_AudioSource = null ;
			}
		}

		//-----------------------------------
		// 固有

		// オーディオクリップ
		private AudioStream		m_Clip ;

		/// <summary>
		/// オーディオクリップ
		/// </summary>
		public AudioStream		Clip	=> m_Clip ;


		//-----------------------------------

		/// <summary>
		/// ミュート状態
		/// </summary>
		internal protected bool Mute
		{
			get
			{
				return m_Mute ;
			}
			set
			{
				if( m_Mute != value )
				{
					m_Mute = value ;
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// 総合的なループ状態
		/// </summary>
		internal protected bool Loop
		{
			get
			{
				return SourceLoop ;
			}
		}

		/// <summary>
		/// ソースのループ状態
		/// </summary>
		internal protected bool SourceLoop
		{
			get
			{
				if( m_Clip == null )
				{
					return false ;
				}

				if( m_Clip is AudioStreamWav wav )
				{
					return ( wav.LoopMode != AudioStreamWav.LoopModeEnum.Disabled ) ;
				}
				else
				if( m_Clip is AudioStreamOggVorbis ogg )
				{
					return ogg.Loop ;
				}
				else
				if( m_Clip is AudioStreamMP3 mp3 )
				{
					return mp3.Loop ;
				}

				return false ;
			}
		}

		/// <summary>
		/// ベースボリューム(0～1)
		/// </summary>
		internal protected float BaseVolume
		{
			get
			{
				return m_BaseVolume ;
			}
			set
			{
				if( m_BaseVolume != value )
				{
					m_BaseVolume = value ;
					m_FadeVolume = 1 ;
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float Volume
		{
			get
			{
				return m_Volume ;
			}
			set
			{
				if( m_Volume != value )
				{
					m_Volume = value ;
					m_FadeVolume = 1 ;
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float FadeVolume
		{
			get
			{
				return m_FadeVolume ;
			}
			set
			{
				if( m_FadeVolume != value )
				{
					m_FadeVolume  = value ;
					UpdateVolume() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// Volume( 0.0 ～ 1.0 ) と dB( -80 ～ 0 ) の相互変換式
		//
		// Volume → dB
//		private static float ConvertVolumeToDB( float volume ) => Mathf.Clamp( 20f * MathF.Log10( Mathf.Clamp( volume, 0f, 1f ) ), -80f, 0f ) ; 
		//
		// dB → Volume
//		private static float ConvertDBToVolume( float db ) => Mathf.Clamp( Mathf.Pow( 10, Mathf.Clamp( db, -80, 0 ) / 20f ), 0, 1 ) ;
		//
		//-------------------------------------------------------------------------------------------

		// ボリュームを更新する
		private void UpdateVolume()
		{
			if( m_AudioSource == null )
			{
				return ;
			}

			if( m_Mute == false )
			{
				// 単純なリニアな計算式の方が良さげ
				float factor = m_BaseVolume * m_Volume * m_FadeVolume ;
				float volumeDb = -80.0f * ( 1.0f - factor ) ; 

//				m_AudioSource.VolumeDb = ConvertVolumeToDB( m_BaseVolume * m_Volume * m_FadeVolume ) ;
				m_AudioSource.VolumeDb = volumeDb ;
			}
			else
			{
//				m_AudioSource.VolumeDb = ConvertVolumeToDB( 0 ) ;
				m_AudioSource.VolumeDb = -80.0f ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// 再生位置(秒)
		/// </summary>
		internal protected float Time
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return m_AudioSource.GetPlaybackPosition() ;
			}
		}

		/// <summary>
		/// 再生位置(サンプル)
		/// </summary>
		internal protected int TimeSamples
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return 0 ;
			}
		}

		//-----------------------------------------------------------
	
		/// <summary>
		/// サウンドを再生する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="loop">ループの有無(true=する・false=しない)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=1オクターブ上)</param>
		/// <param name="tag">タグ名</param>
		internal protected void Play( int playId, AudioStream audioClip, bool loop, float baseVolume, float volume, float pan, Vector3 audioSourcePosition, bool isSpatial, float pitch, float offset, string tag )
		{
			if( m_AudioSource == null || audioClip == null )
			{
				return ;
			}

			// ＳＥなどの音は消えてもチャンネルとしては動き続けている状態で別の音を鳴らすとループ設定が効かないので一度明示的に停止させる必要がある
			m_AudioSource.Stop() ;

			//----------------------------------

			m_Clip	= audioClip ;

			PlayId	= playId ;

			Tag		= tag ;

			//----------------------------------

			if( isSpatial == false )
			{
				// 2D : Stereo

				m_AudioSource.MaxDistance = 2000 ;
				m_AudioSource.Position = new Vector3( ( pan * 2.0f ) + audioSourcePosition.X, audioSourcePosition.Y, 1.0f + audioSourcePosition.Z ) ;

				// 注意：３Ｄ再生の場合に座標をリスナーと同じにしてしまうとボリュームの減衰が効かなくなる
				// 　　　Ｚを＋１しているのはボリュームの減衰を効かせるため
			}
			else
			{
				// 3D : Surround

				m_AudioSource.MaxDistance = 2000 ;
				m_AudioSource.Position = new Vector3( audioSourcePosition.X, audioSourcePosition.Y, audioSourcePosition.Z ) ;
			}

			//----------------------------------

			m_AudioSource.PitchScale	= Mathf.Pow( 2.0f, pitch ) ;
		
//			m_AudioSource.loop			= loop ;	// メソッドからのコントロール不可(非ループクリップはプログラムでループ管理する)

			m_AudioSource.Stream		= null ;
			m_AudioSource.Stream		= m_Clip ;

			UpdateVolume() ;

			//--------------

			// ベースボリューム
			m_BaseVolume			= baseVolume ;

			// ボリューム
			m_Volume				= volume ;

			// フェードボリューム
			m_FadeVolume			= 1 ;

			//-----------------------------------------------------

			// 再生
			m_AudioSource.Play( offset ) ;

			// 念のため再生後にも再度設定しておく
			UpdateVolume() ;

			m_IsPlaying = true ;
			m_IsPausing = false ;

			//-----------------------------------------------------

			IsUsing = true ;	// 最後にクリーンアップされたかの判定にも使用する

			Busy	= true ;	// 内部的に使用している状態とする
		}
	
		/// <summary>
		/// サウンドを完全停止する
		/// </summary>
		internal protected void Stop()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
			
			m_AudioSource.Stop() ;
			
			if( m_AudioSource.Stream != null )
			{
				// 他のチャンネルからの参照があるかもしれないので破棄はしない
//				Resources.UnloadAsset( m_AudioSource.clip ) ;	// アセットバンドルから展開したリソースの場合は再度同じリソースを異なるインスタンスで展開しようとすると以前のインスタンスが誤動作を引き起こす事があるので以前のインスタンスは完全にシステムリソースキャッシュから破棄する必要がある。
				m_AudioSource.Stream = null ; // 参照を完全になくすため(これをしないと AudioClip がアセットバンドルから生成されたものの際にバグるので超重要)
			}

			// ストップでボーズもビジィも解除される
			m_IsPausing	= false ;
			m_IsPlaying = false ;

			m_IsLocking	= false ;
		}
	
		/// <summary>
		/// サウンドを一時停止する
		/// </summary>
		internal protected void Pause()
		{
			if( m_AudioSource == null )
			{
				return ;
			}

			if( m_AudioSource.Playing == true && m_IsPausing == false )
			{
				// ボーズ位置を記録する
//				var pausePosition = m_AudioSource.GetPlaybackPosition() ;

				m_AudioSource.StreamPaused = true ;

				m_IsPausing	= true ;
			}
		}
	
		/// <summary>
		/// サウンドの一時停止を解除する
		/// </summary>
		internal protected void Unpause()
		{
			if( m_AudioSource == null )
			{
				return ;
			}

			if(  m_AudioSource.Playing == false && m_IsPausing == true )
			{
				m_AudioSource.StreamPaused = false ;

				m_IsPausing	= false ;
			}
		}
		
		//-----------------------------------------------------------

		internal protected void Lock()
		{
			m_IsLocking = true ;
		}

		internal protected void Unlock()
		{
			m_IsLocking = false ;
		}

		internal protected void Suspend()
		{
			m_IsSuspending = true ;
		}

		internal protected void Resume()
		{
			m_IsSuspending = false ;
		}

		//-----------------------------------------------------------
		/// <summary>
		/// 情報を消去する
		/// </summary>
		internal protected void Clear()
		{
			// 情報をクリアする

			if( m_AudioSource != null )
			{
				// チャンネルのキューシート名とキュー名をクリアするのが重要なのかもしれない
				m_AudioSource.Stream		= null ;

//					m_AudioSource.loop		= false ;
//				m_AudioSource.volume	= 1 ;		// 即座に停止しない場合があるためボリュームをリセットしてはならない
			}

			//--------------
			// 以後、このチャンネルに対する操作はできない

			Tag				= null ;

			PlayId			= -1 ;

			m_Clip			= null ;
		}

		//-------------------------------------------------------------------------------------------


		/// <summary>
		/// 実際に再生中かの判定を行う（ポーズ中は false とみなされる）
		/// </summary>
		/// <returns>結果(true=再生中・false=再生中ではない)</returns>
		internal protected bool IsPlaying
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}

				if( m_IsPausing == false && m_IsSuspending == false )
				{
					m_IsPlaying = m_AudioSource.Playing ;
				}

				return m_IsPlaying ;
			}
		}
		
		/// <summary>
		/// 一時停止中か判定を行う
		/// </summary>
		/// <returns>結果(true=一時停止中・false=一時停止中ではない)</returns>
		internal protected bool IsPausing
		{
			get
			{
				return m_IsPausing ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ロック中かどうか
		/// </summary>
		internal protected bool IsLocking
		{
			get
			{
				return m_IsLocking ;
			}
		}

		/// <summary>
		/// 中断中かどうか
		/// </summary>
		internal protected bool IsSuspending
		{
			get
			{
				return m_IsSuspending ;
			}
		}
	}
}
