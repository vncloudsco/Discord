namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class PackageOperationExtensions
    {
        private static object GetOperationKey(PackageOperation operation) => 
            Tuple.Create<PackageAction, string, SemanticVersion>(operation.Action, operation.Package.Id, operation.Package.Version);

        private static object GetOpposingOperationKey(PackageOperation operation) => 
            Tuple.Create<PackageAction, string, SemanticVersion>((operation.Action == PackageAction.Install) ? PackageAction.Uninstall : PackageAction.Install, operation.Package.Id, operation.Package.Version);

        public static IList<PackageOperation> Reduce(this IEnumerable<PackageOperation> operations)
        {
            Dictionary<object, List<IndexedPackageOperation>> dictionary = Enumerable.ToDictionary<IGrouping<object, IndexedPackageOperation>, object, List<IndexedPackageOperation>>(Enumerable.ToLookup<IndexedPackageOperation, object>(Enumerable.Select<PackageOperation, IndexedPackageOperation>(operations, (o, index) => new IndexedPackageOperation(index, o)), o => GetOperationKey(o.Operation)), g => g.Key, g => g.ToList<IndexedPackageOperation>());
            using (IEnumerator<PackageOperation> enumerator = operations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    object opposingOperationKey = GetOpposingOperationKey(enumerator.Current);
                    if (dictionary.ContainsKey(opposingOperationKey))
                    {
                        List<IndexedPackageOperation> source = dictionary[opposingOperationKey];
                        source.RemoveAt(0);
                        if (!source.Any<IndexedPackageOperation>())
                        {
                            dictionary.Remove(opposingOperationKey);
                        }
                    }
                }
            }
            return (from o in dictionary select o.Value).ToList<IndexedPackageOperation>().Reorder();
        }

        private static IList<PackageOperation> Reorder(this List<IndexedPackageOperation> operations)
        {
            operations.Sort((a, b) => a.Index - b.Index);
            List<IndexedPackageOperation> list = new List<IndexedPackageOperation>();
            for (int i = 0; i < operations.Count; i++)
            {
                IndexedPackageOperation item = operations[i];
                if (item.Operation.Package.IsSatellitePackage())
                {
                    list.Add(item);
                    operations.RemoveAt(i);
                    i--;
                }
            }
            if (list.Count > 0)
            {
                operations.InsertRange(0, from s in list
                    where s.Operation.Action == PackageAction.Uninstall
                    select s);
                operations.AddRange(from s in list
                    where s.Operation.Action == PackageAction.Install
                    select s);
            }
            return (from o in operations select o.Operation).ToList<PackageOperation>();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageOperationExtensions.<>c <>9 = new PackageOperationExtensions.<>c();
            public static Func<PackageOperation, int, PackageOperationExtensions.IndexedPackageOperation> <>9__0_0;
            public static Func<PackageOperationExtensions.IndexedPackageOperation, object> <>9__0_1;
            public static Func<IGrouping<object, PackageOperationExtensions.IndexedPackageOperation>, object> <>9__0_2;
            public static Func<IGrouping<object, PackageOperationExtensions.IndexedPackageOperation>, List<PackageOperationExtensions.IndexedPackageOperation>> <>9__0_3;
            public static Func<KeyValuePair<object, List<PackageOperationExtensions.IndexedPackageOperation>>, IEnumerable<PackageOperationExtensions.IndexedPackageOperation>> <>9__0_4;
            public static Comparison<PackageOperationExtensions.IndexedPackageOperation> <>9__1_0;
            public static Func<PackageOperationExtensions.IndexedPackageOperation, bool> <>9__1_1;
            public static Func<PackageOperationExtensions.IndexedPackageOperation, bool> <>9__1_2;
            public static Func<PackageOperationExtensions.IndexedPackageOperation, PackageOperation> <>9__1_3;

            internal PackageOperationExtensions.IndexedPackageOperation <Reduce>b__0_0(PackageOperation o, int index) => 
                new PackageOperationExtensions.IndexedPackageOperation(index, o);

            internal object <Reduce>b__0_1(PackageOperationExtensions.IndexedPackageOperation o) => 
                PackageOperationExtensions.GetOperationKey(o.Operation);

            internal object <Reduce>b__0_2(IGrouping<object, PackageOperationExtensions.IndexedPackageOperation> g) => 
                g.Key;

            internal List<PackageOperationExtensions.IndexedPackageOperation> <Reduce>b__0_3(IGrouping<object, PackageOperationExtensions.IndexedPackageOperation> g) => 
                g.ToList<PackageOperationExtensions.IndexedPackageOperation>();

            internal IEnumerable<PackageOperationExtensions.IndexedPackageOperation> <Reduce>b__0_4(KeyValuePair<object, List<PackageOperationExtensions.IndexedPackageOperation>> o) => 
                o.Value;

            internal int <Reorder>b__1_0(PackageOperationExtensions.IndexedPackageOperation a, PackageOperationExtensions.IndexedPackageOperation b) => 
                (a.Index - b.Index);

            internal bool <Reorder>b__1_1(PackageOperationExtensions.IndexedPackageOperation s) => 
                (s.Operation.Action == PackageAction.Uninstall);

            internal bool <Reorder>b__1_2(PackageOperationExtensions.IndexedPackageOperation s) => 
                (s.Operation.Action == PackageAction.Install);

            internal PackageOperation <Reorder>b__1_3(PackageOperationExtensions.IndexedPackageOperation o) => 
                o.Operation;
        }

        private sealed class IndexedPackageOperation
        {
            public IndexedPackageOperation(int index, PackageOperation operation)
            {
                this.Index = index;
                this.Operation = operation;
            }

            public int Index { get; set; }

            public PackageOperation Operation { get; set; }
        }
    }
}

