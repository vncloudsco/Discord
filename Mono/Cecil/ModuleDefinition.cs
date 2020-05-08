namespace Mono.Cecil
{
    using Mono;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Metadata;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class ModuleDefinition : ModuleReference, ICustomAttributeProvider, IMetadataTokenProvider
    {
        internal Mono.Cecil.PE.Image Image;
        internal Mono.Cecil.MetadataSystem MetadataSystem;
        internal Mono.Cecil.ReadingMode ReadingMode;
        internal ISymbolReaderProvider SymbolReaderProvider;
        internal ISymbolReader symbol_reader;
        internal IAssemblyResolver assembly_resolver;
        internal IMetadataResolver metadata_resolver;
        internal Mono.Cecil.TypeSystem type_system;
        private readonly MetadataReader reader;
        private readonly string fq_name;
        internal string runtime_version;
        internal ModuleKind kind;
        private TargetRuntime runtime;
        private TargetArchitecture architecture;
        private ModuleAttributes attributes;
        private ModuleCharacteristics characteristics;
        private Guid mvid;
        internal AssemblyDefinition assembly;
        private MethodDefinition entry_point;
        private Mono.Cecil.MetadataImporter importer;
        private Collection<CustomAttribute> custom_attributes;
        private Collection<AssemblyNameReference> references;
        private Collection<ModuleReference> modules;
        private Collection<Resource> resources;
        private Collection<ExportedType> exported_types;
        private TypeDefinitionCollection types;
        private readonly object module_lock;

        internal ModuleDefinition()
        {
            this.module_lock = new object();
            this.MetadataSystem = new Mono.Cecil.MetadataSystem();
            base.token = new MetadataToken(TokenType.Module, 1);
        }

        internal ModuleDefinition(Mono.Cecil.PE.Image image) : this()
        {
            this.Image = image;
            this.kind = image.Kind;
            this.RuntimeVersion = image.RuntimeVersion;
            this.architecture = image.Architecture;
            this.attributes = image.Attributes;
            this.characteristics = image.Characteristics;
            this.fq_name = image.FileName;
            this.reader = new MetadataReader(this);
        }

        private static void CheckContext(IGenericParameterProvider context, ModuleDefinition module)
        {
            if ((context != null) && !ReferenceEquals(context.Module, module))
            {
                throw new ArgumentException();
            }
        }

        private static void CheckField(object field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
        }

        private static void CheckFullName(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }
            if (fullName.Length == 0)
            {
                throw new ArgumentException();
            }
        }

        private static void CheckMethod(object method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
        }

        private static void CheckStream(object stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
        }

        private static void CheckType(object type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
        }

        private static AssemblyNameDefinition CreateAssemblyName(string name)
        {
            if (name.EndsWith(".dll") || name.EndsWith(".exe"))
            {
                name = name.Substring(0, name.Length - 4);
            }
            return new AssemblyNameDefinition(name, new Version(0, 0, 0, 0));
        }

        public static ModuleDefinition CreateModule(string name, ModuleKind kind)
        {
            ModuleParameters parameters = new ModuleParameters {
                Kind = kind
            };
            return CreateModule(name, parameters);
        }

        public static ModuleDefinition CreateModule(string name, ModuleParameters parameters)
        {
            Mixin.CheckName(name);
            Mixin.CheckParameters(parameters);
            ModuleDefinition definition = new ModuleDefinition {
                Name = name,
                kind = parameters.Kind,
                Runtime = parameters.Runtime,
                architecture = parameters.Architecture,
                mvid = Guid.NewGuid(),
                Attributes = ModuleAttributes.ILOnly,
                Characteristics = ModuleCharacteristics.TerminalServerAware | ModuleCharacteristics.NoSEH | ModuleCharacteristics.NXCompat | ModuleCharacteristics.DynamicBase
            };
            if (parameters.AssemblyResolver != null)
            {
                definition.assembly_resolver = parameters.AssemblyResolver;
            }
            if (parameters.MetadataResolver != null)
            {
                definition.metadata_resolver = parameters.MetadataResolver;
            }
            if (parameters.Kind != ModuleKind.NetModule)
            {
                AssemblyDefinition definition2 = new AssemblyDefinition();
                definition.assembly = definition2;
                definition.assembly.Name = CreateAssemblyName(name);
                definition2.main_module = definition;
            }
            definition.Types.Add(new TypeDefinition(string.Empty, "<Module>", TypeAttributes.AnsiClass));
            return definition;
        }

        private static ImportGenericContext GenericContextFor(IGenericParameterProvider context)
        {
            if (context != null)
            {
                return new ImportGenericContext(context);
            }
            return new ImportGenericContext();
        }

        public ImageDebugDirectory GetDebugHeader(out byte[] header)
        {
            if (!this.HasDebugHeader)
            {
                throw new InvalidOperationException();
            }
            return this.Image.GetDebugHeader(out header);
        }

        private static Stream GetFileStream(string fileName, FileMode mode, FileAccess access, FileShare share)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0)
            {
                throw new ArgumentException();
            }
            return new FileStream(fileName, mode, access, share);
        }

        public IEnumerable<MemberReference> GetMemberReferences()
        {
            if (!this.HasImage)
            {
                return Empty<MemberReference>.Array;
            }
            return this.Read<ModuleDefinition, IEnumerable<MemberReference>>(this, (_, reader) => reader.GetMemberReferences());
        }

        private TypeDefinition GetNestedType(string fullname)
        {
            string[] strArray = fullname.Split(new char[] { '/' });
            TypeDefinition type = this.GetType(strArray[0]);
            if (type == null)
            {
                return null;
            }
            for (int i = 1; i < strArray.Length; i++)
            {
                TypeDefinition nestedType = type.GetNestedType(strArray[i]);
                if (nestedType == null)
                {
                    return null;
                }
                type = nestedType;
            }
            return type;
        }

        public TypeDefinition GetType(string fullName)
        {
            CheckFullName(fullName);
            return ((fullName.IndexOf('/') <= 0) ? ((TypeDefinitionCollection) this.Types).GetType(fullName) : this.GetNestedType(fullName));
        }

        public TypeReference GetType(string fullName, bool runtimeName) => 
            (runtimeName ? TypeParser.ParseType(this, fullName) : this.GetType(fullName));

        public TypeDefinition GetType(string @namespace, string name)
        {
            Mixin.CheckName(name);
            return ((TypeDefinitionCollection) this.Types).GetType(@namespace ?? string.Empty, name);
        }

        private TypeReference GetTypeReference(string scope, string fullname) => 
            this.Read<Row<string, string>, TypeReference>(new Row<string, string>(scope, fullname), (row, reader) => reader.GetTypeReference(row.Col1, row.Col2));

        public IEnumerable<TypeReference> GetTypeReferences()
        {
            if (!this.HasImage)
            {
                return Empty<TypeReference>.Array;
            }
            return this.Read<ModuleDefinition, IEnumerable<TypeReference>>(this, (_, reader) => reader.GetTypeReferences());
        }

        public IEnumerable<TypeDefinition> GetTypes() => 
            GetTypes(this.Types);

        private static IEnumerable<TypeDefinition> GetTypes(Collection<TypeDefinition> types) => 
            new <GetTypes>d__14(-2) { <>3__types = types };

        public bool HasTypeReference(string fullName) => 
            this.HasTypeReference(string.Empty, fullName);

        public bool HasTypeReference(string scope, string fullName)
        {
            CheckFullName(fullName);
            return (this.HasImage ? !ReferenceEquals(this.GetTypeReference(scope, fullName), null) : false);
        }

        public FieldReference Import(FieldReference field)
        {
            CheckField(field);
            if (ReferenceEquals(field.Module, this))
            {
                return field;
            }
            ImportGenericContext context = new ImportGenericContext();
            return this.MetadataImporter.ImportField(field, context);
        }

        public MethodReference Import(MethodReference method) => 
            this.Import(method, null);

        public TypeReference Import(TypeReference type)
        {
            CheckType(type);
            if (ReferenceEquals(type.Module, this))
            {
                return type;
            }
            ImportGenericContext context = new ImportGenericContext();
            return this.MetadataImporter.ImportType(type, context);
        }

        public FieldReference Import(FieldInfo field) => 
            this.Import(field, null);

        public MethodReference Import(MethodBase method)
        {
            CheckMethod(method);
            ImportGenericContext context = new ImportGenericContext();
            return this.MetadataImporter.ImportMethod(method, context, ImportGenericKind.Definition);
        }

        public TypeReference Import(Type type) => 
            this.Import(type, null);

        public FieldReference Import(FieldReference field, IGenericParameterProvider context)
        {
            CheckField(field);
            if (ReferenceEquals(field.Module, this))
            {
                return field;
            }
            CheckContext(context, this);
            return this.MetadataImporter.ImportField(field, GenericContextFor(context));
        }

        public MethodReference Import(MethodReference method, IGenericParameterProvider context)
        {
            CheckMethod(method);
            if (ReferenceEquals(method.Module, this))
            {
                return method;
            }
            CheckContext(context, this);
            return this.MetadataImporter.ImportMethod(method, GenericContextFor(context));
        }

        public TypeReference Import(TypeReference type, IGenericParameterProvider context)
        {
            CheckType(type);
            if (ReferenceEquals(type.Module, this))
            {
                return type;
            }
            CheckContext(context, this);
            return this.MetadataImporter.ImportType(type, GenericContextFor(context));
        }

        public FieldReference Import(FieldInfo field, IGenericParameterProvider context)
        {
            CheckField(field);
            CheckContext(context, this);
            return this.MetadataImporter.ImportField(field, GenericContextFor(context));
        }

        public MethodReference Import(MethodBase method, IGenericParameterProvider context)
        {
            CheckMethod(method);
            CheckContext(context, this);
            return this.MetadataImporter.ImportMethod(method, GenericContextFor(context), (context != null) ? ImportGenericKind.Open : ImportGenericKind.Definition);
        }

        public TypeReference Import(Type type, IGenericParameterProvider context)
        {
            CheckType(type);
            CheckContext(context, this);
            return this.MetadataImporter.ImportType(type, GenericContextFor(context), (context != null) ? ImportGenericKind.Open : ImportGenericKind.Definition);
        }

        public IMetadataTokenProvider LookupToken(MetadataToken token) => 
            this.Read<MetadataToken, IMetadataTokenProvider>(token, (t, reader) => reader.LookupToken(t));

        public IMetadataTokenProvider LookupToken(int token) => 
            this.LookupToken(new MetadataToken((uint) token));

        private void ProcessDebugHeader()
        {
            if (this.HasDebugHeader)
            {
                byte[] buffer;
                ImageDebugDirectory debugHeader = this.GetDebugHeader(out buffer);
                if (!this.symbol_reader.ProcessDebugHeader(debugHeader, buffer))
                {
                    throw new InvalidOperationException();
                }
            }
        }

        internal TRet Read<TItem, TRet>(TItem item, Func<TItem, MetadataReader, TRet> read)
        {
            lock (this.module_lock)
            {
                int position = this.reader.position;
                IGenericContext context = this.reader.context;
                TRet local = read(item, this.reader);
                this.reader.position = position;
                this.reader.context = context;
                return local;
            }
        }

        internal TRet Read<TItem, TRet>(ref TRet variable, TItem item, Func<TItem, MetadataReader, TRet> read) where TRet: class
        {
            TRet local2;
            lock (this.module_lock)
            {
                if (((TRet) variable) != null)
                {
                    local2 = variable;
                }
                else
                {
                    int position = this.reader.position;
                    IGenericContext context = this.reader.context;
                    TRet local = read(item, this.reader);
                    this.reader.position = position;
                    this.reader.context = context;
                    local2 = variable = local;
                }
            }
            return local2;
        }

        public static ModuleDefinition ReadModule(Stream stream) => 
            ReadModule(stream, new ReaderParameters(Mono.Cecil.ReadingMode.Deferred));

        public static ModuleDefinition ReadModule(string fileName) => 
            ReadModule(fileName, new ReaderParameters(Mono.Cecil.ReadingMode.Deferred));

        public static ModuleDefinition ReadModule(Stream stream, ReaderParameters parameters)
        {
            CheckStream(stream);
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException();
            }
            Mixin.CheckParameters(parameters);
            return ModuleReader.CreateModuleFrom(ImageReader.ReadImageFrom(stream), parameters);
        }

        public static ModuleDefinition ReadModule(string fileName, ReaderParameters parameters)
        {
            using (Stream stream = GetFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadModule(stream, parameters);
            }
        }

        public void ReadSymbols()
        {
            if (string.IsNullOrEmpty(this.fq_name))
            {
                throw new InvalidOperationException();
            }
            ISymbolReaderProvider platformReaderProvider = SymbolProvider.GetPlatformReaderProvider();
            if (platformReaderProvider == null)
            {
                throw new InvalidOperationException();
            }
            this.ReadSymbols(platformReaderProvider.GetSymbolReader(this, this.fq_name));
        }

        public void ReadSymbols(ISymbolReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            this.symbol_reader = reader;
            this.ProcessDebugHeader();
        }

        internal FieldDefinition Resolve(FieldReference field) => 
            this.MetadataResolver.Resolve(field);

        internal MethodDefinition Resolve(MethodReference method) => 
            this.MetadataResolver.Resolve(method);

        internal TypeDefinition Resolve(TypeReference type) => 
            this.MetadataResolver.Resolve(type);

        public bool TryGetTypeReference(string fullName, out TypeReference type) => 
            this.TryGetTypeReference(string.Empty, fullName, out type);

        public bool TryGetTypeReference(string scope, string fullName, out TypeReference type)
        {
            TypeReference reference;
            CheckFullName(fullName);
            if (!this.HasImage)
            {
                type = null;
                return false;
            }
            type = reference = this.GetTypeReference(scope, fullName);
            return !ReferenceEquals(reference, null);
        }

        public void Write(Stream stream)
        {
            this.Write(stream, new WriterParameters());
        }

        public void Write(string fileName)
        {
            this.Write(fileName, new WriterParameters());
        }

        public void Write(Stream stream, WriterParameters parameters)
        {
            CheckStream(stream);
            if (!stream.CanWrite || !stream.CanSeek)
            {
                throw new ArgumentException();
            }
            Mixin.CheckParameters(parameters);
            ModuleWriter.WriteModuleTo(this, stream, parameters);
        }

        public void Write(string fileName, WriterParameters parameters)
        {
            using (Stream stream = GetFileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                this.Write(stream, parameters);
            }
        }

        public bool IsMain =>
            (this.kind != ModuleKind.NetModule);

        public ModuleKind Kind
        {
            get => 
                this.kind;
            set => 
                (this.kind = value);
        }

        public TargetRuntime Runtime
        {
            get => 
                this.runtime;
            set
            {
                this.runtime = value;
                this.runtime_version = this.runtime.RuntimeVersionString();
            }
        }

        public string RuntimeVersion
        {
            get => 
                this.runtime_version;
            set
            {
                this.runtime_version = value;
                this.runtime = this.runtime_version.ParseRuntime();
            }
        }

        public TargetArchitecture Architecture
        {
            get => 
                this.architecture;
            set => 
                (this.architecture = value);
        }

        public ModuleAttributes Attributes
        {
            get => 
                this.attributes;
            set => 
                (this.attributes = value);
        }

        public ModuleCharacteristics Characteristics
        {
            get => 
                this.characteristics;
            set => 
                (this.characteristics = value);
        }

        public string FullyQualifiedName =>
            this.fq_name;

        public Guid Mvid
        {
            get => 
                this.mvid;
            set => 
                (this.mvid = value);
        }

        internal bool HasImage =>
            !ReferenceEquals(this.Image, null);

        public bool HasSymbols =>
            !ReferenceEquals(this.symbol_reader, null);

        public ISymbolReader SymbolReader =>
            this.symbol_reader;

        public override Mono.Cecil.MetadataScopeType MetadataScopeType =>
            Mono.Cecil.MetadataScopeType.ModuleDefinition;

        public AssemblyDefinition Assembly =>
            this.assembly;

        internal Mono.Cecil.MetadataImporter MetadataImporter
        {
            get
            {
                if (this.importer == null)
                {
                    Interlocked.CompareExchange<Mono.Cecil.MetadataImporter>(ref this.importer, new Mono.Cecil.MetadataImporter(this), null);
                }
                return this.importer;
            }
        }

        public IAssemblyResolver AssemblyResolver
        {
            get
            {
                if (this.assembly_resolver == null)
                {
                    Interlocked.CompareExchange<IAssemblyResolver>(ref this.assembly_resolver, new DefaultAssemblyResolver(), null);
                }
                return this.assembly_resolver;
            }
        }

        public IMetadataResolver MetadataResolver
        {
            get
            {
                if (this.metadata_resolver == null)
                {
                    Interlocked.CompareExchange<IMetadataResolver>(ref this.metadata_resolver, new Mono.Cecil.MetadataResolver(this.AssemblyResolver), null);
                }
                return this.metadata_resolver;
            }
        }

        public Mono.Cecil.TypeSystem TypeSystem
        {
            get
            {
                if (this.type_system == null)
                {
                    Interlocked.CompareExchange<Mono.Cecil.TypeSystem>(ref this.type_system, Mono.Cecil.TypeSystem.CreateTypeSystem(this), null);
                }
                return this.type_system;
            }
        }

        public bool HasAssemblyReferences =>
            ((this.references == null) ? (this.HasImage && this.Image.HasTable(Table.AssemblyRef)) : (this.references.Count > 0));

        public Collection<AssemblyNameReference> AssemblyReferences
        {
            get
            {
                if (this.references != null)
                {
                    return this.references;
                }
                if (!this.HasImage)
                {
                    Collection<AssemblyNameReference> collection;
                    this.references = collection = new Collection<AssemblyNameReference>();
                    return collection;
                }
                return this.Read<ModuleDefinition, Collection<AssemblyNameReference>>(ref this.references, this, (_, reader) => reader.ReadAssemblyReferences());
            }
        }

        public bool HasModuleReferences =>
            ((this.modules == null) ? (this.HasImage && this.Image.HasTable(Table.ModuleRef)) : (this.modules.Count > 0));

        public Collection<ModuleReference> ModuleReferences
        {
            get
            {
                if (this.modules != null)
                {
                    return this.modules;
                }
                if (!this.HasImage)
                {
                    Collection<ModuleReference> collection;
                    this.modules = collection = new Collection<ModuleReference>();
                    return collection;
                }
                return this.Read<ModuleDefinition, Collection<ModuleReference>>(ref this.modules, this, (_, reader) => reader.ReadModuleReferences());
            }
        }

        public bool HasResources
        {
            get
            {
                if (this.resources != null)
                {
                    return (this.resources.Count > 0);
                }
                if (!this.HasImage)
                {
                    return false;
                }
                return (this.Image.HasTable(Table.ManifestResource) || this.Read<ModuleDefinition, bool>(this, (_, reader) => reader.HasFileResource()));
            }
        }

        public Collection<Resource> Resources
        {
            get
            {
                if (this.resources != null)
                {
                    return this.resources;
                }
                if (!this.HasImage)
                {
                    Collection<Resource> collection;
                    this.resources = collection = new Collection<Resource>();
                    return collection;
                }
                return this.Read<ModuleDefinition, Collection<Resource>>(ref this.resources, this, (_, reader) => reader.ReadResources());
            }
        }

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this));

        public bool HasTypes =>
            ((this.types == null) ? (this.HasImage && this.Image.HasTable(Table.TypeDef)) : (this.types.Count > 0));

        public Collection<TypeDefinition> Types
        {
            get
            {
                if (this.types != null)
                {
                    return this.types;
                }
                if (!this.HasImage)
                {
                    TypeDefinitionCollection definitions;
                    this.types = definitions = new TypeDefinitionCollection(this);
                    return definitions;
                }
                return this.Read<ModuleDefinition, TypeDefinitionCollection>(ref this.types, this, (_, reader) => reader.ReadTypes());
            }
        }

        public bool HasExportedTypes =>
            ((this.exported_types == null) ? (this.HasImage && this.Image.HasTable(Table.ExportedType)) : (this.exported_types.Count > 0));

        public Collection<ExportedType> ExportedTypes
        {
            get
            {
                if (this.exported_types != null)
                {
                    return this.exported_types;
                }
                if (!this.HasImage)
                {
                    Collection<ExportedType> collection;
                    this.exported_types = collection = new Collection<ExportedType>();
                    return collection;
                }
                return this.Read<ModuleDefinition, Collection<ExportedType>>(ref this.exported_types, this, (_, reader) => reader.ReadExportedTypes());
            }
        }

        public MethodDefinition EntryPoint
        {
            get
            {
                if (this.entry_point != null)
                {
                    return this.entry_point;
                }
                if (!this.HasImage)
                {
                    MethodDefinition definition;
                    this.entry_point = (MethodDefinition) (definition = null);
                    return definition;
                }
                return this.Read<ModuleDefinition, MethodDefinition>(ref this.entry_point, this, (_, reader) => reader.ReadEntryPoint());
            }
            set => 
                (this.entry_point = value);
        }

        internal object SyncRoot =>
            this.module_lock;

        public bool HasDebugHeader =>
            ((this.Image != null) && !this.Image.Debug.IsZero);

        [CompilerGenerated]
        private sealed class <GetTypes>d__14 : IEnumerable<TypeDefinition>, IEnumerable, IEnumerator<TypeDefinition>, IEnumerator, IDisposable
        {
            private TypeDefinition <>2__current;
            private int <>1__state;
            private int <>l__initialThreadId;
            public Collection<TypeDefinition> types;
            public Collection<TypeDefinition> <>3__types;
            public int <i>5__15;
            public TypeDefinition <type>5__16;
            public TypeDefinition <nested>5__17;
            public IEnumerator<TypeDefinition> <>7__wrap18;

            [DebuggerHidden]
            public <GetTypes>d__14(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally19()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap18 != null)
                {
                    this.<>7__wrap18.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<i>5__15 = 0;
                            goto TR_0004;

                        case 1:
                            this.<>1__state = -1;
                            if (!this.<type>5__16.HasNestedTypes)
                            {
                                goto TR_0006;
                            }
                            else
                            {
                                this.<>7__wrap18 = ModuleDefinition.GetTypes(this.<type>5__16.NestedTypes).GetEnumerator();
                                this.<>1__state = 2;
                            }
                            break;

                        case 3:
                            this.<>1__state = 2;
                            break;

                        default:
                            goto TR_0002;
                    }
                    if (this.<>7__wrap18.MoveNext())
                    {
                        this.<nested>5__17 = this.<>7__wrap18.Current;
                        this.<>2__current = this.<nested>5__17;
                        this.<>1__state = 3;
                        flag = true;
                    }
                    else
                    {
                        this.<>m__Finally19();
                        goto TR_0006;
                    }
                    return flag;
                TR_0002:
                    return false;
                TR_0004:
                    if (this.<i>5__15 < this.types.Count)
                    {
                        this.<type>5__16 = this.types[this.<i>5__15];
                        this.<>2__current = this.<type>5__16;
                        this.<>1__state = 1;
                        return true;
                    }
                    goto TR_0002;
                TR_0006:
                    this.<i>5__15++;
                    goto TR_0004;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TypeDefinition> IEnumerable<TypeDefinition>.GetEnumerator()
            {
                ModuleDefinition.<GetTypes>d__14 d__;
                if ((Environment.CurrentManagedThreadId != this.<>l__initialThreadId) || (this.<>1__state != -2))
                {
                    d__ = new ModuleDefinition.<GetTypes>d__14(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.types = this.<>3__types;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<Mono.Cecil.TypeDefinition>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally19();
                        }
                        return;
                }
            }

            TypeDefinition IEnumerator<TypeDefinition>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

