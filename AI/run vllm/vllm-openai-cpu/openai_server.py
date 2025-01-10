import os
import time
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from vllm import LLM
import vllm.engine.llm_engine as llm_engine

app = FastAPI()

# Патчим метод is_async_output_supported для обхода ошибки
llm_engine.LLMEngine._verify_async_output_supported = lambda *args, **kwargs: None

# Принудительное использование CPU
os.environ['CUDA_VISIBLE_DEVICES'] = ''

# Получаем имя модели из переменной окружения, по умолчанию 'gpt2'
model_name = os.getenv('MODEL_NAME', 'gpt2')
print(f"Инициализация LLM с моделью: {model_name} на устройстве: cpu")

try:
    llm = LLM(model=model_name, device='cpu')
    print("LLM успешно инициализирован.")
except Exception as e:
    print("Ошибка при инициализации LLM:")
    import traceback
    traceback.print_exc()
    raise

class OpenAIRequest(BaseModel):
    model: str
    prompt: str
    max_tokens: int = 50
    temperature: float = 0.7
    top_p: float = 0.9
    n: int = 1
    stream: bool = False
    stop: list = None

class OpenAIResponseChoice(BaseModel):
    text: str

class OpenAIResponse(BaseModel):
    id: str
    object: str
    created: int
    model: str
    choices: list[OpenAIResponseChoice]

@app.post("/v1/completions", response_model=OpenAIResponse)
async def create_completion(request: OpenAIRequest):
    try:
        completions = llm.generate(
            request.prompt,
            max_tokens=request.max_tokens,
            temperature=request.temperature,
            top_p=request.top_p,
            n=request.n
        )
        choices = [OpenAIResponseChoice(text=completion) for completion in completions]
        return OpenAIResponse(
            id="cmpl_123",
            object="text_completion",
            created=int(time.time()),
            model=request.model,
            choices=choices
        )
    except Exception as e:
        print("Ошибка при генерации завершения:")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))
