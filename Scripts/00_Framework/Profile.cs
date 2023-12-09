using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;

namespace Sample_001
{
	public partial class Profile : ExCanvasLayer
	{
		// インスタンス(シングルトン)
		private static Profile m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static Profile Instance	=> m_Instance ;

		//-----------------------------------------------------------

		[Export]
		private Label		_Fps ;

		//-----------------------------------------------------------

		private ulong		m_FrameTimer ;
		private float		m_FrameCount ;

		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			// シングルトンインスタンスを保持する
			m_Instance = this ;

			//----------------------------------

//			Engine.TimeScale = 0.5f ;

			//----------------------------------

			m_FrameTimer = Time.GetTicksMsec() ;
			m_FrameCount = 0 ;

			_Fps.Text = "FPS --" ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( InputManager.GetKeyDown( KeyCodes.F3 ) == true )
			{
				// 表示のオンオフ
				_Fps.Visible = !_Fps.Visible ;
			}

			//----------------------------------------------------------

			m_FrameCount ++ ;

			ulong frameTimer = Time.GetTicksMsec() ;
			if( ( frameTimer - m_FrameTimer ) >= 1000 )
			{
				m_FrameTimer += 1000 ;

				_Fps.Text = $"FPS {m_FrameCount}" ;

				m_FrameCount = 0 ;
			}
		}
	}
}
