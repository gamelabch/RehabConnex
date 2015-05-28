using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;

using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;

// todo
// thread an check for connection!
// 
// http://msdn.microsoft.com/de-de/library/system.net.sockets.socket.connected.aspx
// ClientSocket.Client.Connect

namespace RehabConnex {

	/// <summary>
	/// Client handler handels the functionality of a connected client
	/// </summary>
	public class ClientHandler {
			
		public ConfigService configServiceObj; //!< Reference to the config service

		public TcpClient ClientSocket; //!< TcpClient socket of the client.
	    public NetworkStream networkStream; //!< networkstream
		bool ContinueProcess = false; //!< as long as this is true the clienthandler handles the clients input
	    public byte[] bytes; //!< Data buffer for incoming data.
	    public StringBuilder sb =  new StringBuilder(); //!< Received data string.
		public string data = null; //!< Incoming data from the client.
		public string stringBuffer = ""; //!< temp buffer for the incoming messages

		public Node clientNode = new Node(); //!< reference to the node of this client
		
		public bool debugClass=false;

		/// <summary>
		/// Initializes a new instance of the <see cref="RehabConnex.ClientHandler"/> class. (constructor)
		/// </summary>
		/// <param name='ClientSocket'>
		/// TcpClient socket of the client.
		/// </param>
		/// <param name='iconfigServiceObj'>
		/// Reference to the config service
		/// </param>
		public ClientHandler (TcpClient ClientSocket, ConfigService iconfigServiceObj ) {
			
			configServiceObj = iconfigServiceObj;
				
			ClientSocket.ReceiveTimeout = 10 ; // 100 miliseconds
			this.ClientSocket = ClientSocket ;
	    	networkStream = ClientSocket.GetStream();
	    	// bytes = new byte[ClientSocket.ReceiveBufferSize];
	    	bytes = new byte[1024];
	    	ContinueProcess = true ;

			Console.WriteLine ("[Connection]: New Client");
				
			// onConnect
			configServiceObj.OnConnect(this);	
				
			// http://msdn.microsoft.com/de-de/library/system.net.sockets.socket.connected.aspx
			// ClientSocket.Client.Connect
				
		}
			
		/// <summary>
		/// Processes this instance and catches communication errors
		/// </summary>
		public void Process () {


			try {
				if (networkStream.CanRead) {
					// http://msdn.microsoft.com/en-us/library/system.net.sockets.tcpclient.getstream.aspx
				
					// version 1.0
					// int BytesRead = networkStream.Read(bytes, 0, (int) bytes.Length);
					int readBytesNumber = (int)ClientSocket.ReceiveBufferSize;
// todo: ??? what ?
					if (readBytesNumber > 1000)
						readBytesNumber = 1000;
				
					int BytesRead = networkStream.Read (bytes, 0, readBytesNumber);
					if (BytesRead > 0) {
					
						// There might be more data, so store the data received so far.
			           
						// version 1
						sb.Append (Encoding.ASCII.GetString (bytes, 0, BytesRead)); 
						// version 2 todo: ??
						// sb.Append (Encoding.UTF8.GetString (bytes));
					
						stringBuffer = stringBuffer + sb.ToString ();
						sb.Length = 0;
				
						ProcessDataReceived ();
					} else {
						// ProcessDataReceived();
					}  
				} else {
					// close 
					// Console.WriteLine ("CLOSE1");
					Console.WriteLine ("[Connection]: Closed Connection Type: 1");

				}
			} catch (IOException io) { 
				// TODO: ERRORS!
				// All the data has arrived; put it in response.
				// Console.WriteLine("IOException.an error"+io);
				// ProcessDataReceived( sb.ToString()  ) ;
				ProcessDataReceived ();
				// Console.WriteLine ("error in ioexception io");
				// configServiceObj.OnDisconnect(this,clientNode);

			} catch (SocketException ex) {
				Console.WriteLine("msg error Connection is broken. " + ex);
				Console.WriteLine ("[Connection]: Closed Connection");

				//disconnecting
				configServiceObj.OnDisconnect(this, clientNode);	
				networkStream.Close();
				ClientSocket.Close();			
				ContinueProcess = false;
			}  

			//check if client is still connected
			if (!this.IsConnected(ClientSocket.Client)) {
				
				// Console.WriteLine("msg error Client dropped the connection.");
				Console.WriteLine ("[Connection]: Closed Connection Type: 2)");

				//disconnecting
				configServiceObj.OnDisconnect(this, clientNode);	
				networkStream.Close();
				ClientSocket.Close();			
				ContinueProcess = false;
			}				
		}  // Process()

