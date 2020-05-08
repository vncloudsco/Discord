namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class SR
    {
        private static System.Resources.ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal SR()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new System.Resources.ResourceManager("Microsoft.Web.XmlTransform.SR", typeof(SR).Assembly);
                }
                return resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => 
                resourceCulture;
            set => 
                (resourceCulture = value);
        }

        internal static string XMLTRANSFORMATION_AmbiguousTypeMatch =>
            ResourceManager.GetString("XMLTRANSFORMATION_AmbiguousTypeMatch", resourceCulture);

        internal static string XMLTRANSFORMATION_BadAttributeValue =>
            ResourceManager.GetString("XMLTRANSFORMATION_BadAttributeValue", resourceCulture);

        internal static string XMLTRANSFORMATION_FatalTransformSyntaxError =>
            ResourceManager.GetString("XMLTRANSFORMATION_FatalTransformSyntaxError", resourceCulture);

        internal static string XMLTRANSFORMATION_ImportAttributeConflict =>
            ResourceManager.GetString("XMLTRANSFORMATION_ImportAttributeConflict", resourceCulture);

        internal static string XMLTRANSFORMATION_ImportMissingAssembly =>
            ResourceManager.GetString("XMLTRANSFORMATION_ImportMissingAssembly", resourceCulture);

        internal static string XMLTRANSFORMATION_ImportMissingNamespace =>
            ResourceManager.GetString("XMLTRANSFORMATION_ImportMissingNamespace", resourceCulture);

        internal static string XMLTRANSFORMATION_ImportUnknownAttribute =>
            ResourceManager.GetString("XMLTRANSFORMATION_ImportUnknownAttribute", resourceCulture);

        internal static string XMLTRANSFORMATION_IncorrectBaseType =>
            ResourceManager.GetString("XMLTRANSFORMATION_IncorrectBaseType", resourceCulture);

        internal static string XMLTRANSFORMATION_InsertBadXPath =>
            ResourceManager.GetString("XMLTRANSFORMATION_InsertBadXPath", resourceCulture);

        internal static string XMLTRANSFORMATION_InsertBadXPathResult =>
            ResourceManager.GetString("XMLTRANSFORMATION_InsertBadXPathResult", resourceCulture);

        internal static string XMLTRANSFORMATION_InsertMissingArgument =>
            ResourceManager.GetString("XMLTRANSFORMATION_InsertMissingArgument", resourceCulture);

        internal static string XMLTRANSFORMATION_InsertTooManyArguments =>
            ResourceManager.GetString("XMLTRANSFORMATION_InsertTooManyArguments", resourceCulture);

        internal static string XMLTRANSFORMATION_MatchAttributeDoesNotExist =>
            ResourceManager.GetString("XMLTRANSFORMATION_MatchAttributeDoesNotExist", resourceCulture);

        internal static string XMLTRANSFORMATION_NoValidConstructor =>
            ResourceManager.GetString("XMLTRANSFORMATION_NoValidConstructor", resourceCulture);

        internal static string XMLTRANSFORMATION_RequiresExactArguments =>
            ResourceManager.GetString("XMLTRANSFORMATION_RequiresExactArguments", resourceCulture);

        internal static string XMLTRANSFORMATION_RequiresMinimumArguments =>
            ResourceManager.GetString("XMLTRANSFORMATION_RequiresMinimumArguments", resourceCulture);

        internal static string XMLTRANSFORMATION_TooManyArguments =>
            ResourceManager.GetString("XMLTRANSFORMATION_TooManyArguments", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformArgumentFoundNoAttributes =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformArgumentFoundNoAttributes", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformBeginExecutingMessage =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformBeginExecutingMessage", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformDoesNotExpectArguments =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformDoesNotExpectArguments", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformEndExecutingMessage =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformEndExecutingMessage", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformErrorExecutingMessage =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformErrorExecutingMessage", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageInsert =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageInsert", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageNoRemoveAttributes =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageNoRemoveAttributes", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageNoSetAttributes =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageNoSetAttributes", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageRemove =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageRemove", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageRemoveAttribute =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageRemoveAttribute", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageRemoveAttributes =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageRemoveAttributes", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageReplace =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageReplace", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageSetAttribute =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageSetAttribute", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformMessageSetAttributes =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformMessageSetAttributes", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformNameFormatLong =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformNameFormatLong", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformNameFormatShort =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformNameFormatShort", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformNoMatchingTargetNodes =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformNoMatchingTargetNodes", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformNotExecutingMessage =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformNotExecutingMessage", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformOnlyAppliesOnce =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformOnlyAppliesOnce", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformSourceMatchWasRemoved =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformSourceMatchWasRemoved", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformStatusApplyTarget =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformStatusApplyTarget", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformStatusApplyTargetNoLineInfo =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformStatusApplyTargetNoLineInfo", resourceCulture);

        internal static string XMLTRANSFORMATION_TransformStatusXPath =>
            ResourceManager.GetString("XMLTRANSFORMATION_TransformStatusXPath", resourceCulture);

        internal static string XMLTRANSFORMATION_UnknownTypeName =>
            ResourceManager.GetString("XMLTRANSFORMATION_UnknownTypeName", resourceCulture);

        internal static string XMLTRANSFORMATION_UnknownXdtTag =>
            ResourceManager.GetString("XMLTRANSFORMATION_UnknownXdtTag", resourceCulture);
    }
}

