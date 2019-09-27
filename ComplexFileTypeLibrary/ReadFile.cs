using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ComplexFileTypeLibrary
{
    class ReadFile
    {
        // GoRead() - Inits Main Flow Of Reading 
        public static void GoRead(string fileLocation)
        {
            // Outputs File Location And Prompts For Desired Output Directory
            Console.WriteLine("I am reading the file at: " + fileLocation);
            Console.WriteLine("Where do you want to read the files to?");
            string outputLoc = Console.ReadLine();

            // Method To Get Headers And Start File Extract Process
            GetHeadersAndCreate(fileLocation, outputLoc);
        }

        // GetHeadersAndCreate() - Uses File And Gets Headers In Order To Extract
        public static void GetHeadersAndCreate(string headerFilePath, string outLocation)
        {
            // Uses Binary Reader To Get Stream
            using (BinaryReader b = new BinaryReader(File.Open(headerFilePath, FileMode.Open)))
            {
                byte[] hBytes;

                // Go To End Of File And Back 500 To Read Only Headers
                b.BaseStream.Seek(-500, SeekOrigin.End);

                // For Naming Convention Sake - Just Updating Indexes
                int fileIndex = 0;
                do
                {
                    // Updates To Have New Names
                    fileIndex++;

                    // Gets Next 16 Bytes For Every Header Object
                    hBytes = b.ReadBytes(16);

                    // Stores Current Possition Of Stream Since It Will Be Moved In Extract Function
                    long currentPosition = b.BaseStream.Position;

                    // Extract Function Outputs Particular Header File
                    Extract(headerFilePath, outLocation, hBytes, b, fileIndex);

                    // Restores Position To Place Where It Left Of
                    b.BaseStream.Seek(currentPosition, SeekOrigin.Begin);

                    // Tests Console Writes 
                    //Console.WriteLine(BitConverter.ToInt32(hBytes, 0));
                    //Console.WriteLine(b.BaseStream.Position != b.BaseStream.Length);
                } while (BitConverter.ToInt32(hBytes, 0) != 0 && (b.BaseStream.Position != b.BaseStream.Length)); // !EOF & First Four Bytes != to 0! 
            }
        }

        // Extract() - Extracts File Based On Header Data
        public static void Extract(string mtccLocation, string extractLocation, Byte[] hData, BinaryReader bR, int fileIndex)
        {
            // File Info From Header
            string extension = Encoding.ASCII.GetString(hData, 0, 4);
            int size = BitConverter.ToInt32(hData, 4);
            int start = BitConverter.ToInt32(hData, 8);
            int end = BitConverter.ToInt32(hData, 12);

            // Writes To Console To Test
            Console.WriteLine("The Extension Is: " + extension);
            Console.WriteLine("The Size Is: " + size);
            Console.WriteLine("The Start Is: " + start);
            Console.WriteLine("The End Is: " + end);

            // Allow Write If First 3 Bytes Are Not Empty
            if (BitConverter.ToInt32(hData, 0) == 0)
            {
                return;
            }

            // Space Allocated For Particular File About To Be Created
            byte[] theData = new byte[size];

            // Start Writing From Start Position Coming From The Header
            bR.BaseStream.Seek(start, SeekOrigin.Begin);

            // Reads Data To Allocated Byte Array
            int n = bR.Read(theData, 0, size);

            // Writes File To Out Location With FileIndex And Extension. First Opens Stream, Then Writes TheData
            var fsWrite = new FileStream(extractLocation + fileIndex + extension, FileMode.Create, FileAccess.Write);
            fsWrite.Write(theData, 0, theData.Length);

            fsWrite.Close();
        }
    }
}