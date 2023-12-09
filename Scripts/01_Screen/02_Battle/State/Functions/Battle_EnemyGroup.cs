using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading ;
using System.Threading.Tasks ;

using InputHelper ;
using StorageHelper ;


namespace Sample_001
{
	public partial class Battle
	{
		// エネミーをグループを処理する
		private async Task ProcessEnemyGroup()
		{
			// エネミーグループ管理情報を初期化する
			m_EnemyGroupCounters.Clear() ;

			// エネミーグループの識別子
			int enemyGroupId = 0 ;

			// レベル計測タイマー
			var levelTimer = new SimpleTimer() ;

			// 多目的用途タイマー
			var otherTimer = new SimpleTimer() ;

			//----------------------------------

			// 予め配列化
			var enemyGroups = m_EnemyGroups.Values.ToArray() ;

			// 処理中フラグを初期化する
			CleanupEnamyGroups( enemyGroups ) ;

//			await WaitForSeconds( 3 ) ;

			//----------------------------------

			int levelChecker = -1 ;

			int level, levelMax = 100 ;
			float levelUpInterval = 6.0f ;

			while( IsPlayerDestroyed == false )
			{
				// ６秒で１上昇する・最大レベルは１００で１０分で到達する
				level = ( int )( levelTimer.Value / levelUpInterval ) ;
				if( level >  levelMax )
				{
					level  = levelMax ;
				}
//				level = 100 ; // デバッグ

				if( level >  levelChecker )
				{
					levelChecker = level ;
					GD.Print( "レベルアップ : " + levelChecker ) ;
				}

				//---------------------------------------------------------
				
				if( m_CombatAudio.IsFading == true )
				{
					// ステージＢＧＭフェード中は新しいエネミーグループを出現させない

					// これが無いとフリーブするから注意
					await Yield() ;

					continue ;
				}

				//---------------------------------------------------------
				// 出現させるエネミーグループを選別する

//				GD.Print( "出現エネミーグループを選別する : " + enemyGroups.Length ) ;

//				var enemyGroup = m_EnemyGroups[ 0 ] ;	// デバッグ(テスト中のエネミーグループ)
				var enemyGroup = ChoiceEnemyGroup( enemyGroups ) ;
				if( enemyGroup == null )
				{
					// 現在処理できるものが無い

					GD.Print( "現在処理可能なエネミーグループが見つからない(異常)" ) ;

					// これが無いとフリーブするから注意
					await Yield() ;

					continue ;
				}

				GD.Print( "出現エネミーグループ : " + enemyGroup.GetType().ToString() ) ;

				//---------------------------------

				// エネミーグループの出現処理を実行する
				var intervalTime = enemyGroup.Run( this, level, enemyGroupId, m_CombatFinishedTokenSource.Token ) ;

				if( intervalTime <= 0 )
				{
//					GD.Print( "プレイヤーによる殲滅待ち : " + intervalTime ) ;

					// プレイヤーによって殲滅させられるのを待つ
					while( IsPlayerDestroyed == false )
					{
						if( m_EnemyGroupCounters.ContainsKey( enemyGroupId ) == false )
						{
							// Run を実行した直後に必ずカウンターは登録される(登録が無くなるのを待つ)

							m_CombatAudio.StopBossBgm() ;

							break ;
						}

						await Yield() ;
					}
				}
				else
				{
					// 時間経過で次のエネミーグループの出現処理へ

//					GD.Print( "待ち時間 : " + intervalTime ) ;

					otherTimer.Reset() ;
					while( IsPlayerDestroyed == false )
					{
						if( otherTimer.IsFinished( intervalTime ) == true )
						{
							// 時間経過
							break ;
						}

						//-------------------------------
						// エネミーグループ出現後の待ち時間中にそのエネミーグループが殲滅できた場合は待ち時間を終了する

						if( m_EnemyGroupCounters.ContainsKey( enemyGroupId ) == false )
						{
							break ;
						}

						await Yield() ;
					}
				}

				//---------------------------------------------------------

				// エネミーグループ識別子を増加させる(次のエネミーグループ識別子へ)
				enemyGroupId ++ ;

//				GD.Print( "次のエネミーグループへ : 識別子 = " + enemyGroupId ) ;

				//---------------------------------
				// レベル上昇によって短縮される待ち時間の処理

				intervalTime = 3.0f - ( 3.0f * level / 100.0f ) ;
				if( intervalTime <  0.1f )
				{
					intervalTime  = 0.1f ;
				}

//				GD.Print( "レベルによって減少する待ち時間 : " + intervalTime ) ;

				otherTimer.Reset() ;
				while( IsPlayerDestroyed == false )
				{
					if( otherTimer.IsFinished( intervalTime ) == true )
					{
						// 時間経過
						break ;
					}

					await Yield() ;
				}

//				GD.Print( "次のサイクルへ" ) ;
			}
		}

