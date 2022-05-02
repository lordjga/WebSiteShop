using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Models;
using WebSiteShop_Models.ViewModels;
using WebSiteShop_Utility;
using WebSiteShop_Utility.BraiTree;

namespace WebSiteShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderDetailRepository _ordDetRepo;
        private readonly IOrderHeaderRepository _ordHeadRepo;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IOrderDetailRepository ordDetRepo, IOrderHeaderRepository ordHeadRepo,
            IBrainTreeGate brain)
        {
            _ordHeadRepo = ordHeadRepo;
            _ordDetRepo = ordDetRepo;
            _brain = brain;
        }
        public IActionResult Index(string searchName=null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _ordHeadRepo.GetAll(),
                StatusList = WC.listStatus.ToList().Select(i=>new SelectListItem
                {
                    Text  = i,
                    Value = i
                })
            };

            if (!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }
            if (!string.IsNullOrEmpty(Status) && Status!= "--Order Status--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }

            return View(orderListVM);
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _ordHeadRepo.FirstOrDefault(u=>u.Id==id),
                OrderDetail = _ordDetRepo.GetAll(o=>o.OrderHeaderId==id, includeProperties:"Product")
            };

            return View(OrderVM);
        }
        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _ordHeadRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusProcessing;
            _ordHeadRepo.Save();
            TempData[WC.Success] = "Order is in process";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _ordHeadRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _ordHeadRepo.Save();
            TempData[WC.Success] = "Order shipped successfully";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _ordHeadRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            var gateway = _brain.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if (transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            { //не нужно возвращать деньги 
                Result<Transaction> resultVoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else 
            {
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }
            orderHeader.OrderStatus = WC.StatusRefunded;
            _ordHeadRepo.Save();
            TempData[WC.Success] = "Order canseled successfully";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            OrderHeader orderHeader = _ordHeadRepo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            
            orderHeader.FullName = OrderVM.OrderHeader.FullName;
            orderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeader.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeader.City = OrderVM.OrderHeader.City;
            orderHeader.State = OrderVM.OrderHeader.State;
            orderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;
            orderHeader.Email = OrderVM.OrderHeader.Email;

            _ordHeadRepo.Save();

            TempData[WC.Success] = "Order details update successfully";
            return RedirectToAction("Details", "Order", new {id=orderHeader.Id});
        }
    }
}
