using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DaftDev.FileCopier
{
    public class FileCopyService
    {
        private readonly string _sourceDir;
        private readonly string _destDir;
        private readonly bool _isParallel;

        private readonly IEnumerable<string> _allowedExtensions;

        public FileCopyService(string sourceDir, string destDir, IEnumerable<string> allowedExtensions, bool isParallel = true)
        {
            _sourceDir = sourceDir;
            _destDir = destDir;
            _allowedExtensions = allowedExtensions;
            _isParallel = isParallel;
        }

        public void ProcessMoves()
        {
            var allFiles = Directory.EnumerateFiles(_sourceDir, "*.*", SearchOption.AllDirectories);

            if (_allowedExtensions != null && _allowedExtensions.Any())
                allFiles = allFiles.Where(file => _allowedExtensions.Any(allowedExt => String.Compare(allowedExt, Path.GetExtension(file), true) == 0));

            var fileCount = allFiles.Count();
            var begin = DateTime.UtcNow;

            Console.WriteLine($"Processing {fileCount} files.");

            if (_isParallel) ProcessFilesParallel(allFiles);
            else ProcessFiles(allFiles);

            var end = DateTime.UtcNow;
            var duration = end - begin;

            Console.WriteLine($"Processed {fileCount} files in {Math.Floor(duration.TotalMinutes)} minutes {duration.Seconds} seconds");
        }

        private void ProcessFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                ProcessFile(file);
            }
        }

        private void ProcessFilesParallel(IEnumerable<string> files)
        {
            Console.WriteLine("Parallel processing enabled.");

            Parallel.ForEach(files, (file) => {
                ProcessFile(file);
            });
        }

        private void ProcessFile(string file)
        {
            Console.WriteLine($"Processing file {file}");

            var fileInfo = new FileInfo(file);

            var newFilename = $"{fileInfo.CreationTime.ToString("yyyyMMdd-HHmmss")}_{fileInfo.Name}";

            var newDirectory = Path.Combine(_destDir,
                fileInfo.CreationTime.ToString("yyyy"),
                fileInfo.CreationTime.ToString("yyyy-MM-dd"));

            var newPath = Path.Combine(newDirectory, newFilename);
            Directory.CreateDirectory(newDirectory);

            if (!File.Exists(newPath))
            {
                Console.WriteLine($"Copying {file} to {newPath}");
                File.Copy(file, newPath);
            }
            else
            {
                Console.WriteLine($"{newPath} already exists");
            }
        }
    }
}
