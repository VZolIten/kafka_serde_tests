# Демонстратор С#/.NET + Apache Kafka

* [Описание](#описание)
* [Требования](#требования)
* [Использование](#использование)
* [Настройки](#настройки)
* [Отладка](#отладка)
* [Разработка](#разработка)

## Описание
Приложение представляет собой пару Consumer-Producer клиентских приложений на C#, связанных вместе контрактами (Contracts) и некоторым общим кодом (Infrastructure). В `docker-compose.yml` описан запуск этих приложений с сервисами Kafka: самим Apacke Kafka, ZooKeeper и веб-интерфейсом для Kafka.

Продьюссеры и консьюмеры пишут в лог информацию о полученных/отправленных сообщениях. Продьюсеры перед завершением работы пишут общее количество отправленных сообщений, а консьюмеры сообщают значения оффсетов при коммите.

Есть возможность [запускать локальные инстансы](#отладка) консьюмеров и продьюссеров к запущенному сетапу в Docker.

## Требования
Для запуска необходим [установленный Docker](https://docs.docker.com/get-docker/) и Docker Compose (в составе дистрибутива Docker Desktop).

## Использование
В директории проекта выполнить:
```bash
docker compose up
```
Спустя некоторое время после старта становятся доступны:

* по адресу `localhost:29092` - кластер Apache Kafka,
* по адресу [http://localhost:9000/](http://localhost:9000/) - веб-интефейс Kafka ([Kafdrop](https://github.com/obsidiandynamics/kafdrop)), где можно найти:
    * информацию о Kafka-кластере,
    * список существующих топиков,
    * [партишны топика](http://localhost:9000/topic/first.topic),
    * [сообщения](http://localhost:9000/topic/first.topic/messages?partition=0&offset=0&count=100) в отдельном партишне. Для Protobuf, чтобы увидеть сообщения в человекочитаемом виде, необходимо на странице партишна [выбрать](http://localhost:9000/topic/first.topic/messages?partition=0&offset=0&count=100&keyFormat=DEFAULT&format=PROTOBUF&descFile=Contact.desc&msgTypeName=ContactProto&isAnyProto=false) `Message format=PROTOBUF`, `Protobuf descriptor=Contact.desc` и `Protobuf message type name=ContactProto`),
    * текущие оффсеты консьюмеров в служебном топике [__consumer_offsets](http://localhost:9000/topic/__consumer_offsets).

## Настройки

Сетап можно настраивать с помощью перменных в файле `.env`. В частности:

* способ (де)сериализации сообщений (`SERDE_TYPE`), доcтупны значения:
    * `Json`,
    * `JsonAot`,
    * `Protobuf`,
* количество партишнов в топике и соответствующее им количество консьюмеров (`KAFKA_PARTITIONS_COUNT`),
* периодичность коммита оффсетов для консьюмеров (`KAFKA_COSUMERS_COMMIT_EACH`),
* количество продьюссеров (`KAFKA_PRODUCERS_COUNT`),
* задержку в секундах между генерацией сообщений для каждого продьюссера (`KAFKA_PRODUCTION_DELAY_SEC`),
* время в секундах, через которое продьюссер прекращает работу (`KAFKA_PRODUCTION_TIMEOUT_SEC`), где значение `0` означает бесконечную генерацию.

## Отладка

В `docker-compose.yml` настроены лисенеры для подключения как внутри, так и снаружи докера. Чтобы запустить приложение в IDE необходимо, чтобы подтягивался конфиг `appsettings.Development.json`, для этого следует установить переменную окружения `DOTNET_ENVIRONMENT=Development`.

_Для отладки **консьюмера** может потребоваться скорректировать значение `consumer:deploy:replicas` в `docker-compose.yml`, чтобы оно было больше переменной `KAFKA_PARTITIONS_COUNT` (из файла `.env`), дабы избежать конкуренции за партишны с консьюмерами в контейнерах._

## Разработка

### Внесение изменений в сборку приложений (Dockerfile)

Общие для проектов изменения необходимо вносить в `Dockerfile.template`, затем вызывать скрипт `generate_dockerfiles.sh`, чтобы изменения синхронизировались для продьюссера и консьюмера.

### Обновление контрактов Protobuf

После обновления/добавления proto-файлов (`./Contracts/ProtoEntities/*`) необходимо запустить скрипт `./Contracts/regenerate_contracts.sh`, который помимо (пере)генерации C#-контракта сгенерирует описатели (`*.desc`), которые позволят Kafdrop просматривать сообщения. Должен быть [установлен](https://github.com/protocolbuffers/protobuf/releases) (и добавлен в переменную окружения `PATH`) `protoc` >= 22.2.