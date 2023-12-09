using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	/// <summary>
	/// プレイヤーの弾のコリジョンタイプ
	/// </summary>
	public enum PlayerShotCollisionTypes
	{
		/// <summary>
		/// コリジョン無効
		/// </summary>
		None,

		/// <summary>
		/// ショットタイプ
		/// </summary>
		Shot,

		/// <summary>
		/// ボムタイプ(破壊不能)
		/// </summary>
		Bomb,
	}

	/// <summary>
	/// エネミーのコリジョンタイプ
	/// </summary>
	public enum EnemyCollisionTypes
	{
		/// <summary>
		/// コリジョン無効
		/// </summary>
		None,

		/// <summary>
		/// Shot と Bomb 有効
		/// </summary>
		Unit,

		/// <summary>
		/// Shot のみ有効
		/// </summary>
		Boss,
	}

	/// <summary>
	/// プレイヤーの弾が破壊された理由
	/// </summary>
	public enum PlayerShotDestroyedReasonTypes
	{
		/// <summary>
		/// エネミーに当たった
		/// </summary>
		Enemy,

		/// <summary>
		/// 自己破壊
		/// </summary>
		Self,

		/// <summary>
		/// 画面外に出た
		/// </summary>
		OutOfScreen,
	}


	/// <summary>
	/// エネミーが破壊された理由
	/// </summary>
	public enum EnemyDestroyedReasonTypes
	{
		/// <summary>
		/// プレイヤーの弾
		/// </summary>
		PlayerShot,

		/// <summary>
		/// プレイヤーのボム
		/// </summary>
		PlayerBomb,

		/// <summary>
		/// 自己破壊
		/// </summary>
		Self,

		/// <summary>
		/// 画面外に出た
		/// </summary>
		OutOfScreen,
	}

	//---------------------------------------------------------------------------------------------

	/// <summary>
	/// プレイヤー弾の外観
	/// </summary>
	public enum PlayerShotShapeTypes
	{
		ShotWeakly	= 0,	// 弱い弾
		ShotStrong	= 1,	// 強い球
		GravityBall	= 2,	// 拡散タイプボムの弾
		GravityWave	= 3,	// 放射タイプボムの弾
	}


	//---------------------------------------------------------------------------------------------

	/// <summary>
	/// エネミーの外観
	/// </summary>
	public enum EnemyShapeTypes
	{
		/// <summary>
		/// パワーアップユニット
		/// </summary>
		No_000	=   0,

		No_001	=   1,
		No_002	=   2,
		No_003	=   3,
		No_004	=   4,
		No_005	=   5,
		No_006	=   6,
		No_007	=   7,
		No_008	=   8,
		No_009	=   9,
		No_010	=  10,
		No_011	=  11,
		No_012	=  12,
		No_013	=  13,
		No_014	=  14,
		No_015	=  15,
		No_016	=  16,
		No_017	=  17,
		No_018	=  18,
		No_019	=  19,
		No_020	=  20,
		No_021	=  21,
		No_022	=  22,
		No_023	=  23,
		No_024	=  24,
		No_025	=  25,
		No_026	=  26,
		No_027	=  27,
		No_028	=  28,
		No_029	=  29,
		No_030	=  30,
		No_031	=  31,
		No_032	=  32,
		No_033	=  33,
		No_034	=  34,


		No_999	=  35,
	}

	/// <summary>
	/// エネミー弾の外観
	/// </summary>
	public enum EnemyBulletShapeTypes
	{
		/// <summary>
		/// 小さい弾
		/// </summary>
		BulletSmall = 0,

		/// <summary>
		/// 大きい弾
		/// </summary>
		BulletLarge = 1,

		/// <summary>
		/// 衝撃波
		/// </summary>
		ShockWave	= 2,

		/// <summary>
		/// 小さいレーザー
		/// </summary>
		LaserTiny	= 3,

		/// <summary>
		/// 細いレーザー
		/// </summary>
		LaserSlim	= 4,

		/// <summary>
		/// 太いレーザー
		/// </summary>
		LaserWide	= 5,

		/// <summary>
		/// ミサイル
		/// </summary>
		Missile		= 6,
	}

	//---------------------------------------------------------------------------------------------

	/// <summary>
	/// アイテムの種類
	/// </summary>
	public enum ItemShapeTypes
	{
		Power	= 0,
		Shield	= 1,

		Bomb_Compression	= 2,
		Bomb_Diffusion		= 3,
		Bomb_Emission		= 4,
	}

	/// <summary>
	/// ボムの種別
	/// </summary>
	public enum BombTypes
	{
		Compression			= 0,
		Diffusion			= 1,
		Emission			= 2,
	}
}
