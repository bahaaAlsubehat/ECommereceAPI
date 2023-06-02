using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ref.Lect4.DTO;
using Ref.Lect4.Models;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Ref.Lect4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly OnlineStoreContext _storeContext;

        public int UserTypeId { get; private set; }

        public AdminController(OnlineStoreContext storeContext)
        {
            this._storeContext = storeContext;
        }
        
        // First Action we need to get user information for admin 
        [HttpGet]
        [Route("UserInformationList/{Id}")]
        public IActionResult GetUserInformation(int Id)
        {
           

            var users = _storeContext.User.Where(x => x.UserId == Id).ToList();
            List<UserInformationList> ListOfUser = new List<UserInformationList>();
            users.ForEach(x => { ListOfUser.Add(new UserInformationList(x.Name, x.Email, x.Phone)); });
            return Ok(ListOfUser);
            
            
        }
       
        [HttpGet]
        [Route("UserInformationList")]
        public IActionResult GetUserInformation1()
        {


            var users = _storeContext.User.Where(x => x.UserTypeId == 1).ToList();
            List<UserInformationList> ListOfUser = new List<UserInformationList>();
            users.ForEach(x => { ListOfUser.Add(new UserInformationList(x.Name, x.Email, x.Phone)); });
            return Ok(ListOfUser);


        }
       
        // Another way to get users 
        [HttpGet]
        [Route("UserInformationonList/{Id}")]
        public IActionResult GetUserInformation1(int Id)
        {
            var users = _storeContext.User.Where(x => x.UserId == Id).FirstOrDefault();
            if (users != null)
            {
                var response = new
                {
                    Name = users.Name,
                    Email = users.Email,
                    Phone = users.Phone

                };


                return Ok(response);

            }
            else
            {
                return NotFound();

            }
            
        }
       
        [HttpGet]
        [Route("GetOrders")]
        public IActionResult SearchOrders(int PageSize, int PageNumber, int? orderId, int? statusId, bool? isApproved , DateTime? deliveryDatefrom , DateTime? deliveryDateto , DateTime? orderDatefrom, DateTime? orderDateto)
        {
            var orders = _storeContext.Order.ToList();
            if (orderId != null)
            {
                orders = _storeContext.Order.Where(x => x.OrderId == orderId).ToList();
            }
            if(statusId != null)
            {
                orders = _storeContext.Order.Where(x => x.StatusId == statusId).ToList();
            }
            if(isApproved != null) 
            {
                orders = _storeContext.Order.Where(x => x.IsApproved == isApproved).ToList();

            }
            // DeliveryDate
            if (deliveryDatefrom != null && deliveryDateto == null)
            {
                orders = _storeContext.Order.Where(x => x.DeliveryDate >= deliveryDatefrom).ToList();

            }
            if (deliveryDatefrom == null && deliveryDateto != null)
            {
                orders = _storeContext.Order.Where(x => x.DeliveryDate <= deliveryDateto).ToList();

            }
            if (deliveryDatefrom != null && deliveryDateto != null)
            {
                orders = _storeContext.Order.Where(x => x.DeliveryDate >= deliveryDatefrom && x.DeliveryDate <= deliveryDateto).ToList();

            }
            // 
            if(orderDatefrom != null && orderDateto == null)
            {
                orders = _storeContext.Order.Where(x => x.OrderDate >= orderDatefrom).ToList();

            }
            if (orderDatefrom == null && orderDateto != null)
            {
                orders = _storeContext.Order.Where(x => x.OrderDate <= orderDateto).ToList();

            }
            if (orderDatefrom != null && orderDateto != null)
            {
                orders = _storeContext.Order.Where(x => x.OrderDate >= orderDatefrom && x.OrderDate <= orderDateto).ToList();

            }

            int SkipAmount = PageSize * PageNumber - PageSize;
            return Ok(orders.Skip(SkipAmount).Take(PageSize));
        }
       
        [HttpGet]
        [Route("GetOrderDetails/{orderid}")]
        public IActionResult OrderDetails(int orderid)
        {
            var order = _storeContext.Order.Where(x => x.OrderId == orderid).SingleOrDefault();
            if (order != null)
            {
                OrderDetailsForAdmin ForAdmin = new OrderDetailsForAdmin(); // Create object
                // We will fetch the data
                ForAdmin.OrderDate = order.OrderDate.ToString();
                ForAdmin.Note = order.Note;
                ForAdmin.DelivaryDate = order.DeliveryDate.ToString();
                ForAdmin.TotalPrics = order.TotalPrice.ToString();
                ForAdmin.OrderStatus = _storeContext.OrderStatus.Where(x => x.OrderStatusId == order.StatusId).Single().Name;
                ForAdmin.IsApproved = order.IsApproved == true ? "Approved" : order.IsApproved == false ? "Rejected" : "Uncompleted"; // because we defined the IsApproved in DTO as string

                var cart = _storeContext.Cart.Where(x => x.CartId == order.CartId).ToList();
                var cartitem = _storeContext.CartItem.Where(x => x.CartId == order.CartId).ToList();
                var item = _storeContext.Items.ToList();

                var OrderItems = from c in cart
                                 join ci in cartitem
                                 on c.CartId equals ci.CartId
                                 join i in item
                                 on ci.ItemId equals i.ItemId
                                 select new OrderItemsCart
                                 {
                                     Id = i.ItemId.ToString(),
                                     Name = i.Name,
                                     Price = i.Price.ToString(),
                                     Quantity = ci.Qtn.ToString(),
                                     NetPrice = ci.NetPrice.ToString()
                                 };

                ForAdmin.ItemsinOrder = OrderItems.ToList();
                var user = _storeContext.User.Where(x => x.UserId == cart.ElementAt(0).UserId).SingleOrDefault();
                ForAdmin.UserInfo = new UserInformationList(user.Name, user.Email, user.Phone);
                return Ok(ForAdmin);
            }
            else
            {
                return NotFound();
            }
            

            
        }

        [HttpPost]
        [Route("CreateItem")]
        public IActionResult CreateItem(createitemDTO ItemCreated)
        {
            var cateogory = _storeContext.Category.Where(x => x.CategoryId == ItemCreated.catigoryid).SingleOrDefault();
            if (cateogory != null)
            {
                Items item = new Items();
                item.ItemId = ItemCreated.itemid;
                item.Price = ItemCreated.price;
                item.Description = ItemCreated.description;
                item.Name = ItemCreated.name;
                item.Qtn = ItemCreated.qnt;
                item.IsAvailable = true;
                _storeContext.Add(item);
                _storeContext.SaveChanges();
                return Ok("Item has been added");
            }
            else
            {
                return NotFound("The cateogry is not found");
            }

        }

        // There is another way to create an Action:
        //public record createitemdto(int cateogoryid, double price, string description, string name, int qnt, bool isavailable); // This record method alternative DTO class 
        //[HttpPost]

        //public async Task<IActionResult> CreateItem(createitemdto itemtocreate)
        //{
        //    if (!_storeContext.Category.Any(x => x.CategoryId == itemtocreate.cateogoryid))  // Validation
        //        return BadRequest("The Category is not Exist");

        //    if (itemtocreate.price < 0 || itemtocreate.name.IsNullOrEmpty() || itemtocreate.description.IsNullOrEmpty() || itemtocreate.qnt < 0)
        //      return BadRequest("Recheck you Parameters");

        //    Items item = new();
        //    item.CategoryId = itemtocreate.cateogoryid;
        //    item.Price = itemtocreate.price;
        //    item.Description = itemtocreate.description;
        //    item.Name = itemtocreate.name;
        //    item.Qtn = itemtocreate.qnt;
        //    item.IsAvailable = itemtocreate.isavailable;

        //    _storeContext.Add(item);
        //    await _storeContext.SaveChangesAsync();
        //    return Ok($"The item Has Been Added with Name {item.Name}");
        //}

        [HttpPut]
        [Route("ManageOrder/{orderid}/{flag}/{adminid}")]
        public IActionResult ManageOrder(int orderid, string flag, int adminid)
                {
                    var order = _storeContext.Order.Where(x => x.OrderId == orderid).SingleOrDefault();
                    if (order != null)
                    {
                        var admin = _storeContext.User.Where(x => x.UserId == adminid && UserTypeId == 2).SingleOrDefault();
                        if (admin != null)
                        {
                            if (flag != null)
                            {
                                order.ApprovedBy = adminid;
                                order.StatusId = 2;
                                order.IsApproved = true;
                            }
                            else
                            {
                                order.ApprovedBy = adminid;
                                order.StatusId = 3;
                                order.IsApproved = false;
                            }
                            _storeContext.Update(order);
                            _storeContext.SaveChanges();
                            return Ok("Done");

                        }
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }

        [HttpDelete]
        [Route("DeleteItem/{Id}")]
        public IActionResult DeleteItem(int Id)
        {
            var item = _storeContext.Items.Where(x => x.ItemId == Id && x.IsAvailable == true).SingleOrDefault(); // check item
            if (item != null)
            {
                var checkcart = _storeContext.Cart.Where(x => x.IsActive == true).ToList(); // Check if the cart is active to check if the item exist in cart to remove it 
                foreach (var c in checkcart )
                {
                    var checkcaeritem = _storeContext.CartItem.Where(z => z.CartId == c.CartId && z.ItemId == Id).SingleOrDefault();  // wh want to check if the item exist in cart
                    if (checkcaeritem != null)
                    {
                        _storeContext.Remove(item);
                        _storeContext.SaveChanges();
                    }
                    else
                    {
                       continue;
                    }
                }
                item.IsAvailable = false;
                _storeContext.Update(item);
                _storeContext.SaveChanges();
                return Ok("Updated");
            }
            else
            {
                return NotFound();
            }
            
        }

        // I want to use delete item in another way 
        //public record DeleteItem1(int itemid);
        //[HttpDelete]
        //public async Task<IActionResult> NonAvailableItem([FromRoute]int itemid)
        //{
        //    var item1 = _storeContext.Items.Where(x => x.ItemId == itemid).SingleOrDefault();
        //    if (item1 != null)
        //    {
        //        var checkcart1 = _storeContext.Cart.Where(z => z.IsActive == true).ToList();
        //        foreach( var c1 in checkcart1)
        //        {
        //            var checkcartitem1 = _storeContext.CartItem.Where(y => y.CartId == c1.CartId && y.ItemId == itemid).SingleOrDefault();
        //            if(checkcartitem1 != null)
        //            {
        //                _storeContext.Remove(item1);
        //                _storeContext.SaveChanges();

        //            }
        //            else
        //            {
        //                continue;
        //            }
        //        }
        //        item1.IsAvailable = false;
        //        _storeContext.Update(item1);
        //        _storeContext.SaveChanges();
        //        return Ok("Updated");
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}


       public record UpdateItemDTO(int itemid, int categoryid, double price, string description, string name, int qnt, bool isavailable);

        [HttpPut]
        public async Task<IActionResult> UpdateItem(UpdateItemDTO itemupdated)
        {
            var item = _storeContext.Items.SingleOrDefault(x => x.ItemId == itemupdated.itemid);
            if (item != null)
            {
                item.Qtn = itemupdated.qnt > 0 ? itemupdated.qnt : item.Qtn;
                item.Price = itemupdated.price > 0 ? itemupdated.price : item.Price;
                item.Description = itemupdated.description != null ? itemupdated.description : item.Description;
                item.Name = itemupdated.name != null ? itemupdated.name : item.Name;
                item.IsAvailable = itemupdated.isavailable == false ? itemupdated.isavailable : true;
                _storeContext.Update(item);
               await  _storeContext.SaveChangesAsync();
                return Ok($"The item has been updated with itemID {item.ItemId}");
            }
            else
            {
                return Ok("Item Not Found");
            }
        }
    }
}
