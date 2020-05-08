namespace NuGet
{
    using Microsoft.CSharp.RuntimeBinder;
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class Preprocessor : IPackageFileTransformer
    {
        internal static string Process(IPackageFile file, IPropertyProvider propertyProvider)
        {
            using (Stream stream = file.GetStream())
            {
                return Process(stream, propertyProvider, false);
            }
        }

        public static string Process(Stream stream, IPropertyProvider propertyProvider, bool throwIfNotFound = true)
        {
            Tokenizer tokenizer = new Tokenizer(stream.ReadToEnd());
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                Token token = tokenizer.Read();
                if (token == null)
                {
                    return builder.ToString();
                }
                if (token.Category == TokenCategory.Variable)
                {
                    builder.Append(ReplaceToken(token.Value, propertyProvider, throwIfNotFound));
                    continue;
                }
                builder.Append(token.Value);
            }
        }

        private static string ReplaceToken(string propertyName, IPropertyProvider propertyProvider, bool throwIfNotFound)
        {
            object local1;
            object propertyValue = propertyProvider.GetPropertyValue(propertyName);
            if (<>o__4.<>p__3 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                <>o__4.<>p__3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, (ExpressionType) 0x53, typeof(Preprocessor), argumentInfo));
            }
            if (<>o__4.<>p__0 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null) };
                <>o__4.<>p__0 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(Preprocessor), argumentInfo));
            }
            object obj3 = <>o__4.<>p__0.Target(<>o__4.<>p__0, propertyValue, null);
            if (<>o__4.<>p__2 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                <>o__4.<>p__2 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, (ExpressionType) 0x54, typeof(Preprocessor), argumentInfo));
            }
            if (<>o__4.<>p__2.Target(<>o__4.<>p__2, obj3))
            {
                local1 = obj3;
            }
            else
            {
                if (<>o__4.<>p__1 == null)
                {
                    CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                    <>o__4.<>p__1 = CallSite<Func<CallSite, object, bool, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(Preprocessor), argumentInfo));
                }
                local1 = <>o__4.<>p__1.Target(<>o__4.<>p__1, obj3, throwIfNotFound);
            }
            if (<>o__4.<>p__3.Target(<>o__4.<>p__3, local1))
            {
                object[] args = new object[] { propertyName };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.TokenHasNoValue, args));
            }
            if (<>o__4.<>p__4 == null)
            {
                <>o__4.<>p__4 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(Preprocessor)));
            }
            return <>o__4.<>p__4.Target(<>o__4.<>p__4, propertyValue);
        }

        public void RevertFile(IPackageFile file, string targetPath, IEnumerable<IPackageFile> matchingFiles, IProjectSystem projectSystem)
        {
            Func<Stream> streamFactory = () => Process(file, projectSystem).AsStream();
            projectSystem.DeleteFileSafe(targetPath, streamFactory);
        }

        public void TransformFile(IPackageFile file, string targetPath, IProjectSystem projectSystem)
        {
            ProjectSystemExtensions.TryAddFile(projectSystem, targetPath, () => Process(file, projectSystem).AsStream());
        }

        [CompilerGenerated]
        private static class <>o__4
        {
            public static CallSite<Func<CallSite, object, object, object>> <>p__0;
            public static CallSite<Func<CallSite, object, bool, object>> <>p__1;
            public static CallSite<Func<CallSite, object, bool>> <>p__2;
            public static CallSite<Func<CallSite, object, bool>> <>p__3;
            public static CallSite<Func<CallSite, object, string>> <>p__4;
        }
    }
}

