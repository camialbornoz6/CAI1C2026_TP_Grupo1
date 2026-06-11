# Pruebas sugeridas - Cart.API

Estas pruebas asumen que `Products.API` esta levantado al mismo tiempo que `Cart.API`.

## Servicios necesarios

- Products.API: `https://localhost:7266/swagger` y `http://localhost:5222`
- Cart.API: `https://localhost:7269/swagger` y `http://localhost:5225`

Antes de probar el carrito, verificar que Products.API responda:

```powershell
Invoke-RestMethod http://localhost:5222/health
Invoke-RestMethod http://localhost:5222/api/products
```

Copiar un `id` real de producto y usarlo en las pruebas.

---

## 1. Health checks

### GET /health

Esperado: `200 OK` con estado general `Healthy`.

### GET /health/ready

Esperado: `200 OK` con check de SQLite.

### GET /health/live

Esperado: `200 OK` con check de API viva.

---

## 2. Obtener carrito existente

```http
GET /api/cart/a1b2c3d4-0000-0000-0000-111122223333
```

Esperado:

```text
200 OK
```

La respuesta debe tener `usuarioId`, `items` y `fechaActualizacion`.

---

## 3. Obtener carrito inexistente

```http
GET /api/cart/usuario-sin-carrito
```

Esperado:

```text
404 Not Found
CRT-001
```

---

## 4. Agregar producto al carrito

```http
POST /api/cart/usuario-prueba/items
```

Body:

```json
{
  "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cantidad": 1
}
```

Esperado:

```text
200 OK
```

Si el carrito no existia, se crea automaticamente. Si el producto ya existia, suma la cantidad solicitada.

---

## 5. Agregar producto inexistente

```http
POST /api/cart/usuario-prueba/items
```

Body:

```json
{
  "productoId": "producto-inexistente",
  "cantidad": 1
}
```

Esperado:

```text
404 Not Found
CRT-002
```

---

## 6. Agregar cantidad invalida

```http
POST /api/cart/usuario-prueba/items
```

Body:

```json
{
  "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cantidad": 0
}
```

Esperado:

```text
400 Bad Request
CRT-004
```

---

## 7. Agregar mas stock del disponible

```http
POST /api/cart/usuario-prueba/items
```

Body:

```json
{
  "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cantidad": 999999
}
```

Esperado:

```text
422 Unprocessable Entity
CRT-003
```

---

## 8. Actualizar cantidad de un item existente

```http
PUT /api/cart/usuario-prueba/items/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Body:

```json
{
  "cantidad": 2
}
```

Esperado:

```text
200 OK
```

---

## 9. Actualizar item con stock insuficiente

```http
PUT /api/cart/usuario-prueba/items/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Body:

```json
{
  "cantidad": 999999
}
```

Esperado:

```text
422 Unprocessable Entity
CRT-003
```

---

## 10. Quitar producto del carrito

```http
DELETE /api/cart/usuario-prueba/items/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Esperado:

```text
204 No Content
```

---

## 11. Vaciar carrito completo

Primero agregar un producto y luego ejecutar:

```http
DELETE /api/cart/usuario-prueba
```

Esperado:

```text
204 No Content
```

Luego:

```http
GET /api/cart/usuario-prueba
```

Esperado:

```text
404 Not Found
CRT-001
```
