#include "pch.h"
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
#include <mutex>


static nl::rakis::Logger logger{ nl::rakis::Logger::getLogger("CsSimConnectInterOp") };

static bool logInitialized{ false };

void initLog() {
	if (!logInitialized) {
		nl::rakis::File logConfig{ "rakisLog2.properties" };
		if (logConfig.exists()) {
			nl::rakis::Logger::configure(logConfig);
		}
		logInitialized = true;
	}
}

static std::mutex scMutex;

/*
 * Lifecycle functions
 */

CS_SIMCONNECT_DLL_EXPORT_BOOL CsConnect(const char* appName, HANDLE& handle) {
	initLog();
	logger.info("Trying to connect through SimConnect using client name '{}'", appName);
	HANDLE h;

//	std::unique_lock<std::mutex> scLock(scMutex);
	HRESULT hr = SimConnect_Open(&h, appName, nullptr, 0, nullptr, 0);

	if (SUCCEEDED(hr)) {
		logger.info("Connected to SimConnect.");
		handle = h;
	}
	else if (hr != E_FAIL) {
		logger.error("Failed to connect to SimConnect");
	}
	else {
		logger.error("Failed to connect to SimConnect (hr={})", hr);
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT_BOOL CsDisconnect(HANDLE handle) {
	initLog();

	std::unique_lock<std::mutex> scLock(scMutex);
	HRESULT hr = SimConnect_Close(handle);

	if (hr != E_FAIL) {
		logger.error("Call to SimConnect_Close() failed.");
	}
	return SUCCEEDED(hr);
}

void* messageHandler;

void CsDispatch(SIMCONNECT_RECV* pData, DWORD cbData, void* pContext)
{
	logger.trace("Received message {}", long(pData->dwID));
	((DispatchProc)messageHandler)(pData, cbData, pContext);
}

CS_SIMCONNECT_DLL_EXPORT_BOOL CsCallDispatch(HANDLE handle, DispatchProc callback) {
	initLog();
	messageHandler = callback;
	logger.debug("Calling CallDispatch()");

	HRESULT hr = SimConnect_CallDispatch(handle, CsDispatch, nullptr);

	if (hr != E_FAIL) {
		logger.error("Dispatch failed (HRESULT = {}).", hr);
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT_BOOL CsGetNextDispatch(HANDLE handle, DispatchProc callback) {
	initLog();
	logger.trace("Calling GetNextDispatch()");

	SIMCONNECT_RECV* msgPtr;
	DWORD msgLen;
	HRESULT hr = SimConnect_GetNextDispatch(handle, &msgPtr, &msgLen);

	if (SUCCEEDED(hr)) {
		logger.trace("Dispatching message {}", long(msgPtr->dwID));
		callback(msgPtr, msgLen, nullptr);
	}
	else if (hr != E_FAIL) {
		logger.error("Could not get a new message (HRESULT = {}).", hr);
	}
	return SUCCEEDED(hr);
}

/*
 * Utilities
 */

long fetchSendId(HANDLE handle, HRESULT hr, const char* api)
{
	DWORD sendId{ 0 };

	if (SUCCEEDED(hr)) {
		if (FAILED(SimConnect_GetLastSentPacketID(handle, &sendId))) {
			logger.error("Failed to retrieve SendID for '{}' call.", api);
		}
	}
	return SUCCEEDED(hr) ? sendId : hr;
}

/*
 * Client Event handling.
 */

CS_SIMCONNECT_DLL_EXPORT_LONG CsAddClientEventToNotificationGroup(HANDLE handle, uint32_t groupId, uint32_t eventId, uint32_t maskable) {
	initLog();

	logger.trace("CsAddClientEventToNotificationGroup(..., {}, {}, {})", groupId, eventId, maskable);
	if (handle == nullptr) {
		logger.error("Handle passed to CsAddClientEventToNotificationGroup is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_AddClientEventToNotificationGroup(handle, groupId, eventId, maskable), "AddClientEventToNotificationGroup");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsMapClientEventToSimEvent(HANDLE handle, uint32_t eventId, const char* eventName) {
	initLog();

	logger.trace("CsMapClientEventToSimEvent(..., {}, '{}')", eventId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsMapClientEventToSimEvent is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_MapClientEventToSimEvent(handle, eventId, eventName), "MapClientEventToSimEvent");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsMapInputEventToClientEvent(HANDLE handle, uint32_t groupId, const char* inputDefinition, uint32_t downEventId, DWORD downValue, uint32_t upEventId, DWORD upValue, uint32_t maskable) {
	initLog();

	logger.trace("CsMapInputEventToClientEvent(..., {}, '{}', {}, {}, {}, {}, {})", groupId, inputDefinition, downEventId, downValue, upEventId, upValue, maskable);
	if (handle == nullptr) {
		logger.error("Handle passed to CsMapInputEventToClientEvent is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_MapInputEventToClientEvent(handle, groupId, inputDefinition, downEventId, downValue, upEventId, upValue, maskable), "MapInputEventToClientEvent");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRemoveClientEvent(HANDLE handle, uint32_t groupId, uint32_t eventId) {
	initLog();

	logger.trace("CsRemoveClientEvent(..., {}, {})", groupId, eventId);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRemoveClientEvent is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_RemoveClientEvent(handle, groupId, eventId), "RemoveClientEvent");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsTransmitClientEvent(HANDLE handle, uint32_t objectId, uint32_t eventId, uint32_t data, uint32_t groupId, uint32_t flags) {
	initLog();

	logger.trace("CsTransmitClientEvent(..., {}, {}, {}, {}, {})", objectId, eventId, data, groupId, flags);
	if (handle == nullptr) {
		logger.error("Handle passed to CsTransmitClientEvent is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_TransmitClientEvent(handle, objectId, eventId, data, groupId, flags), "TransmitClientEvent");
}

#if IS_PREPAR3D

CS_SIMCONNECT_DLL_EXPORT_LONG CsTransmitClientEvent64(HANDLE handle, uint32_t objectId, uint32_t eventId, uint64_t data, uint32_t groupId, uint32_t flags) {
	initLog();

	logger.trace("CsTransmitClientEvent64(..., {}, {}, {}, {}, {})", objectId, eventId, data, groupId, flags);
	if (handle == nullptr) {
		logger.error("Handle passed to CsTransmitClientEvent64 is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_TransmitClientEvent64(handle, objectId, eventId, data, groupId, flags), "TransmitClientEvent");
}

#endif

/*
 * Client Data handling.
 */

CS_SIMCONNECT_DLL_EXPORT_LONG CsAddToClientDataDefinition(HANDLE handle, uint32_t defId, DWORD offset, int32_t sizeOrType, float epsilon, DWORD datumId)
{
	initLog();

	logger.trace("CsAddToClientDataDefinition(..., {}, {}, {}, {}, {})", defId, offset, sizeOrType, epsilon, datumId);
	if (handle == nullptr) {
		logger.error("Handle passed to CsAddToClientDataDefinition is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_AddToClientDataDefinition(handle, defId, offset, sizeOrType, epsilon, datumId), "AddToClientDataDefinition");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsCreateClientData(HANDLE handle, uint32_t clientDataId, DWORD size, uint32_t flags)
{
	initLog();

	logger.trace("CsCreateClientData(..., {}, {}, {})", clientDataId, size, flags);
	if (handle == nullptr) {
		logger.error("Handle passed to CsCreateClientData is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_CreateClientData(handle, clientDataId, size, flags), "CreateClientData");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsMapClientDataNameToID(HANDLE handle, const char* clientDataName, uint32_t clientDataId) {
	initLog();

	logger.trace("CsMapClientDataNameToID(..., '{}', {})", clientDataName, clientDataId);
	if (handle == nullptr) {
		logger.error("Handle passed to CsMapClientDataNameToID is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_MapClientDataNameToID(handle, clientDataName, clientDataId), "MapClientDataNameToID");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestClientData(HANDLE handle, uint32_t clientDataId, uint32_t requestId, uint32_t defineId, uint32_t period, uint32_t flags, DWORD origin, DWORD interval, DWORD limit)
{
	initLog();

	logger.trace("CsRequestClientData(..., {}, {}, {}, {}, {}, {}, {}, {})", clientDataId, requestId, defineId, period, flags, origin, interval, limit);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestClientData is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_RequestClientData(handle, clientDataId, requestId, defineId, SIMCONNECT_CLIENT_DATA_PERIOD(period), flags, origin, interval, limit), "RequestClientData");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsSetClientData(HANDLE handle, uint32_t clientDataId, uint32_t defineId, DWORD flags, DWORD unitSize, void* dataSet) {
	initLog();

	logger.trace("CsSetClientData(..., {}, {}, {}, ..., {}, ...)", clientDataId, defineId, flags, unitSize);
	if (handle == nullptr) {
		logger.error("Handle passed to CsSetClientData is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_SetClientData(handle, clientDataId, defineId, flags, 0, unitSize, dataSet), "SetClientData");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsClearClientDataDefinition(HANDLE handle, uint32_t clientDataId) {
	initLog();

	logger.trace("CsClearClientDataDefinition(..., {})", clientDataId);
	if (handle == nullptr) {
		logger.error("Handle passed to CsClearClientDataDefinition is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_ClearClientDataDefinition(handle, clientDataId), "ClearClientDataDefinition");
}

/*
 * System state handling.
 */

CS_SIMCONNECT_DLL_EXPORT_LONG CsSubscribeToSystemEvent(HANDLE handle, int eventId, const char* eventName) {
	initLog();

	logger.trace("CsSubscribeToSystemEvent(..., {}, '{}')", eventId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsSubscribeToSystemEvent is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_SubscribeToSystemEvent(handle, eventId, eventName), "SubScribeToSystemEvent");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestSystemState(HANDLE handle, int requestId, const char* eventName) {
	initLog();

	logger.trace("CsRequestSystemState(..., {}, '{}'", requestId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestSystemState is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_RequestSystemState(handle, requestId, eventName), "RequestSystemState");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestDataOnSimObject(HANDLE handle, uint32_t requestId, uint32_t defId, uint32_t objectId, uint32_t period, uint32_t dataRequestFlags,
	DWORD origin, DWORD interval, DWORD limit)
{
	initLog();

	logger.info("CsRequestDataOnSimObject(..., {}, {}, {}, {}, {}, {}, {}, {})", requestId, defId, objectId, period, dataRequestFlags, origin, interval, limit);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestDataOnSimObject is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_RequestDataOnSimObject(handle, requestId, defId, objectId, SIMCONNECT_PERIOD(period), dataRequestFlags, origin, interval, limit), "RequestDataOnSimObject");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestDataOnSimObjectType(HANDLE handle, uint32_t requestId, uint32_t defineId, uint32_t radius, uint32_t objectType) {
	initLog();

	logger.trace("CsRequestDataOnSimObjectType(..., {}, {}, {}, {})", requestId, defineId, radius, objectType);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestDataOnSimObjectType is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_RequestDataOnSimObjectType(handle, requestId, defineId, radius, SIMCONNECT_SIMOBJECT_TYPE(objectType)), "RequestDataOnSimObjectType");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsSetDataOnSimObject(HANDLE handle, uint32_t defId, uint32_t objectId, uint32_t flags, uint32_t count, uint32_t unitSize, void* data)
{
	initLog();

	logger.info("CsSetDataOnSimObject(..., {}, {}, {}, {}, {}, ...)", defId, objectId, flags, count, unitSize);
	if (handle == nullptr) {
		logger.error("Handle passed to CsSetDataOnSimObject is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_SetDataOnSimObject(handle, defId, objectId, flags, count, unitSize, data), "SetDataOnSimObject");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsAddToDataDefinition(HANDLE handle, uint32_t defId, const char* datumName, const char* unitsName, uint32_t datumType, float epsilon, uint32_t datumId)
{
	initLog();

	logger.trace("CsAddToDataDefinition(..., {}, {}, {}, {}, {}, {})", defId, datumName, unitsName, datumType, epsilon, datumId);
	if (handle == nullptr) {
		logger.error("Handle passed to CsAddToDataDefinition is null!");
		return FALSE;
	}

	if ((unitsName != nullptr) && (strcmp(unitsName, "NULL") == 0)) {
		unitsName = nullptr;
	}
	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_AddToDataDefinition(handle, defId, datumName, unitsName, SIMCONNECT_DATATYPE(datumType), epsilon, datumId), "AddToDataDefinition");
}

/*
 * AI
 */

CS_SIMCONNECT_DLL_EXPORT_LONG CsAICreateParkedATCAircraft(HANDLE handle, const char* title, const char* tailNumber, const char* airportId, uint32_t requestId)
{
	initLog();

	logger.info("CsAICreateParkedATCAircraft(..., '{}', '{}', '{}', {})", title, tailNumber, airportId, requestId);
	if (handle == nullptr) {
		logger.error("Handle passed to CsAICreateParkedATCAircraft is null!");
		return FALSE;
	}

	std::unique_lock<std::mutex> scLock(scMutex);
	return fetchSendId(handle, SimConnect_AICreateParkedATCAircraft(handle, title, tailNumber, airportId, requestId), "AICreateParkedATCAircraft");
}
