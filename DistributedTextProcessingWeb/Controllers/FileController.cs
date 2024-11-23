using Microsoft.AspNetCore.Mvc;

namespace DistributedTextProcessingWeb.Controllers
{
    [Route("[controller]")]
    public class FileController : Controller
    {
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Заглушка MPI вызова (можно заменить на ваш процесс обработки)
                TempData["Result"] = $"Файл {file.FileName} успешно обработан.";

                return RedirectToAction("Result");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("Result")]
        public IActionResult Result()
        {
            var result = TempData["Result"];
            return View("Result", result);
        }
    }
}
