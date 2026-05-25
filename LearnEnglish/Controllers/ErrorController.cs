using LearnEnglish.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LearnEnglish.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {

        [Route("[controller]/[action]")]
        public IActionResult UserError(string msg, int type)
        {
            // 使用msg和type来构建错误视图模型
            var model = new UserErrorViewModel { Msg = msg, Type = type };
            return View(model);
            //   return RedirectToAction("UserError", "Error");
        }

    }
}
