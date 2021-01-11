namespace DeltaX.RestApiDemo1
{
    using DeltaX.LinSql.Table;
    using DeltaX.RestApiDemo1.Repository;
    using DeltaX.RestApiDemo1.SqliteHelper;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using System.Data;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("RestApiDemo");

            DapperSqliteTypeHandler.SetSqliteTypeHandler();
            services.AddSingleton<TableQueryFactory>(s => new TableQueryFactory(DialectType.SQLite));
            services.AddTransient<IDbConnection, SqliteConnection>(s =>
            {
                var db = new SqliteConnection(connectionString);
                db.Open();
                return db;
            });
            services.AddTransient<IUserRepository, UserRepository>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeltaX.RestApiDemo1", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CreateTable(app, env);
            ConfigureTable(app, env);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeltaX.RestApiDemo1 v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void CreateTable(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Create todo schema
            var connection = app.ApplicationServices.GetService<IDbConnection>();
            var logger = app.ApplicationServices.GetService<ILogger>();
            var tableCrator = new TableCrator(connection, logger);
            tableCrator.Start();
        }
        
        public void ConfigureTable(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Create todo schema
            var queryFactory = app.ApplicationServices.GetService<TableQueryFactory>();
            ConfigureTables.Configure(queryFactory);
        }
    }
}
