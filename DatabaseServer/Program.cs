using DatabaseServer.Services;

var builder = WebApplication.CreateBuilder(args);

// ������ ������
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// �������� DatabaseStorageService �� Singleton (���� ���� �� ���� ������)
builder.Services.AddSingleton<DatabaseStorageService>();

// ������������ CORS ��� ���-�볺���
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// ������������ HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();