using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Arcaim.DI.Scanner;

public class Scan : IScan
{
  private readonly IServiceCollection _services;
  private IEnumerable<Assembly> _assemblies;
  private IEnumerable<Type> _scannedTypes;

public Scan(IServiceCollection services)
    => _services = services;

  public IScan ImplementationOf(Type searchedType)
  {
    var types = _assemblies.SelectMany(assembly => assembly.GetTypes());
    if (searchedType.IsAbstract && !searchedType.IsInterface)
    {
      _scannedTypes = types.Where(x => x.BaseType is not null && !x.ContainsGenericParameters &&
        x.BaseType.Name.Equals(searchedType.Name, StringComparison.InvariantCulture));

        return this;
    }

    _scannedTypes = types.Where(type => !type.IsInterface && !type.IsAbstract && type.GetInterfaces()
      .Any(y => y.Name.Equals(searchedType.Name, StringComparison.InvariantCulture)));

    return this;
  }

  public IScan InheritedFrom(Type searchedType)
  {
    _scannedTypes = _assemblies.SelectMany(assembly => assembly.GetTypes())
      .Where(type => type.IsClass &&
        !type.IsAbstract &&
        !type.IsInterface &&
        !type.IsGenericType &&
        type.BaseType.Name.Equals(searchedType.Name, StringComparison.InvariantCulture));

    return this;
  }

  public IScan ByAppAssemblies()
  {
    _assemblies = Directory
      .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
      .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));

    return this;
  }

  public void WithTransientLifetime() => _scannedTypes.ToList().ForEach(type =>
  {
    if (type.BaseType.IsAbstract)
    {
      _services.AddTransient(type.BaseType, type);

      return;
    }

    type.GetInterfaces().ToList().ForEach(implementedInterfaces =>
      _services.AddTransient(implementedInterfaces, type)
    );
  });
}