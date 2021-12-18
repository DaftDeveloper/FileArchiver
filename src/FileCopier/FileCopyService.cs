using DaftDev.FileCopier.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DaftDev.FileCopier
{
    public class FileCopyService
    {
        readonly string _sourceDir;
        readonly string _destDir;
        readonly bool _isParallel;

        readonly IEnumerable<string> _allowedExtensions;
        readonly TimestampSource _timestampSource;

        public FileCopyService(string sourceDir, string destDir, IEnumerable<string> allowedExtensions, bool isParallel = true, TimestampSource timestampSource = TimestampSource.CreatedTimestamp)
        {
            _sourceDir = sourceDir;
            _destDir = destDir;
            _allowedExtensions = allowedExtensions;
            _isParallel = isParallel;
            _timestampSource = timestampSource;
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

        void ProcessFilesParallel(IEnumerable<string> files)
        {
            Console.WriteLine("Parallel processing enabled.");

            Parallel.ForEach(files, (file) => {
                ProcessFile(file);
            });
        }

        void ProcessFile(string file)
        {
            Console.WriteLine($"Processing file {file}");

            var fileInfo = new FileInfo(file);

            var newFilename = $"{fileInfo.CreationTime.ToString("yyyyMMdd-HHmmss")}_{fileInfo.Name}";

            var newDirectory = GetDestinationDirectory(fileInfo);

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

        string GetDestinationDirectory(FileInfo fileInfo)
        {
            if(_timestampSource == TimestampSource.CreatedTimestamp)
            {
                return Path.Combine(_destDir,
                fileInfo.CreationTime.ToString("yyyy"),
                fileInfo.CreationTime.ToString("yyyy-MM-dd"));
            }
            else
            {
                return Path.Combine(_destDir,
                fileInfo.LastWriteTime.ToString("yyyy"),
                fileInfo.LastWriteTime.ToString("yyyy-MM-dd"));
            }
        }
    }
}
