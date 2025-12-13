using BarcodeLaminationAPI.Services;
using Microsoft.EntityFrameworkCore;
using BarcodeLaminationAPI.Data;
using BarcodeLaminationAPI.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 配置数据库连接（添加重试策略）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);
// 启用文件上传支持
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB 文件大小限制
});
// 注册服务
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IFilmCoatingService, FilmCoatingService>();
builder.Services.AddScoped<IFeedingService, FeedingService>();

// 注册打印服务
builder.Services.AddScoped<IWindowsPrintService, WindowsPrintService>();
builder.Services.AddScoped<IUnloadingService, UnloadingService>();
builder.Services.AddScoped<ITemplatePrintService, TemplatePrintService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB 文件大小限制
});

// CORS配置（允许PDA和PC端访问）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.WebHost.UseUrls("http://0.0.0.0:5200");
builder.Services.AddSingleton<IBarTenderPrintService, BarTenderPrintService>();
var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 初始化数据库（开发环境）
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();