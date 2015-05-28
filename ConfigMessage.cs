using System;
using System.Net;
using System.Web;

// used for communication
// input/output 
// exp output: out set    job2 root.server.name  complex+server 	
// exp input:  in  return job2 ok                [answer]			
// exp input:  in  return job2 error             [answer]

// exp input-client-command [befehl]: do set .....

namespace RehabConnex {

	/// <summary>
	/// Config message structure and de- and encode functions for the packing of a message
	/// </summary>
	public class ConfigMessage {
		bool incoming = false; //!< <c>true</c> if the message is incomming

		// * not used 
		public string type="in"; //!< message type can be in/out

		public string command=""; //!< message command
		public string externalId=""; //!< id of the job
		public string path=""; //!< path of the config message
		public string argument=""; //!< argument of the config message

		public string appendix=""; //!< used for done jobs ....

		/// <summary>
		/// Initializes a new instance of the <see cref="RehabConnex.ConfigMessage"/> class. (Constructor)
		/// </summary>
		public ConfigMessage() {

		}

		/// <summary>
		/// Packs the config message.
		/// </summary>
		/// <returns>
		/// The packed config message.
		/// </returns>
		public string GetPackedConfigMessage() {
			string packedMessage="";
			// pack argument twice (could be urlencoded yet!)
			packedMessage = command+" "+externalId+" "+path+" "+UrlEncodeAlternative(argument);
			packedMessage = UrlEncodeAlternative(packedMessage);

			return packedMessage;
		}

		/// <summary>
		/// Gets the unpacked config message.
		/// </summary>
		/// <returns>
		/// The unpacked config message.
		/// </returns>
		/// <param name='msg'>
		/// The config message to unpack
		/// </param>
		public string GetUnpackedConfigMessage( string msg ) {
			return UrlDecodeAlternative(msg);
		}
			
			
		// todo: compiler could not find urlencode in system.web.HttpUtility 
					
		/// <summary>
		/// Converts the config message like URLEncode
		/// </summary>
		/// <returns>
		/// A <c>string</c> with the encoded message
		/// </returns>
		/// <param name='toconvert'>
		/// The config message to encode.
		/// </param>
		public string UrlEncodeAlternative( string toconvert ) {
			string stringconverted=""+toconvert;
			stringconverted=stringconverted.Replace ("+","{plus}");
			stringconverted=stringconverted.Replace (" ","+");
			stringconverted=stringconverted.Replace ("\n","%13");
			
			return stringconverted;
		}
						
		/// <summary>
		/// Converts the config message like URLDecode
		/// </summary>
		/// <returns>
		/// A <c>string</c> with the decoded message
		/// </returns>
		/// <param name='toconvert'>
		/// The config message to decode
		/// </param>
		public string UrlDecodeAlternative( string toconvert ) {
			string stringconverted=""+toconvert;
			stringconverted=stringconverted.Replace ("+"," ");
			stringconverted=stringconverted.Replace ("%13","\n");
			stringconverted=stringconverted.Replace ("{plus}","+");
			
			return stringconverted;
		}
		
		// path#hash:client
		// (used in push/pull)
		public string GetPushPullPath( ) {
			
			int posHash=path.IndexOf ("#");
			
			if 	(posHash!=-1) {
			
				return path.Substring (0,posHash);	
			}
			
			return null;
		}
		
		public string GetPushPullHash( ) {
			
			int posHash=path.IndexOf ("#");
			int posClient=path.IndexOf (":");
			
			if (posHash!=-1) {
				
				if (posClient!=-1) {
					
					if (posHash<posClient) {
						
						return path.Substring (posHash+1,posClient-posHash-1);	
					}
				}
				else {
					
					return path.Substring (posHash+1);
					
				}
				
			}
			
			return null;
		}
		
		public string GetPushPullClient( ) {
			
			int posHash=path.IndexOf ("#");
			int posClient=path.IndexOf (":");
			
			if (
				(posHash!=-1)
				&&
				(posClient!=-1)
			   ) {
			
				if (posHash<posClient)
				{
					return path.Substring (posClient+1);	
				}				
			}
			
			return null;
		}
		
		
			
	}
}

