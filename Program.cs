using ApbdTest.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IDbService, DbService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();

app.Run();