using System.Linq.Expressions;
using System.Reflection.Metadata;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using WebApi.Settings;

namespace WebApi.Repositories
{

    public class S3Repository
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IOptions<S3Settings> _s3Settings;

        public S3Repository(IAmazonS3 s3Client, IOptions<S3Settings> s3Settings)
        {
            _s3Client = s3Client;
            _s3Settings = s3Settings;
        }

        public async Task<GetObjectResponse> DownloadAsync(string key)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = $"files/{key}"
            };

            var response = await _s3Client.GetObjectAsync(getRequest);

            return response;
        }

        public async Task<string> GetDownloadPreSignedURL(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = $"files/{key}",
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            string preSignedURL = _s3Client.GetPreSignedURL(request);

            return preSignedURL;
        }

        public async Task<Guid> UploadAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var key = Guid.NewGuid();

            var putRequest = new PutObjectRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = $"files/{key}",
                InputStream = stream,
                ContentType = file.ContentType,
                Metadata =
                {
                    ["file-name"] = file.FileName
                }
            };

            await _s3Client.PutObjectAsync(putRequest);

            return key;
        }

        internal async Task<string> GetUploadPreSignedURL(string fileName, string contentType)
        {
            var key = new Guid();
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = $"files/{key}",
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(30),
                ContentType = contentType,
                Metadata =
                {
                    ["file-name"] = fileName
                }

            };

            string preSignedURL = _s3Client.GetPreSignedURL(request);

            return preSignedURL;
        }

        public async Task RemoveAsync(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = $"files/{key}"
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}