using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Models;
using WebSiteShop_Models.ViewModels;
using WebSiteShop_Utility;

namespace WebSiteShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]//определяется доступ ко всем экшнметодам
    public class ProductController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductRepository prodRepo, IWebHostEnvironment webHostEnvironment)
        {
            _prodRepo = prodRepo as IProductRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objList = _prodRepo.GetAll(includeProperties: "Category,ApplicationType");
            //foreach (Product obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //}
            return View(objList);
        }

        //Get - UPSERT
        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(u => new SelectListItem
            //{
            //    Text = u.CategoryName,
            //    Value = u.Id.ToString()
            //});
            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;
            //Product product = new Product();

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _prodRepo.GetAllDropdownList(WC.CstegoryName),
                ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicationTypeName)
            };

            if(id==null)
            {
                return View(productVM);// create new product
            }
            else
            {
                productVM.Product = _prodRepo.Find(id.GetValueOrDefault());
                if(productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files; //тут будет хранится файл (var - IFormFileCollection), улавливается самостоятельно
                string webRootPath = _webHostEnvironment.WebRootPath; //путь ка папке wwwroot
                if(productVM.Product.Id == 0)//определяем для создания или обновления вызывается
                {
                    //creating
                    string upload = webRootPath + WC.ImagePath;//полный путь к нужной папке
                    string filename = Guid.NewGuid().ToString();//для имени файла используется случайный guid (глобальный уникальный идентификатор )
                    string extension = Path.GetExtension(files[0].FileName);//расширение для файла, присваивается из файла, который уже был загружен?????

                    using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);//копируется файл в новое местоположение
                    }
                    productVM.Product.Image = filename + extension;

                    _prodRepo.Add(productVM.Product);
                    TempData[WC.Success] = "Product created";
                }
                else
                {
                    //updating

                    //AsNoTracking для того, что бы объект objFromDb не отслеживался. 
                    //т.к у нас 2 объекта Product 
                    //и нужно записать только этот _db.Product.Update(productVM.Product);
                    //var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u=>u.Id == productVM.Product.Id);
                    var objFromDb = _prodRepo.FirstOrDefault(u => u.Id == productVM.Product.Id, isTracking:false);


                    if (files.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;//полный путь к нужной папке
                        string filename = Guid.NewGuid().ToString();//для имени файла используется случайный guid (глобальный уникальный идентификатор )
                        string extension = Path.GetExtension(files[0].FileName);//расширение для файла, присваивается из файла, который уже был загружен?????
                        
                        var oldFile=Path.Combine(upload, objFromDb.Image);// ссылка на старое фото

                        if(System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);//если файл фото существует, то удаляем его
                        }
                        
                        using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);//копируется файл в новое местоположение
                        }

                        productVM.Product.Image = filename + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _prodRepo.Update(productVM.Product);
                    TempData[WC.Success] = "Product updated";
                }

                _prodRepo.Save();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _prodRepo.GetAllDropdownList(WC.CstegoryName);
            productVM.ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicationTypeName);

            TempData[WC.Error] = "Error";
            return View(productVM);
        }

        //Get - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Product product = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType).FirstOrDefault(u => u.Id == id);//eager loading
            Product product = _prodRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category,ApplicationType");
            if (product == null)
            {
                return NotFound();
            }
            return View(product);

        }

        //POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            Product product = _prodRepo.Find(id.GetValueOrDefault());
            if (product == null)
            {
                TempData[WC.Error] = "Error";
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;

            var oldFile = Path.Combine(upload, product.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }


            _prodRepo.Remove(product);
            _prodRepo.Save();
            TempData[WC.Error] = "Product deleted successfully";
            return RedirectToAction("Index");

        }
    }
}
