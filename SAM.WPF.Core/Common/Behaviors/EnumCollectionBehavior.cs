using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core.Behaviors
{
    public class EnumCollectionBehavior : MarkupExtension
    {
        public Type EnumType { get; set; }
        public bool UseEnumValue { get; set; }
 
        public override object ProvideValue(IServiceProvider _)
        {
            if (EnumType != null)
            {
                return CreateEnumValueList(EnumType);
            }
            return default;
        }
 
        private List<object> CreateEnumValueList(Type enumType)
        {
            if (UseEnumValue)
            {
                var intValues = Enum.GetValues(enumType)
                    .Cast<object>();

                return intValues.ToList();
            }

            var values = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(i => i.GetDescription())
                .ToList<object>();
            return values;
        }
    }
}
