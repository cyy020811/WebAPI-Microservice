using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Entities;
using WebApi.Repositories;
using WebApi.Settings;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Service
builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items");

// AWS S3 Bucket Service
builder.Services
    .AddS3Settings()
    .AddS3Repository();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapidotnet dev-certs https --trust
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
