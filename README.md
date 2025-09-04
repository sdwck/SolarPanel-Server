# SolarPanel

Это проект для сбора и хранения данных с солнечных панелей, которые приходят через MQTT в формате JSON. Данные сохраняются в PostgreSQL.

---

## Структура проекта

- `/src/SolarPanel.API` — Web API, принимает и обрабатывает запросы.
- `/src/SolarPanel.Application` — логика.
- `/src/SolarPanel.Core` — сущности и интерфейсы.
- `/src/SolarPanel.Infrastructure` — реализация доступа к данным и прочих интерфейсов, работа с БД, MQTT.

---

## Как поднять проект локально

### 1. Запуск PostgreSQL через Docker

```bash
docker run -e POSTGRES_PASSWORD=Strongpassword1234 -e POSTGRES_DB=SolarPanel -p 5432:5432 --name pgsql -d postgres
````

Скачать и установить Docker: 
https://docs.docker.com/desktop/setup/install/windows-install/

* Это создаст и запустит контейнер с БД.

---

### 2. Настройка секретов пользователя (user-secrets)

Перейдите в каталог с API:

```bash
cd .\src\SolarPanel.API
```

Установите MQTT учётные данные:

```bash
dotnet user-secrets set "MqttSettings:Username" "my-mqtt-username"
dotnet user-secrets set "MqttSettings:Password" "my-mqtt-password"
```

**Важно:** Убедитесь, что у вас установлен .NET SDK, иначе команда `dotnet` не сработает.

Скачать .NET SDK:
https://dotnet.microsoft.com/en-us/download

---

### 3. Запуск проекта

* Откройте проект в IDE.
* Запустите проект.
* Swagger UI доступен по адресу:
  https://localhost:7039/swagger/index.html — для проверки API.

---

## Работа с мокнутыми и реальными данными

### В чём разница?

* **Mock-данные** — это заранее подготовленные тестовые данные, которые помогают разрабатывать и тестировать без подключения к реальному облаку и MQTT.
* **Реальные данные** — поступают из облака солнечных панелей через MQTT.

### Как переключаться?

1. В `appsettings.Development.json` найдите параметр (пример):

```json
{
  "UseMockData": true
}
```

* `true` — использовать mock-данные (для разработки без подключения к MQTT).
* `false` — использовать реальные данные из MQTT.

2. Перезапустите приложение после изменения.
