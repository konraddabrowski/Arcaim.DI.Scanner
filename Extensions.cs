using System;
using Microsoft.Extensions.DependencyInjection;

namespace Arcaim.DI.Scanner
{
    public static class Extensions
    {
        public static IServiceCollection Scan(this IServiceCollection services, Action<IScan> scan)
        {
            scan.Invoke(new Scan(services));

            return services;
        }
    }
}