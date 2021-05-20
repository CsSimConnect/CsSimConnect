# CsSimConnectInterOp - A Layer between the SimConnect Library and Managed Code

The `CsSimConnectInterOp.dll` library is a C++ layer between the (static) SimConnect library and Managed Code.
The intent is to keep it as thin as possible, but provide useful logging and translation functions as needed.

## Building the InterOp DLL

If you want to use the commandline tools of Visual Studio to build this DLL, use:

```
C:\dev\FS\CsSimConnect\CsSimConnectInterOp> msbuild CsSimConnectInterOp.vcxproj -property:Configuration=<config>
```

Where Config is one of:
| Configuration | Required env var | Goal |
| :--- | :--- | :--- |
| P3Dv4_Debug | P3D45_SDK | `..\P3Dv4\CsSimConnectInterOp.dll` |
| P3Dv4_Release | P3D45_SDK | `..\dist\P3Dv4\CsSimConnectInterOp.dll` |
| P3Dv5_Debug | P3D51_SDK | `..\P3Dv5\CsSimConnectInterOp.dll` |
| P3Dv5_Release | P3D51_SDK | `..\dist\P3Dv5\CsSimConnectInterOp.dll` |
| MSFS2020_Debug | MSFS_SDK | `..\MSFS\CsSimConnectInterOp.dll` |
| MSFS2020_Release | MSFS_SDK | `..\dist\MSFS\CsSimConnectInterOp.dll` |


## Function interfaces and DLL exports

A DLL in Windows is essentially an executable with a customized entry point defined in the
[`dllmain.cpp`](./dllmain.cpp) module. This entry point (called "`dllmain`") differs from the usual "`main()`"
function in that it receives information on the sharing mode under which it was invoked, which can involve
a new process or a new thread, and wether it is an "attach" or "detach" event. Once attached, the client
process (or thread) can call the library's exported functions by name or number.

The easiest way to match exported functions is by name, but then the DLL must use easily recognizable names,
which isn't always the case for modern languages. When a language supports overloading of functions, or is
Object Oriented, the compiler will "mangle" the full name. Mangled names work well when the client is written
in the same language, but in our case the client is in C#. An simple alternative is to use the entry number, which
can be fixed by including a "`.DEF`" file when building the DLL. The approach chosen here is to force the compiler
_not_ to mangle the names, by delaring them as "`extern "C"`", because C does not support overloading.

## Matching errors with Requests

Because some errors won't be known until the simulator has processed the request, they are generally reported
asynchronously, using a "`SIMCONNECT_RECV_EXCEPTION`" message. This message includes a "`dwSendID`" field,
which has no relation to all the other Ids used. To help matching requests with exceptions, the InterOp layer
will perform a `SimConnect_GetLastSentPacketID()` call. The return value of all calls witll therefore be:

* Less than zero to return a direct error value,
* Zero to indicate an error that has no error code associated, which typically means an invalid or null handle,
* One to indicate a success that has no `PacketSendID` associated with it, or
* A `PacketSendID` value if higher than one.