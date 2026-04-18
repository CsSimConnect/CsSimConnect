@echo off
rd /s /q dist
md dist
copy rakisLog.properties dist\
dotnet publish -o dist --force -c Release -p:PublishReadyToRun=true --self-contained false -r win-x64
