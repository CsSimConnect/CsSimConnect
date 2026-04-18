# CsSimConnect - A new SimConnect library for C#

This project will become a full SimConnect interface in modern C#, not using old programming models with Windows handles and such.

## What is in this repo

This repository contains just the .Net DLL sources.

## What is _not_ in this repo

This repository no longer ships the native `CsSimConnectInterOp.dll`.

Applications should reference `CsSimConnect` together with the matching native package(s) from the
[CsSiMConnectInterOp](https://github.com/CsSimConnect/CsSimConnectInterOp) repository, for example:

- `CsSimConnect.Native.MSFS2020`
- `CsSimConnect.Native.MSFS2024`

These native packages copy simulator-specific builds to subfolders such as `MSFS2020\CsSimConnectInterOp.dll` in the application output.

`CSSC_INTEROP_PATH` and `CSSC_INTEROP_DIR` remain available as manual overrides for development or custom deployment layouts.
