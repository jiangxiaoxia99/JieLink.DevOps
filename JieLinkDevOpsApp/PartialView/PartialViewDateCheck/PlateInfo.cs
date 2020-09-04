using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialViewDateCheck
{
    public class PlateInfo
    {
        /// <summary>
        /// 同步记录ID
        /// </summary>
        public string Plate { get; set; }
        /// <summary>
        /// 盒子Ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 盒子人事名字
        /// </summary>
        public string BoxpersonName { get; set; }

        /// <summary>
        /// 中心人事名字
        /// </summary>
        public string CenterpersonName { get; set; }

        /// <summary>
        /// 中心人事编号
        /// </summary>
        public string CenterpersonNo { get; set; }
        /// <summary>
        /// 盒子人事编号
        /// </summary>
        public string BoxpersonNo { get; set; }


    }
}
