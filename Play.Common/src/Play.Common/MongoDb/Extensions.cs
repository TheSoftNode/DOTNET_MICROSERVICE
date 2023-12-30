using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Play.Common.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Play.Common.MongoDb;

public static class Extensions
{
	public static void ConfigureMongoServices(this WebApplicationBuilder builder)
	{
		// Add services to the container.
		BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
		BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

		// Configuration
		var configuration = builder.Configuration;
		var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
		var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

		// MongoDB setup
		builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
		{
			return new MongoClient(mongoDbSettings.ConnectionString);
		});

		builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
		{
			var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
			return mongoClient.GetDatabase(serviceSettings.ServiceName);
		});

	}

	public static void ConfigureRepositoryServices<T>(this WebApplicationBuilder builder, string collectionName) 
		where T : IEntity
	{

		builder.Services.AddSingleton<IRepository<T>>(serviceProvider =>
		{
			var database = serviceProvider.GetRequiredService<IMongoDatabase>();
			return new MongoRepository<T>(database, collectionName);
		});

	}
}
