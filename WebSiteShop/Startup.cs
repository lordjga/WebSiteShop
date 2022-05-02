using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_DataAccess.Inicializer;
using WebSiteShop_DataAccess.Repository;
using WebSiteShop_DataAccess.Repository.IRepository;
using WebSiteShop_Utility;
using WebSiteShop_Utility.BraiTree;

namespace WebSiteShop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSession(Options =>
            {
                Options.IdleTimeout = TimeSpan.FromMinutes(10);
                Options.Cookie.HttpOnly = true;
                Options.Cookie.IsEssential = true;
            });
            services.Configure<BrainTreeSettings>(Configuration.GetSection("BrainTree"));//берем настройки из appsettings и
                                                                                         //совмещаем с BrainTreeSettings (так правильнее)
                                                                                         //в мэйлджет же мы делали это внутри класса
            services.AddSingleton<IBrainTreeGate, BrainTreeGate>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IApplicationTypeRepository, ApplicationTypeRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<IInquiryHeaderRepository, InquiryHeaderRepository>();
            services.AddScoped<IInquiryDetailRepository, InquiryDetailRepository>();
            services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

            //services.AddScoped(typeof(IRepository<>),typeof(Repository<>));

            services.AddScoped<IDbInicializer, DbInicializer>();

            services.AddAuthentication().AddFacebook(Options =>
            {
                Options.AppId = "394883935610029";
                Options.AppSecret = "ddd2320ad1ee09d6e0e92a6881948d39";
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInicializer dbInicializer)
        {
            if (env.IsDevelopment())//проверяется режим запуска приложения
            {
                app.UseDeveloperExceptionPage();//компонент развернутого анализа исключений
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // упрощенное представление о сбое представления
                app.UseHsts();
            }
            app.UseHttpsRedirection();//определяется переход на защищенное соединение
            app.UseStaticFiles();//добавляем доступ к статическим файлам

            app.UseRouting();//промежуточное ПО для маршрутизации
            app.UseAuthentication();
            app.UseAuthorization();//включение авторизации
            dbInicializer.Inicialize(); //инициализация при пустой бд
            app.UseSession();//обработка сеансов

            app.UseEndpoints(endpoints =>//конечные точки, настраиваем маршрут по умолчанию для этого приложения (бывают разные mvc/razor)
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
