using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace SAM.WPF.Core.Behaviors
{
    public class RangeCollectionBehavior : MarkupExtension
    {
        public int Min { get; set; }
        public int Max { get; set; }
 
        public override object ProvideValue(IServiceProvider _)
        {
            return CreateRange();
        }
 
        private List<object> CreateRange()
        {
            if (Min > Max) throw new ArgumentException($"{nameof(Min)} cannot be greater than {nameof(Max)}.", nameof(Min));

            var count = Max - Min;
            var collection = Enumerable.Range(Min, count);

            return collection.Cast<object>().ToList();
        }
    }
}
