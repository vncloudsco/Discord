namespace Mono.Cecil
{
    internal interface IMetadataResolver
    {
        FieldDefinition Resolve(FieldReference field);
        MethodDefinition Resolve(MethodReference method);
        TypeDefinition Resolve(TypeReference type);
    }
}

