using Godot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;


namespace Sample_001
{
	/// <summary>
	/// ＨＵＤ管理クラス
	/// </summary>
	public partial class HUD : CanvasLayer
	{
		[Export]
		private Label				_ScoreValue ;

		[Export]
		private Label				_HiScoreValue ;

		[Export]
		private Label				_CrashRateValue ;

		[Export]
		private Label				_HitRateValue ;

		//---------------
		// タイトル関係

		[Export]
		private CanvasLayer			_LayerTitle ;

		[Export]
		private Label				_Version ;

		//---------------
		// コンバット関係

		[Export]
		private CanvasLayer			_LayerCombat ;

		[Export]
		private Label				_Pause ;

		//-----
		// パワースピード

		[Export]
		private Control				_ShotSpeedBase ;

		[Export]
		private TextureRect			_ShotSpeed ;

		//-----
		// シールド

		[Export]
		private Control				_ShieldBase ;

		[Export]
		private Control				_ShieldFrame ;

		[Export]
		private TextureRect[]		_ShiledGauges = new TextureRect[ 10 ] ;

		[Export]
		private TextureRect			_ShieldDuration ;

		//-----
		// ボム

		[Export]
		private Control				_BombBase ;

		[Export]
		private Control				_BombFrame ;

		[Export]
		private TextureRect			_BombName ;

		[Export]
		private TextureRect[]		_BombStocks = new TextureRect[ 10 ] ;

		[Export]
		private TextureRect			_BombCursor ;

		[Export]
		private TextureRect			_BombCooldown ;


		[Export]
		private Texture2D[]			_BombNames = new Texture2D[ 3 ] ;

		[Export]
		private Texture2D[]			_BombIcons = new Texture2D[ 3 ] ;

		//-----
		// ボーナス

		[Export]
		private Control				_BonusBase ;

		[Export]
		private Label				_Bonus ;

		[Export]
		private AnimationPlayer		_BonusAnimationPlayer ;

		//---------------
		// デフィート関係

		[Export]
		private CanvasLayer			_LayerDefeat ;

		[Export]
		private AnimationPlayer		_DefeatAnimationPlayer ;

		[Export]
		private Label				_SurvivalTime ;

		[Export]
		private Control				_AnnounceBase ;

		[Export]
		private AnimationPlayer		_AnnounceAnimation ;

		[Export]
		private Node2D				_Flash ;

		[Export]
		private AnimatedSprite2D	_FlashAnimation ;

		//-------------------------------------------------------------------------------------------

		private int				m_Score_New ;
		private int				m_Score_Old ;

		private int				m_HiScore_New ;
		private int				m_HiScore_Old ;

		private double			m_BonusDuration ;
		private double			m_BonusProgress ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			_LayerTitle.Hide() ;
			_LayerTitle.SetProcess( false ) ;

			_LayerCombat.Hide() ;
			_LayerCombat.SetProcess( false ) ;

			_LayerDefeat.Hide() ;
			_LayerDefeat.SetProcess( false ) ;

			//----------------------------------

			_BonusAnimationPlayer.AnimationFinished += OnBonusAnimationFinished ;
		}


		//-----------------------------------------------------------

		/// <summary>
		/// タイトル画面の表示
		/// </summary>
		public void ShowLayerTitle( string version )
		{
			_Version.Text = $"Version {version}" ;

			_LayerTitle.Show() ;
			_LayerTitle.SetProcess( true ) ;
		}

		/// <summary>
		/// タイトル画面の隠蔽
		/// </summary>
		public void HideLayerTitle()
		{
			_LayerTitle.SetProcess( false ) ;
			_LayerTitle.Hide() ;
		}

		/// <summary>
		/// バトル画面の表示
		/// </summary>
		public void ShowLayerCombat()
		{
			_Pause.Hide() ;

			_ShieldBase.Hide() ;
			_BombBase.Hide() ;

			_BonusBase.Hide() ;
			m_BonusDuration = 0 ;

			_LayerCombat.Show() ;
			_LayerCombat.SetProcess( true ) ;
		}

