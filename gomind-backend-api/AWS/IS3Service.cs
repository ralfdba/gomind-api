using Amazon.S3;
using Amazon.S3.Transfer;

namespace gomind_backend_api.AWS
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string bucketName, string keyPrefix = "", string jobId = "");
        Task<Stream> GetFileAsync(string bucketName, string keyPrefix = "", string jobId = "");

    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;

        public S3Service(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string bucketName, string keyPrefix = "", string jobId = "")
        {
            var finalKeyPrefix = string.IsNullOrEmpty(keyPrefix) ? "raw/" : $"{keyPrefix}/";
            var key = $"{finalKeyPrefix}{jobId}.pdf";
            // -------------------------

            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = key, 
                BucketName = bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return key;
        }
        public async Task<Stream> GetFileAsync(string bucketName, string keyPrefix = "", string jobId = "")
        {
            var finalKeyPrefix = string.IsNullOrEmpty(keyPrefix) ? "results-ok/" : $"{keyPrefix}/";
            var key = $"{finalKeyPrefix}{jobId}.json";
            var response = await _s3Client.GetObjectAsync(bucketName, key);
            
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; 
            return memoryStream;
        }   
    }
}
