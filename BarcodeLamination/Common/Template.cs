using MistakeProofing.Printing.BarTender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Drawing; // 需要安装 System.Drawing.Common 包
using System.IO;

namespace BarcodeLaminationPrint
{
    public enum TemplateType
    {
        Inner = 1,
        Middle = 2,
        Outer = 3,
    }

    public class Template : Entity, IEquatable<Template>
    {
        public static Template Create(string filename, sysUser? user = null)
        {
            var document = new XmlDocument();
            document.Load(filename);

            var templateEl = document["template"] ?? throw new FormatException("Could not resolve file.");

            string fileName = templateEl.GetAttribute("name") ?? throw new FormatException("Name attribute is required");

            string? s = templateEl.GetAttribute("type");
            if (string.IsNullOrEmpty(s) ||
                !Enum.TryParse(s, out TemplateType templateType))
            {
                throw new FormatException("Could not resolve template type.");
            }

            s = templateEl.GetAttribute("supportsSerialization");
            if (!bool.TryParse(s, out var supportsSerialization))
            {
                supportsSerialization = false;
            }

            string? description = templateEl["description"]?.InnerText;

            var dataBindings = new TemplateBindingCollection();
            var elements = templateEl.SelectNodes("dataBindings/binding");
            if (elements != null)
            {
                foreach (XmlElement bindingEl in elements)
                {
                    s = bindingEl.GetAttribute("namedSubString");
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }

                    var binding = new TemplateBinding { NamedSubString = s };

                    s = bindingEl.GetAttribute("propertyName");
                    if (!string.IsNullOrEmpty(s))
                    {
                        binding.PropertyName = s;
                    }

                    s = bindingEl.GetAttribute("isCustomAttribute");
                    if (bool.TryParse(s, out var isCustomAttribute))
                    {
                        binding.IsCustomAttribute = isCustomAttribute;
                    }

                    s = bindingEl.GetAttribute("customAttributeName");
                    if (!string.IsNullOrEmpty(s))
                    {
                        binding.CustomAttributeName = s;
                    }

                    s = bindingEl.GetAttribute("formatString");
                    if (!string.IsNullOrEmpty(s))
                    {
                        binding.FormatString = s;
                    }

                    dataBindings.Add(binding);
                }
            }

            var imageEl = templateEl["image"];
            var image = imageEl?.FirstChild as XmlCDataSection ?? throw new FormatException("Could not resolve image.");
            byte[] imageBytes = Convert.FromBase64String(image.Data);

            string? hash = templateEl["hash"]?.InnerText;
            if (string.IsNullOrEmpty(hash))
            {
                throw new FormatException("Could not resolve file hash.");
            }

            var contentEl = templateEl["content"];
            var content = contentEl?.FirstChild as XmlCDataSection ?? throw new FormatException("Could not resolve file content.");
            byte[] fileContent = Convert.FromBase64String(content.Data);

            // 在.NET 6.0中，使用MD5.HashData
            string fileHash = BitConverter.ToString(MD5.HashData(fileContent)).Replace("-", "").ToLowerInvariant();
            if (hash != fileHash)
            {
                throw new FormatException("Content validation failed.");
            }

            return new Template
            {
                TemplateType = templateType,
                FileName = fileName,
                FileHash = fileHash,
                FileContent = fileContent,
                ImageBytes = imageBytes,
                Description = description,
                SupportsSerialization = supportsSerialization,
                DataBindings = dataBindings,
                Creator = user?.Username,
                LastModifiedBy = user?.Username,
            };
        }

