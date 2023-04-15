
namespace Game_of_Life
{
    internal class Program
    {
        
        
        public static string[,] initializeTable()
        {
            string[,] table = new string[25, 40];
            return table;
        }
        /*Den här funktionen initierar en dubbel array. Dubbla arrayer är som 2D tabeller, första värden
         * är antal rader, andra värden är antal kolumner. Position i tabell kan man komma åt genom att ange rad och kolumn
         * i den dubble arrayen, t.ex table [1,3] blir platsen på första raden, tredje kolumnen.*/

        public static void PrintTable(string[,] table)
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    Console.Write("□ ");
                }
                Console.WriteLine();
            }
        }
        /*Den här funktionen går igenom varje möjlig poisition i tabellen och tilldelar den strängen "□ " som sedan skrivs ut,
         * för den automatiska körläget behöver vi nog ändra det så att strängen som tilldelas är randomiserad och väljer mellan 
         * "□ " och "■ ".*/

            static void Main(string[] args)
        {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                // Console.WriteLine("□ ■");
                string[,] table = initializeTable();
                PrintTable(table);
            }
    }

}