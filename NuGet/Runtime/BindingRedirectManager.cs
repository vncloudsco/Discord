namespace NuGet.Runtime
{
    using NuGet;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    internal class BindingRedirectManager
    {
        private static readonly XName AssemblyBindingName = AssemblyBinding.GetQualifiedName("assemblyBinding");
        private static readonly XName DependentAssemblyName = AssemblyBinding.GetQualifiedName("dependentAssembly");
        private readonly IFileSystem _fileSystem;
        private readonly string _configurationPath;

        public BindingRedirectManager(IFileSystem fileSystem, string configurationPath)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (string.IsNullOrEmpty(configurationPath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "configurationPath");
            }
            this._fileSystem = fileSystem;
            this._configurationPath = configurationPath;
        }

        public void AddBindingRedirects(IEnumerable<AssemblyBinding> bindingRedirects)
        {
            if (bindingRedirects == null)
            {
                throw new ArgumentNullException("bindingRedirects");
            }
            if (bindingRedirects.Any<AssemblyBinding>())
            {
                XElement element;
                XDocument configuration = this.GetConfiguration();
                if (configuration.Root.Element("runtime") == null)
                {
                    element = new XElement("runtime");
                    configuration.Root.AddIndented(element);
                }
                ILookup<AssemblyBinding, XElement> assemblyBindings = GetAssemblyBindings(configuration);
                XElement container = null;
                foreach (AssemblyBinding binding in bindingRedirects)
                {
                    if (assemblyBindings.Contains(binding))
                    {
                        IEnumerable<XElement> source = assemblyBindings[binding];
                        if (source.Any<XElement>())
                        {
                            using (IEnumerator<XElement> enumerator2 = source.Skip<XElement>(1).GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    RemoveElement(enumerator2.Current);
                                }
                            }
                            UpdateBindingRedirectElement(source.First<XElement>(), binding);
                            continue;
                        }
                    }
                    if (container == null)
                    {
                        container = GetAssemblyBindingElement(element);
                    }
                    container.AddIndented(binding.ToXElement());
                }
                this.Save(configuration);
            }
        }

        private static XElement GetAssemblyBindingElement(XElement runtime)
        {
            XElement content = runtime.Elements(AssemblyBindingName).FirstOrDefault<XElement>();
            if (content == null)
            {
                content = new XElement(AssemblyBindingName);
                runtime.AddIndented(content);
            }
            return content;
        }

        private static IEnumerable<XElement> GetAssemblyBindingElements(XElement runtime) => 
            runtime.Elements(AssemblyBindingName).Elements<XElement>(DependentAssemblyName);

        private static ILookup<AssemblyBinding, XElement> GetAssemblyBindings(XDocument document)
        {
            XElement runtime = document.Root.Element("runtime");
            IEnumerable<XElement> assemblyBindingElements = Enumerable.Empty<XElement>();
            if (runtime != null)
            {
                assemblyBindingElements = GetAssemblyBindingElements(runtime);
            }
            return Enumerable.ToLookup(from dependentAssemblyElement in assemblyBindingElements select new { 
                Binding = AssemblyBinding.Parse(dependentAssemblyElement),
                Element = dependentAssemblyElement
            }, p => p.Binding, p => p.Element);
        }

        private XDocument GetConfiguration() => 
            XmlUtility.GetOrCreateDocument("configuration", this._fileSystem, this._configurationPath);

        public void RemoveBindingRedirects(IEnumerable<AssemblyBinding> bindingRedirects)
        {
            if (bindingRedirects == null)
            {
                throw new ArgumentNullException("bindingRedirects");
            }
            if (bindingRedirects.Any<AssemblyBinding>())
            {
                XDocument configuration = this.GetConfiguration();
                ILookup<AssemblyBinding, XElement> assemblyBindings = GetAssemblyBindings(configuration);
                if (assemblyBindings.Any<IGrouping<AssemblyBinding, XElement>>())
                {
                    foreach (AssemblyBinding binding in bindingRedirects)
                    {
                        if (assemblyBindings.Contains(binding))
                        {
                            using (IEnumerator<XElement> enumerator2 = assemblyBindings[binding].GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    RemoveElement(enumerator2.Current);
                                }
                            }
                        }
                    }
                    this.Save(configuration);
                }
            }
        }

        private static void RemoveElement(XElement element)
        {
            XElement parent = element.Parent;
            element.RemoveIndented();
            if (!parent.HasElements)
            {
                parent.RemoveIndented();
            }
        }

        private void Save(XDocument document)
        {
            this._fileSystem.AddFile(this._configurationPath, new Action<Stream>(document.Save));
        }

        private static void UpdateBindingRedirectElement(XElement element, AssemblyBinding bindingRedirect)
        {
            XElement element1 = element.Element(AssemblyBinding.GetQualifiedName("bindingRedirect"));
            element1.Attribute("oldVersion").SetValue(bindingRedirect.OldVersion);
            element1.Attribute("newVersion").SetValue(bindingRedirect.NewVersion);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BindingRedirectManager.<>c <>9 = new BindingRedirectManager.<>c();
            public static Func<XElement, <>f__AnonymousType17<AssemblyBinding, XElement>> <>9__10_0;
            public static Func<<>f__AnonymousType17<AssemblyBinding, XElement>, AssemblyBinding> <>9__10_1;
            public static Func<<>f__AnonymousType17<AssemblyBinding, XElement>, XElement> <>9__10_2;

            internal <>f__AnonymousType17<AssemblyBinding, XElement> <GetAssemblyBindings>b__10_0(XElement dependentAssemblyElement) => 
                new { 
                    Binding = AssemblyBinding.Parse(dependentAssemblyElement),
                    Element = dependentAssemblyElement
                };

            internal AssemblyBinding <GetAssemblyBindings>b__10_1(<>f__AnonymousType17<AssemblyBinding, XElement> p) => 
                p.Binding;

            internal XElement <GetAssemblyBindings>b__10_2(<>f__AnonymousType17<AssemblyBinding, XElement> p) => 
                p.Element;
        }
    }
}

