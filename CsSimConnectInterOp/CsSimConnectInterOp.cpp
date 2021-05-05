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

CS_SIMCONNECT_DLL_EXPORT_BOOL CsConnect(const char* appName, HANDLE& handle) {
	initLog();
	logger.info("Trying to connect through SimConnect using client name '{}'", appName);
	HANDLE h;
	HRESULT hr = SimConnect_Open(&h, appName, nullptr, 0, nullptr, 0);

	if (SUCCEEDED(hr)) {
		logger.info("Connected to SimConnect.");
		handle = h;
	}
	else if (hr != E_FAIL) {
		logger.error("Failed to connect to SimConnect");
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT_BOOL CsDisconnect(HANDLE handle) {
	initLog();
	HRESULT hr = SimConnect_Close(handle);

	if (SUCCEEDED(hr)) {
		logger.info("Disconnected from SimConnect.");
	}
	else if (hr != E_FAIL) {
		logger.error("Failed to disconnect from SimConnect");
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

	if (SUCCEEDED(hr)) {
		logger.trace("Dispatch succeeded.");
	}
	else if (hr != E_FAIL) {
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
 * System state handling.
 */

CS_SIMCONNECT_DLL_EXPORT_LONG CsSubscribeToSystemEvent(HANDLE handle, int eventId, const char* eventName) {
	initLog();
	logger.trace("CsSubscribeToSystemEvent(..., {}, '{}'", eventId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsSubscribeToSystemEvent is null!");
		return FALSE;
	}

	return fetchSendId(handle, SimConnect_SubscribeToSystemEvent(handle, eventId, eventName), "SubScribeToSystemEvent");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestSystemState(HANDLE handle, int requestId, const char* eventName) {
	initLog();
	logger.trace("CsRequestSystemState(..., {}, '{}'", requestId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestSystemState is null!");
		return FALSE;
	}

	return fetchSendId(handle, SimConnect_RequestSystemState(handle, requestId, eventName), "RequestSystemState");
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestDataOnSimObject(HANDLE handle, uint32_t requestId, uint32_t defId, uint32_t objectId, uint32_t period, uint32_t dataRequestFlags,
	DWORD origin, DWORD interval, DWORD limit)
{
	initLog();
	logger.trace("CsRequestDataOnSimObject(..., {}, {}, {}, {}, {}, {}, {}, {})", requestId, defId, objectId, period, dataRequestFlags, origin, interval, limit);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestDataOnSimObject is null!");
		return FALSE;
	}

	return fetchSendId(handle, SimConnect_RequestDataOnSimObject(handle, requestId, defId, objectId, SIMCONNECT_PERIOD(period), dataRequestFlags, origin, interval, limit), "RequestDataOnSimObject");
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
	return fetchSendId(handle, SimConnect_AddToDataDefinition(handle, defId, datumName, unitsName, SIMCONNECT_DATATYPE(datumType), epsilon, datumId), "AddToDataDefinition");
}
