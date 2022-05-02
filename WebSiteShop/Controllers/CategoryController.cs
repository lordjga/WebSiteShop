using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System.Collections.Generic;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_DataAccess.Repository;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Models;
using WebSiteShop_Utility;

namespace WebSiteShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _catRepo;

        //private readonly ApplicationDbContext _db;
        public CategoryController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;  
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objList = _catRepo.GetAll();
            return View(objList);
        }
        //Get - Create
        public IActionResult Create()
        {
            return View();
        }
        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]//встроенный механизм для форм ввода
                                  //в которром добавляется специальный токен защиты от взлома
                                  //и в пост происходит проверка, что этот токен все еще действителен
                                  //и безопастность данных сохранена
        public IActionResult Create(Category obj)
        {
            if(ModelState.IsValid)
            {
                _catRepo.Add(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Category created successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Error while creating category";
            return View(obj);
        }

        //Get - Edit
        public IActionResult Edit(int? id)
        {
            if(id==null || id==0)
            {
                return NotFound();
            }
            var obj = _catRepo.Find(id.GetValueOrDefault());

            if(obj==null)
                return NotFound();

            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Category edited successfully";
                return RedirectToAction("Index");
                
            }
            TempData[WC.Error] = "Category change error";
            return View(obj);
        }

        //Get - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _catRepo.Find(id.GetValueOrDefault());

            if (obj == null)
                return NotFound();

            return View(obj);
        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
                return NotFound();

            _catRepo.Remove(obj);
            _catRepo.Save();
            TempData[WC.Success] = "Category deleted successfully";
            return RedirectToAction("Index");
            
        }
    }
}