        public static Template Create(Format format, sysUser? user = null)
        {
            string path = format.FileName;
            string filename = Path.GetFileName(path);
            string fileHash = FileHelper.GetFileHash(path);
            byte[] fileContent = FileHelper.GetFileContent(path);

            path = Path.GetTempFileName();
            format.ExportToFile(path, "png", ColorConstants.Color24Bit, ResolutionConstants.Screen, SaveOptionConstants.SaveChanges);
            byte[] imageBytes = FileHelper.GetFileContent(path);
            File.Delete(path);

            Template template = new()
            {
                FileName = filename,
                FileHash = fileHash,
                FileContent = fileContent,
                ImageBytes = imageBytes,
                Creator = user?.Username,
                LastModifiedBy = user?.Username,
            };

            foreach (var subString in format.NamedSubStrings.OrderBy(x => x.Name))
            {
                template.DataBindings.Add(new TemplateBinding
                {
                    NamedSubString = subString.Name,
                    CustomAttributeName= subString.Name,
                    // 其他属性按需初始化
                });
            }

            return template;
        }

        private Template()
        { }

        public TemplateType TemplateType { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileHash { get; set; } = string.Empty;

        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();

        public Image? Image
        {
            get
            {
                if (ImageBytes.Length == 0)
                {
                    return null;
                }

                using var ms = new MemoryStream(ImageBytes);
                return Image.FromStream(ms);
            }
        }

        public string? Description { get; set; }

        public bool SupportsSerialization { get; set; }

        public TemplateBindingCollection DataBindings { get; init; } = new TemplateBindingCollection();

        public List<customproductattributes>? Cusproattributes { get; set; }

        public bool ReadOnly { get; set; }

        public DateTime Created { get; private set; }

        public string? Creator { get; init; }

        public DateTime LastModified { get; private set; }

        public string? LastModifiedBy { get; set; }

        public bool Load(Format format)
        {
            string path = format.FileName;
            string fileHash = FileHelper.GetFileHash(path);
            if (fileHash == FileHash)
            {
                return false;
            }

            FileName = Path.GetFileName(path);
            FileHash = fileHash;
            FileContent = FileHelper.GetFileContent(path);

            path = Path.GetTempFileName();
            format.ExportToFile(path, "png", ColorConstants.Color24Bit, ResolutionConstants.Screen, SaveOptionConstants.SaveChanges);
            ImageBytes = FileHelper.GetFileContent(path);
            File.Delete(path);

            var bindings = new TemplateBindingCollection(DataBindings);
            DataBindings.Clear();
            foreach (var subString in format.NamedSubStrings)
            {
                string name = subString.Name;
                if (bindings.TryGetValue(name, out var item))
                {
                    DataBindings.Add(name,
                                     item.PropertyName,
                                     item.IsCustomAttribute,
                                     item.CustomAttributeName,
                                     item.FormatString);
                }
                else
                {
                    DataBindings.Add(name);
                }
            }

            return true;
        }

        public void SaveToFile(string filename)
        {
            if (File.Exists(filename))
            {
                string fileHash = FileHelper.GetFileHash(filename);
                if (fileHash == FileHash)
                {
                    return;
                }
            }

            using var fs = File.Create(filename);
            fs.Write(FileContent, 0, FileContent.Length);
        }

        public void WriteXml(string filename)
        {
            var settings = new XmlWriterSettings()
            {
                Indent = true,
            };
            using var writer = XmlWriter.Create(filename, settings);
            writer.WriteStartDocument();

            writer.WriteStartElement("template");
            writer.WriteAttributeString("name", FileName);
            writer.WriteAttributeString("type", TemplateType.ToString());
            if (SupportsSerialization)
            {
                writer.WriteAttributeString("supportsSerialization", "true");
            }

            writer.WriteStartElement("dataBindings");
            foreach (var item in DataBindings)
            {
                writer.WriteStartElement("binding");
                writer.WriteAttributeString("namedSubString", item.NamedSubString);
                if (!string.IsNullOrEmpty(item.PropertyName))
                {
                    writer.WriteAttributeString("propertyName", item.PropertyName);
                }
                if (item.IsCustomAttribute)
                {
                    writer.WriteAttributeString("CustomAttribute", "true");
                    writer.WriteAttributeString("customAttributeName", item.CustomAttributeName);
                }
                if (!string.IsNullOrEmpty(item.FormatString))
                {
                    writer.WriteAttributeString("formatString", item.FormatString);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("image");
            writer.WriteCData(Convert.ToBase64String(ImageBytes));
            writer.WriteEndElement();

            writer.WriteElementString("hash", FileHash);

            writer.WriteStartElement("content");
            writer.WriteCData(Convert.ToBase64String(FileContent));
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

        public void ReadXml(string filename)
        {
            var document = new XmlDocument();
            document.Load(filename);

            var templateEl = document["template"] ?? throw new FormatException("Could not resolve file.");

            string fileName = templateEl.GetAttribute("name") ?? throw new FormatException("Name attribute is required");

            string? s = templateEl.GetAttribute("type");
            if (string.IsNullOrEmpty(s) ||
                !Enum.TryParse(s, out TemplateType templateType))
            {
                throw new FormatException("Could not resolve template type.");
            }

            s = templateEl.GetAttribute("supportsSerialization");
            if (!bool.TryParse(s, out var supportsSerialization))
            {
                supportsSerialization = false;
            }

            string? description = templateEl["description"]?.InnerText;

            var imageEl = templateEl["image"];
            var image = imageEl?.FirstChild as XmlCDataSection ?? throw new FormatException("Could not resolve image.");
            byte[] imageBytes = Convert.FromBase64String(image.Data);

            string? hash = templateEl["hash"]?.InnerText;
            if (string.IsNullOrEmpty(hash))
            {
                throw new FormatException("Could not resolve file hash.");
            }

            var contentEl = templateEl["content"];
            var content = contentEl?.FirstChild as XmlCDataSection ?? throw new FormatException("Could not resolve file content.");
            byte[] fileContent = Convert.FromBase64String(content.Data);

            // 在.NET 6.0中，使用MD5.HashData
            string fileHash = BitConverter.ToString(MD5.HashData(fileContent)).Replace("-", "").ToLowerInvariant();
            if (hash != fileHash)
            {
                throw new FormatException("Content validation failed.");
            }

            DataBindings.Clear();
            var dataBindings = templateEl.SelectNodes("dataBindings/binding");
            if (dataBindings != null)
            {
                foreach (XmlElement bindingEl in dataBindings)
                {
                    s = bindingEl.GetAttribute("namedSubString");
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }

                    var binding = new TemplateBinding { NamedSubString = s };

                    s = bindingEl.GetAttribute("propertyName");
                    if (!string.IsNullOrEmpty(s))
                    {
                        binding.PropertyName = s;
                    }

                    s = bindingEl.GetAttribute("isCustomAttribute");
                    if (bool.TryParse(s, out var isCustomAttribute))
                    {
                        binding.IsCustomAttribute = isCustomAttribute;
                    }

                    s = bindingEl.GetAttribute("customAttributeName");
                    if (!string.IsNullOrEmpty(s))
                    {
                        binding.CustomAttributeName = s;
                    }

                    s = bindingEl.GetAttribute("formatString");
                    if (!string.IsNullOrEmpty(s))
                    {
                        binding.FormatString = s;
                    }

                    DataBindings.Add(binding);
                }
            }

            TemplateType = templateType;
            FileName = fileName;
            FileHash = fileHash;
            FileContent = fileContent;
            ImageBytes = imageBytes;
            Description = description;
            SupportsSerialization = supportsSerialization;
        }

        public bool Equals(Template? other)
        {
            if (other is null)
            {
                return false;
            }

            return FileHash == other.FileHash;
        }

        public override bool Equals(object? obj) => Equals(obj as Template);

        public override int GetHashCode() => FileHash.GetHashCode();

        public static bool operator ==(Template? lhs, Template? rhs)
        {
            if (lhs is null || rhs is null)
                return Equals(lhs, rhs);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Template? lhs, Template? rhs) => !(lhs == rhs);

        public override string ToString() => FileName;
    }
}