# CsSimConnect - A new SimConnect library for C#

This project will become a full SimConnect interface in modern C#, not using old programming models with Windows handles and such.

## Structure of this repository

There are three sub-projects:
* [CsSimConnectInterOp(CsSimConnectInterOp/) is a non-managed C++ DLL that provides an interface to the static SimConnect library.
* [CsSimConnect(CsSimConnect/) is the actual C# Class Library, using the .Net 5.0 platform.
* [CsSimConnectUI(CsSimConnectUI/) is a test/demo app, showing how to use the library.

## How to run the demo app

Build the solution, and copy the `CsSimConnectInterOp.dll`, CsSimConnect.dll`