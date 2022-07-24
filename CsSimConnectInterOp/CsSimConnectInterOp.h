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

CS_SIMCONNECT_DLL_EXPORT_LONG CsAddClientEventToNotificationGroup(HANDLE handle, uint32_t groupId, uint32_t eventId, uint32_t maskable);
CS_SIMCONNECT_DLL_EXPORT_LONG CsMapClientEventToSimEvent(HANDLE handle, uint32_t eventId, const char* eventName);
CS_SIMCONNECT_DLL_EXPORT_LONG CsMapInputEventToClientEvent(HANDLE handle, uint32_t groupId, const char* inputDefinition, uint32_t downEventId, DWORD downValue, uint32_t upEventId, DWORD upValue, uint32_t maskable);
CS_SIMCONNECT_DLL_EXPORT_LONG CsRemoveClientEvent(HANDLE handle, uint32_t groupId, uint32_t eventId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsTransmitClientEvent(HANDLE handle, uint32_t objectId, uint32_t eventId, uint32_t data, uint32_t groupId, uint32_t flags);

#if IS_PREPAR3D
CS_SIMCONNECT_DLL_EXPORT_LONG CsTransmitClientEvent64(HANDLE handle, uint32_t objectId, uint32_t eventId, uint64_t data, uint32_t groupId, uint32_t flags);
#endif

CS_SIMCONNECT_DLL_EXPORT_LONG CsClearNotificationGroup(HANDLE handle, uint32_t groupId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestNotificationGroup(HANDLE handle, uint32_t groupId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsSetNotificationGroupPriority(HANDLE handle, uint32_t groupId, uint32_t priority);

CS_SIMCONNECT_DLL_EXPORT_LONG CsSubscribeToSystemEvent(HANDLE handle, int id, const char* eventName);
CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestSystemState(HANDLE handle, int id, const char* eventName);

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestDataOnSimObject(HANDLE handle, uint32_t requestId, uint32_t defId, uint32_t objectId, uint32_t period, uint32_t dataRequestFlags,
													   DWORD origin, DWORD interval, DWORD limit);
CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestDataOnSimObjectType(HANDLE handle, uint32_t requestId, uint32_t defId, uint32_t radius, uint32_t objectType);
CS_SIMCONNECT_DLL_EXPORT_LONG CsSetDataOnSimObject(HANDLE handle, uint32_t defId, uint32_t objectId, uint32_t flags, uint32_t count, uint32_t unitSize, void* data);
CS_SIMCONNECT_DLL_EXPORT_LONG CsAddToDataDefinition(HANDLE handle, uint32_t defId, const char* datumName, const char* UnitsName, uint32_t datumType, float epsilon, uint32_t datumId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsClearDataDefinition(HANDLE handle, uint32_t defineId);

CS_SIMCONNECT_DLL_EXPORT_LONG CsAICreateEnrouteATCAircraft(HANDLE handle, const char* title, const char* tailNumber, int flightNumber, const char* flightPlanPath, double flightPlanPosition, uint32_t touchAndGo, uint32_t requestId);
#if IS_PREPAR3D
CS_SIMCONNECT_DLL_EXPORT_LONG CsAICreateEnrouteATCAircraftW(HANDLE handle, const wchar_t* title, const wchar_t* tailNumber, int flightNumber, const wchar_t* flightPlanPath, double flightPlanPosition, uint32_t touchAndGo, uint32_t requestId);
#endif
CS_SIMCONNECT_DLL_EXPORT_LONG CsAICreateNonATCAircraft(HANDLE handle, const char* title, const char* tailNumber, SIMCONNECT_DATA_LATLONALT* pos, SIMCONNECT_DATA_XYZ* pbh, uint32_t onGround, uint32_t airspeed, uint32_t requestId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsAICreateParkedATCAircraft(HANDLE handle, const char* title, const char* tailNumber, const char* airportId, uint32_t requestId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsAICreateSimulatedObject(HANDLE handle, const char* title, SIMCONNECT_DATA_LATLONALT* pos, SIMCONNECT_DATA_XYZ* pbh, uint32_t onGround, uint32_t airspeed, uint32_t requestId);
CS_SIMCONNECT_DLL_EXPORT_LONG CsAIRemoveObject(HANDLE handle, uint32_t objectId, uint32_t requestId);
