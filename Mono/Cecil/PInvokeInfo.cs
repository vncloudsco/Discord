namespace Mono.Cecil
{
    using System;

    internal sealed class PInvokeInfo
    {
        private ushort attributes;
        private string entry_point;
        private ModuleReference module;

        public PInvokeInfo(PInvokeAttributes attributes, string entryPoint, ModuleReference module)
        {
            this.attributes = (ushort) attributes;
            this.entry_point = entryPoint;
            this.module = module;
        }

        public PInvokeAttributes Attributes
        {
            get => 
                ((PInvokeAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public string EntryPoint
        {
            get => 
                this.entry_point;
            set => 
                (this.entry_point = value);
        }

        public ModuleReference Module
        {
            get => 
                this.module;
            set => 
                (this.module = value);
        }

        public bool IsNoMangle
        {
            get => 
                this.attributes.GetAttributes(1);
            set => 
                (this.attributes = this.attributes.SetAttributes(1, value));
        }

        public bool IsCharSetNotSpec
        {
            get => 
                this.attributes.GetMaskedAttributes(6, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(6, 0, value));
        }

        public bool IsCharSetAnsi
        {
            get => 
                this.attributes.GetMaskedAttributes(6, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(6, 2, value));
        }

        public bool IsCharSetUnicode
        {
            get => 
                this.attributes.GetMaskedAttributes(6, 4);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(6, 4, value));
        }

        public bool IsCharSetAuto
        {
            get => 
                this.attributes.GetMaskedAttributes(6, 6);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(6, 6, value));
        }

        public bool SupportsLastError
        {
            get => 
                this.attributes.GetAttributes(0x40);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x40, value));
        }

        public bool IsCallConvWinapi
        {
            get => 
                this.attributes.GetMaskedAttributes(0x700, 0x100);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x700, 0x100, value));
        }

        public bool IsCallConvCdecl
        {
            get => 
                this.attributes.GetMaskedAttributes(0x700, 0x200);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x700, 0x200, value));
        }

        public bool IsCallConvStdCall
        {
            get => 
                this.attributes.GetMaskedAttributes(0x700, 0x300);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x700, 0x300, value));
        }

        public bool IsCallConvThiscall
        {
            get => 
                this.attributes.GetMaskedAttributes(0x700, 0x400);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x700, 0x400, value));
        }

        public bool IsCallConvFastcall
        {
            get => 
                this.attributes.GetMaskedAttributes(0x700, 0x500);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x700, 0x500, value));
        }

        public bool IsBestFitEnabled
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30, 0x10);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30, 0x10, value));
        }

        public bool IsBestFitDisabled
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30, 0x20);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30, 0x20, value));
        }

        public bool IsThrowOnUnmappableCharEnabled
        {
            get => 
                this.attributes.GetMaskedAttributes(0x3000, 0x1000);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x3000, 0x1000, value));
        }

        public bool IsThrowOnUnmappableCharDisabled
        {
            get => 
                this.attributes.GetMaskedAttributes(0x3000, 0x2000);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x3000, 0x2000, value));
        }
    }
}

