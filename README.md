# CsSimConnect - A new SimConnect library for C#

This project will become a full SimConnect interface in modern C#, not using old programming models with Windows handles and such.

## What is in this repo

This repository contains just the .Net DLL sources.

## What is _not_ in this repo

This repository needs a matching `CsSimConnectInterOp.dll` non-dotnet dynamic library from the [CsSiMConnectInterOp](https://github.com/CsSimConnect/CsSimConnectInterOp)
repository. You can either build that yourself, or get a binary if you trust me. Put the DLL in a suitable location and define an environment variable named
`CSSC_INTEROP_PATH` with the full path to this DLL.