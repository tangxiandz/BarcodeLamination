using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationPrint
{
    public class TemplateBindingCollection : KeyedCollection<string, TemplateBinding>
    {
        internal TemplateBindingCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        { }

        internal TemplateBindingCollection(IEnumerable<TemplateBinding> collection)
            : this()
        {
            foreach (var item in collection)
            {
                Items.Add(item);
            }
        }

        protected override string GetKeyForItem(TemplateBinding item) => item.NamedSubString;

        public TemplateBinding Add(
            string namedSubString,
            string? propertyName = null,
            bool isCustomAttribute = false,
            string? customAttributeName = null,
            string? formatString = null)
        {
            TemplateBinding item = new()
            {
                NamedSubString = namedSubString,
                PropertyName = propertyName,
                IsCustomAttribute = isCustomAttribute,
                CustomAttributeName = customAttributeName,
                FormatString = formatString
            };
            Add(item);
            return item;
        }
    }
}
