@echo off
rd /s /q dist
md dist
copy rakisLog.properties dist\
copy x64\Release\CsSimConnectInterOp.dll dist\
dotnet publish -o dist --force -c Release -p:PublishReadyToRun=true --self-contained false -r win10-x64
