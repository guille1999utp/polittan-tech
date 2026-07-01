# Transfer Reservations API

API REST para la gestión de **reservas de traslados**, desarrollada como prueba técnica de Backend con **.NET**.

Permite crear, consultar, confirmar y cancelar reservas, aplicando un motor de tarifación con reglas de negocio, validaciones de entrada y una regla de no duplicidad. **No requiere base de datos**: la persistencia es en memoria.

---

## Tabla de contenido

- [Stack y librerías](#stack-y-librerías)
- [Arquitectura](#arquitectura)
- [Cómo ejecutar el proyecto](#cómo-ejecutar-el-proyecto)
- [Endpoints](#endpoints)
- [Reglas de negocio](#reglas-de-negocio)
  - [Estados y transiciones](#estados-y-transiciones)
  - [Cálculo de precio](#cálculo-de-precio)
  - [Validaciones](#validaciones)
  - [Regla de duplicidad](#regla-de-duplicidad)
- [Decisiones técnicas y patrones](#decisiones-técnicas-y-patrones)
- [Supuestos](#supuestos)
- [Pruebas](#pruebas)
- [Qué mejoraría con más tiempo](#qué-mejoraría-con-más-tiempo)

---

## Stack y librerías

| Componente | Elección | Motivo |
|---|---|---|
| Framework | **.NET 8** (LTS) | Requisito de la prueba (.NET 8+). Compilado y probado con el SDK 10. |
| API | **ASP.NET Core Web API** (Controllers) | Contrato REST claro y explícito; controllers delgados. |
| Validación | **FluentValidation** | Reglas de validación declarativas, testeables y desacopladas de los DTOs. |
| Documentación | **Swashbuckle / Swagger** | OpenAPI + UI interactiva para probar los endpoints. |
| Errores | **ProblemDetails (RFC 7807)** | Respuestas de error estandarizadas vía `IExceptionHandler`. |
| Tiempo | **`TimeProvider`** (.NET 8) | Reloj inyectable → cálculos deterministas y fáciles de testear. |
| Pruebas | **xUnit + FluentAssertions + FakeTimeProvider** | Pruebas legibles y deterministas. |

---

## Arquitectura

El proyecto sigue **Clean Architecture** (arquitectura en capas con dependencias hacia adentro). El dominio no conoce a nadie; la infraestructura y la API dependen de las abstracciones, no al revés.

```
┌──────────────────────────────────────────────────────────┐
│  Reservations.Api            (Controllers, Swagger,         │
│                               manejo de excepciones)        │
│        │ depende de                                         │
│        ▼                                                    │
│  Reservations.Application    (Casos de uso, DTOs,           │
│                               validadores, interfaces)      │
│        │ depende de                                         │
│        ▼                                                    │
│  Reservations.Domain         (Entidades, enums, motor de    │
│                               precios, Result/Error)        │
│        ▲                                                    │
│        │ implementa las abstracciones                       │
│  Reservations.Infrastructure (Repositorio in-memory)        │
└──────────────────────────────────────────────────────────┘
```

| Proyecto | Responsabilidad |
|---|---|
| **Reservations.Domain** | Corazón del negocio: entidad `Reservation` con sus invariantes de estado, enums, el motor de precios (`ReservationPricingEngine` + reglas), y el patrón `Result`/`Error`. Sin dependencias externas. |
| **Reservations.Application** | Casos de uso (`ReservationService`), DTOs de entrada/salida, validadores (FluentValidation) y las abstracciones de persistencia (`IReservationRepository`). |
| **Reservations.Infrastructure** | Implementación concreta de persistencia: `InMemoryReservationRepository` (thread-safe). Reemplazable por EF Core sin tocar otras capas. |
| **Reservations.Api** | Capa de entrega HTTP: controllers, configuración de Swagger, manejo global de excepciones y traducción de errores de negocio a códigos HTTP. |
| **Reservations.UnitTests** | Pruebas unitarias de dominio, aplicación y tarifación. |

### Estructura de carpetas

```
polittan-tech/
├─ TransferReservations.slnx
├─ README.md
├─ src/
│  ├─ Reservations.Domain/
│  │  ├─ Common/            # Result, Error
│  │  ├─ Entities/          # Reservation (aggregate root)
│  │  ├─ Enums/             # ReservationStatus, ServiceType
│  │  └─ Pricing/           # Motor de precios + reglas (Strategy)
│  │     └─ Rules/
│  ├─ Reservations.Application/
│  │  ├─ DependencyInjection.cs
│  │  └─ Reservations/
│  │     ├─ Abstractions/   # IReservationService, IReservationRepository
│  │     ├─ Dtos/
│  │     ├─ Validation/     # CreateReservationRequestValidator
│  │     └─ ReservationService.cs
│  ├─ Reservations.Infrastructure/
│  │  ├─ DependencyInjection.cs
│  │  └─ Persistence/       # InMemoryReservationRepository
│  └─ Reservations.Api/
│     ├─ Controllers/       # ReservationsController
│     ├─ ExceptionHandling/ # IExceptionHandler (validación + fallback)
│     ├─ Extensions/        # Error -> ProblemDetails
│     └─ Program.cs
└─ tests/
   └─ Reservations.UnitTests/
```

---

## Cómo ejecutar el proyecto

### Requisitos

- **.NET SDK 8.0 o superior** ([descargar](https://dotnet.microsoft.com/download)).

### 1. Restaurar y compilar

```bash
dotnet build
```

### 2. Ejecutar la API

```bash
dotnet run --project src/Reservations.Api
```

Por defecto la API queda disponible (perfil `http`) en:

- **API:** `http://localhost:5059`
- **Swagger UI:** `http://localhost:5059/` (la raíz redirige a Swagger)

> Puedes fijar el puerto con: `dotnet run --project src/Reservations.Api --urls "http://localhost:5199"`

### 3. Ejecutar las pruebas

```bash
dotnet test
```

### 4. Probar rápidamente

- Abre **Swagger UI** en el navegador, o
- Usa el archivo [`src/Reservations.Api/Reservations.Api.http`](src/Reservations.Api/Reservations.Api.http) (VS Code / Rider / Visual Studio), o
- Usa `curl` (ver ejemplos abajo).

---

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| `POST` | `/reservations` | Crea una reserva y calcula su precio | `201`, `400`, `409` |
| `GET` | `/reservations` | Lista todas las reservas | `200` |
| `GET` | `/reservations/{id}` | Obtiene una reserva por id | `200`, `404` |
| `PATCH` | `/reservations/{id}/confirm` | Confirma una reserva | `200`, `404`, `409` |
| `PATCH` | `/reservations/{id}/cancel` | Cancela una reserva | `200`, `404`, `409` |

### Ejemplo — crear reserva

**Request**

```bash
curl -X POST http://localhost:5059/reservations \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Juan Pérez",
    "origin": "Bogotá",
    "destination": "Aeropuerto El Dorado",
    "date": "2026-12-20T10:00:00",
    "passengers": 5,
    "serviceType": "premium"
  }'
```

**Response `201 Created`** (incluye el **desglose** del precio para trazabilidad):

```json
{
  "id": "3c18295f-edd2-4724-b8ef-09b9fc8e7d95",
  "customerName": "Juan Pérez",
  "origin": "Bogotá",
  "destination": "Aeropuerto El Dorado",
  "date": "2026-12-20T10:00:00",
  "passengers": 5,
  "serviceType": "Premium",
  "status": "Created",
  "price": 156000,
  "priceBreakdown": [
    { "concept": "Base fare Premium", "amount": 80000 },
    { "concept": "Passengers (5 x 10,000)", "amount": 50000 },
    { "concept": "Large group surcharge (+15%)", "amount": 19500 },
    { "concept": "Premium large group surcharge (+10%)", "amount": 13000 },
    { "concept": "Advance booking discount (-5%)", "amount": -6500 }
  ],
  "createdAt": "2026-07-01T17:35:42",
  "updatedAt": "2026-07-01T17:35:42"
}
```

### Ejemplo — error de validación `400`

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "CustomerName": ["Customer name is required."],
    "Passengers": ["The number of passengers must be between 1 and 6."],
    "Date": ["Date must be valid and not in the past."],
    "ServiceType": ["Service type must be 'standard' or 'premium'."],
    "Destination": ["Origin and destination must be different."]
  }
}
```

### Ejemplo — error de negocio `409`

```json
{
  "title": "A reservation with the same customer, origin, destination, date and service type already exists.",
  "status": 409,
  "code": "reservation.duplicate"
}
```

---

## Reglas de negocio

### Estados y transiciones

Estados posibles: `Created`, `Confirmed`, `Cancelled`. Estado inicial: **`Created`**.

```
Created ──confirm──► Confirmed
   │                    │
 cancel               cancel
   │                    │
   ▼                    ▼
Cancelled  ◄────────────┘
```

- No se puede **confirmar** una reserva `Cancelled` ni una ya `Confirmed` → `409 Conflict`.
- No se puede **cancelar** una reserva ya `Cancelled` → `409 Conflict`.
- Las transiciones están encapsuladas en la entidad `Reservation` (`Confirm`/`Cancel`), que devuelve un `Result` en lugar de lanzar excepciones.

### Cálculo de precio

El precio se compone de un **subtotal base** más una serie de **recargos/descuentos porcentuales**:

**1. Subtotal base**

- Tarifa por tipo de servicio: `Standard = 50.000 COP`, `Premium = 80.000 COP`.
- `+ 10.000 COP` por cada pasajero.

**2. Recargos y descuentos** (porcentajes sobre el subtotal base):

| Regla | Condición | Ajuste |
|---|---|---|
| Mismo día | La reserva es para el mismo día calendario | **+20%** |
| Grupo grande | Más de 4 pasajeros (5 o 6) | **+15%** |
| Premium grupo grande | Premium **y** más de 3 pasajeros (4, 5 o 6) | **+10%** |
| Anticipación | 2 o más días de anticipación | **−5%** |

> **Supuesto clave (ver [Supuestos](#supuestos)):** los porcentajes son **aditivos** y se calculan **siempre sobre el subtotal base**, no compuestos entre sí. Esto hace el cálculo determinista, auditable y fácil de explicar línea por línea (por eso la API devuelve el `priceBreakdown`).

**Ejemplo trabajado** (Premium, 5 pasajeros, con 2+ días de anticipación):

```
Tarifa base Premium ............  80.000
Pasajeros (5 × 10.000) .........  50.000
                                 ────────
Subtotal base .................. 130.000
  +15% grupo grande ............  19.500
  +10% premium grupo grande ....  13.000
   −5% anticipación ............. − 6.500
                                 ────────
Total ......................... 156.000 COP
```

### Validaciones

Se aplican al crear una reserva (`POST /reservations`) y devuelven `400` con el detalle por campo:

- **Campos obligatorios:** `customerName`, `origin`, `destination`, `date`, `passengers`, `serviceType`.
- **Pasajeros:** entero entre **1 y 6**.
- **Fecha:** válida y **no en el pasado**.
- **Origen y destino distintos** (comparación sin distinguir mayúsculas ni espacios).
- **Tipo de servicio** válido: `standard` o `premium` (case-insensitive).

### Regla de duplicidad

No se permiten dos reservas **idénticas** en la combinación **(cliente, origen, destino, fecha, tipo de servicio)** → `409 Conflict`.

- La comparación de textos ignora mayúsculas/minúsculas y espacios sobrantes.
- Las reservas **canceladas se ignoran**: si cancelas una reserva, puedes volver a crear una idéntica.

---

## Decisiones técnicas y patrones

- **Clean Architecture:** separación clara de responsabilidades y dependencias hacia el dominio. Facilita sustituir infraestructura (p. ej. memoria → EF Core) sin tocar la lógica de negocio.
- **Patrón Result (`Result`/`Error`):** los errores de negocio esperados (no encontrado, conflicto de estado, duplicidad) se modelan como valores, no como excepciones. Las excepciones quedan reservadas para fallos realmente excepcionales.
- **Patrón Strategy en la tarifación:** cada recargo/descuento es una clase que implementa `ISurchargeRule`. Agregar o quitar una regla no modifica el motor (`ReservationPricingEngine`) → principio Open/Closed. Las reglas se descubren por inyección de dependencias.
- **Entidad de dominio rica:** `Reservation` protege sus invariantes; las transiciones de estado solo ocurren a través de sus métodos.
- **Repository pattern:** la capa de aplicación depende de `IReservationRepository`; la implementación in-memory vive en Infraestructura.
- **`TimeProvider` inyectable:** el "ahora" no se lee con `DateTime.Now` disperso por el código, sino desde una abstracción → cálculos de precio y validaciones **deterministas y testeables** (en pruebas se usa `FakeTimeProvider`).
- **Manejo de errores centralizado con `IExceptionHandler` + ProblemDetails (RFC 7807):** respuestas de error uniformes; la validación produce `ValidationProblemDetails` y cualquier fallo no controlado, un `500` sin filtrar detalles internos.
- **DTOs de entrada/salida:** la entidad de dominio nunca se expone directamente; el mapeo es explícito.
- **Enums serializados como texto** para respuestas legibles (`"Created"`, `"Premium"`).
- **Desglose de precio (`priceBreakdown`)** en la respuesta: aporta transparencia y facilita la defensa/depuración del cálculo.

---

## Supuestos

1. **Porcentajes aditivos sobre el subtotal base** (no compuestos). Es la interpretación más común y transparente del enunciado; se documenta explícitamente y el desglose lo hace verificable.
2. **"Mismo día"** = la fecha de la reserva cae en el mismo día calendario que el momento de la cotización.
3. **"2+ días de anticipación"** = diferencia de días completos entre hoy y la fecha de la reserva ≥ 2. (Con exactamente 1 día no aplica ni recargo de mismo día ni descuento.)
4. **Duplicidad:** solo bloquean las reservas **no canceladas**; la comparación de textos es *case/space-insensitive*.
5. **Persistencia en memoria:** los datos se pierden al reiniciar el proceso (el enunciado indica que no se requiere base de datos). El repositorio se registra como *singleton* para conservar estado durante la ejecución.
6. **Moneda COP sin decimales:** el total se redondea a peso entero.
7. **Fechas sin zona horaria:** se trabaja con la hora local del servidor de forma consistente (entrada y validación).
8. **Autenticación/autorización fuera de alcance** para esta prueba.

---

## Pruebas

`dotnet test` — **30 pruebas** unitarias que cubren:

- **Motor de precios:** cada regla por separado, combinaciones, casos límite (grupo de 4 vs 5, 1 día vs 2 días) y que el desglose sume el total.
- **Entidad `Reservation`:** transiciones de estado válidas e inválidas.
- **Validador:** campos obligatorios, rango de pasajeros, fecha pasada, origen = destino, tipo de servicio inválido y case-insensitive.
- **`ReservationService`:** creación con precio correcto, duplicidad (incluida la excepción de canceladas), *not found*, confirmación/cancelación y errores de validación.

Las pruebas usan `FakeTimeProvider` para fijar el "ahora" y ser 100% deterministas.

---

## Qué mejoraría con más tiempo

- **Persistencia real** con EF Core (SQL Server / PostgreSQL) o un almacén configurable, aprovechando que la abstracción `IReservationRepository` ya lo permite sin tocar el dominio.
- **Pruebas de integración** end-to-end con `WebApplicationFactory` (ya dejé `Program` como `partial` para habilitarlas).
- **Paginación y filtros** en `GET /reservations` (por estado, cliente, rango de fechas).
- **Idempotencia** en la creación (cabecera `Idempotency-Key`) para evitar duplicados por reintentos de red, complementando la regla de duplicidad de negocio.
- **Concurrencia optimista** (versión/ETag) para las transiciones de estado en un escenario multiusuario real.
- **Tarifario configurable** (montos y porcentajes desde `appsettings`/base de datos) en lugar de constantes, con vigencias por fecha.
- **Observabilidad:** logging estructurado (Serilog), health checks y OpenTelemetry.
- **CI/CD:** pipeline que ejecute build + tests + análisis estático, y contenedor Docker para despliegue.
- **Versionado de API** (`/v1`) y `CorrelationId` en las respuestas de error.
