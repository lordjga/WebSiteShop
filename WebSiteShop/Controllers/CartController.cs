using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Models;
using WebSiteShop_Models.ViewModels;
using WebSiteShop_Utility;
using WebSiteShop_Utility.BraiTree;

namespace WebSiteShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IApplicationUserRepository _appUserRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IInquiryHeaderRepository _inqHeadRepo;
        private readonly IInquiryDetailRepository _inqDetRepo;
        private readonly IOrderDetailRepository _ordDetRepo;
        private readonly IOrderHeaderRepository  _ordHeadRepo;
        private readonly IBrainTreeGate _brain;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        [BindProperty] //привязывает свойство (автоматически получаем его, не указывая в параметрах)
        public ProductUserVM ProductUserVM { get; set; }
        public CartController(IApplicationUserRepository appUserRepo, IProductRepository prodRepo,
            IInquiryHeaderRepository inqHeadRepo, IInquiryDetailRepository inqDetRepo,
            IOrderDetailRepository ordDetRepo , IOrderHeaderRepository ordHeadRepo,
            IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IBrainTreeGate brain)
        {
            _appUserRepo = appUserRepo;
            _prodRepo = prodRepo;
            _inqHeadRepo = inqHeadRepo;
            _inqDetRepo = inqDetRepo;
            _ordHeadRepo = ordHeadRepo;
            _ordDetRepo = ordDetRepo;
            _webHostEnvironment = webHostEnvironment;
            _emailSender=emailSender;
            _brain=brain;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppingCartList.Select(i=>i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u=>prodInCart.Contains(u.Id));

            IList<Product> prodList = new List<Product>();
            foreach (var cart in shoppingCartList)
            {
                Product product = prodListTemp.FirstOrDefault(i => i.Id == cart.ProductId);
                product.TempSqFt = cart.SqFt;
                prodList.Add(product);
            }

            return View(prodList);
        }

        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            ApplicationUser applicationUser;

            if(User.IsInRole(WC.AdminRole))
            {
                if(HttpContext.Session.Get<int>(WC.SessionInquiryId)!=0)//проверка на то есть ли какой-то уже запрос
                {
                    //обрабатывается какой-то запрос, карзина загружена на основании запроса
                    InquiryHeader inquiryHeader = _inqHeadRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };
                }
                else
                {
                    //админ размещает заказ клиента, который просто пришел в магазин не используя сайт
                    applicationUser = new ApplicationUser();
                }

                
                var gateway = _brain.GetGateway();//получение ссылки на вход в шлюз платежной системы
                var clientToken = gateway.ClientToken.Generate();//создание токена клиента
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                //пользователь не является админом нужно передать информацию о нем
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                applicationUser = _appUserRepo.FirstOrDefault(u => u.Id == claim.Value);
            }

            

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser
            };

            foreach (var cart in shoppingCartList)
            {
                Product product = prodList.FirstOrDefault(i => i.Id == cart.ProductId);
                product.TempSqFt = cart.SqFt;
                ProductUserVM.ProductList.Add(product);
            }

            return View(ProductUserVM);
        }


        [HttpPost, ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM productUserVM)//оставляем параметр для примера на будущее 
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WC.AdminRole))//создание заказа
            {
                //var orderTotal = 0.0;
                //foreach (var product in productUserVM.ProductList)
                //{
                //    orderTotal += product.Price * product.TempSqFt;
                //}

                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserVM.ProductList.Sum(x=>x.TempSqFt*x.Price),
                    City = productUserVM.ApplicationUser.City,
                    StreetAddress = productUserVM.ApplicationUser.StreetAddress,
                    State = productUserVM.ApplicationUser.State,
                    PostalCode = productUserVM.ApplicationUser.PostalCode,
                    FullName = productUserVM.ApplicationUser.FullName,
                    Email = productUserVM.ApplicationUser.Email,
                    PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WC.StatusPending
                };
                _ordHeadRepo.Add(orderHeader);
                _ordHeadRepo.Save();

                foreach (var prod in productUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqFt,
                        ProductId = prod.Id,
                    };
                    _ordDetRepo.Add(orderDetail);
                }
                _ordDetRepo.Save();

                string nonceFromTheClient = collection["payment_method_nonce"];
                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = orderHeader.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true //при запросе транзакции происходит автоматическое подтверждение
                    }
                };
                var gateway = _brain.GetGateway();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if(result.Target.ProcessorResponseText == "Approved")
                {
                    orderHeader.TransactionId = result.Target.Id;
                    orderHeader.OrderStatus = WC.StatusApproved;
                }
                else
                {
                    orderHeader.OrderStatus = WC.StatusCancelled;
                }
                _ordHeadRepo.Save();
                return RedirectToAction(nameof(InquiryConfirmation), new {id=orderHeader.Id});
            }
            else//создание запроса
            {
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "Templates" + Path.DirectorySeparatorChar.ToString()
                + "Inquiry.html";
                var subject = "New Inquiry";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }
                //Name: { 0}
                //Email: { 1}
                //Phone: { 2}
                //Products { 3}

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in productUserVM.ProductList)
                {
                    productListSB.Append($" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
                }
                string messageBody = string.Format(HtmlBody,
                    productUserVM.ApplicationUser.FullName,
                    productUserVM.ApplicationUser.Email,
                    productUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString()
                    );

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now
                };

                _inqHeadRepo.Add(inquiryHeader);
                _inqHeadRepo.Save();

                foreach (var prod in productUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id,
                    };
                    _inqDetRepo.Add(inquiryDetail);
                }
                _inqDetRepo.Save();
            }
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(int id=0) //если id больше 0, то означает.
                                                           //что метод вызывается для ситуации с размещенным заказом
        {
            OrderHeader orderHeader = _ordHeadRepo.FirstOrDefault(x => x.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u=>u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            TempData[WC.Success] = "Removed successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Clear()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
