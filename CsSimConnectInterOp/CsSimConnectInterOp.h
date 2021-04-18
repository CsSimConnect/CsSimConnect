#pragma once

#include "framework.h"
#include <SimConnect.h>

//#define CS_SIMCONNECT_DLL_EXPORT	__declspec(dllexport) bool __stdcall
#define CS_SIMCONNECT_DLL_EXPORT	extern "C" __declspec(dllexport) bool


CS_SIMCONNECT_DLL_EXPORT CsConnect(const char* appName, HANDLE& handle);
CS_SIMCONNECT_DLL_EXPORT CsDisconnect(HANDLE handle);
CS_SIMCONNECT_DLL_EXPORT CsCallDispatch(HANDLE handle, DispatchProc callback);
CS_SIMCONNECT_DLL_EXPORT CsSubscribeToSystemEvent(HANDLE handle, int id, const char* eventName);
