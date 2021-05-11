using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppIdentityContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;
      

        public ProductController(UserManager<IdentityUser> userManager, AppIdentityContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _userManager = userManager;
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1,10)}";

            UserFile userFile = new UserFile() { 
            UserId=user.Id,
            FileName=fileName,
            FileStatus=FileStatus.Creating
            };

            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();

            //RabbitMQ'ya mesaj gönder.
            _rabbitMQPublisher.Publish(new CreateExcelMessage() { 
            FileId=userFile.Id,
            
            });

            //TempData["ExcelOlusturmaBasladi"] = true;//bir requestten diğer requeste datayı taşır. Biz buradan Files actionuna gidicez bu bilgiyide yanında görütecek 

            return RedirectToAction(nameof(Files));
        }


        public async Task<IActionResult> Files()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View(await _context.UserFiles.Where(x=>x.UserId == user.Id).OrderByDescending(x=>x.Id).ToListAsync());//Kullanıcıya ait dosyaları listeler
        }


    }
}
