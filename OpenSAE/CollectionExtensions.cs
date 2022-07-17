using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE
{
    public static class CollectionExtensions
    {
        public static IEnumerable<string?> ToEnumerable(this StringCollection collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }

        public static StringCollection ToStringCollection(this IEnumerable<string> strings)
        {
            var collection = new StringCollection();

            collection.AddRange(strings.ToArray());

            return collection;
        }
    }
}