		/// <summary>
		/// バトル画面の隠蔽
		/// </summary>
		public void HideLayerCombat()
		{
			_LayerCombat.SetProcess( false ) ;
			_LayerCombat.Hide() ;
		}

		/// <summary>
		/// ゲームオーバー画面の表示
		/// </summary>
		public void ShowLayerDefeat()
		{
			_LayerDefeat.Show() ;
			_LayerDefeat.SetProcess( true ) ;

			_DefeatAnimationPlayer.Play( "FadeIn" ) ;

			HideCongratulations() ;
		}

		/// <summary>
		/// ゲームオーバー画面の隠蔽
		/// </summary>
		public void HideLayerDefeat()
		{
			HideCongratulations() ;

			_LayerDefeat.SetProcess( false ) ;
			_LayerDefeat.Hide() ;
		}

		/// <summary>
		/// 生存時間を設定する
		/// </summary>
		/// <param name="survivalTime"></param>
		public void SetSurvivalTime( float survivalTime )
		{
			int time = ( int )survivalTime ;

			int minute = time / 60 ;
			int second = time % 60 ;

			_SurvivalTime.Text = $"{minute:D2}:{second:D2}" ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ハイスコア更新通知を表示する
		/// </summary>
		public void ShowCongratulations()
		{
			_AnnounceBase.Show() ;
			_AnnounceAnimation.Play( "FadeIn" ) ;

			_Flash.Hide() ;
			_FlashAnimation.Stop() ;
		}

		/// <summary>
		/// ハイスコア更新通知を隠蔽する
		/// </summary>
		public void HideCongratulations()
		{
			_AnnounceBase.Hide() ;
			_Flash.Hide() ;
		}

		// フラッシュアニメーションが停止していたら再生する
		private void ProcessCongratulations()
		{
			if( _FlashAnimation.IsPlaying() == false )
			{
				PlayFalsh() ;
			}
		}

		// フラッシュアニメーションを再生する
		private void PlayFalsh()
		{
			float x = ExMath.GetRandomRange( 0, _AnnounceBase.Size.X ) ;
			float y = ExMath.GetRandomRange( 0, _AnnounceBase.Size.Y ) ;

			_Flash.Visible = true ;
			_Flash.Position = new Vector2( x, y ) ;
			_FlashAnimation.Play() ;
		}


		//-----------------------------------------------------------

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			delta = ApplicationManager.MasterTimeDelta ;

			CountUpScoreValue( delta ) ;
			CountUpHiScoreValue( delta ) ;

			if( _LayerDefeat.Visible == true && _AnnounceBase.Visible == true )
			{
				ProcessCongratulations() ;
			}

			//----------------------------------

			if( m_BonusDuration >  0 )
			{
				// ボーナス表示中
				m_BonusProgress += delta ;

				if( m_BonusProgress >= m_BonusDuration )
				{
					// ボーナスの消去
					m_BonusDuration = 0 ;

					_BonusAnimationPlayer.Play( "Out" ) ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ポーズの表示を設定する
		/// </summary>
		/// <param name="isPaused"></param>
		public void SetPause( bool isPaused )
		{
			_Pause.Visible = isPaused ;
		}

		/// <summary>
		/// スコアを更新する
		/// </summary>
		/// <param name="score"></param>
		public void SetScoreValue( int score, bool isDelay )
		{
			if( isDelay == false )
			{
				_ScoreValue.Text = score.ToString() ;

				m_Score_New = score ;
				m_Score_Old = score ;
			}
			else
			{
				m_Score_New = score ;
			}
		}

		// スコアをカウントアップさせる
		private void CountUpScoreValue( double delta )
		{
			if( m_Score_Old <  m_Score_New )
			{
				int scoreDelta = m_Score_New - m_Score_Old ;

				int gainScore ;
				if( scoreDelta <= 1000 )
				{
					// 差分が１０００点未満なら１秒間に５００点(１００点は０．２秒)
					gainScore = ( int )( 500 * delta ) ;
				}
				else
				{
					// それ以上であれば差分の１／２とする
					gainScore = ( int )( ( scoreDelta * 0.5 ) * delta ) ;
				}

				m_Score_Old += gainScore ;
				if( m_Score_Old >  m_Score_New )
				{
					m_Score_Old  = m_Score_New ;
				}

				_ScoreValue.Text = m_Score_Old.ToString() ;
			}
		}

		/// <summary>
		/// ハイスコアを更新する
		/// </summary>
		/// <param name="score"></param>
		public void SetHiScoreValue( int hiScore, bool isDelay )
		{
			if( isDelay == false )
			{
				_HiScoreValue.Text = hiScore.ToString() ;

				m_HiScore_New = hiScore ;
				m_HiScore_Old = hiScore ;
			}
			else
			{
				m_HiScore_New = hiScore ;
			}
		}

		// ハイスコアをカウントアップさせる
		private void CountUpHiScoreValue( double _ )
		{
			if( m_Score_Old <  m_HiScore_Old || m_Score_New >  m_HiScore_New )
			{
				// まだカウントアップはさせない
				return ;
			}

			m_HiScore_Old = m_Score_Old ;

			_HiScoreValue.Text = m_HiScore_Old.ToString() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 撃破率を更新する
		/// </summary>
		/// <param name="hitCount"></param>
		/// <param name="popCount"></param>
		public void SetCrashRateValue( int crashCount, int crashMaxCount )
		{
			if( crashMaxCount <= 0 )
			{
				_CrashRateValue.Text = "000.00%" ;
				return ;
			}

			int crashRate = ( int )( ( ( double )crashCount / ( double )crashMaxCount ) * 10000.0 ) ;

			int integerValue = crashRate / 100 ;
			int decimalValue = crashRate % 100 ;

			_CrashRateValue.Text = $"{integerValue}.{decimalValue:D2}%" ; 
		}

		/// <summary>
		/// 命中率を更新する
		/// </summary>
		/// <param name="hitCount"></param>
		/// <param name="popCount"></param>
		public void SetHitRateValue( int hitCount, int hitMaxCount )
		{
			if( hitMaxCount <= 0 )
			{
				_HitRateValue.Text = "000.00%" ;
				return ;
			}

			int hitRate = ( int )( ( ( double )hitCount / ( double )hitMaxCount ) * 10000.0 ) ;

			int integerValue = hitRate / 100 ;
			int decimalValue = hitRate % 100 ;

			_HitRateValue.Text = $"{integerValue}.{decimalValue:D2}%" ; 
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ショットスピードの表示を行う
		/// </summary>
		/// <param name="power"></param>
		public void SetShotSpeed( bool visible, float speedRate )
		{
			if( visible == false )
			{
				_ShotSpeedBase.Visible = false ;
			}
			else
			{
				_ShotSpeedBase.Visible = true ;

				if( speedRate == 0 )
				{
					_ShotSpeed.Visible = false ;
				}
				else
				{
					_ShotSpeed.Visible = true ;
					_ShotSpeed.Scale = new Vector2( speedRate, 1.0f ) ;
				}
			}
		}

		//-----------------------------------

		/// <summary>
		/// シールドケージの表示を行う
		/// </summary>
		/// <param name="shield"></param>
		public void SetShieldPod( int shieldPod, float shieldActive )
		{
			if( shieldPod <= 0 && shieldActive == 0 )
			{
				// 隠蔽
				_ShieldBase.Visible = false ;
			}
			else
			{
				// 表示
				_ShieldBase.Visible = true ;

				int i, l = shieldPod ;

				int m =  _ShiledGauges.Length ;
				if( l >  m )
				{
					l  = m ;
				}

				for( i  = 0 ; i <   l ; i ++ )
				{
					_ShiledGauges[ i ].Visible = true ;
				}
				for( ; i <  m ; i ++ )
				{
					_ShiledGauges[ i ].Visible = false ;
				}

				//---------------------------------------------------------

				SetShieldActive( shieldActive ) ;
			}
		}

		/// <summary>
		/// シールドケージの表示を行う
		/// </summary>
		/// <param name="shield"></param>
		public void SetShieldActive( float shieldActive )
		{
			if( shieldActive == 0 )
			{
//				_ShieldFrame.Modulate = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
				_ShieldDuration.Visible = false ;
			}
			else
			{
//				_ShieldFrame.Modulate = new Color( 1.0f, 1.0f, 1.0f, 0.5f ) ;
				_ShieldDuration.Visible = true ;
				_ShieldDuration.Scale = new Vector2( shieldActive, 1.0f ) ;
			}
		}

		//---------------

		/// <summary>
		/// ボムストックの表示を行う
		/// </summary>
		/// <param name="shield"></param>
		public void SetBombStock( List<BombTypes> bombStocks, int bombCursor, float bombCooldown )
		{
			if( bombStocks.Count <= 0 && bombCooldown == 0 )
			{
				// 隠蔽
				_BombBase.Visible = false ;
			}
			else
			{
				// 表示
				_BombBase.Visible = true ;

				int i, l = bombStocks.Count ;

				int m =  _BombStocks.Length ;
				if( l >  m )
				{
					l  = m ;
				}

				for( i  = 0 ; i <   l ; i ++ )
				{
					_BombStocks[ i ].Visible = true ;

					var bomStock = bombStocks[ i ] ;

					_BombStocks[ i ].Texture = _BombIcons[ ( int )bomStock ] ;
					
					if( i == bombCursor )
					{
						_BombCursor.Position = _BombStocks[ i ].Position ;
						_BombCursor.Visible = true ;
					}
				}
				for( ; i <  m ; i ++ )
				{
					_BombStocks[ i ].Visible = false ;
				}

				//---------------------------------

				if( bombStocks.Count >  0 )
				{
					// ボムの名前
					_BombName.Visible = true ;
					_BombName.Texture = _BombNames[ ( int )bombStocks[ bombCursor ]  ] ;
				}
				else
				{
					_BombName.Visible = false ;
					_BombCursor.Visible = false ;
				}

				//---------------------------------

				SetBombCooldown( bombCooldown ) ;
			}
		}

		/// <summary>
		/// ボムストックの表示を行う
		/// </summary>
		/// <param name="shield"></param>
		public void SetBombCooldown( float bombCooldown )
		{
			if( bombCooldown == 0 )
			{
				_BombFrame.Modulate = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
				_BombCooldown.Visible = false ;
			}
			else
			{
				_BombFrame.Modulate = new Color( 1.0f, 1.0f, 1.0f, 0.5f ) ;
				_BombCooldown.Visible = true ;
				_BombCooldown.Scale = new Vector2( bombCooldown, 1.0f ) ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// ボーナスを表示する
		/// </summary>
		/// <param name="bonus"></param>
		public void ShowBonus( int bonus )
		{
			// ボーナスのＳＥを再生する
			SE.Play( SE.Bonus ) ;

			_Bonus.Text = bonus.ToString() ;

			_BonusBase.Show() ;
			_BonusAnimationPlayer.Play( "In" ) ;

			// ３秒表示
			m_BonusDuration = 3 ;
			m_BonusProgress = 0 ;
		}

		// Out アニメーションが終わった後に呼ばれるコールバック
		private void OnBonusAnimationFinished( StringName animationName )
		{
			if( animationName == "Out" )
			{
				_BonusBase.Hide() ;
			}
		}
	}
}
