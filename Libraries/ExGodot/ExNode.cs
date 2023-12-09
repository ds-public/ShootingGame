using Godot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading ;
using System.Threading.Tasks ;
using System.Runtime.CompilerServices ;

namespace ExGodot
{
	/// <summary>
	/// Unity と同名のログ出力クラス
	/// </summary>
	public class Debug
	{
		/// <summary>
		/// メッセージを出力する
		/// </summary>
		/// <param name="message"></param>
		public static void Log( string message )
		{
			GD.Print( message ) ;
		}

		/// <summary>
		/// ワーニングメッセージを出力する
		/// </summary>
		/// <param name="message"></param>
		public static void LogWarning( string message )
		{
			GD.PushWarning( message ) ;
		}

		/// <summary>
		/// エラーメッセージを出力する
		/// </summary>
		/// <param name="message"></param>
		public static void LogError( string message )
		{
			GD.PushError( message ) ;
		}
	}


	/// <summary>
	/// 待機するタイミング
	/// </summary>
	public enum LoopTiming
	{
		Process,
		PhysicsProcess,
	}

	/// <summary>
	/// 実装クラス
	/// </summary>
	public class Implementation
	{
		/// <summary>
		/// ノードの状態を設定する
		/// </summary>
		/// <param name="state"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetActive( Node node, bool state, ref Node stackedParent )
		{
			if( state == true )
			{
				var parent = node.GetParent() ;
				if( parent != null || stackedParent == null )
				{
					// 既に有効
					return ;
				}

				stackedParent.AddChild( node ) ;
				stackedParent = null ;
			}
			else
			{
				var parent = node.GetParent() ;
				if( parent == null || stackedParent != null )
				{
					// 既に無効
					return ;
				}

				stackedParent = parent ;
				stackedParent.RemoveChild( node ) ;
			}
		}

