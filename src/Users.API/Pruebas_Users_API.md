# Pruebas sugeridas para Users.API

Base URL sugerida para Swagger:

```text
https://localhost:7267/swagger
```

Health checks:

```text
https://localhost:7267/health
https://localhost:7267/health/ready
https://localhost:7267/health/live
```

## 1. Health check general

**Request**

```http
GET /health
```

**Esperado**

```text
200 OK
status = Healthy
service = Users.API
```

## 2. Registrar usuario valido

**Request**

```http
POST /api/users/register
```

**Body**

```json
{
  "nombre": "Carlos",
  "apellido": "Ramirez",
  "email": "carlos.ramirez@email.com",
  "password": "Password123!"
}
```

**Esperado**

```text
201 Created
```

La respuesta debe incluir `id`, `nombre`, `apellido`, `email`, `fechaRegistro` y `activo`.
No debe incluir `passwordHash`.

## 3. Registrar usuario duplicado

Repetir el mismo request anterior.

**Esperado**

```text
409 Conflict
errorCode = USR-001
```

## 4. Registrar usuario invalido

**Request**

```http
POST /api/users/register
```

**Body**

```json
{
  "nombre": "",
  "apellido": "",
  "email": "email-mal-formado",
  "password": "123"
}
```

**Esperado**

```text
400 Bad Request
errorCode = USR-002
```

## 5. Login correcto con usuario seed

**Request**

```http
POST /api/users/login
```

**Body**

```json
{
  "email": "maria@email.com",
  "password": "MiPassword123!"
}
```

**Esperado**

```text
200 OK
```

La respuesta debe incluir `id`, `nombre`, `apellido`, `email`.
No debe incluir `passwordHash`, `fechaRegistro`, `activo` ni `intentosFallidos`.

## 6. Login con credenciales incorrectas

**Request**

```http
POST /api/users/login
```

**Body**

```json
{
  "email": "juan@email.com",
  "password": "PasswordIncorrecta"
}
```

**Esperado en el intento 1 y 2**

```text
401 Unauthorized
errorCode = USR-003
```

## 7. Bloqueo al tercer intento fallido

Ejecutar por tercera vez el login incorrecto para `juan@email.com`.

**Esperado en el intento 3**

```text
403 Forbidden
errorCode = USR-004
```

A partir de ese momento, incluso con la password correcta, el usuario debe seguir bloqueado:

```json
{
  "email": "juan@email.com",
  "password": "Password123!"
}
```

**Esperado**

```text
403 Forbidden
errorCode = USR-004
```

## 8. Usuario bloqueado por fraude simulado

La base inicial contiene un usuario seed bloqueado con motivo `Fraude` para poder demostrar `USR-005`.

**Request**

```http
POST /api/users/login
```

**Body**

```json
{
  "email": "ana.bloqueada@email.com",
  "password": "Password123!"
}
```

**Esperado**

```text
403 Forbidden
errorCode = USR-005
```

## 9. Usuario inexistente en login

**Request**

```http
POST /api/users/login
```

**Body**

```json
{
  "email": "noexiste@email.com",
  "password": "Password123!"
}
```

**Esperado**

```text
401 Unauthorized
errorCode = USR-003
```

## 10. Reset de base para repetir pruebas

Cerrar Users.API y borrar `users.db`:

```powershell
Get-ChildItem -Recurse -Filter users.db | Remove-Item -Force
```

Al volver a levantar Users.API, el inicializador recrea la base y los datos seed.
