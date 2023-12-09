using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading ;
using System.Threading.Tasks ;


namespace Sample_001
{
	/// <summary>
	/// キャンセル可能なタスククラス
	/// </summary>
	public partial class CancelableTask
	{
		private Node m_Owner ;

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		public CancelableTask()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="owner"></param>
		public CancelableTask( Node owner )
		{
			m_Owner = owner ;
		}

		/// <summary>
		/// オーナーを設定する
		/// </summary>
		/// <param name="owner"></param>
		public void SetOwner( Node owner )
		{
			m_Owner = owner ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// １フレーム待機する
		/// </summary>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		public async Task Yield( LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			if( m_Owner == null )
			{
				return ;
			}

			if( GodotObject.IsInstanceValid( m_Owner ) == false )
			{
				// 自身は破棄されている
				throw new OperationCanceledException() ;
			}

			// 明示的なキャンセルが要求された
			cancellationToken.ThrowIfCancellationRequested() ;

			switch( timing )
			{
				case LoopTiming.Process :
					// １フレーム待機する
					await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
				break ;

				case LoopTiming.PhysicsProcess :
					// １フレーム待機する
					await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
				break ;
			}
		}

		/// <summary>
		/// 指定した時間(秒単位)だけ待機する
		/// </summary>
		/// <param name="node"></param>
		/// <param name="time"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		public async Task WaitForSeconds( float time, bool ignoreTimeScale = false, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			// タイムスケールは後で対応する

			if( m_Owner == null || time <= 0 )
			{
				return ;
			}

			ulong millisecondsDelay = ( ulong )( time * 1000.0f ) ;
			ulong basis = Time.GetTicksMsec() ;

			switch( timing )
			{
				case LoopTiming.Process :
					while( ( Time.GetTicksMsec() - basis ) <  millisecondsDelay )
					{
						if( GodotObject.IsInstanceValid( m_Owner ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( ( Time.GetTicksMsec() - basis ) <  millisecondsDelay )
					{
						if( GodotObject.IsInstanceValid( m_Owner ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
					}
				break ;
			}
		}

		/// <summary>
		/// 条件が満たされている間は待機する
		/// </summary>
		/// <param name="node"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		public async Task WaitWhile( Func<bool> action, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			if( m_Owner == null || action == null )
			{
				return ;
			}

			switch( timing )
			{
				case LoopTiming.Process :
					while( action() == true )
					{
						if( GodotObject.IsInstanceValid( m_Owner ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( action() == true )
					{
						if( GodotObject.IsInstanceValid( m_Owner ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
					}
				break ;
			}
		}

		/// <summary>
		/// 条件が満たされるまでは待機する
		/// </summary>
		/// <param name="node"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		public async Task WaitUntil( Func<bool> action, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			if( m_Owner == null || action == null )
			{
				return ;
			}

			switch( timing )
			{
				case LoopTiming.Process :
					while( action() == false )
					{
						if( GodotObject.IsInstanceValid( m_Owner ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( action() == false )
					{
						if( GodotObject.IsInstanceValid( m_Owner ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await m_Owner.ToSignal( m_Owner.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;
			}
		}

	}
}

