# Используем официальный Python образ
FROM python:3.10-slim

# Устанавливаем системные зависимости
RUN apt-get update && apt-get install -y git build-essential && rm -rf /var/lib/apt/lists/*

# Устанавливаем необходимые Python-пакеты
RUN pip install --upgrade pip
RUN pip install vllm==0.6.5 fastapi uvicorn

# Копируем скрипт API-сервера
COPY openai_server.py /app/openai_server.py

WORKDIR /app

# Указываем команду для запуска сервера
CMD ["uvicorn", "openai_server:app", "--host", "0.0.0.0", "--port", "8000"]
