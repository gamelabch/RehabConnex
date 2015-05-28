using System;
using System.Collections;

namespace RehabConnex {

	/// <summary>
	/// structure off all nodes of the server as tree
	/// </summary>
	public class Structure {
		
		private float serverVersion=0.0f; // version set by configservice
			
		public int nodeId=0; //!< unique id for a node
		public Node tree; //!< a reference to thr root node
		private Node nodeObjTemp; //!< temp nodeObject

		public ArrayList arrObjectIndex=new ArrayList(); //!< list of all nodes
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RehabConnex.Structure"/> class. (constructor)
		/// </summary>
		public Structure ( ) {

		}
		
		public void Start(float iserverVersion) {
			
			serverVersion=iserverVersion;
			
			tree=GenerateNode();
			tree.name="root";
			tree.type="";
			tree.nodeType="list";
			
			// server and software
			Node serverObj=AddNodeByValuesTo("server","","","server","",tree);
			Node serverNameObj=AddNodeByValuesTo("name","","","string","RehabConnex",serverObj);
				 serverNameObj.attribute=true;
			Node serverVersionObj=AddNodeByValuesTo("version","","","float",""+serverVersion,serverObj);
				 serverVersionObj.attribute=true;

			Node serverAlias=AddNodeByValuesTo("alias","","","list",""+serverVersion,serverObj);
				 serverAlias.argument="";
			Node serverPatchesAuto=AddNodeByValuesTo("patchesauto","","","list",""+serverVersion,serverObj);
				 serverPatchesAuto.argument="";
				 
			
			// predefined 
			Node clientsObj=AddNodeByValuesTo("clients","","","list","",serverObj);
						
			// main structure 
			// Node devicesObj=AddNodeByValuesTo("devices","","","","list",tree);
			
			// add virtual nodes
			// devices
		/*
			Node clientsObjServer=AddNodeByValuesTo("clients","","","list","",tree);
				 clientsObjServer.virtualNode=true;
			Node devicesObj=AddNodeByValuesTo("devices","","","list","server.clients.*.devices",tree);
				 devicesObj.virtualNode=true;
			Node slotsetsObj=AddNodeByValuesTo("slotsets","","","list","",tree);
				 slotsetsObj.virtualNode=true;
			Node slotsObj=AddNodeByValuesTo("slots","","","list","",tree);
				 slotsObj.virtualNode=true;
			Node patchesSlot=AddNodeByValuesTo("patches","","","list","",tree);
				 patchesSlot.virtualNode=true;
		*/	
			// object
			// object.attributes
			// object.children
			// object.234324
		}

		/// <summary>
		/// Generates a new default node.
		/// </summary>
		/// <returns>
		/// The new generated node.
		/// </returns>
		public Node GenerateNode() {
			Node obj=new Node();
			nodeId++;
			obj.id=nodeId;
			return obj;
		}
	
		/// <summary>
		/// Adds a new node with values.
		/// </summary>
		/// <returns>
		/// The new generated node.
		/// </returns>
		/// <param name='name'>
		/// Name of the node.
		/// </param>
		/// <param name='type'>
		/// (not used)
		/// </param>
		/// <param name='typesub'>
		/// (not used)
		/// </param>
		/// <param name='nodetype'>
		/// Type of the node.
		/// </param>
		/// <param name='argument'>
		/// Argument of the node.
		/// </param>
		/// <param name='parentNodeObj'>
		/// Parent node object.
		/// </param>
		public Node AddNodeByValuesTo( string name, string type, string typesub, string nodetype, string argument, Node parentNodeObj ) {
			Node nodeObjTemp=GenerateNode();
				 nodeObjTemp.name=name;
				 nodeObjTemp.type=type;
				 nodeObjTemp.typeSub=typesub;
				 nodeObjTemp.nodeType=nodetype;
				 nodeObjTemp.argument=argument;
		  	AddNodeTo(nodeObjTemp,parentNodeObj);
			return nodeObjTemp;
		}
		
	
		/// <summary>
		/// Adds a device to the tree.
		/// </summary>
		/// <returns>
		/// Returns the new generated device node
		/// </returns>
		public Node AddDevice() {
			Node deviceObj=GenerateNode();
			AddNodeTo(deviceObj,tree); 
			return deviceObj;
		}
		
