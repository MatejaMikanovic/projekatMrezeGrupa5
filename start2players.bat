@echo off
echo Pokretanje Ping Pong turnira...

:: Pokreni server
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Server\bin\Debug\net8.0\Server.exe"

:: Sacekaj malo da se server pokrene
timeout /t 2 >nul

:: Pokreni 2 klijenta
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Client\bin\Debug\net8.0\Client.exe"
start "" "C:\Users\matej\source\repos\PingPongTurnir.sln\Client\bin\Debug\net8.0\Client.exe"

exit
