using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eSale.Models
{
    public class OrderDetails
    {
        /// <summary>
        /// 訂單編號
        /// </summary>
        public int OrderId {get;set;}

        /// <summary>
        /// 產品代號
        /// </summary>
        public int ProductId {get;set;}

        /// <summary>
        /// 單價
        /// </summary>
        public int UnitPrice {get;set;}

        /// <summary>
        /// 數量
        /// </summary>
        public decimal Qty {get;set;}

        /// <summary>
        /// 折扣
        /// </summary>
        public int Discount {get;set;}

        /*/// <summary>
        /// 以選代號
        /// </summary>
        public int SelectedProductId { get; set; }
        */
    }
}