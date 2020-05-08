namespace NuGet
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class SemanticVersionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => 
            (sourceType == typeof(string));

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            SemanticVersion version;
            string str = value as string;
            return (((str == null) || !SemanticVersion.TryParse(str, out version)) ? null : version);
        }
    }
}

