using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApolloSpoilers.Domain.Interfaces
{
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder = "products");
        Task<bool> DeleteImageAsync(string publicId);
    }
}
