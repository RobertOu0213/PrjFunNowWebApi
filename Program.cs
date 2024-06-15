using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PrjFunNowWebApi.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// 設定資料庫連線
builder.Services.AddDbContext<FunNowContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("FunNowConnection")
));

// 添加 CORS 服務
builder.Services.AddCors(options =>
{
    // 定義允許所有來源的策略
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());

   // 定義允許特定來源的策略
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://localhost:7284")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());


  

});

// 添加 SignalR 服務
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

// Add services to the container.

// 添加控制器服務
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();


// 註冊 IEmailService
builder.Services.AddSingleton<IEmailService, EmailService>();

// 創建 IConfiguration 實例並設置環境變數
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
var tkConf = builder.Configuration.GetSection("Jwt");

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true, //讓使用者可以知道發行者
    ValidateAudience = true,
    ValidateLifetime = true, //可以針對過期的token給予拒絕
    ValidateIssuerSigningKey = true,
    ValidIssuer = tkConf["Issuer"],
    ValidAudience = tkConf["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tkConf["Key"]))
};
// 將 IConfiguration 添加到服務容器
builder.Services.AddSingleton(configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = tokenValidationParameters;
    });


// 添加内存缓存
builder.Services.AddDistributedMemoryCache();

// 添加 Session 服務
builder.Services.AddSession(options =>
{
    // 設置 Session 的 cookie 名稱
    options.Cookie.Name = ".YourApp.Session";

    // 設置 Session 的過期時間
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // 設置 cookie 是不是只在 HTTPS 中有效
    options.Cookie.HttpOnly = true;

    // 設置 cookie 的安全等級
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");
// 使用 CORS 中間件
app.UseCors("AllowSpecificOrigin");



// 配置開發環境
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession(); //註冊Session 服務
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 配置 SignalR 端點
app.MapHub<ChatHub>("/chatHub");

app.Run();
