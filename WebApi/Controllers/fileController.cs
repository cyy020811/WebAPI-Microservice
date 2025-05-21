using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Repositories;
using WebApi.Settings;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("files")]
    public class FileController : ControllerBase
    {
        private readonly S3Repository _s3Repository;

        public FileController(S3Repository s3Repository)
        {
            _s3Repository = s3Repository;
        }

        [IgnoreAntiforgeryToken]
        [HttpGet("{key}")]
        public async Task<IActionResult> GetAsync(string key)
        {
            GetObjectResponse response = await _s3Repository.DownloadAsync(key);

            return File(response.ResponseStream, response.Headers.ContentType, response.Metadata["file-name"]);
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> PostAsync(IFormFile file)
        {
            if (file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var key = await _s3Repository.UploadAsync(file);

            return Ok(key);
        }

        [IgnoreAntiforgeryToken]
        [HttpDelete("{key}")]
        public async Task<ActionResult> DeleteAsync(string key)
        {
            await _s3Repository.RemoveAsync(key);

            return Ok($"File {key} deleted successfully");
        }
    }
}