		// add slot
		public Node AddSlot(Node clientNode, Node nodObj, string name)
		{
			return AddSlot( clientNode, nodObj,  name, "input");
		}
		public Node AddSlot(Node clientNode,Node nodObj, string name, string slotType)
		{
			Node nodeSlot=AddNodeByValuesTo(""+name,"","","slot","",nodObj);
			
						 // nodeSlot.name=""+cfm.argument;
						 nodeSlot.clientId=clientNode.id;
					Node nodeCommunication=AddNodeByValuesTo("type","","","communicationtype",""+slotType,nodeSlot);
						 nodeCommunication.clientId=clientNode.id;
					Node nodeProtocol=AddNodeByValuesTo("protocol","","","communicationprotocol","tcp",nodeSlot);
						 nodeProtocol.clientId=clientNode.id;
					Node nodePort=AddNodeByValuesTo("port","","","int","43000",nodeSlot);
						 nodePort.clientId=clientNode.id;
					Node nodeValue=AddNodeByValuesTo("value","","","float","0",nodeSlot);
						 nodeValue.clientId=clientNode.id;
					Node nodeStreaming=AddNodeByValuesTo("stream","","","boolean","true",nodeSlot);
						 nodeStreaming.clientId=clientNode.id;

					// slots: label (human read), abstract (short description), description
					Node nodeLabel=AddNodeByValuesTo("label",""+name,"","string","",nodeSlot);
						nodeLabel.clientId=clientNode.id;
					Node nodeAbstract=AddNodeByValuesTo("abstract","","","string","",nodeSlot);
						 nodeAbstract.clientId=clientNode.id;
					Node nodeDescription=AddNodeByValuesTo("description","","","string","",nodeSlot);
						 nodeDescription.clientId=clientNode.id;
			
					// function
					// '':default, 'config': config only
					Node nodeFunction=AddNodeByValuesTo("function","","","string","",nodeSlot);
						 nodeFunction.clientId=clientNode.id;
					Node nodeFunctionSub=AddNodeByValuesTo("functionsub","","","string","",nodeSlot);
						 nodeFunctionSub.clientId=clientNode.id;

					// config
					// case in config in function

						// is configurating which slot? locomat.normalized.biofeedback
						Node configNode=AddNodeByValuesTo("functionslotfor","","","string","",nodeSlot);
							 configNode.clientId=clientNode.id;

							// which is the raw data slot? (not normalized) locomat.raw.biofeedback
							Node relyNode=AddNodeByValuesTo("relayslot","","","string","",nodeSlot);
								 relyNode.clientId=clientNode.id;
						
						// 'field','button','list' (config)
						Node nodeGUI=AddNodeByValuesTo("configgui","textfield","","string","",nodeSlot);
							 nodeGUI.clientId=clientNode.id;
					
							// values
							// used for button, list ...
							// '','start','stop','pause' (config)
							Node nodeValues=AddNodeByValuesTo("configvalues","","","string","",nodeSlot);
								 nodeValues.clientId=clientNode.id;
						
							Node nodeValuesLabels=AddNodeByValuesTo("configvalueslabels","","","string","",nodeSlot);
								nodeValuesLabels.clientId=clientNode.id;

			// default value
							Node nodeValuesDefault=AddNodeByValuesTo("configvaluedefault","","","string","",nodeSlot);
								nodeValuesDefault.clientId=clientNode.id;

			Console.WriteLine("[Slot] + "+name+"  ["+slotType+"]  {"+nodeSlot.id+"} ");


			return nodeSlot;
		}
		
		
		// add patch ... 
		public Node AddPatch( Node hereObj, string path ) {
			
						Node nodeAddObject=AddNodeByValuesTo("patch","","","patch",""+path,hereObj);
						// Node nodeSlotSetsbject=structureServiceObj.AddNodeByValuesTo("slotsets","","","list","",nodeAddObject);
							 // nodeAddObject.clientId=clientHandlerObj.clientNode.id;
						
							// patch to a pushpull 
							// > add a tag there!!!
			
							// version 2
	// todo: tmpNodeActive.argument
							Node argNodeObject=GetObjectForPath(  path );
			
							/*
							// version 
							int targetId=-1;
// todo: add path > id ...							
							try {
								targetId=Convert.ToInt32(path);
							} catch( Exception ex ) {
								
								Console.WriteLine (""+ex.ToString());
							}
							
							// todo: unify this code 2x times
							Node argNodeObject=SearchNodeById(targetId);
							*/
			if (argNodeObject != null) {

				Node slotType = GetChildByName (argNodeObject, "type"); 
				if (slotType != null) {
					if (slotType.argument.Equals ("pushpull")) {	
										
						nodeAddObject.name = "patch.pushpull";
									
					}
				}

				Console.WriteLine ("[Patching] + (" + nodeAddObject.name + ")  " + hereObj.id + ">" + argNodeObject.id + " =>   "+path);
			} else {
				Console.WriteLine ("[Patching][Error] Could not find path {"+path+"} ");
			}


		
			return nodeAddObject;
		}



