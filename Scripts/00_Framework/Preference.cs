using Godot ;
using ExGodot ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;
using System.Text ;
using System.Text.Json.Serialization ;

using StorageHelper ;
using JsonHelper ;


namespace Sample_001
{
	/// <summary>
	/// ブリファレンスのファサードクラス(シングルトン)
	/// </summary>
	public partial class Preference : ExNode
	{
		//-------------------------------------------------------------------------------------------
		// 定義しておきたいキー名の一覧

		/// <summary>
		/// 定義されたキー名の一覧
		/// </summary>
		public static class Keys
		{
			/// <summary>
			/// マスターデータの解凍ハッシュ値
			/// </summary>
			public const string MasterDataHash	= "MasterDataHash" ;
		}

		//-------------------------------------------------------------------------------------------

		// インスタンス(シングルトン)
		private static Preference m_Instance ;

		/// <summary>
		/// インスタンス(シングルトン)
		/// </summary>
		public static Preference Instance	=> m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static Preference Create( Node parent )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			var node = new Preference()
			{
				Name = "Preference"
			} ;

			if( parent == null )
			{
				node.GetTree().Root.AddChild( node ) ;
			}
			else
			{
				parent.AddChild( node ) ;
			}

			return node ;
		}

		/// <summary>
		/// マネージャを破棄する
		/// </summary>
		public static void Delete()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.QueueFree() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インスタンスが生成された際に呼び出される(Awake)
		/// </summary>
		public override void _Ready()
		{
			if( m_Instance == null )
			{
				m_Instance = this ;

				// ここで初期化の処理が行える
				Initialize() ;
			}
		}

		/// <summary>
		/// インスタンスがツリーから除外される際に呼び出される
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree() ;

			if( m_Instance == this )
			{
				// ここで後始末の処理が行える
				Terminate() ;

				m_Instance  = null ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 毎フレーム呼び出される(Update)
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ){}

		//-------------------------------------------------------------------------------------------

		// [JsonIgnore] アトリビュートでメンバーをシリアライズ対象から除外できる

		/// <summary>
		/// プリファレンス情報
		/// </summary>
		public class PreferencePack
		{
			public class Record
			{
				public string Key { get ; set ; }
				public string Value { get ; set ; }
			}

			public DateTime LastUpdate { get ; set ; }
			public List<Record> Records { get ; set ; }
		}

		private Dictionary<string,string>	m_Records ;

		//-----------------------------------

		private const string m_FolderPath	= "System" ;
		private const string m_FileName		= "Preference.bin" ;

		// 初期化する
		private void Initialize()
		{
			if( StorageManager.Exists( m_FolderPath ) != StorageManager.TargetTypes.Folder )
			{
				StorageManager.CreateFolder( m_FolderPath ) ;
			}

			//----------------------------------

			// 最初に展開を行う
			Load_Private() ;
		}


		private void Terminate()
		{
			// 最後に保存を行う
			Save_Private() ;

			m_Records = null ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 明示的なロードを実行する
		/// </summary>
		public static void Load()
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			m_Instance.Load_Private() ;
		}

