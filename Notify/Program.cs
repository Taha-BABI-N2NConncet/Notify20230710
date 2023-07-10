using Encryption;
using Notify.Classes;
using Notify.Interfaces;
using Notify.Repositories;
using Notify.Repositories.NOVUSettingsClasses;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
Log.Logger = new LoggerConfiguration()
   .WriteTo.Console(new CompactJsonFormatter())
   .CreateLogger();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Serilog.ILogger, NotifyLogger>();
builder.Services.AddTransient<IIdentityGenerator, IdentityGenerator>();
builder.Services.AddTransient<INOVUContext, NOVUContext>();
BackgroundQueueLogger.StartProcessLogging(Serilog.Log.Logger);

var sponsors = LoadJson();
//string apikey = AES.ATPEncrypt("fc1e46c15ff95a6e2e90ea7acc6610a0");
for (int i = 0; i < sponsors.Count(); i++)
{
    sponsors[i].APIKeyEncryption = AES.ATPDecrypt(sponsors[i].APIKeyEncryption);
}
//string apikey = AES.ATPEncrypt("1310af94eafa29d62bbeb697b66397e4");
//sponsors[0].APIKeyEncryption = AES.ATPDecrypt(sponsors[0].APIKeyEncryption);
builder.Services.AddSingleton(sponsors);
LoggingEnable loggingEnable = new LoggingEnable(){ Enabled = true };
builder.Services.AddSingleton<ILoggingEnable>(loggingEnable);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "V2");
    });
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();



List<Sponsor> LoadJson()
{
    using (StreamReader r = new StreamReader("./Repositories/NovuSettings.json"))
    {
        string json = r.ReadToEnd();
        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sponsor>>(json);
    }
}