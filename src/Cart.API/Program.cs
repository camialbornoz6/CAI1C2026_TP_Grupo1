using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Cart.API.Clients;
using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Repositories;
using Cart.API.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Cart.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(new JsonFormatter(), "logs/cart-api-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        string correlationId = context.HttpContext.Response.Headers.TryGetValue("X-Correlation-Id", out var valor)
            ? valor.ToString()
            : context.HttpContext.TraceIdentifier;

        string mensaje = string.Join("; ",
            context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                    ? "Los datos del carrito son invalidos."
                    : e.ErrorMessage));

        var respuesta = new RespuestaError
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Los datos del carrito son invalidos.",
            Instance = context.HttpContext.Request.Path.Value ?? string.Empty,
            ErrorCode = "CRT-004",
            ErrorMessage = mensaje,
            CorrelationId = correlationId
        };

        return new BadRequestObjectResult(respuesta);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<InicializadorBaseDatos>();
builder.Services.AddScoped<ICarritoRepositorio, CarritoRepositorio>();
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();
builder.Services.AddScoped<IProductosCliente, ProductosHttpCliente>();

builder.Services.AddHttpClient("Products.API", client =>
{
    string baseUrl = builder.Configuration.GetValue<string>("ExternalServices:ProductsApi:BaseUrl") ?? "http://localhost:5222";
    client.BaseAddress = new Uri(AsegurarSlashFinal(baseUrl));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHealthChecks()
    .AddCheck<ApiStatusHealthCheck>("api-status", tags: new[] { "live", "ready", "api" })
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "ready", "database" });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var inicializador = scope.ServiceProvider.GetRequiredService<InicializadorBaseDatos>();
    inicializador.Inicializar();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    string correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var valor)
        ? valor.ToString()
        : Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var stopwatch = Stopwatch.StartNew();

    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("Endpoint", context.Request.Path.Value))
    {
        logger.LogInformation("Inicio request {Metodo} {Path}",
            context.Request.Method,
            context.Request.Path.Value);

        await next();

        stopwatch.Stop();

        logger.LogInformation("Fin request {Metodo} {Path}. StatusCode: {StatusCode}. DuracionMs: {DuracionMs}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null || httpContext.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : LogEventLevel.Information;

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Endpoint", httpContext.Request.Path.Value);
        diagnosticContext.Set("CorrelationId", httpContext.Response.Headers["X-Correlation-Id"].ToString());
        diagnosticContext.Set("Servicio", "Cart.API");
    };
});

app.UseExceptionHandler();

app.MapControllers();

MapearHealthChecks(app);

app.Run();

static string AsegurarSlashFinal(string baseUrl)
{
    return baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";
}

static void MapearHealthChecks(WebApplication app)
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = EscribirRespuestaHealthCheck
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = EscribirRespuestaHealthCheck
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = EscribirRespuestaHealthCheck
    });
}

static Task EscribirRespuestaHealthCheck(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var respuesta = new
    {
        status = report.Status.ToString(),
        service = "Cart.API",
        timestampUtc = DateTime.UtcNow,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            durationMs = e.Value.Duration.TotalMilliseconds,
            data = e.Value.Data
        })
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}
