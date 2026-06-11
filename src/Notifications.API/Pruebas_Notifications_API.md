# Pruebas sugeridas — Notifications.API

Estas pruebas asumen que estan levantados al mismo tiempo:

- Users.API en http://localhost:5223
- Notifications.API en https://localhost:7270 y http://localhost:5226

Para validacion real contra Users.API, aplicar previamente el parche que agrega `GET /api/users/{id}` en Users.API.

## 1. Verificar Users.API

PowerShell:

```powershell
Invoke-RestMethod http://localhost:5223/health
Invoke-RestMethod http://localhost:5223/api/users/a1b2c3d4-0000-0000-0000-111122223333
```

Resultado esperado:

- HTTP 200
- Datos publicos del usuario
- No debe aparecer PasswordHash

## 2. Verificar Notifications.API

```powershell
Invoke-RestMethod http://localhost:5226/health
```

Resultado esperado:

- status Healthy
- sqlite-db Healthy

## 3. POST /api/notifications/send — caso exitoso

Request:

```json
{
  "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
  "mensaje": "Su orden fue confirmada.",
  "tipo": "Email"
}
```

Resultado esperado:

- HTTP 201 Created
- estado = Enviada
- tipo = Email
- fechaEnvio asignada automaticamente

## 4. POST /api/notifications/send — usuario inexistente

Request:

```json
{
  "usuarioId": "usuario-inexistente",
  "mensaje": "Mensaje de prueba.",
  "tipo": "Email"
}
```

Resultado esperado:

- HTTP 404 Not Found
- errorCode = NTF-001
- errorMessage = El usuario destinatario no fue encontrado.

## 5. POST /api/notifications/send — datos invalidos

Request:

```json
{
  "usuarioId": "",
  "mensaje": "",
  "tipo": "Fax"
}
```

Resultado esperado:

- HTTP 400 Bad Request
- errorCode = NTF-002
- errorMessage con los problemas detectados

## 6. GET /api/notifications/{userId} — listar notificaciones

Request:

```http
GET /api/notifications/a1b2c3d4-0000-0000-0000-111122223333
```

Resultado esperado:

- HTTP 200 OK
- Lista de notificaciones del usuario

## 7. GET /api/notifications/{userId} — usuario sin notificaciones

Request:

```http
GET /api/notifications/usuario-sin-notificaciones
```

Resultado esperado:

- HTTP 404 Not Found
- errorCode = NTF-003
- errorMessage = No se encontraron notificaciones para el usuario.

## 8. Validar logs

Buscar archivo:

```powershell
Get-ChildItem -Recurse -Filter "notifications-api-*.json"
```

Validar que los logs incluyan:

- Servicio = Notifications.API
- Endpoint
- CorrelationId
- StatusCode

## 9. Validar health checks

Abrir:

```text
https://localhost:7270/health
https://localhost:7270/health/ready
https://localhost:7270/health/live
```

Resultado esperado:

- /health: estado general
- /health/ready: sqlite-db + api-status
- /health/live: api-status
