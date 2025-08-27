using log4net.Config;
using log4net;
using PartnerAPI.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//add the log settings
var repo = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));

//log path settins
var logPath = Path.GetFullPath("logs/partnerapi.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
Console.WriteLine("Logging to: " + logPath);

// Add services to the container.
builder.Services.AddSingleton<PartnerAuthService>();
builder.Services.AddSingleton<SignatureService>();
builder.Services.AddSingleton<DiscountService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())); // test on the web 

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
