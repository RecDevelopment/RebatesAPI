using RebatesAPI.Controllers;
using Newtonsoft;
using RebatesAPI;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// Enable multipart form data support
//builder.Services.AddMvc(options =>
//{
//    options.EnableEndpointRouting = false;
//    options.ModelBinderProviders.Insert(0, new FormDataJsonBinderProvider());
//});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();
//builder.Services.AddCors();
// Default Policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://10.177.50.7:3000/", "https://10.177.50.7:3000/")
                                .AllowAnyHeader()
                                .AllowAnyOrigin()
                                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
////app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
//app.UseAuthorization<ApiKeyMiddleware>();
app.UseCors();



app.MapControllers();

app.Run();
