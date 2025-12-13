using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationPrint
{
    public class ControlInfo
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string tableName { set; get; }

        /// <summary>
        /// 数据库字段名称
        /// </summary>
        public string dataBaseFieldName { set; get; }

        /// <summary>
        /// 数据库字段类型
        /// </summary>
        public string dataBaseFieldType { set; get; }

        /// <summary>
        /// 数据库字段说明
        /// </summary>
        public string dataBaseFieldDDesr { set; get; }

        /// <summary>
        /// 控件类型
        /// </summary>
        public string controlType { set; get; }

        /// <summary>
        /// 控件name
        /// </summary>
        public string controlName { set; get; }

        /// <summary>
        /// 控件类型
        /// </summary>
        public string labelName { set; get; }

        /// <summary>
        /// 控件name
        /// </summary>
        public string labelText { set; get; }

        /// <summary>
        /// 是否自增字段
        /// </summary>
        public bool isIdentity { set; get; }

        /// <summary>
        /// 是否搜索列
        /// </summary>
        public bool isSearch { set; get; }

        /// <summary>
        /// 是否为空检验
        /// </summary>
        public bool isCheck { set; get; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public bool isEdit { set; get; } = true;
    }
}
