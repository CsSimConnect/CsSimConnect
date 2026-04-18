@echo off
rd /s /q dist
if exist bin rd /s /q bin
if exist obj rd /s /q obj
md dist
copy rakisLog.properties dist\
dotnet publish -o dist --force -c Release -p:PublishReadyToRun=true --self-contained false -r win-x64
