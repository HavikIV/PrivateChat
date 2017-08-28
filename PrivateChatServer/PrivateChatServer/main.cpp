// PrivateChatServer.cpp : main project file.

#include "stdafx.h"
#include "MainWindow.h"
#include <Windows.h>
#include <Windows.Applicationmodel.Activation.h>

using namespace System;
using namespace System::Windows::Forms;

int CALLBACK WinMain(_In_  HINSTANCE hInstance, _In_  HINSTANCE hPrevInstance, _In_  LPSTR lpCmdLine, _In_  int nCmdShow)
{
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false);
	PrivateChatServer::MainWindow form;
	Application::Run(%form);
	return 0;
}
