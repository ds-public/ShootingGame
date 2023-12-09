using Godot ;
using System ;

namespace Sample_001
{
	/// <summary>
	/// プレイヤーのオプションの制御クラス
	/// </summary>
	public partial class PlayerOption : CombatUnit
	{
		// 方向
		private Vector2						m_Direction ;

		/// <summary>
		/// 速度
		/// </summary>
		public	float						  Speed => m_Speed ;
		
		// 速度
		private float						m_Speed ;

		//-----------------------------------

		/// <summary>
		/// 与えるダメージ
		/// </summary>
		public	int								  Damage => m_Damage ;

		// ダメージ
		private int								m_Damage ;


		private Action<PlayerOption,Vector2>	m_OnDestroy ;

		//-----------------------------------

		private bool							m_IsAreaEntered ;
		private Area2D							m_CollisionTargetArea ;

		//-----------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready(){}

		/// <summary>
		/// 動作を開始させる
		/// </summary>
		/// <param name="position"></param>
		public void Start
		(
			Vector2 position,
			Vector2 direction,
			float speed,
			int damage,
			Action<PlayerOption,Vector2> onDestroy,
			Battle owner,
			bool isFlip = false
		)
		{
			// 基底クラスを初期化する
			Initialize( owner, isFlip, false, false ) ;

			//----------------------------------

			Position	= position ;

			m_Direction = direction ;

			// 方向に応じて角度をつける
			Rotation	= ExMath.GetRativeAngle( new Vector2(  0, -1 ), direction ) ;

			// 速度
			m_Speed		= speed ;

			//--------------

			// エネミーが接触した際に与えるダメージを保存する
			m_Damage	= damage ;

			//----------------------------------

			m_OnDestroy = onDestroy ;

			// プレイヤーより少し奥に表示
			ZIndex		= -2 ;

			//----------------------------------

			// コリジョンの接触コールバック設定
			this.AreaEntered += OnAreaEntered ;
			m_IsAreaEntered = false ;

			//----------------------------------

			Visible = true ;
			SetProcess( true ) ;
		}

		/// <summary>
		/// 終了
		/// </summary>
		public void End()
		{
			Visible = false ;
			SetProcess( false ) ;

			//----------------------------------

			// コリジョンヒットのコールバックを解除する
			this.AreaEntered -= OnAreaEntered ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			if( m_Owner != null && m_Owner.IsPausing == true )
			{
				// ポーズ中は操作できない
				return ;
			}

			//----------------------------------------------------------

			// カスタムシェーダーにモジュレートカラーを設定する
//			SetModulateColor() ;

			//----------------------------------

			if( m_IsAreaEntered == true )
			{
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------

			// ダメージエフェクト処理
//			ProcessDamageEffectColor() ;

			//----------------------------------------------------------

			if( m_Owner.IsPlayerDestroyed == false )
			{
				if( m_Owner.PlayerOptions.Count >  0 )
				{
					// 自身が何番目のオプションが確認する

					int i, l = m_Owner.PlayerOptions.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_Owner.PlayerOptions[ i ] == this )
						{
							break ;
						}
					}

					if( i <  l )
					{
						// 基本的に該当しないという事はありえない

						Vector2 basePosition ;

						if( i  == 0 )
						{
							// 最初のオプションなので基準位置はプレイヤー
							basePosition = m_Owner.Player.Position ;
						}
						else
						{
							// ２つ目以降のオプションなので基準位置は１つ前のオプション
							basePosition = m_Owner.PlayerOptions[ i - 1 ].Position ;
						}

						var velocity = basePosition - Position ;

						float distance = velocity.Length() ;
						if( distance >  48.0f )
						{
							// プレイヤーから一定距離離れていたらプレイヤーの方に近づく

							float min =  24.0f ;
							float max = 480.0f ;

							if( distance <   min )
							{
								// 最小速度
								velocity = velocity.Normalized() * min ;
							}
							else
							if( distance >  max )
							{
								// 最大速度
								velocity = velocity.Normalized() * max ;
							}

							Position += ( velocity * ( float )delta * 2.8f ) ;
						}
					}
				}
			}
		}

		//-----------------------------------

		// コリジョンに接触した際に呼び出される
		// 注意：コリジョン接触中のコールバック内で新規の Area2D 追加等を行うとエラーとなるため
		// 　　　コールバック発生直後の _Process で処理を実行する
		private void OnAreaEntered( Area2D area )
		{
			m_IsAreaEntered = true ;
			m_CollisionTargetArea = area ;
		}

		// 実際のコリジョンヒット処理(_Processから実行する)
		private void OnAreaHit( Area2D area )
		{
			// 何かに当たれば消える
			m_OnDestroy?.Invoke( this, Position ) ;
		}

		/// <summary>
		/// 自爆
		/// </summary>
		public void SelfDestroy()
		{
			m_OnDestroy?.Invoke( this, Position ) ;
		}
	}
}
