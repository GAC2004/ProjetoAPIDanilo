using ProjetoAPIDanilo.Data;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// Serviços
// ================================================
builder.Services.AddSingleton<Database>();

builder.Services.AddControllers();

// ---- Adiciona Swagger ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================================================
// Pipeline
// ================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
