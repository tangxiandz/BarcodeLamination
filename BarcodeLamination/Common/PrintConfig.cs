using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BarcodeLaminationPrint
{
   public static class PrintConfig
    {
        public static Template ConvertToTemplate(templates dbTemplate, List<templatebindings> bindings)
        {
            // 创建XML文档结构
            var xmlDoc = new XmlDocument();

            // 创建根节点
            XmlElement templateElement = xmlDoc.CreateElement("template");
            templateElement.SetAttribute("name", dbTemplate.FileName);
            templateElement.SetAttribute("type", ((TemplateType)dbTemplate.TemplateType).ToString());
            templateElement.SetAttribute("supportsSerialization", dbTemplate.SupportsSerialization == 1 ? "true" : "false");
            xmlDoc.AppendChild(templateElement);

            // 添加描述（如果有）
            if (!string.IsNullOrEmpty(dbTemplate.Description))
            {
                XmlElement descElement = xmlDoc.CreateElement("description");
                descElement.InnerText = dbTemplate.Description;
                templateElement.AppendChild(descElement);
            }

            // 数据绑定节点（动态生成）
            XmlElement bindingsElement = xmlDoc.CreateElement("dataBindings");
            templateElement.AppendChild(bindingsElement);
            if (bindings != null)
            {
                foreach (var binding in bindings)
                {
                    XmlElement bindingElement = xmlDoc.CreateElement("binding");
                    bindingElement.SetAttribute("namedSubString", binding.NamedSubString);
                    bindingElement.SetAttribute("propertyName", binding.PropertyName ?? "");
                    bindingElement.SetAttribute("customAttributeName", binding.CustomAttributeName);
                    bindingElement.SetAttribute("isCustomAttribute", binding.IsCustomAttribute == 1 ? "true" : "false");

                    if (!string.IsNullOrEmpty(binding.FormatString))
                    {
                        bindingElement.SetAttribute("formatString", binding.FormatString);
                    }

                    bindingsElement.AppendChild(bindingElement);
                }
            }
            //// 数据绑定节点（动态生成）
            //XmlElement cusproattributesElement = xmlDoc.CreateElement("cusproattributes");
            //templateElement.AppendChild(cusproattributesElement);
            //if (bindings != null)
            //{
            //    foreach (var attribute in cusproattributes)
            //    {
            //        XmlElement attributeElement = xmlDoc.CreateElement("attribute");
            //        attributeElement.SetAttribute("name", attribute.Name);
            //        attributeElement.SetAttribute("value", attribute.Value ?? "");

            //        cusproattributesElement.AppendChild(attributeElement);
            //    }
            //}

            // 添加图片数据
            XmlElement imageElement = xmlDoc.CreateElement("image");
            XmlCDataSection imageCData = xmlDoc.CreateCDataSection(Convert.ToBase64String(dbTemplate.ImageBytes));
            imageElement.AppendChild(imageCData);
            templateElement.AppendChild(imageElement);

            // 添加文件哈希
            XmlElement hashElement = xmlDoc.CreateElement("hash");
            hashElement.InnerText = dbTemplate.FileHash;
            templateElement.AppendChild(hashElement);

            // 添加文件内容
            XmlElement contentElement = xmlDoc.CreateElement("content");
            XmlCDataSection contentCData = xmlDoc.CreateCDataSection(Convert.ToBase64String(dbTemplate.FileContent));
            contentElement.AppendChild(contentCData);
            templateElement.AppendChild(contentElement);

            // 临时保存XML文件
            string tempFilePath = Path.GetTempFileName();
            xmlDoc.Save(tempFilePath);

            try
            {
                // 使用Template类的Create方法创建实例
                var template = Template.Create(tempFilePath, null);

                // 补充设置其他属性
                template.ReadOnly = dbTemplate.ReadOnly == 1;

                return template;
            }
            finally
            {
                // 删除临时文件
                File.Delete(tempFilePath);
            }
        }


    }
}
