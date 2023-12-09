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
		/// <summary>
		/// 状態
		/// </summary>
		public enum States
		{
			Title,
			Combat,
			Defeat,

			Unknown,
		}
	}
}
