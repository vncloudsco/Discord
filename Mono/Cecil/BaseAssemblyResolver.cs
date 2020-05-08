namespace Mono.Cecil
{
    using Mono;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal abstract class BaseAssemblyResolver : IAssemblyResolver
    {
        private static readonly bool on_mono = (Type.GetType("Mono.Runtime") != null);
        private readonly Collection<string> directories;
        private Collection<string> gac_paths;
        private AssemblyResolveEventHandler ResolveFailure;

        public event AssemblyResolveEventHandler ResolveFailure
        {
            add
            {
                AssemblyResolveEventHandler resolveFailure = this.ResolveFailure;
                while (true)
                {
                    AssemblyResolveEventHandler a = resolveFailure;
                    AssemblyResolveEventHandler handler3 = (AssemblyResolveEventHandler) Delegate.Combine(a, value);
                    resolveFailure = Interlocked.CompareExchange<AssemblyResolveEventHandler>(ref this.ResolveFailure, handler3, a);
                    if (ReferenceEquals(resolveFailure, a))
                    {
                        return;
                    }
                }
            }
            remove
            {
                AssemblyResolveEventHandler resolveFailure = this.ResolveFailure;
                while (true)
                {
                    AssemblyResolveEventHandler source = resolveFailure;
                    AssemblyResolveEventHandler handler3 = (AssemblyResolveEventHandler) Delegate.Remove(source, value);
                    resolveFailure = Interlocked.CompareExchange<AssemblyResolveEventHandler>(ref this.ResolveFailure, handler3, source);
                    if (ReferenceEquals(resolveFailure, source))
                    {
                        return;
                    }
                }
            }
        }

        protected BaseAssemblyResolver()
        {
            Collection<string> collection = new Collection<string>(2) { 
                ".",
                "bin"
            };
            this.directories = collection;
        }

        public void AddSearchDirectory(string directory)
        {
            this.directories.Add(directory);
        }

        private AssemblyDefinition GetAssembly(string file, ReaderParameters parameters)
        {
            if (parameters.AssemblyResolver == null)
            {
                parameters.AssemblyResolver = this;
            }
            return ModuleDefinition.ReadModule(file, parameters).Assembly;
        }

        private static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
        {
            StringBuilder builder = new StringBuilder().Append(prefix).Append(reference.Version).Append("__");
            for (int i = 0; i < reference.PublicKeyToken.Length; i++)
            {
                builder.Append(reference.PublicKeyToken[i].ToString("x2"));
            }
            return Path.Combine(Path.Combine(Path.Combine(gac, reference.Name), builder.ToString()), reference.Name + ".dll");
        }

        private AssemblyDefinition GetAssemblyInGac(AssemblyNameReference reference, ReaderParameters parameters)
        {
            if ((reference.PublicKeyToken == null) || (reference.PublicKeyToken.Length == 0))
            {
                return null;
            }
            if (this.gac_paths == null)
            {
                this.gac_paths = GetGacPaths();
            }
            return (!on_mono ? this.GetAssemblyInNetGac(reference, parameters) : this.GetAssemblyInMonoGac(reference, parameters));
        }

        private AssemblyDefinition GetAssemblyInMonoGac(AssemblyNameReference reference, ReaderParameters parameters)
        {
            for (int i = 0; i < this.gac_paths.Count; i++)
            {
                string gac = this.gac_paths[i];
                string path = GetAssemblyFile(reference, string.Empty, gac);
                if (File.Exists(path))
                {
                    return this.GetAssembly(path, parameters);
                }
            }
            return null;
        }

        private AssemblyDefinition GetAssemblyInNetGac(AssemblyNameReference reference, ReaderParameters parameters)
        {
            string[] strArray = new string[] { "GAC_MSIL", "GAC_32", "GAC_64", "GAC" };
            string[] strArray2 = new string[] { string.Empty, "v4.0_" };
            int index = 0;
            while (index < 2)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= strArray.Length)
                    {
                        index++;
                        break;
                    }
                    string gac = Path.Combine(this.gac_paths[index], strArray[num2]);
                    string path = GetAssemblyFile(reference, strArray2[index], gac);
                    if (Directory.Exists(gac) && File.Exists(path))
                    {
                        return this.GetAssembly(path, parameters);
                    }
                    num2++;
                }
            }
            return null;
        }

        private AssemblyDefinition GetCorlib(AssemblyNameReference reference, ReaderParameters parameters)
        {
            Version version = reference.Version;
            if ((typeof(object).Assembly.GetName().Version == version) || IsZero(version))
            {
                return this.GetAssembly(typeof(object).Module.FullyQualifiedName, parameters);
            }
            string fullName = Directory.GetParent(Directory.GetParent(typeof(object).Module.FullyQualifiedName).FullName).FullName;
            if (on_mono)
            {
                if (version.Major == 1)
                {
                    fullName = Path.Combine(fullName, "1.0");
                }
                else if (version.Major == 2)
                {
                    fullName = (version.MajorRevision != 5) ? Path.Combine(fullName, "2.0") : Path.Combine(fullName, "2.1");
                }
                else
                {
                    if (version.Major != 4)
                    {
                        throw new NotSupportedException("Version not supported: " + version);
                    }
                    fullName = Path.Combine(fullName, "4.0");
                }
            }
            else
            {
                switch (version.Major)
                {
                    case 1:
                        fullName = (version.MajorRevision != 0xce4) ? Path.Combine(fullName, "v1.0.5000.0") : Path.Combine(fullName, "v1.0.3705");
                        break;

                    case 2:
                        fullName = Path.Combine(fullName, "v2.0.50727");
                        break;

                    case 4:
                        fullName = Path.Combine(fullName, "v4.0.30319");
                        break;

                    default:
                        throw new NotSupportedException("Version not supported: " + version);
                }
            }
            string path = Path.Combine(fullName, "mscorlib.dll");
            return (!File.Exists(path) ? null : this.GetAssembly(path, parameters));
        }

        private static string GetCurrentMonoGac() => 
            Path.Combine(Directory.GetParent(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName)).FullName, "gac");

        private static Collection<string> GetDefaultMonoGacPaths()
        {
            Collection<string> collection = new Collection<string>(1);
            string currentMonoGac = GetCurrentMonoGac();
            if (currentMonoGac != null)
            {
                collection.Add(currentMonoGac);
            }
            string environmentVariable = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                char[] separator = new char[] { Path.PathSeparator };
                foreach (string str3 in environmentVariable.Split(separator))
                {
                    if (!string.IsNullOrEmpty(str3))
                    {
                        string path = Path.Combine(Path.Combine(Path.Combine(str3, "lib"), "mono"), "gac");
                        if (Directory.Exists(path) && !collection.Contains(currentMonoGac))
                        {
                            collection.Add(path);
                        }
                    }
                }
            }
            return collection;
        }

        private static Collection<string> GetGacPaths()
        {
            if (on_mono)
            {
                return GetDefaultMonoGacPaths();
            }
            Collection<string> collection = new Collection<string>(2);
            string environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
            if (environmentVariable != null)
            {
                collection.Add(Path.Combine(environmentVariable, "assembly"));
                collection.Add(Path.Combine(environmentVariable, Path.Combine("Microsoft.NET", "assembly")));
            }
            return collection;
        }

        public string[] GetSearchDirectories()
        {
            string[] destinationArray = new string[this.directories.size];
            Array.Copy(this.directories.items, destinationArray, destinationArray.Length);
            return destinationArray;
        }

        private static bool IsZero(Version version) => 
            ((version == null) || ((version.Major == 0) && ((version.Minor == 0) && ((version.Build == 0) && (version.Revision == 0)))));

        public void RemoveSearchDirectory(string directory)
        {
            this.directories.Remove(directory);
        }

        public virtual AssemblyDefinition Resolve(AssemblyNameReference name) => 
            this.Resolve(name, new ReaderParameters());

        public virtual AssemblyDefinition Resolve(string fullName) => 
            this.Resolve(fullName, new ReaderParameters());

        public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (parameters == null)
            {
                parameters = new ReaderParameters();
            }
            AssemblyDefinition assemblyInGac = this.SearchDirectory(name, this.directories, parameters);
            if (assemblyInGac != null)
            {
                return assemblyInGac;
            }
            if (name.IsRetargetable)
            {
                AssemblyNameReference reference = new AssemblyNameReference(name.Name, new Version(0, 0, 0, 0)) {
                    PublicKeyToken = Empty<byte>.Array
                };
                name = reference;
            }
            string directoryName = Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName);
            if (IsZero(name.Version))
            {
                assemblyInGac = this.SearchDirectory(name, new string[] { directoryName }, parameters);
                if (assemblyInGac != null)
                {
                    return assemblyInGac;
                }
            }
            if (name.Name == "mscorlib")
            {
                assemblyInGac = this.GetCorlib(name, parameters);
                if (assemblyInGac != null)
                {
                    return assemblyInGac;
                }
            }
            assemblyInGac = this.GetAssemblyInGac(name, parameters);
            if (assemblyInGac != null)
            {
                return assemblyInGac;
            }
            assemblyInGac = this.SearchDirectory(name, new string[] { directoryName }, parameters);
            if (assemblyInGac != null)
            {
                return assemblyInGac;
            }
            if (this.ResolveFailure != null)
            {
                assemblyInGac = this.ResolveFailure(this, name);
                if (assemblyInGac != null)
                {
                    return assemblyInGac;
                }
            }
            throw new AssemblyResolutionException(name);
        }

        public virtual AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }
            return this.Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }

        private AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
        {
            AssemblyDefinition definition;
            string[] strArray = new string[] { ".exe", ".dll" };
            using (IEnumerator<string> enumerator = directories.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        string[] strArray3 = strArray;
                        int index = 0;
                        while (true)
                        {
                            if (index >= strArray3.Length)
                            {
                                break;
                            }
                            string path = Path.Combine(current, name.Name + strArray3[index]);
                            if (!File.Exists(path))
                            {
                                index++;
                                continue;
                            }
                            return this.GetAssembly(path, parameters);
                        }
                        continue;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                }
            }
            return definition;
        }
    }
}

