using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Services;

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

// 添加控制器服務
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 註冊 IEmailService
builder.Services.AddSingleton<IEmailService, EmailService>();

// 創建 IConfiguration 實例並設置環境變數
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// 將 IConfiguration 添加到服務容器
builder.Services.AddSingleton(configuration);

var app = builder.Build();

// 使用 CORS 中間件
app.UseCors("AllowSpecificOrigin");

// 配置開發環境
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 配置 SignalR 端點
app.MapHub<ChatHub>("/chatHub");

app.Run();
