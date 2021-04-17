#include "pch.h"

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

CS_SIMCONNECT_DLL_EXPORT connect(const char* appName, HANDLE& handle) {
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

CS_SIMCONNECT_DLL_EXPORT disconnect(HANDLE handle) {
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

CS_SIMCONNECT_DLL_EXPORT callDispatch(HANDLE handle, DispatchProc callback) {
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

CS_SIMCONNECT_DLL_EXPORT subscribeToSystemEvent(HANDLE handle, int requestId, const char* eventName) {
	initLog();
	logger.debug("subscribeToSystemEvent(..., {}, '{}'", requestId, eventName);
	if (handle == nullptr) {
		logger.error("Handle passed to subscribeToSystemEvent is null!");
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