		/// <summary>
		/// シーンをインスタンス化すると同時に指定した親に追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="packedScene"></param>
		/// <param name="parent"></param>
		/// <param name="forceReadableName"></param>
		/// <param name="internal"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T AddChild<T>( PackedScene packedScene, Node parent, bool forceReadableName = false, Node.InternalMode @internal = Node.InternalMode.Disabled ) where T : Node
		{
			T instance = packedScene.Instantiate<T>() ;
			if( instance != null )
			{
				parent.AddChild( instance, forceReadableName, @internal ) ;
			}

			return instance ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ツリーの中から指定の型のインスタンスを探し最初に見つかったものを返す
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="myself"></param>
		/// <param name="includeInternal"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T FindObjectOfType<T>( Node myself, bool includeInternal = false ) where T : Node
		{
			var nodes = myself.GetTree().Root.GetChildren( includeInternal ) ;
			if( nodes != null && nodes.Count >  0 )
			{
				foreach( var node in nodes )
				{
					if( node.GetType() == typeof( T ) )
					{
						return node as T ;
					}
				}
			}

			return default ;
		}

		/// <summary>
		/// ツリーの中から指定の型のインスタンスを探し見つかったものを全て返す
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="myself"></param>
		/// <param name="includeInternal"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T[] FindObjectsOfType<T>( Node myself, bool includeInternal = false ) where T : Node
		{
			var targets = new List<T>() ; 

			var nodes = myself.GetTree().Root.GetChildren( includeInternal ) ;
			if( nodes != null && nodes.Count >  0 )
			{
				foreach( var node in nodes )
				{
					if( node.GetType() == typeof( T ) )
					{
						targets.Add( node as T ) ;
					}
				}
			}

			return targets.ToArray() ;
		}


		//-----------------------------------------------------------

		/// <summary>
		/// １フレーム待機する
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static async Task Yield( Node node, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			if( GodotObject.IsInstanceValid( node ) == false )
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
					await node.ToSignal( node.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
				break ;

				case LoopTiming.PhysicsProcess :
					// １フレーム待機する
					await node.ToSignal( node.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
				break ;
			}
		}

		/// <summary>
		/// 指定した時間(ミリ秒単位)だけ待機する
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static async Task Delay( Node node, int millisecondsDelay, bool ignoreTimeScale = false, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			// タイムスケールは後で対応する

			if( millisecondsDelay <= 0 )
			{
				return ;
			}

			ulong basis = Time.GetTicksMsec() ;

			switch( timing )
			{
				case LoopTiming.Process :
					while( ( int )( Time.GetTicksMsec() - basis ) <  millisecondsDelay )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( ( int )( Time.GetTicksMsec() - basis ) <  millisecondsDelay )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
					}
				break ;
			}
		}

		/// <summary>
		/// 指定したフレーム数だけ待機する
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static async Task DelayFrame( Node node, int delayFrameCount, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			// タイムスケールは後で対応する

			if( delayFrameCount <= 0 )
			{
				return ;
			}

			int frameCount = 0 ;

			switch( timing )
			{
				case LoopTiming.Process :
					while( frameCount <  delayFrameCount )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;

						frameCount ++ ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( frameCount <  delayFrameCount )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.ProcessFrame ) ;

						frameCount ++ ;
					}
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static async Task WaitForSeconds( Node node, float time, bool ignoreTimeScale = false, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			// タイムスケールは後で対応する

			if( time <= 0 )
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
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( ( Time.GetTicksMsec() - basis ) <  millisecondsDelay )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static async Task WaitWhile( Node node, Func<bool> action, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			if( action == null )
			{
				return ;
			}

			switch( timing )
			{
				case LoopTiming.Process :
					while( action() == true )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( action() == true )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static async Task WaitUntil( Node node, Func<bool> action, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{
			if( action == null )
			{
				return ;
			}

			switch( timing )
			{
				case LoopTiming.Process :
					while( action() == false )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.ProcessFrame ) ;
					}
				break ;

				case LoopTiming.PhysicsProcess :
					while( action() == false )
					{
						if( GodotObject.IsInstanceValid( node ) == false )
						{
							// 自身は破棄されている
							throw new OperationCanceledException() ;
						}

						// 明示的なキャンセルが要求された
						cancellationToken.ThrowIfCancellationRequested() ;

						// １フレーム待機する
						await node.ToSignal( node.GetTree(), SceneTree.SignalName.PhysicsFrame ) ;
					}
				break ;
			}
		}
	}

	//------------------------------------

	/// <summary>
	/// Node の機能拡張クラス
	/// </summary>
	[GlobalClass]
	public partial class ExNode : Node
	{
		//-------------------------------------------------------------------------------------------------------------------
		// 全てのメソッド拡張クラスで共通のメソッド群

		// 親ノード
		private Node m_Parent ;

		/// <summary>
		/// ノードの状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{ Implementation.SetActive( this, state, ref m_Parent ) ; }

		/// <summary>
		/// アクティブ状態であるかどうか
		/// </summary>
		public bool ActivateSelf => ( m_Parent == null ) ;

		/// <summary>
		/// シーンをインスタンス化すると同時に指定した親に追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="packedScene"></param>
		/// <param name="parent"></param>
		/// <param name="forceReadableName"></param>
		/// <param name="internal"></param>
		/// <returns></returns>
		public T AddChild<T>( PackedScene packedScene, Node parent = null, bool forceReadableName = false, Node.InternalMode @internal = Node.InternalMode.Disabled ) where T : Node
		{
			parent ??= this ;

			return Implementation.AddChild<T>( packedScene, parent, forceReadableName, @internal ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// １フレーム待機する
		/// </summary>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		protected async Task Yield( LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{ await Implementation.Yield( this, timing, cancellationToken ) ; }

		/// <summary>
		/// 指定した時間(ミリ秒単位)だけ待機する
		/// </summary>
		/// <param name="millisecondsDelay"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected async Task Delay( int millisecondsDelay, bool ignoreTimeScale = false, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{await Implementation.Delay( this, millisecondsDelay, ignoreTimeScale, timing, cancellationToken ) ; }

		/// <summary>
		/// 指定したフレーム数だけ待機する
		/// </summary>
		/// <param name="delayFrameCount"></param>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected async Task DelayFrame( int delayFrameCount, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{await Implementation.DelayFrame( this, delayFrameCount, timing, cancellationToken ) ; }

		/// <summary>
		/// 指定した時間(秒単位)だけ待機する
		/// </summary>
		/// <param name="time"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected async Task WaitForSeconds( float time, bool ignoreTimeScale = false, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{await Implementation.WaitForSeconds( this, time, ignoreTimeScale, timing, cancellationToken ) ; }

		/// <summary>
		/// 条件が満たされている間は待機する
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		protected async Task WaitWhile( Func<bool> action, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{ await Implementation.WaitWhile( this, action, timing, cancellationToken ) ; }

		/// <summary>
		/// 条件が満たされるまでは待機する
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		protected async Task WaitUntil( Func<bool> action, LoopTiming timing = LoopTiming.Process, CancellationToken cancellationToken = default )
		{ await Implementation.WaitUntil( this, action, timing, cancellationToken ) ; }

	}




}
