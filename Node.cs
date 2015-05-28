using System;
using System.Collections;

namespace RehabConnex {

	/// <summary>
	/// represents a node in the tree structure of the server
	/// </summary>
	public class Node {
		public int id=-1; //!< node id
		public int clientId=-1; //!< clientId of the owner of this node (-1 = root!)
								// todo: implement for simpler access control!
								// -1: root!
		public string name=""; //!< node name
		
		// todo: or more as attributes ?
		public bool accessOtherClients=false; //!< <c>true</c> allows non-owner-clients to change this node		
		
		public bool attribute=false; //!< <c>true</c> if this node is an attribute of its parent node
								    // example: chair-color (attribute)
								    // example: chair-personsitting (not an attribute)		
		public string type=""; 		//!< not used
		public string typeSub="";  	//!< not used
		
		// not used!
		public string nodeType=""; 	//!< defines the type of the node (float/object/list ...)
		public string argument=""; 	//!< argument of this node
		
		// internal arguments for speed performance
		public Boolean argumentTypedBoolean=true; //!< internal argument type boolean
		public int argumentTypedInt=0; //!< internal argument type int
		public int argumentTypedFloat=0; //!< internal argument type float
		public string argumentTypedString=""; //!< internal argument type string
	
		public object argumentTypedObject; //!< not used

		public int sortIndex=0; //!< sortindex for slots

		// not used if structure is a tree
		// public int client=-1; // owner of the client [-1 serverobject etc ... ]
		
		// add the objects here 
		// like config-connections
		// public ArrayList objects = new ArrayList();

		// tree structure
		public Node parent; //!< parent of this node
		public ArrayList children; //!< ArrayList with all the children of this node

		// virtual node
		public bool virtualNode = false; //!< is <c>true</c> if node is virtual

		//temp
		private Node nodeObjTemp; //!< temp. cache for a node

		/// <summary>
		/// Initializes a new instance of the <see cref="RehabConnex.Node"/> class. (constructor)
		/// </summary>
		public Node() {
			children = new ArrayList();	
		}
		
		/// <summary>
		/// Converts argument of nodeType string to typed argument.
		/// </summary>
		public void convertToTypedArgument() {
			if (nodeType=="string") {
				argumentTypedString=argument;	
			}
		}

		/// <summary>
		/// Converts typed argument to normal argument.
		/// </summary>
		public void convertToArgument() {
			if (nodeType=="float") {
				argument=""+argumentTypedFloat;	
			}
			if (nodeType=="int") {
				argument=""+argumentTypedInt;	
			}
			if (nodeType=="string") {
				argument=""+argumentTypedString;	
			}
		}
		
		
		/// <summary>
		/// Sets the parent of this node.
		/// </summary>
		/// <param name='parentObj'>
		/// The Parent object.
		/// </param>
		public void SetParent( Node parentObj ) {
			parent=parentObj;	
		}

		/// <summary>
		/// Adds an node as child of this node
		/// </summary>
		/// <param name='childObj'>
		/// Child node.
		/// </param>
		public void AddChild( Node childObj ) { 
			children.Add(childObj);	
		}
		
		/// <summary>
		/// Searches the child node with the name <c>childName</c>
		/// </summary>
		/// <returns>
		/// The reference to the first found object, if none was found it returns null
		/// </returns>
		/// <param name='childName'>
		/// Name of the child to search for.
		/// </param>
		public Node SearchChildByName( string childName ) {
			foreach( Node nodeObj in children) {
				if (nodeObj.name.Equals(childName)) {
					return nodeObj;	
				}
			}
			return null;
		}
		
		/// <summary>
		/// Searchs the children nodes with the name <c>childName</c> 
		/// </summary>
		/// <returns>
		/// ArrayList with all the children with the name <c>childName</c>, if none was found it returns the arraylist is empty
		/// </returns>
		/// <param name='childName'>
		/// Name of the children to search for.
		/// </param>
		public ArrayList SearchChildrenByName( string childName ) {
			ArrayList arr=new ArrayList();
			foreach( Node nodeObj in children) {
				if (nodeObj.name.Equals(childName)) {
					arr.Add(nodeObj);	
				}
			}
			return arr;
		}
	}
}


