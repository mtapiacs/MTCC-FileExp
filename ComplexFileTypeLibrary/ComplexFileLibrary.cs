using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ComplexFileTypeLibrary
{
    class ComplexFileLibrary
    {
        static void Main(string[] args)
        {
            // * READING OR WRITING COMPLEX FILE TYPE - MT Compressed Code *
            Console.WriteLine("Reading or Writing .mtcc File? R/W"); 
            string method = Console.ReadLine().ToUpper().Substring(0,1);

            if (method == "R")
            {
                Console.WriteLine("You are in reading mode...Insert File Path");
                string fileLocation = Console.ReadLine();

                ReadFile.GoRead(fileLocation); // Read Driver
            }

            else if (method == "W")
            {
                Console.WriteLine("You are in writing mode...Insert Asset Directory");
                string assetDirectory = Console.ReadLine();

                Console.WriteLine("Insert Code Directory");
                string codeDirectory = Console.ReadLine();

                WriteFile.GoWrite(assetDirectory, codeDirectory); // Write Driver
            }
        }
    }
}
