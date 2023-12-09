using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;


namespace Sample_001
{
	/// <summary>
	/// フェード制御クラス(シングルトン)
	/// </summary>
	public partial class Fade : ExCanvasLayer
	{
		// インスタンス(シングルトン)
		private static Fade m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static Fade Instance	=> m_Instance ;

		//-----------------------------------------------------------

		[Export]
		private AnimationPlayer	m_AnimationPlayer ;

		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			m_Instance = this ;

			Hide() ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ){}

		//-----------------------------------------------------------

		/// <summary>
		/// フェードイン
		/// </summary>
		/// <returns></returns>
		public static async Task In( float duration = 1.0f )
		{
			if( m_Instance == null )
			{
				return ;
			}

			await m_Instance.In_Private( duration ) ;
		}

		// フェードイン
		private async Task In_Private( float duration )
		{
			if( duration <= 0 )
			{
				Hide() ;
			}

			//----------------------------------

			Show() ;

			m_AnimationPlayer.SpeedScale = 1.0f / duration ;

			m_AnimationPlayer.Play( "Hide" ) ;

			while( m_AnimationPlayer.IsPlaying() == true )
			{
				await Yield() ;
			}

			Hide() ;
		}

		/// <summary>
		/// フェードアウト
		/// </summary>
		/// <returns></returns>
		public static async Task Out( float duration = 1.0f )
		{
			if( m_Instance == null )
			{
				return ;
			}

			await m_Instance.Out_Private( duration ) ;
		}

		// フェードアウト
		private async Task Out_Private( float duration )
		{
			Show() ;

			m_AnimationPlayer.SpeedScale = 1.0f / duration ;

			m_AnimationPlayer.Play( "Show" ) ;

			while( m_AnimationPlayer.IsPlaying() == true )
			{
				await Yield() ;
			}
		}

		//---------------

		/// <summary>
		/// 表示設定
		/// </summary>
		/// <returns></returns>
		public static void Mask( bool state )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Mask_Private( state ) ;
		}

		// フェードイン
		private void Mask_Private( bool state )
		{
			m_AnimationPlayer.Play( "Default" ) ;
			if( state == true )
			{
				Show() ;
			}
			else
			{
				Hide() ;
			}
		}
	}
}

