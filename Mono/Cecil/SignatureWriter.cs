namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class SignatureWriter : ByteBuffer
    {
        private readonly MetadataBuilder metadata;

        public SignatureWriter(MetadataBuilder metadata) : base(6)
        {
            this.metadata = metadata;
        }

        private static int GetNamedArgumentCount(ICustomAttribute attribute)
        {
            int num = 0;
            if (attribute.HasFields)
            {
                num += attribute.Fields.Count;
            }
            if (attribute.HasProperties)
            {
                num += attribute.Properties.Count;
            }
            return num;
        }

        private static string GetXmlSecurityDeclaration(SecurityDeclaration declaration)
        {
            if ((declaration.security_attributes == null) || (declaration.security_attributes.Count != 1))
            {
                return null;
            }
            SecurityAttribute attribute = declaration.security_attributes[0];
            if (!attribute.AttributeType.IsTypeOf("System.Security.Permissions", "PermissionSetAttribute"))
            {
                return null;
            }
            if ((attribute.properties == null) || (attribute.properties.Count != 1))
            {
                return null;
            }
            CustomAttributeNamedArgument argument = attribute.properties[0];
            return ((argument.Name == "XML") ? ((string) argument.Argument.Value) : null);
        }

        private uint MakeTypeDefOrRefCodedRID(TypeReference type) => 
            CodedIndex.TypeDefOrRef.CompressMetadataToken(this.metadata.LookupToken(type));

        private bool TryWriteElementType(TypeReference type)
        {
            ElementType etype = type.etype;
            if (etype == ElementType.None)
            {
                return false;
            }
            this.WriteElementType(etype);
            return true;
        }

        private void WriteArrayTypeSignature(ArrayType array)
        {
            this.WriteElementType(ElementType.Array);
            this.WriteTypeSignature(array.ElementType);
            Collection<ArrayDimension> dimensions = array.Dimensions;
            int count = dimensions.Count;
            base.WriteCompressedUInt32((uint) count);
            int num2 = 0;
            int num3 = 0;
            for (int i = 0; i < count; i++)
            {
                ArrayDimension dimension = dimensions[i];
                if (dimension.UpperBound != null)
                {
                    num2++;
                    num3++;
                }
                else if (dimension.LowerBound != null)
                {
                    num3++;
                }
            }
            int[] numArray = new int[num2];
            int[] numArray2 = new int[num3];
            for (int j = 0; j < num3; j++)
            {
                ArrayDimension dimension2 = dimensions[j];
                numArray2[j] = dimension2.LowerBound.GetValueOrDefault();
                if (dimension2.UpperBound != null)
                {
                    numArray[j] = (dimension2.UpperBound.Value - numArray2[j]) + 1;
                }
            }
            base.WriteCompressedUInt32((uint) num2);
            for (int k = 0; k < num2; k++)
            {
                base.WriteCompressedUInt32((uint) numArray[k]);
            }
            base.WriteCompressedUInt32((uint) num3);
            for (int m = 0; m < num3; m++)
            {
                base.WriteCompressedInt32(numArray2[m]);
            }
        }

        public void WriteConstantPrimitive(object value)
        {
            this.WritePrimitiveValue(value);
        }

        public void WriteConstantString(string value)
        {
            base.WriteBytes(Encoding.Unicode.GetBytes(value));
        }

        public void WriteCustomAttributeConstructorArguments(CustomAttribute attribute)
        {
            if (attribute.HasConstructorArguments)
            {
                Collection<CustomAttributeArgument> constructorArguments = attribute.ConstructorArguments;
                Collection<ParameterDefinition> parameters = attribute.Constructor.Parameters;
                if (parameters.Count != constructorArguments.Count)
                {
                    throw new InvalidOperationException();
                }
                for (int i = 0; i < constructorArguments.Count; i++)
                {
                    this.WriteCustomAttributeFixedArgument(parameters[i].ParameterType, constructorArguments[i]);
                }
            }
        }

        private void WriteCustomAttributeElement(TypeReference type, CustomAttributeArgument argument)
        {
            if (type.IsArray)
            {
                this.WriteCustomAttributeFixedArrayArgument((ArrayType) type, argument);
            }
            else if (type.etype != ElementType.Object)
            {
                this.WriteCustomAttributeValue(type, argument.Value);
            }
            else
            {
                argument = (CustomAttributeArgument) argument.Value;
                type = argument.Type;
                this.WriteCustomAttributeFieldOrPropType(type);
                this.WriteCustomAttributeElement(type, argument);
            }
        }

        private void WriteCustomAttributeEnumValue(TypeReference enum_type, object value)
        {
            TypeDefinition self = enum_type.CheckedResolve();
            if (!self.IsEnum)
            {
                throw new ArgumentException();
            }
            this.WriteCustomAttributeValue(self.GetEnumUnderlyingType(), value);
        }

        private void WriteCustomAttributeFieldOrPropType(TypeReference type)
        {
            if (type.IsArray)
            {
                ArrayType type2 = (ArrayType) type;
                this.WriteElementType(ElementType.SzArray);
                this.WriteCustomAttributeFieldOrPropType(type2.ElementType);
            }
            else
            {
                ElementType etype = type.etype;
                ElementType type4 = etype;
                if (type4 != ElementType.None)
                {
                    if (type4 == ElementType.Object)
                    {
                        this.WriteElementType(ElementType.Boxed);
                    }
                    else
                    {
                        this.WriteElementType(etype);
                    }
                }
                else if (type.IsTypeOf("System", "Type"))
                {
                    this.WriteElementType(ElementType.Type);
                }
                else
                {
                    this.WriteElementType(ElementType.Enum);
                    this.WriteTypeReference(type);
                }
            }
        }

        private void WriteCustomAttributeFixedArgument(TypeReference type, CustomAttributeArgument argument)
        {
            if (type.IsArray)
            {
                this.WriteCustomAttributeFixedArrayArgument((ArrayType) type, argument);
            }
            else
            {
                this.WriteCustomAttributeElement(type, argument);
            }
        }

        private void WriteCustomAttributeFixedArrayArgument(ArrayType type, CustomAttributeArgument argument)
        {
            CustomAttributeArgument[] argumentArray = argument.Value as CustomAttributeArgument[];
            if (argumentArray == null)
            {
                base.WriteUInt32(uint.MaxValue);
            }
            else
            {
                base.WriteInt32(argumentArray.Length);
                if (argumentArray.Length != 0)
                {
                    TypeReference elementType = type.ElementType;
                    for (int i = 0; i < argumentArray.Length; i++)
                    {
                        this.WriteCustomAttributeElement(elementType, argumentArray[i]);
                    }
                }
            }
        }

        private void WriteCustomAttributeNamedArgument(byte kind, CustomAttributeNamedArgument named_argument)
        {
            CustomAttributeArgument argument = named_argument.Argument;
            base.WriteByte(kind);
            this.WriteCustomAttributeFieldOrPropType(argument.Type);
            this.WriteUTF8String(named_argument.Name);
            this.WriteCustomAttributeFixedArgument(argument.Type, argument);
        }

        public void WriteCustomAttributeNamedArguments(CustomAttribute attribute)
        {
            int namedArgumentCount = GetNamedArgumentCount(attribute);
            base.WriteUInt16((ushort) namedArgumentCount);
            if (namedArgumentCount != 0)
            {
                this.WriteICustomAttributeNamedArguments(attribute);
            }
        }

        private void WriteCustomAttributeNamedArguments(byte kind, Collection<CustomAttributeNamedArgument> named_arguments)
        {
            for (int i = 0; i < named_arguments.Count; i++)
            {
                this.WriteCustomAttributeNamedArgument(kind, named_arguments[i]);
            }
        }

        private void WriteCustomAttributeValue(TypeReference type, object value)
        {
            ElementType etype = type.etype;
            if (etype == ElementType.None)
            {
                if (type.IsTypeOf("System", "Type"))
                {
                    this.WriteTypeReference((TypeReference) value);
                }
                else
                {
                    this.WriteCustomAttributeEnumValue(type, value);
                }
            }
            else if (etype != ElementType.String)
            {
                this.WritePrimitiveValue(value);
            }
            else
            {
                string str = (string) value;
                if (str == null)
                {
                    base.WriteByte(0xff);
                }
                else
                {
                    this.WriteUTF8String(str);
                }
            }
        }

        public void WriteElementType(ElementType element_type)
        {
            base.WriteByte((byte) element_type);
        }

        public void WriteGenericInstanceSignature(IGenericInstance instance)
        {
            Collection<TypeReference> genericArguments = instance.GenericArguments;
            int count = genericArguments.Count;
            base.WriteCompressedUInt32((uint) count);
            for (int i = 0; i < count; i++)
            {
                this.WriteTypeSignature(genericArguments[i]);
            }
        }

        private void WriteICustomAttributeNamedArguments(ICustomAttribute attribute)
        {
            if (attribute.HasFields)
            {
                this.WriteCustomAttributeNamedArguments(0x53, attribute.Fields);
            }
            if (attribute.HasProperties)
            {
                this.WriteCustomAttributeNamedArguments(0x54, attribute.Properties);
            }
        }

        public void WriteMarshalInfo(MarshalInfo marshal_info)
        {
            this.WriteNativeType(marshal_info.native);
            NativeType native = marshal_info.native;
            if (native == NativeType.FixedSysString)
            {
                FixedSysStringMarshalInfo info4 = (FixedSysStringMarshalInfo) marshal_info;
                if (info4.size > -1)
                {
                    base.WriteCompressedUInt32((uint) info4.size);
                }
            }
            else
            {
                switch (native)
                {
                    case NativeType.SafeArray:
                    {
                        SafeArrayMarshalInfo info2 = (SafeArrayMarshalInfo) marshal_info;
                        if (info2.element_type != VariantType.None)
                        {
                            this.WriteVariantType(info2.element_type);
                        }
                        return;
                    }
                    case NativeType.FixedArray:
                    {
                        FixedArrayMarshalInfo info3 = (FixedArrayMarshalInfo) marshal_info;
                        if (info3.size > -1)
                        {
                            base.WriteCompressedUInt32((uint) info3.size);
                        }
                        if (info3.element_type != NativeType.None)
                        {
                            this.WriteNativeType(info3.element_type);
                        }
                        return;
                    }
                }
                switch (native)
                {
                    case NativeType.Array:
                    {
                        ArrayMarshalInfo info = (ArrayMarshalInfo) marshal_info;
                        if (info.element_type != NativeType.None)
                        {
                            this.WriteNativeType(info.element_type);
                        }
                        if (info.size_parameter_index > -1)
                        {
                            base.WriteCompressedUInt32((uint) info.size_parameter_index);
                        }
                        if (info.size > -1)
                        {
                            base.WriteCompressedUInt32((uint) info.size);
                        }
                        if (info.size_parameter_multiplier > -1)
                        {
                            base.WriteCompressedUInt32((uint) info.size_parameter_multiplier);
                        }
                        return;
                    }
                    case NativeType.LPStruct:
                        break;

                    case NativeType.CustomMarshaler:
                    {
                        CustomMarshalInfo info5 = (CustomMarshalInfo) marshal_info;
                        this.WriteUTF8String((info5.guid != Guid.Empty) ? info5.guid.ToString() : string.Empty);
                        this.WriteUTF8String(info5.unmanaged_type);
                        this.WriteTypeReference(info5.managed_type);
                        this.WriteUTF8String(info5.cookie);
                        break;
                    }
                    default:
                        return;
                }
            }
        }

        public void WriteMethodSignature(IMethodSignature method)
        {
            byte callingConvention = (byte) method.CallingConvention;
            if (method.HasThis)
            {
                callingConvention = (byte) (callingConvention | 0x20);
            }
            if (method.ExplicitThis)
            {
                callingConvention = (byte) (callingConvention | 0x40);
            }
            IGenericParameterProvider provider = method as IGenericParameterProvider;
            int num2 = ((provider == null) || !provider.HasGenericParameters) ? 0 : provider.GenericParameters.Count;
            if (num2 > 0)
            {
                callingConvention = (byte) (callingConvention | 0x10);
            }
            int num3 = method.HasParameters ? method.Parameters.Count : 0;
            base.WriteByte(callingConvention);
            if (num2 > 0)
            {
                base.WriteCompressedUInt32((uint) num2);
            }
            base.WriteCompressedUInt32((uint) num3);
            this.WriteTypeSignature(method.ReturnType);
            if (num3 != 0)
            {
                Collection<ParameterDefinition> parameters = method.Parameters;
                for (int i = 0; i < num3; i++)
                {
                    this.WriteTypeSignature(parameters[i].ParameterType);
                }
            }
        }

        private void WriteModifierSignature(ElementType element_type, IModifierType type)
        {
            this.WriteElementType(element_type);
            base.WriteCompressedUInt32(this.MakeTypeDefOrRefCodedRID(type.ModifierType));
            this.WriteTypeSignature(type.ElementType);
        }

        private void WriteNativeType(NativeType native)
        {
            base.WriteByte((byte) native);
        }

        private void WritePrimitiveValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    this.WriteByte(((bool) value) ? ((byte) 1) : ((byte) 0));
                    return;

                case TypeCode.Char:
                    base.WriteInt16((short) ((char) value));
                    return;

                case TypeCode.SByte:
                    base.WriteSByte((sbyte) value);
                    return;

                case TypeCode.Byte:
                    base.WriteByte((byte) value);
                    return;

                case TypeCode.Int16:
                    base.WriteInt16((short) value);
                    return;

                case TypeCode.UInt16:
                    base.WriteUInt16((ushort) value);
                    return;

                case TypeCode.Int32:
                    base.WriteInt32((int) value);
                    return;

                case TypeCode.UInt32:
                    base.WriteUInt32((uint) value);
                    return;

                case TypeCode.Int64:
                    base.WriteInt64((long) value);
                    return;

                case TypeCode.UInt64:
                    base.WriteUInt64((ulong) value);
                    return;

                case TypeCode.Single:
                    base.WriteSingle((float) value);
                    return;

                case TypeCode.Double:
                    base.WriteDouble((double) value);
                    return;
            }
            throw new NotSupportedException(value.GetType().FullName);
        }

        private void WriteSecurityAttribute(SecurityAttribute attribute)
        {
            this.WriteTypeReference(attribute.AttributeType);
            int namedArgumentCount = GetNamedArgumentCount(attribute);
            if (namedArgumentCount == 0)
            {
                base.WriteCompressedUInt32(1);
                base.WriteCompressedUInt32(0);
            }
            else
            {
                SignatureWriter buffer = new SignatureWriter(this.metadata);
                buffer.WriteCompressedUInt32((uint) namedArgumentCount);
                buffer.WriteICustomAttributeNamedArguments(attribute);
                base.WriteCompressedUInt32((uint) buffer.length);
                base.WriteBytes(buffer);
            }
        }

        public void WriteSecurityDeclaration(SecurityDeclaration declaration)
        {
            base.WriteByte(0x2e);
            Collection<SecurityAttribute> collection = declaration.security_attributes;
            if (collection == null)
            {
                throw new NotSupportedException();
            }
            base.WriteCompressedUInt32((uint) collection.Count);
            for (int i = 0; i < collection.Count; i++)
            {
                this.WriteSecurityAttribute(collection[i]);
            }
        }

        private void WriteTypeReference(TypeReference type)
        {
            this.WriteUTF8String(TypeParser.ToParseable(type));
        }

        public void WriteTypeSignature(TypeReference type)
        {
            GenericParameter parameter;
            TypeSpecification specification;
            if (type == null)
            {
                throw new ArgumentNullException();
            }
            ElementType etype = type.etype;
            ElementType type7 = etype;
            if (type7 > ElementType.GenericInst)
            {
                switch (type7)
                {
                    case ElementType.FnPtr:
                    {
                        FunctionPointerType method = (FunctionPointerType) type;
                        this.WriteElementType(ElementType.FnPtr);
                        this.WriteMethodSignature(method);
                        return;
                    }
                    case ElementType.Object:
                    case ElementType.SzArray:
                        break;

                    case ElementType.MVar:
                        goto TR_0007;

                    case ElementType.CModReqD:
                    case ElementType.CModOpt:
                        this.WriteModifierSignature(etype, (IModifierType) type);
                        return;

                    default:
                        if ((type7 != ElementType.Sentinel) && (type7 != ElementType.Pinned))
                        {
                            break;
                        }
                        goto TR_0004;
                }
                goto TR_0003;
            }
            else
            {
                if (type7 == ElementType.None)
                {
                    this.WriteElementType(type.IsValueType ? ElementType.ValueType : ElementType.Class);
                    base.WriteCompressedUInt32(this.MakeTypeDefOrRefCodedRID(type));
                    return;
                }
                switch (type7)
                {
                    case ElementType.Ptr:
                    case ElementType.ByRef:
                        goto TR_0004;

                    case ElementType.Var:
                        break;

                    case ElementType.Array:
                    {
                        ArrayType array = (ArrayType) type;
                        if (!array.IsVector)
                        {
                            this.WriteArrayTypeSignature(array);
                            return;
                        }
                        this.WriteElementType(ElementType.SzArray);
                        this.WriteTypeSignature(array.ElementType);
                        return;
                    }
                    case ElementType.GenericInst:
                    {
                        GenericInstanceType instance = (GenericInstanceType) type;
                        this.WriteElementType(ElementType.GenericInst);
                        this.WriteElementType(instance.IsValueType ? ElementType.ValueType : ElementType.Class);
                        base.WriteCompressedUInt32(this.MakeTypeDefOrRefCodedRID(instance.ElementType));
                        this.WriteGenericInstanceSignature(instance);
                        return;
                    }
                    default:
                        goto TR_0003;
                }
            }
            goto TR_0007;
        TR_0003:
            if (!this.TryWriteElementType(type))
            {
                throw new NotSupportedException();
            }
            return;
        TR_0004:
            specification = (TypeSpecification) type;
            this.WriteElementType(etype);
            this.WriteTypeSignature(specification.ElementType);
            return;
        TR_0007:
            parameter = (GenericParameter) type;
            this.WriteElementType(etype);
            int position = parameter.Position;
            if (position == -1)
            {
                throw new NotSupportedException();
            }
            base.WriteCompressedUInt32((uint) position);
        }

        public void WriteUTF8String(string @string)
        {
            if (@string == null)
            {
                base.WriteByte(0xff);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(@string);
                base.WriteCompressedUInt32((uint) bytes.Length);
                base.WriteBytes(bytes);
            }
        }

        private void WriteVariantType(VariantType variant)
        {
            base.WriteByte((byte) variant);
        }

        public void WriteXmlSecurityDeclaration(SecurityDeclaration declaration)
        {
            string xmlSecurityDeclaration = GetXmlSecurityDeclaration(declaration);
            if (xmlSecurityDeclaration == null)
            {
                throw new NotSupportedException();
            }
            base.WriteBytes(Encoding.Unicode.GetBytes(xmlSecurityDeclaration));
        }
    }
}

