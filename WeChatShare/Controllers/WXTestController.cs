using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeChatShare.Models;

namespace WeChatShare.Controllers
{
    public class WXTestController : Controller
    {
        // GET: WXTest
        public ActionResult Index()
        {
            string url = Request.Url.ToString();
            WXRequestModel model = new WXRequestModel();
            //model.AppID = "wxd2a8a39ae0787c30";
            //model.AppSecret = "9e00204f95f7d060401d72dcf75d7a3e";
            model.AppID = "wx24f55a573d938afb";
            model.AppSecret = "279ab0e4bb200b692a358ba3ef2c2088";
            model.TimeStamp = WXRequestModel.GetTimeStamp();
            model.NonceStr = WXRequestModel.GetRandomString();
            model.Signature = WXRequestModel.GetSignature(model.AppID, model.AppSecret, model.NonceStr, model.TimeStamp, url);
            return View(model);
        }
    }
}