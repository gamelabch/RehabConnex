using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;

using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;

/*
 * # RehabConnex #
 * 
 * RehabConnex is a server/client system similair to OSC but more flexibel and TCP-based. It is used in rehabilitation.
 * 
 * http://rehabconnex.zhdk.ch
 * 
 * TODOS
 * - Exception Handler! for GET/SET
 * 
 * KNOWNBUGS
 * - Patches to remove clients > point to server! > remove them!!!
 * 
 * Version 0.84
 * 0.84 new command insertoutputslotat
 * 0.83 patches to patch.pushpull possible now (logs) & patches to patch.auto!
 * 0.82 bug fix automatic patches ... 
 * 0.81 set jx path value > possible for input slots to write direct to it (config todo: only for config slots!)
 * 0.8 patch something > sending last value
 * 0.76 Added some more infos/commands on start up
 * 0.75 BugFix - "get","parent","" > crash (problem no parent-object there!)
 * 
 * */

namespace RehabConnex {

	/// <summary>
	/// This class handels the pool of all connected clients.
	/// </summary>	
	class ClientConnectionPool {
	    private Queue SyncdQ = Queue.Synchronized( new Queue() ); //!< Creates a synchronized wrapper around the Queue.
	  	
		/// <summary>
		/// The Enqueue method adds a client into the synchronized client queue.
		/// </summary>
		/// <param name=client>The client which is added to the queue.</param>
	    public void Enqueue(ClientHandler client) {
	        SyncdQ.Enqueue(client) ;
	    }

		/// <summary>
		/// The Dequeue method removes the next client form the queue of the client pool.
		/// </summary>
		/// <returns>A Clienthandler instance with the last client form the client pool.</returns>
	    public ClientHandler Dequeue() {
	        return (ClientHandler) ( SyncdQ.Dequeue() ) ;
	    }

		/// <summary>
		/// Contains the number of clients from the client pool.
		/// </summary>
	    public int Count {
	        get { return SyncdQ.Count ; }
	    }

		/// <summary>
		/// Contains the root of the client queue.
		/// </summary>
	    public object SyncRoot {
	  		get { return SyncdQ.SyncRoot ; }
	    }
	        
	} //end class ClientConnectionPool

	/// <summary>
	/// This class processes all clients within the ConnectionPool.
	/// </summary>	
	class ClientService {
	        
		const int NUM_OF_THREAD = 30; //!< Thread array length

		private ClientConnectionPool ConnectionPool; //!< Connection Pool
		private bool ContinueProcess = false; //!< Defines if the service is listening for clients
		private Thread[] ThreadTask  = new Thread[NUM_OF_THREAD]; //!< Array with the threads for the connections of the clients

		/// <summary>
		/// Initializes a new instance of the <see cref="RehabConnex.ClientService"/> class.
		/// </summary>
		/// <param name='ConnectionPool'>
		/// Reference to the pool that contains all the clients that are connected at the moment
		/// </param>  
	    public ClientService(ClientConnectionPool ConnectionPool) {
	    	this.ConnectionPool = ConnectionPool ;
	    }        

		/// <summary>
		/// Starts the processing of the clients within the <see cref="RehabConnex.ClientConnectionPool"/>.
		/// </summary>
	    public void Start() {
		    ContinueProcess = true;
		    // Start threads to handle Client Task
		    for ( int i = 0 ; i < ThreadTask.Length ; i++) {
	            ThreadTask[i] = new Thread( new ThreadStart(this.Process) );
	            ThreadTask[i].Start();
		    }
	    }
	        
		/// <summary>
		/// Process the next client in the <see cref="RehabConnex.ClientConnectionPool"/> queue. As long as there are clients in the queue the next client is processed and then readded to the queue.
		/// </summary>
		private void Process()  {
			while ( ContinueProcess ) {	
				ClientHandler client = null;
				lock( ConnectionPool.SyncRoot ) {
					if  ( ConnectionPool.Count > 0 )
						client = ConnectionPool.Dequeue();
				}		                         
				if ( client != null ) {
					client.Process() ; // Provoke client
					// if client still connect, schedufor later processingle it 
					if ( client.Alive ) 
						ConnectionPool.Enqueue(client);
				}
			    Thread.Sleep(10) ;
			}         
		}

		/// <summary>
		/// Stop the processing of the clients within the <see cref="RehabConnex.ClientConnectionPool"/>. It dequeues all clients and terminates the connection.
		/// </summary>
		public void Stop() {
	        ContinueProcess = false;        
	        for ( int i = 0 ; i < ThreadTask.Length ; i++) {
	            if ( ThreadTask[i] != null &&  ThreadTask[i].IsAlive )  
	                ThreadTask[i].Join() ;
	        }
	                
	        // Close all client connections
	        while ( ConnectionPool.Count > 0 ) {
	            ClientHandler client = ConnectionPool.Dequeue();
	            client.Close(); 
	            Console.WriteLine("Client connection is closed!");
	        }
		}      
	} //end class ClientService


	/// <summary>
	/// Contains the main entrypoint of the application.
	/// </summary>
	public class SynchronousSocketListener {

		/// <summary>
		/// Instantiates and starts the <see cref="RehabConnex.ConfigService"/> and loads the config file.
		/// </summary>
		public static void StartListening() {
			
			// ConfigService
			ConfigService configService = new ConfigService();
			
			// Load Config 
			configService.LoadConfigFile();
			
			// Start it
			configService.Start();		
		} 
	  
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		/// <returns>
		/// The exit code that is given to the operating system after the program ends.
		/// </returns>
		public static int Main(String[] args) { 
			
			/*
			 *  TCP-IP CONFIG CONNECTIONS
			 */
			// start tcp ip listening
			StartListening();

			/*
			 * ROUTING ETC
			 * TCPIP/UDP
			 */
		
		    return 0;
		}
	} //end class SynchronousSocketListener

}
