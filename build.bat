@ECHO OFF
set /p version="Enter Moq.AutoMocker package version: "

powershell -ExecutionPolicy ByPass -Command "& './build.ps1' '%version%'" 

PAUSE