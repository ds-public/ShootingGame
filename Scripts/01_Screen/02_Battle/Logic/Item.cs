using Godot ;
using System ;


namespace Sample_001
{
	/// <summary>
	/// プレイヤーの弾の制御クラス
	/// </summary>
	public partial class Item : CombatEntity
	{
		// 移動速度
		private	float				m_Speed = 200.0f ; 

		//-----------------------------------------------------------

		// パワーアップアイテムの種類
		private ItemShapeTypes				m_ShapeType ;

		/// <summary>
		/// パワーアップアイテムの種類
		/// </summary>
		public	ItemShapeTypes				 ShapeType => m_ShapeType ;

		private Action<Item,Vector2,bool>	m_OnDestroy ;

		
		/// <summary>
		/// ニセアイテムかどうか
		/// </summary>
		public	bool						  IsFake => m_IsFake ;

		// ニセアイテムかどうか
		private bool						m_IsFake ;

		//-----------------------------------

		private bool						m_IsAreaEntered ;
		private Area2D						m_CollisionTargetArea ;

		//-----------------------------------------------------------

		// アニメーション名群
		private static readonly string[] m_AnimationNames =
		{
			"P", "S", "BC", "BD", "BE"
		} ;

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
			ItemShapeTypes shapeType,
			Vector2 position,
			float angle,
			Action<Item,Vector2,bool> onDestroy,
			Battle owner,
			bool isFlip = false
		)
		{
			// 基底クラスの初期化を実行する
			Initialize( owner, isFlip, false ) ;

			//----------------------------------

			// 外観情報保存
			m_ShapeType = shapeType ;

			// 外観とアニメーション設定
			_Sprite.Play( m_AnimationNames[ ( int )shapeType ] ) ;

			//----------------------------------

			Position		= position ;

			m_OnDestroy		= onDestroy ;

			SetAngle( ExMath.GetRotatedVector( new Vector2(  0, -1 ), angle ) ) ;

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

		// 基本移動方向
		private static Vector2 m_BaseDirection = new (  0, +1 ) ;

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
				// エネミーの撃破数カウントのため必要
				m_IsAreaEntered  = false ;
				OnAreaHit( m_CollisionTargetArea ) ;

				return ;
			}

			//----------------------------------

			// ダメージエフェクト処理
//			ProcessDamageEffectColor() ;

			//----------------------------------------------------------

			float x = Position.X ;
			float y = Position.Y ;

			float bx0 = x - 32 ;
			float by0 = y - 32 ;
			float bx1 = x + 32 ;
			float by1 = y + 32 ;

			float sx0 = 0 ;
			float sy0 = 0 ;
			float sx1 = ScreenSize.X ;
			float sy1 = ScreenSize.Y ;

			if( bx1 <  sx0 || by1 <  sy0 || bx0 >  sx1 || by0 >  sy1 )
			{
				this.AreaEntered -= OnAreaEntered ;	// 実際は意味が無い

				m_OnDestroy?.Invoke( this, Position, false ) ;

				return ;
			}

			//----------------------------------

			// 位置を更新する
			Position += ( m_BaseDirection * ( m_Speed * ( float )delta ) ) ;
		}

		//-----------------------------------------------------------

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
			// 何かに当たれば対象を消滅させる
			m_OnDestroy?.Invoke( this, Position, true ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 偽物設定
		/// </summary>
		/// <param name="isFake"></param>
		public void SetFake( bool isFake )
		{
			// 偽物設定
			m_IsFake = isFake ;
		}

		/// <summary>
		/// 方向指定
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public void SetAngle( Vector2 direction )
		{
			direction = direction.Normalized() ;

			Rotation = ExMath.GetRativeAngle( new Vector2(  0, -1 ), direction ) ;
		}
	}
}
