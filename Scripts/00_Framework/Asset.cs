using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using AudioHelper ;

namespace Sample_001
{
	/// <summary>
	/// ＡｓｓｅｔＢｕｎｄｌｅファサードクラス(シングルトン)
	/// </summary>
	public partial class Asset : ExNode
	{
		// インスタンス(シングルトン)
		private static Asset m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static Asset Instance	=> m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static Asset Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var node = new Asset()
			{
				Name = "Asset"
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
			if( m_Instance == null )
			{
				m_Instance = this ;

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
				m_Instance  = null ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ){}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// キャッシュ方法の種別
		/// </summary>
		public enum CachingTypes
		{
			None,
			ResourceOnly,
			AssetBundleOnly,
			Same,
		}


		private Dictionary<( string, Type ),GodotObject>	m_Cache ;


		// 初期化する
		private void Initialize()
		{
			m_Cache = new () ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アセットバンドルがストレージに格納されているかどうか
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static bool Exists( string path )
		{
			if( m_Instance == null )
			{
				throw new Exception( "Asset is not initialied." ) ;
			}

			return m_Instance.Exists_Private( path ) ;
		}

		// アセットバンドルがストレージに格納されているかどうか
		private bool Exists_Private( string path )
		{
			return true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// // アセットをロードする(同期)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T Load<T>( string path, CachingTypes cachingType ) where T : GodotObject
		{
			if( m_Instance == null )
			{
				throw new Exception( "Asset is not initialied." ) ;
			}

			return m_Instance.Load_Private<T>( path, cachingType ) ;
		}

		// // アセットをロードする(同期)
		private T Load_Private<T>( string path, CachingTypes cachingType ) where T : GodotObject
		{
			// キャシュを確認する
			var key = ( path, typeof( T ) ) ;
			if( m_Cache.ContainsKey( key ) == true )
			{
				// 既にキャッシュに格納されている
				return m_Cache[ key ] as T ;
			}

			//----------------------------------
			
			// ロードする
			var asset = GD.Load<T>( path ) ;

			if( asset != null )
			{
				if( cachingType == CachingTypes.ResourceOnly || cachingType == CachingTypes.Same )
				{
					// キャッシュに格納する
					m_Cache.Add( key, asset ) ;
				}
			}

			return asset ;
		}

		/// <summary>
		/// // アセットをロードする(非同期)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static async Task<T> LoadAsync<T>( string path, CachingTypes cachingType ) where T : GodotObject
		{
			if( m_Instance == null )
			{
				throw new Exception( "Asset is not initialied." ) ;
			}

			return await m_Instance.LoadAsync_Private<T>( path, cachingType ) ;
		}

		// // アセットをロードする(非同期)
		private async Task<T> LoadAsync_Private<T>( string path, CachingTypes cachingType ) where T : GodotObject
		{
			// キャシュを確認する
			var key = ( path, typeof( T ) ) ;
			if( m_Cache.ContainsKey( key ) == true )
			{
				// 既にキャッシュに格納されている
				return m_Cache[ key ] as T ;
			}

			//----------------------------------
			// アセットバンドルがストレージに格納済みであれば同期メソッドを使用する

			//----------------------------------

			// ロードする
			var asset = GD.Load<T>( path ) ;

			if( asset != null )
			{
				if( cachingType == CachingTypes.ResourceOnly || cachingType == CachingTypes.Same )
				{
					// キャッシュに格納する
					m_Cache.Add( key, asset ) ;
				}
			}
			else
			{
				// ダイアログで警告を表示する
			}

			// ダミー
			await Yield() ;

			return asset ;
		}
	}

	public class Resources
	{
		/// <summary>
		/// リソースをロードする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static T Load<T>( string path ) where T : class
		{
			if( typeof( T ) == typeof( string ) )
			{
				var file = FileAccess.Open( $"res://Resources/{path}", FileAccess.ModeFlags.Read ) ;
				if( file == null )
				{
					return string.Empty as T ;
				}
				return file.GetAsText() as T ;
			}
			else
			if( typeof( T ) == typeof( GodotObject ) )
			{
				return GD.Load<T>( $"Resources/{path}" ) ;
			}
			else
			{
				return default ;
			}
		}
	}
}