		// ロードする
		private void Load_Private()
		{
			m_Records = new Dictionary<string,string>() ;

			//----------------------------------

			string path = GetPreferencePath() ;

			if( StorageManager.Exists( path ) == StorageManager.TargetTypes.File )
			{
				// ファイルが存在する

				// ストレージからロードする
				var data = StorageManager.Load( path ) ;
				if( data != null && data.Length >  0 )
				{
					( var key, var vector ) = GetCryptoKeyAndVector() ;
					if( key != null && vector != null )
					{
						data = Security.Decrypt( data, key, vector ) ;
					}

					var text = Encoding.UTF8.GetString( data ) ;

					// デシリアライズする
					var pack = JsonUtility.FromJson<PreferencePack>( text ) ;
					if( pack != null && pack.Records != null && pack.Records.Count >  0 )
					{
						// ハッシュに展開する
						foreach( var record in pack.Records )
						{
							if( m_Records.ContainsKey( record.Key ) == false )
							{
								m_Records.Add( record.Key, record.Value ) ;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 明示的なセーブを実行する
		/// </summary>
		public static void Save()
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			m_Instance.Save_Private() ;
		}

		// セーブする
		private void Save_Private()
		{
			var pack = new PreferencePack()
			{
				LastUpdate	= DateTime.Now,
				Records		= new List<PreferencePack.Record>()
			} ;

			// 値を格納する
			foreach( var record in m_Records )
			{
				pack.Records.Add( new PreferencePack.Record()
				{
					Key		= record.Key,
					Value	= record.Value
				} ) ;
			}

			//----------------------------------

			// シリアライズする
			string text = JsonUtility.ToJson( pack ) ;
			if( string.IsNullOrEmpty( text ) == false )
			{
				text = text.Replace( "\x0D\x0A", "\x0A" ) ;	// 念のため改行を LF に統一する

				byte[] data = Encoding.UTF8.GetBytes( text ) ;

				// ストレージにセーブする
				( var key, var vector ) = GetCryptoKeyAndVector() ;
				if( key != null && vector != null )
				{
					data = Security.Encrypt( data, key, vector ) ;
				}

				// ストレージにセーブする
				StorageManager.Save( GetPreferencePath(), data ) ;
			}
		}

		// パスを取得する
		private static string GetPreferencePath()
		{
			string path = m_FileName ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				path = Security.GetHash( path ) ;
			}

			path = m_FolderPath + "/" + path ;

			return path ;
		}

		// 暗号化のキーとベクターを取得する
		private static ( byte[], byte[] ) GetCryptoKeyAndVector()
		{
			byte[] key		= null ;
			byte[] vector	= null ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				key		= Encoding.UTF8.GetBytes( Define.CryptoKey ) ;
				vector	= Encoding.UTF8.GetBytes( Define.CryptoVector ) ;
			}

			return ( key, vector ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 値を設定する
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <exception cref="Exception"></exception>
		public static void SetValue<T>( string key, T value )
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			m_Instance.SetValue_Private( key, value ) ;
		}

		// 値を設定する
		private void SetValue_Private<T>( string key, T value )
		{
			if( m_Records.ContainsKey( key ) == false )
			{
				m_Records.Add( key, value.ToString() ) ;
			}
			else
			{
				m_Records[ key ] = value.ToString() ;
			}
		}

		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static T GetValue<T>( string key, T defaultValue = default )
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			return m_Instance.GetValue_Private<T>( key, defaultValue ) ;
		}

		// 値を取得する
		private T GetValue_Private<T>( string key, T defaultValue )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				throw new Exception( "Key is empty." ) ;
			}

			if( m_Records.ContainsKey( key ) == false )
			{
				return defaultValue ;
			}

			string value = m_Records[ key ] ;

			var type = typeof( T ) ;

			if( type.IsEnum == false )
			{
				if( type == typeof( bool ) )
				{
					if( string.IsNullOrEmpty( value ) == true )
					{
						return ( T )( object )false ;
					}
					else
					{
						value = value.ToLower() ;
						if( value == "0" || value == "false" )
						{
							return ( T )( object )false ;
						}
						else
						{
							return ( T )( object )true ;
						}
					}
				}
				else
				if( type == typeof( byte ) )
				{
					if( byte.TryParse( value, out var byteValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )byteValue ;
				}
				else
				if( type == typeof( sbyte ) )
				{
					if( sbyte.TryParse( value, out var sbyteValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )sbyteValue ;
				}
				else
				if( type == typeof( char ) )
				{
					if( char.TryParse( value, out var charValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )charValue ;
				}
				else
				if( type == typeof( short ) )
				{
					if( short.TryParse( value, out var shortValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )shortValue ;
				}
				else
				if( type == typeof( ushort ) )
				{
					if( ushort.TryParse( value, out var ushortValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )ushortValue ;
				}
				else
				if( type == typeof( int ) )
				{
					if( int.TryParse( value, out var intValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )intValue ;
				}
				else
				if( type == typeof( uint ) )
				{
					if( uint.TryParse( value, out var uintValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )uintValue ;
				}
				else
				if( type == typeof( long ) )
				{
					if( long.TryParse( value, out var longValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )longValue ;
				}
				else
				if( type == typeof( ulong ) )
				{
					if( ulong.TryParse( value, out var ulongValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )ulongValue ;
				}
				else
				if( type == typeof( float ) )
				{
					if( float.TryParse( value, out var floatValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )floatValue ;
				}
				else
				if( type == typeof( double ) )
				{
					if( double.TryParse( value, out var doubleValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )doubleValue ;
				}
				else
				if( type == typeof( decimal ) )
				{
					if( decimal.TryParse( value, out var decimalValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )decimalValue ;
				}
				else
				if( type == typeof( string ) )
				{
					return ( T )( object )value ;
				}
				else
				if( type == typeof( DateTime ) )
				{
					if( DateTime.TryParse( value, out var dateTimeValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )dateTimeValue ;
				}
				else
				if( type == typeof( DateTimeOffset ) )
				{
					if( DateTimeOffset.TryParse( value, out var dateTimeOffsetValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )dateTimeOffsetValue ;
				}
				else
				if( type == typeof( TimeSpan ) )
				{
					if( TimeSpan.TryParse( value, out var timeSpanValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )timeSpanValue ;
				}
				else
				if( type == typeof( Guid ) )
				{
					if( Guid.TryParse( value, out var guidValue ) == false )
					{
						return defaultValue ;
					}
					return ( T )( object )guidValue ;
				}
			}
			else
			{
				// 列挙子
				if( Enum.TryParse( type, value, out var enumValue ) == true )
				{
					if( Enum.IsDefined( type, enumValue ) == true )
					{
						return ( T )( object )enumValue ;
					}
				}
			}

			throw new Exception( "Unknown type." ) ;
		}

		/// <summary>
		/// 指定したキーの値が存在するかどうか
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static bool HasKey( string key )
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			return m_Instance.HasKey_Private( key ) ;
		}

		// 指定したキーの値が存在するかどうか
		private bool HasKey_Private( string key )
		{
			return m_Records.ContainsKey( key ) ;
		}

		/// <summary>
		/// 指定したキーの値を削除する
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static bool Delete( string key )
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			return m_Instance.Delete_Private( key ) ;
		}

		// 指定したキーの値を削除する
		private bool Delete_Private( string key )
		{
			if( m_Records.ContainsKey( key ) == false )
			{
				return false ;
			}

			return m_Records.Remove( key ) ;
		}

		/// <summary>
		/// 全ての値を消去する
		/// </summary>
		public static void Clear()
		{
			if( m_Instance == null )
			{
				throw new Exception( "Preference is not created." ) ;
			}

			m_Instance.Clear_Private() ;
		}

		// 全ての値を消去する
		private void Clear_Private()
		{
			m_Records.Clear() ;
		}
	}
}
