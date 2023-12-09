using Godot ;
using ExGodot ;
using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;


namespace StorageHelper
{
	/// <summary>
	/// ストレージ管理マネージャ Version 2023/11/08 0
	/// </summary>
	public partial class StorageManager : ExNode
	{
		// マネージャのインスタンス(シングルトン)
		private static StorageManager m_Instance = null ; 

		/// <summary>
		/// マネージャのインスタンス(シングルトン)
		/// </summary>
		public static StorageManager Instance => m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マネージャを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static StorageManager Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var storageManager = new StorageManager()
			{
				Name = "StorageManager"
			} ;

			if( parent == null )
			{
				storageManager.GetTree().Root.AddChild( storageManager ) ;
			}
			else
			{
				parent.AddChild( storageManager ) ;
			}

			return storageManager ;
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
			if( m_Instance == null )
			{
				m_Instance = this ;

				//----------------------------------

				Initialize() ;
			}
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ){}

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

		// ディレクティブ
		// https://docs.godotengine.org/ja/4.x/tutorials/scripting/c_sharp/c_sharp_features.html


		// #DEBUG がどういう動作をしているのか、汎用ダイアログにでも対応した際にアプリケーションビルドで確認する

		// エディターモードでの仮想ストレージのパス
		private readonly static string m_TemporaryDataFolderPath = "TemporaryDataFolder" ;

		// 初期化
		private void Initialize()
		{
#if TOOLS
			GD.Print( "CurrentDirectory : " + Directory.GetCurrentDirectory() ) ;

			if( Directory.Exists( m_TemporaryDataFolderPath ) == false )
			{
				// 仮想ストレージのパスを生成する
				Directory.CreateDirectory( m_TemporaryDataFolderPath ) ;
			}
#endif

			//----------------------------------------------------------

			m_CancellationTokenSourceOnDestroy = new CancellationTokenSource() ;
		}

