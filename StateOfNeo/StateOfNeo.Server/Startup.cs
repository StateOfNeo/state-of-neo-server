using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;
using Neo.Network;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Seed;
using StateOfNeo.Infrastructure.Mapping;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace StateOfNeo.Server
{
    public class Startup
    {
        public static LocalNode localNode;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            this.StartBlockchain();
        }

        public ILoggerFactory LoggerFactory { get; set; }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Automapper configuration initialization
            AutoMapperConfig.Init();

            services.Configure<NetSettings>(Configuration.GetSection("NetSettings"));

            services.AddSingleton<NodeCache>();
            services.AddSingleton<NodeSynchronizer>();
            services.AddSingleton<RPCNodeCaller>(); 
            services.AddSingleton<LocationCaller>();

            services.AddDbContext<StateOfNeoContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            })
            .AddEntityFrameworkSqlServer();

            services.AddTransient<StateOfNeoSeedData>();

            services.AddSingleton<NotificationEngine>();

            services.AddCors();
            services.AddSignalR();
            services.AddMvc(options =>
            {
                options.SslPort = 5001;
                options.Filters.Add(new RequireHttpsAttribute());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            StateOfNeoSeedData seeder,
            NotificationEngine notificationEngine)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Shows UseCors with CorsPolicyBuilder.
            app.UseCors(builder =>
            {
                builder
                    .WithOrigins("http://localhost:8111", "http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<BlockHub>("/hubs/block");
                routes.MapHub<NodeHub>("/hubs/node");
            });

            //app.UseHttpsRedirection();
            seeder.Init();

            notificationEngine.Init();

            app.UseMvc();
        }

        public void StartBlockchain()
        {
            Neo.Core.Blockchain.RegisterBlockchain(new LevelDBBlockchain(NeoSettings.Default.DataDirectoryPath));

            localNode = new LocalNode();
            Task.Run(() =>
            {
                const string acc_path = "chain.acc";
                const string acc_zip_path = acc_path + ".zip";
                if (File.Exists(acc_path))
                {
                    using (FileStream fs = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        ImportBlocks(fs);
                    }
                    File.Delete(acc_path);
                }
                else if (File.Exists(acc_zip_path))
                {
                    using (FileStream fs = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                    using (Stream zs = zip.GetEntry(acc_path).Open())
                    {
                        ImportBlocks(zs);
                    }
                    File.Delete(acc_zip_path);
                }

                localNode.Start(NeoSettings.Default.NodePort, NeoSettings.Default.WsPort);
            });
        }

        private void ImportBlocks(Stream stream)
        {
            LevelDBBlockchain blockchain = (LevelDBBlockchain)Neo.Core.Blockchain.Default;
            blockchain.VerifyBlocks = false;
            using (BinaryReader r = new BinaryReader(stream))
            {
                uint count = r.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    byte[] array = r.ReadBytes(r.ReadInt32());
                    if (height > Neo.Core.Blockchain.Default.Height)
                    {
                        Block block = array.AsSerializable<Block>();
                        Neo.Core.Blockchain.Default.AddBlock(block);
                    }
                }
            }
            blockchain.VerifyBlocks = true;
        }
    }
}
