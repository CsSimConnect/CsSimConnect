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
		nl::rakis::File logConfig{ "rakisLog.properties" };
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
 * System state handling.
 */

CS_SIMCONNECT_DLL_EXPORT_LONG CsSubscribeToSystemEvent(HANDLE handle, int eventId, const char* eventName) {
	initLog();
	logger.trace("CsSubscribeToSystemEvent(..., {}, '{}'", eventId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsSubscribeToSystemEvent is null!");
		return FALSE;
	}

	HRESULT hr = SimConnect_SubscribeToSystemEvent(handle, eventId, eventName);
	DWORD sendId{ 0 };

	if (SUCCEEDED(hr)) {
		if (FAILED(SimConnect_GetLastSentPacketID(handle, &sendId))) {
			logger.error("Failed to retrieve SendID for SimConnect_SubscribeToSystemEvent(..., {}, '{}') call.", eventId, eventName);
		}
		else {
			logger.debug("Subscribed to system event '{}', EventID={}, SendID={}.", eventName, eventId, sendId);
		}
	}
	else if (hr != E_FAIL) {
		logger.error("Failed to subscribe to system event '{}'. (HRESULT = {})", eventName, hr);
	}
	return SUCCEEDED(hr) ? sendId : hr;
}

CS_SIMCONNECT_DLL_EXPORT_LONG CsRequestSystemState(HANDLE handle, int requestId, const char* eventName) {
	initLog();
	logger.trace("CsRequestSystemState(..., {}, '{}'", requestId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestSystemState is null!");
		return FALSE;
	}

	HRESULT hr = SimConnect_RequestSystemState(handle, requestId, eventName);
	DWORD sendId{ 0 };

	if (SUCCEEDED(hr)) {
		if (FAILED(SimConnect_GetLastSentPacketID(handle, &sendId))) {
			logger.error("Failed to retrieve SendID for SimConnect_SubscribeToSystemEvent(..., {}, '{}') call.", requestId, eventName);
		}
		else {
			logger.debug("Requested system state '{}', RequestID={}, SendID={}.", eventName, requestId, sendId);
		}
	}
	else if (hr != E_FAIL) {
		logger.error("Failed to requedst system state '{}'. (HRESULT = {})", eventName, hr);
	}
	return SUCCEEDED(hr) ? sendId : hr;
}
