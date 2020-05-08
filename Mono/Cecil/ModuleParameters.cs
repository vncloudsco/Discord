namespace Mono.Cecil
{
    using System;

    internal sealed class ModuleParameters
    {
        private ModuleKind kind = ModuleKind.Dll;
        private TargetRuntime runtime;
        private TargetArchitecture architecture;
        private IAssemblyResolver assembly_resolver;
        private IMetadataResolver metadata_resolver;

        public ModuleParameters()
        {
            this.Runtime = GetCurrentRuntime();
            this.architecture = TargetArchitecture.I386;
        }

        private static TargetRuntime GetCurrentRuntime() => 
            typeof(object).Assembly.ImageRuntimeVersion.ParseRuntime();

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
            set => 
                (this.runtime = value);
        }

        public TargetArchitecture Architecture
        {
            get => 
                this.architecture;
            set => 
                (this.architecture = value);
        }

        public IAssemblyResolver AssemblyResolver
        {
            get => 
                this.assembly_resolver;
            set => 
                (this.assembly_resolver = value);
        }

        public IMetadataResolver MetadataResolver
        {
            get => 
                this.metadata_resolver;
            set => 
                (this.metadata_resolver = value);
        }
    }
}

