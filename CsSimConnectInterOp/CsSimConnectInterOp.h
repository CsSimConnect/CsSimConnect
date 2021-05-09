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

// The MSFS SDK adds defines for several C++ keywords
#if defined(SIMCONNECT_ENUM)

#define IS_PREPAR3D 0
#define IS_MSFS2020 1

#else

#define IS_PREPAR3D 1
#define IS_MSFS2020 0

#endif

#define CS_SIMCONNECT_DLL_EXPORT_LONG	extern "C" __declspec(dllexport) int64_t
#define CS_SIMCONNECT_DLL_EXPORT_BOOL	extern "C" __declspec(dllexport) bool

CS_SIMCONNECT_DLL_EXPORT_BOOL CsConnect(const char* appName, HANDLE& handle);
CS_SIMCONNECT_DLL_EXPORT_BOOL CsDisconnect(HANDLE handle);
CS_SIMCONNECT_DLL_EXPORT_BOOL CsCallDispatch(HANDLE handle, DispatchProc callback);
CS_SIMCONNECT_DLL_EXPORT_BOOL CsGetNextDispatch(HANDLE handle, DispatchProc callback);
CS_SIMCONNECT_DLL_EXPORT_LONG CsSubscribeToSystemEvent(HANDLE handle, int id, const char* eventName);
CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestSystemState(HANDLE handle, int id, const char* eventName);
CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestDataOnSimObject(HANDLE handle, uint32_t requestId, uint32_t defId, uint32_t objectId, uint32_t period, uint32_t dataRequestFlags,
													   DWORD origin, DWORD interval, DWORD limit);
CS_SIMCONNECT_DLL_EXPORT_LONG CsAddToDataDefinition(HANDLE handle, uint32_t defId, const char *datumName, const char *UnitsName, uint32_t datumType, float epsilon, uint32_t datumId);