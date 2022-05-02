using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Models;

namespace WebSiteShop_DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Category.Update(obj);

            //var objFromDb = _db.Category.FirstOrDefault(u=>u.Id==obj.Id);
            //if (objFromDb != null)
            //{
            //    objFromDb.CategoryName = obj.CategoryName;
            //    objFromDb.DisplayOrder = obj.DisplayOrder;
            //}
        }
    }
}
