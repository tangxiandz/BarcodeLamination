using MistakeProofing.Printing.BarTender;
using BarcodeLaminationPrint;

public class LabelP
{
    private readonly Format _format;
    private readonly TemplateBindingCollection _bindings;
    private readonly List<customproductattributes> _cusproattributes;
    private object? _dataSource;

    public LabelP(Format format, TemplateBindingCollection bindings, List<customproductattributes> cusproattributes)
    {
        _format = format;
        _bindings = bindings;
        _cusproattributes = cusproattributes;
    }

    public object? DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource != value)
            {
                _dataSource = value;

                DataBind();
            }
        }
    }

    protected virtual void DataBind()
    {
        if (_dataSource is null)
        {
            return;
        }

        foreach (var binding in _bindings)
        {
            if (!_format.NamedSubStrings.Contains(binding.NamedSubString)) continue;

            var subString = _format.NamedSubStrings[binding.NamedSubString];
            object? rawValue = GetRawValue(binding); // 获取原始值 
            //// 处理日期格式化
            if (rawValue is DateTime dateValue && IsDateFormat(binding.FormatString))
            {
                subString.Value = ApplyDateTimeFormat(dateValue, binding.FormatString);
            }
            else
            {
                subString.Value = rawValue?.ToString() ?? string.Empty;
            }
        }
    }
    private object? GetRawValue(TemplateBinding binding)
    {
        if (!binding.IsCustomAttribute)
        {
            return ReflectionHelper.GetProperty(_dataSource, binding.CustomAttributeName);
        }

        if (binding.IsCustomAttribute && !string.IsNullOrEmpty(binding.CustomAttributeName))
        {
            var attr = _cusproattributes?.FirstOrDefault(a => a.Name == binding.CustomAttributeName);
            return attr?.Value;
        }

        return null;
    }

    // 判断是否是日期格式
    private static bool IsDateFormat(string? format)
    {
        return !string.IsNullOrEmpty(format) &&
              (format.Contains("yyyy") || format.Contains("MM") || format.Contains("dd"));
    }

    // 应用日期格式
    private static string ApplyDateTimeFormat(DateTime date, string? format)
    {
        // 修复格式字符串中的常见错误（如将 "mm" 改为 "MM"）
        string correctedFormat = format?
            .Replace("yyyy", "yyyy")
            .Replace("mm", "MM")  // 分钟 -> 月份
            .Replace("dd", "dd") ?? string.Empty;

        return date.ToString(correctedFormat);
    }
    public void Print(int identicalCopiesOfLabel = 1,
                      int numberSerializedLabels = 1)
    {
        _format.IdenticalCopiesOfLabel = identicalCopiesOfLabel;
        _format.NumberSerializedLabels = numberSerializedLabels;
        _format.PrintOut();
    }
}