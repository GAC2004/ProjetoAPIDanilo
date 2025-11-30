using ProjetoAPIDanilo.Data;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços do controlador
builder.Services.AddControllers();

// Configura Database para injeção de dependência
builder.Services.AddSingleton<Database>();

// Configura Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Habilita Swagger sempre, independente do ambiente
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Escola Estoque v1");
    c.RoutePrefix = "swagger"; // Swagger ficará em /swagger
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
