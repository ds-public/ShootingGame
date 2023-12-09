﻿using Godot ;
using System ;
using System.Threading ;
using System.Threading.Tasks ;
using System.Runtime.CompilerServices ;


namespace ExGodot
{
	/// <summary>
	/// Node3D の機能拡張クラス
	/// </summary>
	[GlobalClass]
	public partial class ExNode3D : Node3D
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

		/// <summary>
		/// ツリーの中から指定の型のインスタンスを探し最初に見つかったものを返す
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="includeInternal"></param>
		/// <returns></returns>
		public T FindObjectOfType<T>( bool includeInternal = false ) where T : Node
		{ return Implementation.FindObjectOfType<T>( this, includeInternal ) ; }

		/// <summary>
		/// ツリーの中から指定の型のインスタンスを探し見つかったものを全て返す
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="includeInternal"></param>
		/// <returns></returns>
		public T[] FindObjectsOfType<T>( bool includeInternal = false ) where T : Node
		{ return Implementation.FindObjectsOfType<T>( this, includeInternal ) ; }

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
