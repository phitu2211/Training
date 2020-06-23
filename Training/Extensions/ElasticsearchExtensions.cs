using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Training.Extensions
{
    public static class ElasticsearchExtensions
    {
       public static void AddElasticsearch(
               this IServiceCollection services, IConfiguration configuration)
           {
               var url = configuration["ElasticConfiguration:Uri"];
               var defaultIndex = configuration["ElasticConfiguration:DefaultIndex"];
       
               var settings = new ConnectionSettings(new Uri(url))
                   .DefaultIndex(defaultIndex);
       
               var client = new ElasticClient(settings);
       
               services.AddSingleton<IElasticClient>(client);
           } 
    }
}