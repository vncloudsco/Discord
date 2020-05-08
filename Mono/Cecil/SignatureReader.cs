namespace Mono.Cecil
{
    using Mono;
    using Mono.Cecil.Metadata;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class SignatureReader : ByteBuffer
    {
        private readonly MetadataReader reader;
        private readonly uint start;
        private readonly uint sig_length;

        public SignatureReader(uint blob, MetadataReader reader) : base(reader.buffer)
        {
            this.reader = reader;
            this.MoveToBlob(blob);
            this.sig_length = base.ReadCompressedUInt32();
            this.start = (uint) base.position;
        }

        public bool CanReadMore() => 
            ((base.position - this.start) < this.sig_length);

        private static void CheckGenericContext(IGenericParameterProvider owner, int index)
        {
            Collection<GenericParameter> genericParameters = owner.GenericParameters;
            for (int i = genericParameters.Count; i <= index; i++)
            {
                genericParameters.Add(new GenericParameter(owner));
            }
        }

        private static Collection<CustomAttributeNamedArgument> GetCustomAttributeNamedArgumentCollection(ref Collection<CustomAttributeNamedArgument> collection)
        {
            Collection<CustomAttributeNamedArgument> collection2;
            if (collection != null)
            {
                return collection;
            }
            collection = collection2 = new Collection<CustomAttributeNamedArgument>();
            return collection2;
        }

        private GenericParameter GetGenericParameter(GenericParameterType type, uint var)
        {
            IGenericParameterProvider method;
            IGenericContext context = this.reader.context;
            int index = (int) var;
            if (context == null)
            {
                return this.GetUnboundGenericParameter(type, index);
            }
            switch (type)
            {
                case GenericParameterType.Type:
                    method = context.Type;
                    break;

                case GenericParameterType.Method:
                    method = context.Method;
                    break;

                default:
                    throw new NotSupportedException();
            }
            if (!context.IsDefinition)
            {
                CheckGenericContext(method, index);
            }
            return ((index < method.GenericParameters.Count) ? method.GenericParameters[index] : this.GetUnboundGenericParameter(type, index));
        }

        private TypeReference GetPrimitiveType(ElementType etype)
        {
            switch (etype)
            {
                case ElementType.Boolean:
                    return this.TypeSystem.Boolean;

                case ElementType.Char:
                    return this.TypeSystem.Char;

                case ElementType.I1:
                    return this.TypeSystem.SByte;

                case ElementType.U1:
                    return this.TypeSystem.Byte;

                case ElementType.I2:
                    return this.TypeSystem.Int16;

                case ElementType.U2:
                    return this.TypeSystem.UInt16;

                case ElementType.I4:
                    return this.TypeSystem.Int32;

                case ElementType.U4:
                    return this.TypeSystem.UInt32;

                case ElementType.I8:
                    return this.TypeSystem.Int64;

                case ElementType.U8:
                    return this.TypeSystem.UInt64;

                case ElementType.R4:
                    return this.TypeSystem.Single;

                case ElementType.R8:
                    return this.TypeSystem.Double;

                case ElementType.String:
                    return this.TypeSystem.String;
            }
            throw new NotImplementedException(etype.ToString());
        }

        private TypeReference GetTypeDefOrRef(MetadataToken token) => 
            this.reader.GetTypeDefOrRef(token);

        private GenericParameter GetUnboundGenericParameter(GenericParameterType type, int index) => 
            new GenericParameter(index, type, this.reader.module);

        private void MoveToBlob(uint blob)
        {
            base.position = (int) (this.reader.image.BlobHeap.Offset + blob);
        }

        private ArrayType ReadArrayTypeSignature()
        {
            ArrayType type = new ArrayType(this.ReadTypeSignature());
            uint num = base.ReadCompressedUInt32();
            uint[] numArray = new uint[base.ReadCompressedUInt32()];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = base.ReadCompressedUInt32();
            }
            int[] numArray2 = new int[base.ReadCompressedUInt32()];
            for (int j = 0; j < numArray2.Length; j++)
            {
                numArray2[j] = base.ReadCompressedInt32();
            }
            type.Dimensions.Clear();
            for (int k = 0; k < num; k++)
            {
                int? lowerBound = null;
                int? upperBound = null;
                if (k < numArray2.Length)
                {
                    lowerBound = new int?(numArray2[k]);
                }
                if (k < numArray.Length)
                {
                    int? nullable1;
                    int? nullable7;
                    int? nullable3 = lowerBound;
                    int num5 = (int) numArray[k];
                    if (nullable3 != null)
                    {
                        nullable1 = new int?(nullable3.GetValueOrDefault() + num5);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    int? nullable5 = nullable1;
                    if (nullable5 != null)
                    {
                        nullable7 = new int?(nullable5.GetValueOrDefault() - 1);
                    }
                    else
                    {
                        nullable7 = null;
                    }
                    upperBound = nullable7;
                }
                type.Dimensions.Add(new ArrayDimension(lowerBound, upperBound));
            }
            return type;
        }

        public object ReadConstantSignature(ElementType type) => 
            this.ReadPrimitiveValue(type);

        public void ReadCustomAttributeConstructorArguments(CustomAttribute attribute, Collection<ParameterDefinition> parameters)
        {
            int count = parameters.Count;
            if (count != 0)
            {
                attribute.arguments = new Collection<CustomAttributeArgument>(count);
                for (int i = 0; i < count; i++)
                {
                    attribute.arguments.Add(this.ReadCustomAttributeFixedArgument(parameters[i].ParameterType));
                }
            }
        }

        private CustomAttributeArgument ReadCustomAttributeElement(TypeReference type) => 
            (!type.IsArray ? new CustomAttributeArgument(type, (type.etype == ElementType.Object) ? this.ReadCustomAttributeElement(this.ReadCustomAttributeFieldOrPropType()) : this.ReadCustomAttributeElementValue(type)) : this.ReadCustomAttributeFixedArrayArgument((ArrayType) type));

        private object ReadCustomAttributeElementValue(TypeReference type)
        {
            ElementType etype = type.etype;
            ElementType type3 = etype;
            return ((type3 == ElementType.None) ? (!type.IsTypeOf("System", "Type") ? this.ReadCustomAttributeEnum(type) : this.ReadTypeReference()) : ((type3 != ElementType.String) ? this.ReadPrimitiveValue(etype) : this.ReadUTF8String()));
        }

        private object ReadCustomAttributeEnum(TypeReference enum_type)
        {
            TypeDefinition self = enum_type.CheckedResolve();
            if (!self.IsEnum)
            {
                throw new ArgumentException();
            }
            return this.ReadCustomAttributeElementValue(self.GetEnumUnderlyingType());
        }

        private TypeReference ReadCustomAttributeFieldOrPropType()
        {
            ElementType etype = (ElementType) base.ReadByte();
            ElementType type2 = etype;
            if (type2 == ElementType.SzArray)
            {
                return new ArrayType(this.ReadCustomAttributeFieldOrPropType());
            }
            switch (type2)
            {
                case ElementType.Type:
                    return this.TypeSystem.LookupType("System", "Type");

                case ElementType.Boxed:
                    return this.TypeSystem.Object;
            }
            return ((type2 == ElementType.Enum) ? this.ReadTypeReference() : this.GetPrimitiveType(etype));
        }

        private CustomAttributeArgument ReadCustomAttributeFixedArgument(TypeReference type) => 
            (!type.IsArray ? this.ReadCustomAttributeElement(type) : this.ReadCustomAttributeFixedArrayArgument((ArrayType) type));

        private CustomAttributeArgument ReadCustomAttributeFixedArrayArgument(ArrayType type)
        {
            uint num = base.ReadUInt32();
            if (num == uint.MaxValue)
            {
                return new CustomAttributeArgument(type, null);
            }
            if (num == 0)
            {
                return new CustomAttributeArgument(type, Empty<CustomAttributeArgument>.Array);
            }
            CustomAttributeArgument[] argumentArray = new CustomAttributeArgument[num];
            TypeReference elementType = type.ElementType;
            for (int i = 0; i < num; i++)
            {
                argumentArray[i] = this.ReadCustomAttributeElement(elementType);
            }
            return new CustomAttributeArgument(type, argumentArray);
        }

        private void ReadCustomAttributeNamedArgument(ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
        {
            Collection<CustomAttributeNamedArgument> customAttributeNamedArgumentCollection;
            TypeReference type = this.ReadCustomAttributeFieldOrPropType();
            string name = this.ReadUTF8String();
            switch (base.ReadByte())
            {
                case 0x53:
                    customAttributeNamedArgumentCollection = GetCustomAttributeNamedArgumentCollection(ref fields);
                    break;

                case 0x54:
                    customAttributeNamedArgumentCollection = GetCustomAttributeNamedArgumentCollection(ref properties);
                    break;

                default:
                    throw new NotSupportedException();
            }
            customAttributeNamedArgumentCollection.Add(new CustomAttributeNamedArgument(name, this.ReadCustomAttributeFixedArgument(type)));
        }

        public void ReadCustomAttributeNamedArguments(ushort count, ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
        {
            for (int i = 0; i < count; i++)
            {
                this.ReadCustomAttributeNamedArgument(ref fields, ref properties);
            }
        }

        public void ReadGenericInstanceSignature(IGenericParameterProvider provider, IGenericInstance instance)
        {
            uint num = base.ReadCompressedUInt32();
            if (!provider.IsDefinition)
            {
                CheckGenericContext(provider, ((int) num) - 1);
            }
            Collection<TypeReference> genericArguments = instance.GenericArguments;
            for (int i = 0; i < num; i++)
            {
                genericArguments.Add(this.ReadTypeSignature());
            }
        }

        public MarshalInfo ReadMarshalInfo()
        {
            NativeType native = this.ReadNativeType();
            NativeType type2 = native;
            if (type2 == NativeType.FixedSysString)
            {
                FixedSysStringMarshalInfo info4 = new FixedSysStringMarshalInfo();
                if (this.CanReadMore())
                {
                    info4.size = (int) base.ReadCompressedUInt32();
                }
                return info4;
            }
            switch (type2)
            {
                case NativeType.SafeArray:
                {
                    SafeArrayMarshalInfo info2 = new SafeArrayMarshalInfo();
                    if (this.CanReadMore())
                    {
                        info2.element_type = this.ReadVariantType();
                    }
                    return info2;
                }
                case NativeType.FixedArray:
                {
                    FixedArrayMarshalInfo info3 = new FixedArrayMarshalInfo();
                    if (this.CanReadMore())
                    {
                        info3.size = (int) base.ReadCompressedUInt32();
                    }
                    if (this.CanReadMore())
                    {
                        info3.element_type = this.ReadNativeType();
                    }
                    return info3;
                }
            }
            switch (type2)
            {
                case NativeType.Array:
                {
                    ArrayMarshalInfo info = new ArrayMarshalInfo();
                    if (this.CanReadMore())
                    {
                        info.element_type = this.ReadNativeType();
                    }
                    if (this.CanReadMore())
                    {
                        info.size_parameter_index = (int) base.ReadCompressedUInt32();
                    }
                    if (this.CanReadMore())
                    {
                        info.size = (int) base.ReadCompressedUInt32();
                    }
                    if (this.CanReadMore())
                    {
                        info.size_parameter_multiplier = (int) base.ReadCompressedUInt32();
                    }
                    return info;
                }
                case NativeType.CustomMarshaler:
                {
                    CustomMarshalInfo info5 = new CustomMarshalInfo();
                    string str = this.ReadUTF8String();
                    info5.guid = !string.IsNullOrEmpty(str) ? new Guid(str) : Guid.Empty;
                    info5.unmanaged_type = this.ReadUTF8String();
                    info5.managed_type = this.ReadTypeReference();
                    info5.cookie = this.ReadUTF8String();
                    return info5;
                }
            }
            return new MarshalInfo(native);
        }

        public void ReadMethodSignature(IMethodSignature method)
        {
            byte num = base.ReadByte();
            if ((num & 0x20) != 0)
            {
                method.HasThis = true;
                num = (byte) (num & -33);
            }
            if ((num & 0x40) != 0)
            {
                method.ExplicitThis = true;
                num = (byte) (num & -65);
            }
            method.CallingConvention = (MethodCallingConvention) num;
            MethodReference owner = method as MethodReference;
            if ((owner != null) && !owner.DeclaringType.IsArray)
            {
                this.reader.context = owner;
            }
            if ((num & 0x10) != 0)
            {
                uint num2 = base.ReadCompressedUInt32();
                if ((owner != null) && !owner.IsDefinition)
                {
                    CheckGenericContext(owner, ((int) num2) - 1);
                }
            }
            uint num3 = base.ReadCompressedUInt32();
            method.MethodReturnType.ReturnType = this.ReadTypeSignature();
            if (num3 != 0)
            {
                Collection<ParameterDefinition> parameters;
                MethodReference reference2 = method as MethodReference;
                if (reference2 == null)
                {
                    parameters = method.Parameters;
                }
                else
                {
                    parameters = reference2.parameters = new ParameterDefinitionCollection(method, (int) num3);
                }
                for (int i = 0; i < num3; i++)
                {
                    parameters.Add(new ParameterDefinition(this.ReadTypeSignature()));
                }
            }
        }

        private NativeType ReadNativeType() => 
            ((NativeType) base.ReadByte());

        private object ReadPrimitiveValue(ElementType type)
        {
            switch (type)
            {
                case ElementType.Boolean:
                    return (base.ReadByte() == 1);

                case ElementType.Char:
                    return (char) base.ReadUInt16();

                case ElementType.I1:
                    return (sbyte) base.ReadByte();

                case ElementType.U1:
                    return base.ReadByte();

                case ElementType.I2:
                    return base.ReadInt16();

                case ElementType.U2:
                    return base.ReadUInt16();

                case ElementType.I4:
                    return base.ReadInt32();

                case ElementType.U4:
                    return base.ReadUInt32();

                case ElementType.I8:
                    return base.ReadInt64();

                case ElementType.U8:
                    return base.ReadUInt64();

                case ElementType.R4:
                    return base.ReadSingle();

                case ElementType.R8:
                    return base.ReadDouble();
            }
            throw new NotImplementedException(type.ToString());
        }

        public SecurityAttribute ReadSecurityAttribute()
        {
            SecurityAttribute attribute = new SecurityAttribute(this.ReadTypeReference());
            base.ReadCompressedUInt32();
            this.ReadCustomAttributeNamedArguments((ushort) base.ReadCompressedUInt32(), ref attribute.fields, ref attribute.properties);
            return attribute;
        }

        public TypeReference ReadTypeReference() => 
            TypeParser.ParseType(this.reader.module, this.ReadUTF8String());

        public TypeReference ReadTypeSignature() => 
            this.ReadTypeSignature((ElementType) base.ReadByte());

        private TypeReference ReadTypeSignature(ElementType etype)
        {
            ElementType type3 = etype;
            switch (type3)
            {
                case ElementType.Void:
                    return this.TypeSystem.Void;

                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                case ElementType.String:
                case (ElementType.Array | ElementType.Boolean | ElementType.Void):
                case (ElementType.Boolean | ElementType.ByRef | ElementType.I4):
                    break;

                case ElementType.Ptr:
                    return new PointerType(this.ReadTypeSignature());

                case ElementType.ByRef:
                    return new ByReferenceType(this.ReadTypeSignature());

                case ElementType.ValueType:
                {
                    TypeReference typeDefOrRef = this.GetTypeDefOrRef(this.ReadTypeTokenSignature());
                    typeDefOrRef.IsValueType = true;
                    return typeDefOrRef;
                }
                case ElementType.Class:
                    return this.GetTypeDefOrRef(this.ReadTypeTokenSignature());

                case ElementType.Var:
                    return this.GetGenericParameter(GenericParameterType.Type, base.ReadCompressedUInt32());

                case ElementType.Array:
                    return this.ReadArrayTypeSignature();

                case ElementType.GenericInst:
                {
                    bool flag = base.ReadByte() == 0x11;
                    TypeReference typeDefOrRef = this.GetTypeDefOrRef(this.ReadTypeTokenSignature());
                    GenericInstanceType instance = new GenericInstanceType(typeDefOrRef);
                    this.ReadGenericInstanceSignature(typeDefOrRef, instance);
                    if (flag)
                    {
                        instance.IsValueType = true;
                        typeDefOrRef.GetElementType().IsValueType = true;
                    }
                    return instance;
                }
                case ElementType.TypedByRef:
                    return this.TypeSystem.TypedReference;

                case ElementType.I:
                    return this.TypeSystem.IntPtr;

                case ElementType.U:
                    return this.TypeSystem.UIntPtr;

                case ElementType.FnPtr:
                {
                    FunctionPointerType method = new FunctionPointerType();
                    this.ReadMethodSignature(method);
                    return method;
                }
                case ElementType.Object:
                    return this.TypeSystem.Object;

                case ElementType.SzArray:
                    return new ArrayType(this.ReadTypeSignature());

                case ElementType.MVar:
                    return this.GetGenericParameter(GenericParameterType.Method, base.ReadCompressedUInt32());

                case ElementType.CModReqD:
                    return new RequiredModifierType(this.GetTypeDefOrRef(this.ReadTypeTokenSignature()), this.ReadTypeSignature());

                case ElementType.CModOpt:
                    return new OptionalModifierType(this.GetTypeDefOrRef(this.ReadTypeTokenSignature()), this.ReadTypeSignature());

                default:
                    if (type3 == ElementType.Sentinel)
                    {
                        return new SentinelType(this.ReadTypeSignature());
                    }
                    if (type3 != ElementType.Pinned)
                    {
                        break;
                    }
                    return new PinnedType(this.ReadTypeSignature());
            }
            return this.GetPrimitiveType(etype);
        }

        private MetadataToken ReadTypeTokenSignature() => 
            CodedIndex.TypeDefOrRef.GetMetadataToken(base.ReadCompressedUInt32());

        private string ReadUTF8String()
        {
            if (base.buffer[base.position] == 0xff)
            {
                base.position++;
                return null;
            }
            int num = (int) base.ReadCompressedUInt32();
            if (num == 0)
            {
                return string.Empty;
            }
            string str = Encoding.UTF8.GetString(base.buffer, base.position, (base.buffer[(base.position + num) - 1] == 0) ? (num - 1) : num);
            base.position += num;
            return str;
        }

        private VariantType ReadVariantType() => 
            ((VariantType) base.ReadByte());

        private Mono.Cecil.TypeSystem TypeSystem =>
            this.reader.module.TypeSystem;
    }
}

