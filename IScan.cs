using System;

namespace Arcaim.DI.Scanner;

public interface IScan
{
  IScan ByAppAssemblies();
  IScan ImplementationOf(Type type);
  IScan InheritedFrom(Type type);
  void WithTransientLifetime();
}