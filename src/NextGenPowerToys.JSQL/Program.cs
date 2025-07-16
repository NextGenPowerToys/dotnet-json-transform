using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NextGenPowerToys.JSQL.Core;
using NextGenPowerToys.JSQL.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NextGenPowerToys.JSQL
{
    /// <summary>
    /// Program entry point for NextGenPowerToys.JSQL library
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for console application (optional - library can be used without this)
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Exit code</returns>
        public static async Task<int> Main(string[] args)
        {
            // Build the service container
            var host = CreateHostBuilder(args).Build();
            
            // Get the transformation analyzer
            var analyzer = host.Services.GetRequiredService<ITransformationAnalyzer>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("NextGenPowerToys.JSQL Library initialized successfully");
            
            // Example usage if run as console app
            if (args.Length >= 2)
            {
                var jsonExample = args[0];
                var sqlQuery = args[1];
                
                try
                {
                    var result = await analyzer.AnalyzeTransformation(jsonExample, sqlQuery);
                    
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var resultJson = JsonSerializer.Serialize(result, options);
                    
                    Console.WriteLine("Analysis Result:");
                    Console.WriteLine(resultJson);
                    
                    return result.IsSuccess ? 0 : 1;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during analysis");
                    Console.WriteLine($"Error: {ex.Message}");
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("NextGenPowerToys.JSQL Library");
                Console.WriteLine("Usage: NextGenPowerToys.JSQL <json-example> <sql-query>");
                Console.WriteLine("Or use as a library by referencing NextGenPowerToys.JSQL");
            }
            
            return 0;
        }
        
        /// <summary>
        /// Creates and configures the host builder for dependency injection
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Configured host builder</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                });
        
        /// <summary>
        /// Configures dependency injection services for the JSQL library
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        public static void ConfigureServices(IServiceCollection services)
        {
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            // Register core services
            services.AddSingleton<ITransformationAnalyzer, TransformationAnalyzer>();
            
            // Register individual service implementations
            services.AddSingleton<IJsonAnalyzer, JsonAnalyzer>();
            services.AddSingleton<ISqlParser, SqlParser>();
            services.AddSingleton<ICompatibilityValidator, CompatibilityValidator>();
            services.AddSingleton<ITemplateGenerator, TemplateGenerator>();
        }
    }
    
    /// <summary>
    /// Extension methods for service registration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds NextGenPowerToys.JSQL services to the dependency injection container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddNextGenPowerToysJSQL(this IServiceCollection services)
        {
            Program.ConfigureServices(services);
            return services;
        }
        
        /// <summary>
        /// Adds NextGenPowerToys.JSQL services with custom configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Configuration action</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddNextGenPowerToysJSQL(
            this IServiceCollection services, 
            Action<JSQLOptions> configure)
        {
            var options = new JSQLOptions();
            configure(options);
            
            services.AddSingleton(options);
            Program.ConfigureServices(services);
            
            return services;
        }
    }
    
    /// <summary>
    /// Configuration options for NextGenPowerToys.JSQL
    /// </summary>
    public class JSQLOptions
    {
        /// <summary>
        /// Maximum complexity allowed for SQL queries
        /// </summary>
        public double MaxComplexity { get; set; } = 10.0;
        
        /// <summary>
        /// Maximum nesting depth for JSON analysis
        /// </summary>
        public int MaxNestingDepth { get; set; } = 10;
        
        /// <summary>
        /// Timeout for analysis operations in milliseconds
        /// </summary>
        public int AnalysisTimeoutMs { get; set; } = 30000; // 30 seconds
        
        /// <summary>
        /// Whether to enable aggressive optimizations
        /// </summary>
        public bool EnableOptimizations { get; set; } = true;
        
        /// <summary>
        /// Whether to generate detailed error messages
        /// </summary>
        public bool DetailedErrors { get; set; } = true;
        
        /// <summary>
        /// Custom SQL function compatibility overrides
        /// </summary>
        public Dictionary<string, bool> FunctionCompatibilityOverrides { get; set; } = new();
    }
}
