namespace Squirrel.Update
{
    using Microsoft.Win32;
    using Mono.Options;
    using NuGet;
    using Splat;
    using Squirrel;
    using Squirrel.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Program : IEnableLogger
    {
        private static OptionSet opts;
        private static int consoleCreated;

        [AsyncStateMachine(typeof(<CheckForUpdates>d__8))]
        public Task<string> CheckForUpdates(string updateUrl, string appName = null)
        {
            <CheckForUpdates>d__8 d__;
            d__.<>4__this = this;
            d__.updateUrl = updateUrl;
            d__.appName = appName;
            d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<CheckForUpdates>d__8>(ref d__);
            return d__.<>t__builder.Task;
        }

        public void ClearAllCompatibilityFlags(string forDir)
        {
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
            if (key != null)
            {
                string[] valueNames = key.GetValueNames();
                int index = 0;
                while (true)
                {
                    if (index >= valueNames.Length)
                    {
                        key.Close();
                        break;
                    }
                    string argument = valueNames[index];
                    if (argument.StartsWith(forDir))
                    {
                        this.Log<Program>().Info<string>("Clearing compatibility flags from {0}", argument);
                        key.DeleteValue(argument);
                    }
                    index++;
                }
            }
        }

        [AsyncStateMachine(typeof(<createMsiPackage>d__20))]
        private static Task createMsiPackage(string setupExe, IPackage package)
        {
            <createMsiPackage>d__20 d__;
            d__.setupExe = setupExe;
            d__.package = package;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<createMsiPackage>d__20>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<createSetupEmbeddedZip>d__17))]
        private Task<string> createSetupEmbeddedZip(string fullPackage, string releasesDir, string backgroundGif, string signingOpts)
        {
            <createSetupEmbeddedZip>d__17 d__;
            d__.<>4__this = this;
            d__.fullPackage = fullPackage;
            d__.backgroundGif = backgroundGif;
            d__.signingOpts = signingOpts;
            d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<createSetupEmbeddedZip>d__17>(ref d__);
            return d__.<>t__builder.Task;
        }

        public void Deshortcut(string exeName, string shortcutArgs)
        {
            if (string.IsNullOrWhiteSpace(exeName))
            {
                this.ShowHelp();
            }
            else
            {
                string applicationName = getAppNameFromDirectory(null);
                ShortcutLocation? nullable = parseShortcutLocations(shortcutArgs);
                using (UpdateManager manager = new UpdateManager("", applicationName, null, null))
                {
                    ShortcutLocation? nullable2 = nullable;
                    manager.RemoveShortcutsForExecutable(exeName, (nullable2 != null) ? nullable2.GetValueOrDefault() : ShortcutLocations.Defaults);
                }
            }
        }

        [AsyncStateMachine(typeof(<Download>d__9))]
        public Task<string> Download(string updateUrl, string appName = null)
        {
            <Download>d__9 d__;
            d__.<>4__this = this;
            d__.updateUrl = updateUrl;
            d__.appName = appName;
            d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<Download>d__9>(ref d__);
            return d__.<>t__builder.Task;
        }

        private static void ensureConsole()
        {
            if (Interlocked.CompareExchange(ref consoleCreated, 1, 0) != 1)
            {
                if (!NativeMethods.AttachConsole(-1))
                {
                    NativeMethods.AllocConsole();
                }
                NativeMethods.GetStdHandle(StandardHandles.STD_ERROR_HANDLE);
                NativeMethods.GetStdHandle(StandardHandles.STD_OUTPUT_HANDLE);
            }
        }

        private int executeCommandLine(string[] args)
        {
            CancellationTokenSource animatedGifWindowToken = new CancellationTokenSource();
            using (Disposable.Create(delegate {
                animatedGifWindowToken.Cancel()}))
            {
                int num;
                this.Log<Program>().Info("Starting Squirrel Updater: " + string.Join(" ", args));
                if (!Enumerable.Any<string>(args, x => x.StartsWith("/squirrel", StringComparison.OrdinalIgnoreCase)))
                {
                    bool silentInstall = false;
                    UpdateAction updateAction = UpdateAction.Unset;
                    string target = null;
                    string releaseDir = null;
                    string packagesDir = null;
                    string bootstrapperExe = null;
                    string backgroundGif = null;
                    string signingParameters = null;
                    string baseUrl = null;
                    string processStart = null;
                    string processStartArgs = null;
                    string setupIcon = null;
                    string icon = null;
                    string shortcutArgs = null;
                    bool shouldWait = false;
                    bool noMsi = false;
                    bool updateOnly = false;
                    OptionSet set1 = new OptionSet();
                    set1.Add("Usage: Squirrel.exe command [OPTS]");
                    set1.Add("Manages Squirrel packages");
                    set1.Add("");
                    set1.Add("Commands");
                    set1.Add("install=", "Install the app whose package is in the specified directory", delegate (string v) {
                        updateAction = UpdateAction.Install;
                        target = v;
                    });
                    set1.Add("check=", "Download the releases information specified by the URL and write new results to stdout as JSON. Does not download the actual packages", delegate (string v) {
                        updateAction = UpdateAction.CheckForUpdates;
                        target = v;
                    });
                    set1.Add("uninstall", "Uninstall the app the same dir as Update.exe", delegate (string v) {
                        updateAction = UpdateAction.Uninstall;
                    });
                    set1.Add("download=", "Download the releases specified by the URL and write new results to stdout as JSON", delegate (string v) {
                        updateAction = UpdateAction.Download;
                        target = v;
                    });
                    set1.Add("update=", "Update the application to the latest remote version specified by URL", delegate (string v) {
                        updateAction = UpdateAction.Update;
                        target = v;
                    });
                    set1.Add("releasify=", "Update or generate a releases directory with a given NuGet package", delegate (string v) {
                        updateAction = UpdateAction.Releasify;
                        target = v;
                    });
                    set1.Add("createShortcut=", "Create a shortcut for the given executable name", delegate (string v) {
                        updateAction = UpdateAction.Shortcut;
                        target = v;
                    });
                    set1.Add("removeShortcut=", "Remove a shortcut for the given executable name", delegate (string v) {
                        updateAction = UpdateAction.Deshortcut;
                        target = v;
                    });
                    set1.Add("updateSelf=", "Copy the currently executing Update.exe into the default location", delegate (string v) {
                        updateAction = UpdateAction.UpdateSelf;
                        target = v;
                    });
                    set1.Add("processStart=", "Start an executable in the latest version of the app package", delegate (string v) {
                        updateAction = UpdateAction.ProcessStart;
                        processStart = v;
                    }, true);
                    set1.Add("processStartAndWait=", "Start an executable in the latest version of the app package", delegate (string v) {
                        updateAction = UpdateAction.ProcessStart;
                        processStart = v;
                        shouldWait = true;
                    }, true);
                    set1.Add("");
                    set1.Add("Options:");
                    set1.Add("h|?|help", "Display Help and exit", delegate (string _) {
                    });
                    OptionSet local3 = set1;
                    local3.Add("r=|releaseDir=", "Path to a release directory to use with releasify", delegate (string v) {
                        releaseDir = v;
                    });
                    local3.Add("p=|packagesDir=", "Path to the NuGet Packages directory for C# apps", delegate (string v) {
                        packagesDir = v;
                    });
                    local3.Add("bootstrapperExe=", "Path to the Setup.exe to use as a template", delegate (string v) {
                        bootstrapperExe = v;
                    });
                    local3.Add("g=|loadingGif=", "Path to an animated GIF to be displayed during installation", delegate (string v) {
                        backgroundGif = v;
                    });
                    local3.Add("i=|icon", "Path to an ICO file that will be used for icon shortcuts", delegate (string v) {
                        icon = v;
                    });
                    local3.Add("setupIcon=", "Path to an ICO file that will be used for the Setup executable's icon", delegate (string v) {
                        setupIcon = v;
                    });
                    local3.Add("n=|signWithParams=", "Sign the installer via SignTool.exe with the parameters given", delegate (string v) {
                        signingParameters = v;
                    });
                    local3.Add("s|silent", "Silent install", delegate (string _) {
                        silentInstall = true;
                    });
                    local3.Add("b=|baseUrl=", "Provides a base URL to prefix the RELEASES file packages with", delegate (string v) {
                        baseUrl = v;
                    }, true);
                    local3.Add("a=|process-start-args=", "Arguments that will be used when starting executable", delegate (string v) {
                        processStartArgs = v;
                    }, true);
                    local3.Add("l=|shortcut-locations=", "Comma-separated string of shortcut locations, e.g. 'Desktop,StartMenu'", delegate (string v) {
                        shortcutArgs = v;
                    });
                    local3.Add("no-msi", "Don't generate an MSI package", delegate (string v) {
                        noMsi = true;
                    });
                    local3.Add("updateOnly", "For createShortcut, should we only update an existing link", delegate (string v) {
                        updateOnly = true;
                    });
                    opts = local3;
                    opts.Parse(args);
                    setupIcon = setupIcon ?? icon;
                    if (updateAction != UpdateAction.Unset)
                    {
                        switch (updateAction)
                        {
                            case UpdateAction.Install:
                            {
                                ProgressSource progressSource = new ProgressSource();
                                if (!silentInstall)
                                {
                                    AnimatedGifWindow.ShowWindow(TimeSpan.FromSeconds(4.0), animatedGifWindowToken.Token, progressSource);
                                }
                                this.Install(silentInstall, progressSource, Path.GetFullPath(target)).Wait();
                                animatedGifWindowToken.Cancel();
                                break;
                            }
                            case UpdateAction.Uninstall:
                                this.Uninstall(null).Wait();
                                break;

                            case UpdateAction.Download:
                                Console.WriteLine(this.Download(target, null).Result);
                                break;

                            case UpdateAction.Update:
                                this.Update(target, null).Wait();
                                break;

                            case UpdateAction.Releasify:
                                this.Releasify(target, releaseDir, packagesDir, bootstrapperExe, backgroundGif, signingParameters, baseUrl, setupIcon, !noMsi);
                                break;

                            case UpdateAction.Shortcut:
                                this.Shortcut(target, shortcutArgs, processStartArgs, setupIcon, updateOnly);
                                break;

                            case UpdateAction.Deshortcut:
                                this.Deshortcut(target, shortcutArgs);
                                break;

                            case UpdateAction.ProcessStart:
                                this.ProcessStart(processStart, processStartArgs, shouldWait);
                                break;

                            case UpdateAction.UpdateSelf:
                                this.UpdateSelf().Wait();
                                break;

                            case UpdateAction.CheckForUpdates:
                                Console.WriteLine(this.CheckForUpdates(target, null).Result);
                                break;

                            default:
                                break;
                        }
                        goto TR_0006;
                    }
                    else
                    {
                        this.ShowHelp();
                        num = -1;
                    }
                }
                else
                {
                    num = 0;
                }
                return num;
            }
        TR_0006:
            return 0;
        }

        private static string getAppNameFromDirectory(string path = null)
        {
            path = path ?? Path.GetDirectoryName(Utility.GetAssembyLocation());
            return new DirectoryInfo(path).Name;
        }

        [AsyncStateMachine(typeof(<Install>d__5))]
        public Task Install(bool silentInstall, ProgressSource progressSource, string sourceDirectory = null)
        {
            <Install>d__5 d__;
            d__.<>4__this = this;
            d__.silentInstall = silentInstall;
            d__.progressSource = progressSource;
            d__.sourceDirectory = sourceDirectory;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<Install>d__5>(ref d__);
            return d__.<>t__builder.Task;
        }

        private int main(string[] args)
        {
            int num;
            Console.WriteLine("Starting Update.exe");
            SetupLogLogger logger1 = new SetupLogLogger(Enumerable.Any<string>(args, x => x.Contains("uninstall")));
            logger1.Level = LogLevel.Info;
            using (SetupLogLogger logger = logger1)
            {
                Locator.CurrentMutable.Register(() => logger, typeof(ILogger), null);
                try
                {
                    num = this.executeCommandLine(args);
                }
                catch (Exception exception)
                {
                    logger.Write("Unhandled exception: " + exception, LogLevel.Fatal);
                    throw;
                }
            }
            return num;
        }

        public static int Main(string[] args)
        {
            Program program = new Program();
            try
            {
                return program.main(args);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return -1;
            }
        }

        private static ShortcutLocation? parseShortcutLocations(string shortcutArgs)
        {
            ShortcutLocation? nullable = null;
            if (!string.IsNullOrWhiteSpace(shortcutArgs))
            {
                char[] separator = new char[] { ',' };
                foreach (string str in shortcutArgs.Split(separator))
                {
                    ShortcutLocation location = (ShortcutLocation) Enum.Parse(typeof(ShortcutLocation), str, false);
                    if (nullable == null)
                    {
                        nullable = new ShortcutLocation?(location);
                    }
                    else
                    {
                        ShortcutLocation? nullable1;
                        ShortcutLocation? nullable2 = nullable;
                        ShortcutLocation location2 = location;
                        if (nullable2 != null)
                        {
                            nullable1 = new ShortcutLocation?(((ShortcutLocation) nullable2.GetValueOrDefault()) | location2);
                        }
                        else
                        {
                            nullable1 = null;
                        }
                        nullable = nullable1;
                    }
                }
            }
            return nullable;
        }

        private static string pathToWixTools()
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(Path.Combine(directoryName, "candle.exe")))
            {
                return directoryName;
            }
            string[] paths = new string[] { directoryName, "..", "..", "..", "vendor", "wix", "candle.exe" };
            string path = Path.Combine(paths);
            if (!File.Exists(path))
            {
                throw new Exception("WiX tools can't be found");
            }
            return Path.GetFullPath(path);
        }

        public void ProcessStart(string exeName, string arguments, bool shouldWait)
        {
            if (string.IsNullOrWhiteSpace(exeName))
            {
                this.ShowHelp();
            }
            else
            {
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string str = Enumerable.FirstOrDefault<string>(from x in ReleaseEntry.ParseReleaseFile(File.ReadAllText(Utility.LocalReleaseFileForAppDir(appDir), Encoding.UTF8))
                    orderby x.Version descending
                    select new string[] { Utility.AppDirForRelease(appDir, x), Utility.AppDirForVersion(appDir, new SemanticVersion(x.Version.Version.Major, x.Version.Version.Minor, x.Version.Version.Build, "")) }, x => Directory.Exists(x));
                FileInfo info = new FileInfo(Path.Combine(str, exeName));
                if (!info.FullName.StartsWith(str, StringComparison.Ordinal))
                {
                    this.Log<Program>().Error<FileInfo, string>("Want to launch '{0}', but it is not in {1}", info, str);
                    throw new ArgumentException();
                }
                if (!info.Exists)
                {
                    this.Log<Program>().Error<FileInfo>("File {0} doesn't exist in current release", info);
                    throw new ArgumentException();
                }
                if (shouldWait)
                {
                    this.waitForParentToExit();
                }
                try
                {
                    this.Log<Program>().Info<string, string>("About to launch: '{0}': {1}", info.FullName, arguments ?? "");
                    ProcessStartInfo startInfo = new ProcessStartInfo(info.FullName, arguments ?? "");
                    startInfo.WorkingDirectory = Path.GetDirectoryName(info.FullName);
                    Process.Start(startInfo);
                }
                catch (Exception exception)
                {
                    this.Log<Program>().ErrorException("Failed to start process", exception);
                }
            }
        }

        public void Releasify(string package, string targetDir = null, string packagesDir = null, string bootstrapperExe = null, string backgroundGif = null, string signingOpts = null, string baseUrl = null, string setupIcon = null, bool generateMsi = true)
        {
            if (baseUrl != null)
            {
                if (!Utility.IsHttpUrl(baseUrl))
                {
                    throw new Exception($"Invalid --baseUrl '{baseUrl}'. A base URL must start with http or https and be a valid URI.");
                }
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl = baseUrl + "/";
                }
            }
            targetDir = targetDir ?? @".\Releases";
            packagesDir = packagesDir ?? ".";
            bootstrapperExe = bootstrapperExe ?? @".\Setup.exe";
            char[] separator = new char[] { '.' };
            string str = Path.GetFileNameWithoutExtension(package).Split(separator)[0];
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            if (!File.Exists(bootstrapperExe))
            {
                bootstrapperExe = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Setup.exe");
            }
            this.Log<Program>().Info("Bootstrapper EXE found at:" + bootstrapperExe);
            DirectoryInfo info = new DirectoryInfo(targetDir);
            File.Copy(package, Path.Combine(info.FullName, Path.GetFileName(package)), true);
            IEnumerable<FileInfo> enumerable = from x in info.EnumerateFiles()
                where x.Name.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase)
                where !x.Name.Contains("-delta") && !x.Name.Contains("-full")
                select x;
            List<string> list = new List<string>();
            string path = Path.Combine(info.FullName, "RELEASES");
            List<ReleaseEntry> releaseEntries = new List<ReleaseEntry>();
            if (File.Exists(path))
            {
                releaseEntries.AddRange(ReleaseEntry.ParseReleaseFile(File.ReadAllText(path, Encoding.UTF8)));
            }
            foreach (FileInfo info2 in enumerable)
            {
                Action<string> <>9__2;
                this.Log<Program>().Info("Creating release package: " + info2.FullName);
                ReleasePackage package2 = new ReleasePackage(info2.FullName, false);
                Action<string> contentsPostProcessHook = <>9__2;
                if (<>9__2 == null)
                {
                    Action<string> local6 = <>9__2;
                    contentsPostProcessHook = <>9__2 = delegate (string pkgPath) {
                        if (signingOpts != null)
                        {
                            Func<FileInfo, Task> <>9__4;
                            IEnumerable<FileInfo> enumerable1 = Enumerable.Where<FileInfo>(new DirectoryInfo(pkgPath).GetAllFilesRecursively(), <>c.<>9__11_3 ?? (<>c.<>9__11_3 = x => x.Name.ToLowerInvariant().EndsWith(".exe")));
                            if (<>9__4 == null)
                            {
                                IEnumerable<FileInfo> local2 = Enumerable.Where<FileInfo>(new DirectoryInfo(pkgPath).GetAllFilesRecursively(), <>c.<>9__11_3 ?? (<>c.<>9__11_3 = x => x.Name.ToLowerInvariant().EndsWith(".exe")));
                                Func<FileInfo, Task> func1 = <>9__4 = x => signPEFile(x.FullName, signingOpts);
                                enumerable1 = (IEnumerable<FileInfo>) func1;
                            }
                            ((IEnumerable<FileInfo>) <>9__4).ForEachAsync<FileInfo>(((Func<FileInfo, Task>) enumerable1), 4).Wait();
                        }
                    };
                }
                package2.CreateReleasePackage(Path.Combine(info.FullName, package2.SuggestedReleaseFileName), packagesDir, null, contentsPostProcessHook);
                list.Add(package2.ReleasePackageFile);
                ReleasePackage basePackage = ReleaseEntry.GetPreviousRelease(releaseEntries, package2, targetDir);
                if (basePackage != null)
                {
                    list.Insert(0, new DeltaPackageBuilder(null).CreateDeltaPackage(basePackage, package2, Path.Combine(info.FullName, package2.SuggestedReleaseFileName.Replace("full", "delta"))).InputPackageFile);
                }
            }
            using (IEnumerator<FileInfo> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    File.Delete(enumerator.Current.FullName);
                }
            }
            List<ReleaseEntry> newReleaseEntries = (from packageFilename in list select ReleaseEntry.GenerateFromFile(packageFilename, baseUrl)).ToList<ReleaseEntry>();
            List<ReleaseEntry> list1 = (from x in releaseEntries
                where !(from e in newReleaseEntries select e.Version).Contains<SemanticVersion>(x.Version)
                select x).Concat<ReleaseEntry>(newReleaseEntries).ToList<ReleaseEntry>();
            ReleaseEntry.WriteReleaseFile(list1, path);
            string targetSetupExe = Path.Combine(info.FullName, str + "Setup.exe");
            ReleaseEntry entry = (from x in list1.MaxBy<ReleaseEntry, SemanticVersion>(x => x.Version)
                where !x.IsDelta
                select x).First<ReleaseEntry>();
            File.Copy(bootstrapperExe, targetSetupExe, true);
            string result = this.createSetupEmbeddedZip(Path.Combine(info.FullName, entry.Filename), info.FullName, backgroundGif, signingOpts).Result;
            try
            {
                byte[] pData = File.ReadAllBytes(result);
                IntPtr ptr1 = NativeMethods.BeginUpdateResource(targetSetupExe, false);
                if (NativeMethods.BeginUpdateResource(targetSetupExe, false) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                IntPtr handle = ptr1;
                if (!NativeMethods.UpdateResource(handle, "DATA", new IntPtr(0x83), 0x409, pData, pData.Length))
                {
                    throw new Win32Exception();
                }
                if (!NativeMethods.EndUpdateResource(handle, false))
                {
                    throw new Win32Exception();
                }
            }
            catch (Exception exception)
            {
                this.Log<Program>().ErrorException("Failed to update Setup.exe with new Zip file", exception);
            }
            finally
            {
                File.Delete(result);
            }
            () => setPEVersionInfoAndIcon(targetSetupExe, new ZipPackage(package), setupIcon).Wait().Retry(2);
            if (signingOpts != null)
            {
                signPEFile(targetSetupExe, signingOpts).Wait();
            }
            if (generateMsi)
            {
                createMsiPackage(targetSetupExe, new ZipPackage(package)).Wait();
            }
        }

        [AsyncStateMachine(typeof(<setPEVersionInfoAndIcon>d__19))]
        private static Task setPEVersionInfoAndIcon(string exePath, IPackage package, string iconPath = null)
        {
            <setPEVersionInfoAndIcon>d__19 d__;
            d__.exePath = exePath;
            d__.package = package;
            d__.iconPath = iconPath;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<setPEVersionInfoAndIcon>d__19>(ref d__);
            return d__.<>t__builder.Task;
        }

        public void Shortcut(string exeName, string shortcutArgs, string processStartArgs, string icon, bool updateOnly)
        {
            if (string.IsNullOrWhiteSpace(exeName))
            {
                this.ShowHelp();
            }
            else
            {
                string applicationName = getAppNameFromDirectory(null);
                ShortcutLocation? nullable = parseShortcutLocations(shortcutArgs);
                using (UpdateManager manager = new UpdateManager("", applicationName, null, null))
                {
                    ShortcutLocation? nullable2 = nullable;
                    manager.CreateShortcutsForExecutable(exeName, (nullable2 != null) ? nullable2.GetValueOrDefault() : ShortcutLocations.Defaults, updateOnly, processStartArgs, icon);
                }
            }
        }

        public void ShowHelp()
        {
            ensureConsole();
            opts.WriteOptionDescriptions(Console.Out);
        }

        [AsyncStateMachine(typeof(<signPEFile>d__18))]
        private static Task signPEFile(string exePath, string signingOpts)
        {
            <signPEFile>d__18 d__;
            d__.exePath = exePath;
            d__.signingOpts = signingOpts;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<signPEFile>d__18>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<Uninstall>d__10))]
        public Task Uninstall(string appName = null)
        {
            <Uninstall>d__10 d__;
            d__.<>4__this = this;
            d__.appName = appName;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<Uninstall>d__10>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<Update>d__6))]
        public Task Update(string updateUrl, string appName = null)
        {
            <Update>d__6 d__;
            d__.<>4__this = this;
            d__.updateUrl = updateUrl;
            d__.appName = appName;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<Update>d__6>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<UpdateSelf>d__7))]
        public Task UpdateSelf()
        {
            <UpdateSelf>d__7 d__;
            d__.<>4__this = this;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<UpdateSelf>d__7>(ref d__);
            return d__.<>t__builder.Task;
        }

        private void waitForParentToExit()
        {
            int parentProcessId = NativeMethods.GetParentProcessId();
            IntPtr hHandle = new IntPtr();
            try
            {
                hHandle = NativeMethods.OpenProcess(ProcessAccess.Synchronize, false, parentProcessId);
                if (!(hHandle != IntPtr.Zero))
                {
                    this.Log<Program>().Info<int>("Parent PID {0} no longer valid - ignoring", parentProcessId);
                }
                else
                {
                    this.Log<Program>().Info<int>("About to wait for parent PID {0}", parentProcessId);
                    NativeMethods.WaitForSingleObject(hHandle, uint.MaxValue);
                    this.Log<Program>().Info<int>("parent PID {0} exited", parentProcessId);
                }
            }
            finally
            {
                if (hHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(hHandle);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Program.<>c <>9 = new Program.<>c();
            public static Func<string, bool> <>9__2_0;
            public static Func<string, bool> <>9__4_1;
            public static Action<string> <>9__4_13;
            public static Func<FileInfo, bool> <>9__5_0;
            public static Func<FileInfo, ReleaseEntry> <>9__5_1;
            public static Action<int> <>9__6_0;
            public static Action<int> <>9__6_1;
            public static Action<int> <>9__6_2;
            public static Action<int> <>9__8_0;
            public static Func<ReleaseEntry, <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>> <>9__8_1;
            public static Action<int> <>9__9_0;
            public static Action<int> <>9__9_1;
            public static Func<FileInfo, bool> <>9__11_0;
            public static Func<FileInfo, bool> <>9__11_1;
            public static Func<FileInfo, bool> <>9__11_3;
            public static Func<ReleaseEntry, SemanticVersion> <>9__11_7;
            public static Func<ReleaseEntry, SemanticVersion> <>9__11_8;
            public static Func<ReleaseEntry, bool> <>9__11_9;
            public static Func<ReleaseEntry, SemanticVersion> <>9__14_0;
            public static Func<string, bool> <>9__14_2;
            public static Func<FileInfo, bool> <>9__17_2;
            public static Func<FileInfo, string> <>9__17_3;
            public static Func<StringBuilder, KeyValuePair<string, string>, StringBuilder> <>9__19_0;
            public static Action<string> <>9__20_0;

            internal void <CheckForUpdates>b__8_0(int x)
            {
                Console.WriteLine((int) (x / 3));
            }

            internal <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string> <CheckForUpdates>b__8_1(ReleaseEntry x) => 
                new <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>(x.Version.ToString(), "");

            internal void <createMsiPackage>b__20_0(string x)
            {
                Utility.DeleteFileHarder(x, false);
            }

            internal bool <createSetupEmbeddedZip>b__17_2(FileInfo x) => 
                x.Name.ToLowerInvariant().EndsWith(".exe");

            internal string <createSetupEmbeddedZip>b__17_3(FileInfo x) => 
                x.FullName;

            internal void <Download>b__9_0(int x)
            {
                Console.WriteLine((int) (x / 3));
            }

            internal void <Download>b__9_1(int x)
            {
                Console.WriteLine((int) (0x21 + (x / 3)));
            }

            internal bool <executeCommandLine>b__4_1(string x) => 
                x.StartsWith("/squirrel", StringComparison.OrdinalIgnoreCase);

            internal void <executeCommandLine>b__4_13(string _)
            {
            }

            internal bool <Install>b__5_0(FileInfo x) => 
                x.Name.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase);

            internal ReleaseEntry <Install>b__5_1(FileInfo x) => 
                ReleaseEntry.GenerateFromFile(x.FullName, null);

            internal bool <main>b__2_0(string x) => 
                x.Contains("uninstall");

            internal SemanticVersion <ProcessStart>b__14_0(ReleaseEntry x) => 
                x.Version;

            internal bool <ProcessStart>b__14_2(string x) => 
                Directory.Exists(x);

            internal bool <Releasify>b__11_0(FileInfo x) => 
                x.Name.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase);

            internal bool <Releasify>b__11_1(FileInfo x) => 
                (!x.Name.Contains("-delta") && !x.Name.Contains("-full"));

            internal bool <Releasify>b__11_3(FileInfo x) => 
                x.Name.ToLowerInvariant().EndsWith(".exe");

            internal SemanticVersion <Releasify>b__11_7(ReleaseEntry e) => 
                e.Version;

            internal SemanticVersion <Releasify>b__11_8(ReleaseEntry x) => 
                x.Version;

            internal bool <Releasify>b__11_9(ReleaseEntry x) => 
                !x.IsDelta;

            internal StringBuilder <setPEVersionInfoAndIcon>b__19_0(StringBuilder acc, KeyValuePair<string, string> x)
            {
                acc.AppendFormat(" --set-version-string \"{0}\" \"{1}\"", x.Key, x.Value);
                return acc;
            }

            internal void <Update>b__6_0(int x)
            {
                Console.WriteLine((int) (x / 3));
            }

            internal void <Update>b__6_1(int x)
            {
                Console.WriteLine((int) (0x21 + (x / 3)));
            }

            internal void <Update>b__6_2(int x)
            {
                Console.WriteLine((int) (0x42 + (x / 3)));
            }
        }

        [CompilerGenerated]
        private struct <CheckForUpdates>d__8 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<string> <>t__builder;
            public string appName;
            public Program <>4__this;
            public string updateUrl;
            private UpdateManager <mgr>5__1;
            private TaskAwaiter<UpdateInfo> <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num != 0)
                    {
                        this.appName = this.appName ?? Program.getAppNameFromDirectory(null);
                        this.<>4__this.Log<Program>().Info("Fetching update information, downloading from " + this.updateUrl);
                        this.<mgr>5__1 = new UpdateManager(this.updateUrl, this.appName, null, null);
                    }
                    try
                    {
                        TaskAwaiter<UpdateInfo> awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<UpdateInfo>();
                            this.<>1__state = num = -1;
                            goto TR_0007;
                        }
                        else
                        {
                            awaiter = this.<mgr>5__1.CheckForUpdate(false, Program.<>c.<>9__8_0 ?? (Program.<>c.<>9__8_0 = new Action<int>(this.<CheckForUpdates>b__8_0))).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0007;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<UpdateInfo>, Program.<CheckForUpdates>d__8>(ref awaiter, ref this);
                            }
                        }
                        return;
                    TR_0007:
                        awaiter = new TaskAwaiter<UpdateInfo>();
                        UpdateInfo result = awaiter.GetResult();
                        string str = SimpleJson.SerializeObject(new { 
                            currentVersion = (result.CurrentlyInstalledVersion != null) ? result.CurrentlyInstalledVersion.Version.ToString() : "",
                            futureVersion = result.FutureReleaseEntry.Version.ToString(),
                            releasesToApply = Enumerable.Select<ReleaseEntry, <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>>(result.ReleasesToApply, Program.<>c.<>9__8_1 ?? (Program.<>c.<>9__8_1 = new Func<ReleaseEntry, <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>>(this.<CheckForUpdates>b__8_1))).ToArray<<8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>>()
                        });
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(str);
                    }
                    finally
                    {
                        if ((num < 0) && (this.<mgr>5__1 != null))
                        {
                            this.<mgr>5__1.Dispose();
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <createMsiPackage>d__20 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string setupExe;
            public IPackage package;
            private string <candleParams>5__1;
            private string <wxsTarget>5__2;
            private string <pathToWix>5__3;
            private string <lightParams>5__4;
            private TaskAwaiter<Tuple<int, string>> <>u__1;
            private TaskAwaiter <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<Tuple<int, string>> awaiter;
                    TaskAwaiter awaiter2;
                    switch (num)
                    {
                        case 0:
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<Tuple<int, string>>();
                            this.<>1__state = num = -1;
                            break;

                        case 1:
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<Tuple<int, string>>();
                            this.<>1__state = num = -1;
                            goto TR_000B;

                        case 2:
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0008;

                        default:
                        {
                            this.<pathToWix>5__3 = Program.pathToWixTools();
                            string directoryName = Path.GetDirectoryName(this.setupExe);
                            string str2 = string.Join(",", this.package.Authors);
                            Dictionary<string, string> identifiers = new Dictionary<string, string>();
                            identifiers.Add("Id", this.package.Id);
                            identifiers.Add("Title", this.package.Title);
                            identifiers.Add("Author", str2);
                            identifiers.Add("Summary", this.package.Summary ?? (this.package.Description ?? this.package.Id));
                            string contents = CopStache.Render(File.ReadAllText(Path.Combine(this.<pathToWix>5__3, "template.wxs")), identifiers);
                            this.<wxsTarget>5__2 = Path.Combine(directoryName, "Setup.wxs");
                            File.WriteAllText(this.<wxsTarget>5__2, contents, Encoding.UTF8);
                            this.<candleParams>5__1 = $"-nologo -ext WixNetFxExtension -out "{this.<wxsTarget>5__2.Replace(".wxs", ".wixobj")}" "{this.<wxsTarget>5__2}"";
                            awaiter = Utility.InvokeProcessAsync(Path.Combine(this.<pathToWix>5__3, "candle.exe"), this.<candleParams>5__1, CancellationToken.None).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                break;
                            }
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<Tuple<int, string>>, Program.<createMsiPackage>d__20>(ref awaiter, ref this);
                            return;
                        }
                    }
                    Tuple<int, string> result = new TaskAwaiter<Tuple<int, string>>().GetResult();
                    if (result.Item1 != 0)
                    {
                        throw new Exception($"Failed to compile WiX template, command invoked was: '{"candle.exe"} {this.<candleParams>5__1}'

Output was:
{result.Item2}");
                    }
                    this.<lightParams>5__4 = $"-ext WixNetFxExtension -sval -out "{this.<wxsTarget>5__2.Replace(".wxs", ".msi")}" "{this.<wxsTarget>5__2.Replace(".wxs", ".wixobj")}"";
                    awaiter = Utility.InvokeProcessAsync(Path.Combine(this.<pathToWix>5__3, "light.exe"), this.<lightParams>5__4, CancellationToken.None).GetAwaiter();
                    if (!awaiter.IsCompleted)
                    {
                        this.<>1__state = num = 1;
                        this.<>u__1 = awaiter;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<Tuple<int, string>>, Program.<createMsiPackage>d__20>(ref awaiter, ref this);
                        return;
                    }
                    goto TR_000B;
                TR_0008:
                    awaiter2.GetResult();
                    awaiter2 = new TaskAwaiter();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                    return;
                TR_000B:
                    awaiter = new TaskAwaiter<Tuple<int, string>>();
                    result = awaiter.GetResult();
                    if (result.Item1 != 0)
                    {
                        throw new Exception($"Failed to link WiX template, command invoked was: '{"light.exe"} {this.<lightParams>5__4}'

Output was:
{result.Item2}");
                    }
                    string[] source = new string[] { this.<wxsTarget>5__2, this.<wxsTarget>5__2.Replace(".wxs", ".wixobj"), this.<wxsTarget>5__2.Replace(".wxs", ".wixpdb") };
                    awaiter2 = source.ForEachAsync<string>((Program.<>c.<>9__20_0 ?? (Program.<>c.<>9__20_0 = new Action<string>(this.<createMsiPackage>b__20_0))), 4).GetAwaiter();
                    if (awaiter2.IsCompleted)
                    {
                        goto TR_0008;
                    }
                    else
                    {
                        this.<>1__state = num = 2;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<createMsiPackage>d__20>(ref awaiter2, ref this);
                    }
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <createSetupEmbeddedZip>d__17 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<string> <>t__builder;
            public string fullPackage;
            public string backgroundGif;
            public string signingOpts;
            public Program <>4__this;
            private Program.<>c__DisplayClass17_1 <>8__1;
            private IDisposable <>7__wrap1;
            private TaskAwaiter <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num != 0)
                    {
                        string tempPath;
                        string fullPackage = this.fullPackage;
                        string backgroundGif = this.backgroundGif;
                        string signingOpts = this.signingOpts;
                        this.<>4__this.Log<Program>().Info("Building embedded zip file for Setup.exe");
                        this.<>7__wrap1 = Utility.WithTempDirectory(out tempPath, null);
                    }
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0008;
                        }
                        else
                        {
                            Program.<>c__DisplayClass17_0 class_;
                            this.<>8__1 = new Program.<>c__DisplayClass17_1();
                            this.<>8__1.CS$<>8__locals1 = class_;
                            this.<>4__this.ErrorIfThrows(new Action(this.<>8__1.CS$<>8__locals1.<createSetupEmbeddedZip>b__0), "Failed to write package files to temp dir: " + this.<>8__1.CS$<>8__locals1.tempPath);
                            if (!string.IsNullOrWhiteSpace(this.<>8__1.CS$<>8__locals1.backgroundGif))
                            {
                                this.<>4__this.ErrorIfThrows(new Action(this.<>8__1.CS$<>8__locals1.<createSetupEmbeddedZip>b__1), "Failed to write animated GIF to temp dir: " + this.<>8__1.CS$<>8__locals1.tempPath);
                            }
                            ReleaseEntry[] releaseEntries = new ReleaseEntry[] { ReleaseEntry.GenerateFromFile(this.<>8__1.CS$<>8__locals1.fullPackage, null) };
                            ReleaseEntry.WriteReleaseFile(releaseEntries, Path.Combine(this.<>8__1.CS$<>8__locals1.tempPath, "RELEASES"));
                            this.<>8__1.target = Path.GetTempFileName();
                            File.Delete(this.<>8__1.target);
                            if (this.<>8__1.CS$<>8__locals1.signingOpts == null)
                            {
                                goto TR_0007;
                            }
                            else
                            {
                                awaiter = Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(new DirectoryInfo(this.<>8__1.CS$<>8__locals1.tempPath).EnumerateFiles(), Program.<>c.<>9__17_2 ?? (Program.<>c.<>9__17_2 = new Func<FileInfo, bool>(this.<createSetupEmbeddedZip>b__17_2))), Program.<>c.<>9__17_3 ?? (Program.<>c.<>9__17_3 = new Func<FileInfo, string>(this.<createSetupEmbeddedZip>b__17_3))).ForEachAsync<string>(new Func<string, Task>(this.<>8__1.CS$<>8__locals1.<createSetupEmbeddedZip>b__4), 4).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0008;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<createSetupEmbeddedZip>d__17>(ref awaiter, ref this);
                                }
                            }
                        }
                        return;
                    TR_0007:
                        this.<>4__this.ErrorIfThrows(new Action(this.<>8__1.<createSetupEmbeddedZip>b__5), "Failed to create Zip file from directory: " + this.<>8__1.CS$<>8__locals1.tempPath);
                        string target = this.<>8__1.target;
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(target);
                        return;
                    TR_0008:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0007;
                    }
                    finally
                    {
                        if ((num < 0) && (this.<>7__wrap1 != null))
                        {
                            this.<>7__wrap1.Dispose();
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <Download>d__9 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<string> <>t__builder;
            public string appName;
            public Program <>4__this;
            public string updateUrl;
            private UpdateManager <mgr>5__1;
            private Program.<>c__DisplayClass9_0 <>8__2;
            private UpdateInfo <updateInfo>5__3;
            private TaskAwaiter<UpdateInfo> <>u__1;
            private TaskAwaiter <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if ((num != 0) && (num != 1))
                    {
                        this.appName = this.appName ?? Program.getAppNameFromDirectory(null);
                        this.<>4__this.Log<Program>().Info("Fetching update information, downloading from " + this.updateUrl);
                        this.<mgr>5__1 = new UpdateManager(this.updateUrl, this.appName, null, null);
                    }
                    try
                    {
                        TaskAwaiter<UpdateInfo> awaiter;
                        TaskAwaiter awaiter2;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<UpdateInfo>();
                            this.<>1__state = num = -1;
                        }
                        else if (num == 1)
                        {
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0008;
                        }
                        else
                        {
                            this.<>8__2 = new Program.<>c__DisplayClass9_0();
                            awaiter = this.<mgr>5__1.CheckForUpdate(false, Program.<>c.<>9__9_0 ?? (Program.<>c.<>9__9_0 = new Action<int>(this.<Download>b__9_0))).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<UpdateInfo>, Program.<Download>d__9>(ref awaiter, ref this);
                                return;
                            }
                        }
                        UpdateInfo result = new TaskAwaiter<UpdateInfo>().GetResult();
                        this.<updateInfo>5__3 = result;
                        awaiter2 = this.<mgr>5__1.DownloadReleases(this.<updateInfo>5__3.ReleasesToApply, Program.<>c.<>9__9_1 ?? (Program.<>c.<>9__9_1 = new Action<int>(this.<Download>b__9_1))).GetAwaiter();
                        if (awaiter2.IsCompleted)
                        {
                            goto TR_0008;
                        }
                        else
                        {
                            this.<>1__state = num = 1;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<Download>d__9>(ref awaiter2, ref this);
                        }
                        return;
                    TR_0008:
                        awaiter2.GetResult();
                        awaiter2 = new TaskAwaiter();
                        this.<>8__2.releaseNotes = this.<updateInfo>5__3.FetchReleaseNotes();
                        string str = SimpleJson.SerializeObject(new { 
                            currentVersion = this.<updateInfo>5__3.CurrentlyInstalledVersion.Version.ToString(),
                            futureVersion = this.<updateInfo>5__3.FutureReleaseEntry.Version.ToString(),
                            releasesToApply = Enumerable.Select<ReleaseEntry, <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>>(this.<updateInfo>5__3.ReleasesToApply, new Func<ReleaseEntry, <8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>>(this.<>8__2.<Download>b__2)).ToArray<<8c55c000-da1e-4a28-babd-5661a416da4a><>f__AnonymousType1<string, string>>()
                        });
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(str);
                    }
                    finally
                    {
                        if ((num < 0) && (this.<mgr>5__1 != null))
                        {
                            this.<mgr>5__1.Dispose();
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <Install>d__5 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string sourceDirectory;
            public Program <>4__this;
            private Program.<>c__DisplayClass5_0 <>8__1;
            private Program.<>c__DisplayClass5_1 <>8__2;
            public bool silentInstall;
            public ProgressSource progressSource;
            private TaskAwaiter <>u__1;
            private TaskAwaiter<RegistryKey> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    switch (num)
                    {
                        case 0:
                        case 1:
                        case 2:
                            break;

                        default:
                        {
                            this.sourceDirectory = this.sourceDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            string path = Path.Combine(this.sourceDirectory, "RELEASES");
                            this.<>4__this.Log<Program>().Info<string>("Starting install, writing to {0}", this.sourceDirectory);
                            if (!File.Exists(path))
                            {
                                this.<>4__this.Log<Program>().Info("RELEASES doesn't exist, creating it at " + path);
                                ReleaseEntry.WriteReleaseFile(Enumerable.Select<FileInfo, ReleaseEntry>(Enumerable.Where<FileInfo>(new DirectoryInfo(this.sourceDirectory).GetFiles(), Program.<>c.<>9__5_0 ?? (Program.<>c.<>9__5_0 = new Func<FileInfo, bool>(this.<Install>b__5_0))), Program.<>c.<>9__5_1 ?? (Program.<>c.<>9__5_1 = new Func<FileInfo, ReleaseEntry>(this.<Install>b__5_1))), path);
                            }
                            string packageName = ReleaseEntry.ParseReleaseFile(File.ReadAllText(path, Encoding.UTF8)).First<ReleaseEntry>().PackageName;
                            this.<>8__1 = new Program.<>c__DisplayClass5_0();
                            this.<>8__1.mgr = new UpdateManager(this.sourceDirectory, packageName, null, null);
                            break;
                        }
                    }
                    try
                    {
                        TaskAwaiter awaiter;
                        TaskAwaiter<RegistryKey> awaiter2;
                        switch (num)
                        {
                            case 0:
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_000F;

                            case 1:
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_000B;

                            case 2:
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter<RegistryKey>();
                                this.<>1__state = num = -1;
                                goto TR_000A;

                            default:
                                this.<>8__2 = new Program.<>c__DisplayClass5_1();
                                this.<>4__this.Log<Program>().Info("About to install to: " + this.<>8__1.mgr.RootAppDirectory);
                                this.<>4__this.ClearAllCompatibilityFlags(this.<>8__1.mgr.RootAppDirectory);
                                if (!Directory.Exists(this.<>8__1.mgr.RootAppDirectory))
                                {
                                    break;
                                }
                                this.<>4__this.Log<Program>().Warn<string>("Install path {0} already exists, burning it to the ground", this.<>8__1.mgr.RootAppDirectory);
                                awaiter = this.<>4__this.ErrorIfThrows(new Func<Task>(this.<>8__1.<Install>b__2), "Failed to remove existing directory on full install, is the app still running???").GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_000F;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<Install>d__5>(ref awaiter, ref this);
                                }
                                return;
                        }
                        goto TR_000D;
                    TR_000A:
                        awaiter2.GetResult();
                        awaiter2 = new TaskAwaiter<RegistryKey>();
                        this.<>8__2 = null;
                        goto TR_0009;
                    TR_000B:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        awaiter2 = this.<>4__this.ErrorIfThrows<RegistryKey>(new Func<Task<RegistryKey>>(this.<>8__1.<Install>b__6), "Failed to create uninstaller registry entry").GetAwaiter();
                        if (awaiter2.IsCompleted)
                        {
                            goto TR_000A;
                        }
                        else
                        {
                            this.<>1__state = num = 2;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<RegistryKey>, Program.<Install>d__5>(ref awaiter2, ref this);
                        }
                        return;
                    TR_000D:
                        Directory.CreateDirectory(this.<>8__1.mgr.RootAppDirectory);
                        this.<>8__2.updateTarget = Path.Combine(this.<>8__1.mgr.RootAppDirectory, "Update.exe");
                        this.<>4__this.ErrorIfThrows(new Action(this.<>8__2.<Install>b__5), "Failed to copy Update.exe to " + this.<>8__2.updateTarget);
                        awaiter = this.<>8__1.mgr.FullInstall(this.silentInstall, new Action<int>(this.progressSource.Raise)).GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.<>1__state = num = 1;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<Install>d__5>(ref awaiter, ref this);
                            return;
                        }
                        goto TR_000B;
                    TR_000F:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        this.<>4__this.ErrorIfThrows(new Action(this.<>8__1.<Install>b__3), "Couldn't recreate app directory, perhaps Antivirus is blocking it");
                        goto TR_000D;
                    }
                    finally
                    {
                        if ((num < 0) && (this.<>8__1.mgr != null))
                        {
                            this.<>8__1.mgr.Dispose();
                        }
                    }
                    return;
                TR_0009:
                    this.<>8__1 = null;
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <setPEVersionInfoAndIcon>d__19 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public IPackage package;
            public string exePath;
            public string iconPath;
            private string <exe>5__1;
            private StringBuilder <args>5__2;
            private TaskAwaiter<Tuple<int, string>> <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<Tuple<int, string>> awaiter;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter<Tuple<int, string>>();
                        this.<>1__state = num = -1;
                        goto TR_0006;
                    }
                    else
                    {
                        string str = string.Join(",", this.package.Authors);
                        Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
                        dictionary1.Add("CompanyName", str);
                        dictionary1.Add("LegalCopyright", this.package.Copyright ?? ("Copyright \x00a9 " + DateTime.Now.Year.ToString() + " " + str));
                        Dictionary<string, string> local9 = dictionary1;
                        Dictionary<string, string> local10 = dictionary1;
                        local10.Add("FileDescription", this.package.Summary ?? (this.package.Description ?? ("Installer for " + this.package.Id)));
                        Dictionary<string, string> local7 = local10;
                        Dictionary<string, string> local8 = local10;
                        local8.Add("ProductName", this.package.Description ?? (this.package.Summary ?? this.package.Id));
                        Dictionary<string, string> dictionary = local8;
                        this.<args>5__2 = Enumerable.Aggregate<KeyValuePair<string, string>, StringBuilder>(dictionary, new StringBuilder("\"" + this.exePath + "\""), Program.<>c.<>9__19_0 ?? (Program.<>c.<>9__19_0 = new Func<StringBuilder, KeyValuePair<string, string>, StringBuilder>(this.<setPEVersionInfoAndIcon>b__19_0)));
                        this.<args>5__2.AppendFormat(" --set-file-version {0} --set-product-version {0}", this.package.Version.ToString());
                        if (this.iconPath != null)
                        {
                            this.<args>5__2.AppendFormat(" --set-icon \"{0}\"", Path.GetFullPath(this.iconPath));
                        }
                        this.<exe>5__1 = @".\rcedit.exe";
                        if (!File.Exists(this.<exe>5__1))
                        {
                            this.<exe>5__1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "rcedit.exe");
                            if (!File.Exists(this.<exe>5__1))
                            {
                                this.<exe>5__1 = "rcedit.exe";
                            }
                        }
                        awaiter = Utility.InvokeProcessAsync(this.<exe>5__1, this.<args>5__2.ToString(), CancellationToken.None).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0006;
                        }
                        else
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<Tuple<int, string>>, Program.<setPEVersionInfoAndIcon>d__19>(ref awaiter, ref this);
                        }
                    }
                    return;
                TR_0006:
                    awaiter = new TaskAwaiter<Tuple<int, string>>();
                    Tuple<int, string> result = awaiter.GetResult();
                    if (result.Item1 != 0)
                    {
                        throw new Exception($"Failed to modify resources, command invoked was: '{this.<exe>5__1} {this.<args>5__2}'

Output was:
{result.Item2}");
                    }
                    Console.WriteLine(result.Item2);
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <signPEFile>d__18 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string signingOpts;
            public string exePath;
            private string <exe>5__1;
            private TaskAwaiter<Tuple<int, string>> <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<Tuple<int, string>> awaiter;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter<Tuple<int, string>>();
                        this.<>1__state = num = -1;
                        goto TR_0006;
                    }
                    else
                    {
                        this.<exe>5__1 = @".\signtool.exe";
                        if (!File.Exists(this.<exe>5__1))
                        {
                            this.<exe>5__1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "signtool.exe");
                            if (!File.Exists(this.<exe>5__1))
                            {
                                this.<exe>5__1 = "signtool.exe";
                            }
                        }
                        awaiter = Utility.InvokeProcessAsync(this.<exe>5__1, $"sign {this.signingOpts} "{this.exePath}"", CancellationToken.None).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0006;
                        }
                        else
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<Tuple<int, string>>, Program.<signPEFile>d__18>(ref awaiter, ref this);
                        }
                    }
                    return;
                TR_0006:
                    awaiter = new TaskAwaiter<Tuple<int, string>>();
                    Tuple<int, string> result = awaiter.GetResult();
                    if (result.Item1 != 0)
                    {
                        throw new Exception($"Failed to sign, command invoked was: '{this.<exe>5__1} sign {this.signingOpts} {this.exePath}'");
                    }
                    Console.WriteLine(result.Item2);
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <Uninstall>d__10 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public Program <>4__this;
            public string appName;
            private UpdateManager <mgr>5__1;
            private TaskAwaiter <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num != 0)
                    {
                        this.<>4__this.Log<Program>().Info("Starting uninstall for app: " + this.appName);
                        this.appName = this.appName ?? Program.getAppNameFromDirectory(null);
                        this.<mgr>5__1 = new UpdateManager("", this.appName, null, null);
                    }
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0008;
                        }
                        else
                        {
                            awaiter = this.<mgr>5__1.FullUninstall().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0008;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<Uninstall>d__10>(ref awaiter, ref this);
                            }
                        }
                        return;
                    TR_0008:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        this.<mgr>5__1.RemoveUninstallerRegistryEntry();
                        goto TR_0007;
                    }
                    finally
                    {
                        if ((num < 0) && (this.<mgr>5__1 != null))
                        {
                            this.<mgr>5__1.Dispose();
                        }
                    }
                    return;
                TR_0007:
                    this.<mgr>5__1 = null;
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <Update>d__6 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string appName;
            public Program <>4__this;
            public string updateUrl;
            private Program.<>c__DisplayClass6_0 <>8__1;
            private UpdateInfo <updateInfo>5__2;
            private bool <ignoreDeltaUpdates>5__3;
            private TaskAwaiter<UpdateInfo> <>u__1;
            private TaskAwaiter <>u__2;
            private TaskAwaiter<string> <>u__3;
            private TaskAwaiter<RegistryKey> <>u__4;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    switch (num)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            break;

                        default:
                            this.appName = this.appName ?? Program.getAppNameFromDirectory(null);
                            this.<>4__this.Log<Program>().Info("Starting update, downloading from " + this.updateUrl);
                            this.<>8__1 = new Program.<>c__DisplayClass6_0();
                            this.<>8__1.mgr = new UpdateManager(this.updateUrl, this.appName, null, null);
                            break;
                    }
                    try
                    {
                        TaskAwaiter<RegistryKey> awaiter4;
                        switch (num)
                        {
                            case 0:
                            case 1:
                            case 2:
                                break;

                            case 3:
                                awaiter4 = this.<>u__4;
                                this.<>u__4 = new TaskAwaiter<RegistryKey>();
                                this.<>1__state = num = -1;
                                goto TR_000F;

                            default:
                                this.<ignoreDeltaUpdates>5__3 = false;
                                this.<>4__this.Log<Program>().Info("About to update to: " + this.<>8__1.mgr.RootAppDirectory);
                                this.<>4__this.ClearAllCompatibilityFlags(this.<>8__1.mgr.RootAppDirectory);
                                break;
                        }
                        goto TR_001E;
                    TR_000F:
                        awaiter4.GetResult();
                        awaiter4 = new TaskAwaiter<RegistryKey>();
                        goto TR_000E;
                    TR_0010:
                        Path.Combine(this.<>8__1.mgr.RootAppDirectory, "Update.exe");
                        awaiter4 = this.<>4__this.ErrorIfThrows<RegistryKey>(new Func<Task<RegistryKey>>(this.<>8__1.<Update>b__3), "Failed to create uninstaller registry entry").GetAwaiter();
                        if (awaiter4.IsCompleted)
                        {
                            goto TR_000F;
                        }
                        else
                        {
                            this.<>1__state = num = 3;
                            this.<>u__4 = awaiter4;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<RegistryKey>, Program.<Update>d__6>(ref awaiter4, ref this);
                        }
                        return;
                    TR_001E:
                        while (true)
                        {
                            try
                            {
                                TaskAwaiter<UpdateInfo> awaiter;
                                TaskAwaiter awaiter2;
                                TaskAwaiter<string> awaiter3;
                                switch (num)
                                {
                                    case 0:
                                        awaiter = this.<>u__1;
                                        this.<>u__1 = new TaskAwaiter<UpdateInfo>();
                                        this.<>1__state = num = -1;
                                        break;

                                    case 1:
                                        awaiter2 = this.<>u__2;
                                        this.<>u__2 = new TaskAwaiter();
                                        this.<>1__state = num = -1;
                                        goto TR_0013;

                                    case 2:
                                        awaiter3 = this.<>u__3;
                                        this.<>u__3 = new TaskAwaiter<string>();
                                        this.<>1__state = num = -1;
                                        goto TR_0012;

                                    default:
                                        awaiter = this.<>8__1.mgr.CheckForUpdate(this.<ignoreDeltaUpdates>5__3, Program.<>c.<>9__6_0 ?? (Program.<>c.<>9__6_0 = new Action<int>(this.<Update>b__6_0))).GetAwaiter();
                                        if (awaiter.IsCompleted)
                                        {
                                            break;
                                        }
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<UpdateInfo>, Program.<Update>d__6>(ref awaiter, ref this);
                                        return;
                                }
                                UpdateInfo result = new TaskAwaiter<UpdateInfo>().GetResult();
                                this.<updateInfo>5__2 = result;
                                awaiter2 = this.<>8__1.mgr.DownloadReleases(this.<updateInfo>5__2.ReleasesToApply, Program.<>c.<>9__6_1 ?? (Program.<>c.<>9__6_1 = new Action<int>(this.<Update>b__6_1))).GetAwaiter();
                                if (!awaiter2.IsCompleted)
                                {
                                    this.<>1__state = num = 1;
                                    this.<>u__2 = awaiter2;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<Update>d__6>(ref awaiter2, ref this);
                                    break;
                                }
                                goto TR_0013;
                            TR_0012:
                                awaiter3.GetResult();
                                awaiter3 = new TaskAwaiter<string>();
                                this.<updateInfo>5__2 = null;
                                goto TR_0010;
                            TR_0013:
                                awaiter2.GetResult();
                                awaiter2 = new TaskAwaiter();
                                awaiter3 = this.<>8__1.mgr.ApplyReleases(this.<updateInfo>5__2, Program.<>c.<>9__6_2 ?? (Program.<>c.<>9__6_2 = new Action<int>(this.<Update>b__6_2))).GetAwaiter();
                                if (awaiter3.IsCompleted)
                                {
                                    goto TR_0012;
                                }
                                else
                                {
                                    this.<>1__state = num = 2;
                                    this.<>u__3 = awaiter3;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, Program.<Update>d__6>(ref awaiter3, ref this);
                                }
                                return;
                            }
                            catch (Exception exception)
                            {
                                if (this.<ignoreDeltaUpdates>5__3)
                                {
                                    this.<>4__this.Log<Program>().ErrorException("Really couldn't apply updates!", exception);
                                    throw;
                                }
                                this.<>4__this.Log<Program>().WarnException("Failed to apply updates, falling back to full updates", exception);
                                this.<ignoreDeltaUpdates>5__3 = true;
                                continue;
                            }
                            break;
                        }
                    }
                    finally
                    {
                        if ((num < 0) && (this.<>8__1.mgr != null))
                        {
                            this.<>8__1.mgr.Dispose();
                        }
                    }
                    return;
                TR_000E:
                    this.<>8__1 = null;
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }
                catch (Exception exception2)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception2);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <UpdateSelf>d__7 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public Program <>4__this;
            private TaskAwaiter <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter awaiter;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter();
                        this.<>1__state = num = -1;
                        goto TR_0004;
                    }
                    else
                    {
                        this.<>4__this.waitForParentToExit();
                        string src = Assembly.GetExecutingAssembly().Location;
                        string updateDotExeForOurPackage = Path.Combine(Path.GetDirectoryName(src), "..", "Update.exe");
                        awaiter = Task.Run(new Action(class_1.<UpdateSelf>b__0)).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0004;
                        }
                        else
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Program.<UpdateSelf>d__7>(ref awaiter, ref this);
                        }
                    }
                    return;
                TR_0004:
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }
    }
}

