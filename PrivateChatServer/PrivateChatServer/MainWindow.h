#pragma once

#undef UNICODE

#define WIN32_LEAN_AND_MEAN
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "stdafx.h"
#include <WinSock2.h>
#include <Windows.h>
#include <WS2tcpip.h>
#include <WinInet.h>
#include <process.h>
#include <string>
#include <list>
#include <msclr\marshal.h>
#include "Client.h"

#pragma comment(lib, "wininet")	// Need to let the linker know that the project needs the wininet library
#pragma comment(lib, "Ws2_32.lib")

#define DEFAULT_BUFLEN 512

//class Client
//{
//	SOCKET sock;
//	std::string name, phoneNo;
//	char buffer[DEFAULT_BUFLEN];
//	int recvbuflen;
//public: Client(SOCKET s, std::string n, std::string p) : sock(s), name(n), phoneNo(p), recvbuflen(0) {}
//};
//
//std::list<std::list<Client>> connections;

namespace PrivateChatServer {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Collections::Generic;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::Threading;

	// static unsigned __stdcall ListenThread(void* data)
	//{
	//	int iResult;
	//	DATA* d = (DATA*)data;
	//	SOCKET listenSocket = d->l;
	//	addrinfo* result = d->r;

	//	SOCKET client = INVALID_SOCKET;

	//	//	Bind the listening socket
	//	iResult = bind(listenSocket, result->ai_addr, (int)result->ai_addrlen);

	//	//	Check for errors
	//	if (iResult == SOCKET_ERROR)
	//	{
	//		//OutputLog("bind() failed with error: " + WSAGetLastError());
	//		freeaddrinfo(result);
	//		closesocket(listenSocket);
	//		WSACleanup();
	//		return 1;
	//	}

	//	//	Need to free address information obtained from the getaddrinfo() function call as it's no longer needed
	//	freeaddrinfo(result);

	//	//	Listen on the socket
	//	iResult = listen(listenSocket, SOMAXCONN);

	//	//	Check for errors
	//	if (iResult == SOCKET_ERROR)
	//	{
	//		//OutputLog("listen() failed with error: " + WSAGetLastError());
	//		closesocket(listenSocket);
	//		WSACleanup();
	//		return 1;
	//	}

	//	while (client = accept(listenSocket, NULL, NULL))
	//	{
	//		//	Check for errors
	//		if (client == INVALID_SOCKET)
	//		{
	//			closesocket(listenSocket);
	//			WSACleanup();
	//			return 1;
	//		}

	//		//	Change the client Socket to be nonblocking
	//	}
	//}

