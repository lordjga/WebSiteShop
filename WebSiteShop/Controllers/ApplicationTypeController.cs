using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Models;
using WebSiteShop_Utility;

namespace WebSiteShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        
        private readonly IApplicationTypeRepository _appTypeRepo;
        public ApplicationTypeController(IApplicationTypeRepository appTypeRepo)
        {
            _appTypeRepo = appTypeRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<ApplicationType> obj = _appTypeRepo.GetAll();
            return View(obj);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Create(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                _appTypeRepo.Add(obj);
                _appTypeRepo.Save();
                TempData[WC.Success] = "Application Type created successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Error while creating Application Type";
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _appTypeRepo.Find(id.GetValueOrDefault());

            if (obj == null)
                return NotFound();

            return View(obj);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Edit(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                _appTypeRepo.Update(obj);
                _appTypeRepo.Save(); 
                TempData[WC.Success] = "Application Type edited successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Application Type change error";
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _appTypeRepo.Find(id.GetValueOrDefault());

            if (obj == null)
                return NotFound();

            return View(obj);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult DeletePost(ApplicationType obj)
        {
            _appTypeRepo.Remove(obj);
            _appTypeRepo.Save();
            TempData[WC.Success] = "Application Type deleted successfully";
            return RedirectToAction("Index");
        
        }
    }
}
