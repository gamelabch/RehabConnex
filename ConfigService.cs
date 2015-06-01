
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;

using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;

/*
 * 
 * push slotpath#heise 4444
 * pull slotpath#heise 
 * 
 * 
 * SPECIALSLOT: IN
 * SLOT TYPE: input.hash
 * PATCH - special-attribute hash
 * 
 * SPECIALSLOT: OUT
 * SLOT TYPE: output.hash
 * 
 * 
 * */

namespace RehabConnex {

	/// <summary>
	/// Config service handels all messages between the tcp clients
	/// </summary>
	public class ConfigService {

		bool debugClass = false; //!< enables debug output
		float version = 0.87f; //!< version of the rehabconnex server
		public int port = 42000; //!< default server port, if no port is set in the config file

		// Structure
		Structure structureServiceObj = new Structure(); //!< reference to the structure of the connected clients

		// structurejob id
		int structureJobId = 1; //!< unique number of the job

		// add job done 
		// set job1 root.server.name 10 > reply job1 ok set+job1+root.server.name
		bool addToReplyJobDone=false;

		// all cmds only for ownclient
		// exceptions: get, patch
		bool accessControl=false; 

		// url
		string rehabconnexClaim="Combine any reha- or game software and any interfaces. You decide!";
		string rehabconnexProjectURL="http://rehabconnex.zhdk.ch";

		// receipt * say ok etc ...
		// in stream no quittierung

		/// <summary>
		/// Initializes a new instance of the <see cref="RehabConnex.ConfigService"/> class. (Constructor)
		/// </summary>
		public ConfigService () {

			// Start
			structureServiceObj.Start (version);

		}

		/// <summary>
		/// Starts the <see cref="RehabConnex.ConfigService"/>, enables tcp listening and sets server-ip and port
		/// </summary>
		public void Start() {
			// client tasks
			ClientService ClientTask  ;

			// Client Connections Pool
			ClientConnectionPool ConnectionPool = new ClientConnectionPool()  ;          

			// Client Task to handle client requests
			ClientTask = new ClientService(ConnectionPool) ;
			ClientTask.Start() ;

			// StructureService
			TcpListener listener = new TcpListener(port);
			try {
				listener.Start();

				int TestingCycle = 3 ; // Number of testing cycles
				int ClientNbr = 0 ;

				// Start listening for connections.
				Console.WriteLine("\n\n\n\n\n\n\n\n");
				Console.WriteLine("--------------------------------------------------------------");
				Console.WriteLine("REHABConnexServer");
				Console.WriteLine("--------------------------------------------------------------");
				Console.WriteLine(""+rehabconnexClaim);
				Console.WriteLine("Project-URL: "+rehabconnexProjectURL);

				// load config
				//LoadConfigFile(); //obsolete
				Console.WriteLine ("Version: "+version);

				// config here 
				Console.WriteLine ("\nPreferences");
				Console.WriteLine ("- AccessControl (accesscontrol): "+accessControl);
				Console.WriteLine ("- Debug (debugstructure): "+debugClass);

				Console.WriteLine ("\nWaiting for tcp-ip-config-connection");

				string firstIP="";

				// http://stackoverflow.com/questions/1069103/how-to-get-my-own-ip-address-in-c
				IPHostEntry host;
				string localIP = "?";
				host = Dns.GetHostEntry(Dns.GetHostName());
				foreach (IPAddress ip in host.AddressList) {
					if (ip.AddressFamily == AddressFamily.InterNetwork) {
						localIP = ip.ToString();
						if (firstIP=="") firstIP=localIP;
						Console.WriteLine ("IP: "+ip.ToString()+"");
					}
				}
				Console.WriteLine ("on Port: "+port+"...");

				// add startupitems
				ConfigMessage confMsg;
				// Console.WriteLine ("---- config: patch.auto COUNT: "+startupItems .Count);	

				for (int z=0;z<startupItems .Count;z++) {
					confMsg=(ConfigMessage)startupItems[z];

					// add patches here ...
					if (confMsg.command.Equals ("patch.auto")) { 
						// get autopatch object ...
						Node nodeAutopatchTmp=structureServiceObj.SearchNodeByPathRecursive(structureServiceObj.tree,"root.server.patchesauto");
						if (nodeAutopatchTmp!=null) {

							// Console.WriteLine ("---- config: patch.auto "+confMsg.path+"  "+confMsg.argument);	

							// add here and now ...
							Node nodeSlot=structureServiceObj.AddNodeByValuesTo(""+confMsg.path,"patch.auto","","patch.auto",""+confMsg.argument,nodeAutopatchTmp);
							nodeSlot.name=confMsg.path;
							nodeSlot.argument=confMsg.argument;
						}
					}
				}

				Console.WriteLine ("\nCommand-Structure: cmd cmdId cmdPath|cmdId cmdArgument");
				// this > device ! ... 
				Console.WriteLine ("\nImportant commands: \n\n  add/remove\n  complex-adds: insertdeviceinputslot/insertdeviceoutputslot/insertdevicepushpullslot \n  complex-adds: adddevice/insertinputslotat/insertslotat/addslotsets/addslotset/addslots/addslot\n  set/get\n  set/get[/.id/.name/.nodetype/.argument/.owner/.path/.length/.objects_x/.clientfor/.devicefor] \n  push/pull\n  patch/patch.auto (use patch will be converted to patch.pushpull)\n\n  get jx clients/devices/slotsets/slots/slots.input/slots.output/slots.pushpull/patches/structure");
				Console.WriteLine ("\nDont forget to rename your client: set.name jx this 'clientname' ");

				Console.WriteLine ("\nTest: nc "+firstIP+" "+port+" \n");

				// start ... 
				while ( true ) { 
					TcpClient handler = listener.AcceptTcpClient();

					if ( handler != null )  {
						// Console.WriteLine("\nClient#{0} accepted!\n", ++ClientNbr) ;

						// An incoming connection needs to be processed.
						ClientHandler newClientHandler = new ClientHandler(handler,this);
						// structureServiceObj
						ConnectionPool.Enqueue( newClientHandler ) ;
					}
				}

			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}

			Console.WriteLine("\nHit enter to continue...");
			Console.Read();	
		}

		/// <summary>
		/// Loads the config file. (config.txt, situated in the same folder as the executable)
		/// </summary>
		/// 
		ArrayList startupItems=new ArrayList();

		public void LoadConfigFile(  ) {
			// Load Config File
			string exeFile = (new System.Uri(Assembly.GetEntryAssembly().CodeBase)).AbsolutePath;
			string exeDir = Path.GetDirectoryName(exeFile);	
			// Console.WriteLine("Path: "+exeDir);
			string exeDirPath= exeDir+"/config.txt";

			// check if existing ..
			bool existsF=File.Exists(exeDirPath);

			if (existsF) {
				// load it now and here
				StreamReader fileObj = File.OpenText(exeDirPath);
				while (!fileObj.EndOfStream)
				{
					String line = fileObj.ReadLine();
					// Console.WriteLine("line: "+line);

					// # comments?
					if (line.Length>0) {
						if (line.Substring (0,1).Equals("#")) {
							// comment	
						}
						else {
							// split by space
							string[] strArr = null;
							char[] splitchar = { ' ' };
							strArr = line.Split(splitchar);
							// Console
							if (strArr.Length>1) {
								string cmd=strArr[0];
								string cmdValue=strArr[1];
								/*if (cmd.Equals("port"))
								{
									// value?
									rehabConnexClientObject.ip=cmdValue;
								}
								*/	

								if (cmd.Equals("name")) {
									// value?
									// name
									Node serverObj=structureServiceObj.SearchNodeByPath("root.server");
									if (serverObj!=null) {
										serverObj.argument=""+cmdValue;
									}
									// todo: do this with a special 
									// root client!
								}

								if (cmd.Equals("port")) {
									// value?
									port=int.Parse (cmdValue);
								}

								if (cmd.Equals("responseaddjob")) {
									//  value?
									// port=int.Parse (cmdValue);
									if (cmdValue.Equals ("true")) {
										addToReplyJobDone=true;
									}

									if (cmdValue.Equals ("false")) {
										addToReplyJobDone=false;
									}
								}

								if (cmd.Equals("accesscontrol")) {
									//  value?
									// port=int.Parse (cmdValue);
									if (cmdValue.Equals ("true")) {
										accessControl=true;
									}

									if (cmdValue.Equals ("false")) {
										accessControl=false;
									}
								}



								if (cmd.Equals("debugstructure")) {
									//  value?
									// port=int.Parse (cmdValue);
									if (cmdValue.Equals ("true")) {
										debugClass=true;
									}

									if (cmdValue.Equals ("false")) {
										debugClass=false;
									}
								}


								// add patches aut
								// patch.auto pathA pathTarget
								if (cmd.Equals("patch.auto")) {
									// patch.auto x y z
									if (strArr.Length>2)
									{
										Console.WriteLine("\nConfig: "+strArr[1]+" "+strArr[2]);

										// add to startup
										ConfigMessage newMsg=new ConfigMessage();
										newMsg.command="patch.auto";
										newMsg.path=""+strArr[1];
										newMsg.argument=""+strArr[2];
										startupItems.Add (newMsg);

									}					

								}


							}




						}
					}	
				}

			} else {
				// no config file
				Console.WriteLine("ConfigFile: Config.txt not found");	
			}
		}

