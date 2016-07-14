using System;

namespace InputIdentifier
{
    class Program
    {
        //Given a snippet of code, containing one or more lines, identify all the undeclared identifiers accurately
        public static void Main()
        {
            SemanticAnalyzer s = new SemanticAnalyzer();
            s.CompareSymbols();
            Console.ReadKey();
        }
    }
}
