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

CS_SIMCONNECT_DLL_EXPORT CsConnect(const char* appName, HANDLE& handle) {
	initLog();
	logger.info("Trying to connect through SimConnect using client name '{}'", appName);
	HANDLE h;
	HRESULT hr = SimConnect_Open(&h, appName, nullptr, 0, nullptr, 0);

	if (SUCCEEDED(hr)) {
		logger.info("Connected to SimConnect.");
		handle = h;
	}
	else {
		logger.error("Failed to connect to SimConnect");
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT CsDisconnect(HANDLE handle) {
	initLog();
	HRESULT hr = SimConnect_Close(handle);

	if (SUCCEEDED(hr)) {
		logger.info("Disconnected from SimConnect.");
	}
	else {
		logger.error("Failed to disconnect from SimConnect");
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT CsCallDispatch(HANDLE handle, DispatchProc callback) {
	initLog();
	logger.debug("Calling CallDispatch()");

	HRESULT hr = SimConnect_CallDispatch(handle, callback, nullptr);

	if (SUCCEEDED(hr)) {
		logger.trace("Dispatch succeeded.");
	}
	else {
		logger.error("Dispatch failed (HRESULT = {}).", hr);
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT CsSubscribeToSystemEvent(HANDLE handle, int requestId, const char* eventName) {
	initLog();
	logger.debug("CsSubscribeToSystemEvent(..., {}, '{}'", requestId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsSubscribeToSystemEvent is null!");
		return false;
	}

	HRESULT hr = SimConnect_SubscribeToSystemEvent(handle, requestId, eventName);

	if (SUCCEEDED(hr)) {
		logger.trace("Subscribed to system event '{}'", eventName);
	}
	else {
		logger.error("Failed to subscribe to system event '{}'. (HRESULT = {})", eventName, hr);
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT CsRequestSystemState(HANDLE handle, int requestId, const char* eventName) {
	initLog();
	logger.debug("CsRequestSystemState(..., {}, '{}'", requestId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to CsRequestSystemState is null!");
		return false;
	}

	HRESULT hr = SimConnect_RequestSystemState(handle, requestId, eventName);

	if (SUCCEEDED(hr)) {
		logger.trace("Requested system state '{}'", eventName);
	}
	else {
		logger.error("Failed to requedst system state '{}'. (HRESULT = {})", eventName, hr);
	}
	return SUCCEEDED(hr);
}
