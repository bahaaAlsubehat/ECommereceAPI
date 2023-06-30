using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ref.Lect4.Helper;
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
        private readonly IConfiguration _configuration;
        private readonly OnlineStoreHelper _helper;

        public ShareController (OnlineStoreContext storeContext, IConfiguration configuration)
        {
            this._storeContext = storeContext;
            _configuration = configuration;
            _helper = new OnlineStoreHelper(_storeContext, _configuration);
            
        }


        // We want to cache all categories to redeuse hits on APi 
        // First we want to select all Gategories 
        [HttpGet]
        [Route("GetCategories")]
        public IActionResult GetAllCategories([FromHeader]string token)
        {
            //return Ok(DecodeToken(token));
            if(_helper.ValidateJWTtoken(token))
            {
                var categories = _storeContext.Categories.ToList();
                return Ok(categories);
            }
            return Unauthorized("Invalide Token");
           
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


    }
}