		// children checks
		public Node GetChildByName( Node nodObj, string childName ) {
			
			foreach( Node nodeObj in nodObj.children) {
				// patch this here
				if (nodeObj.name.Equals (childName)) {
					return nodeObj;
				}
			}
			
			return null;
		}
		
		// path to 
		// only one object
		// direct without clients ... 
		public Node GetObjectForPath( string path ) {
		
			ArrayList arrObjectsTmp=GetObjectsForPath(null,path);
			Node nodObjTmp;
			for (int uz=0;uz<arrObjectsTmp.Count;uz++) {
				
				nodObjTmp=(Node) arrObjectsTmp[uz];
				return nodObjTmp;
			
			}
			
			return null;
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
			
			bool debugThis=false;
		
			ArrayList arrList=new ArrayList();
			
			// getFirstPathComponent
			Node startNode=tree;
		
			// special startObjects
			string firstCompartement=GetFirstPathComponent(path);
		
			if (debugThis) Console.WriteLine ("0. GetObjectsForPath() firstCompartement .... "+firstCompartement);
		
			// first component
			if (firstCompartement!=null) {
				if (debugThis) Console.WriteLine ("1-GetObjectsForPath("+path+") ");
			
				bool firstCompartenmentIsAKey=false;
			
				// root
				if (firstCompartement.Equals("root")) {
					startNode=tree;
					firstCompartenmentIsAKey=true;
				}
			
				// special entry points
				// server 
				if (firstCompartement.Equals("server")) {
					Node objHere=SearchNodeByPathRecursive(tree,"root.server");
					if (objHere!=null) {	
						startNode=objHere;
					}
					firstCompartenmentIsAKey=true;
				}

				// clients
				if (firstCompartement.Equals("clients")) {
					Node objHere=SearchNodeByPathRecursive(tree,"root.server.clients");
					if (objHere!=null)
					{	
						startNode=objHere;
					}
					firstCompartenmentIsAKey=true;
				}
		
				// virtuals ...
					

				// check special keywords 
				if (nodeClientObj!=null)
				if (firstCompartement.Equals("this")) {
					startNode=nodeClientObj;
					// path=structureServiceObj.GetPathWithoutFirstComponent(path);
					firstCompartenmentIsAKey=true;
				}
			
				// concrete objects
				// 3423
				try {
					int searchId = Int32.Parse(firstCompartement);
					if (debugThis) Console.WriteLine ("1-GetObjectsForPath("+path+") searchId "+searchId);
					
					// object 
					Node nodeWithNumber = SearchNodeById(searchId);
					if (nodeWithNumber!=null) {
						if (debugThis) Console.WriteLine ("1-GetObjectsForPath("+path+") nodeWithNumber found ");
	 				 	startNode=nodeWithNumber;
						// path=structureServiceObj.GetPathWithoutFirstComponent(path);
						firstCompartenmentIsAKey=true; 
					}
					else {
						if (debugThis) Console.WriteLine ("1-GetObjectsForPath("+path+") nodeWithNumber could not find!!!! ");
					}
				}
				catch {
					// default case .. do nothing
					if (debugThis)  Console.WriteLine ("1-GetObjectsForPath("+path+") Could not identify as Integer searchId "+path);
// todo: crashes if there is no valid id!
				}
			
				// only this?
				if (GetPathComponentLength(path)==1) {
					// client
					arrList.Add(startNode);	
					return arrList;
				}
			
				// is first a key thing?
				if (firstCompartenmentIsAKey) {
					// cut away frist compartment
					int posFirst=path.IndexOf (".");
					if (posFirst!=-1) {
						path=path.Substring (posFirst+1);
					}
				
					// check for ._name etc
					// not there: _object_1 etc
					if (CheckForInternalKeywords(path)) {
						arrList.Add(startNode);
						return arrList;
					}
				}
			
				// ...
				// now search the clients for more ...
				// version 2.0
				Node objHereTmp;
				foreach( Node nodeObj in startNode.children) {
					objHereTmp=SearchNodeByPathRecursive(nodeObj, path);
					if (objHereTmp!=null) arrList.Add(objHereTmp);
				}
			
			
				// more than this
				// get path .. 
				/* version 1.0
				if (debugThis) Console.WriteLine ("2-GetObjectsForPath("+startNode.id+","+startNode.name+","+path+") ");
				Node obj=structureServiceObj.SearchNodeByPathRecursive( startNode, path  );

				// found something
				if (obj!=null) {
					// if (obj.nodeType.Equals ("list")
					// {
							arrList.Add (obj);
					// }
					// else
					// {
						// default - not a list
						// show a list ...
					// }
					
					if (debugThis) Console.WriteLine ("3-GetObjectsForPath() Found("+obj.id+","+obj.name+") ");
				
					return arrList;
				}
				*/
			}
			
			return arrList;
		}
			
