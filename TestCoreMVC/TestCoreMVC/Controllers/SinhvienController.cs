using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestCoreMVC.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestCoreMVC.Controllers
{
    public class SinhvienController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            ViewData["ten"] = "Nguyễn Xuân Tùng";
            ViewData["maso"] = "015234567";

            return View();
        }
        
        public IActionResult XemDiem()
        {
            ViewData["Khoa"] = new Khoa()
            {
                MaKhoa = "MK01",
                TenKhoa = "Hệ thống thông tin"
            };
            return View();
        }
        public string XemDiem1(string name, double diem = 9, string ID = "1")
        {
            return HtmlEncoder.Default.Encode($"ID {ID} Ten {name} Diem trung binh {diem}");
        }
        public string XemDiem_temp()
        {
            return "Đây là  trang web xem điểm sinh viên";
        }

    }
}
