using Godot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;

namespace SceneHelper
{
	/// <summary>
	/// シーン管理マネージャ
	/// </summary>
	public partial class SceneManager : Node
	{
		// マネージャのインスタンス(シングルトン)
		private static SceneManager m_Instance = null ; 

		/// <summary>
		/// マネージャのインスタンス(シングルトン)
		/// </summary>
		public static SceneManager Instance => m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動的にマネージャを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static SceneManager Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var sceneManager = new SceneManager()
			{
				Name = "SceneManager"
			} ;

			if( parent == null )
			{
				sceneManager.GetTree().Root.AddChild( sceneManager ) ;
			}
			else
			{
				parent.AddChild( sceneManager ) ;
			}

			return sceneManager ;
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

			Viewport root = GetTree().Root ;
			CurrentScene = root.GetChild( root.GetChildCount() - 1 ) ;
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

		/// <summary>
		/// 現在のシーン
		/// </summary>
		public Node CurrentScene { get ; set ; }


		//-----------------------------------------------------------

		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool Load( string path )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Load_Private( path ) ;
		}

		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		private bool Load_Private( string path )
		{
			// 現在のシーンを破棄する
			if( CurrentScene != null )
			{
				CurrentScene.Free() ;
				CurrentScene = null ;
			}

			//----------------------------------------------------------

			var nextScene = ( PackedScene )GD.Load( path ) ;
			if( nextScene == null )
			{
				return false ;
			}

			CurrentScene = nextScene.Instantiate() ;

			GetTree().Root.AddChild( CurrentScene ) ;

			// オプション(古い記述との互換性のため)
			GetTree().CurrentScene = CurrentScene ;

			return true ;
		}

		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static T Load<T>( string path ) where T : Node
		{
			if( m_Instance == null )
			{
				return default ;
			}

			return m_Instance.Load_Private<T>( path ) ;
		}

		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		private T Load_Private<T>( string path ) where T : Node
		{
			// 現在のシーンを破棄する
			if( CurrentScene != null )
			{
				CurrentScene.Free() ;
				CurrentScene = null ;
			}

			//----------------------------------------------------------

			var nextScene = ( PackedScene )GD.Load( path ) ;
			if( nextScene == null )
			{
				return default ;
			}

			T currentScene = nextScene.Instantiate<T>() ;
			if( currentScene == null )
			{
				return default ;
			}

			CurrentScene = currentScene ;

			GetTree().Root.AddChild( CurrentScene ) ;

			// オプション(古い記述との互換性のため)
			GetTree().CurrentScene = CurrentScene ;

			return currentScene ;
		}

/*
		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		public async Task LoadAsync( string path, bool withFade = false, float fadeDuration = 0.25f, bool isBlockingFade = false )
		{
			if( withFade == true )
			{
				if( Fade.Instance != null )
				{
					Fade.Out( fadeDuration ) ;
				}
			}

			//----------------------------------------------------------

			// 現在のシーンを破棄する
			if( CurrentScene != null )
			{
				CurrentScene.Free() ;
				CurrentScene = null ;
			}

			//----------------------------------------------------------

			var nextScene = ( PackedScene )GD.Load( path ) ;
			if( nextScene == null )
			{
				return ;
			}

			CurrentScene = nextScene.Instantiate() ;

			GetTree().Root.AddChild( CurrentScene ) ;

			// オプション(古い記述との互換性のため)
			GetTree().CurrentScene = CurrentScene ;
		}
*/
		//-------------------------------------------------------------------------------------------

		public override void _Process( double delta ){}
	}
}

