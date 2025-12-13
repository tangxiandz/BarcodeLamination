using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationPrint
{
    public class TemplateBinding
    {
        public  string? NamedSubString { get; init; }

        public string? PropertyName { get; set; }

        public bool IsCustomAttribute { get; set; }

        public string? CustomAttributeName { get; set; }

        public string? FormatString { get; set; }

        public override string ToString() => NamedSubString;
    }
    public partial class templatebindings
    {
        public templatebindings()
        {


        }
        public long ID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string NamedSubString { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PropertyName { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int IsCustomAttribute { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CustomAttributeName { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FormatString { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long TemplateId { get; set; }

    }
    public class ComboBoxItems
    {
        public int ID { get; set; }
        public string Value { get; set; }
    }

    public class AttributeItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}
