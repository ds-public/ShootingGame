using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;

using SceneHelper ;

namespace Sample_001
{
	/// <summary>
	/// 画面(シーン)の管理を行うクラス
	/// </summary>

	public partial class Scene : ExNode
	{
		/// <summary>
		/// スクリーン系のシーン
		/// </summary>
		public class Screen
		{
			public const string Battle = "Scenes/01_Screen/02_Battle/Battle.tscn" ;
		}

		/// <summary>
		/// レイアウト系のシーン
		/// </summary>
		public class Layout
		{
		}

		/// <summary>
		/// ダイアログ系のシーン
		/// </summary>
		public class Dialog
		{
		}

		//-------------------------------------------------------------------------------------------

		// インスタンス(シングルトン)
		private static Scene m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static Scene Instance	=> m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static Scene Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var node = new Scene()
			{
				Name = "Scene"
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

		//-------------------------------------------------------------------------------------------

		// 遷移先のシーンで準備が整ったかどうか
		private bool	m_IsReady ;

		/// <summary>
		/// 遷移先のシーンで準備が整ったかどうか
		/// </summary>
		public static bool IsReady
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_IsReady ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_IsReady = value ;
			}
		}

		// フェード効果を実行中かどうか
		private bool m_IsFading ;

		/// <summary>
		/// フェード効果を実行中かどうか
		/// </summary>
		public static bool IsFading
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_IsFading ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_IsFading = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool Load( string path, bool isFade = true, float fadeDuration = 0.25f, bool isBlocking = true )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			_ = m_Instance.Load_Private( path, isFade, fadeDuration, isBlocking ) ;

			return true ;
		}

		/// <summary>
		/// シーンをロードする
		/// </summary>
		/// <param name="path"></param>
		private async Task Load_Private( string path, bool isFade, float fadeDuration, bool isBlocking )
		{
			if( isFade == true && Fade.Instance != null )
			{
				// フェード実行中
				m_IsFading = true ;

				// フェード付き
				await Fade.Out( fadeDuration ) ;
			}

			if( isBlocking == true )
			{
				m_IsReady = false ;
			}

			if( SceneManager.Load( path ) == false )
			{
				// 失敗
				m_IsReady = true ;

				if( isFade == true && Fade.Instance != null )
				{
					// フェード付き
					await Fade.In( fadeDuration ) ;
				}

				// ダイアログを出したい

				return ;
			}

			//----------------------------------
			// 成功

			if( isBlocking == true )
			{
				// 遷移先のシーンの準備が整った事の応答を待つ
				await WaitUntil( () => ( m_IsReady == true ) ) ;
			}

			if( isFade == true && Fade.Instance != null )
			{
				// フェード付き
				await Fade.In( fadeDuration ) ;

				m_IsFading = false ;
			}
		}

		/// <summary>
		/// フェードが完了するのを待つ
		/// </summary>
		/// <returns></returns>
		public static async Task WaitForFading( bool isReady = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			await m_Instance.WaitForFading_Private( isReady ) ;
		}

		// フェードが完了するのを待つ
		private async Task WaitForFading_Private( bool isReady )
		{
			if( m_IsFading == false )
			{
				return ;
			}

			if( isReady == true )
			{
				m_IsReady = true ;
			}

			await WaitUntil( () => ( m_IsFading == false ) ) ;
		}
	}
}