		/// <summary>
		/// Handels the connection event, adds the new client to the <see cref="RehabConnex.Structure"/>
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// Client handler object (TcpClient clientSocket)
		/// </param>
		public void OnConnect( ClientHandler clientHandlerObj ) {
			bool debugThis=false;

			if (debugClass) debugThis=true;

			// say hello
			// byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
			//	networkStream.Write(sendBytes, 0, sendBytes.Length);

			ConfigMessage cfM=new ConfigMessage();
			cfM.command="message";
			cfM.path="";
			cfM.argument="Welcome to RehabConnex ("+version+") ["+rehabconnexProjectURL+"] {"+rehabconnexClaim+"";
			SendConfigMessage(clientHandlerObj, cfM);

			// SendMessage(clientSocket,"\nset 1 server.version "+version);
			// SendMessage(clientSocket,"\n");

			// get clients
			// todo: store it in clients
			// server.devices
			Node clientObjects=structureServiceObj.SearchNodeByPath("root.server.clients");
			if (debugThis) Console.WriteLine("root.server.clients ---"+clientObjects);
			if (clientObjects!=null) {

				/*
				 *   Connection > add device here ... 
				 * 
				 * */
				// add a client
				Node nodeClient=structureServiceObj.AddNodeByValuesTo("client","","","client","",clientObjects);
				nodeClient.name=nodeClient.name+nodeClient.id;
				Socket s = clientHandlerObj.ClientSocket.Client;
				Node nodeClientIP=structureServiceObj.AddNodeByValuesTo("ip","","","string","",nodeClient);
				nodeClientIP.argument=""+((IPEndPoint)s.RemoteEndPoint).Address.ToString();
				nodeClientIP.attribute=true;
				nodeClient.argumentTypedObject=clientHandlerObj;
				Node nodeClientPort=structureServiceObj.AddNodeByValuesTo("port","","","string","",nodeClient);
				nodeClientPort.argument=""+((IPEndPoint)s.RemoteEndPoint).Port.ToString();
				nodeClientPort.attribute=true;
				Node nodeFunctions=structureServiceObj.AddNodeByValuesTo("category","","","string","",nodeClient);
				nodeFunctions.argument="sensorsystem"; // comma seperated if there is more than one function
				// softwaresystem/sensorsystem/configsystem/monitorsystem/logsystem/storagesystem
				Node nodeFunctionsSub=structureServiceObj.AddNodeByValuesTo("categorysub","","","string","",nodeClient);
				nodeFunctionsSub.argument="";
				// possible values  (comma seperated)
				//  softwaresystem: game|training
				//  sensorsystem: input|output|inputoutput
				//  configsystem: config|calibration
				//  monitorsystem: monitoring
				// 


				// nodeClientPort.attribute=true;
				clientHandlerObj.clientNode=nodeClient; // =nodeClient;
				// if (debugThis) Console.WriteLine ("clientNode: "+clientHandlerObj.clientNode.name);

				/*
				 *  Add a devices 
				 */
				// devices node
				Node nodeDevices=structureServiceObj.AddNodeByValuesTo("devices","","","list","",nodeClient);

				// add a slotsets
				/*	Node nodeSlotSet=structureServiceObj.AddNodeByValuesTo("slotset","","","slotset","",nodeSlotSets);
						 nodeSlotSet.name=nodeSlotSet.name+nodeFirstDevice.id;
				
						// nodeslot
						Node nodeSlots=structureServiceObj.AddNodeByValuesTo("slots","","","slots","",nodeSlotSet);
							
							// slots
							Node nodeSlot=structureServiceObj.AddNodeByValuesTo("slot","","","slot","",nodeSlots);
								nodeSlot.name=nodeSlotSet.name+nodeFirstDevice.id;
								nodeSlot.name="fingerleft";
								Node nodeSlotPort=structureServiceObj.AddNodeByValuesTo("port","","","port","",nodeSlot);
									 nodeSlotPort.attribute=true; 
								Node nodeSlotInput=structureServiceObj.AddNodeByValuesTo("slottype","","","input","",nodeSlot);
									 nodeSlotInput.attribute=true; 
								Node nodeSlotCommunitcation=structureServiceObj.AddNodeByValuesTo("communictiontype","","","udp","",nodeSlot);
									 nodeSlotCommunitcation.attribute=true; 
									// nodeSlotPort.name=nodeSlotSet.name+nodeFirstDevice.id;
				
				// monitor devices				
				Node nodeMonitors=structureServiceObj.AddNodeByValuesTo("monitors","","","list","",clientObjects);
					 Node nodeMonitorThis=structureServiceObj.AddNodeByValuesTo("monitor","","","everything","id",nodeMonitors);
				*/				

			}

			// found?
			if (debugClass) {

				string strDebug=structureServiceObj.DebugStructure();
				Console.WriteLine("  "+strDebug);
			}

			// give back some info ...
		}

		/// <summary>
		/// Sends the configuration message to the specified client
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// The client that gets the config message
		/// </param>
		/// <param name='cfm'>
		/// Config message, that is sent to the client
		/// </param>
		public void SendConfigMessage(ClientHandler clientHandlerObj, ConfigMessage cfm ) {
			string msg="";

			// send direct ...
			msg=""+cfm.command+" "+cfm.externalId+" "+cfm.path+" "+cfm.UrlEncodeAlternative(cfm.argument);
			SendMessage(clientHandlerObj, msg);
		}

		/// <summary>
		/// Sends the reply of a configuration message
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// Target client to send the message to.
		/// </param>
		/// <param name='cfm'>
		/// The reply configuration message
		/// </param>
		/// <param name='cfmOriginal'>
		/// The original configuration message
		/// </param>
		public void SendReturnConfigMessage(ClientHandler clientHandlerObj, ConfigMessage cfm, ConfigMessage cfmOriginal ) {
			string msg="";

			msg="reply "+cfmOriginal.externalId+" "+cfm.path+" "+cfm.UrlEncodeAlternative(cfm.argument)+" ";

			// add to reply jobdone
			if (addToReplyJobDone) msg=msg+cfmOriginal.GetPackedConfigMessage()+"";
			else msg=msg+"-";

			// Console.WriteLine (msg);
			SendMessage(clientHandlerObj, msg);
		}

		// org
		/*		public void SendMessage(ClientHandler clientHandlerObj, string msg)
				{
					byte[] sendBytes = Encoding.ASCII.GetBytes(msg.ToString());
        	        clientHandlerObj.ClientSocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
			
				}
		*/

		/// <summary>
		/// Sends the message as bytecode to the specified client
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// The tcp client to send the message to.
		/// </param>
		/// <param name='msg'>
		/// The message that is sent
		/// </param>
		public void SendMessage(ClientHandler clientHandlerObj, string msg) {
			byte[] sendBytes = Encoding.ASCII.GetBytes(msg.ToString()+"\n");
			clientHandlerObj.ClientSocket.GetStream().Write(sendBytes, 0, sendBytes.Length);

			//	Byte[] sendBytes = Encoding.UTF8.GetBytes (msg+"\n");
			//	clientHandlerObj.ClientSocket.GetStream().Write (sendBytes, 0, sendBytes.Length);
		}

		/// <summary>
		/// Sends a configuration command to the specified client with a unique jobID
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// The client that gets the config message
		/// </param>
		/// <param name='cfm'>
		/// Config message, that is sent to the client
		/// </param>
		public void SendConfigCommand(ClientHandler clientHandlerObj, ConfigMessage cfm ) {

			SendConfigCommandBase(clientHandlerObj, cfm, true );

		}

		/// <summary>
		/// Sends a configuration command to the specified client with a unique jobID
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// The client that gets the config message
		/// </param>
		/// <param name='cfm'>
		/// Config message, that is sent to the client
		/// </param>
		public void SendConfigCommandBase(ClientHandler clientHandlerObj, ConfigMessage cfm, bool overwriteJobId ) {
			string msg="";
			structureJobId++;
			if (overwriteJobId) cfm.externalId="rehabJobID"+structureJobId;

			// send direct ...
			msg=""+cfm.command+" "+cfm.externalId+" "+cfm.path+" "+cfm.UrlEncodeAlternative(cfm.argument);

			SendMessage(clientHandlerObj, msg);
		}

		// do the message
		/*
		public  void  ProcessConfigMessageRaw(TcpClient clientSocket , string cmd, string cmdExternalId, string cmdPath, string cmdArgument ) {
			Console.WriteLine("processConfigMessage(cmd({0}) cmdId({1}) cmdPath({2}) cmdArgument({3}))",cmd,cmdExternalId,cmdPath,cmdArgument);
			
			ConfigMessage cfm= new ConfigMessage();
			cfm.command=cmd;
			cfm.externalId=cmdExternalId;
			cfm.path=cmdPath;
			cfm.argument=cmdArgument;
			
			ProcessConfigMessage(clientSocket, cfm);
		}
		*/

		// check commands
		public bool checkCommands(ClientHandler clientHandlerObj, ConfigMessage cfm)
		{
			// todo: do the whole check here
			// set.x etc ... 

			if (cfm.command.IndexOf("add")==0) return true; 
			if (cfm.command.IndexOf("insert")==0) return true; 

			if (cfm.command.IndexOf("get")==0) return true; 
			if (cfm.command.IndexOf("set")==0) return true; 
			//todo: stream
			if (cfm.command.IndexOf("stream")==0) return true; 

			if (cfm.command.IndexOf("patch")==0) return true; 

			if (cfm.command.IndexOf("remove")==0) return true; 

			// direct communication
			if (cfm.command.IndexOf("push")==0) return true; 
			if (cfm.command.IndexOf("pull")==0) return true; 

			return false;
		}

