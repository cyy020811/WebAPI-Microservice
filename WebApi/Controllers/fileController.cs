using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
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

        [HttpGet("{key}")]
        public async Task<IActionResult> GetAsync(string key)
        {
            try
            {
                GetObjectResponse response = await _s3Repository.DownloadAsync(key);

                return File(response.ResponseStream, response.Headers.ContentType, response.Metadata["file-name"]);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"File {key} is not found");
            }
        }

        [HttpGet("presigned/{key}")]
        public async Task<IActionResult> GetPreSignedURLAsync(string key)
        {
            try
            {
                var preSignedURL = await _s3Repository.GetDownloadPreSignedURL(key);
                return Ok(new { key, url = preSignedURL });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
            }
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

        [HttpPost("presigned")]
        public async Task<IActionResult> PostPreSignedURLAsync(string fileName, string contentType)
        {
            try
            {
                var preSignedURL = await _s3Repository.GetUploadPreSignedURL(fileName, contentType);
                return Ok(preSignedURL);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
            }
        }

        [HttpDelete("{key}")]
        public async Task<ActionResult> DeleteAsync(string key)
        {
            try{
                await _s3Repository.RemoveAsync(key);

                return Ok($"File {key} deleted successfully");
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
            }
        }
    }
}

