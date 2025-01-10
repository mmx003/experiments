@echo off
REM Батник для остановки и удаления всех контейнеров, использующих образ vllm-openai-cpu,
REM удаления самого образа и его пересборки.

chcp 65001 >nul
SET IMAGE_NAME=vllm-openai-cpu

echo.
echo ===============================
echo Остановка контейнеров, использующих образ %IMAGE_NAME%...
echo ===============================
echo.

REM Получение списка всех контейнеров, использующих указанный образ
FOR /F "tokens=*" %%i IN ('docker ps -a --filter "ancestor=%IMAGE_NAME%" -q') DO (
    echo Остановка контейнера %%i...
    docker stop %%i
)

echo.
echo ===============================
echo Удаление контейнеров, использующих образ %IMAGE_NAME%...
echo ===============================
echo.

REM Удаление остановленных контейнеров
FOR /F "tokens=*" %%i IN ('docker ps -a --filter "ancestor=%IMAGE_NAME%" -q') DO (
    echo Удаление контейнера %%i...
    docker rm %%i
)

echo.
echo ===============================
echo Удаление образа %IMAGE_NAME%...
echo ===============================
echo.

REM Удаление образа. Используйте флаг -f для принудительного удаления, если необходимо
docker rmi %IMAGE_NAME%

echo.
echo ===============================
echo Сборка образа %IMAGE_NAME%...
echo ===============================
echo.

REM Сборка Docker-образа
docker build -t %IMAGE_NAME% .

echo.
echo ===============================
echo Процесс завершён успешно.
echo ===============================
echo.
