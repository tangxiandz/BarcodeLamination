
using BarcodeLaminationPrint;
using MistakeProofing.Printing.BarTender;
using Application = MistakeProofing.Printing.BarTender.Application;

internal static class ApplicationExtensions
{
    [Obsolete]
    internal static Format CreateFormat(this Application application, Template template, object? dataSource)
    {
        string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Templates", template.FileName);
        template.SaveToFile(path);

        var format = application.Formats.Open(path);

        if (dataSource is not null)
        {
            foreach (var binding in template.DataBindings)
            {
                if (format.NamedSubStrings.Contains(binding.NamedSubString))
                {
                    var subString = format.NamedSubStrings[binding.NamedSubString];
                    if (!string.IsNullOrEmpty(binding.PropertyName))
                    {
                        object? value = ReflectionHelper.GetProperty(dataSource, binding.PropertyName);
                        if (binding.IsCustomAttribute)
                        {
                            if (!string.IsNullOrEmpty(binding.CustomAttributeName) &&
                                value is ICustomAttributeCollection customAttributes &&
                                customAttributes.Contains(binding.CustomAttributeName))
                            {
                                value = customAttributes[binding.CustomAttributeName]?.Value;
                            }
                            else
                            {
                                value = null;
                            }
                        }

                        subString.Value = string.Format(binding.FormatString ?? "{0}", value);
                    }
                }
            }
        }

        return format;
    }

    internal static Format CreateFormat(this Application application, Template template)
    {
        string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Templates", template.FileName);
        template.SaveToFile(path);
        return application.Formats.Open(path);
    }

    [Obsolete]
    internal static LabelP CreateLabel(this Application application, Template template)
    {
        string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Templates", template.FileName);
        template.SaveToFile(path);

        var format = application.Formats.Open(path);

        return new LabelP(format, template.DataBindings, template.Cusproattributes);
    }
}