		// searching

		/// <summary>
		/// Gets the number of components within a path.
		/// </summary>
		/// <returns>
		/// Number of components of the path.
		/// </returns>
		/// <param name='path'>
		/// The path to count its components
		/// </param>
		public int GetPathComponentLength( string path ) {
			string[] strArr = null;
			char[] splitchar = { '.' };
	        strArr = path.Split(splitchar);
			return strArr.Length;
		}
	
		/// <summary>
		/// Gets the last component of a path.
		/// </summary>
		/// <returns>
		/// The last component of the specified path.
		/// </returns>
		/// <param name='path'>
		/// The path of which the method retruns the last component.
		/// </param>
		public string GetLastPathComponent( string path ) {
			// find it
			string[] strArr = null;
			char[] splitchar = { '.' };
	            strArr = path.Split(splitchar);
			if (strArr.Length>0)
			{
			   return strArr[strArr.Length-1];	
			}
			
			return null;
		}
		
		/// <summary>
		/// Gets the first component of a path.
		/// </summary>
		/// <returns>
		/// The first component of the specified path.
		/// </returns>
		/// <param name='path'>
		/// The path of which the method retruns the first component.
		/// </param>
		public string GetFirstPathComponent( string path ) {
			// find it
			string[] strArr = null;
			char[] splitchar = { '.' };
	            strArr = path.Split(splitchar);
			if (strArr.Length>0)
			{
			   return strArr[0];	
			}
			
			return null;
		}
		
		/// <summary>
		/// Gets the path without the first component.
		/// </summary>
		/// <returns>
		/// The path without the first component.
		/// </returns>
		/// <param name='path'>
		/// The path.
		/// </param>
		public string GetPathWithoutFirstComponent( string path ) {
			// Console.WriteLine ("GetPathWithoutFirstComponent( "+path+" )");
		
			// find it
			string[] strArr = null;
			char[] splitchar = { '.' };
	            strArr = path.Split(splitchar);
			if (strArr.Length>1)
			{
				string newPath="";
			   // return strArr[0];	
			   for (int z=1;z<strArr.Length;z++)
				{
					newPath=newPath+strArr[z];
					if ((strArr.Length-1)!=z) { newPath=newPath+"."; } 
				}
				// Console.WriteLine ("GetPathWithoutFirstComponent( "+newPath+" )");
				return newPath;
			}
			else
			{
				return "";
			}
			
			return null;
		}
		
		/// <summary>
		/// Searchs the node by identifier.
		/// </summary>
		/// <returns>
		/// The node by identifier.
		/// </returns>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public Node SearchNodeById(int id) {
			// begin with tree
			return SearchNodeByIdRecursive(tree,id);
		}