		// 全てのエネミーグループを見初期化状態にする(途中バトルが中断されるとフラグがクリアされない)
		private void CleanupEnamyGroups( EnemyGroupBase[] enemyGroups )
		{
			// 処理中フラグを初期化する
			foreach( var enemyGroup in enemyGroups )
			{
				enemyGroup.IsProcerssing = false ;
			}
		}

		// 出現させるエネミーグループをチョイスする
		private EnemyGroupBase ChoiceEnemyGroup( EnemyGroupBase[] enemyGroups )
		{
			int totalWeight = 0 ;
			foreach( var enemyGroup in enemyGroups )
			{
				// 現在処理中のエネミーグループと同じ種類のものは選出から外す
				if( enemyGroup.IsProcerssing == false )
				{
					totalWeight += enemyGroup.EncountWeight ;
				}
			}

//			GD.Print( "選出対象エネミーグループのトータルウェイト : " + totalWeight ) ;

			if( totalWeight == 0 )
			{
				// 現在処理できるものが無い
				return null ;
			}

			int weightValue = ( int )( GD.Randi() % totalWeight ) ;

//			GD.Print( "選出乱数値 : " + weightValue ) ;

			int i, l = enemyGroups.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var enemyGroup = enemyGroups[ i ] ;
//				GD.Print( "選出確認 : " + i + " / " + l + " P = " + enemyGroup.IsProcerssing ) ;

				if( enemyGroup.IsProcerssing == false )
				{
//					GD.Print( "ウェイト比較 : " + weightValue + " / " + enemyGroups[ i ].EncountWeight ) ;
					if( weightValue <  enemyGroups[ i ].EncountWeight )
					{
//						GD.Print( "選出確定 : " + i + " / " + l ) ;
						return enemyGroups[ i ] ;
					}
					else
					{
						weightValue -= enemyGroups[ i ].EncountWeight ;
					}
				}
			}

//			GD.Print( "対象エネミーグループが選出できなかった" ) ;

			// 現在処理出来るものが無い
			return null ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// エネミーグループ情報(基底クラス)
		/// </summary>
		public class EnemyGroupBase : CancelableTask
		{
			protected Battle	m_Owner ;

			//----------------------------------

			/// <summary>
			/// 出現最低レベル
			/// </summary>
			public int		MinimumLevel ;

			/// <summary>
			/// 出現確率の重み
			/// </summary>
			public int		EncountWeight ;

			/// <summary>
			/// 次のグループ出現までの最低待ち時間(秒)　※０でボス扱い
			/// </summary>
			public float	IntervalTime ;

			/// <summary>
			/// パワーアップアイテムの強制出現(０＝グループ全滅時に確率で出現・１～＝個々を破壊した際に指定個数が出現する)
			/// </summary>
			public int		ItemAmount ;

			/// <summary>
			/// グループ殲滅時の１体あたりの基本アイテム出現率
			/// </summary>
			public int		ItemBaseAvarage = 5 ;

			/// <summary>
			/// 現在生成処理中かどうか(現在処理中のものは処理対象にしない)
			/// </summary>
			public bool		IsProcerssing ;

			//----------------------------------

			// グループ処理の汎用タイマー
			protected	SimpleTimer			m_Timer ;

			// 強制中断用のトークン
			protected	CancellationToken	m_CombatFinishedToken ;

			//----------------------------------

			/// <summary>
			/// デフォルトコンストラクタ
			/// </summary>
			public EnemyGroupBase()
			{
				m_Timer = new SimpleTimer() ;
			}

			/// <summary>
			/// スタートアップを行う
			/// </summary>
			/// <param name="owner"></param>
			/// <param name="combatFinishedToken"></param>
			protected void Startup( Battle owner, CancellationToken combatFinishedToken )
			{
				// オーナーを保持しておく
				m_Owner					= owner ;
				m_CombatFinishedToken	= combatFinishedToken ;

				//---------------------------------

				// アプリケーションが突然終了した場合に備えてタスク中断判定用のオーナーを登録する
				SetOwner( owner ) ;

				//---------------------------------

				// タイマーをリセットする(重要)
				m_Timer.Reset() ;
			}

