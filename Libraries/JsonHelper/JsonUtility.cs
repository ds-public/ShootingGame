using Godot ;
using System ;
using System.IO ;
using System.Text.Json ;
using System.Text.Json.Serialization ;
using System.Text.Encodings.Web ;


namespace JsonHelper
{
	/// <summary>
	/// ＪＳＯＮのヘルパー(ラッパー)クラス Version 2023/11/09
	/// </summary>
	public partial class JsonUtility
	{
		/// <summary>
		/// シリアライズ(テキスト化)を行う
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="isNotMultiByteEncoding"></param>
		/// <param name="isCamelCaseName"></param>
		/// <param name="isIndentEnabled"></param>
		/// <returns></returns>
		public static string ToJson<T>( T data, bool isNotMultiByteEncoding = false, bool isCamelCaseName = false, bool isIndentEnabled = false ) where T : class
		{
			// シリアライズする
			var options = new JsonSerializerOptions() ;

			if( isNotMultiByteEncoding == true )
			{
				// マルチバイトコードをエスケープしないようにする
				options.Encoder					= JavaScriptEncoder.UnsafeRelaxedJsonEscaping ;
			}

            if( isCamelCaseName == true )
            {
				// ラベル名をアッパーキャメルにする
                options.PropertyNamingPolicy	= JsonNamingPolicy.CamelCase ;
            }

			if( isIndentEnabled == true )
			{
				// 改行とインデント有効化
				options.WriteIndented			= true ;
			}

			return JsonSerializer.Serialize( data, options ) ;
		}

		//-----------------------------------------------------------

		// デシリアライズのデフォルトオプション
		private static JsonSerializerOptions m_DefaultDeserializeOption = new JsonSerializerOptions
		{
			Encoder					= JavaScriptEncoder.UnsafeRelaxedJsonEscaping,	// マルチバイト文字のエスケープを許可
			ReadCommentHandling		= JsonCommentHandling.Skip,						// コメントを許可
			AllowTrailingCommas		= true											// 末尾のカンマを許可
		} ;


		/// <summary>
		/// デシリアライズ(バイナリ化)を行う
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		public static T FromJson<T>( string text ) where T : class
			=> JsonSerializer.Deserialize<T>( text, m_DefaultDeserializeOption ) ;

		/// <summary>
		/// デシリアライズ(バイナリ化)を行う
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		public static T FromJson<T>( ReadOnlySpan<byte> text ) where T : class
			=> JsonSerializer.Deserialize<T>( text, m_DefaultDeserializeOption ) ;

		/// <summary>
		/// デシリアライズ(バイナリ化)を行う
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		public static T FromJson<T>( ReadOnlySpan<char> text ) where T : class
			=> JsonSerializer.Deserialize<T>( text, m_DefaultDeserializeOption ) ;

		/// <summary>
		/// デシリアライズ(バイナリ化)を行う
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		public static T FromJson<T>( Stream text ) where T : class
			=> JsonSerializer.Deserialize<T>( text, m_DefaultDeserializeOption ) ;
	}
}

