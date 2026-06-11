# Parche Users.API — endpoint tecnico para validacion interservicios

Este parche agrega un endpoint tecnico en Users.API:

```http
GET /api/users/{id}
```

Motivo: Notifications.API necesita validar el usuario destinatario antes de registrar una notificacion. El contrato principal de Users.API solo define register/login, pero para comunicacion entre microservicios hace falta una forma de consultar si el usuario existe.

La respuesta devuelve solo datos publicos del usuario mediante `RespuestaUsuario`; no devuelve `PasswordHash`.

Archivos modificados:

- Controllers/UsuariosController.cs
- Services/IUsuarioServicio.cs
- Services/UsuarioServicio.cs
- Repositories/IUsuarioRepositorio.cs
- Repositories/UsuarioRepositorio.cs

Prueba rapida:

```powershell
Invoke-RestMethod http://localhost:5223/api/users/a1b2c3d4-0000-0000-0000-111122223333
```

Esperado:

- HTTP 200
- Datos publicos del usuario Maria Gonzalez
- Sin PasswordHash
