namespace NuGet
{
    using System;
    using System.Runtime.Versioning;

    internal interface IProjectSystem : IFileSystem, IPropertyProvider
    {
        void AddFrameworkReference(string name);
        void AddImport(string targetFullPath, ProjectImportLocation location);
        void AddReference(string referencePath);
        bool FileExistsInProject(string path);
        bool IsSupportedFile(string path);
        bool ReferenceExists(string name);
        void RemoveImport(string targetFullPath);
        void RemoveReference(string name);
        string ResolvePath(string path);

        FrameworkName TargetFramework { get; }

        string ProjectName { get; }

        bool IsBindingRedirectSupported { get; }
    }
}

