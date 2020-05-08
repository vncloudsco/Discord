namespace Mono.Cecil
{
    using System;

    internal interface IAssemblyResolver
    {
        AssemblyDefinition Resolve(AssemblyNameReference name);
        AssemblyDefinition Resolve(string fullName);
        AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters);
        AssemblyDefinition Resolve(string fullName, ReaderParameters parameters);
    }
}