		/// <summary>
		/// Searchs the node by identifier recursive.
		/// </summary>
		/// <returns>
		/// The node by identifier.
		/// </returns>
		/// <param name='startNode'>
		/// Start node for the search
		/// </param>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		public Node SearchNodeByIdRecursive(Node startNode, int id) {
			// Console.WriteLine ("SearchNodeByIdRecursive ("+startNode.name+"/"+startNode.id+","+id+")");		
	
			// children
			foreach( Node nodeObj in startNode.children) {
			   Node nod=SearchNodeByIdRecursive(nodeObj,id);
			   if (nod!=null) return nod;	
			}
	
			// end node
			if (startNode.id==id) {
				// Console.WriteLine ("SearchNodeByIdRecursive FOUND! ("+startNode.name+"/"+startNode.id+","+id+")");		
				return startNode;
			}
			
			// nothing found
			return null;
		}
	
		/// <summary>
		/// Searchs a node by path.
		/// </summary>
		/// <returns>
		/// The node by path.
		/// </returns>
		/// <param name='path'>
		/// Path you like to fetch the node from
		/// </param>
		public Node SearchNodeByPath(string path) {
			bool debugThis=false;
			if (debugThis)
				Console.WriteLine ("SearchNodeByPath("+path+")");

			// begin with tree
			Node answerObj=SearchNodeByPathRecursive(tree,path);
			if (debugThis) {
				if (answerObj!=null) Console.WriteLine ("SearchNodeByPath("+path+").found("+answerObj.id+","+answerObj.name+")");
				else Console.WriteLine ("SearchNodeByPath("+path+").notFound");
			}
			return answerObj;
		}

		/// <summary>
		/// Searchs a node by path.
		/// </summary>
		/// <returns>
		/// The node by path.
		/// </returns>
		/// <param name='startObject'>
		/// Node to start the search with.
		/// </param>
		/// <param name='path'>
		/// Path you like to fetch the node from
		/// </param>
		/// todo: ? path can't be id?
		public Node SearchNodeByPathRecursive (Node startObject, string path)
		{
			bool debugThis = false;
	
			if (startObject != null) {
				// ...
				string[] strArr = null;
				char[] splitchar = { '.' };
				strArr = path.Split (splitchar);
		
				if (debugThis)
					Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")");
		
				string actualName = strArr [0];
				
				// end node
				if (strArr.Length == 0) {
					if (debugThis)
						Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ") Length===0");
					// use this?
					if (CheckForInternalKeywords ("" + strArr [0])) {
						if (debugThis)
							Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ") Length===0 InternalWordsFound!");
						return startObject; 
					}
				} else if (strArr.Length == 1) {
					
					if (debugThis)
						Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").lookat=" + startObject.name + " LENGTH==1");
			
					if (CheckForInternalKeywords ("" + strArr [0])) {
						if (debugThis)
							Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").lookat=" + startObject.name + " LENGTH==1 InternalKeynote");
						return startObject.parent; 
					}
			
					// same area
					if (actualName.Equals ("" + startObject.name)) {
						if (debugThis)
							Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").found=" + startObject.name + " LENGTH==1");
			
						// specials
						// todo: unify!
					
						return startObject;
					} else {
						if (debugThis)
							Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").notfound=" + startObject.name + " LENGTH==1");
					}
					
					// same argument .'argument'
					if (actualName.Equals ("'" + startObject.argument+"'")) {
						if (debugThis)
							Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").found=" + startObject.argument + " (argument) LENGTH==1");
			
						// specials
						// todo: unify!
					
						return startObject;
					} else {
						if (debugThis)
							Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").notfound=" + startObject.argument + " (argument) LENGTH==1");
					}
			
					return null;
				} // / length == 1
				else if (strArr.Length > 1) {
			
					if (debugThis)
						Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").notfound=" + startObject.name + " LENGTH==" + strArr.Length);
					// normal ...
					if (actualName.Equals ("" + startObject.name)) {
						// specials ...
						if (CheckForInternalKeywords ("" + strArr [1])) {
							if (debugThis)
								Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").lookat=" + startObject.name + " LENGTH==" + strArr.Length);
			
							return startObject; 
						}
						// / specials
				
						// length>0
						// more nodes
						Node objHere;
						foreach (Node nodeObj in startObject.children) {
							string newPath = "";
							for (int z=1; z<strArr.Length; z++) {
								newPath = newPath + strArr [z];
								if (z != (strArr.Length - 1))
									newPath = newPath + ".";
							}
							objHere = SearchNodeByPathRecursive (nodeObj, newPath);
							if (objHere != null)
								return objHere;
						}
				
						// not found go home
						return null;
				
					} // correct name
					
					// by value 'argument' 
					if (actualName.Equals ("'" + startObject.argument+"'")) {
						// specials ...
						if (CheckForInternalKeywords ("" + strArr [1])) {
							if (debugThis)
								Console.WriteLine ("SearchNodeByPathRecursive(" + startObject.id + "/" + startObject.name + ", " + path + ")(" + actualName + ").lookat= "+startObject.argument + "(argument) LENGTH==" + strArr.Length);
			
							return startObject; 
						}
						// / specials
				
						// length>0
						// more nodes
						Node objHere;
						foreach (Node nodeObj in startObject.children) {
							string newPath = "";
							for (int z=1; z<strArr.Length; z++) {
								newPath = newPath + strArr [z];
								if (z != (strArr.Length - 1))
									newPath = newPath + ".";
							}
							objHere = SearchNodeByPathRecursive (nodeObj, newPath);
							if (objHere != null)
								return objHere;
						}
				
						// not found go home
						return null;
				
					} // correct name
					
				}
			}
			return null;				
		}
						
		/// <summary>
		/// Checks if a sting is an internal keywords.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if the <c>internalAttribute</c> is an internal keyword, else <c>false</c>.
		/// </returns>
		/// <param name='internalAttribute'>
		/// String to check wehter its an internal keywords.
		/// </param>
		///
		// not anymore used
		public bool CheckForInternalKeywords( string internalAttribute ) {
			if (internalAttribute.Equals ("_id")) return true;
			if (internalAttribute.Equals ("_name")) return true;
			if (internalAttribute.Equals ("_length")) return true;
			if (internalAttribute.Equals ("_path")) return true;
			// if (internalAttribute.Equals ("_pathclient")) return true;
			
			if (internalAttribute.Equals ("_objects")) return true;

			// _object_
			if (internalAttribute.Length>8) {
				string strObject=internalAttribute.Substring (0,8);
				if (strObject.Equals ("_object_")) {
					string strNumber=internalAttribute.Substring (8);
					// Console.WriteLine (strNumber);
					try {
						int numb=Convert.ToInt32(strNumber);
						return true;
					}
					catch(Exception e) {
						
					}
				}
		
			}
			return false;
		}
	
		/*
		// SearchNodeByPathRecursiveExtended
		// finds also startobjects
		// fixed
		public Node SearchNodeByPathRecursiveExtended(Node startObject, string path) {
			Console.WriteLine ("SearchNodeByPathRecursiveExtended DONT USE");
			return null;
	
			// ...
			string[] strArr = null;
			char[] splitchar = { '.' };
	        strArr = path.Split(splitchar);
	
			// end node
			if (strArr.Length==1) {
				// direct this?
				// todo: test!
				if (startObject.Equals(strArr[0])) {
					return startObject;
				}
		
				// search for endnode
				Node childObj=startObject.SearchChildByName(strArr[0]);
				if (childObj!=null) return childObj;
			}
	
			// Console
			if (strArr.Length>1) {
				// find ...
				string firstNodeName=strArr[0];
				
				// existing?
				Node childObj=startObject.SearchChildByName(firstNodeName);
				
				if (childObj!=null) {
					// put rest path togheter
					string newPath="";
					for (int i=1;i<strArr.Length;i++) {
						newPath=newPath+strArr[i];
						if (i!=strArr.Length-1) newPath=newPath+".";
						Console.WriteLine ("  path "+newPath);
						return SearchNodeByPathRecursive(childObj,newPath);
					}
				}
			}
	
			// nothing found
			return null;
		}
		*/
		 
		/* todo: get Nodes ...
		public Node SearchNodesByPath(string path) {
			return null;		
		}
		*/
		
		/// <summary>
		/// Searchs all the nodes of a type.
		/// </summary>
		/// <returns>
		/// The nodes of the type <c>typeName</c>
		/// </returns>
		/// <param name='typeName'>
		/// The type of the nodes to find
		/// </param>
		public ArrayList SearchNodesByType( string typeName ) {
			// begin with tree
			return SearchNodesByTypeRecursive(tree, typeName, new ArrayList());
		}

		/// <summary>
		/// Searchs all the nodes of a type.
		/// </summary>
		/// <returns>
		/// The nodes of the type <c>typeName</c>
		/// </returns>
		/// <param name='startNode'>
		/// The node to start the search.
		/// </param>
		/// <param name='typeName'>
		/// The type of the nodes to find
		/// </param>
		/// <param name='arr'>
		/// ArrayList with all sofar found nodes
		/// </param>
		public ArrayList SearchNodesByTypeRecursive(Node startNode, string typeName, ArrayList arr) {
			bool debugThis=false;
			if (debugThis) Console.WriteLine ("SearchNodesByTypeRecursive ("+startNode.name+"/"+startNode.id+")");		
	
			// children
			foreach( Node nodeObj in startNode.children) {
			   SearchNodesByTypeRecursive(nodeObj,typeName, arr);
			}
	
			// end node
			if (startNode.nodeType.Equals (typeName)) {
				if (debugThis) Console.WriteLine ("SearchNodesByTypeRecursive ("+startNode.name+"/"+startNode.id+") found");		
				arr.Add(startNode);
			}
			
			// nothing found
			return arr;
		}
		
		
		/// <summary>
		/// Adds the <c>newNode</c> directly below <c>parentNode</c>
		/// </summary>
		/// <param name='newNode'>
		/// The node to add.
		/// </param>
		/// <param name='parentNode'>
		/// The new parent node.
		/// </param>
		public void AddNodeTo(Node newNode, Node parentNode) {
			// parent
			newNode.SetParent(parentNode);

			// add 
			//MonitorThisNode(parentNode,"add");
			
			// add children
			parentNode.AddChild(newNode);
			
			// add to index
			arrObjectIndex.Add(newNode);
			
			//todo * monitors
			// send changes here to client ...
			// todo: MonitorThisObject(parentNode); 
		}
		
		/// <summary>
		/// Removes a node and all his subnodes recursively
		/// </summary>
		/// <param name='removeThisNode'>
		/// The node to remove
		/// </param>
		public void RemoveNode(Node removeThisNode) {
			//remove all nodes recursively
			for (int i = 0; i<removeThisNode.children.Count; i++) {
				this.RemoveNode((Node)removeThisNode.children[i]);
			}

			// remove on the parents!
			if (removeThisNode.parent != null) {
				Node parentObj = removeThisNode.parent;

				// search and remove here
				parentObj.children.Remove(removeThisNode);
			
				// parent
				removeThisNode.SetParent(null);
			}
			//todo * monitors
			// send changes here to client ...
		}

		/* TODO: monitoring
		// something changed here 
		public void MonitorThisNode( Node monitorThisNode, string changeType ) {
			// update this to all interested nodes
			
			// search for added nodes monitor 5
			
			// change new
			// change update
			// change remove
		}
		*/


		/// <summary>
		/// Generates a string with all nodes 
		/// </summary>
		/// <returns>
		/// a string with all nodes
		/// </returns>
		public string DebugStructure() {
			// start with the tree node 
			string allNodes="";
			
			// allNodes="\n---------------------------\nDebugStructure\n---------------------------\n";
			
			allNodes=allNodes+DebugRecursive(tree, 0);
			
			return allNodes;
		}
		
		/// <summary>
		/// Generates a string with all nodes 
		/// </summary>
		/// <returns>
		/// a string with all nodes
		/// </returns>
		/// <param name='startNode'>
		/// Node to start the listing.
		/// </param>
		/// <param name='depth'>
		/// depth of the recursiv search
		/// </param>
		public string DebugRecursive( Node startNode, int depth ) {
			string str="";
			
			for (int o=0;o<depth;o++) { str=str+"-"; }
			str=str+" "+startNode.id+". ";
			if (startNode.attribute) str=str+"[";
			str=str+startNode.name+":"+startNode.argument+"  ("+startNode.nodeType+"){"+startNode.clientId+"} ";
			if (startNode.attribute) str=str+"]";
			str=str+"\n";
				
			++depth;
					
			// get children 
			foreach( Node nodeObj in startNode.children) {
				str=str+DebugRecursive(nodeObj,depth);
				// str=str+" "*depth;
			}
			return str;	
		}
	}
}

