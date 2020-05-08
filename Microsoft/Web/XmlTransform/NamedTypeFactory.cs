namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    internal class NamedTypeFactory
    {
        private string relativePathRoot;
        private List<Registration> registrations = new List<Registration>();

        internal NamedTypeFactory(string relativePathRoot)
        {
            this.relativePathRoot = relativePathRoot;
            this.CreateDefaultRegistrations();
        }

        internal void AddAssemblyRegistration(Assembly assembly, string nameSpace)
        {
            this.registrations.Add(new Registration(assembly, nameSpace));
        }

        internal void AddAssemblyRegistration(string assemblyName, string nameSpace)
        {
            this.registrations.Add(new AssemblyNameRegistration(assemblyName, nameSpace));
        }

        internal void AddPathRegistration(string path, string nameSpace)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Path.GetDirectoryName(this.relativePathRoot), path);
            }
            this.registrations.Add(new PathRegistration(path, nameSpace));
        }

        internal ObjectType Construct<ObjectType>(string typeName) where ObjectType: class
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return default(ObjectType);
            }
            Type type = this.GetType(typeName);
            if (type == null)
            {
                object[] objArray = new object[] { typeName, typeof(ObjectType).Name };
                throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_UnknownTypeName, objArray));
            }
            if (!type.IsSubclassOf(typeof(ObjectType)))
            {
                object[] objArray2 = new object[] { type.FullName, typeof(ObjectType).Name };
                throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_IncorrectBaseType, objArray2));
            }
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                return (constructor.Invoke(new object[0]) as ObjectType);
            }
            object[] args = new object[] { type.FullName };
            throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_NoValidConstructor, args));
        }

        private void CreateDefaultRegistrations()
        {
            this.AddAssemblyRegistration(base.GetType().Assembly, base.GetType().Namespace);
        }

        private Type GetType(string typeName)
        {
            Type type = null;
            foreach (Registration registration in this.registrations)
            {
                if (registration.IsValid)
                {
                    Type type2 = registration.Assembly.GetType(registration.NameSpace + "." + typeName);
                    if (type2 != null)
                    {
                        if (type != null)
                        {
                            object[] args = new object[] { typeName };
                            throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_AmbiguousTypeMatch, args));
                        }
                        type = type2;
                    }
                }
            }
            return type;
        }

        private class AssemblyNameRegistration : NamedTypeFactory.Registration
        {
            public AssemblyNameRegistration(string assemblyName, string nameSpace) : base(Assembly.Load(assemblyName), nameSpace)
            {
            }
        }

        private class PathRegistration : NamedTypeFactory.Registration
        {
            public PathRegistration(string path, string nameSpace) : base(Assembly.LoadFrom(path), nameSpace)
            {
            }
        }

        private class Registration
        {
            private System.Reflection.Assembly assembly;
            private string nameSpace;

            public Registration(System.Reflection.Assembly assembly, string nameSpace)
            {
                this.assembly = assembly;
                this.nameSpace = nameSpace;
            }

            public bool IsValid =>
                (this.assembly != null);

            public string NameSpace =>
                this.nameSpace;

            public System.Reflection.Assembly Assembly =>
                this.assembly;
        }
    }
}

