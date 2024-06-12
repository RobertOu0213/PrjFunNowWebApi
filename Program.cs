using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PrjFunNowWebApi.Services;

var builder = WebApplication.CreateBuilder(args);



// �]�w��Ʈw�s�u
builder.Services.AddDbContext<FunNowContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("FunNowConnection")
));

// �K�[ CORS �A��
builder.Services.AddCors(options =>
{
    // �w�q���\�Ҧ��ӷ�������
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());

    // �w�q���\�S�w�ӷ�������
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://localhost:7284")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
});

// �K�[ SignalR �A��
builder.Services.AddSignalR();

// �K�[����A��
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();


// ���U IEmailService
builder.Services.AddSingleton<IEmailService, EmailService>();

// �Ы� IConfiguration ��Ҩó]�m�����ܼ�
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
var tkConf = builder.Configuration.GetSection("Jwt");

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true, //���ϥΪ̥i�H���D�o���
    ValidateAudience = true,
    ValidateLifetime = true, //�i�H�w��L����token�����ڵ�
    ValidateIssuerSigningKey = true,
    ValidIssuer = tkConf["Issuer"],
    ValidAudience = tkConf["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tkConf["Key"]))
};
// �N IConfiguration �K�[��A�Ȯe��
builder.Services.AddSingleton(configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = tokenValidationParameters;
    });


var app = builder.Build();

// �ϥ� CORS ������
app.UseCors("AllowSpecificOrigin");

// �t�m�}�o����
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// �t�m SignalR ���I
app.MapHub<ChatHub>("/chatHub");

app.Run();