		private void Terminate()
		{
			if( m_CancellationTokenSourceOnDestroy != null )
			{
				if( m_CancellationTokenSourceOnDestroy.IsCancellationRequested == false )
				{
					m_CancellationTokenSourceOnDestroy.Cancel() ;
				}
				m_CancellationTokenSourceOnDestroy.Dispose() ;
				m_CancellationTokenSourceOnDestroy = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// Node が破棄された際にタスクを強制停止させるトークンソース
		private CancellationTokenSource m_CancellationTokenSourceOnDestroy ;


		// キャンセルトークンを取得する
		private ( CancellationTokenSource, CancellationToken ) GetActiveCancellation( CancellationToken cancellationToken )
		{
			// MonoBehaviour が破棄されるタイミングでキャンセルが実行されるトークンを取得する。
			CancellationToken cancellationTokenOnDestroy	= m_CancellationTokenSourceOnDestroy.Token ;

			//----------------------------------------------------------

			CancellationTokenSource		resultCancellationTokenSource ;
			CancellationToken			resultCancellationToken ;

			if( cancellationToken == default || cancellationToken.CanBeCanceled == false )
			{
				// 基本のキャンセルトークン生成

				resultCancellationTokenSource	= CancellationTokenSource.CreateLinkedTokenSource( cancellationTokenOnDestroy ) ;
				resultCancellationToken			= resultCancellationTokenSource.Token ;
			}
			else
			{
				// 独自のキャンセルトークン生成

				resultCancellationTokenSource	= CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, cancellationTokenOnDestroy ) ;
				resultCancellationToken			= resultCancellationTokenSource.Token ;
			}

			return ( resultCancellationTokenSource, resultCancellationToken ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 存在の種別
		/// </summary>
		public enum TargetTypes
		{
			Unknown	= -1,

			None	=  0,
			File	=  1,
			Folder	=  2,
		}

		/// <summary>
		/// ファイル・フォルダが存在しているかどうか
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static TargetTypes Exists( string path )
		{
			path = $"{m_TemporaryDataFolderPath}/{path}" ; 

			if( File.Exists( path ) == true )
			{
				return TargetTypes.File ;
			}
			else
			if( Directory.Exists( path ) == true )
			{
				return TargetTypes.Folder ;
			}

			return TargetTypes.None ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ファイルにバイナリをセーブする(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public static void Save( string path, byte[] data, int offset = 0, int length = -1 )
		{
			path = $"{m_TemporaryDataFolderPath}/{path}" ; 

			if( length == 0 )
			{
				// ファイル生成のみ
				try
				{
					var fs = File.Create( path ) ;
					fs.Close() ;
				}
				catch{ throw ; }

				return ;
			}

			if( offset == 0 && length <  0 )
			{
				// 全体対象
				try
				{
					File.WriteAllBytes( path, data ) ;
				}
				catch{ throw ; }

				return ;
			}
			else
			{
				// 部分対象
				var span = data.AsSpan<byte>( offset, length ) ;

				try
				{
					File.WriteAllBytes( path, span.ToArray() ) ;
				}
				catch{ throw ; }
			}
		}

		/// <summary>
		/// ファイルにバイナリをセーブする(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public static async Task SaveAsync( string path, byte[] data, int offset = 0, int length = -1, CancellationToken cancellationToken = default )
		{
			if( m_Instance == null )
			{
				GD.PushWarning( "StorageManager is not created." ) ;
				throw new OperationCanceledException() ;
			}

			await m_Instance.SaveAsync_Private( path, data, offset, length, cancellationToken ) ;
		}

		private async Task SaveAsync_Private( string path, byte[] data, int offset = 0, int length = -1, CancellationToken cancellationToken = default )
		{
			path = $"{m_TemporaryDataFolderPath}/{path}" ; 

			if( length == 0 )
			{
				// ファイル生成のみ
				try
				{
					var fs = File.Create( path ) ;
					fs.Close() ;
				}
				catch{ throw ; }

				return ;
			}

			//----------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled	= false ;

			if( offset == 0 && length <  0 )
			{
				// 全体対象
				try
				{
					await File.WriteAllBytesAsync( path, data, token ) ;
				}
				catch( Exception e )
				{
					if( e is OperationCanceledException )
					{
						isCanceled = true ;
					}
					else
					{
						GD.PushWarning( e.Message ) ;
					}
				}
			}
			else
			{
				// 注意 : Span は構造体(スタックに積まれるもの)であるため、async Task 内では使用できない。

				// 部分対象
				byte[] span = new byte[ length ] ;
				Array.Copy( data, offset, span, 0, length )	;

				try
				{
					await File.WriteAllBytesAsync( path, span, token ) ;
				}
				catch( Exception e )
				{
					if( e is OperationCanceledException )
					{
						isCanceled = true ;
					}
					else
					{
						GD.PushWarning( e.Message ) ;
					}
				}
			}

			//----------------------------------------------------------

			// 使用し終わったトークンソースを破棄する
			tokenSource?.Dispose() ;

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// ファイルからバイナリをロードする(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public static byte[] Load( string path )
		{
			path = $"{m_TemporaryDataFolderPath}/{path}" ; 

			byte[] data ;

			try
			{
				data = File.ReadAllBytes( path ) ;
			}
			catch{ throw ; }

			return data ;
		}

		/// <summary>
		/// ファイルからバイナリをロードする(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		public static async Task<byte[]> LoadAsync( string path, CancellationToken cancellationToken = default )
		{
			if( m_Instance == null )
			{
				GD.PushWarning( "StorageManager is not created." ) ;
				throw new OperationCanceledException() ;
			}

			return await m_Instance.LoadAsync_Private( path, cancellationToken ) ;
		}

		private async Task<byte[]> LoadAsync_Private( string path, CancellationToken cancellationToken = default )
		{
			path = $"{m_TemporaryDataFolderPath}/{path}" ; 

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled	= false ;

			byte[] data = null ;

			try
			{
				data = await File.ReadAllBytesAsync( path, token ) ;
			}
			catch( Exception e )
			{
				if( e is OperationCanceledException )
				{
					isCanceled = true ;
				}
				else
				{
					GD.PushWarning( e.Message ) ;
				}
			}

			//----------------------------------------------------------

			// 使用し終わったトークンソースを破棄する
			tokenSource?.Dispose() ;

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return data ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ファイルにテキストをセーブする(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="text"></param>
		public static void SaveText( string path, string text )
		{
			var data = Encoding.UTF8.GetBytes( text ) ;

			Save( path, data ) ;
		}

		/// <summary>
		/// ファイルにテキストをセーブする(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static async Task SaveTextAsync( string path, string text, CancellationToken cancellationToken = default )
		{
			var data = Encoding.UTF8.GetBytes( text ) ;

			await SaveAsync( path, data, cancellationToken: cancellationToken ) ;
		}

		/// <summary>
		/// ファイルからテキストをロードする(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="text"></param>
		public static string LoadText( string path )
		{
			var data = Load( path ) ;

			return Encoding.UTF8.GetString( data ) ;
		}

		/// <summary>
		/// ファイルからテキストをロードする(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="text"></param>
		public static async Task<string> LoadTextAsync( string path, CancellationToken cancellationToken = default )
		{
			var data = await LoadAsync( path, cancellationToken: cancellationToken ) ;

			return Encoding.UTF8.GetString( data ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// フォルダを生成する
		/// </summary>
		/// <param name="path"></param>
		public static void CreateFolder( string path )
		{
			path = $"{m_TemporaryDataFolderPath}/{path}" ; 

			if( Directory.Exists( path ) == true )
			{
				return ;
			}

			Directory.CreateDirectory( path ) ;
		}
	}
}
