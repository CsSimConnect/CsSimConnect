#pragma once

#include "framework.h"
#include <SimConnect.h>

#define CS_SIMCONNECT_DLL_EXPORT	__declspec(dllexport) bool __stdcall


CS_SIMCONNECT_DLL_EXPORT connect(const char* appName, HANDLE& handle);
CS_SIMCONNECT_DLL_EXPORT disconnect(HANDLE handle);
CS_SIMCONNECT_DLL_EXPORT callDispatch(HANDLE handle, DispatchProc callback);
CS_SIMCONNECT_DLL_EXPORT subscribeToSystemEvent(HANDLE handle, int id, const char* eventName);
