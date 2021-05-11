using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesAPIController : ControllerBase
    {
        private readonly AppIdentityContext _context;

        public FilesAPIController(AppIdentityContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file is not { Length: > 0 })
                return BadRequest();

            var userFile = await _context.UserFiles.FirstAsync(x=>x.Id==fileId);

            var filePath = userFile.FileName + Path.GetExtension(file.FileName);//extension uzantıyı bulmak için yani dosyaadı artı uzantı

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);


            using FileStream stream = new(path,FileMode.Create);

            await file.CopyToAsync(stream);

            //oluşturulma işlemi tamamnlanınca veritabanını güncelleme
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _context.SaveChangesAsync();
            //Signal notification oluşturulacak

            return Ok();
        }
    }
}
