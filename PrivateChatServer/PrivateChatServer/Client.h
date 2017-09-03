#pragma once

#undef UNICODE

#define WIN32_LEAN_AND_MEAN
#define _WINSOCK_DEPRECATED_NO_WARNINGS

//#include "stdafx.h"
//#include <WinSock2.h>
//#include <Windows.h>
//#include <WS2tcpip.h>
//#include <WinInet.h>
//#include <process.h>
//#include <string>
//#include <list>
//#include <msclr\marshal.h>
//
//#pragma comment(lib, "wininet")	// Need to let the linker know that the project needs the wininet library
//#pragma comment(lib, "Ws2_32.lib")
//
//#define DEFAULT_BUFLEN 512
//
//using namespace System;
//using namespace System::ComponentModel;
//using namespace System::Collections;
//using namespace System::Collections::Generic;
//using namespace System::Windows::Forms;
//using namespace System::Data;
//using namespace System::Drawing;
//using namespace System::Threading;

#define DEFAULT_BUFLEN 512

namespace PrivateChatServer {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Collections::Generic;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::Threading;

	public ref class Client
	{
	public:
		SOCKET sock;
		String^ name, ^phoneNo;
		array<Char>^ buffer = gcnew array<Char>(DEFAULT_BUFLEN);
		int recvbuflen;
		Client(SOCKET s, String^ n, String^ p) : sock(s), name(n), phoneNo(p), recvbuflen(0) {}
	};
}