@echo off
rd /s /q dist
md dist
copy rakisLog.properties dist\
dotnet publish -o dist --force -c Release -p:PublishReadyToRun=true --self-contained false -r win10-x64
cd CsSimConnectInterOp
msbuild CsSimConnectInterOp.vcxproj -property:Configuration=P3Dv5_Release
msbuild CsSimConnectInterOp.vcxproj -property:Configuration=MSFS2020_Release
cd ..
