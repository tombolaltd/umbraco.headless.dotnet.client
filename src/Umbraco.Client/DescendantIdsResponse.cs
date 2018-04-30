using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Client
{
    [Serializable]
    class DescendantIdsResponse
    {
        public int Origin { get; set; }
        public IEnumerable<int> Descendants { get; set; }
        public bool PublishedOnly { get; set; }
        public int DescendantCount => Descendants?.Count() ?? 0;

        public static DescendantIdsResponse Empty => new DescendantIdsResponse() { Descendants = new int[] { } };
    }
}
