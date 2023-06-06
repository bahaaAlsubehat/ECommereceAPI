using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ref.Lect4.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Ref.Lect4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareController : ControllerBase
    {
        // We want to build Dependency injection
        private readonly OnlineStoreContext _storeContext;

        public ShareController (OnlineStoreContext storeContext)
        {
            this._storeContext = storeContext;
        }


        // We want to cache all categories to redeuse hits on APi 
        // First we want to select all Gategories 
        [HttpGet]
        [Route("GetCategories")]
        public IActionResult GetAllCategories([FromHeader]string token)
        {
            return Ok(DecodeToken(token));

            var categories = _storeContext.Category.ToList();
            return Ok(categories);
        }

        // Second we want to called all items but we want to note the there is pagination 
        // Pagination is defined all itemes arranging in pages in every category and there is Page Size and Page Number
        //Page Size is defined the amount of items in one page (Ex: 10 items in one pages )
        //Page Number is defined the number of page i need (Example I select page 5)
        [HttpGet]
        [Route("GetItems")]
        public IActionResult GetAllItems(int PageSize , int PageNumber)
        {
            var items = _storeContext.Items;  // As IEnumerable not List 
            int SkipAmount = PageSize* PageNumber - PageSize;  // skip amount : the amount I want to skip (Ex: if the Page size 10 and I want page Number 5 So Referring to equation I will start from Item Number 41)
            return Ok(items.Skip(SkipAmount).Take(PageSize));
        }

        // Select Item by ID

        [HttpGet]
        [Route("Item/{Id}")]
        public IActionResult GetItemById(int Id)
        {
            var item = _storeContext.Items.Where(y => y.ItemId == Id && y.IsAvailable== true).SingleOrDefault();
             
            if (item != null)
            {
               return Ok(item); 
            }
            else
            {
                return NotFound("The Item Is Not Found");
            }
        }

        // Search on Item 
        // We will search and Filltering referring Item Name, Price, CategoryId(Cause if the user select category name the data will search refer to catgory id) and description 
        [HttpGet]
        [Route("SearchItem")]
        public IActionResult FilterItem(int? catogryId , string? name, double? price, string? description, int PageSize , int PageNumber)
        {
            // First Step Select all Items

            var item = _storeContext.Items.ToList();

            // Second  Step We Filter on Items
            
            if (catogryId != null)
            {
                item = item.Where(x => x.CategoryId == catogryId && x.IsAvailable == true).ToList();
            }
            if (name != null)
            {
                item = item.Where(z => z.Name.Contains(name) && z.IsAvailable == true).ToList();
            }
            if (description != null)
            {
                item = item.Where(x => x.Description.Contains(description) && x.IsAvailable == true).ToList();
            }
            if (price != null)
            {
                if ( price < 50)
                {
                    item = item.Where(x => x.Price == price && x.IsAvailable == true).ToList();
                }
                if (price >=50 && price <= 100)
                {
                    item = item.Where(x => x.Price == price && x.IsAvailable == true).ToList();
                }
                if(price > 100)
                {
                    item = item.Where(x => x.Price == price && x.IsAvailable == true).ToList();
                }
            }
            // Thirs Step Select Items from Page Selected and filtering 

            int SkipAmount = PageNumber * PageSize - PageSize;
            return Ok(item.Skip(SkipAmount).Take(PageSize));
        }


        // Decoding Code Below

        [NonAction]
        public int DecodeToken(string tokenString)
        {
            String toke = "Bearer " + tokenString;
            var jwtEncodedString = toke.Substring(7);

            var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
            int UserId = Int32.Parse((token.Claims.First(c => c.Type == "Userid").Value.ToString()));
            return UserId;
        }

    }
}
