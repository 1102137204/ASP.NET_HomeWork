using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace eSale.Models
{
    public class OrderService
    {
        /// <summary>
        /// 取得DB連線字串
        /// </summary>
        /// <returns></returns>
        private string GetDBConnectionString()
        {
            return
                System.Configuration.ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString.ToString();
        }

        /// <summary>
        /// 新增訂單
        /// </summary>
        /// <param name="order"></param>
        /// <returns>訂單編號</returns>
        public int InsertOrder(Models.Order order)
        {
            string sql = @" Insert INTO Sales.Orders
						 (
							CustomerID,EmployeeID,orderdate,requireddate,shippeddate,shipperid,freight,
							shipname,shipaddress,shipcity,shipregion,shippostalcode,shipcountry
						)
						VALUES
						(
							@custid,@empid,@orderdate,@requireddate,@shippeddate,@shipperid,@freight,
							@shipname,@shipaddress,@shipcity,@shipregion,@shippostalcode,@shipcountry
						)
						Select SCOPE_IDENTITY()
						";
            int orderId;
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@custid", order.CustId == null ? string.Empty : order.CustId));
                cmd.Parameters.Add(new SqlParameter("@empid", order.EmpId == -1 ? -1 : order.EmpId));
                cmd.Parameters.Add(new SqlParameter("@orderdate", order.Orderdate == null ? DateTime.Now : order.Orderdate));
                cmd.Parameters.Add(new SqlParameter("@requireddate", order.RequireDdate == null ? DateTime.Now : order.RequireDdate));
                cmd.Parameters.Add(new SqlParameter("@shippeddate", order.ShippedDate == null ? DateTime.Now : order.ShippedDate));
                cmd.Parameters.Add(new SqlParameter("@shipperid", order.ShipperId));
                cmd.Parameters.Add(new SqlParameter("@freight", order.Freight == -1 ? -1 : order.Freight));
                cmd.Parameters.Add(new SqlParameter("@shipname", order.ShipperName == null ? string.Empty : order.ShipperName));
                cmd.Parameters.Add(new SqlParameter("@shipaddress", order.ShipAddress == null ? string.Empty : order.ShipAddress));
                cmd.Parameters.Add(new SqlParameter("@shipcity", order.ShipCity == null ? string.Empty : order.ShipCity));
                cmd.Parameters.Add(new SqlParameter("@shipregion", order.ShipRegion == null ? string.Empty : order.ShipRegion));
                cmd.Parameters.Add(new SqlParameter("@shippostalcode", order.ShipPostalCode == null ? string.Empty : order.ShipPostalCode));
                cmd.Parameters.Add(new SqlParameter("@shipcountry", order.ShipCountry == null ? string.Empty : order.ShipCountry));

                orderId = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
            }
            return orderId;

        }
        /// <summary>
        /// 新增明細
        /// </summary>
        /// <returns></returns>
        internal void InsertOrderDetails(int orderid, OrderDetails[] orderdetails)
        {
            string sql = @" Insert INTO Sales.OrderDetails
						 (
							OrderID,ProductID,UnitPrice,Qty,Discount
						)
						VALUES
						(
							@orderid,@proid,@unit,@qty,@disc
						)
						";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                for (int i = 0; i < orderdetails.Count(); i++)
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@orderid", orderid));
                    cmd.Parameters.Add(new SqlParameter("@proid", orderdetails[i].ProductId));
                    cmd.Parameters.Add(new SqlParameter("@unit", orderdetails[i].UnitPrice));
                    cmd.Parameters.Add(new SqlParameter("@qty", orderdetails[i].Qty));
                    cmd.Parameters.Add(new SqlParameter("@disc", 0.1));
                    cmd.ExecuteNonQuery();

                }
                conn.Close();
            }
        }

        /// <summary>
        /// 依照Id 取得訂單資料
        /// </summary>
        /// <returns></returns>
        public Models.Order GetOrderById(string orderId)
        {
            DataTable dt = new DataTable();
            string sql = @"SELECT 
					A.OrderId,A.CustomerID,B.Companyname As CustName,
					A.EmployeeID,C.lastname+ C.firstname As EmpName,
					A.Orderdate,A.RequireDdate,A.ShippedDate,
					A.ShipperId,D.companyname As ShipperName,A.Freight,
					A.ShipName,A.ShipAddress,A.ShipCity,A.ShipRegion,A.ShipPostalCode,A.ShipCountry
					From Sales.Orders As A 
					INNER JOIN Sales.Customers As B ON A.CustomerID=B.CustomerID
					INNER JOIN HR.Employees As C On A.EmployeeID=C.EmployeeID
					inner JOIN Sales.Shippers As D ON A.shipperid=D.shipperid
					Where  A.OrderId=@OrderId";


            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderId));

                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            return this.MapOrderDataToList(dt).FirstOrDefault();
        }
        /// <summary>
        /// 依照條件取得訂單資料
        /// </summary>
        /// <returns></returns>
        public List<Models.Order> GetOrderByCondtioin(Models.OrderSearchArg arg)
        {

            DataTable dt = new DataTable();
            string sql = @"SELECT 
					A.OrderID,A.CustomerID,B.CompanyName As CustName,
					A.EmployeeID,C.LastName+ C.FirstName As EmpName,
					A.OrderDate,A.RequireDdate,A.ShippedDate,
					A.ShipperId,D.CompanyName As ShipperName,A.Freight,
					A.ShipName,A.ShipAddress,A.ShipCity,A.ShipRegion,A.ShipPostalCode,A.ShipCountry
					From Sales.Orders As A 
					INNER JOIN Sales.Customers As B ON A.CustomerID=B.CustomerID
					INNER JOIN HR.Employees As C On A.EmployeeID=C.EmployeeID
					inner JOIN Sales.Shippers As D ON A.shipperid=D.ShipperID
					Where (A.OrderID = @OrderId Or @OrderId='') And
                          (B.CompanyName Like '%'+@CustName+'%'Or @CustName='') And
                          (A.EmployeeID = @EmpId Or @EmpId=-1) And 
						  (A.OrderDate=@Orderdate Or @Orderdate='') ";


            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", arg.OrderId == null ? string.Empty : arg.OrderId));
                cmd.Parameters.Add(new SqlParameter("@CustName", arg.CustName == null ? string.Empty : arg.CustName));
                cmd.Parameters.Add(new SqlParameter("@Orderdate", arg.OrderDate == null ? string.Empty : arg.OrderDate));
                cmd.Parameters.Add(new SqlParameter("@EmpId", arg.EmpId == -1 ? -1 : arg.EmpId));
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }


            return this.MapOrderDataToList(dt);
        }
        /// <summary>
        /// 刪除訂單
        /// </summary>
        public void DeleteOrderById(string orderId)
        {
            try
            {
                string sql = "Delete FROM Sales.OrderDetails Where orderid=@orderid;Delete FROM Sales.Orders Where orderid=@orderid";
                using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@orderid", orderId));
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        /// <summary>
        /// 更新訂單
        /// </summary>
        /// <param name="order"></param>
        public void UpdateOrder(Models.Order order, int id)
        {
            string sql = @"Update 
							Sales.Orders SET
							CustomerID=@custid,EmployeeID=@empid,
							orderdate=@orderdate,requireddate=@requireddate,
							shippeddate=@shippeddate,shipperid=@shipperid,
							freight=@freight,shipname=@shipname,
							shipaddress=@shipaddress,shipcity=@shipcity,
							shipregion=@shipregion,shippostalcode=@shippostalcode,
							shipcountry=@shipcountry
							WHERE orderid=@orderid";

            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@custid", order.CustId == null ? string.Empty : order.CustId));
                cmd.Parameters.Add(new SqlParameter("@empid", order.EmpId == -1 ? -1 : order.EmpId));
                cmd.Parameters.Add(new SqlParameter("@orderdate", order.Orderdate == null ? DateTime.Now : order.Orderdate));
                cmd.Parameters.Add(new SqlParameter("@requireddate", order.RequireDdate == null ? DateTime.Now : order.RequireDdate));
                cmd.Parameters.Add(new SqlParameter("@shippeddate", order.ShippedDate == null ? DateTime.Now : order.ShippedDate));
                cmd.Parameters.Add(new SqlParameter("@shipperid", order.ShipperId));
                cmd.Parameters.Add(new SqlParameter("@freight", order.Freight == -1 ? -1 : order.Freight));
                cmd.Parameters.Add(new SqlParameter("@shipname", order.ShipperName == null ? string.Empty : order.ShipperName));
                cmd.Parameters.Add(new SqlParameter("@shipaddress", order.ShipAddress == null ? string.Empty : order.ShipAddress));
                cmd.Parameters.Add(new SqlParameter("@shipcity", order.ShipCity == null ? string.Empty : order.ShipCity));
                cmd.Parameters.Add(new SqlParameter("@shipregion", order.ShipRegion == null ? string.Empty : order.ShipRegion));
                cmd.Parameters.Add(new SqlParameter("@shippostalcode", order.ShipPostalCode == null ? string.Empty : order.ShipPostalCode));
                cmd.Parameters.Add(new SqlParameter("@shipcountry", order.ShipCountry == null ? string.Empty : order.ShipCountry));
                cmd.Parameters.Add(new SqlParameter("@orderid", id));
                cmd.ExecuteNonQuery();
                conn.Close();
            }

        }

        /// <summary>
        /// 更新訂單明細
        /// </summary>
        public void UpdateOrderDetailById(int orderId, Models.OrderDetails[] orderdetails)
        {
            try
            {
                string sql = @"Delete FROM Sales.OrderDetails Where orderid=@orderid;";
                using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@orderid", orderId));
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                sql = @" Insert INTO Sales.OrderDetails
						 (
							OrderID,ProductID,UnitPrice,Qty,Discount
						)
						VALUES
						(
							@orderid,@proid,@unit,@qty,@disc
						)
						";
                using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
                {
                    conn.Open();
                    for (int i = 0; i < orderdetails.Count(); i++)
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.Add(new SqlParameter("@orderid", orderId));
                        cmd.Parameters.Add(new SqlParameter("@proid", orderdetails[i].ProductId));
                        cmd.Parameters.Add(new SqlParameter("@unit", orderdetails[i].UnitPrice));
                        cmd.Parameters.Add(new SqlParameter("@qty", orderdetails[i].Qty));
                        cmd.Parameters.Add(new SqlParameter("@disc", 0.1));
                        cmd.ExecuteNonQuery();

                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        /// <summary>
        /// 依照Id 取得訂單明細
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<Models.OrderDetails> GetOrderDetails(string orderId)
        {

            DataTable dt = new DataTable();
            string sql = @"SELECT 
					OrderID, ProductID, UnitPrice, Qty, Discount
					From Sales.OrderDetails
					Where OrderId=@OrderId";


            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderId));

                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            return this.MapOrderDataDetailToList(dt);
        }

        private List<Models.Order> MapOrderDataToList(DataTable orderData)
        {
            List<Models.Order> result = new List<Order>();


            foreach (DataRow row in orderData.Rows)
            {
                result.Add(new Order()
                {
                    CustId = row["CustomerID"].ToString(),
                    CustName = row["CustName"].ToString(),
                    EmpId = (int)row["EmployeeID"],
                    EmpName = row["EmpName"].ToString(),
                    Freight = (decimal)row["Freight"],
                    Orderdate = row["Orderdate"] == DBNull.Value ? (DateTime?)null : (DateTime)row["Orderdate"],
                    OrderId = (int)row["OrderId"],
                    RequireDdate = row["RequireDdate"] == DBNull.Value ? (DateTime?)null : (DateTime)row["RequireDdate"],
                    ShipAddress = row["ShipAddress"].ToString(),
                    ShipCity = row["ShipCity"].ToString(),
                    ShipCountry = row["ShipCountry"].ToString(),
                    ShipName = row["ShipName"].ToString(),
                    ShippedDate = row["ShippedDate"] == DBNull.Value ? (DateTime?)null : (DateTime)row["ShippedDate"],
                    ShipperId = (int)row["ShipperId"],
                    ShipperName = row["ShipperName"].ToString(),
                    ShipPostalCode = row["ShipPostalCode"].ToString(),
                    ShipRegion = row["ShipRegion"].ToString()
                });
            }
            return result;
        }
        private List<Models.OrderDetails> MapOrderDataDetailToList(DataTable orderDetailData)
        {
            List<Models.OrderDetails> result = new List<OrderDetails>();


            foreach (DataRow row in orderDetailData.Rows)
            {
                result.Add(new OrderDetails()
                {
                    OrderId = Convert.ToInt32(row["OrderID"]),
                    ProductId = Convert.ToInt32(row["ProductID"]),
                    UnitPrice = Convert.ToInt32(row["UnitPrice"]),
                    Qty = Convert.ToDecimal(row["Qty"]),
                    Discount = Convert.ToInt32(row["Discount"])
                });
            }
            return result;
        }
    }
        
}
