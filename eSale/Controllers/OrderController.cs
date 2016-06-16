using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eSale.Models;

namespace eSale.Controllers
{
    public class OrderController : Controller
    {

        Models.CodeService codeService = new Models.CodeService();
        private Models.Order order;

        /// <summary>
        /// 訂單管理首頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.EmpCodeData = this.codeService.GetEmp();
            return View();
        }
        
        /// <summary>
        /// 取得訂單查詢結果
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        [HttpPost()]
        public ActionResult Index(Models.OrderSearchArg arg)
        {
            ViewBag.EmpCodeData = this.codeService.GetEmp();
            Models.OrderService orderService = new Models.OrderService();
            ViewBag.SearchResult = orderService.GetOrderByCondtioin(arg);
            return View("Index");
        }


        /// <summary>
        /// 新增訂單畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult InsertOrder()
        {
            ViewBag.CustCodeData = this.codeService.GetCustomer();
            ViewBag.EmpCodeData = this.codeService.GetEmp();
            ViewBag.ShipperCodeData = this.codeService.GetShipper();
            ViewBag.ProductCodeData = this.codeService.GetProduct();
            return View(new Models.Order());
        }

        /// <summary>
        /// 新增訂單
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost()]
        public ActionResult InsertOrder(Models.Order order,Models.OrderDetails[] orderdetails)
        {
            if (ModelState.IsValid)
            {
                Models.OrderService orderService = new Models.OrderService();
                int orderid = orderService.InsertOrder(order);
                orderService.InsertOrderDetails(orderid, orderdetails);
                return RedirectToAction("Index");
                
            }
            return View(order);
            //return View();
        }

        /// <summary>
        /// 更新訂單畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult UpdateOrder(int id)
        {

            if (ModelState.IsValid)
            {
                ViewBag.EmpCodeData = this.codeService.GetEmp();
                ViewBag.CustCodeData = this.codeService.GetCustomer();
                ViewBag.ShipperCodeData = this.codeService.GetShipper();
                ViewBag.ProductCodeData = this.codeService.GetProduct();
                Models.OrderService orderService = new Models.OrderService();
                order =orderService.GetOrderById(id.ToString());
                order.OrderDetails = orderService.GetOrderDetails(id.ToString());
                ViewBag.OrderDate = string.Format("{0:yyyy-MM-dd}", order.Orderdate);
                ViewBag.RequiredDate = string.Format("{0:yyyy-MM-dd}", order.RequireDdate);
                ViewBag.ShippedDate = string.Format("{0:yyyy-MM-dd}", order.ShippedDate);
            }
            return View(order);
        }

        /// <summary>
        /// 更新訂單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateOrder(int id, Models.Order order, Models.OrderDetails[] orderdetails)
        {
            if (ModelState.IsValid)
            {
                ViewBag.CustCodeData = this.codeService.GetCustomer();
                Models.OrderService orderService = new Models.OrderService();
                orderService.UpdateOrder(order, id);
                orderService.UpdateOrderDetailById(id, orderdetails);
                return RedirectToAction("Index");

            }
            return View(order);
        }

        /// <summary>
        /// 刪除訂單
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost()]
        public JsonResult DeleteOrder(string orderId)
        {
            try
            {
                Models.OrderService orderService = new Models.OrderService();
                orderService.DeleteOrderById(orderId);
                return this.Json(true);
            }
            catch (Exception )
            {

                return this.Json(false);
            }

            
        }

        /// <summary>
        /// 取得系統時間
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSysDate()
        {
            return PartialView("_SysDatePartial");
        }
    }
}
