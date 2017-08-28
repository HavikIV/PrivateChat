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

#pragma comment(lib, "wininet")	// Need to let the linker know that the project needs the wininet library
#pragma comment(lib, "Ws2_32.lib")

namespace PrivateChatServer {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Collections::Generic;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::Threading;

	public class Client
	{
		SOCKET sock;
		std::string name, phoneNo;
		Client(SOCKET s, std::string n, std::string p) : sock(s), name(n), phoneNo(p) {}
	};

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


	private: System::Windows::Forms::Label^  lblLIP;
	private: System::Windows::Forms::TextBox^  Error;




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
			this->lblLIP = (gcnew System::Windows::Forms::Label());
			this->Error = (gcnew System::Windows::Forms::TextBox());
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
			this->lblLocal->Location = System::Drawing::Point(114, 96);
			this->lblLocal->Name = L"lblLocal";
			this->lblLocal->Size = System::Drawing::Size(90, 13);
			this->lblLocal->TabIndex = 3;
			this->lblLocal->Text = L"Local IP Address:";
			// 
			// lblPIP
			// 
			this->lblPIP->AutoSize = true;
			this->lblPIP->Location = System::Drawing::Point(294, 39);
			this->lblPIP->Name = L"lblPIP";
			this->lblPIP->Size = System::Drawing::Size(35, 13);
			this->lblPIP->TabIndex = 4;
			this->lblPIP->Text = L"label3";
			// 
			// lblLIP
			// 
			this->lblLIP->AutoSize = true;
			this->lblLIP->Location = System::Drawing::Point(294, 95);
			this->lblLIP->Name = L"lblLIP";
			this->lblLIP->Size = System::Drawing::Size(35, 13);
			this->lblLIP->TabIndex = 5;
			this->lblLIP->Text = L"label4";
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
			// MainWindow
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(512, 339);
			this->Controls->Add(this->Error);
			this->Controls->Add(this->lblLIP);
			this->Controls->Add(this->lblPIP);
			this->Controls->Add(this->lblLocal);
			this->Controls->Add(this->lblPublic);
			this->Controls->Add(this->btnStop);
			this->Controls->Add(this->btnStart);
			this->Name = L"MainWindow";
			this->Text = L"MainWindow";
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

private: bool shouldStop;

private: List<List<SOCKET>^> connections;

private: System::Void StartServer(System::Object^  sender, System::EventArgs^  e)
{
	//	Lets disable the Start button as the user should be allowed to clicked it again once the server as started
	btnStart->Enabled = false;

	shouldStop = false;

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
	iResult = getaddrinfo("192.168.1.4", "1986", &hints, &result);
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

			//	Lets added the socket to the list of connections
			if (connections.Count == 0)
			{
				//	Create a new list and add the socket to it
				connections.Add(gcnew List<SOCKET>());
				connections[0]->Add(client);
			}
			else
			{
				for each (List<SOCKET>^ list  in connections)
				{
					if (list->Count < 64)
					{
						list->Add(client);
						break;	//	Break out of the for each loop
					}

					//	Checked every list and they are full, so need to create a new one
					if (connections[connections.Count] == list)
					{
						//	Create a new list and add the socket to it
						connections.Add(gcnew List<SOCKET>());
						connections[0]->Add(client);
					}
				}
			}

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
}
};
}