	/// <summary>
	/// Summary for MainWindow
	/// </summary>
	public ref class MainWindow : public System::Windows::Forms::Form
	{
		//	This delegate should help in outputting logs to from the other running treads
		delegate void logDelegate(String^ message);

	public:
		MainWindow(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//

			// Grab the User's Public IP Address from icanhazip.com
			HINTERNET hInternet, hFile;
			DWORD rSize;
			char buffer[32];

			hInternet = InternetOpen(NULL, INTERNET_OPEN_TYPE_PRECONFIG, NULL, NULL, 0);
			hFile = InternetOpenUrl(hInternet, "http://icanhazip.com/", NULL, 0, INTERNET_FLAG_RELOAD, 0);
			InternetReadFile(hFile, buffer, sizeof(buffer), &rSize);
			buffer[rSize] = '\0';

			InternetCloseHandle(hFile);
			InternetCloseHandle(hInternet);

			lblPIP->Text = gcnew String(buffer); // Display the Public IP Address
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MainWindow()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Button^  btnStart;
	private: System::Windows::Forms::Button^  btnStop;
	private: System::Windows::Forms::Label^  lblPublic;
	protected:

	protected:


	private: System::Windows::Forms::Label^  lblLocal;
	private: System::Windows::Forms::Label^  lblPIP;



	private: System::Windows::Forms::TextBox^  Error;
	private: System::Windows::Forms::TextBox^  txtIP;
	private: System::Windows::Forms::Label^  lblPort;
	private: System::Windows::Forms::TextBox^  txtPort;





	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->btnStart = (gcnew System::Windows::Forms::Button());
			this->btnStop = (gcnew System::Windows::Forms::Button());
			this->lblPublic = (gcnew System::Windows::Forms::Label());
			this->lblLocal = (gcnew System::Windows::Forms::Label());
			this->lblPIP = (gcnew System::Windows::Forms::Label());
			this->Error = (gcnew System::Windows::Forms::TextBox());
			this->txtIP = (gcnew System::Windows::Forms::TextBox());
			this->lblPort = (gcnew System::Windows::Forms::Label());
			this->txtPort = (gcnew System::Windows::Forms::TextBox());
			this->SuspendLayout();
			// 
			// btnStart
			// 
			this->btnStart->Location = System::Drawing::Point(117, 148);
			this->btnStart->Name = L"btnStart";
			this->btnStart->Size = System::Drawing::Size(75, 23);
			this->btnStart->TabIndex = 0;
			this->btnStart->Text = L"Start Server";
			this->btnStart->UseVisualStyleBackColor = true;
			this->btnStart->Click += gcnew System::EventHandler(this, &MainWindow::StartServer);
			// 
			// btnStop
			// 
			this->btnStop->Enabled = false;
			this->btnStop->Location = System::Drawing::Point(297, 148);
			this->btnStop->Name = L"btnStop";
			this->btnStop->Size = System::Drawing::Size(75, 23);
			this->btnStop->TabIndex = 1;
			this->btnStop->Text = L"Stop Server";
			this->btnStop->UseVisualStyleBackColor = true;
			this->btnStop->Click += gcnew System::EventHandler(this, &MainWindow::StopServer);
			// 
			// lblPublic
			// 
			this->lblPublic->AutoSize = true;
			this->lblPublic->Location = System::Drawing::Point(114, 40);
			this->lblPublic->Name = L"lblPublic";
			this->lblPublic->Size = System::Drawing::Size(93, 13);
			this->lblPublic->TabIndex = 2;
			this->lblPublic->Text = L"Public IP Address:";
			// 
			// lblLocal
			// 
			this->lblLocal->AutoSize = true;
			this->lblLocal->Location = System::Drawing::Point(114, 71);
			this->lblLocal->Name = L"lblLocal";
			this->lblLocal->Size = System::Drawing::Size(90, 13);
			this->lblLocal->TabIndex = 3;
			this->lblLocal->Text = L"Local IP Address:";
			// 
			// lblPIP
			// 
			this->lblPIP->AutoSize = true;
			this->lblPIP->Location = System::Drawing::Point(294, 40);
			this->lblPIP->Name = L"lblPIP";
			this->lblPIP->Size = System::Drawing::Size(35, 13);
			this->lblPIP->TabIndex = 4;
			this->lblPIP->Text = L"label3";
			// 
			// Error
			// 
			this->Error->Location = System::Drawing::Point(12, 177);
			this->Error->Multiline = true;
			this->Error->Name = L"Error";
			this->Error->ReadOnly = true;
			this->Error->ScrollBars = System::Windows::Forms::ScrollBars::Both;
			this->Error->Size = System::Drawing::Size(488, 150);
			this->Error->TabIndex = 7;
			// 
			// txtIP
			// 
			this->txtIP->Location = System::Drawing::Point(297, 68);
			this->txtIP->Name = L"txtIP";
			this->txtIP->Size = System::Drawing::Size(100, 20);
			this->txtIP->TabIndex = 8;
			// 
			// lblPort
			// 
			this->lblPort->AutoSize = true;
			this->lblPort->Location = System::Drawing::Point(114, 107);
			this->lblPort->Name = L"lblPort";
			this->lblPort->Size = System::Drawing::Size(69, 13);
			this->lblPort->TabIndex = 9;
			this->lblPort->Text = L"Port Number:";
			// 
			// txtPort
			// 
			this->txtPort->Location = System::Drawing::Point(297, 104);
			this->txtPort->Name = L"txtPort";
			this->txtPort->Size = System::Drawing::Size(100, 20);
			this->txtPort->TabIndex = 10;
			// 
			// MainWindow
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(512, 339);
			this->Controls->Add(this->txtPort);
			this->Controls->Add(this->lblPort);
			this->Controls->Add(this->txtIP);
			this->Controls->Add(this->Error);
			this->Controls->Add(this->lblPIP);
			this->Controls->Add(this->lblLocal);
			this->Controls->Add(this->lblPublic);
			this->Controls->Add(this->btnStop);
			this->Controls->Add(this->btnStart);
			this->Name = L"PrivateChatServer";
			this->Text = L"PrivateChat";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

private: void OutputLog(String ^logMessage)
{
	if (Error->InvokeRequired)
	{
		logDelegate^ d = gcnew logDelegate(this, &MainWindow::OutputLog);
		Invoke(d, gcnew array<Object^>{logMessage});
	}
	else
	{
		Error->AppendText(logMessage + "\r\n");
	}
}

private: HANDLE hThread;

private: bool shouldStop, shouldWait;

private: List<List<Client^>^> connections;

private: System::Void StartServer(System::Object^  sender, System::EventArgs^  e)
{
	//	Let's make sure the textboxes are filled out before continuing
	if (txtIP->Text == "" || txtPort->Text == "")
	{
		MessageBox::Show("Either the IP Address or Port Number wasn't provided.");
		OutputLog("Provide the necessary information (Local IP Address and Port Number) before starting the Server.");
		return;
	}

	//	Lets disable the Start button as the user should be allowed to clicked it again once the server as started
	btnStart->Enabled = false;

	//	Disable the textboxes for the Local IP address and Port Number
	txtIP->Enabled = false;
	txtPort->Enabled = false;

	shouldStop = false;
	shouldWait = false;

	////	Lets start the server, meaning lets start listening for connections
	//OutputLog("Start Server Button clicked");

	////	Create a WSADATA object
	//WSADATA wsaData;

	////	SOCKET for listening to any incoming attempts to connect to the server
	//SOCKET listenSocket = INVALID_SOCKET;

	////	Declaring an addrinfo object, hints used for providing for what kind of IP address to get, result will be populated by getaddrinfo()
	//struct addrinfo *result = NULL, hints;

	////	Used to see what is returned by the function calls
	//int iResult;

	////	Initialize WinSock, we're requesting for version 2.2 of WinSock to be started
	//iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);

	////	Check for errors
	//if (iResult != 0)
	//{
	//	//	Something went wrong, so lets inform the user of the error
	//	OutputLog("WSAStartup failed with error: " + iResult); // Leave it like this for now, TODO: Add an error reporting function that will print the error on the form
	//	WSACleanup();	// Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
	//	return;	// End the function call right here
	//}

	//ZeroMemory(&hints, sizeof(hints));	//	ZeroMemory -> fills a block a memory with zeros; Used to initialize all members of the hints structure to 0;
	//hints.ai_family = AF_INET;			//	The address family is IPv4
	//hints.ai_socktype = SOCK_STREAM;	//	SOCK_STREAM provides a sequenced, reliable, two-way, connection-based byte streams with an OOB data transmission mechanism. Uses TCP
	//hints.ai_protocol = IPPROTO_TCP;	//	Specifies Transmission Control Protocol (TCP)
	//hints.ai_flags = AI_PASSIVE;		//	AI_PASSIVE flag indicates that the caller intends to use the returned socket address structure in a call to the bind function.

	////	Resolve the local address and port to be used by the server
	//iResult = getaddrinfo("192.168.1.4", "1986", &hints, &result);

	////	Check for errors
	//if (iResult != 0)
	//{
	//	OutputLog("getaddrinfo() failed with error: " + iResult);
	//	WSACleanup();
	//	return;
	//}

	////	TODO: move listening on the socket to a different thread to prevent the UI from being locked up

	////	Create the listening socket
	//listenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);

	////	Check for errors
	//if (listenSocket == INVALID_SOCKET)
	//{
	//	OutputLog("socket() failed with error: " + WSAGetLastError());
	//	freeaddrinfo(result);
	//	WSACleanup();
	//	return;
	//}

	////	Bind the listening socket
	//iResult = bind(listenSocket, result->ai_addr, (int)result->ai_addrlen);

	////	Check for errors
	//if (iResult == SOCKET_ERROR)
	//{
	//	OutputLog("bind() failed with error: " + WSAGetLastError());
	//	freeaddrinfo(result);
	//	closesocket(listenSocket);
	//	WSACleanup();
	//	return;
	//}

	////	Need to free address information obtained from the getaddrinfo() function call as it's no longer needed
	//freeaddrinfo(result);

	////	Listen on the socket
	//iResult = listen(listenSocket, SOMAXCONN);

	////	Check for errors
	//if (iResult == SOCKET_ERROR)
	//{
	//	OutputLog("listen() failed with error: " + WSAGetLastError());
	//	closesocket(listenSocket);
	//	WSACleanup();
	//	return;
	//}

	//	Stop button should now be Enabled
	btnStop->Enabled = true;

	/*DATA* d = new DATA(listenSocket, result);

	unsigned threadID;
	hThread = (HANDLE)_beginthreadex(NULL, 0, &ListenThread, (LPVOID *)d, 0, &threadID);
*/
	Thread^ thread = gcnew Thread(gcnew ThreadStart(this, &MainWindow::StartListening));
	thread->Start();
}

private: System::Void StartListening()
{
	OutputLog("Starting the worker thread to handle the incoming connections.");

	//	Create a WSADATA object
	WSADATA wsaData;

	//	SOCKET for listening to any incoming attempts to connect to the server
	SOCKET listenSocket = INVALID_SOCKET;

	//	Declaring an addrinfo object, hints used for providing for what kind of IP address to get, result will be populated by getaddrinfo()
	struct addrinfo *result = NULL, hints;

	//	Used to see what is returned by the function calls
	int iResult;

	//	Initialize WinSock, we're requesting for version 2.2 of WinSock to be started
	iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	//	Check for errors
	if (iResult != 0)
	{
		//	Something went wrong, so lets inform the user of the error
		OutputLog("WSAStartup failed with error: " + iResult); // Leave it like this for now, TODO: Add an error reporting function that will print the error on the form
		WSACleanup();	// Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
		return;	// End the function call right here
	}
	OutputLog("Initialized WinSock version 2.2.");

	ZeroMemory(&hints, sizeof(hints));	//	ZeroMemory -> fills a block a memory with zeros; Used to initialize all members of the hints structure to 0;
	hints.ai_family = AF_INET;			//	The address family is IPv4
	hints.ai_socktype = SOCK_STREAM;	//	SOCK_STREAM provides a sequenced, reliable, two-way, connection-based byte streams with an OOB data transmission mechanism. Uses TCP
	hints.ai_protocol = IPPROTO_TCP;	//	Specifies Transmission Control Protocol (TCP)
	hints.ai_flags = AI_PASSIVE;		//	AI_PASSIVE flag indicates that the caller intends to use the returned socket address structure in a call to the bind function.

	//	Resolve the local address and port to be used by the server
	msclr::interop::marshal_context mc;	//	Using the msclr/marshal library to convert String^ to PCSTR for getaddrinfo()
	iResult = getaddrinfo(mc.marshal_as<PCSTR>(txtIP->Text), mc.marshal_as<PCSTR>(txtPort->Text), &hints, &result);
	//	Check for errors
	if (iResult != 0)
	{
		OutputLog("getaddrinfo() failed with error: " + iResult);
		WSACleanup();					//	Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
		return;
	}
	OutputLog("getaddrinfo() resolved the local IP address.");

	//	Create the listening socket
	listenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
	//	Check for errors
	if (listenSocket == INVALID_SOCKET)
	{
		OutputLog("socket() failed with error: " + WSAGetLastError());
		freeaddrinfo(result);			//	Frees address information that getaddrinfo() dynamically allocated into the addrinfo structure result
		WSACleanup();					//	Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
		return;
	}
	OutputLog("Socket for listening created.");

	//	Bind the listening socket
	iResult = bind(listenSocket, result->ai_addr, (int)result->ai_addrlen);
	//	Check for errors
	if (iResult == SOCKET_ERROR)
	{
		OutputLog("bind() failed with error: " + WSAGetLastError());
		freeaddrinfo(result);			//	Frees address information that getaddrinfo() dynamically allocated into the addrinfo structure result
		closesocket(listenSocket);
		WSACleanup();					//	Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
		return;
	}
	OutputLog("The Socket was binded successfully.");

	//	Need to free address information obtained from the getaddrinfo() function call as it's no longer needed
	freeaddrinfo(result);

	//	Listen on the socket
	//	listen() takes in the created socket and the maximum length of a queue of pending connections to accept.
	//	SOMAXXONN is a special constant to allow a maximum reasonable number of pending connections
	iResult = listen(listenSocket, SOMAXCONN);
	//	Check for errors
	if (iResult == SOCKET_ERROR)
	{
		OutputLog("listen() failed with error: " + WSAGetLastError());
		closesocket(listenSocket);
		WSACleanup();					//	Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
		return;
	}
	OutputLog("Start listening on the Socket.");

	//	Lets make the listening socket nonblocking
	u_long iMode = 1;
	ioctlsocket(listenSocket, FIONBIO, &iMode);

	SOCKET client;
	sockaddr_in sinRemote;
	int addrSize = sizeof(sinRemote);
	//	TODO:	USE SELECT ON THE LISTENSOCKET SO THAT ACCEPT() IS ONLY CALLED IF THERE'S A INCOMING CONNECTION AND
	//			THE SERVER DOESN'T END UP WAITING FOR A CONNECTION TO BE MADE EVEN AFTER THE USER CLICKED ON STOP SERVER.

	//	This while loop will handle accepting any and all incoming connections to the server.
	//	Each accepted connection will be added to a list to maximum of 64 sockets.
	//	It will also handle creating additional worker threads that will handle any actions on the accepted connections.
	while (!shouldStop && (client = accept(listenSocket, (sockaddr*)&sinRemote, &addrSize)))
	{
		//	Check for errors except for the 10035 error which occurs when there's no incoming connection at time of calling accept on a nonblocking socket
		if (client == INVALID_SOCKET && (WSAGetLastError() != 10035))
		{
			OutputLog("accept() failed with error: " + WSAGetLastError());
			closesocket(listenSocket);
			WSACleanup();				//	Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
			return;
		}

		//	This portion should only run if an connection was accepted
		if (client != INVALID_SOCKET)
		{
			OutputLog("Accepted connection from " + gcnew String(inet_ntoa(sinRemote.sin_addr)) + ":" + ntohs(sinRemote.sin_port) + ".");

			//	Lets receive the phone number of the newly connected user
			char buffer[512];
			memset(buffer, '\0', sizeof(char) * 512);
			int bytes;
			do
			{
				bytes = recv(client, buffer, 512, 0);
			} while (bytes == -1);

			//	Check for errors
			if (bytes == 0)
			{
				// connection was closed
				OutputLog("The client was closed.");
			}
			else if (bytes == SOCKET_ERROR)
			{
				OutputLog("Error receiving the client's phone number.");
			}

			OutputLog("The client's phone number is: " + gcnew String(buffer));
			
			//	Change the socket to be nonblocking
			ioctlsocket(client, FIONBIO, &iMode);

			//	TODO: BUFFER MANIPLUATION: NEED TO EXTRACT THE PHONE NUMBER AND CLIENT'S NAME SO CAN PASS THEM IN THE CLIENT OBJECT PROPERLY
			//	USE strncpy_s TO EXTRACT THE NESSECCARY INFORMATION

			char phone[11];
			memset(phone, 0, 11);	//	Set phone to all 0's
			strncpy_s(phone, buffer, 10);	//	Extract the phone number from buffer and place it into phone
			
			// Remove the phone number from the buffer so that only the client's name will be left in it
			memmove(buffer, buffer + 11, bytes - 11);

			// Making sure we don't read the any of the garbage left in the buffer after the memmove call
			buffer[bytes - 11] = '\0'; // This will place a single null char to indicate the end of the message; This is faster as array access if O(1) time and no loop iteration needed
			//memset(buffer + (bytes - 11), '\0', (sizeof(char) * bytes)); // This will place null chars after the message; This is a bit slower has it has to iterate through a loop

			Client^ c = gcnew Client(client, gcnew String(buffer), gcnew String(phone));

			//	Lets added the socket to the list of connections
			if (connections.Count == 0)
			{
				//	Create a new list and add the socket to it
				connections.Add(gcnew List<Client^>());
				connections[0]->Add(c);

				//	Need to start a new worker thread that will handle all of the clients in this list
				Thread^ thread = gcnew Thread(gcnew ParameterizedThreadStart(this, &MainWindow::HandleClients));
				thread->Start(connections[0]);
			}
			else
			{

				//	Lets see if this connection was added previously and now only the socket needs to be updated
				Client^ old = FindClient(std::string(phone));

				if (old->phoneNo != "" && old->sock == INVALID_SOCKET)
				{
					//	Lets update the previously added connection with the new socket
					old->sock = c->sock;
					OutputLog("Updated the socket for " + c->name);
				}
				else
				{
					for each (List<Client^>^ list  in connections)
					{
						if (list->Count < 64)
						{
							list->Add(c);
							break;	//	Break out of the for each loop
						}

						//	Checked every list and they are full, so need to create a new one
						if (connections[connections.Count - 1] == list)
						{
							//	Create a new list and add the socket to it
							connections.Add(gcnew List<Client^>());
							connections[connections.Count - 1]->Add(c);

							//	Need to start a new thread that will handle all of the clients in this list
							Thread^ thread = gcnew Thread(gcnew ParameterizedThreadStart(this, &MainWindow::HandleClients));
							thread->Start(connections[connections.Count - 1]);
						}
					}
				}
			}
		}

		//	TODO:	Recieve the message containing the user's phone and full name, add the accepted connection a list of connections,
		//			start a new worker thread for each list that's not empty if needed (when the socket in each is added)

		////	Create three FD sets that will be used to see if there's any activity on the Listen Socket
		//fd_set readFDs, exceptFDs;

		////	FD_ZERO(*set) is a macro used to initialize the given set to NULL
		//FD_ZERO(&readFDs);
		////FD_ZERO(&writeFDs);
		//FD_ZERO(&exceptFDs);

		//if (listenSocket == INVALID_SOCKET)
		//{
		//	//	FD_SET(s, *set) is a macro used to add the given socket to the given set
		//	FD_SET(listenSocket, &exceptFDs);
		//}
		//else
		//{
		//	//	FD_SET(s, *set) is a macro used to add the given socket to the given set
		//	FD_SET(listenSocket, &readFDs);
		//}

		////	Lets call the select() method to see if there's any incoming connections
		//if (select(0, &readFDs, NULL, &exceptFDs, 0) > 0)
		//{
		//	//	Check to see if the Listening socket is in the read fd_set
		//	//	FD_ISSET(s, *set) is a macro used to see if the given socket is in the given fd_set
		//	if (FD_ISSET(listenSocket, &readFDs))
		//	{
		//		sockaddr_in sinRemote;
		//		int addrSize = sizeof(sinRemote);

		//		SOCKET client = accept(listenSocket, (sockaddr*)&sinRemote, &addrSize);

		//		//	Check for errors
		//		if (client == INVALID_SOCKET)
		//		{
		//			OutputLog("accept() failed with error: " + WSAGetLastError());
		//			closesocket(listenSocket);
		//			WSACleanup();				//	Terminates the use of the WinSock 2 DLL (Ws2_32.dll)
		//			return;
		//		}
		//		OutputLog("Accepted connection from " + gcnew String(inet_ntoa(sinRemote.sin_addr)) + ":" + ntohs(sinRemote.sin_port) + ".");

		//		//	TODO:	Recieve the message containing the user's phone and full name, add the accepted connection a list of connections,
		//		//			start a new worker thread for each list that's not empty if needed (when the socket in each is added)
		//	}
		//	else if (FD_ISSET(listenSocket, &exceptFDs))
		//	{
		//		int err;
		//		int errLen = sizeof(err);
		//		getsockopt(listenSocket, SOL_SOCKET, SO_ERROR, (char*)&err, &errLen);
		//		OutputLog("Exception on listening socket: " + err);
		//		break;
		//	}
		//}
	}

	OutputLog("Closing the sockets in the listening thread.");
	//	Close the sockets before exiting the function
	closesocket(listenSocket);
	closesocket(client);
	OutputLog("Sockets closed in the listening thread.");

	//	Terminates the use of the WinSock 2 DLL (Ws2_32.sll)
	WSACleanup();
	OutputLog("Terminated the use of the WinSock 2 DLL and now closing listening thread.");
}

//	This function will run in a separate thread and use select() on the sockets of the given clients to send and receive messages.
private: System::Void HandleClients(Object^ obj)
{
	List<Client^>^ list = (List<Client^>^)obj;
	while (!shouldStop)
	{
		while (shouldWait)
		{
			//	Since one of the other worker threads require the full use of the list of clients this thread should wait till it's done working
			//	I should try to make this to be randomize instead to prevent any one thread from monopolizing the CPU
			System::Threading::Thread::Sleep(1000);
		}

		//	Lets set up the FD_SETS here before continuing
		//	Create three FD sets that will be used to see if there's any activity on the Listen Socket
		fd_set readFDs, writeFDs, exceptFDs;

		//	Move the clients in to the appropriate FD_SETS for the select() method
		SetupFDSets(readFDs, writeFDs, exceptFDs, list);

		//	Need to tell the other threads know that they should wait for this thread to finish it's work first
		shouldWait = true;

		bool empty = allEmpty(readFDs, writeFDs, exceptFDs);

		//	Call the select() method to start handling the work to be done for each of the clients
		if (select(0, &readFDs, &writeFDs, &exceptFDs, 0) > 0 && !empty)
		{
			//	Iterate through each of the clients to determine what needs to be done for each of them
			for each (Client^ c in list)
			{
				bool ok = true;

				//	Check to see if there's any errors on the socket
				if (FD_ISSET(c->sock, &exceptFDs))
				{
					ok = false;
					OutputLog("Something went wrong on the server");

					//	Remove the socket from the exceptFDs
					FD_CLR(c->sock, &exceptFDs);
				}
				else
				{
					//	Check if the client's socket is ready to receive a message
					if (FD_ISSET(c->sock, &readFDs))
					{
						//	Try to receive a message
						ok = ReadMsg(c);

						//	Remove the socket from the readFDs
						FD_CLR(c->sock, &readFDs);

						//	TO CONVERT FROM array<wchar_t>^ to char*, use reinterpret_cast<char*>(char*)
						/*pin_ptr<Char> x = &c->buffer[0];
						Char* p = x;
						char* buffer = reinterpret_cast<char*>(p);*/
					}

					//	Check if the client's socket is ready to send out a message
					if (FD_ISSET(c->sock, &writeFDs))
					{
						//	Send out a message
						ok = SendMsg(c);

						//	Remove the socket from the writeFDs
						FD_CLR(c->sock, &writeFDs);
					}
				}

				if (!ok)
				{
					//	Something went wrong with the client's socket so lets deal with it here before moving on
					int err;
					int errLen = sizeof(err);
					getsockopt(c->sock, SOL_SOCKET, SO_ERROR, (char*)&err, &errLen);

					if (err != NO_ERROR)
					{
						//	Lets print out what the error was
						OutputLog("Error code: " + WSAGetLastError());
					}
					else
					{
						//	Connection was closed on the client side, so lets inform the user
						OutputLog("The connection was closed for socket: " + c->sock);

					}

					//	Lets close the socket
					closesocket(c->sock);
					c->sock = INVALID_SOCKET;	//	Setting the socket descriptor to INVALID_SOCKET to avoid running into to problems
				}
			}
		}
		else
		{
			// Since not all three of the fd_sets were empty when select() was called, unexpected error occurred so output the log.
			// An error is expected when all three sets are empty when passed to select so don't need to output a log.
			if (!empty)
			{
				OutputLog("select() failed with error code: " + WSAGetLastError());
			}
		}

		//	Since this thread is finished doing it's work for this iteration of the while loop, inform the other threads that they can continue
		shouldWait = false;

		//	To prevent this thread from trying to claim the work time for itself again going to have it wait for 1 second before moving on
		System::Threading::Thread::Sleep(1000);
	}

	//	Since the server is going offline, need to perform some cleanup
	//	Close of the client sockets that this thread is in charge of
	OutputLog("Closing all of the client sockets.");
	for each (Client^ c in list)
	{
		closesocket(c->sock);
	}
	OutputLog("Closed all of the client sockets.");

	//	Lets delete the list completely so that it will be repopulated when the server is online again
	list->Clear();
	OutputLog("Cleared all of the clients in the list.");
	connections.Remove(list);
}

private: bool allEmpty(fd_set &read, fd_set &write, fd_set &except)
{
	return ((read.fd_count == 0) && (write.fd_count == 0) && (except.fd_count == 0));
}

//	This method will move the Client sockets into the appropriate fd_sets for the select() method
private: System::Void SetupFDSets(fd_set &read, fd_set &write, fd_set &except, List<Client^>^ list)
{
	//	FD_ZERO(*set) is a macro used to initialize the given set to NULL
	FD_ZERO(&read);
	FD_ZERO(&write);
	FD_ZERO(&except);

	//	Lets place the client sockets into the proper fd_sets
	for each (Client^ c in list)
	{
		//	Check to make sure the socket is a valid before proceeding
		if (c->sock != INVALID_SOCKET)
		{
			//	Check if there's an space left in the buffer, if there is then pay attention for incoming messages
			if (c->recvbuflen < DEFAULT_BUFLEN)
			{
				//	Place the socket in the read fd_set using FD_SET(s, *set)
				FD_SET(c->sock, &read);
			}

			//	Check to see if there's any messages in the buffer that need to be sent out
			if (c->recvbuflen > 0)
			{
				//	Place the socket in the write fd_set using FD_SET(s, *set)
				FD_SET(c->sock, &write);
			}

			//	If the client socket isn't either ready to read or send messages, then it goes into the except set
			FD_SET(c->sock, &except);
		}
	}
}

//	This method is called to try to receive any incoming message from the given Client
//	The method returns false if there errors, but if the error is WSAWOULDBLOCK then it
//	will return true as if the socket received a message successfully
private: System::Boolean ReadMsg(Client^ c)
{
	//	Read the message
	char buffer[DEFAULT_BUFLEN];
	memset(buffer, '\0', sizeof(char)* DEFAULT_BUFLEN);
	int bytes = recv(c->sock, buffer, DEFAULT_BUFLEN - c->recvbuflen, 0);

	//	Check for errors
	if (bytes == 0)
	{
		//	Connection was closed
		return false;
	}
	else if (bytes == SOCKET_ERROR)
	{
		int err;
		int errlen = sizeof(err);

		getsockopt(c->sock, SOL_SOCKET, SO_ERROR, (char*)&err, &errlen);

		//	Check to see if the error was WSASHOULDBLOCK
		if (err != WSAEWOULDBLOCK)
		{
			OutputLog("Something went wrong while receiving a message. Error code: " + err);
		}

		return (err == WSAEWOULDBLOCK);
	}

	OutputLog("Message received.");
	c->recvbuflen = bytes;
	//	Convert from char* to array<wchar_t>^
	System::Runtime::InteropServices::Marshal::Copy(IntPtr(&buffer), c->buffer, 0, bytes);
	
	return true;
}

//	This method is called to try to send out the message that is in the given Client's buffer
//	The method returns false if there errors, but if the error is WSAWOULDBLOCK then it
//	will return true as if the socket was able to send a message successfully
private: System::Boolean SendMsg(Client^ c)
{
	//	buffer that will hold the message to send out
	pin_ptr<Char> x = &c->buffer[0];
	Char* p = x;
	char* buffer = reinterpret_cast<char*>(p);

	// Create a copy of the message so we can extract the recipient phone number(s) without altering the message
	char* temp = new char[strlen(buffer) + 1];
	strcpy(temp, buffer);
	int tempLen = c->recvbuflen;


	//	Find out who the message is intended for
	int recipients = temp[0] - 48;	//	Number of recipients of the message (ASCII CHAR VALUE MINUS 48 WILL GIVE THE RIGHT INTEGER VALUE)
	//	Shift the message down a byte
	memmove(temp, temp + 2, tempLen - 2);
	tempLen -= 2;	//	Update tempLen to reflect the extraction

	//	Place the recipient's phone number in here, max group size is 5 ATM
	std::string recipient[5];
	for (int i = 0; i < recipients; i++)
	{
		char str[15];
		memset(str, 0, 10);
		strncpy_s(str, temp, 10);
		recipient[i] = std::string(str);

		// Remove the phone number from the buffer before the next iteration/moving on
		memmove(temp, temp + 11, tempLen - 11);
		tempLen -= 11;	//	Update recvbuflen to reflect the extraction
	}

	//	Since the all of the phone numbers have been extracted now only the message should be left in the buffer, so lets send out it out to the intended recipients

	int mostBytesSent = 0;

	//	Send out the message to the intended recipient(s)
	for (int i = 0; i < recipients; i++)
	{
		// Lets find the intended recipient from the list of connections
		Client^ r = FindClient(recipient[i]);

		if (r->name != "")
		{
			//	Send out the message to the client
			int bytes = send(r->sock, buffer, c->recvbuflen, 0);

			if (bytes > mostBytesSent)
			{
				mostBytesSent = bytes;
			}

			//	Check for errors
			if (bytes == SOCKET_ERROR)
			{
				int err;
				int errLen = sizeof(err);

				getsockopt(r->sock, SOL_SOCKET, SO_ERROR, (char*)&err, &errLen);

				if (err != WSAEWOULDBLOCK)
				{
					OutputLog("send() failed with error code: " + err);
				}

				return (err == WSAEWOULDBLOCK);
			}
		}
	}

	//	Check to see if the entire message was sent out or not
	if (mostBytesSent == c->recvbuflen)
	{
		//	Everything in the buffer was sent out to the recipient so need to clear out the sender's buffer and reset it recvbuflen back to 0
		c->buffer = gcnew array<Char>(DEFAULT_BUFLEN);
		c->recvbuflen = 0;
	}
	else
	{
		//	Since only a part of the message was sent out, need to remove the part that was sent out and update the sender's recvbuflen to reflect the change
		char* b;
		strcpy(b, buffer);
		memmove(b, b + mostBytesSent, c->recvbuflen - mostBytesSent);

		//	Convert from char* to array<wchar_t>^ and place the unsent message in the sender's buffer
		System::Runtime::InteropServices::Marshal::Copy(IntPtr(&b), c->buffer, 0, c->recvbuflen - mostBytesSent);

		c->recvbuflen -= mostBytesSent;
	}

	OutputLog("Message was sent out to the recipient(s).");

	return true;
}

//	This method will search the entire list of connections for any client that matches the given phone number
private: Client^ FindClient(std::string s)
{
	for each (List<Client^>^ l in connections)
	{
		for each (Client^ c in l)
		{
			if (c->phoneNo == gcnew String(s.c_str()))
			{
				//	Found the client so need to return it to the calling calling method
				return c;
			}
		}
	}

	//	Didn't find a client with the given phone number, so will return an empty client
	return gcnew Client(INVALID_SOCKET, gcnew String(""), gcnew String(""));
}

private: System::Void StopServer(System::Object^  sender, System::EventArgs^  e)
{
	//	Disable the Stop button
	btnStop->Enabled = false;

	OutputLog("Letting the worker threads know that they should stop working as the server is going offline.");

	//	Close the handle to the thread handling listening on a socket
	//CloseHandle(hThread);

	//	The while loop can exit in the listening thread
	shouldStop = true;

	//	Enable the Start button
	btnStart->Enabled = true;
	//	Enable the textboxes for the Local IP address and Port Number
	txtIP->Enabled = true;
	txtPort->Enabled = true;
}
};
}
