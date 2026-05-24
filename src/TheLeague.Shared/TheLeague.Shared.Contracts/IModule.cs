using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheLeague.Shared.Contracts;

public interface IModule
{
    string Name { get; }
    void RegisterModule(IServiceCollection services, IConfiguration configuration);
    void UseModule(IApplicationBuilder app);
}
