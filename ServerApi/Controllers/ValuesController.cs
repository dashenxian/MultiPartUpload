using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Runtime.Caching;

namespace ServerApi.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [System.Web.Http.HttpPost()]
        [System.Web.Http.Route("api/upload")]
        public async Task<ActionResult> Upload()
        {
            var file = HttpContext.Current.Request.Files[0];
            var fileId = HttpContext.Current.Request.Form["fileId"];
            var chunkIndex = int.Parse(HttpContext.Current.Request.Form["chunkIndex"]);
            var totalChunks = int.Parse(HttpContext.Current.Request.Form["totalChunks"]);

            CacheHelper.Cache.Set(fileId, "myvalue", TimeSpan.FromMinutes(10));

            var filePath = Path.Combine(HostingEnvironment.MapPath("~/uploads"), fileId, Path.GetFileNameWithoutExtension(file.FileName), $"{chunkIndex}.tmp");
            var path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.InputStream.CopyToAsync(fileStream);
            }

            if (Directory.GetFiles(path).Length != totalChunks)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Accepted); // 返回 Accepted 状态码
            }

            filePath = Path.Combine(HostingEnvironment.MapPath("~/uploads"), file.FileName);
            using (var output = new FileStream(filePath, FileMode.Create))
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    var tempFile = Path.Combine(HostingEnvironment.MapPath("~/uploads"), fileId, Path.GetFileNameWithoutExtension(file.FileName), $"{i}.tmp");
                    using (var input = new FileStream(tempFile, FileMode.Open))
                    {
                        await input.CopyToAsync(output);
                    }
                    //System.IO.File.Delete(tempFile);
                }
            }
            Directory.Delete(path, true);
            return new HttpStatusCodeResult(HttpStatusCode.OK);

        }
    }
}