		// check access ...
		public bool checkAccess(ClientHandler clientHandlerObj, ConfigMessage cfm)
		{
			bool debugThis=false;

			if (debugThis) Console.WriteLine("--- checkAccess ------------------- ");

			if (accessControl)
			{
				// notdefault: commands don't need path
				if (cfm.command.IndexOf("insert")==0) return true; 
				if (cfm.command.IndexOf("get")==0) return true; 
				if (cfm.command.IndexOf("patch")==0) return true; 

				// direct communication
				if (cfm.command.IndexOf("push")==0) return true; 
				if (cfm.command.IndexOf("pull")==0) return true; 

				// todo: remove patches!!!

				// default: comments with paths
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode, cfm.path);
				if (arrObjects.Count>0)
				{
					Node nodeStart=(Node)arrObjects[arrObjects.Count-1];
					if (debugThis) Console.WriteLine("--- checkAccess.clientfound.start: "+nodeStart.id+" ---["+nodeStart.name+"]{"+nodeStart.nodeType+"}["+nodeStart.argument+"]---------------- ");

					// special case: patches
					// 	everybody can change patch properties!!
					// 	(otherwise it wouldn't be possible to change ...)
					if (nodeStart.nodeType.Equals ("patch"))
					{
						return true;
					}	

					if (nodeStart.parent!=null)
					if (nodeStart.parent.nodeType.Equals ("patch"))
					{
						return true;
					}

					// alias
					if (nodeStart.parent!=null)
					{
						// todo: not so dirty

						if (nodeStart.name.Equals ("alias"))
						{
							return true;
						}
						if (nodeStart.parent.name.Equals ("alias"))
						{
							return true;
						}
						if (nodeStart.parent.argument.Equals ("alias"))
						{
							return true;
						}

					}

					// patches auto
					if (nodeStart.parent!=null)
					if (nodeStart.parent.name.Equals ("patchesauto"))
					{
						return true;
					}

					// todo: security correct
					// add ... 
					if (nodeStart!=null)
					if (nodeStart.name.Equals ("patchesauto"))
					{
						return true;
					}

					// default
					Node clientNode=GetClientObjectNodeRecursive(nodeStart);
					if (clientNode!=null)
					{
						if (debugThis) Console.WriteLine("--- checkAccess.clientfound ------------------- ");

						// check if this is the parent/owner!
						if (clientHandlerObj.clientNode.id==clientNode.id)
						{
							if (debugThis) Console.WriteLine("--- checkAccess.clientfound and parent!!!! ------------------- ");
							return true;	
						}



						/*
							else
							{
								// no owner but a special command 
								// like get/patch ...
								if (debugThis) Console.WriteLine("--- checkAccess.nodirectacces command? "+cfm.command+" ------------------- ");
								if (cfm.command.IndexOf("get")==0) return true;
								if (cfm.command.Equals("patch")) return true;
							}
							*/

						// check if the same
					}

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Processes the recived config message
		/// </summary>
		/// <param name='clientHandlerObj'>
		/// The source client of this message
		/// </param>
		/// <param name='cfm'>
		/// The recived config message
		/// </param>
		public void ProcessConfigMessage(ClientHandler clientHandlerObj, ConfigMessage cfm) {
			bool debugThis=false;

			if (debugClass) debugThis=true;

			// debug specific ..
			bool debugStructureChange=false;
			bool debugStructureChanging=false;
			// Monitor.Enter(this);

			// changes in structure?
			bool flagStructureChange=false;
			// changes in the slots
			bool flagSlotChange = false;

			// convert cfm argument
			cfm.argument=convertConfigMessageToString(cfm.argument);

			// do it here ...
			if (debugThis) Console.WriteLine("--- processConfigMessage ------------------- ");
			// if (debugThis) Console.WriteLine("processConfigMessage( "+clientHandlerObj.clientNode.id+"/"+clientHandlerObj.clientNode.name+"  (cmd({0}) cmdId({1}) cmdPath({2}) cmdArgument({3}))",cfm.command,cfm.externalId,cfm.path,cfm.argument);
			// if (debugThis) 
			if (debugThis) Console.WriteLine(""+clientHandlerObj.clientNode.id+"/"+clientHandlerObj.clientNode.name+"--cmd:{0}--id:{1}--path:{2}--arg:{3}--",cfm.command,cfm.externalId,cfm.path,cfm.argument);

			//todo: adddevice/removedevice
			//todo: addslotset/removeslotset
			//todo: addslot/removeslot

			// command accepted?
			// 
			// access control
			if (!checkCommands(clientHandlerObj,cfm)) {

				ConfigMessage cfmAnswer=new ConfigMessage();
				cfmAnswer.path="error";  
				cfmAnswer.argument="command.not.found"; // ); // +nodeAddObject.id;
				SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );


				if (debugThis) Console.WriteLine("--- checkCommands.NotFound! ------------------- ");

				return;
			}

			// access control
			if (!checkAccess(clientHandlerObj,cfm)) {

				ConfigMessage cfmAnswer=new ConfigMessage();
				cfmAnswer.path="error.noaccess";
				cfmAnswer.argument="no access";
				// send reply
				SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );

				if (debugThis) Console.WriteLine("--- checkAccess.NoAccess ------------------- ");

				return;
			}

			string commandKey=""; 
			string commandInternalKey="";
			string[] arrKCommandKeys = null;
			char[] splitcharKey = { '.' };
			arrKCommandKeys = cfm.command.Split(splitcharKey);
			// internal key add?
			commandKey=arrKCommandKeys[0];
			if (arrKCommandKeys.Length>1) {
				commandInternalKey=arrKCommandKeys[1];
			}	
			if (debugThis) Console.WriteLine ("---------PROCESS-("+commandKey+"/"+commandInternalKey+")--------");

			bool commandFound=false;
			/*
			 * 
			 *   add & remove
			 * 
			 **/
			if (cfm.command.Equals("add")) {
				if (debugThis) Console.WriteLine("processConfigMessage() --- ADD --"+cfm.path+"-----------------");

				flagStructureChange=true;
				commandFound=true;

				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode, cfm.path);

				Node nodObj = null;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++) {
					//fetch object
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

					// add
					Node nodeAddObject=structureServiceObj.AddNodeByValuesTo(""+cfm.argument,"","","","",nodObj);
					nodeAddObject.clientId=clientHandlerObj.clientNode.id;
					// todo: MonitorThisObject(nodeAddObject);  > monitored in structures
					// todo: access

					// reply
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";
					cfmAnswer.argument="objectid:"+nodeAddObject.id;
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm);
					debugStructureChanging=true; // debugStructureChange

				}	
				// reply if nothing could be found!
				if (nodObj.Equals(null)) {
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm);
				}

			} // add 

			// remove
			if (cfm.command.Equals("remove")) {

				commandFound=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- REMOVE --"+cfm.path+"----------------- ");
				flagStructureChange=true;

				if (!cfm.path.Equals ("")) {
					ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode, cfm.path);
					bool accessAllowed=false;
					Node nodObj = null;
					if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
					for (int z=0;z<arrObjects.Count;z++) {
						//cache object
						nodObj=(Node)arrObjects[z];

						if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

						// remove if allowed
						if (nodObj.accessOtherClients || clientHandlerObj.clientNode.id == nodObj.clientId || accessControl==false) {
							accessAllowed = true;
							structureServiceObj.RemoveNode(nodObj);
						}

						// prepare reply
						ConfigMessage cfmAnswer=new ConfigMessage();
						if (accessAllowed) {
							cfmAnswer.path="ok";
							cfmAnswer.argument="removed";
						} else {
							cfmAnswer.path="error";
							cfmAnswer.argument="access not allowed";
						}
						// send reply
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm);
						debugStructureChanging=true; //debugStructureChange
					}

					// reply if nothing could be found!
					if (nodObj.Equals(null)) {
						ConfigMessage cfmAnswerHereTmp=new ConfigMessage();
						cfmAnswerHereTmp.path="error";
						cfmAnswerHereTmp.argument="object not found";
						// send reply
						SendReturnConfigMessage(clientHandlerObj, cfmAnswerHereTmp, cfm);
					}	
				}
			} // remove 			

			/*
			 *  adddevice
			 */
			// add device to the channel
			// usage: adddevice job25 this.devices glove
			if (
				(cfm.command.Equals("adddevice"))
				||
				(cfm.command.Equals("add.device"))
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- ADDDEVICE --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++) {
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeAddObject=structureServiceObj.AddNodeByValuesTo(""+cfm.argument,"","","client",""+cfm.argument,nodObj);
					nodeAddObject.clientId=clientHandlerObj.clientNode.id;

					// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject) {
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // adddevice 

			// todo: remove

			/*
			 *  addslotsets
			 */
			// add slot to the channel
			// usage: addslotsets job114 this.devices.glove
			if (
				(cfm.command.Equals("addslotsets")) 
				||
				(cfm.command.Equals("add.slotsets")) 
			)
			{
				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- ADDSLOTSETS --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++) {
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeAddObject=structureServiceObj.AddNodeByValuesTo("slotsets","","","slotsets","",nodObj);
					nodeAddObject.clientId=clientHandlerObj.clientNode.id;
					// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject) {
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // addslotsets 

			/*
			 *  addslotset
			 */
			// add slot to the channel
			// usage: addslotset job3 this.devices.glove.slotsets training
			if (
				(cfm.command.Equals("addslotset")) 
				||
				(cfm.command.Equals("add.slotset")) 
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- ADDSLOTSET --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++) {
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeAddObject=structureServiceObj.AddNodeByValuesTo(""+cfm.argument,"","","slotset","",nodObj);
					nodeAddObject.clientId=clientHandlerObj.clientNode.id;
					// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject) {
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // addslotset 

			/*
			 *  addslots
			 * 
			 * */
			// add slot to the channel
			// usage: addslotset job3 this.devices.glove.slotsets.training
			if (
				(cfm.command.Equals("addslots"))
				||
				(cfm.command.Equals("add.slots"))
			)
			{
				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- ADDSLOTS --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeAddObject=structureServiceObj.AddNodeByValuesTo("slots","","","slots","",nodObj);
					nodeAddObject.clientId=clientHandlerObj.clientNode.id;
					// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // addslots 


			/*
			 *  addslot
			 * 
			 * */
			// add slot to the channel
			// usage: addslotset job3 this.devices.glove.slotsets.training.slots
			if (
				(cfm.command.Equals("addslot"))
				||
				(cfm.command.Equals("add.slot"))
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- ADDSLOT --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

					// version 1
					Node nodeAddObject=structureServiceObj.AddNodeByValuesTo(""+cfm.argument,"","","slots","",nodObj);
					nodeAddObject.clientId=clientHandlerObj.clientNode.id;

					// version 2
					// add a slot here ...
					// Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodObj, "slot");

					// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument=""+nodeAddObject.id; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange

					Console.WriteLine("[Slot][Add]"+cfm.path);

				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // addslot 	

			/*
			 *  inserts only in this!!!
			 * 
			 * */

			/*
			* 
			* insertdeviceinputslot
			* 
			* */
			// add device to the channel
			// usage: insertdeviceinputslot job16 glove
			// result: this.devices.glove.slotsets.slotset.slots.slot
			if (
				(cfm.command.Equals("insertdeviceinputslot"))
				||
				(cfm.command.Equals("insert.deviceinputslot"))
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- adddeviceinputslot --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,"this.devices"); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeDevice=structureServiceObj.AddNodeByValuesTo(""+cfm.path,"","","device",""+cfm.argument,nodObj);
					nodeDevice.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlotsets=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeDevice);
					nodeSlotsets.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlotset=structureServiceObj.AddNodeByValuesTo("slotset" ,"","","slotset","normalized",nodeSlotsets);
					nodeSlotset.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlots=structureServiceObj.AddNodeByValuesTo("slots","","","list","Slots",nodeSlotset);
					nodeSlots.clientId=clientHandlerObj.clientNode.id;

					Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodeSlots, "slot");

					/*
					Node nodeSlot=structureServiceObj.AddNodeByValuesTo("slot","","","slot","",nodeSlots);
						 nodeSlot.clientId=clientHandlerObj.clientNode.id;
					Node nodeCommunication=structureServiceObj.AddNodeByValuesTo("type","","","communicationtype","input",nodeSlot);
						 nodeCommunication.clientId=clientHandlerObj.clientNode.id;
					Node nodeProtocol=structureServiceObj.AddNodeByValuesTo("protocol","","","communicationprotocol","tcp",nodeSlot);
						 nodeProtocol.clientId=clientHandlerObj.clientNode.id;
					Node nodePort=structureServiceObj.AddNodeByValuesTo("port","","","int","43000",nodeSlot);
						 nodePort.clientId=clientHandlerObj.clientNode.id;
					Node nodeValue=structureServiceObj.AddNodeByValuesTo("value","","","float","0",nodeSlot);
						 nodeValue.clientId=clientHandlerObj.clientNode.id;
					Node nodeStreaming=structureServiceObj.AddNodeByValuesTo("stream","","","boolean","true",nodeSlot);
						 nodeStreaming.clientId=clientHandlerObj.clientNode.id;
					*/

					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets.slotset.slots.slot"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange

					// Console.WriteLine("[Slot]["+cfm.command+"] + "+cfm.path+" ("+cfmAnswer.argument+") {"+nodeSlot.id+"}");

				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // insertdeviceinputslot 	

			/*
			 * 
			 * insertdeviceoutputslot
			 * 
			 * */
			// add device to the channel
			// usage: insertdeviceoutputslot job16 glove
			// result: this.devices.glove.slotsets.slotset.slots.slot
			if (
				(cfm.command.Equals("insertdeviceoutputslot"))
				||
				(cfm.command.Equals("insert.deviceinputslot"))
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- adddeviceinputslot --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,"this.devices"); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeDevice=structureServiceObj.AddNodeByValuesTo(""+cfm.path,"","","device",""+cfm.argument,nodObj);
					nodeDevice.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlotsets=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeDevice);
					nodeSlotsets.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlotset=structureServiceObj.AddNodeByValuesTo("slotset" ,"","","slotset","normalized",nodeSlotsets);
					nodeSlotset.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlots=structureServiceObj.AddNodeByValuesTo("slots","","","list","Slots",nodeSlotset);
					nodeSlots.clientId=clientHandlerObj.clientNode.id;

					Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodeSlots, "slot", "output");

					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets.slotset.slots.slot"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange

					// Console.WriteLine("[Slot]["+cfm.command+"] + "+cfm.path+" ("+cfmAnswer.argument+") {"+nodeSlot.id+"}");

				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // insertdeviceoutputslot 	

			/*
			 * 
			 * insertdevicepushpullslot
			 * 
			 * */
			// add device to the channel
			// usage: insertdevicepushpullslot job16 glove
			// result: this.devices.glove.slotsets.slotset.slots.slot
			if (
				(cfm.command.Equals("insertdevicepushpullslot"))
				||
				(cfm.command.Equals("insert.devicepushpullslot"))
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- adddevicepushpullslot --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,"this.devices"); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);
					Node nodeDevice=structureServiceObj.AddNodeByValuesTo(""+cfm.path,"","","device",""+cfm.argument,nodObj);
					nodeDevice.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlotsets=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeDevice);
					nodeSlotsets.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlotset=structureServiceObj.AddNodeByValuesTo("slotset" ,"","","slotset","normalized",nodeSlotsets);
					nodeSlotset.clientId=clientHandlerObj.clientNode.id;
					Node nodeSlots=structureServiceObj.AddNodeByValuesTo("slots","","","list","Slots",nodeSlotset);
					nodeSlots.clientId=clientHandlerObj.clientNode.id;

					Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodeSlots, "slot", "pushpull");

					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument="this.devices."+cfm.path+".slotsets.slotset.slots.slot"; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange

					// Console.WriteLine("[Slot]["+cfm.command+"] + "+cfm.path+" ("+cfmAnswer.argument+") {"+nodeSlot.id+"}");

				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // insertdevicepushpullslot 	


			/*
			 * 
			 * insertinputslotat
			 * 
			 * */
			// add device to the channel
			// usage: insertinputslot job16 this.devices.locomat.slotsets.slotset.slots newinputname
			// result: this.devices.glove.slotsets.slotset.slots.slot
			if (
				(cfm.command.Equals("insertinputslotat"))
				||
				(cfm.command.Equals("insert.inputslotat"))
			)
			{
				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- adddeviceinputslot --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("-------processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("---------processConfigMessage()  "+z+". objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

					Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodObj, cfm.argument);

					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument=""+cfm.path+"."+cfm.argument; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // insertinputslotat 		


			/*
			 *  addslot
			 * 
			 * */
			// add slot to the channel
			// usage: addslotset job3 this.devices.glove.slotsets.training.slots
			if (
				(cfm.command.Equals("insertslotat"))
				||
				(cfm.command.Equals("insert.slotat"))
			) {

				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- INSERT SLOT AT --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

					// version 1
					// Node nodeAddObject=structureServiceObj.AddNodeByValuesTo(""+cfm.argument,"","","slots","",nodObj);
					//	 nodeAddObject.clientId=clientHandlerObj.clientNode.id;

					// version 2
					// add a slot here ...
					Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodObj, "slot");

					// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument=""+nodeSlot.id; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // addslot 	

			/*
			 * 
			 * insertoutputslotat
			 * 
			 * */
			// add device to the channel
			// usage: insertinputslot job16 this.devices.locomat.slotsets.slotset.slots newinputname
			// result: this.devices.glove.slotsets.slotset.slots.slot
			if (
				(cfm.command.Equals("insertoutputslotat"))
				||
				(cfm.command.Equals("insert.inputslotat"))
			)
			{
				commandFound=true;
				flagStructureChange=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- adddeviceinputslot --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("-------processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("---------processConfigMessage()  "+z+". objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

					Node nodeSlot=structureServiceObj.AddSlot(clientHandlerObj.clientNode, nodObj, cfm.argument);

					// change type to output 
					Node nodeType=nodeSlot.SearchChildByName("type");
					if (nodeType!=null)
					{
						nodeType.argument="output";
					}

					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";  
					cfmAnswer.argument=""+cfm.path+"."+cfm.argument; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					foundObject=true;
					debugStructureChanging=true; // debugStructureChange
				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // insertoutputslotat 

			/*
			 *  patch & unpatch
			 * 
			 * 
			 * */
			/*
			*  patch
			* 
			* */
			// add patch > redirect
			// 
			// usage: patch job3 this.devices.glove.slotsets.training.slots
			if (cfm.command.Equals("patch"))
			{

				commandFound=true;
				if (debugThis) Console.WriteLine("processConfigMessage() --- patch --"+cfm.path+"----------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.path); // "this.devices");
				bool error=false;
				string errorMessage="";
				string valueGet="null";
				bool foundObject=false;
				Node nodObj;
				if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
				for (int z=0;z<arrObjects.Count;z++)
				{
					// patch output to input!
					bool flagPatchOk=true;

					nodObj=(Node)arrObjects[z];

					// check if possible

					// todo: implement here
					/*
					// output
					Node slotType=GetChildByName( nodObj, "type" ); 
					if (slotType!=null) {
						if (slotType.argument.Equals("output")) {	
							// default true 
						}
						else {
							flagPatchOk=false;		
						}
					}
					else {
						flagPatchOk=false;	
					}
					// input or pushpull!
					// cfm.argument;
					String arg=GetChildArgumentByIdAndName(fm.argument);
					if (arg!=null) {
						flagPatchOk=false;

					}
					*/

					if (flagPatchOk)
					{

						if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

						Node nodeAddObject=structureServiceObj.AddPatch( nodObj, cfm.argument );

						ConfigMessage cfmAnswer=new ConfigMessage();
						cfmAnswer.path="ok";  
						cfmAnswer.argument=""+nodeAddObject.id; // ); // +nodeAddObject.id;
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );

						// Console.WriteLine("         processConfigMessage() patching() ");
						// Console.WriteLine ("[Patching] "+nodObj.id+" > "+cfm.argument);

						string value = ""+nodObj.argument;
						// Console.WriteLine ("[Patching] value="+value);
						arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,cfm.argument);
						if (arrObjects.Count>0) {
							Node nodeStart=(Node)arrObjects[arrObjects.Count-1];
							// Console.WriteLine ("[Patching] value="+nodeStart.id);

							// found the object to send
							Node clientNodeStreamObj=GetClientObjectNode(nodeStart);
							// send over the client the message!
							if (clientNodeStreamObj != null) {
								string localPath=GetClientObjectNodePath(nodeStart,"");
								try
								{
									ClientHandler clientHandlerObjHere=(ClientHandler)clientNodeStreamObj.argumentTypedObject;
									// send with this now
									// todo: work with lists etc ..
									ConfigMessage cfmObj=new ConfigMessage();
									cfmObj.command="stream";
									cfmObj.path=localPath; // ".value" / 
									cfmObj.argument=""+value;
									SendConfigCommand(clientHandlerObjHere,cfmObj);
								}
								catch(Exception ex)
								{
									Console.WriteLine (""+ex.ToString());	
								}
							}

						}

						/*
						ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode, cfm.path);
						if (arrObjects.Count>0)
						{
							Node nodeStart=(Node)arrObjects[arrObjects.Count-1];
							if (debugThis) Console.WriteLine("--- checkAccess.clientfound.start: "+nodeStart.id+" ---["+nodeStart.name+"]{"+nodeStart.nodeType+"}["+nodeStart.argument+"]---------------- ");

						}
						*/

						// send the actual value after patching
						// cfm.argument (VALUE) SEND TO nodeAddObject.id
						// send the actual value ...


						foundObject=true;
						debugStructureChanging=true; // debugStructureChange


					}
					else {

						// not found
						// error.nopatch
						ConfigMessage cfmAnswer=new ConfigMessage();
						cfmAnswer.path="error.nocorrectinput";
						cfmAnswer.argument="patch output-slot to input-slot!";
						// send reply
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );

					}

				}	
				// reply if nothing could be found!
				if (!foundObject)
				{
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

			} // addslot 

			// add autopatches ...
			if (cfm.command.Equals("patch.auto"))
			{
				commandFound=true;
				// get autopatch object ...
				Node nodeAutopatchTmp=structureServiceObj.SearchNodeByPathRecursive(structureServiceObj.tree,"root.server.patchesauto");
				if (nodeAutopatchTmp!=null) {

					// add here and now ...
					// 		public Node AddNodeByValuesTo( string name, string type, string typesub, string nodetype, string argument, Node parentNodeObj ) {
					Node nodeSlot=structureServiceObj.AddNodeByValuesTo(""+cfm.path,"patch.auto","","patch.auto",""+cfm.argument,nodeAutopatchTmp);
					nodeSlot.name=cfm.path;
					nodeSlot.argument=cfm.argument;
					//			nodeSlot.name="fingerleft";

					// ... 
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";
					cfmAnswer.argument=""+nodeSlot.id;
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}


			}

			// unpatch > delete patch object !
			// by hand ...

			/*
			 * 
			 *   get & set
			 * 
			 * */

			/*
			*   get
			* 
			* */
			// let's start with parsing
			if (commandKey.Equals("get")) {

				commandFound=true;

				if (debugThis) Console.WriteLine("processConfigMessage() --- GET --"+cfm.path+"----------------- ");
				string path=cfm.path;

				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,path);
				bool error=false;
				string errorMessage="";

				string valueGet="null";

				bool foundObject=false;

				// get all 
				bool searchObjectsByPath=true;

				// not path relevant objects
				// todo: make this simpler
				bool searchObjectsByPathDone=false;
				if (path.Equals ("structure")) { valueGet=""+structureServiceObj.DebugStructure(); /* Console.WriteLine ("***************"+valueGet); */  searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true; }

				// 2 ways for searching
				// define
				if 
					(path.Equals ("clients")) 

				{
					if (debugThis) Console.WriteLine ("----------FIND----CLIENTS--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("client");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];
						valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}
				if (path.Equals ("devices")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----DEVICES--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("device");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];
						valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}
				if (path.Equals ("slotsets")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOTSETS--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slotset");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];
						valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}
				if (path.Equals ("slots")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];
						valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				// slots and types - input/output/pushpull
				if (path.Equals ("slots.input")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.INPUT--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=false;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (nodeType.argument.Equals ("input")) addThis=true;
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				// clientHandlerObj.clientNode
				if (path.Equals ("slots.input.internal")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.INPUT.INTERNAL--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=false;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (nodeType.argument.Equals ("input")) 
							{
								addThis=true;
								if (nodeType.clientId!=clientHandlerObj.clientNode.id) addThis=false;
							}
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				if (path.Equals ("slots.input.external")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.INPUT.EXTERNAL--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=false;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (nodeType.argument.Equals ("input")) 
							{
								addThis=true;
								if (nodeType.clientId==clientHandlerObj.clientNode.id) addThis=false;
							}
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				if (path.Equals ("slots.output")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.OUTPUT--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=false;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (nodeType.argument.Equals ("output")) addThis=true;
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				// clientHandlerObj.clientNode
				if (path.Equals ("slots.output.internal")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.OUTPUT.INTERNAL--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=false;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (nodeType.argument.Equals ("output")) 
							{
								addThis=true;
								if (nodeType.clientId!=clientHandlerObj.clientNode.id) addThis=false;
							}
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				if (path.Equals ("slots.output.external")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.OUTPUT.EXTERNAL--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=false;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (nodeType.argument.Equals ("output")) 
							{
								addThis=true;
								if (nodeType.clientId==clientHandlerObj.clientNode.id) addThis=false;
							}
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}


				if (path.Equals ("slots.pushpull")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----SLOT.PUSHPULL--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("slot");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];

						// get special node here
						bool addThis=true;

						Node nodeType=tmpNode.SearchChildByName("type");
						if (nodeType!=null)
						{
							if (!nodeType.argument.Equals ("pushpull")) addThis=false;
						}

						if (addThis) valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}



				if (path.Equals ("patches")) 
				{
					if (debugThis) Console.WriteLine ("----------FIND----PATCHES--------");
					ArrayList arr=structureServiceObj.SearchNodesByType("patch");
					valueGet="";
					Node tmpNode;
					for (int z=0;z<arr.Count;z++)
					{
						tmpNode=(Node) arr[z];
						valueGet=valueGet+tmpNode.id+",";
					}
					// remove value
					valueGet=CutAwayEndingSemicolon(valueGet);
					searchObjectsByPath=false; foundObject=true; searchObjectsByPathDone=true;
				}

				// special? 
				// patches.name.xyz
				// patches.id.xyz
				// patches.notetype.xyz


				if (searchObjectsByPathDone)
				{
					// reply
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="ok";
					cfmAnswer.argument=""+valueGet;

					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

				// get something for path
				if (searchObjectsByPath)
				{
					Node nodObj;
					if (debugThis) Console.WriteLine("      processConfigMessage()  size: "+arrObjects.Count);
					for (int z=0;z<arrObjects.Count;z++)
					{
						nodObj=(Node)arrObjects[z];
						if (debugThis) Console.WriteLine("         processConfigMessage()  "+z+" objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

						// last component
						string lastComp=structureServiceObj.GetLastPathComponent(path);


						// get.name etc
						if (!commandInternalKey.Equals (""))
						{
							if (commandInternalKey.Equals ("id"))
							{
								valueGet=""+nodObj.id; 
								foundObject=true;	
							}
							if (commandInternalKey.Equals ("parent"))
							{
								if (nodObj.parent != null) {
									valueGet = "" + nodObj.parent.id; 
									foundObject = true;	
								}
							}
							if (commandInternalKey.Equals ("name"))
							{
								valueGet=""+nodObj.name; 
								foundObject=true;	
							}
							if (commandInternalKey.Equals ("nodetype"))
							{
								valueGet=""+nodObj.nodeType; 
								foundObject=true;	
							}
							if (commandInternalKey.Equals ("argument"))
							{
								valueGet=""+nodObj.argument; 
								foundObject=true;	
							}
							if (commandInternalKey.Equals ("owner"))
							{
								valueGet="false"; 
								Node clientObj=GetClientObjectNodeRecursive(nodObj);
								if (clientObj!=null) {
									if (clientHandlerObj.clientNode.id==clientObj.id) valueGet="true";
								}
								foundObject=true;	
							}
							if (commandInternalKey.Equals ("path"))
							{
								// Console.WriteLine("SearchSomePath._path"+nodObj.name);
								string pathToRoot=GetRootObjectNodePath( nodObj,"");
								if (!valueGet.Equals (""))
								{
									valueGet="root."+pathToRoot; 
									foundObject=true;  
								}
							}
							if (commandInternalKey.Equals ("length"))
							{
								valueGet=""+nodObj.children.Count; foundObject=true; 
							}
							if (commandInternalKey.Equals ("objects"))
							{
								if (debugThis) Console.WriteLine ("get "+path+".objects");
								string retStr="";
								foreach( Node nodeObj in nodObj.children) {
									retStr=retStr+nodeObj.id+",";
								}	
								retStr=CutAwayEndingSemicolon(retStr);
								foundObject=true;
								valueGet=retStr;
							}

							// devicefor
							// slotsetfor
							// slotsetnormalizedfor
							// todo ...
							if (commandInternalKey.Equals ("clientfor"))
							{
								string argId=GetNodeUpInTreeByType(nodObj,"client");
								if (argId!=null)
								{
									valueGet=argId; 
									foundObject=true;  
								}
							}

							if (commandInternalKey.Equals ("devicefor"))
							{
								string argId=GetNodeUpInTreeByType(nodObj,"device");
								if (argId!=null)
								{
									valueGet=argId; 
									foundObject=true;  
								}
							}

							if (commandInternalKey.Equals ("slotsetfor"))
							{
								string argId=GetNodeUpInTreeByType(nodObj,"slotset");
								if (argId!=null)
								{
									valueGet=argId; 
									foundObject=true;  
								}
							}



						} else {
							// .name etc
							valueGet=nodObj.argument;
							foundObject=true;	
						}

					}	

					// publish this
					if (foundObject) {
						// reply
						ConfigMessage cfmAnswer=new ConfigMessage();
						cfmAnswer.path="ok";
						cfmAnswer.argument=""+valueGet;

						// send reply
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					}

					// search for the client 

					// todo: process change!
					// converto ...
					// todo: processThis 
					// change > monitoring
				}	


				// reply if nothing could be found!
				if (!foundObject) {
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}

				// monitor(); > ....
			}


			/*
			 *   set
			 * 
			 * */
			// let's start with parsing
			// todo: split "stream" to another optimised
			// todo: #there			
			if ( (commandKey.Equals("set")) ||
				(commandKey.Equals("stream"))) {

				commandFound=true;
				// get the object of the path
				// convert the object to an 

				// bypass for set slot type to 'pushpull' 
				// simple check for set abc "pushpull" (used for change )
				// todo: 
				if (cfm.argument.Equals("pushpull")) {
					// flagStructureChange = true;
					flagSlotChange = true;
					// Console.WriteLine ("SLOT TO PUSH CHANGE!");
				}
				// todo
				// check recursive patches  .... !
				// slot.patch
				// slot.value
				// todo: definition!!!!
				// set xzy.slot.value ??? 

				// todo: stream - no feedback ... at all!

				// this. > clientx.
				// 234. > object number
				if (debugThis) Console.WriteLine("processConfigMessage() --- SET ------------------- ");
				string path=cfm.path;
				ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,path);
				bool error=false;
				string errorMessage="";

				bool foundObject=false;
				Node nodObj;
				// todo: why not just take the last one? (go through everything?)
				for (int z=0;z<arrObjects.Count;z++) {
					nodObj=(Node)arrObjects[z];
					if (debugThis) Console.WriteLine("          processConfigMessage()  objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

					// last component
					string lastComp=structureServiceObj.GetLastPathComponent(path);

					// change value
					if (commandInternalKey.Equals ("")) {
						// bool setNewArgument=false;

						bool newValue=false;

						// todo: something to change?
						if (!nodObj.argument.Equals(cfm.argument)) newValue=true;

						// todo: is streaming do it always!

						// default set value ...
						nodObj.argument=cfm.argument;

						// is this a slot?
						// case: slot
						//       slot.value
						//       slot.type
						//       slot.patch
						// if (newValue)
						if (nodObj.nodeType.Equals ("slot")) {

							// get value
							// store there and parse!
							//
							// store to node value!
							// value
							Node valObj=GetChildByName(nodObj,"value");
							if (valObj!=null) {
								valObj.argument=cfm.argument;
							}

							// config
							// input node? (config)
							// possible to write directly into the slot
							Node clientNodeObjX=GetClientObjectNode(nodObj);
							// send over the client the message!
							if (clientNodeObjX!=null)
							{
								if (debugThis) Console.WriteLine ("patchToObject.ClientObject."+clientNodeObjX.id+"/"+clientNodeObjX.name);
								// send now to client?
								if (clientNodeObjX.argumentTypedObject!=null)
								{
									// send it over this client!!!...
									// get string
									string localPath=GetClientObjectNodePath(nodObj,"");
									if (debugThis) Console.WriteLine ("patchToObject.path."+localPath);

									try
									{
										ClientHandler clientHandlerObjHere=(ClientHandler)clientNodeObjX.argumentTypedObject;
										// send with this now
										// todo: work with lists etc ..
										ConfigMessage cfmObj=new ConfigMessage();
										cfmObj.command="stream";
										cfmObj.path=localPath; // ".value" / 
										cfmObj.argument=nodObj.argument;

										SendConfigCommand(clientHandlerObjHere,cfmObj);
									}
									catch(Exception ex)
									{
										Console.WriteLine (""+ex.ToString());	
									}
								}
							} // client

							// patches in children? ...
							// 
							// slot
							// set x to SLOT
							//     \___ patch1
							//     \____ patch2
							foreach( Node nodeObj in nodObj.children) {


								// default: patch this here
								if (nodeObj.name.Equals ("patch")) {
									// ok patch to this 
									// argument
									string arg=nodeObj.argument;
									// path 
									// object?
									if (debugThis) Console.WriteLine("patch "+arg);

									// send argument
									ArrayList arrObjectsTmp=GetObjectsForPath(clientHandlerObj.clientNode,arg);
									Node nodObjTmp;
									for (int uz=0;uz<arrObjectsTmp.Count;uz++) {
										nodObjTmp=(Node) arrObjectsTmp[uz];

										// ok the path
										if (nodObjTmp!=null) {

											if (debugThis) Console.WriteLine("patchToObject: "+nodObjTmp.id+"/"+nodObjTmp.name);

											// set value
											nodObjTmp.argument=cfm.argument;
											// monitor 

											// send actualisation 
											// search for the client
											// clientId

											Node clientNodeObj=GetClientObjectNode(nodObjTmp);
											// send over the client the message!
											if (clientNodeObj!=null)
											{
												if (debugThis) Console.WriteLine ("patchToObject.ClientObject."+clientNodeObj.id+"/"+clientNodeObj.name);
												// send now to client?
												if (clientNodeObj.argumentTypedObject!=null)
												{
													// send it over this client!!!...
													// get string
													string localPath=GetClientObjectNodePath(nodObjTmp,"");
													if (debugThis) Console.WriteLine ("patchToObject.path."+localPath);

													try
													{
														ClientHandler clientHandlerObjHere=(ClientHandler)clientNodeObj.argumentTypedObject;
														// send with this now
														// todo: work with lists etc ..
														ConfigMessage cfmObj=new ConfigMessage();
														cfmObj.command="stream";
														cfmObj.path=localPath; // ".value" / 
														cfmObj.argument=nodObjTmp.argument;

														SendConfigCommand(clientHandlerObjHere,cfmObj);
													}
													catch(Exception ex)
													{
														Console.WriteLine (""+ex.ToString());	
													}
												}
											} // client

											// is this a patch again?
											// just patch patch.pushpull!
											// patch->slot
											//        \__ patch.pushpull
											// recursive (1x)
											if (nodObjTmp != null) {

												// for simpler auto logging!
												// is there a patch.pushpull in the children??
												foreach( Node nodeObjPatchChildren in nodObjTmp.children) {
													// patch this here
													if (nodeObjPatchChildren.name.Equals ("patch.pushpull")) {
														// return nodeObj;
														if (debugThis) Console.WriteLine("[recursive!patch.pushpull(forloggingetc)]patch>foundslot&patch.pushpull!!! "+nodeObjPatchChildren.id);
														string argdest=nodeObjPatchChildren.argument;

														ArrayList rarrObjectsTmp=GetObjectsForPath(clientHandlerObj.clientNode,argdest);
														Node rnodObjTmp;
														for (int ruz=0;ruz<rarrObjectsTmp.Count;ruz++) {
															rnodObjTmp=(Node) rarrObjectsTmp[ruz];

															// ok the path
															if (rnodObjTmp!=null) {
																	if (debugThis) Console.WriteLine("Patch>PatchPushPull.patchToObject: "+rnodObjTmp.id+"/"+rnodObjTmp.name);

																// set value
																rnodObjTmp.argument=cfm.argument;
																// monitor 

																// send actualisation 
																// search for the client
																// clientId

																Node rclientNodeObj=GetClientObjectNode(rnodObjTmp);
																// send over the client the message!
																if (rclientNodeObj!=null)
																{
																	if (debugThis) Console.WriteLine ("Patch>PatchPushPull.patchToObject.ClientObject."+rclientNodeObj.id+"/"+rclientNodeObj.name);
																	// send now to client?
																	if (rclientNodeObj.argumentTypedObject!=null)
																	{
																		// send it over this client!!!...
																		// get string
																		string localPath=GetClientObjectNodePath(rnodObjTmp,"");
																		if (debugThis) Console.WriteLine ("Patch>PatchPushPull.patchToObject.path."+localPath);

																		try
																		{
																			ClientHandler clientHandlerObjHere=(ClientHandler)rclientNodeObj.argumentTypedObject;
																			// send with this now
																			// todo: work with lists etc ..
																			ConfigMessage cfmObj=new ConfigMessage();
																			cfmObj.command="push";
																			// todo: is this correct or should it come only from patch origin a (a>b) ? 
																			// send over the client ... 
																			string pathOfNode=GetRootObjectNodePath( nodeObj.parent,"");
																			cfmObj.path=localPath+"#"+pathOfNode; // ".value" / 
																			cfmObj.argument=rnodObjTmp.argument;

																			SendConfigCommand(clientHandlerObjHere,cfmObj);
																		}
																		catch(Exception ex)
																		{
																			Console.WriteLine (""+ex.ToString());	
																		}
																	}
																} // client
															} // default
															
														} // end of recursive patching patch.pushpull

													}
												}



											}


										} // default

									} // / for
								} // /default slot


								// default: patch this here
								// is it a set to a slot with a patch.pushpull?
								if (nodeObj.name.Equals ("patch.pushpull")) {
									// ok patch to this 
									// argument
									string arg=nodeObj.argument;
									// path 
									// object?
									if (debugThis) Console.WriteLine("patch.pushpull "+arg);

									// send argument
									ArrayList arrObjectsTmp=GetObjectsForPath(clientHandlerObj.clientNode,arg);
									Node nodObjTmp;
									for (int uz=0;uz<arrObjectsTmp.Count;uz++) {
										nodObjTmp=(Node) arrObjectsTmp[uz];

										// ok the path
										if (nodObjTmp!=null) {
											if (debugThis) Console.WriteLine("PatchPushPull.patchToObject: "+nodObjTmp.id+"/"+nodObjTmp.name);

											// set value
											nodObjTmp.argument=cfm.argument;
											// monitor 

											// send actualisation 
											// search for the client
											// clientId

											Node clientNodeObj=GetClientObjectNode(nodObjTmp);
											// send over the client the message!
											if (clientNodeObj!=null)
											{
												if (debugThis) Console.WriteLine ("PatchPushPull.patchToObject.ClientObject."+clientNodeObj.id+"/"+clientNodeObj.name);
												// send now to client?
												if (clientNodeObj.argumentTypedObject!=null)
												{
													// send it over this client!!!...
													// get string
													string localPath=GetClientObjectNodePath(nodObjTmp,"");
													if (debugThis) Console.WriteLine ("PatchPushPull.patchToObject.path."+localPath);

													try
													{
														ClientHandler clientHandlerObjHere=(ClientHandler)clientNodeObj.argumentTypedObject;
														// send with this now
														// todo: work with lists etc ..
														ConfigMessage cfmObj=new ConfigMessage();
														cfmObj.command="push";
														// todo: is this correct or should it come only from patch origin a (a>b) ? 
														// send over the client ... 
														string pathOfNode=GetRootObjectNodePath( nodeObj.parent,"");
														cfmObj.path=localPath+"#"+pathOfNode; // ".value" / 
														cfmObj.argument=nodObjTmp.argument;

														SendConfigCommand(clientHandlerObjHere,cfmObj);
													}
													catch(Exception ex)
													{
														Console.WriteLine (""+ex.ToString());	
													}
												}
											} // client
										} // default

									} // / for
								} // /default slot


							}
						}

						// do the patches here!!!

						foundObject=true;
					}

					/*
					// version 1.0
					// change internal
					// _name etc
					if (CheckForInternalAttribute(lastComp))
					{
						Console.WriteLine ("SpecialAttribute "+lastComp);
						if (lastComp.Equals ("_id"))
							nodObj.name=cfm.argument;
						if (lastComp.Equals ("_name"))
							nodObj.name=cfm.argument;
						if (lastComp.Equals ("_length"))
							nodObj.name=cfm.argument;
						// _object_
						// todo: set this

						foundObject=true;
					}
					*/

					// set.name etc
					// todo: fix here!
					if (!commandInternalKey.Equals ("")) {
						if (debugThis) Console.WriteLine ("COMMANDINTERNALKEY: "+commandInternalKey);	

						if (commandInternalKey.Equals ("name")) {
							nodObj.name=cfm.argument;
							foundObject=true;	

							// can change ....
							flagStructureChange=true;
						}

						//todo: virtuality

					}

					// replys  
					if (foundObject && !commandKey.Equals("stream")) {
						ConfigMessage cfmAnswer=new ConfigMessage();	
						cfmAnswer.path="ok";
						cfmAnswer.argument="object found";
						// send reply
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
						//structureServiceObj.MonitorThisNode(nodObj,"update");
					}


					// todo: process change!
					// converto ...
					// todo: processThis 
					// change > monitoring

					// patch on change ...
					// patchtypes ...					

					// monitor(); > ....

				}

				// todo: ....
				// asynchronic reply

				/* ANSWER */
				if (!foundObject) {
					ConfigMessage cfmAnswer=new ConfigMessage();	
					cfmAnswer.path="error";
					cfmAnswer.argument="object not found";
					// send reply
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );							
				}

				// specials
				// list[0] etc. 

				// answer here ...

				// turn this to a value ... 
			}


			// push & pull
			// direct communication ...
			// hashes ...
			// no connection needed?
			// to device ...
			// patch hash-slots
			// 
			// send patches add hash ...
			// # ... 
			// 
			// push j11 this.name.x#test 444
			if (commandKey.Equals("push"))
			{
				commandFound=true;

				// do the push to a slot

				// patches to ... 

				// get id ....

				// add target client id here ... 

				// push > slotX 

				// send / receive ... !!!

				// makes a node?
				bool error=false;
				string errorTag="error";
				string errorMessage="";

				if (debugThis) Console.WriteLine("processConfigMessage() --- PUSH ------------------- ");
				string path=cfm.path;
				if (debugThis) Console.WriteLine("-- path: "+path);

				// cut away hash part!
				string pathNormalized=path;
				string hashTag="";

				int posHasTag=pathNormalized.IndexOf ("#");
				if (posHasTag==-1) {
					error=true;
					errorTag="error.malform";
					errorMessage="#-tag is missing";
				}
				else
				{
					pathNormalized=path.Substring(0,posHasTag);
					hashTag=path.Substring(posHasTag+1);

					// get path ... 
					ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,pathNormalized);
					Node nodObj;
					for (int z=0;z<arrObjects.Count;z++) {
						if (debugThis) Console.WriteLine("          processConfigMessage()  objects found : "+arrObjects.Count);

						nodObj=(Node)arrObjects[z];
						if (debugThis) Console.WriteLine("          processConfigMessage()  objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

						// get client for this object
						Node clientObjOfNode=GetClientObjectNode(nodObj);
						if (clientObjOfNode==null) {

							error=true;
							errorTag="error.noclient";
							errorMessage="#-tag is missing";

						}
						else {

							if (debugThis) Console.WriteLine("          processConfigMessage()  client-id"+clientObjOfNode.id);

							// get client handler for this ...

							if (clientObjOfNode.argumentTypedObject!=null)
							{
								// send it over this client!!!...
								// get string
								string localPath=GetClientObjectNodePath(nodObj,"");
								if (debugThis) Console.WriteLine ("      patchToObject.path."+localPath);

								try
								{
									ClientHandler clientHandlerObjHere=(ClientHandler)clientObjOfNode.argumentTypedObject;
									// send with this now
									// todo: work with lists etc ..
									ConfigMessage cfmObj=new ConfigMessage();
									cfmObj.command="push";
									cfmObj.externalId=cfm.externalId;
									cfmObj.path=localPath+"#"+hashTag; // ".value" / 
									cfmObj.argument=cfm.argument;

									SendConfigCommandBase(clientHandlerObjHere,cfmObj,false);
								}
								catch(Exception ex)
								{
									Console.WriteLine (""+ex.ToString());	
								}
							}


						}

					}


					// send ok message!
					if (error) {
						ConfigMessage cfmAnswer=new ConfigMessage();
						cfmAnswer.path=""+errorTag;  
						cfmAnswer.argument=errorMessage; // ); // +nodeAddObject.id;
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					}
					else {
						ConfigMessage cfmAnswer=new ConfigMessage();
						cfmAnswer.path="ok";  
						cfmAnswer.argument="delivered"; // ); // +nodeAddObject.id;
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					}

				}

			}

			// from client
			// > pull > pull.task (client) > pull.task.done (server) > ok answer ...
			// client: #tag:clientid
			// if (commandKey.Equals("pull"))
			if (cfm.command.Equals("pull"))	
			if (commandInternalKey.Equals(""))
			{
				commandFound=true;

				// complex mechanics 

				// clientA: "pull" > server
				//                   server: "pull.task" > client
				//                                         client:"pull.task.done" > server
				//                                                                   server: "ok" > client


				// makes a node?
				bool error=false;
				string errorTag="error";
				string errorMessage="";

				if (debugThis) Console.WriteLine("processConfigMessage() --- PULL ------------------- ");
				string path=cfm.path;
				if (debugThis) Console.WriteLine("-- path: "+path);

				// cut away hash part!
				string pathNormalized=path;
				string hashTag="";

				int posHasTag=pathNormalized.IndexOf ("#");
				if (posHasTag==-1) {
					error=true;
					errorTag="error.malform";
					errorMessage="#-tag is missing";
				}
				else
				{
					pathNormalized=path.Substring(0,posHasTag);
					hashTag=path.Substring(posHasTag+1);

					// get path ... 
					ArrayList arrObjects=GetObjectsForPath(clientHandlerObj.clientNode,pathNormalized);
					Node nodObj;
					for (int z=0;z<arrObjects.Count;z++) {
						if (debugThis) Console.WriteLine("          processConfigMessage()  objects found : "+arrObjects.Count);

						nodObj=(Node)arrObjects[z];
						if (debugThis) Console.WriteLine("          processConfigMessage()  objects found : "+z+":"+nodObj.id+"  "+nodObj.name);

						// get client for this object
						Node clientObjOfNode=GetClientObjectNode(nodObj);
						if (clientObjOfNode==null) {

							error=true;
							errorTag="error.noclient";
							errorMessage="#-tag is missing";

						}
						else {

							if (debugThis) Console.WriteLine("          processConfigMessage()  client-id"+clientObjOfNode.id);

							// get client handler for this ...

							if (clientObjOfNode.argumentTypedObject!=null)
							{
								// send it over this client!!!...
								// get string
								string localPath=GetClientObjectNodePath(nodObj,"");
								if (debugThis) Console.WriteLine ("      patchToObject.path."+localPath);

								try
								{
									ClientHandler clientHandlerObjHere=(ClientHandler)clientObjOfNode.argumentTypedObject;
									// send with this now
									// todo: work with lists etc ..
									ConfigMessage cfmObj=new ConfigMessage();
									cfmObj.command="pull.request";
									cfmObj.externalId=cfm.externalId;
									cfmObj.path=localPath+"#"+hashTag+":"+clientHandlerObj.clientNode.id; // ".value" / 
									// cfmObj.argument=cfm.argument;
									cfmObj.argument="";

									SendConfigCommandBase(clientHandlerObjHere,cfmObj,false);
								}
								catch(Exception ex)
								{
									Console.WriteLine (""+ex.ToString());	
								}
							}


						}

					}


					// send ok message!
					if (error) {
						ConfigMessage cfmAnswer=new ConfigMessage();
						cfmAnswer.path=""+errorTag;  
						cfmAnswer.argument=errorMessage; // ); // +nodeAddObject.id;
						SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
					}


				}

			}


			// pull.task.done
			if (commandKey.Equals("pull"))
			if (commandInternalKey.Equals("answer"))
			{
				commandFound=true;
				// complex mechanics 

				// clientA: "pull" > server
				//                   server: "pull.task" > client
				//                                         client:"pull.task.done" > server
				//                                                                   server: "ok" > client


				// makes a node?
				bool error=false;
				string errorTag="error";
				string errorMessage="";

				if (debugThis) Console.WriteLine("processConfigMessage() --- PULL.TASK.ANSWER ------------------- ");
				string path=cfm.path;
				if (debugThis) Console.WriteLine("-- path: "+path);

				// cut away hash part!
				string pathNormalized=path;
				string hashTag="";
				string clientId="";


				pathNormalized=cfm.GetPushPullPath( );
				if (pathNormalized==null) { pathNormalized="error"; error=true;}

				hashTag=cfm.GetPushPullHash( );
				if (hashTag==null) { hashTag="error"; error=true; }

				clientId=cfm.GetPushPullClient( );
				if (clientId==null) { clientId="-1"; error=true; }

				try
				{
					int intClientId=Convert.ToInt32(clientId);
					Node clientObjOfNode=structureServiceObj.SearchNodeById(intClientId); 

					if (clientObjOfNode.argumentTypedObject!=null)
					{
						ClientHandler clientHandlerObjHere=(ClientHandler)clientObjOfNode.argumentTypedObject;
						// send with this now
						// todo: work with lists etc ..
						ConfigMessage cfmObj=new ConfigMessage();
						cfmObj.command="reply";
						cfmObj.externalId=cfm.externalId;
						cfmObj.path="ok";
						// cfmObj.path=localPath+"#"+hashTag+":"+clientHandlerObj.clientNode.id; // ".value" / 
						// cfmObj.argument=cfm.argument;
						cfmObj.argument=cfm.argument;

						SendConfigCommandBase(clientHandlerObjHere,cfmObj,false);
					}
					else {

						// error to client
						error=true;
						errorTag="error.notcorrectclient";
						errorMessage="wrong clientId from client"+clientId;
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine (""+ex.ToString());	
				}

				/*
							
							// version 1.0

				int posHashTag=pathNormalized.IndexOf ("#");
				int posClientId=pathNormalized.IndexOf (":");

				bool wellFormated=true;

				if ((posHashTag==-1)||(posClientId==-1)) wellFormated=false;
				// if ((posHashTag==-1)||(posClientId==-1)) wellFormated=false;

				if (!wellFormated)
				{
					error=true;
					errorTag="error.malform";
					errorMessage="#-tag or :-tag is missing";
				}
				else
				{
					try
					{
						if (debugThis) Console.WriteLine("          PULL.TASK.DONE "+path+" [path:"+posHashTag+"#"+posClientId+":] ");

						// todo: check input ... but only in secure area visible
						pathNormalized=path.Substring(0,posHashTag); // not used here ..
						hashTag=path.Substring(posHashTag+1,5); // not used here ... 
						clientId=path.Substring(posClientId+1);

						if (debugThis) Console.WriteLine("          PULL.TASK.DONE ["+pathNormalized+"#"+hashTag+":"+clientId+"] ");


						int intClientId=Convert.ToInt32(clientId);
						Node clientObjOfNode=structureServiceObj.SearchNodeById(intClientId); 

						if (clientObjOfNode.argumentTypedObject!=null)
						{
							ClientHandler clientHandlerObjHere=(ClientHandler)clientObjOfNode.argumentTypedObject;
							// send with this now
							// todo: work with lists etc ..
							ConfigMessage cfmObj=new ConfigMessage();
							cfmObj.command="reply";
							cfmObj.externalId=cfm.externalId;
							cfmObj.path="ok";
							// cfmObj.path=localPath+"#"+hashTag+":"+clientHandlerObj.clientNode.id; // ".value" / 
							// cfmObj.argument=cfm.argument;
							cfmObj.argument=cfm.argument;

							SendConfigCommandBase(clientHandlerObjHere,cfmObj,false);
						}
						else {

							// error to client
							error=true;
							errorTag="error.notcorrectclient";
							errorMessage="wrong clientId from client"+clientId;
						}
					}
					catch(Exception ex)
					{
						Console.WriteLine (""+ex.ToString());	
					}


				}

				*/

				// send ok message!
				if (error) {
					ConfigMessage cfmAnswer=new ConfigMessage();
					cfmAnswer.path=""+errorTag;  
					cfmAnswer.argument=errorMessage; // ); // +nodeAddObject.id;
					SendReturnConfigMessage(clientHandlerObj, cfmAnswer, cfm );
				}


			}



			// generate the path object ...

			// check here ... 

			// do the answer here

			// set done 
			// ok 234234
			// problem 234234 /error/ 234234

			// SendMessage(clientSocket, "")

			// set something there 
			// set 1111 min 2324
			// set 1112 /finger/slot 3234223


			// TcpClient ClientSocket
			//		byte[] sendBytes = Encoding.ASCII.GetBytes(response.ToString());
			// 		networkStream.Write(sendBytes, 0, sendBytes.Length);

			// something in structure changed ...
			// > look for patchesauto
			// is there a patch to do

			// do autopatch?
			// todo: nodeobject for whole app for autopatch
			// todo: faster ... poss. go throug all .nodes

			// todo: only every 2 secs!

			// problem: patch.auto not works for pushpulls (because the object is not yet initalised)
			if (flagSlotChange) {

				// check slots and update slottypes
				Console.WriteLine ("SLOTS: Updating ...");
				ArrayList arr=structureServiceObj.SearchNodesByType("patch");
				Node tmpNode;
				for (int z=0;z<arr.Count;z++)
				{
					tmpNode=(Node) arr[z];
					// valueGet=valueGet+tmpNode.id+",";
					// update notes
					// Console.WriteLine ("[UpdatingSlot] {"+tmpNode.id+"} "+tmpNode.argument);

					Node targetObj = structureServiceObj.SearchNodeByPath (tmpNode.argument);
					// tmpNode.argument
					if (targetObj!=null) {
						// ...
						//Node nodePatchedTarget==
						Node slotType = GetChildByName (targetObj, "type"); 
						if (slotType != null) {
							if (slotType.argument.Equals ("pushpull")) {	

								tmpNode.name = "patch.pushpull";

							}
						}
						// Console.WriteLine ("[UpdatingSlot] {"+tmpNode.id+"} "+tmpNode.argument);
					}
				}

			}

			// change in structure
			if (flagStructureChange)
			{
				bool debugPatchesAuto=false;

				if (debugPatchesAuto) Console.WriteLine("--- PatchesAuto --- ");

				// get autopatch object ...
				Node nodeAutopatch=structureServiceObj.SearchNodeByPathRecursive(structureServiceObj.tree,"root.server.patchesauto");
				if (nodeAutopatch!=null) {

					if (debugPatchesAuto) Console.WriteLine("	  1. found patchesauto-node ");

					// clean up all patches?

					// all patches
					ArrayList arrActivePatches=structureServiceObj.SearchNodesByType("patch");

					// go through the childen and check if there is one ...
					int coun=0;
					foreach( Node nodeAutoPatch in nodeAutopatch.children) {

						if (debugPatchesAuto) Console.WriteLine("	  2. ["+coun+"] process patchesauto: ");
						coun=coun+1;

						// name: starting point
						// argument: target point
						string fromObjectId=nodeAutoPatch.name;
						string targetObjectId=nodeAutoPatch.argument;

						if (debugPatchesAuto) Console.WriteLine("	  3. ["+coun+"] process from("+fromObjectId+") to ("+targetObjectId+")");


						Node nodeFrom=GetObjectForPath(  fromObjectId ); // structureServiceObj.SearchNodeByPathRecursive(fromObjectId);
						Node nodeTarget=GetObjectForPath(  targetObjectId ); // structureServiceObj.SearchNodeByPath(structureServiceObj.tree,targetObjectId);

						// are from and target - existing?
						if (
							(nodeFrom!=null)
							&&
							(nodeTarget!=null)
						)
						{
							// 1. get nodes - check if there is one
							//    for start/target
							bool foundPatch=false;

							Node tmpNodeActive;
							for (int z=0;z<arrActivePatches.Count;z++)
							{
								tmpNodeActive=(Node) arrActivePatches[z];

								if (debugPatchesAuto) Console.WriteLine("	  4. ["+coun+"]--- checkAgainstActive: "+tmpNodeActive.id+"--"+tmpNodeActive.parent.id+"--"+tmpNodeActive.argument );		


								// starting point ok!
								if (tmpNodeActive.parent.id==nodeFrom.id)
								{
									if (debugPatchesAuto) Console.WriteLine("	  5. ["+coun+"]--- Found Active Patch with same StartPoint "+nodeFrom.id );		

									// target point?
									Node nodeTargetActivePatch=GetObjectForPath(  tmpNodeActive.argument ); // structureServiceObj.SearchNodeByPathRecursive(structureServiceObj.tree,tmpNodeActive.argument);
									if (nodeTargetActivePatch!=null) {

										if (nodeTargetActivePatch.id==nodeTarget.id) {
											foundPatch=true;
											if (debugPatchesAuto) Console.WriteLine("	  6. ["+coun+"]--- Found Active Patch with same TargetPoint "+tmpNodeActive.argument );		
											break;
										}

									}
									// todo: still active? = in tree ... 
									else {

										if (debugPatchesAuto) Console.WriteLine("        6. ["+coun+"]--- Not found: "+tmpNodeActive.argument );		

									}

								}

							}

							// 2. yes create a patch now ...
							//    do a patch command! instead of direct ...
							if (!foundPatch) {
								// create this patch
								if (debugPatchesAuto) Console.WriteLine("--- CREATE PATCH: "+fromObjectId+"-> "+targetObjectId);		

								// add here a patch ...
								Console.WriteLine ("[PATCH.AUTO] + "+fromObjectId+">"+targetObjectId);
								Node nodeAddObject=structureServiceObj.AddPatch( nodeFrom, targetObjectId );
							}
							else {

								if (debugPatchesAuto) Console.WriteLine("--- THERE IS A PATCH -> no new patch:  "+fromObjectId+"-> "+targetObjectId);		

							}
						}
						else
						{
							if (debugPatchesAuto) Console.WriteLine("--- NOT FOUND start or target-Node"+fromObjectId+"-> "+targetObjectId);
						}

					}
				}

			}



			//write debug structure to console, if something has changed
			if ((debugThis)||((debugStructureChange)&&(debugStructureChanging))) {
				Console.WriteLine("----------------------");
				Console.WriteLine(" DebugStructure");
				Console.WriteLine("----------------------");
				string strDebug=structureServiceObj.DebugStructure();
				Console.WriteLine("  "+strDebug);
			}

			// answer into ..
			// Monitor.Exit(this);
		}


		// CheckForInternal
		// search for CheckForInternalAttribute and register this also
		/*
		public bool CheckForInternalAttribute( string attributeName ) {
			if (attributeName.Equals ("_id")) return true; 
			if (attributeName.Equals ("_name")) return true; 
			if (attributeName.Equals ("_hidden")) return true; 
			if (attributeName.Equals ("_length")) return true; 
			return false;
		}
		*/

		/// <summary>
		/// Helper function to cut away the ending semicolon, if there is one
		/// </summary>
		/// <returns>
		/// The string without the ending semicolon
		/// </returns>
		/// <param name='str'>
		/// The string to check for the ending semicolon and remove it if there
		/// </param>
		public string CutAwayEndingSemicolon( string str ) {
			bool debugThis=false;
			// ".....,"
			if (str.Length>0) { 	
				if (debugThis) Console.WriteLine ("---------"+str.Substring(str.Length-1,1));
				if (str.Substring(str.Length-1,1).Equals (",")) {
					str=str.Substring(0,str.Length-1);
				}
			}
			return str;
		}

		/// <summary>
		/// Checks for internal attribute with the name <c>attributeName</c>
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if there is an internal attribut with this name.
		/// </returns>
		/// <param name='attributeName'>
		/// The attribute name to check.
		/// </param>
		public bool CheckForInternalAttribute( string attributeName ) {
			return structureServiceObj.CheckForInternalKeywords(attributeName);
		}

		// todo: give back and check it ..

		/// <summary>
		/// Gets the child of a node with the <c>childName</c>
		/// </summary>
		/// <returns>
		/// The child node.
		/// </returns>
		/// <param name='nodObj'>
		/// The parent node to search within.
		/// </param>
		/// <param name='childName'>
		/// The name of the child to find
		/// </param>
		public Node GetChildByName( Node nodObj, string childName ) {

			return structureServiceObj.GetChildByName( nodObj, childName );

			// GetChildByName
			// version 1
			/*
			foreach( Node nodeObj in nodObj.children) {
				// patch this here
				if (nodeObj.name.Equals (childName)) {
					return nodeObj;
				}
			}
			
			return null;
			*/
		}

		// todo: docuentation
		// todo: > move to structure!
		public String GetChildArgumentByName( Node nodObj, string childName ) {

			Node nodeValueObj=GetChildByName( nodObj,  childName );
			if (nodeValueObj!=null)
			{
				return nodeValueObj.argument;
			}

			return null;
		}

		// todo: documentation
		public String GetChildArgumentByIdAndName( int objectId, string childName ) {

			Node nodObj=structureServiceObj.SearchNodeById(objectId);
			if (nodObj!=null)
			{
				Node nodeValueObj=GetChildByName( nodObj,  childName );
				if (nodeValueObj!=null)
				{
					return nodeValueObj.argument;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the child of a node with the <c>childValue</c>
		/// </summary>
		/// <returns>
		/// The child node.
		/// </returns>
		/// <param name='nodObj'>
		/// The parent node to search within.
		/// </param>
		/// <param name='childName'>
		/// The name of the child to find
		/// </param>
		public Node GetChildByValue( Node nodObj, string childValue ) {
			foreach( Node nodeObj in nodObj.children) {
				// patch this here
				if (nodeObj.name.Equals (childValue)) {
					return nodeObj;
				}
			}

			return null;
		}

		// todo in 
		// only one object
		// direct without clients ... 
		public Node GetObjectForPath( string path ) {		
			return structureServiceObj.GetObjectForPath(  path );
		}	

		/// <summary>
		/// Gets a list of all nodes on a <c>path</c>.
		/// </summary>
		/// <returns>
		/// List of all objects at the <c>path</c>
		/// </returns>
		/// <param name='nodeClientObj'>
		/// The starting node
		/// </param>
		/// <param name='path'>
		/// The path to search for the objects.
		/// </param>
		public ArrayList GetObjectsForPath( Node nodeClientObj, string path ) {

			return structureServiceObj.GetObjectsForPath(nodeClientObj, path );		
		}

		/// <summary>
		/// Gets the client of a secific node.
		/// </summary>
		/// <returns>
		/// The node of the client.
		/// </returns>
		/// <param name='tmpNode'>
		/// The node form witch we like to have the client.
		/// </param>
		public Node GetClientObjectNode(Node tmpNode) {
			return GetClientObjectNodeRecursive(tmpNode);
		}

		/// <summary>
		/// Gets the client of a secific node.
		/// </summary>
		/// <returns>
		/// The node of the client.
		/// </returns>
		/// <param name='tmpNode'>
		/// The node form witch we like to have the client.
		/// </param>
		public Node GetClientObjectNodeRecursive(Node tmpNode) {

			bool debugThis=false;

			if (debugThis) Console.WriteLine ("GetClientObjectNodeRecursive("+tmpNode.id+"/"+tmpNode.name+")");

			if (tmpNode.nodeType.Equals ("client")) {
				return tmpNode;	
			}

			if (tmpNode.parent!=null) {
				if (tmpNode.parent.nodeType.Equals ("client")) {	
					if (debugThis) Console.WriteLine ("GetClientObjectNodeRecursive("+tmpNode.parent.id+"/"+tmpNode.parent.name+").FOUND");
					return tmpNode.parent;
				} else {
					return GetClientObjectNodeRecursive(tmpNode.parent);
				}
			}

			return null;
		}


		/// <summary>
		/// Gets the path to the client starting at a specific node
		/// </summary>
		/// <returns>
		/// The path to the client as string
		/// </returns>
		/// <param name='tmpNode'>
		/// The node form witch we like to have the path to the client.
		/// </param>
		/// <param name='pathx'>
		/// The path up to the starting node
		/// </param>
		public string GetClientObjectNodePath(Node tmpNode, string pathx){
			bool debugThis=false;

			if (debugThis) Console.WriteLine ("GetClientObjectNodePath("+tmpNode.id+"/"+tmpNode.name+","+pathx+")");

			if (tmpNode.parent!=null) {
				if (tmpNode.parent.nodeType.Equals ("client")) {	
					if (!pathx.Equals ("")) return "this."+tmpNode.name+"."+pathx;
					return "this";
				}
				else {
					if (!pathx.Equals (""))	return GetClientObjectNodePath(tmpNode.parent,tmpNode.name+"."+pathx);
					else return GetClientObjectNodePath(tmpNode.parent,tmpNode.name);
				}
			}

			return ""+pathx;
		}

		/// <summary>
		/// Gets the path to the root object from the node.
		/// </summary>
		/// <returns>
		/// The path to the root object.
		/// </returns>
		/// <param name='tmpNode'>
		/// The object to get the path to root from
		/// </param>
		/// <param name='pathx'>
		/// The path up to the starting node
		/// </param>
		public string GetRootObjectNodePath(Node tmpNode, string pathx) {
			bool debugThis=false;
			if (debugThis) Console.WriteLine ("GetClientObjectNodePath("+tmpNode.id+"/"+tmpNode.name+","+pathx+")");

			if (tmpNode.parent!=null) {
				//if (tmpNode.parent.nodeType.Equals ("client"))
				if (tmpNode.parent==structureServiceObj.tree)
				{	
					if (!pathx.Equals ("")) return ""+tmpNode.name+"."+pathx;
					return "root";
				}
				else {
					if (!pathx.Equals (""))	return GetRootObjectNodePath(tmpNode.parent,tmpNode.name+"."+pathx);
					else return GetRootObjectNodePath(tmpNode.parent,tmpNode.name);
				}
			}

			return ""+pathx;
		}

		/// <summary>
		/// Gets the path to nodetype  object from the node.
		/// </summary>
		/// <returns>
		/// The path to the root object.
		/// </returns>
		/// <param name='tmpNode'>
		/// The object to get the path to root from
		/// </param>
		/// <param name='pathx'>
		/// The path up to the starting node
		/// </param>
		public string GetNodeUpInTreeByType(Node tmpNode, string strType) {

			bool debugThis=false;
			if (debugThis) Console.WriteLine ("GetNodeUpInTreeByType("+tmpNode.id+","+strType+")");

			if (tmpNode.parent!=null) {
				//if (tmpNode.parent.nodeType.Equals ("client"))
				if (tmpNode.nodeType==strType)
				{	
					return ""+tmpNode.id;
				}
				else { 

					return GetNodeUpInTreeByType( tmpNode.parent, strType);
				}
			}

			return null;
		}

		/// <summary>
		/// Handels the disconnect event.
		/// </summary>
		/// <param name='clientHandlerObject'>
		/// Tcp client to to disconnect
		/// </param>
		/// <param name='clientNode'>
		/// Node of the client to disconnect.
		/// </param>
		public void OnDisconnect(ClientHandler clientHandlerObject, Node clientNode) {
			bool debugThis=false;

			if (debugClass) debugThis=true;

			if (debugThis) Console.WriteLine("Remove client " + clientHandlerObject.clientNode.id + " from structure.");

			//remove client from structure
			structureServiceObj.RemoveNode(clientNode);
		}

		//problem: compile can't find urlencode in library!

		/// <summary>
		/// Converts a message to config message. 
		/// </summary>
		/// <returns>
		/// The converted config message.
		/// </returns>
		/// <param name='toconvert'>
		/// The config message to convert
		/// </param>
		public string convertToConfigMessage( string toconvert ) {
			string stringconverted="";

			stringconverted=toconvert.Replace (" ","+");
			stringconverted=toconvert.Replace ("\n","%13");

			return stringconverted;
		}

		/// <summary>
		/// Converts the config message to string.
		/// </summary>
		/// <returns>
		/// A <c>string</c> with the message
		/// </returns>
		/// <param name='toconvert'>
		/// The config message to convert
		/// </param>
		public string convertConfigMessageToString( string toconvert ) {
			string stringconverted="";

			stringconverted=toconvert.Replace ("+"," ");
			stringconverted=toconvert.Replace ("%13","\n");

			return stringconverted;
		}
	}
}

