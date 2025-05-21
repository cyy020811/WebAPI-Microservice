using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using WebApi.Entities;
using WebApi.Settings;

namespace WebApi.Repositories
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {

            services.AddSingleton(serviceProvider =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);

                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();

                return new MongoRepository<T>(database, collectionName);
            });

            return services;
        }

        public static IServiceCollection AddS3Settings(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

            services.Configure<S3Settings>(configuration.GetSection(nameof(S3Settings)));

            return services;
        }

        public static IServiceCollection AddS3Repository(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonS3>(serviceProvider =>
            {
                var s3Settings = serviceProvider.GetRequiredService<IOptions<S3Settings>>().Value;
                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region),
                };

                return new AmazonS3Client(s3Config);
            });

            services.AddSingleton<S3Repository>();

            return services;
        }
    }
}