@echo off
echo Pokretanje Ping Pong turnira...

:: pokrece se server
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Server\bin\Debug\net8.0\Server.exe"


timeout /t 2 >nul

:: pokrecu se 4 clienta
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Client\bin\Debug\net8.0\Client.exe"
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Client\bin\Debug\net8.0\Client.exe"
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Client\bin\Debug\net8.0\Client.exe"
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Client\bin\Debug\net8.0\Client.exe"

exit
