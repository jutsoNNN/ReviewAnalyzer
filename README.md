# Приложение с использованием DeepSeek-R1 для анализа отзывов на маркетплейсах

**ReviewAnalyzer** — консольное приложение на C# и .NET 8, которое автоматически собирает и анализирует отзывы с маркетплейсов (Wildberries, частично Ozon), используя локальную LLM-модель **DeepSeek-R1**. Результат — структурированный отчет с определением тональности, ключевых плюсов и минусов продукта, тем и рекомендацией к покупке.

---

## Основные возможности

- Локальный AI-анализ отзывов с помощью DeepSeek-R1 (через LLamaSharp)
- Парсинг отзывов с маркетплейсов (HTML через Selenium + HtmlAgilityPack)
- Частичная поддержка Ozon, Yandex.Market и др. (в разработке)
- Сохранение отзывов и анализа в `.json`-файлы
- Кросс-платформенная поддержка (Windows, Linux, Mac с настройкой путей)
- Частично оффлайн, без API или облачных запросов

---

## Структура проекта

```plaintext
ReviewAnalyzer
├── Program.cs
├── appsettings.json                  // Конфигурация (модель, настройки анализа)
├── ReviewAnalyzer.csproj             // Файл проекта
│
├── Core                           // Интерфейсы и сущности
│   ├── Entities
│   ├── Enums
│   └── Interfaces
│
├── Infrastructure                 // Реализации
│   ├── AI                           // DeepSeekAnalysisService.cs
│   ├── Storage                      // JsonStorageService.cs
│   └── MarketplaceAdapters          // OzonAdapter.cs, WildberriesAdapter.cs
│
├── BusinessLogic                  // Обработка отзывов и фабрики
├── Presentation                   // Консольный UI
├── Utilities                      // Логгер, конфигурация
│
├── Models                         // 💾 Модель DeepSeek (например: deepseek-llm-7b-chat.Q4_K_M.gguf)
├── AnalysisResults                // Результаты анализа
├── ParsedReviews                  // Сырые отзывы
├── chromedriver                   // Для работы Selenium с Wildberries
```

---

## Установка и запуск

> ❗ .NET 8 SDK: https://dotnet.microsoft.com/en-us/download

1. **Клонировать репозиторий**

```bash
git clone https://github.com/your-username/ReviewAnalyzer.git
cd ReviewAnalyzer
```

chromedriver.exe в папку chromedriver/ или установить пакет Selenium.WebDriver.ChromeDriver(dotnet restore) и использовать путь по умолчанию

2. **Установить зависимости**

```bash
dotnet restore
```

3. **Скачать квантованную модель DeepSeek R1 (например, 7b)**

Модель должна быть в формате `.gguf`, например: `deepseek-llm-7b-chat.Q4_K_M.gguf`, которая используется в проекте.

Скачать можно с [HuggingFace](https://huggingface.co/TheBloke/Deepseek-LLM-7B-Chat-GGUF) (вариант: `Q4_K_M`):
- Файл в папку `Models/`
- Путь в `appsettings.json`:

```json
{
  "ModelPath": "Models\deepseek-llm-7b-chat.Q4_K_M.gguf",
  "GpuLayers": 0,
  "MaxReviews": 20,
  "ContextSize": 4096,
  "AnalysisTimeout": 300
}
```

> Если хотите использовать GPU (CUDA), поменяйте LLamaSharp.Backend.Cpu на соответствующий GPU-бэкенд, а также измените параметры в appsettings.json

---

## Запуск приложения

```bash
dotnet run --project ReviewAnalyzer
```

---

## Пример вывода

```plaintext
=== PRODUCT REVIEW ANALYZER (DeepSeek R1) ===
Enter product URL: https://www.wildberries.ru/catalog/137956621/feedbacks
...
АНАЛИЗ ЗАВЕРШЕН!

Общая тональность: Крайне положительные
Рекомендация: Да
Ключевые преимущества:
- Качественный материал
- Быстрая доставка
- Хорошо упакован
Ключевые недостатки:
- Маломерит
- Плохо отстирывается
...
```

---

## Используемые технологии

- C# / .NET 8
- DeepSeek LLM
- LLamaSharp + llama.cpp (через .dll)
- Selenium + HtmlAgilityPack (парсинг отзывов)
- Newtonsoft.Json
- AngleSharp

---

## TODO / Планы на будущее

- [ ] Полноценная поддержка Ozon
- [ ] Поддержка Yandex.Маркет
- [ ] Веб-интерфейс для запуска анализа
- [ ] Возможность загрузки отзывов вручную (из файла)
- [ ] Тесты и CI-пайплайн
