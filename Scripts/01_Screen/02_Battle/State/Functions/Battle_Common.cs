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
		//-------------------------------------------------------------------------------------------

		private const string m_HiScore_PreferenceKey = "HiScore" ;

		// ハイスコアをプリファレンスからロードする
		private void LoadHiScore()
		{
			m_HiScore = Preference.GetValue<int>( m_HiScore_PreferenceKey ) ;

			if( m_HiScore <  100000 )
			{
				m_HiScore  = 100000 ;	// 最低１０万
			}
		}

		// ハイスコアをプリファレンスにセーブする
		private void SaveHiScore()
		{
			Preference.SetValue( m_HiScore_PreferenceKey, m_HiScore ) ;
			Preference.Save() ;
		}

		//-------------------------------------------------------------------------------------------

		// 残ったエンティティを全て強制的に破棄する
		private void DestroyAllEntities()
		{
			if( m_Entities.Count == 0 )
			{
				// 残ったエンティティは存在しない
				return ;
			}

			foreach( var entity in m_Entities )
			{
				entity.QueueFree() ;
			}

			m_Entities.Clear() ;
		}

		//------------------------------------------------------------------------------------------^

		// スコアを加算する
		private void AddScore( int score )
		{
			if( m_IsNoDeathSuccessful == true )
			{
				// 無敵状態ではスコアは加算されない
				return ;
			}

			//----------------------------------

			// スコア更新
			m_Score += score ;

			if( m_Score >  9999999 )
			{
				// カンスト
				m_Score  = 9999999 ;
			}

			_HUD.SetScoreValue( m_Score, true ) ;

			if( m_Score >  m_HiScore )
			{
				// ハイスコア更新
				m_HiScore = m_Score ;
				_HUD.SetHiScoreValue( m_HiScore, true ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// バックグラウンドの表示位置を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetBackgroundPositionX( float x )
		{
			_Background.SetPositionX( x ) ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 無敵状態を解除して自爆する
		/// </summary>
		public void ClearNoDeathSuccessful()
		{
			if( m_IsNoDeathSuccessful == true )
			{
				m_IsNoDeathSuccessful  = false ;

				OnPlayerDamage( _Player.Position, 10000 ) ;
			}
		}


		//-------------------------------------------------------------------------------------------

	}
}
