namespace Splat
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class ModeDetector
    {
        private static bool? cachedInUnitTestRunnerResult;
        private static bool? cachedInDesignModeResult;

        static ModeDetector()
        {
            current = AssemblyFinder.AttemptToLoadType<IModeDetector>("Splat.PlatformModeDetector");
        }

        public static bool InDesignMode()
        {
            if (cachedInDesignModeResult != null)
            {
                return cachedInDesignModeResult.Value;
            }
            if (current != null)
            {
                cachedInDesignModeResult = current.InDesignMode();
                if (cachedInDesignModeResult != null)
                {
                    return cachedInDesignModeResult.Value;
                }
            }
            Type type = Type.GetType("System.ComponentModel.DesignerProperties, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", false);
            if (type != null)
            {
                MethodInfo method = type.GetMethod("GetIsInDesignMode");
                Type type2 = Type.GetType("System.Windows.Controls.Border, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", false);
                if (type2 != null)
                {
                    object[] parameters = new object[] { Activator.CreateInstance(type2) };
                    cachedInDesignModeResult = new bool?((bool) method.Invoke(null, parameters));
                }
            }
            else
            {
                type = Type.GetType("System.ComponentModel.DesignerProperties, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);
                if (type == null)
                {
                    cachedInDesignModeResult = ((type = Type.GetType("Windows.ApplicationModel.DesignMode, Windows, ContentType=WindowsRuntime", false)) == null) ? false : new bool?((bool) type.GetProperty("DesignModeEnabled").GetMethod.Invoke(null, null));
                }
                else
                {
                    MethodInfo method = type.GetMethod("GetIsInDesignMode");
                    Type type3 = Type.GetType("System.Windows.DependencyObject, WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);
                    if (type3 != null)
                    {
                        object[] parameters = new object[] { Activator.CreateInstance(type3) };
                        cachedInDesignModeResult = new bool?((bool) method.Invoke(null, parameters));
                    }
                }
            }
            return cachedInDesignModeResult.GetValueOrDefault();
        }

        public static bool InUnitTestRunner()
        {
            if (cachedInUnitTestRunnerResult != null)
            {
                return cachedInUnitTestRunnerResult.Value;
            }
            if (current == null)
            {
                return false;
            }
            cachedInUnitTestRunnerResult = current.InUnitTestRunner();
            return ((cachedInUnitTestRunnerResult != null) && cachedInUnitTestRunnerResult.Value);
        }

        public static void OverrideModeDetector(IModeDetector modeDetector)
        {
            current = modeDetector;
            cachedInDesignModeResult = null;
            cachedInUnitTestRunnerResult = null;
        }

        private static IModeDetector current
        {
            get => 
                <current>k__BackingField;
            set => 
                (<current>k__BackingField = value);
        }
    }
}

