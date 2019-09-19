using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExifLib;

namespace PhotoBox
{
    public class PhotoTools
    {
        public string Path { get; private set; }
        private DirectoryInfo folderInfo;


        public void Init(string path)
        {
            this.Path = path;
            folderInfo = new DirectoryInfo(path);
        }

        public IEnumerable<FileInfo> GetFiles()
        {
            if (folderInfo != null)
            {
                IList<FileInfo> result = new List<FileInfo>();
                var res = RecGetFiles(folderInfo, result);
                return res;
            }
            else
            {
                return Enumerable.Empty<FileInfo>();
            }
        }

        private IEnumerable<FileInfo> RecGetFiles(DirectoryInfo folder, IList<FileInfo> result)
        {

            folder.GetFiles().ToList().ForEach(elem => result.Add(elem));
            folder.GetDirectories().ToList().ForEach(f => RecGetFiles(f, result));

            return result;
        }
    

        public string GetNewFileName(FileInfo fileinfo)
        {
            try
            {
                using (ExifReader reader = new ExifReader(fileinfo.FullName))
                {
                    // Extract the tag data using the ExifTags enumeration
                    DateTime datePictureTaken;
                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
                                                    out datePictureTaken))
                    {
                        return fileinfo.DirectoryName + "\\" + Format(datePictureTaken) + fileinfo.Extension;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error on File: " + fileinfo.FullName, exc);
            }
            // TODO: Exception, when datePictureTakenDoesNotExist
            return fileinfo.Name + fileinfo.Extension;
        }

        private string Format(DateTime date)
        {
            return date.ToString(@"yyyyMMdd_HHmmss");
        }

        public bool IsJpegImage(FileInfo fileinfo)
        {
            try
            {
                using (ExifReader reader = new ExifReader(fileinfo.FullName))
                {
                    // Extract the tag data using the ExifTags enumeration
                    DateTime datePictureTaken;
                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
                                                    out datePictureTaken))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error on File: " + fileinfo.FullName, exc);
            }
            return false;
        }
    }
}