			/// <summary>
			/// エネミーグループ生成を実行する
			/// </summary>
			/// <param name="owner"></param>
			/// <param name="groupId"></param>
			/// <param name="combatFinishedToken"></param>
			/// <returns></returns>
			public virtual float Run( Battle owner, int level, int groupId, CancellationToken combatFinishedToken )
			{
				return 0 ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// レベルに応じて補正をかけたシールド値を取得する
			/// </summary>
			/// <param name="min"></param>
			/// <param name="max"></param>
			/// <param name="level"></param>
			/// <returns></returns>
			protected static int GetShield( int min, int max, int level )
			{
				return ( ( max - min ) * level / 100 ) + min ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// エネミーからのプレイヤーの方向ベクトルを取得する
			/// </summary>
			/// <param name="enemy"></param>
			/// <returns></returns>
			protected Vector2 GetPlayerDirection( Enemy enemy )
			{
				if
				(
					m_Owner == null						||
					m_Owner.IsPlayerDestroyed == true	||
					m_Owner.Player.Position == enemy.Position
				)
				{
					// 現在の方向を返す
					return ExMath.GetRotatedVector( new Vector2(  0, +1 ), enemy.Rotation ) ;
				}

				return ( m_Owner.Player.Position - enemy.Position ).Normalized() ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// 完全に画面外かどうか
			/// </summary>
			/// <param name="enemy"></param>
			/// <returns></returns>
			protected static bool IsOutOfScreen( Enemy enemy, float distance = 0.7f )
			{
				Vector2 p = enemy.RatioPosition ;

				if( p.X <  ( - distance ) || p.X >  ( + distance ) || p.Y <  ( - distance ) || p.Y >   ( + distance ) )
				{
					return true ;
				}

				return false ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// 待機(エネミーグループのエネミー生成中の時間待ちはこれを使うこと)
			/// </summary>
			/// <param name="duration"></param>
			/// <returns></returns>
			protected async Task Wait( float duration )
			{
				if( duration <= 0 )
				{
					return ;
				}

				while( m_Timer.IsRunning( duration ) == true )
				{
					// １フレーム待つ
					await Yield( cancellationToken: m_CombatFinishedToken ) ;
				}

				m_Timer.Reset() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// エネミーグループ一覧
		private Dictionary<int,EnemyGroupBase> m_EnemyGroups = new ()
		{
			{   0,	// パワーアップアイテムユニット
				new EnemyGroup_000()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2, ItemAmount = 1,
				}
			},
			{   1,	// 画面の上・左右のいずれかから出現し直線するのみ
				new EnemyGroup_001()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   2,	// 左右大きくに蛇行
				new EnemyGroup_002()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   3,	// 高速に特攻かゆっくり誘導
				new EnemyGroup_003()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   4,	// 画面中央に集まって弾を撃つ
				new EnemyGroup_004()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   5,	// Ｓ字で中央で弾を撃つ
				new EnemyGroup_005()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   6,	// ジグザクに移動する
				new EnemyGroup_006()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   7,	// 画面中央にアルファフェードで出現し弾を撃つか特攻
				new EnemyGroup_007()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   8,	// 画面外周を∪字に移動
				new EnemyGroup_008()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{   9,	// 画面左右に出現して左右から圧殺
				new EnemyGroup_009()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{  10,	// 横から出てきてＸ軸が合うとレーザーを撃つか特攻
				new EnemyGroup_010()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{  11,	// 画面に円を描くように出現し弾を撃つか特攻
				new EnemyGroup_011()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{  12,	// 外周を短い∪字移動
				new EnemyGroup_012()
				{
					MinimumLevel = 0, EncountWeight =  50, IntervalTime = 2
				}
			},
			{  13,	// 逆矢印のような軌跡
				new EnemyGroup_013()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime = 2
				}
			},
			{  14,	// バキュラ
				new EnemyGroup_014()
				{
					MinimumLevel = 0, EncountWeight =  30, IntervalTime = 10, ItemAmount =  1
				}
			},
			{  15,	// 画面中央にアルファフェードで黒い表示状態でランダムに移動する
				new EnemyGroup_015()
				{
					MinimumLevel = 0, EncountWeight =  80, IntervalTime =  7
				}
			},
			{  16,	// アステロイド
				new EnemyGroup_016()
				{
					MinimumLevel = 0, EncountWeight =  30, IntervalTime = 10, ItemAmount =  0
				}
			},
			{  17,	// Ｘランダム位置に大量出現し直進して途中で弾を撃つ
				new EnemyGroup_017()
				{
					MinimumLevel = 0, EncountWeight =  90, IntervalTime =  4, ItemAmount =  0
				}
			},
			{  18,	// ベヘリット:画面中央で回転しながら全方向に弾を撃つ
				new EnemyGroup_018()
				{
					MinimumLevel = 0, EncountWeight =  40, IntervalTime = 10, ItemAmount =  2
				}
			},
			{  19,	// 画面左右にジグザク移動で最後に戻る
				new EnemyGroup_019()
				{
					MinimumLevel = 0, EncountWeight =  90, IntervalTime =  4, ItemAmount =  0
				}
			},
			{  20,	// 中型
				new EnemyGroup_020()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime =  5, ItemAmount =  1
				}
			},
			{  21,	// ロボ
				new EnemyGroup_021()
				{
					MinimumLevel = 0, EncountWeight =  50, IntervalTime =  5, ItemAmount =  1
				}
			},
			{  22,	// [ボス]左右に無限大の軌道でレーザー攻撃をしてくる
				new EnemyGroup_022()
				{
					MinimumLevel = 0, EncountWeight =  40, IntervalTime =  0, ItemAmount =  3
				}
			},
			{  23,	// 前後の左右から２次関数の動きで頂点で弾撃ち(1・3・ホーミング)
				new EnemyGroup_023()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime =  2, ItemAmount =  0
				}
			},
			{  24,	// 上下の左右から出現し高速に画面反対側への斜め移動を繰り返す
				new EnemyGroup_024()
				{
					MinimumLevel = 0, EncountWeight =  80, IntervalTime =  2, ItemAmount =  0
				}
			},
			{  25,	// 対角線に放物線移動
				new EnemyGroup_025()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime =  2, ItemAmount =  0
				}
			},
			{  26,	// 機雷(爆発時に弾を撒き散らす)
				new EnemyGroup_026()
				{
					MinimumLevel = 0, EncountWeight =  70, IntervalTime =  2, ItemAmount =  0
				}
			},
			{  27,	// 画面中央にブロックが集まってその後プレイヤーに突っ込んでくる
				new EnemyGroup_027()
				{
					MinimumLevel = 0, EncountWeight =  70, IntervalTime = 12, ItemAmount =  0
				}
			},
			{  28,	// 移動砲台
				new EnemyGroup_028()
				{
					MinimumLevel = 0, EncountWeight =  80, IntervalTime =  5, ItemAmount =  1
				}
			},
			{  29,	// 画面上部下部の位置にランダム出現し前進しつつプレイヤーにＸ軸を合わせてレーザーを撃つ
				new EnemyGroup_029()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime =  4, ItemAmount =  0
				}
			},
			{  30,	// 左右にホッピングしながら画面中央に近づき出てきた方向に帰る
				new EnemyGroup_030()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime =  4, ItemAmount =  0
				}
			},
			{  31,	// [ボス]エルメス
				new EnemyGroup_031()
				{
					MinimumLevel = 0, EncountWeight =  20, IntervalTime =  0, ItemAmount =  4
				}
			},
			{  32,	// 円運動
				new EnemyGroup_032()
				{
					MinimumLevel = 0, EncountWeight = 100, IntervalTime =  5, ItemAmount =  0
				}
			},
			{  33,	// [ボス]ボーナスアイテム
				new EnemyGroup_033()
				{
					MinimumLevel = 0, EncountWeight =  20, IntervalTime =  0, ItemAmount = -1
				}
			},
			{  34,	// アステムボックス(ザコ)
				new EnemyGroup_034()
				{
					MinimumLevel = 0, EncountWeight =  70, IntervalTime =  8, ItemAmount = -1
				}
			},

			{ 999,	// [ボーナス]隠れキャラ
				new EnemyGroup_999()
				{
					MinimumLevel = 0, EncountWeight =  20, IntervalTime =  1, ItemAmount = 0
				}
			},
		} ;
	}
}
