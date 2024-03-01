@echo off
pushd %~dp0
powershell -ExecutionPolicy Bypass -File ./Setup.ps1
popd