using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Utility.Collections;

namespace XKit.Lib.Common.Utility.Extensions {
    
    public static class EnumerableExtensions {
        public static IEnumerable<T> ForEach<T>(
            this IEnumerable<T> source,
            Action<T> action
        ) {
            foreach (T element in source) {
                action(element);
            }
            return source;
        }

        public async static Task<IEnumerable<T>> ForEach<T>(
            this IEnumerable<T> source,
            Func<T, Task> action
        ) {
            foreach (T element in source) {
                await action(element);
            }
            return source;
        }

        public static SynchronizedList<T> ToSynchronizedList<T>(
            this IEnumerable<T> source
        ) => new SynchronizedList<T>(source);

        public static (IEnumerable<T>, IEnumerable<T>) Split<T>(
            this IEnumerable<T> source, 
            Func<T, bool> splitToFirstListTest
        ) {
            var first = new List<T>();
            var second = new List<T>();
            foreach(T element in source) {
                if (splitToFirstListTest(element)) {
                    first.Add(element);
                } else { 
                    second.Add(element);
                }
            }
            return (first, second);
        }

        public static IList<IList<T>> Split<T>(
            this IEnumerable<T> source, 
            Func<T, int> splitter
        ) {
            var lists = new List<List<T>>();

            foreach(T element in source) {
                int i = splitter(element);
                while (lists.Count <= i) {
                    lists.Add(new List<T>());
                }
                lists[i].Add(element);
            }
            return lists.Select(ls => (IList<T>)ls).ToArray();
        }
    }
}