namespace Mono.Cecil
{
    using Mono.Cecil.PE;
    using System;

    internal sealed class DeferredModuleReader : ModuleReader
    {
        public DeferredModuleReader(Image image) : base(image, ReadingMode.Deferred)
        {
        }

        protected override void ReadModule()
        {
            base.module.Read<ModuleDefinition, ModuleDefinition>(base.module, delegate (ModuleDefinition module, MetadataReader reader) {
                base.ReadModuleManifest(reader);
                return module;
            });
        }
    }
}

