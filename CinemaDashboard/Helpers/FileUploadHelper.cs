using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CinemaDashboard.Helpers
{
    public static class FileUploadHelper
    {
        public static async Task<string> SaveFileAsync(IWebHostEnvironment env, IFormFile file, string subFolder)
        {
            var folder = Path.Combine(env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(folder);

            var storedName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, storedName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return storedName;
        }

        public static void DeleteFile(IWebHostEnvironment env, string subFolder, string storedName)
        {
            if (string.IsNullOrEmpty(storedName)) return;

            var fullPath = Path.Combine(env.WebRootPath, "uploads", subFolder, storedName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
