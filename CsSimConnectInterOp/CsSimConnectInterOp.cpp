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

	HRESULT hr = SimConnect_Open(&handle, appName, nullptr, 0, nullptr, 0);

	if (SUCCEEDED(hr)) {
		logger.info("Connected to SimConnect.");
	}
	else {
		logger.error("Failed to connect to SimConnect");
	}
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT disconnect(HANDLE handle) {
	HRESULT hr = SimConnect_Close(handle);
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT callDispatch(HANDLE handle, DispatchProc callback) {
	HRESULT hr = SimConnect_CallDispatch(handle, callback, nullptr);
	return SUCCEEDED(hr);
}

CS_SIMCONNECT_DLL_EXPORT subscribeToSystemEvent(HANDLE handle, int id, const char* eventName) {
	initLog();

	HRESULT hr = SimConnect_SubscribeToSystemEvent(handle, id, eventName);
	if (SUCCEEDED(hr)) {
		logger.info("Subscribed to system event '{}'", eventName);
	}
	else {
		logger.error("Failed to subscribe to system event '{}'", eventName);
	}
	return SUCCEEDED(hr);
}
