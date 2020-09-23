using System;
using Marchive.App.IO;
using Marchive.App.Services;
using Marchive.App.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Marchive.App
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMarchive(this IServiceCollection services, Action<MArchiveSettings> config = null)
        {
            var cfg = new MArchiveSettings();
            config?.Invoke(cfg);

            services.AddSingleton(cfg);
            services.AddSingleton<IFileSystem, FileSystemProxy>();

            services.AddScoped<Archiver>();
            services.AddScoped<IMarchive, Services.Marchive>();
        }
    }
}
