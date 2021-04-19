#pragma once
/*
 * Copyright (c) 2021. Bert Laverman
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include "framework.h"
#include <SimConnect.h>

//#define CS_SIMCONNECT_DLL_EXPORT	__declspec(dllexport) bool __stdcall
#define CS_SIMCONNECT_DLL_EXPORT	extern "C" __declspec(dllexport) bool

CS_SIMCONNECT_DLL_EXPORT CsConnect(const char* appName, HANDLE& handle);
CS_SIMCONNECT_DLL_EXPORT CsDisconnect(HANDLE handle);
CS_SIMCONNECT_DLL_EXPORT CsCallDispatch(HANDLE handle, DispatchProc callback);
CS_SIMCONNECT_DLL_EXPORT CsGetNextDispatch(HANDLE handle, DispatchProc callback);
CS_SIMCONNECT_DLL_EXPORT CsSubscribeToSystemEvent(HANDLE handle, int id, const char* eventName);
CS_SIMCONNECT_DLL_EXPORT CsRequestSystemState(HANDLE handle, int id, const char* eventName);
