using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ComplexFileTypeLibrary
{
    class WriteFile
    {
        // Global Variables For Offsets, Set End Paddings, and Directories
        public static int offset = 0;
        public static int hOffset = 0;
        public static int endPadding = 500;
        private static string assetDir = "";
        private static string codeDir = "";
        private static string outputDir = "";

        // GoWrite() - Inits Main Flow Of Writing 
        public static void GoWrite(string aDir, string cDir)
        {
            // Gets # Of Bytes Necessary For File
            long fileSize = GetDiretorySizes(aDir, cDir);

            // Assigns Global Vars
            assetDir = aDir;
            codeDir = cDir;

            // Creates File With Necessary Size
            MakeFile(fileSize);
        }

        // GetDirectorySizes() - Driver Get File And Sub-File Sizes
        static long GetDiretorySizes(string aDir, string cDir)
        {
            // DirSize() - Helper Function
            long aSize = DirSize(new DirectoryInfo(aDir));
            long cSize = DirSize(new DirectoryInfo(cDir));
            long byteSize = aSize + cSize;

            return byteSize;
        }

        // DirSize() - Helper Gets Sizes
        static long DirSize(DirectoryInfo d)
        {
            long size = 0;

            // Add File Sizes
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            // Add SubDirectory Sizes
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }

            return size;
        }

        // MakeFile() - Creates File With Adequate Size
        static void MakeFile(long bSize)
        {
            // Prompt File Name
            Console.WriteLine("File Name: ");
            string fileName = Console.ReadLine();

            // Prompt File Store Location
            Console.WriteLine("Where do you wish to store this file?");
            string fileLocation = Console.ReadLine();
            outputDir = fileLocation;

            // Adds Established Padding
            long fSize = bSize + endPadding;

            // Full File Name In System
            string fullMTCCFilePath = fileLocation + fileName + ".mtcc";

            // Makes File To Full Directory And Gives It Established Size
            File.WriteAllBytes(fullMTCCFilePath, new byte[fSize]);

            // Adds Data To Created File
            AddData(fullMTCCFilePath, fileLocation);
        }

        // AddData() - Appends Data To File One After The Other
        static void AddData(string fullFile, string fileDir)
        {
            // String List With All the File Paths In Each Directory
            List<string> filePaths = GetFilesInDir(new DirectoryInfo(assetDir)); // Using Global
            List<string> fPathCode = GetFilesInDir(new DirectoryInfo(codeDir));  // Using Global
            filePaths.AddRange(fPathCode); // Added For One List

            // Creates Header Objects Based On Number Of Paths
            FileHeader[] fHeaders = new FileHeader[filePaths.Count];

            for (int ctr = 0; ctr < filePaths.Count; ctr++)
            {
                // Gets All Bytes From Particular Path
                byte[] bArray = ReadFileBytes(filePaths[ctr]);

                // Method Adds File Contents To File
                ByteArrayToFile(fullFile, bArray);

                // Fills Created File Headers Based On Path And Size
                fHeaders[ctr] = FillFileHeaders(filePaths[ctr], bArray.Length);

                // Offsets The Position Of Write So Files Are Not Overridden 
                offset += bArray.Length;

                // End Gets Added To Struct After Offset Is Incremented
                fHeaders[ctr].fileEnd = offset; 
            }

            // Writes The Headers To The End Of The File
            WriteHeader(fHeaders, fullFile);
        }

        // GetFilesInDir() - Returns String List Of All File Paths In Directory
        static List<string> GetFilesInDir(DirectoryInfo directory)
        {
            List<string> filePaths = new List<string>();

            int subCount = 0;

            // Get Subdirectory Paths
            DirectoryInfo[] dInfo = directory.GetDirectories();
            foreach (DirectoryInfo di in dInfo)
            {
                FileInfo[] file = di.GetFiles();
                subCount += file.Length;
                foreach (FileInfo fi in file)
                {
                    filePaths.Add(fi.FullName);
                }
            }

            // Get Directory Paths
            FileInfo[] fisMain = directory.GetFiles();
            subCount += fisMain.Length;
            foreach (FileInfo fi in fisMain)
            {
                filePaths.Add(fi.FullName);
            }

            return filePaths;
        }

        // ReadFileBytes() - Returns Bytes Of Provided Path
        static byte[] ReadFileBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        // ByteArrayToFile() - Writes Bytes To Provided File
        static void ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Write))
                {
                    // Starts At Offset = 0. Then Changes According To Files Written
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(byteArray, 0, byteArray.Length);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
            }
        }

        // FilleFileHeaders() - Fills The Header Struct
        static FileHeader FillFileHeaders(string fPath, int fileSize)
        {
            // File Info Used To Populate Structure
            FileInfo fi = new FileInfo(fPath);

            // Instantiates A New Struct
            FileHeader fHeader = new FileHeader();

            // Extra 5 Space Padding For Files With No Extension
            fHeader.fileType = fi.Extension + "     ";
            fHeader.fileSize = fileSize;
            fHeader.fileStart = offset;
            fHeader.theFilePath = fPath;

            return fHeader;
        }

        // WriteHeaders() - Writes The Header Structs To File
        static void WriteHeader(FileHeader[] fileHeaders, string mtccPath)
        {
            using (var fs = new FileStream(mtccPath, FileMode.Open, FileAccess.Write))
            {
                // Write Headers To hOffset (0) - 500 Starting From End
                fs.Seek(hOffset - endPadding, SeekOrigin.End);

                // Writes All The File Headers In fileHeader Array
                foreach (FileHeader fH in fileHeaders)
                {
                    // Gets Dot And 3 Characters After
                    string subFileType = fH.fileType.Substring(0, 4);

                    // Console.Write To See Extensions Coming Through
                    Console.WriteLine(subFileType);

                    // Converts Header Information To Byte Arrays
                    byte[] fType = Encoding.ASCII.GetBytes(subFileType);
                    byte[] fSize = BitConverter.GetBytes(fH.fileSize);
                    byte[] fStar = BitConverter.GetBytes(fH.fileStart);
                    byte[] fEnd = BitConverter.GetBytes(fH.fileEnd);

                    // Writes Information At Same Seek Position For Every File
                    fs.Write(fType, 0, fType.Length);
                    fs.Write(fSize, 0, fSize.Length);
                    fs.Write(fStar, 0, fStar.Length);
                    fs.Write(fEnd, 0, fEnd.Length);
                }
                fs.Close();
            }
        }

        // Structure For File Header (MTCC)
        struct FileHeader
        {
            public string fileType;
            public int fileSize;
            public int fileStart;
            public int fileEnd;
            public string theFilePath;
        }
    }
}