		/// <summary>
		/// Processes the data received form the client
		/// </summary>
		private void ProcessDataReceived(  ) {
			string strLine="";
				
			// go on as fast as you can
			// check for return 
			// cuta
			// Console.WriteLine (stringBuffer);
			// check for return
			int pos=-1;
			do {
				pos=stringBuffer.IndexOf("\n");
				if (pos!=-1) {
					// +1
					strLine=stringBuffer.Substring (0,pos);
					stringBuffer=stringBuffer.Substring (pos+1);
					// All the data has arrived; put it in response.
				    // Console.WriteLine ("stringbuffer: "+stringBuffer);
					if (strLine.Equals("")) return;

	//todo			   Console.WriteLine ("INPUT: "+strLine);
				 	// Clear buffer

					// Console.WriteLine( "DEBUG-INPUT:") ;
					// Console.WriteLine(strLine) ;

					// StringBuilder response = new StringBuilder(  ) ;
					// response.Append( "Received at " ) ;
					// response.Append( DateTime.Now.ToString() ) ;
					// response.Append( "\r\n" ) ;
					// response.Append( data ) ;
							
					// configServiceObj
						
					// use divers 
					// build it
					// split here
					// try {

					// set path value
					// get path 

					string[] strArr = null;
					char[] splitchar = { ' ' };
				    strArr = strLine.Split(splitchar);

					// Console
					if (strArr.Length>2) {
							
						// Console.WriteLine("cmd({0}) cmdId{1} cmdPath({2}) cmdArgument({3})", strArr[0] , strArr[1]  , strArr[2] , strArr[3]) ;	 	
						// Console.WriteLine ("processConfigMessage");
						// configServiceObj.ProcessConfigMessage(ClientSocket, strArr[0] , strArr[1]  , strArr[2] , strArr[3]);
							
						ConfigMessage cfm= new ConfigMessage();
						cfm.command=strArr[0];
						cfm.externalId=strArr[1];
						cfm.path=strArr[2];
						// fix problem
						cfm.path=cfm.path.Replace("\n","");	
						cfm.path=cfm.path.Replace("\r","");	
							
						// argument
						string arg="";
						if (strArr.Length>3) {
							arg=strArr[3];
						}
						// fix \n etc 
						arg=arg.Replace("\n","");	
						arg=arg.Replace("\r","");	
						if (debugClass) Console.WriteLine ("arg: "+arg);
						cfm.argument=arg; 
							
						// change ...
						cfm.argument=cfm.GetUnpackedConfigMessage(cfm.argument);
								
						Monitor.Enter (this);
						configServiceObj.ProcessConfigMessage(this, cfm);
						Monitor.Exit (this);
					} else {
						// Console.WriteLine ("mal formulated command {0} ",data);			
						// error
						// send back an error here ...
						ConfigMessage cfmReply= new ConfigMessage();
						cfmReply.command="reply";
						cfmReply.externalId="job";
						cfmReply.path="error";
						cfmReply.argument="wrong input\nuse correct command: [cmd] [cmdid] [cmdpath] [cmdargument] \nget job1 help \nset job2 root.server.name server5\ninsertdeviceinputslot job15 glovea";
						
						Console.WriteLine ("Problem (not recognised command): " + strLine);
							
						// todo - here?
						Monitor.Enter (this);
						// send answer 
						configServiceObj.SendConfigMessage(this, cfmReply );
							
						Monitor.Exit (this);
					}
				} else {
					return;		
				}
			} 
			while(pos!=-1);		
						
			// get all

	    	// Echo the data back to the client.
	        // byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
	        // networkStream.Write(sendBytes, 0, sendBytes.Length);

	    	// Client stop processing  
	       	/*
			if ( bQuit  )  {
				networkStream.Close() ;
				ClientSocket.Close();	
				ContinueProcess = false ; 
			}
			*/
		}        
	        
		/// <summary>
		/// Terminates the connection to the coresponding client.
		/// </summary>
		public void Close() {
		    networkStream.Close() ;
		    ClientSocket.Close();       
			Console.WriteLine ("CLOSED!!!!");
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="RehabConnex.ClientHandler"/> is alive.
		/// </summary>
		/// <value>
		/// <c>true</c> if alive; otherwise, <c>false</c>.
		/// </value>
		public bool Alive {
		    get {
		    	return ContinueProcess;
		    }
		}
	    
		/// <summary>
		/// Determines whether this instance is connected the specified socket.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is connected to the specified socket; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='inSocket'>
		/// The socket to test.
		/// </param>
		public bool IsConnected(Socket inSocket) {
			try {
				return !(inSocket.Poll(1, SelectMode.SelectRead) && inSocket.Available == 0);
			}
			catch (SocketException) { return false; }
		}

	} // class ClientHandler 
}

