@echo off
REM Батник для управления Docker-контейнерами vLLM
chcp 65001 >nul

REM Установка переменных с параметрами или значениями по умолчанию
SET "Action=%~1"
SET "ModelName=%~2"
SET "Token=%~3"
SET "Port=%~4"

REM Установка значений по умолчанию, если параметры не заданы
IF "%Action%"=="" SET "Action=start"
IF "%ModelName%"=="" SET "ModelName=distilgpt2"
IF "%Token%"=="" SET "Token=<token>"
IF "%Port%"=="" SET "Port=8000"

REM Формирование имени контейнера
SET "containerName=vllm-server-%ModelName%"

REM Обработка действия
IF /I "%Action%"=="start" (
    echo Запуск контейнера %containerName% с моделью %ModelName% на порту %Port%...
    docker run -d ^
      -v C:/Users/mmx00/.cache/huggingface:/root/.cache/huggingface ^
      --env "HUGGING_FACE_HUB_TOKEN=%Token%" ^
      --env "MODEL_NAME=%ModelName%" ^
      -p %Port%:8000 ^
      --ipc=host ^
      --name %containerName% ^
      vllm-openai-cpu
    IF ERRORLEVEL 1 (
        echo Ошибка при запуске контейнера.
    ) ELSE (
        echo Контейнер %containerName% успешно запущен на порту %Port% с моделью %ModelName%.
    )
) ELSE IF /I "%Action%"=="stop" (
    echo Остановка контейнера %containerName%...
    docker stop %containerName%
    IF ERRORLEVEL 1 (
        echo Ошибка при остановке контейнера.
    ) ELSE (
        echo Контейнер %containerName% успешно остановлен.
    )
) ELSE IF /I "%Action%"=="restart" (
    echo Перезапуск контейнера %containerName%...
    docker restart %containerName%
    IF ERRORLEVEL 1 (
        echo Ошибка при перезапуске контейнера.
    ) ELSE (
        echo Контейнер %containerName% успешно перезапущен.
    )
) ELSE IF /I "%Action%"=="remove" (
    echo Остановка контейнера %containerName%...
    docker stop %containerName%
    IF ERRORLEVEL 1 (
        echo Ошибка при остановке контейнера.
    ) ELSE (
        echo Контейнер %containerName% остановлен.
        echo Удаление контейнера %containerName%...
        docker rm %containerName%
        IF ERRORLEVEL 1 (
            echo Ошибка при удалении контейнера.
        ) ELSE (
            echo Контейнер %containerName% успешно удалён.
        )
    )
) ELSE IF /I "%Action%"=="rebuild" (
    REM Остановка и удаление контейнеров
    echo Остановка всех контейнеров, использующих образ vllm-openai-cpu...
    FOR /F "tokens=*" %%i IN ('docker ps -a --filter "ancestor=vllm-openai-cpu" -q') DO (
        echo Остановка контейнера %%i...
        docker stop %%i
    )

    echo Удаление всех контейнеров, использующих образ vllm-openai-cpu...
    FOR /F "tokens=*" %%i IN ('docker ps -a --filter "ancestor=vllm-openai-cpu" -q') DO (
        echo Удаление контейнера %%i...
        docker rm %%i
    )

    echo Удаление образа vllm-openai-cpu...
    docker rmi vllm-openai-cpu

    echo Сборка нового Docker-образа vllm-openai-cpu...
    docker build -t vllm-openai-cpu .

    IF ERRORLEVEL 1 (
        echo Ошибка при сборке Docker-образа.
    ) ELSE (
        echo Docker-образ vllm-openai-cpu успешно пересобран.
    )
) ELSE (
    echo Недопустимое действие: %Action%.
    echo Используйте одно из следующих действий: start, stop, restart, remove, rebuild.
)
