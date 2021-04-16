# CsSimConnect - A new SimConnect library for C#

This project will become a full SimConnect interface in modern C#, not using old programming models with Windows handles and such.

## Structure of this repository

There are three sub-projects:
* [CsSimConnectInterOp](CsSimConnectInterOp/) is a non-managed C++ DLL that provides an interface to the static SimConnect library.
* [CsSimConnect](CsSimConnect/) is the actual C# Class Library, using the .Net 5.0 platform.
* [CsSimConnectUI](CsSimConnectUI/) is a test/demo app, showing how to use the library.

## How to build and run the demo app

You need to build the `CsSimConnectInterOp.dll` project using Visual Studio 2019, but once you have that, you can build the C# parts
also from the commandline using the [.Net 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0).

From the commandline, use the provided "`publish.cmd`" script:
```
C:\dev\FS\CsSimConnect>.\publish

C:\dev\FS\CsSimConnect>rd /s /q dist
The system cannot find the file specified.

C:\dev\FS\CsSimConnect>md dist

C:\dev\FS\CsSimConnect>copy rakisLog.properties dist\
        1 file(s) copied.

C:\dev\FS\CsSimConnect>copy x64\Release\CsSimConnectInterOp.dll dist\
        1 file(s) copied.

C:\dev\FS\CsSimConnect>dotnet publish -o dist --force -c Release -p:PublishReadyToRun=true --self-contained false -r win10-x64
Microsoft (R) Build Engine version 16.9.0+57a23d249 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  Restored C:\dev\FS\CsSimConnect\CsSimConnectUI\CsSimConnectUI.csproj (in 104 ms).
  Restored C:\dev\FS\CsSimConnect\CsSimConnect\CsSimConnect.csproj (in 104 ms).
  CsSimConnect -> C:\dev\FS\CsSimConnect\CsSimConnect\bin\Release\net5.0-windows\win10-x64\CsSimConnect.dll
  CsSimConnect -> C:\dev\FS\CsSimConnect\dist\
  CsSimConnect -> C:\dev\FS\CsSimConnect\CsSimConnect\bin\Release\net5.0-windows\CsSimConnect.dll
  CsSimConnectUI -> C:\dev\FS\CsSimConnect\CsSimConnectUI\bin\Release\net5.0-windows\win10-x64\CsSimConnectUI.dll
  CsSimConnectUI -> C:\dev\FS\CsSimConnect\dist\

C:\dev\FS\CsSimConnect>
```

The .Net runtime is not included, so you need to install that first.