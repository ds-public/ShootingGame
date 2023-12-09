using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		// 無敵コマンドが有効かどうか
		private bool m_IsNoDeathSuccessful			= false ;

		// 隠しコマンドが成功したかどうか
		private bool m_IsSpecialCommandSuccessful	= false ;

		//-----------------------------------------------------------

		private static readonly uint[] m_CommandPattern =
		{
			0x00040000, 0x00040000, 0x00080000, 0x00080000, 0x00020000, 0x00010000, 0x00020000, 0x00010000,
		} ;

		private int m_CommandSequence = 0 ;

		private bool m_CommandReady = false ;

		private uint m_CommandNext ;

		// 最後の２つのボタン押し待ち状態に入っているか
		private bool IsSpacialCommandReady()
		{
			return m_CommandReady ;
		}

		// 特殊コマンドを確認する
		private bool CheckSpacialCommand()
		{
			uint flag = 0 ;

			if( GamePad.GetButtonDown( GamePad.B1 ) == true ){ flag |= 0x0001 ; }
			if( GamePad.GetButtonDown( GamePad.B2 ) == true ){ flag |= 0x0002 ; }
			if( GamePad.GetButtonDown( GamePad.B3 ) == true ){ flag |= 0x0004 ; }
			if( GamePad.GetButtonDown( GamePad.B4 ) == true ){ flag |= 0x0008 ; }
			if( GamePad.GetButtonDown( GamePad.R1 ) == true ){ flag |= 0x0010 ; }
			if( GamePad.GetButtonDown( GamePad.L1 ) == true ){ flag |= 0x0020 ; }
			if( GamePad.GetButtonDown( GamePad.R2 ) == true ){ flag |= 0x0040 ; }
			if( GamePad.GetButtonDown( GamePad.L2 ) == true ){ flag |= 0x0080 ; }
			if( GamePad.GetButtonDown( GamePad.R3 ) == true ){ flag |= 0x0100 ; }
			if( GamePad.GetButtonDown( GamePad.L3 ) == true ){ flag |= 0x0200 ; }
			if( GamePad.GetButtonDown( GamePad.O1 ) == true ){ flag |= 0x0400 ; }
			if( GamePad.GetButtonDown( GamePad.O2 ) == true ){ flag |= 0x0800 ; }
			if( GamePad.GetButtonDown( GamePad.O3 ) == true ){ flag |= 0x1000 ; }
			if( GamePad.GetButtonDown( GamePad.O4 ) == true ){ flag |= 0x2000 ; }

			// ※Godot では、↑はY- ↓はY+ である事に注意

			var axis_0 = GamePad.GetAxisDown( 0 ) ;
			if( axis_0.X >  0 ){ flag |= 0x00010000 ; }	// →
			if( axis_0.X <  0 ){ flag |= 0x00020000 ; }	// ←
			if( axis_0.Y <  0 ){ flag |= 0x00040000 ; }	// ↑
			if( axis_0.Y >  0 ){ flag |= 0x00080000 ; }	// ↓

			var axis_1 = GamePad.GetAxisDown( 1 ) ;
			if( axis_1.X >  0 ){ flag |= 0x00010000 ; }	// →
			if( axis_1.X <  0 ){ flag |= 0x00020000 ; }	// ←
			if( axis_1.Y <  0 ){ flag |= 0x00040000 ; }	// ↑
			if( axis_1.Y >  0 ){ flag |= 0x00080000 ; }	// ↓

			var axis_2 = GamePad.GetAxisDown( 2 ) ;
			if( axis_2.X >  0 ){ flag |= 0x01000000 ; }	// →
			if( axis_2.X <  0 ){ flag |= 0x02000000 ; }	// ←
			if( axis_2.Y <  0 ){ flag |= 0x04000000 ; }	// ↑
			if( axis_2.Y >  0 ){ flag |= 0x08000000 ; }	// ↓

			if( flag == 0 )
			{
				// 入力無し
				return false ;
			}

			//----------------------------------------------------------

			if( m_CommandReady == false )
			{
				if( m_CommandPattern[ m_CommandSequence ] == flag )
				{
					m_CommandSequence ++ ;
					if( m_CommandSequence >= m_CommandPattern.Length )
					{
						m_CommandSequence = 0 ;
						m_CommandReady = true ;
					}
				}
				else
				{
					m_CommandSequence = 0 ;
				}
			}
			else
			{
				if( m_CommandSequence == 0 )
				{
					if( flag == 0x0001 )
					{
						m_CommandSequence ++ ;
						m_CommandNext = 0x0002 ;
					}
					else
					if( flag == 0x0002 )
					{
						m_CommandSequence ++ ;
						m_CommandNext = 0x0001 ;
					}
					else
					{
						// リセット
						m_CommandSequence = 0 ;
						m_CommandReady = false ;
					}
				}
				else
				{
					if( flag == m_CommandNext ) 
					{
						// 成功
						m_CommandSequence = 0 ;
						m_CommandReady = false ;

						return true ;
					}
					else
					{
						// リセット
						m_CommandSequence = 0 ;
						m_CommandReady = false ;
					}
				}
			}

			return false ;
		}
	}
}
