namespace NuGet.Resources
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [DebuggerNonUserCode, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), CompilerGenerated]
    internal class NuGetResources
    {
        private static System.Resources.ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal NuGetResources()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan == null)
                {
                    resourceMan = new System.Resources.ResourceManager("NuGet.Resources.NuGetResources", typeof(NuGetResources).Assembly);
                }
                return resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static CultureInfo Culture
        {
            get => 
                resourceCulture;
            set => 
                (resourceCulture = value);
        }

        public static string AdditonalConstraintsDefined =>
            ResourceManager.GetString("AdditonalConstraintsDefined", resourceCulture);

        public static string AggregateQueriesRequireOrder =>
            ResourceManager.GetString("AggregateQueriesRequireOrder", resourceCulture);

        public static string CannotCreateEmptyPackage =>
            ResourceManager.GetString("CannotCreateEmptyPackage", resourceCulture);

        public static string CircularDependencyDetected =>
            ResourceManager.GetString("CircularDependencyDetected", resourceCulture);

        public static string ConflictErrorWithDependent =>
            ResourceManager.GetString("ConflictErrorWithDependent", resourceCulture);

        public static string ConflictErrorWithDependents =>
            ResourceManager.GetString("ConflictErrorWithDependents", resourceCulture);

        public static string Debug_AddedFile =>
            ResourceManager.GetString("Debug_AddedFile", resourceCulture);

        public static string Debug_AddedFileToFolder =>
            ResourceManager.GetString("Debug_AddedFileToFolder", resourceCulture);

        public static string Debug_LookingForUpdates =>
            ResourceManager.GetString("Debug_LookingForUpdates", resourceCulture);

        public static string Debug_RemovedFile =>
            ResourceManager.GetString("Debug_RemovedFile", resourceCulture);

        public static string Debug_RemovedFileFromFolder =>
            ResourceManager.GetString("Debug_RemovedFileFromFolder", resourceCulture);

        public static string Debug_RemovedFolder =>
            ResourceManager.GetString("Debug_RemovedFolder", resourceCulture);

        public static string Debug_TargetFrameworkInfo =>
            ResourceManager.GetString("Debug_TargetFrameworkInfo", resourceCulture);

        public static string Debug_TargetFrameworkInfo_AssemblyReferences =>
            ResourceManager.GetString("Debug_TargetFrameworkInfo_AssemblyReferences", resourceCulture);

        public static string Debug_TargetFrameworkInfo_BuildFiles =>
            ResourceManager.GetString("Debug_TargetFrameworkInfo_BuildFiles", resourceCulture);

        public static string Debug_TargetFrameworkInfo_ContentFiles =>
            ResourceManager.GetString("Debug_TargetFrameworkInfo_ContentFiles", resourceCulture);

        public static string Debug_TargetFrameworkInfo_NotFrameworkSpecific =>
            ResourceManager.GetString("Debug_TargetFrameworkInfo_NotFrameworkSpecific", resourceCulture);

        public static string Debug_TargetFrameworkInfo_PowershellScripts =>
            ResourceManager.GetString("Debug_TargetFrameworkInfo_PowershellScripts", resourceCulture);

        public static string Debug_TargetFrameworkInfoPrefix =>
            ResourceManager.GetString("Debug_TargetFrameworkInfoPrefix", resourceCulture);

        public static string DependencyHasInvalidVersion =>
            ResourceManager.GetString("DependencyHasInvalidVersion", resourceCulture);

        public static string DependencyOnlyCannotMixDependencies =>
            ResourceManager.GetString("DependencyOnlyCannotMixDependencies", resourceCulture);

        public static string DownloadProgressStatus =>
            ResourceManager.GetString("DownloadProgressStatus", resourceCulture);

        public static string DuplicateDependenciesDefined =>
            ResourceManager.GetString("DuplicateDependenciesDefined", resourceCulture);

        public static string Error_InvalidPackage =>
            ResourceManager.GetString("Error_InvalidPackage", resourceCulture);

        public static string Error_NoWritableConfig =>
            ResourceManager.GetString("Error_NoWritableConfig", resourceCulture);

        public static string Error_PackageAlreadyExists =>
            ResourceManager.GetString("Error_PackageAlreadyExists", resourceCulture);

        public static string Error_TooManyRedirections =>
            ResourceManager.GetString("Error_TooManyRedirections", resourceCulture);

        public static string ErrorLoadingPackages =>
            ResourceManager.GetString("ErrorLoadingPackages", resourceCulture);

        public static string ErrorReadingFile =>
            ResourceManager.GetString("ErrorReadingFile", resourceCulture);

        public static string ErrorReadingPackage =>
            ResourceManager.GetString("ErrorReadingPackage", resourceCulture);

        public static string ExternalPackagesCannotDependOnProjectLevelPackages =>
            ResourceManager.GetString("ExternalPackagesCannotDependOnProjectLevelPackages", resourceCulture);

        public static string FileConflictMessage =>
            ResourceManager.GetString("FileConflictMessage", resourceCulture);

        public static string FileDoesNotExit =>
            ResourceManager.GetString("FileDoesNotExit", resourceCulture);

        public static string GetUpdatesParameterMismatch =>
            ResourceManager.GetString("GetUpdatesParameterMismatch", resourceCulture);

        public static string IncompatibleSchema =>
            ResourceManager.GetString("IncompatibleSchema", resourceCulture);

        public static string Info_OverwriteExistingFile =>
            ResourceManager.GetString("Info_OverwriteExistingFile", resourceCulture);

        public static string InvalidFeed =>
            ResourceManager.GetString("InvalidFeed", resourceCulture);

        public static string InvalidFrameworkNameFormat =>
            ResourceManager.GetString("InvalidFrameworkNameFormat", resourceCulture);

        public static string InvalidNullSettingsOperation =>
            ResourceManager.GetString("InvalidNullSettingsOperation", resourceCulture);

        public static string InvalidPackageId =>
            ResourceManager.GetString("InvalidPackageId", resourceCulture);

        public static string InvalidVersionString =>
            ResourceManager.GetString("InvalidVersionString", resourceCulture);

        public static string Log_ApplyingConstraints =>
            ResourceManager.GetString("Log_ApplyingConstraints", resourceCulture);

        public static string Log_AttemptingToRetrievePackageFromSource =>
            ResourceManager.GetString("Log_AttemptingToRetrievePackageFromSource", resourceCulture);

        public static string Log_BeginAddPackageReference =>
            ResourceManager.GetString("Log_BeginAddPackageReference", resourceCulture);

        public static string Log_BeginInstallPackage =>
            ResourceManager.GetString("Log_BeginInstallPackage", resourceCulture);

        public static string Log_BeginRemovePackageReference =>
            ResourceManager.GetString("Log_BeginRemovePackageReference", resourceCulture);

        public static string Log_BeginUninstallPackage =>
            ResourceManager.GetString("Log_BeginUninstallPackage", resourceCulture);

        public static string Log_InstallPackage =>
            ResourceManager.GetString("Log_InstallPackage", resourceCulture);

        public static string Log_InstallPackageIntoProject =>
            ResourceManager.GetString("Log_InstallPackageIntoProject", resourceCulture);

        public static string Log_NoUpdatesAvailable =>
            ResourceManager.GetString("Log_NoUpdatesAvailable", resourceCulture);

        public static string Log_NoUpdatesAvailableForProject =>
            ResourceManager.GetString("Log_NoUpdatesAvailableForProject", resourceCulture);

        public static string Log_PackageAlreadyInstalled =>
            ResourceManager.GetString("Log_PackageAlreadyInstalled", resourceCulture);

        public static string Log_PackageInstalledSuccessfully =>
            ResourceManager.GetString("Log_PackageInstalledSuccessfully", resourceCulture);

        public static string Log_ProjectAlreadyReferencesPackage =>
            ResourceManager.GetString("Log_ProjectAlreadyReferencesPackage", resourceCulture);

        public static string Log_SuccessfullyAddedPackageReference =>
            ResourceManager.GetString("Log_SuccessfullyAddedPackageReference", resourceCulture);

        public static string Log_SuccessfullyRemovedPackageReference =>
            ResourceManager.GetString("Log_SuccessfullyRemovedPackageReference", resourceCulture);

        public static string Log_SuccessfullyUninstalledPackage =>
            ResourceManager.GetString("Log_SuccessfullyUninstalledPackage", resourceCulture);

        public static string Log_UninstallPackage =>
            ResourceManager.GetString("Log_UninstallPackage", resourceCulture);

        public static string Log_UninstallPackageFromProject =>
            ResourceManager.GetString("Log_UninstallPackageFromProject", resourceCulture);

        public static string Log_UpdatingPackages =>
            ResourceManager.GetString("Log_UpdatingPackages", resourceCulture);

        public static string Log_UpdatingPackagesWithoutOldVersion =>
            ResourceManager.GetString("Log_UpdatingPackagesWithoutOldVersion", resourceCulture);

        public static string Manifest_AssemblyNameRequired =>
            ResourceManager.GetString("Manifest_AssemblyNameRequired", resourceCulture);

        public static string Manifest_DependenciesHasMixedElements =>
            ResourceManager.GetString("Manifest_DependenciesHasMixedElements", resourceCulture);

        public static string Manifest_DependencyIdRequired =>
            ResourceManager.GetString("Manifest_DependencyIdRequired", resourceCulture);

        public static string Manifest_ExcludeContainsInvalidCharacters =>
            ResourceManager.GetString("Manifest_ExcludeContainsInvalidCharacters", resourceCulture);

        public static string Manifest_IdMaxLengthExceeded =>
            ResourceManager.GetString("Manifest_IdMaxLengthExceeded", resourceCulture);

        public static string Manifest_InvalidMinClientVersion =>
            ResourceManager.GetString("Manifest_InvalidMinClientVersion", resourceCulture);

        public static string Manifest_InvalidPrereleaseDependency =>
            ResourceManager.GetString("Manifest_InvalidPrereleaseDependency", resourceCulture);

        public static string Manifest_InvalidReference =>
            ResourceManager.GetString("Manifest_InvalidReference", resourceCulture);

        public static string Manifest_InvalidReferenceFile =>
            ResourceManager.GetString("Manifest_InvalidReferenceFile", resourceCulture);

        public static string Manifest_InvalidSchemaNamespace =>
            ResourceManager.GetString("Manifest_InvalidSchemaNamespace", resourceCulture);

        public static string Manifest_NotAvailable =>
            ResourceManager.GetString("Manifest_NotAvailable", resourceCulture);

        public static string Manifest_NotFound =>
            ResourceManager.GetString("Manifest_NotFound", resourceCulture);

        public static string Manifest_ReferencesHasMixedElements =>
            ResourceManager.GetString("Manifest_ReferencesHasMixedElements", resourceCulture);

        public static string Manifest_ReferencesIsEmpty =>
            ResourceManager.GetString("Manifest_ReferencesIsEmpty", resourceCulture);

        public static string Manifest_RequiredElementMissing =>
            ResourceManager.GetString("Manifest_RequiredElementMissing", resourceCulture);

        public static string Manifest_RequiredMetadataMissing =>
            ResourceManager.GetString("Manifest_RequiredMetadataMissing", resourceCulture);

        public static string Manifest_RequireLicenseAcceptanceRequiresLicenseUrl =>
            ResourceManager.GetString("Manifest_RequireLicenseAcceptanceRequiresLicenseUrl", resourceCulture);

        public static string Manifest_SourceContainsInvalidCharacters =>
            ResourceManager.GetString("Manifest_SourceContainsInvalidCharacters", resourceCulture);

        public static string Manifest_TargetContainsInvalidCharacters =>
            ResourceManager.GetString("Manifest_TargetContainsInvalidCharacters", resourceCulture);

        public static string Manifest_UriCannotBeEmpty =>
            ResourceManager.GetString("Manifest_UriCannotBeEmpty", resourceCulture);

        public static string MissingFrameworkName =>
            ResourceManager.GetString("MissingFrameworkName", resourceCulture);

        public static string NewerVersionAlreadyReferenced =>
            ResourceManager.GetString("NewerVersionAlreadyReferenced", resourceCulture);

        public static string PackageAuthoring_FileNotFound =>
            ResourceManager.GetString("PackageAuthoring_FileNotFound", resourceCulture);

        public static string PackageDoesNotContainManifest =>
            ResourceManager.GetString("PackageDoesNotContainManifest", resourceCulture);

        public static string PackageHasDependent =>
            ResourceManager.GetString("PackageHasDependent", resourceCulture);

        public static string PackageHasDependents =>
            ResourceManager.GetString("PackageHasDependents", resourceCulture);

        public static string PackageMinVersionNotSatisfied =>
            ResourceManager.GetString("PackageMinVersionNotSatisfied", resourceCulture);

        public static string PackageRestoreConsentCheckBoxText =>
            ResourceManager.GetString("PackageRestoreConsentCheckBoxText", resourceCulture);

        public static string PackageServerError =>
            ResourceManager.GetString("PackageServerError", resourceCulture);

        public static string PortableFrameworkProfileComponentIsEmpty =>
            ResourceManager.GetString("PortableFrameworkProfileComponentIsEmpty", resourceCulture);

        public static string PortableFrameworkProfileComponentIsPortable =>
            ResourceManager.GetString("PortableFrameworkProfileComponentIsPortable", resourceCulture);

        public static string PortableFrameworkProfileEmpty =>
            ResourceManager.GetString("PortableFrameworkProfileEmpty", resourceCulture);

        public static string PortableFrameworkProfileHasDash =>
            ResourceManager.GetString("PortableFrameworkProfileHasDash", resourceCulture);

        public static string PortableFrameworkProfileHasSpace =>
            ResourceManager.GetString("PortableFrameworkProfileHasSpace", resourceCulture);

        public static string PortableProfileTableMustBeSpecified =>
            ResourceManager.GetString("PortableProfileTableMustBeSpecified", resourceCulture);

        public static string PortableProfileTableRequiresForGetShortFrameworkName =>
            ResourceManager.GetString("PortableProfileTableRequiresForGetShortFrameworkName", resourceCulture);

        public static string ProjectDoesNotHaveReference =>
            ResourceManager.GetString("ProjectDoesNotHaveReference", resourceCulture);

        public static string ReferenceFile_InvalidDevelopmentFlag =>
            ResourceManager.GetString("ReferenceFile_InvalidDevelopmentFlag", resourceCulture);

        public static string ReferenceFile_InvalidRequireReinstallationFlag =>
            ResourceManager.GetString("ReferenceFile_InvalidRequireReinstallationFlag", resourceCulture);

        public static string ReferenceFile_InvalidVersion =>
            ResourceManager.GetString("ReferenceFile_InvalidVersion", resourceCulture);

        public static string SemVerSpecialVersionTooLong =>
            ResourceManager.GetString("SemVerSpecialVersionTooLong", resourceCulture);

        public static string SettingsCredentials_UsingSavedCredentials =>
            ResourceManager.GetString("SettingsCredentials_UsingSavedCredentials", resourceCulture);

        public static string SupportedFrameworkIsNull =>
            ResourceManager.GetString("SupportedFrameworkIsNull", resourceCulture);

        public static string TokenHasNoValue =>
            ResourceManager.GetString("TokenHasNoValue", resourceCulture);

        public static string TypeMustBeASemanticVersion =>
            ResourceManager.GetString("TypeMustBeASemanticVersion", resourceCulture);

        public static string UnableToFindCompatibleItems =>
            ResourceManager.GetString("UnableToFindCompatibleItems", resourceCulture);

        public static string UnableToLocateDependency =>
            ResourceManager.GetString("UnableToLocateDependency", resourceCulture);

        public static string UnableToLocateWIF =>
            ResourceManager.GetString("UnableToLocateWIF", resourceCulture);

        public static string UnableToResolveDependency =>
            ResourceManager.GetString("UnableToResolveDependency", resourceCulture);

        public static string UnableToResolveUri =>
            ResourceManager.GetString("UnableToResolveUri", resourceCulture);

        public static string UnknownPackage =>
            ResourceManager.GetString("UnknownPackage", resourceCulture);

        public static string UnknownPackageSpecificVersion =>
            ResourceManager.GetString("UnknownPackageSpecificVersion", resourceCulture);

        public static string UnknownSchemaVersion =>
            ResourceManager.GetString("UnknownSchemaVersion", resourceCulture);

        public static string UnsupportedHashAlgorithm =>
            ResourceManager.GetString("UnsupportedHashAlgorithm", resourceCulture);

        public static string UserSettings_UnableToParseConfigFile =>
            ResourceManager.GetString("UserSettings_UnableToParseConfigFile", resourceCulture);

        public static string Warning_FileAlreadyExists =>
            ResourceManager.GetString("Warning_FileAlreadyExists", resourceCulture);

        public static string Warning_FileModified =>
            ResourceManager.GetString("Warning_FileModified", resourceCulture);

        public static string Warning_PackageSkippedBecauseItIsInUse =>
            ResourceManager.GetString("Warning_PackageSkippedBecauseItIsInUse", resourceCulture);

        public static string Warning_UninstallingPackageWillBreakDependents =>
            ResourceManager.GetString("Warning_UninstallingPackageWillBreakDependents", resourceCulture);

        public static string XdtError =>
            ResourceManager.GetString("XdtError", resourceCulture);
    }
}

