namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using Mono.Security.Cryptography;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;

    internal static class Mixin
    {
        public const int NotResolvedMarker = -2;
        public const int NoDataMarker = -1;
        internal static object NoValue = new object();
        internal static object NotResolved = new object();

        public static TypeDefinition CheckedResolve(this TypeReference self)
        {
            TypeDefinition definition = self.Resolve();
            if (definition == null)
            {
                throw new ResolutionException(self);
            }
            return definition;
        }

        public static void CheckModifier(TypeReference modifierType, TypeReference type)
        {
            if (modifierType == null)
            {
                throw new ArgumentNullException("modifierType");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
        }

        public static void CheckName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("Empty name");
            }
        }

        public static void CheckParameters(object parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
        }

        public static void CheckType(TypeReference type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
        }

        public static uint CompressMetadataToken(this CodedIndex self, MetadataToken token)
        {
            uint num = 0;
            if (token.RID == 0)
            {
                return num;
            }
            switch (self)
            {
                case CodedIndex.TypeDefOrRef:
                {
                    num = token.RID << 2;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.TypeRef)
                    {
                        return (num | 1);
                    }
                    if (tokenType == TokenType.TypeDef)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.TypeSpec)
                    {
                        break;
                    }
                    return (num | 2);
                }
                case CodedIndex.HasConstant:
                {
                    num = token.RID << 2;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.Field)
                    {
                        return num;
                    }
                    if (tokenType == TokenType.Param)
                    {
                        return (num | 1);
                    }
                    if (tokenType != TokenType.Property)
                    {
                        break;
                    }
                    return (num | 2);
                }
                case CodedIndex.HasCustomAttribute:
                {
                    num = token.RID << 5;
                    TokenType tokenType = token.TokenType;
                    if (tokenType <= TokenType.Signature)
                    {
                        if (tokenType <= TokenType.Method)
                        {
                            if (tokenType <= TokenType.TypeRef)
                            {
                                if (tokenType == TokenType.Module)
                                {
                                    return (num | 7);
                                }
                                if (tokenType == TokenType.TypeRef)
                                {
                                    return (num | 2);
                                }
                            }
                            else
                            {
                                if (tokenType == TokenType.TypeDef)
                                {
                                    return (num | 3);
                                }
                                if (tokenType == TokenType.Field)
                                {
                                    return (num | 1);
                                }
                                if (tokenType == TokenType.Method)
                                {
                                    return num;
                                }
                            }
                        }
                        else if (tokenType <= TokenType.InterfaceImpl)
                        {
                            if (tokenType == TokenType.Param)
                            {
                                return (num | 4);
                            }
                            if (tokenType == TokenType.InterfaceImpl)
                            {
                                return (num | 5);
                            }
                        }
                        else
                        {
                            if (tokenType == TokenType.MemberRef)
                            {
                                return (num | 6);
                            }
                            if (tokenType == TokenType.Permission)
                            {
                                return (num | 8);
                            }
                            if (tokenType == TokenType.Signature)
                            {
                                return (num | ((uint) 11));
                            }
                        }
                    }
                    else if (tokenType <= TokenType.Assembly)
                    {
                        if (tokenType <= TokenType.Property)
                        {
                            if (tokenType == TokenType.Event)
                            {
                                return (num | ((uint) 10));
                            }
                            if (tokenType == TokenType.Property)
                            {
                                return (num | ((uint) 9));
                            }
                        }
                        else
                        {
                            if (tokenType == TokenType.ModuleRef)
                            {
                                return (num | ((uint) 12));
                            }
                            if (tokenType == TokenType.TypeSpec)
                            {
                                return (num | ((uint) 13));
                            }
                            if (tokenType == TokenType.Assembly)
                            {
                                return (num | ((uint) 14));
                            }
                        }
                    }
                    else if (tokenType <= TokenType.File)
                    {
                        if (tokenType == TokenType.AssemblyRef)
                        {
                            return (num | ((uint) 15));
                        }
                        if (tokenType == TokenType.File)
                        {
                            return (num | ((uint) 0x10));
                        }
                    }
                    else
                    {
                        if (tokenType == TokenType.ExportedType)
                        {
                            return (num | ((uint) 0x11));
                        }
                        if (tokenType == TokenType.ManifestResource)
                        {
                            return (num | ((uint) 0x12));
                        }
                        if (tokenType == TokenType.GenericParam)
                        {
                            return (num | ((uint) 0x13));
                        }
                    }
                    break;
                }
                case CodedIndex.HasFieldMarshal:
                {
                    num = token.RID << 1;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.Field)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.Param)
                    {
                        break;
                    }
                    return (num | 1);
                }
                case CodedIndex.HasDeclSecurity:
                {
                    num = token.RID << 2;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.TypeDef)
                    {
                        return num;
                    }
                    if (tokenType == TokenType.Method)
                    {
                        return (num | 1);
                    }
                    if (tokenType != TokenType.Assembly)
                    {
                        break;
                    }
                    return (num | 2);
                }
                case CodedIndex.MemberRefParent:
                {
                    num = token.RID << 3;
                    TokenType tokenType = token.TokenType;
                    if (tokenType <= TokenType.TypeDef)
                    {
                        if (tokenType == TokenType.TypeRef)
                        {
                            return (num | 1);
                        }
                        if (tokenType == TokenType.TypeDef)
                        {
                            return num;
                        }
                    }
                    else
                    {
                        if (tokenType == TokenType.Method)
                        {
                            return (num | 3);
                        }
                        if (tokenType == TokenType.ModuleRef)
                        {
                            return (num | 2);
                        }
                        if (tokenType == TokenType.TypeSpec)
                        {
                            return (num | 4);
                        }
                    }
                    break;
                }
                case CodedIndex.HasSemantics:
                {
                    num = token.RID << 1;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.Event)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.Property)
                    {
                        break;
                    }
                    return (num | 1);
                }
                case CodedIndex.MethodDefOrRef:
                {
                    num = token.RID << 1;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.Method)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.MemberRef)
                    {
                        break;
                    }
                    return (num | 1);
                }
                case CodedIndex.MemberForwarded:
                {
                    num = token.RID << 1;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.Field)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.Method)
                    {
                        break;
                    }
                    return (num | 1);
                }
                case CodedIndex.Implementation:
                {
                    num = token.RID << 2;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.AssemblyRef)
                    {
                        return (num | 1);
                    }
                    if (tokenType == TokenType.File)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.ExportedType)
                    {
                        break;
                    }
                    return (num | 2);
                }
                case CodedIndex.CustomAttributeType:
                {
                    num = token.RID << 3;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.Method)
                    {
                        return (num | 2);
                    }
                    if (tokenType != TokenType.MemberRef)
                    {
                        break;
                    }
                    return (num | 3);
                }
                case CodedIndex.ResolutionScope:
                {
                    num = token.RID << 2;
                    TokenType tokenType = token.TokenType;
                    if (tokenType <= TokenType.TypeRef)
                    {
                        if (tokenType == TokenType.Module)
                        {
                            return num;
                        }
                        if (tokenType == TokenType.TypeRef)
                        {
                            return (num | 3);
                        }
                    }
                    else
                    {
                        if (tokenType == TokenType.ModuleRef)
                        {
                            return (num | 1);
                        }
                        if (tokenType == TokenType.AssemblyRef)
                        {
                            return (num | 2);
                        }
                    }
                    break;
                }
                case CodedIndex.TypeOrMethodDef:
                {
                    num = token.RID << 1;
                    TokenType tokenType = token.TokenType;
                    if (tokenType == TokenType.TypeDef)
                    {
                        return num;
                    }
                    if (tokenType != TokenType.Method)
                    {
                        break;
                    }
                    return (num | 1);
                }
                default:
                    break;
            }
            throw new ArgumentException();
        }

        public static bool ContainsGenericParameter(this IGenericInstance self)
        {
            Collection<TypeReference> genericArguments = self.GenericArguments;
            for (int i = 0; i < genericArguments.Count; i++)
            {
                if (genericArguments[i].ContainsGenericParameter)
                {
                    return true;
                }
            }
            return false;
        }

        public static RSA CreateRSA(this StrongNameKeyPair key_pair)
        {
            byte[] buffer;
            string str;
            if (!TryGetKeyContainer(key_pair, out buffer, out str))
            {
                return CryptoConvert.FromCapiKeyBlob(buffer);
            }
            return new RSACryptoServiceProvider(new CspParameters { 
                Flags = CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = str,
                KeyNumber = 2
            });
        }

        public static void GenericInstanceFullName(this IGenericInstance self, StringBuilder builder)
        {
            builder.Append("<");
            Collection<TypeReference> genericArguments = self.GenericArguments;
            for (int i = 0; i < genericArguments.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.Append(genericArguments[i].FullName);
            }
            builder.Append(">");
        }

        public static bool GetAttributes(this ushort self, ushort attributes) => 
            ((self & attributes) != 0);

        public static bool GetAttributes(this uint self, uint attributes) => 
            ((self & attributes) != 0);

        public static Collection<CustomAttribute> GetCustomAttributes(this ICustomAttributeProvider self, ref Collection<CustomAttribute> variable, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                Collection<CustomAttribute> collection;
                variable = collection = new Collection<CustomAttribute>();
                return collection;
            }
            return module.Read<ICustomAttributeProvider, Collection<CustomAttribute>>(ref variable, self, (provider, reader) => reader.ReadCustomAttributes(provider));
        }

        public static TypeReference GetEnumUnderlyingType(this TypeDefinition self)
        {
            Collection<FieldDefinition> fields = self.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                FieldDefinition definition = fields[i];
                if (!definition.IsStatic)
                {
                    return definition.FieldType;
                }
            }
            throw new ArgumentException();
        }

        public static string GetFullyQualifiedName(this Stream self)
        {
            FileStream stream = self as FileStream;
            return ((stream != null) ? Path.GetFullPath(stream.Name) : string.Empty);
        }

        public static Collection<GenericParameter> GetGenericParameters(this IGenericParameterProvider self, ref Collection<GenericParameter> collection, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                Collection<GenericParameter> collection2;
                collection = collection2 = new GenericParameterCollection(self);
                return collection2;
            }
            return module.Read<IGenericParameterProvider, Collection<GenericParameter>>(ref collection, self, (provider, reader) => reader.ReadGenericParameters(provider));
        }

        public static bool GetHasCustomAttributes(this ICustomAttributeProvider self, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                return false;
            }
            return module.Read<ICustomAttributeProvider, bool>(self, (provider, reader) => reader.HasCustomAttributes(provider));
        }

        public static bool GetHasGenericParameters(this IGenericParameterProvider self, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                return false;
            }
            return module.Read<IGenericParameterProvider, bool>(self, (provider, reader) => reader.HasGenericParameters(provider));
        }

        public static bool GetHasMarshalInfo(this IMarshalInfoProvider self, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                return false;
            }
            return module.Read<IMarshalInfoProvider, bool>(self, (provider, reader) => reader.HasMarshalInfo(provider));
        }

        public static bool GetHasSecurityDeclarations(this ISecurityDeclarationProvider self, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                return false;
            }
            return module.Read<ISecurityDeclarationProvider, bool>(self, (provider, reader) => reader.HasSecurityDeclarations(provider));
        }

        public static MarshalInfo GetMarshalInfo(this IMarshalInfoProvider self, ref MarshalInfo variable, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                return null;
            }
            return module.Read<IMarshalInfoProvider, MarshalInfo>(ref variable, self, (provider, reader) => reader.ReadMarshalInfo(provider));
        }

        public static bool GetMaskedAttributes(this ushort self, ushort mask, uint attributes) => 
            ((self & mask) == attributes);

        public static bool GetMaskedAttributes(this uint self, uint mask, uint attributes) => 
            ((self & mask) == attributes);

        public static MetadataToken GetMetadataToken(this CodedIndex self, uint data)
        {
            uint num;
            TokenType typeDef;
            switch (self)
            {
                case CodedIndex.TypeDefOrRef:
                    num = data >> 2;
                    switch ((data & 3))
                    {
                        case 0:
                            typeDef = TokenType.TypeDef;
                            break;

                        case 1:
                            typeDef = TokenType.TypeRef;
                            break;

                        case 2:
                            typeDef = TokenType.TypeSpec;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.HasConstant:
                    num = data >> 2;
                    switch ((data & 3))
                    {
                        case 0:
                            typeDef = TokenType.Field;
                            break;

                        case 1:
                            typeDef = TokenType.Param;
                            break;

                        case 2:
                            typeDef = TokenType.Property;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.HasCustomAttribute:
                    num = data >> 5;
                    switch ((data & 0x1f))
                    {
                        case 0:
                            typeDef = TokenType.Method;
                            break;

                        case 1:
                            typeDef = TokenType.Field;
                            break;

                        case 2:
                            typeDef = TokenType.TypeRef;
                            break;

                        case 3:
                            typeDef = TokenType.TypeDef;
                            break;

                        case 4:
                            typeDef = TokenType.Param;
                            break;

                        case 5:
                            typeDef = TokenType.InterfaceImpl;
                            break;

                        case 6:
                            typeDef = TokenType.MemberRef;
                            break;

                        case 7:
                            typeDef = TokenType.Module;
                            break;

                        case 8:
                            typeDef = TokenType.Permission;
                            break;

                        case 9:
                            typeDef = TokenType.Property;
                            break;

                        case 10:
                            typeDef = TokenType.Event;
                            break;

                        case 11:
                            typeDef = TokenType.Signature;
                            break;

                        case 12:
                            typeDef = TokenType.ModuleRef;
                            break;

                        case 13:
                            typeDef = TokenType.TypeSpec;
                            break;

                        case 14:
                            typeDef = TokenType.Assembly;
                            break;

                        case 15:
                            typeDef = TokenType.AssemblyRef;
                            break;

                        case 0x10:
                            typeDef = TokenType.File;
                            break;

                        case 0x11:
                            typeDef = TokenType.ExportedType;
                            break;

                        case 0x12:
                            typeDef = TokenType.ManifestResource;
                            break;

                        case 0x13:
                            typeDef = TokenType.GenericParam;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.HasFieldMarshal:
                    num = data >> 1;
                    switch ((data & 1))
                    {
                        case 0:
                            typeDef = TokenType.Field;
                            break;

                        case 1:
                            typeDef = TokenType.Param;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.HasDeclSecurity:
                    num = data >> 2;
                    switch ((data & 3))
                    {
                        case 0:
                            typeDef = TokenType.TypeDef;
                            break;

                        case 1:
                            typeDef = TokenType.Method;
                            break;

                        case 2:
                            typeDef = TokenType.Assembly;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.MemberRefParent:
                    num = data >> 3;
                    switch ((data & 7))
                    {
                        case 0:
                            typeDef = TokenType.TypeDef;
                            break;

                        case 1:
                            typeDef = TokenType.TypeRef;
                            break;

                        case 2:
                            typeDef = TokenType.ModuleRef;
                            break;

                        case 3:
                            typeDef = TokenType.Method;
                            break;

                        case 4:
                            typeDef = TokenType.TypeSpec;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.HasSemantics:
                    num = data >> 1;
                    switch ((data & 1))
                    {
                        case 0:
                            typeDef = TokenType.Event;
                            break;

                        case 1:
                            typeDef = TokenType.Property;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.MethodDefOrRef:
                    num = data >> 1;
                    switch ((data & 1))
                    {
                        case 0:
                            typeDef = TokenType.Method;
                            break;

                        case 1:
                            typeDef = TokenType.MemberRef;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.MemberForwarded:
                    num = data >> 1;
                    switch ((data & 1))
                    {
                        case 0:
                            typeDef = TokenType.Field;
                            break;

                        case 1:
                            typeDef = TokenType.Method;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.Implementation:
                    num = data >> 2;
                    switch ((data & 3))
                    {
                        case 0:
                            typeDef = TokenType.File;
                            break;

                        case 1:
                            typeDef = TokenType.AssemblyRef;
                            break;

                        case 2:
                            typeDef = TokenType.ExportedType;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.CustomAttributeType:
                    num = data >> 3;
                    switch ((data & 7))
                    {
                        case 2:
                            typeDef = TokenType.Method;
                            break;

                        case 3:
                            typeDef = TokenType.MemberRef;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.ResolutionScope:
                    num = data >> 2;
                    switch ((data & 3))
                    {
                        case 0:
                            typeDef = TokenType.Module;
                            break;

                        case 1:
                            typeDef = TokenType.ModuleRef;
                            break;

                        case 2:
                            typeDef = TokenType.AssemblyRef;
                            break;

                        case 3:
                            typeDef = TokenType.TypeRef;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                case CodedIndex.TypeOrMethodDef:
                    num = data >> 1;
                    switch ((data & 1))
                    {
                        case 0:
                            typeDef = TokenType.TypeDef;
                            break;

                        case 1:
                            typeDef = TokenType.Method;
                            break;

                        default:
                            goto TR_0000;
                    }
                    goto TR_0001;

                default:
                    break;
            }
        TR_0000:
            return MetadataToken.Zero;
        TR_0001:
            return new MetadataToken(typeDef, num);
        }

        public static TypeDefinition GetNestedType(this TypeDefinition self, string fullname)
        {
            if (self.HasNestedTypes)
            {
                Collection<TypeDefinition> nestedTypes = self.NestedTypes;
                for (int i = 0; i < nestedTypes.Count; i++)
                {
                    TypeDefinition definition = nestedTypes[i];
                    if (definition.TypeFullName() == fullname)
                    {
                        return definition;
                    }
                }
            }
            return null;
        }

        public static ParameterDefinition GetParameter(this MethodBody self, int index)
        {
            MethodDefinition method = self.method;
            if (method.HasThis)
            {
                if (index == 0)
                {
                    return self.ThisParameter;
                }
                index--;
            }
            Collection<ParameterDefinition> parameters = method.Parameters;
            return (((index < 0) || (index >= parameters.size)) ? null : parameters[index]);
        }

        public static Collection<SecurityDeclaration> GetSecurityDeclarations(this ISecurityDeclarationProvider self, ref Collection<SecurityDeclaration> variable, ModuleDefinition module)
        {
            if (!module.HasImage())
            {
                Collection<SecurityDeclaration> collection;
                variable = collection = new Collection<SecurityDeclaration>();
                return collection;
            }
            return module.Read<ISecurityDeclarationProvider, Collection<SecurityDeclaration>>(ref variable, self, (provider, reader) => reader.ReadSecurityDeclarations(provider));
        }

        public static bool GetSemantics(this MethodDefinition self, MethodSemanticsAttributes semantics) => 
            (((ushort) (self.SemanticsAttributes & semantics)) != 0);

        public static int GetSentinelPosition(this IMethodSignature self)
        {
            if (self.HasParameters)
            {
                Collection<ParameterDefinition> parameters = self.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].ParameterType.IsSentinel)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static int GetSize(this CodedIndex self, Func<Table, int> counter)
        {
            int num;
            Table[] tableArray;
            switch (self)
            {
                case CodedIndex.TypeDefOrRef:
                    num = 2;
                    tableArray = new Table[] { Table.TypeDef, Table.TypeRef, Table.TypeSpec };
                    break;

                case CodedIndex.HasConstant:
                    num = 2;
                    tableArray = new Table[] { Table.Field, Table.Param, Table.Property };
                    break;

                case CodedIndex.HasCustomAttribute:
                {
                    num = 5;
                    Table[] tableArray4 = new Table[] { Table.Method, Table.Field, Table.TypeRef, Table.TypeDef, Table.Param, Table.InterfaceImpl, Table.MemberRef };
                    tableArray4[8] = Table.DeclSecurity;
                    tableArray4[9] = Table.Property;
                    tableArray4[10] = Table.Event;
                    tableArray4[11] = Table.StandAloneSig;
                    tableArray4[12] = Table.ModuleRef;
                    tableArray4[13] = Table.TypeSpec;
                    tableArray4[14] = Table.Assembly;
                    tableArray4[15] = Table.AssemblyRef;
                    tableArray4[0x10] = Table.File;
                    tableArray4[0x11] = Table.ExportedType;
                    tableArray4[0x12] = Table.ManifestResource;
                    tableArray4[0x13] = Table.GenericParam;
                    tableArray = tableArray4;
                    break;
                }
                case CodedIndex.HasFieldMarshal:
                    num = 1;
                    tableArray = new Table[] { Table.Field, Table.Param };
                    break;

                case CodedIndex.HasDeclSecurity:
                    num = 2;
                    tableArray = new Table[] { Table.TypeDef, Table.Method, Table.Assembly };
                    break;

                case CodedIndex.MemberRefParent:
                    num = 3;
                    tableArray = new Table[] { Table.TypeDef, Table.TypeRef, Table.ModuleRef, Table.Method, Table.TypeSpec };
                    break;

                case CodedIndex.HasSemantics:
                    num = 1;
                    tableArray = new Table[] { Table.Event, Table.Property };
                    break;

                case CodedIndex.MethodDefOrRef:
                    num = 1;
                    tableArray = new Table[] { Table.Method, Table.MemberRef };
                    break;

                case CodedIndex.MemberForwarded:
                    num = 1;
                    tableArray = new Table[] { Table.Field, Table.Method };
                    break;

                case CodedIndex.Implementation:
                    num = 2;
                    tableArray = new Table[] { Table.File, Table.AssemblyRef, Table.ExportedType };
                    break;

                case CodedIndex.CustomAttributeType:
                    num = 3;
                    tableArray = new Table[] { Table.Method, Table.MemberRef };
                    break;

                case CodedIndex.ResolutionScope:
                {
                    num = 2;
                    Table[] tableArray13 = new Table[4];
                    tableArray13[1] = Table.ModuleRef;
                    tableArray13[2] = Table.AssemblyRef;
                    tableArray13[3] = Table.TypeRef;
                    tableArray = tableArray13;
                    break;
                }
                case CodedIndex.TypeOrMethodDef:
                    num = 1;
                    tableArray = new Table[] { Table.TypeDef, Table.Method };
                    break;

                default:
                    throw new ArgumentException();
            }
            int num2 = 0;
            for (int i = 0; i < tableArray.Length; i++)
            {
                num2 = Math.Max(counter(tableArray[i]), num2);
            }
            return ((num2 < (1 << ((0x10 - num) & 0x1f))) ? 2 : 4);
        }

        public static VariableDefinition GetVariable(this MethodBody self, int index)
        {
            Collection<VariableDefinition> variables = self.Variables;
            return (((index < 0) || (index >= variables.size)) ? null : variables[index]);
        }

        public static bool HasImage(this ModuleDefinition self) => 
            ((self != null) && self.HasImage);

        public static bool HasImplicitThis(this IMethodSignature self) => 
            (self.HasThis && !self.ExplicitThis);

        public static bool IsCorlib(this ModuleDefinition module) => 
            ((module.Assembly != null) ? (module.Assembly.Name.Name == "mscorlib") : false);

        public static bool IsNullOrEmpty<T>(this Collection<T> self) => 
            ((self == null) || (self.size == 0));

        public static bool IsNullOrEmpty<T>(this T[] self) => 
            ((self == null) || (self.Length == 0));

        public static bool IsPrimitive(this ElementType self)
        {
            switch (self)
            {
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
                case ElementType.I:
                case ElementType.U:
                    return true;
            }
            return false;
        }

        public static bool IsTypeOf(this TypeReference self, string @namespace, string name) => 
            ((self.Name == name) && (self.Namespace == @namespace));

        public static bool IsTypeSpecification(this TypeReference type)
        {
            ElementType etype = type.etype;
            switch (etype)
            {
                case ElementType.Ptr:
                case ElementType.ByRef:
                case ElementType.Var:
                case ElementType.Array:
                case ElementType.GenericInst:
                case ElementType.FnPtr:
                case ElementType.SzArray:
                case ElementType.MVar:
                case ElementType.CModReqD:
                case ElementType.CModOpt:
                    return true;

                case ElementType.ValueType:
                case ElementType.Class:
                case ElementType.TypedByRef:
                case (ElementType.Array | ElementType.Boolean | ElementType.Void):
                case ElementType.I:
                case ElementType.U:
                case (ElementType.Boolean | ElementType.ByRef | ElementType.I4):
                case ElementType.Object:
                    return false;
            }
            return ((etype == ElementType.Sentinel) || (etype == ElementType.Pinned));
        }

        public static bool IsVarArg(this IMethodSignature self) => 
            (((byte) (self.CallingConvention & MethodCallingConvention.VarArg)) != 0);

        public static void MethodSignatureFullName(this IMethodSignature self, StringBuilder builder)
        {
            builder.Append("(");
            if (self.HasParameters)
            {
                Collection<ParameterDefinition> parameters = self.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    ParameterDefinition definition = parameters[i];
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    if (definition.ParameterType.IsSentinel)
                    {
                        builder.Append("...,");
                    }
                    builder.Append(definition.ParameterType.FullName);
                }
            }
            builder.Append(")");
        }

        public static TargetRuntime ParseRuntime(this string self)
        {
            switch (self[1])
            {
                case '1':
                    return ((self[3] == '0') ? TargetRuntime.Net_1_0 : TargetRuntime.Net_1_1);

                case '2':
                    return TargetRuntime.Net_2_0;
            }
            return TargetRuntime.Net_4_0;
        }

        public static uint ReadCompressedUInt32(this byte[] data, ref int position)
        {
            uint num;
            if ((data[position] & 0x80) == 0)
            {
                num = data[position];
                position++;
            }
            else if ((data[position] & 0x40) == 0)
            {
                num = (uint) (((data[position] & -129) << 8) | data[position + 1]);
                position += 2;
            }
            else
            {
                num = (uint) (((((data[position] & -193) << 0x18) | (data[position + 1] << 0x10)) | (data[position + 2] << 8)) | data[position + 3]);
                position += 4;
            }
            return num;
        }

        public static T[] Resize<T>(this T[] self, int length)
        {
            Array.Resize<T>(ref self, length);
            return self;
        }

        public static void ResolveConstant(this IConstantProvider self, ref object constant, ModuleDefinition module)
        {
            if (module == null)
            {
                constant = NoValue;
            }
            else
            {
                lock (module.SyncRoot)
                {
                    if (constant == NotResolved)
                    {
                        if (!module.HasImage())
                        {
                            constant = NoValue;
                        }
                        else
                        {
                            constant = module.Read<IConstantProvider, object>(self, (provider, reader) => reader.ReadConstant(provider));
                        }
                    }
                }
            }
        }

        public static string RuntimeVersionString(this TargetRuntime runtime)
        {
            switch (runtime)
            {
                case TargetRuntime.Net_1_0:
                    return "v1.0.3705";

                case TargetRuntime.Net_1_1:
                    return "v1.1.4322";

                case TargetRuntime.Net_2_0:
                    return "v2.0.50727";
            }
            return "v4.0.30319";
        }

        public static ushort SetAttributes(this ushort self, ushort attributes, bool value) => 
            (!value ? ((ushort) (self & ~attributes)) : ((ushort) (self | attributes)));

        public static uint SetAttributes(this uint self, uint attributes, bool value) => 
            (!value ? (self & ~attributes) : (self | attributes));

        public static ushort SetMaskedAttributes(this ushort self, ushort mask, uint attributes, bool value)
        {
            if (!value)
            {
                return (ushort) (self & ~(mask & attributes));
            }
            self = (ushort) (self & ~mask);
            return (ushort) (self | attributes);
        }

        public static uint SetMaskedAttributes(this uint self, uint mask, uint attributes, bool value)
        {
            if (!value)
            {
                return (self & ~(mask & attributes));
            }
            self &= ~mask;
            return (self | attributes);
        }

        public static void SetSemantics(this MethodDefinition self, MethodSemanticsAttributes semantics, bool value)
        {
            if (value)
            {
                self.SemanticsAttributes = (MethodSemanticsAttributes) ((ushort) (self.SemanticsAttributes | semantics));
            }
            else
            {
                self.SemanticsAttributes = (MethodSemanticsAttributes) ((ushort) (self.SemanticsAttributes & ((ushort) ~semantics)));
            }
        }

        private static bool TryGetKeyContainer(ISerializable key_pair, out byte[] key, out string key_container)
        {
            SerializationInfo info = new SerializationInfo(typeof(StrongNameKeyPair), new FormatterConverter());
            StreamingContext context = new StreamingContext();
            key_pair.GetObjectData(info, context);
            key = (byte[]) info.GetValue("_keyPairArray", typeof(byte[]));
            key_container = info.GetString("_keyPairContainer");
            return (key_container != null);
        }

        public static string TypeFullName(this TypeReference self) => 
            (string.IsNullOrEmpty(self.Namespace) ? self.Name : (self.Namespace + '.' + self.Name));
    }
}

