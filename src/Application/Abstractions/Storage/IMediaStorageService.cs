using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ore.Application.Abstractions.Storage;

public interface IMediaStorageService
{
    Task<string> UploadAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default);
}
