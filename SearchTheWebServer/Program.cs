using Microsoft.EntityFrameworkCore;
using SearchTheWebServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localho.st:8000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("Content-Disposition")
               .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
               .WithExposedHeaders("Content-Type")
               .WithHeaders("Access-Control-Allow-Headers");

    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // This might cause the CORS error
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.Use((ctx,next)=>{
    ctx.Response.Headers["Access-Control-Allow-Origin"]="*";
    return next();
});

app.Use((ctx,next)=>{
    ctx.Response.Headers["Access-Control-Allow-Headers"]="*";
    return next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();