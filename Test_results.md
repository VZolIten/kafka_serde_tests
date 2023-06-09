# Результаты тестирования сериализаторов для Apache Kafka

## Методика
Делалось 5 замеров для каждого режима, по 15 секунд последовательной генерации одним продьюссером. Всякий раз заново создавались контейнеры, чтобы подавить JIT-оптимизации горячего кода между замерами.

```
KAFKA_PARTITIONS_COUNT=1
KAFKA_PRODUCERS_COUNT=1
KAFKA_PRODUCTION_DELAY_SEC=0
KAFKA_PRODUCTION_TIMEOUT_SEC=15
```

## Результаты замеров
|№ | JSON (Runtime) | JSON (AOT) | Protobuf  |
|--|----------------|------------|-----------|
|1 | 2312           | 2264       | 2350      |
|2 | 2276           | 2292       | 2353      |
|3 | 2293           | 2283       | 2356      |
|4 | 2271           | 2296       | 2351      |
|5 | 2263           | 2305       | 2325      |

Из таблицы видно, что AOT-верия JSON-сериализации в среднем не  отличается драматически по производительности от рантаймовой версии. Причиной тому служит кэширование результатов относительно затратных обращений к Reflection API в начале. В условиях постоянного потока сообщений со стабильной стемой, эта разница не будет играть роли.

В то же время в тестах с использованием Protobuf заметно, что данная реализация стабильно имеет отрыв на 60-100 условных единиц. Причиной тому может быть более компактный формат и его дружественность к парсингу (в противовес чисто текстовому JSON).