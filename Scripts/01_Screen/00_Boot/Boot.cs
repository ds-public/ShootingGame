using Godot ;
using ExGodot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using SceneHelper ;

namespace Sample_001
{
	/// <summary>
	/// ブート画面
	/// </summary>
	public partial class Boot : ExNode2D
	{
		// 画面サイズ
		public Vector2		ScreenSize ; // Size of the game window.

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 状態
		/// </summary>
		public enum States
		{
			Startup,

			Unknown,
		}


		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			ScreenSize = GetViewportRect().Size ;

			//----------------------------------

			// メインループ実行
			_ = MainLoop( States.Startup ) ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ){}

		//-------------------------------------------------------------------------------------------

		// メインループ
		private async Task MainLoop( States state )
		{
			States existing = state ;
			States previous = States.Unknown ;
			States next		= States.Unknown ;

			// ステート分岐
			while( existing != States.Unknown )
			{
				switch( existing )
				{
					case States.Startup	: next = await State_Startup( previous )		; break ;
				}

				previous = existing ;
				existing = next ;
			}
		}

		//-----------------------------------

		// ステート：開始
		private async Task<States> State_Startup( States _ )
		{
			await Fade.In( 0.5f ) ;

			//----------------------------------------------------------

			var timer = new SimpleTimer() ;

			while( timer.IsRunning( 2.0f ) )
			{
				if
				(
					GamePad.GetButtonDown( GamePad.B1 ) == true ||
					GamePad.GetButtonDown( GamePad.B2 ) == true ||
					GamePad.GetButtonDown( GamePad.O1 ) == true ||
					Pointer.GetButtonDown( 0 ) == true
				)
				{
					break ;
				}

				// １フレーム待つ
				await Yield() ;
			}

			Scene.Load( Scene.Screen.Battle ) ;

			return States.Unknown ;
		}
	}
}
