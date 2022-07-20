using Microsoft.AspNetCore.Http;

namespace Business.Models
{
    public class FileChunkModel
    {
        public int Index { get; set; }

        public IFormFile ChunkFile { get; set; } = null!;
    }
}
