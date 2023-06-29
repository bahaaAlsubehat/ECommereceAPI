using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ref.Lect4.DTO;
using Ref.Lect4.Models;
using System;
using System.Xml.Linq;

namespace Ref.Lect4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly OnlineStoreContext _storeContext;

        public UserController(OnlineStoreContext storeContext)
        {
            this._storeContext = storeContext;
        }

        // Add item to cart Action:
        [HttpGet]
        [Route("AddToCart")]
        public IActionResult AddItemToCart(int itemId, int userId, int qnt, string note)
        {
            if (itemId >0 && userId >0 && qnt>0)
            {
                var cart = _storeContext.Carts.Where(x => x.UserId == userId && x.IsActive == true).SingleOrDefault();
                if (cart != null)
                {

                    // Check if the item is Exicist 
                    var item = _storeContext.Items.Where(x => x.ItemId == itemId && x.IsAvailable == true).SingleOrDefault();

                    // Check if the item already excist in cart 
                    var IsExistCartItem = _storeContext.CartItems.Where(x => x.ItemId == itemId && x.CartId == cart.CartId).SingleOrDefault();
                    if (IsExistCartItem != null)
                    {
                        if (item != null)
                        {
                            CartItemId cartitem = new CartItemId();
                            cartitem.CartId = cart.CartId;
                            cartitem.ItemId = itemId;
                            cartitem.Qtn = qnt;
                            cartitem.Note = note;
                            cartitem.NetPrice = item.Price * qnt;
                            _storeContext.Add(cartitem);
                            _storeContext.SaveChanges();

                        }
                        else
                        {
                            IsExistCartItem.Qtn += qnt;
                            _storeContext.Update(IsExistCartItem);
                            _storeContext.SaveChanges();
                        }
                    }


                }
                else
                {
                    var user = _storeContext.Users.Where(x => x.UserId == userId).FirstOrDefault();
                    if (user != null)
                    {
                        Cart cart1 = new Cart();
                        cart1.UserId = userId;
                        cart1.IsActive = true;
                        _storeContext.Add(cart1);
                        _storeContext.SaveChanges();
                        var cartnew = _storeContext.Carts.Where(x => x.UserId == userId && x.IsActive == true).SingleOrDefault();
                        if (cartnew != null)
                        {

                            // Check if the item is Exicist 
                            var item = _storeContext.Items.Where(x => x.ItemId == itemId && x.IsAvailable == true).SingleOrDefault();

                            // Check if the item already excist in cart 
                            var IsExistCartItem = _storeContext.CartItems.Where(x => x.ItemId == itemId && x.CartId == cartnew.CartId).SingleOrDefault();
                            if (IsExistCartItem != null)
                            {
                                if (item != null)
                                {
                                    CartItemId cartitem = new CartItemId();
                                    cartitem.CartId = cartnew.CartId;
                                    cartitem.ItemId = itemId;
                                    cartitem.Qtn = qnt;
                                    cartitem.Note = note;
                                    cartitem.NetPrice = item.Price * qnt;
                                    _storeContext.Add(cartitem);
                                    _storeContext.SaveChanges();
                                }
                                else
                                {
                                    IsExistCartItem.Qtn += qnt;
                                    _storeContext.Update(IsExistCartItem);
                                    _storeContext.SaveChanges();
                                }
                            }


                        }
                    }


                }
                return Ok("Added");
            }
            else
            {
                return BadRequest();
            }
            // Check if The User is Active to add to Cart
            

        }
        // Order Details
        [HttpGet]
        [Route("OrderDetails/{OrderId}")]
        public IActionResult OrderDetails(int orderid)
        {
            var order2 = _storeContext.Orders.Where(x => x.OrderId == orderid).SingleOrDefault();
            if (order2 != null)
            {
                OrderDetailsDTO ForUser = new OrderDetailsDTO();
                ForUser.OrderDate = order2.OrderDate.ToString();
                ForUser.DelivaryDate = order2.DeliveryDate.ToString();
                ForUser.IsApproved = order2.IsApproved == true ? "Approved" : order2.IsApproved == false ? "Rejected" : "Uncompleted";
                ForUser.TotalPrics = order2.TotalPrice.ToString();
                ForUser.Note = order2.Note;
                ForUser.OrderStatus = _storeContext.OrderStatuses.Where(x => x.OrderStatusId == order2.StatusId).Single().Name;
                var cart2 = _storeContext.Carts.Where(x => x.CartId == order2.CartId).ToList();
                var cartitem2 = _storeContext.CartItems.Where(x => x.CartId == order2.CartId).ToList();
                var item2 = _storeContext.Items.ToList();
                // Now We will Join between Items and order tables
                var OrderItems = from c in cart2
                                 join cit in cartitem2
                                 on c.CartId equals cit.CartId
                                 join it in item2
                                 on cit.ItemId equals it.ItemId
                                 select new OrderItemsCart
                                 {
                                     Id = it.ItemId.ToString(),
                                     Name = it.Name,
                                     Price = it.Price.ToString(),
                                     Quantity = cit.Qtn.ToString(),
                                     NetPrice = cit.NetPrice.ToString()
                                 };

                ForUser.ItemsinOrder = OrderItems.ToList();
                return Ok(ForUser);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("CheckOrderStatud/{id}")]
        public IActionResult ChckOrderStatus(int id)
        {
            var order = _storeContext.Orders.Where(x => x.OrderId == id).SingleOrDefault();
            if (order != null)
            {
                var CheckSts = _storeContext.OrderStatuses.Where(x => x.OrderStatusId == order.StatusId).First().Name;
                return Ok(CheckSts);
            }
            else
            {
                return NotFound();
            }

        }

        // Check Order 
        [HttpPost]
        [Route("CheckOrdder")]
        // We will create class in DTO Folder to used it in this action (Name : OrderDTO)
        public IActionResult CheckOrder(OrderDTO order)
        {
            var cart = _storeContext.Carts.Where(x => x.CartId == order.CartId && x.IsActive == true).SingleOrDefault();
            if (cart != null)
            {
                if (order.DeliveryDate.AddDays(-2).AddMinutes(1) > DateTime.Now)
                {
                    cart.IsActive = false;  // بما انه وصلنا مرحلة التشييك علاوردر لازم يبطل الكارت اكتيف حتآ مايعدل عليه او يضيف ايتم جديد
                    _storeContext.Update(cart);
                    _storeContext.SaveChanges();
                    Order order1 = new Order();
                    order1.CartId = order.CartId;
                    order1.Note = order.Note;
                    order1.IsApproved = false;
                    order1.DeliveryDate = order.DeliveryDate;
                    order1.OrderDate = DateTime.Now;
                    _storeContext.Add(order1);
                    _storeContext.SaveChanges();

                    return Ok("Added");


                }
                else
                {
                    return NotFound();
                }


            }
            else
            {
                return BadRequest();

            }
            return Ok();
        }

        // We want to remove one peace from item 
        [HttpPut]
        [Route("RemoveFromCart/{cartItemId}")]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartitem = _storeContext.CartItems.Where(x => x.CartItemId == cartItemId).SingleOrDefault();
            var cart = _storeContext.Carts.Where(x => x.CartId == cartitem.CartId && x.IsActive == true).SingleOrDefault();


            if (cartitem != null)
            {
                if (cart != null && cart.IsActive == true)
                {
                    if (cartitem.Qtn == 1)
                    {

                        _storeContext.Remove(cartitem);
                        _storeContext.SaveChanges();
                        return Ok();

                    }
                    else
                    {
                        cartitem.Qtn = -1;
                        cartitem.NetPrice = _storeContext.Items.Where(x => x.ItemId == cartitem.ItemId).First().Price * cartitem.Qtn;
                        _storeContext.Update(cartitem);
                        _storeContext.SaveChanges();
                        return Ok("Remove Item");

                    }
                }
                else
                {
                    return NotFound("Cart Not Found");

                }
                

            }
            else
            {
                return NotFound();
            }

        }

        // If I want to delete item with all Quantity 
        [HttpDelete]
        [Route("RemoveItemFromCart/{cartItemId}")]
        public IActionResult RemoveItemFromCart(int cartItemId)
        {
            var cartitem = _storeContext.CartItems.Where(x => x.CartItemId == cartItemId).SingleOrDefault();
            var cart = _storeContext.Carts.Where(x => x.CartId == cartitem.CartId && x.IsActive == true).SingleOrDefault();
            if (cartitem != null)
            {
                if (cart != null && cart.IsActive == true)
                {
                    _storeContext.Remove(cartitem);
                    _storeContext.SaveChanges();
                    return Ok(" Item Removed From Cart");
                }
                else
                {
                    return NotFound("Cart Not Found");

                }
               
            }
            else
            {
                return NotFound("Item Already Not Exist in Cart");
            }


        }

       

       
    }
}
