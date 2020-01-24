using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Text;
using UnityEngine.Networking;
namespace UnityTools.EditorTools {

	public sealed class GistExporter : EditorWindow
	{   
        const string USER = "tiredamage42";

		public sealed class ResultData
		{
			public string html_url = string.Empty;
		}

		static Vector2 SIZE = new Vector2( 380, 182 );

		string m_path, m_password, m_description, m_filename, response;
		bool m_isPublic;
        int phase;

		[MenuItem( "Assets/Export Gist", true )]
		static bool CanOpen() {
			if (Selection.objects.Length != 1)
			    return false;
			return Selection.activeObject is TextAsset || Selection.activeObject is MonoScript;
		}

		[MenuItem( "Assets/Export Gist", priority = 99999 )]
		static void Open () {
			var win = GetWindow<GistExporter>( true, "Export Gist" );
			win.Init( Selection.activeObject );
		}

		void Init ( UnityEngine.Object target ) {
			m_path = AssetDatabase.GetAssetPath( target );
			m_filename = Path.GetFileName( m_path );
			minSize = SIZE;
			maxSize	= SIZE;
			position.Set( position.x, position.y, SIZE.x, SIZE.y );
			m_description = null;
            phase = 0;
            response = null;
		}

		void OnGUI() {
			m_password = EditorGUILayout.TextField( "Password", m_password );
			m_description = EditorGUILayout.TextField( "Description", m_description );
			m_isPublic = EditorGUILayout.Toggle( "Public", m_isPublic );
			m_filename = EditorGUILayout.TextField( "Filename", m_filename );

			GUI.enabled = !string.IsNullOrEmpty( m_password ) && !string.IsNullOrEmpty( m_filename );
			if ( GUILayout.Button( "Export Gist" ) )
				Post();
			GUI.enabled = true;

			EditorGUILayout.Space();
            if (phase == 2)
				EditorGUILayout.LabelField( "Done!" );
			else if ( phase == -1 )
				EditorGUILayout.HelpBox( response, MessageType.Error );
			else if ( phase == 1 )
				EditorGUILayout.LabelField( "Posting..." );
		}

		void Post() {
            phase = 1;
            response = null;
			if ( !File.Exists( m_path ) ) {
                phase = -1;
				response = "File Not Found " + m_path;
				return;
			}
			var sr = new StreamReader( m_path );
			var content = sr.ReadToEnd();
			sr.Close();

			var coroutine = Create (m_password, m_description, m_isPublic, m_filename, content,
				onComplete : json => {
                    phase = 2;
					response = (JsonUtility.FromJson<ResultData>( json ) ?? new ResultData()).html_url;
					Repaint();
                    Application.OpenURL( response );
				},
				onError : error => {
					phase = -1;
					response = error;
					Repaint();
				}
			);
			EditorCoroutine.Start( coroutine );
		}

		static IEnumerator Create(string password, string description, bool isPublic, string filename, string content, Action<string> onComplete = null, Action<string> onError = null) {
			var enc = Encoding.UTF8;
			var data = enc.GetBytes( CreateDataRaw( description, isPublic, filename, content ) );
			var headers = new Dictionary<string, string> {
				{ "Authorization", string.Format( "Basic {0}", Convert.ToBase64String( enc.GetBytes( string.Format( "{0}:{1}", USER, password ) ) ) )	},
				{ "User-Agent", "GistSharp"	},
			};
			using (var www = new WWW( "https://api.github.com/gists", data, headers )) {
                yield return www;
                if ( !string.IsNullOrEmpty( www.error ) ) {
                    if ( onError != null )
                        onError( www.error );
                    yield break;
                }
                if ( onComplete != null )
                    onComplete( www.text );
            }
		}
		static string CreateDataRaw (string description, bool isPublic, string filename, string content) {
			var dataRaw = @"{""description"":""" + description + @""","
				+ @"""public"":" + isPublic.ToString().ToLower() + ","
				+ @"""files"":{""" + filename + @""":{"
				+ @"""content"":""" + Escape( content ) + @"""}}}";
			return dataRaw;
		}
		static string Escape( string value ) {
			return value.Replace( @"\", @"\\" ).Replace( @"""", @"\""" ).Replace( "\t", @"\t" ).Replace( @"
", @"\n" );
		}
		
	}

}