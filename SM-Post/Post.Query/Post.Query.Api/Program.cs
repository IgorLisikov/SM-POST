using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.Consumers;
using Post.Query.Infrastructure.DataAccess;
using Post.Query.Infrastructure.Handlers;
using Post.Query.Infrastructure.Repositories;
using System.Reflection;
using MediatR;
using EventHandler = Post.Query.Infrastructure.Handlers.EventHandler;
using Post.Common.Configs;
using Post.Query.Infrastructure.Config;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<SqlServerConfig>(builder.Configuration.GetSection("Sql"));
builder.Services.AddDbContext<DatabaseContext>((sp, optionsBuilder) =>
{
    var sqlConfig = sp.GetRequiredService<IOptions<SqlServerConfig>>().Value;
    optionsBuilder
        .UseLazyLoadingProxies()
        .UseSqlServer(sqlConfig.GetConnectionString())
        .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name })
        .EnableSensitiveDataLogging();
});

// Create database and tables from code:
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();
dataContext.Database.EnsureCreated();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();
builder.Services.AddScoped<IEventHandler, EventHandler>();
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<KafkaTopics>(builder.Configuration.GetSection(nameof(KafkaTopics)));
builder.Services.AddHostedService<ConsumerHostedService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
