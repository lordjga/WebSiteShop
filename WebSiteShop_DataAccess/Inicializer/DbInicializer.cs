using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSiteShop_DataAccess.Data;
using WebSiteShop_Models;
using WebSiteShop_Utility;

namespace WebSiteShop_DataAccess.Inicializer
{
    public class DbInicializer : IDbInicializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInicializer(ApplicationDbContext db, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Inicialize()
        {
            //проверяем наличие незавершенных миграций
            try
            {
                if (_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

                
            }

            //если в базе нету роли админа, значит надо добавить роль админа и клиента
            if (!_roleManager.RoleExistsAsync(WC.AdminRole).GetAwaiter().GetResult()) 
                //.GetAwaiter() - (вместо await), Получает объект типа awaiter,
                //используемый для данного объекта Task (Представляет асинхронную операцию).
                //Возвращаемое значение - TaskAwaiter (Предоставляет объект, который ожидает завершения асинхронной задачи.)
                //.GetResult() - Завершает ожидание завершения асинхронной задачи
                //дальнейшее продолжение кода будет, только после получения результата
            {
                _roleManager.CreateAsync(new IdentityRole(WC.AdminRole)).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole(WC.CastomerRole)).GetAwaiter().GetResult();
            }
            else
            {
                return;
            }

            //создаем админа
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                FullName = "Admin Tester",
                PhoneNumber = "11111111"
            }, "_8782125Aa").GetAwaiter().GetResult();

            ApplicationUser user = _db.ApplicationUser.FirstOrDefault(u => u.Email == "admin@admin.com");
            _userManager.AddToRoleAsync(user, WC.AdminRole).GetAwaiter().GetResult();
        }
    }
}
