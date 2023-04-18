
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Game_of_Life
{
    internal class Program
    {
        public static int CurrentState;
        public const int MenuState = 0;
        public const int GameState = 1;
        public const int QuitState = 2;

        /*Den här funktionen initierar en dubbel array. Dubbla arrayer är som 2D tabeller, första värden
        * är antal rader, andra värden är antal kolumner. Position i tabell kan man komma åt genom att ange rad och kolumn
        * i den dubble arrayen, t.ex table [1,3] blir platsen på första raden, tredje kolumnen.*/
        /*TODO: Vi kommer förmodligen behöva två dubbelarrayer. En "gammal" och en "ny" så att den nya arrayen tar emot de nya värden utan att ändra värden
         * på den gamla (om den ändra värden på grannceller under tiden som den loopar igenom så kommer inte spelet antagligen fungera som den ska). 
         * När den har loopat klar igenom array borde värden i nya arrayen överföras till gamla osv.*/
        public static string[,] initializeTable(int height, int width)
        {
            string[,] table = new string[height, width];
            return table;
        }
        public class gameBoard
        {
            private int height, width;
            private bool[,] activeTable;
            private bool[,] inactiveTable;

            public gameBoard(int height, int width, bool randomize = false)
            {
                this.height = height;
                this.width = width;
                activeTable = new bool[height, width];
                inactiveTable = new bool[height, width];
                if(randomize) this.Randomize(height, width);        
            }
            public void Step()
            {
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        inactiveTable[i, j] = SquareStep(i, j);
                activeTable = inactiveTable;
                inactiveTable = new bool[height, width];
            }
            private bool SquareStep(int i, int j)
            {
                //NYI: Check status for activeBoard[i,j] for dead or alive. Check status of neighbours. Return dead (false) or alive (true) according to ruleset.
                return !activeTable[i, j]; //DEBUG: Placeholder method to make sure each square changes status each step to test Step() functionality.
            }
            public void Randomize(int height, int width)
            {
                Random rand = new Random();
                    for(int i = 0; i < height; i++)
                    {
                        for(int j = 0; j < width; j++)
                            activeTable[i,j] = rand.NextDouble() > 0.5;
                    }
            }
            public void PrintTable()
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (activeTable[i, j]) Console.Write("■ ");
                        else Console.Write("□ ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("\n\n" +
                    "Controls: \n" +
                    "Spacebar - Runs the simulation one step\n" +
                    "Escape - Terminates the application");
            }
        }
        //Den här funktionen går igenom varje möjlig position i tabellen och tilldelar den strängen "□ " som sedan skrivs ut.
        /*TODO: för den automatiska körläget behöver vi nog ändra det så att strängen som tilldelas är randomiserad värde 
         * och väljer mellan "□ " och "■ ". För den manuella behöver vi en funktion där användaren uppger vilka positioner
         * som blir "■ "*/
        
        public static void PrintTable(string[,] table)
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    Console.Write(table[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
        public static string[,] randomizeTable()
        {
            string[,] table = new string[25, 40];
            Random random = new Random();

            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    int randomNum = random.Next(2); // Skapar random siffra mellan 0 och 1
                    if (randomNum == 0)
                    {
                        table[i, j] = "□";
                    }
                    else
                    {
                        table[i, j] = "■";
                    }
                }
            }

            return table;
        }
        public static void calculateGeneration(string[,] table)
        {
            int rows = table.GetLength(0);
            int columns = table.GetLength(1);
            string[,] newGen = new string[rows, columns];//Skapa array för att tilldela de nya värden
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int livecount = 0;
                    int deadcount = 0;
                    for (int x = i - 1; x <= i + 1; x++)
                    {
                        for (int y = j - 1; y <= j + 1; y++)
                        {

                            if (x < 0 || y < 0 || x >= rows || y >= columns || (x == i && y == j)) //denna rad skippar räkna med cellen man står på och cellerna utanför rutnätet
                            {
                                continue;
                            }
                            if (table[x, y] == "■")
                            {
                                livecount++;
                            }
                            else if (table[x, y] == "□")
                            {
                                deadcount++;
                            }
                        }
                    }
                    if (table[i, j] == "□" && livecount >= 3) //Döda celler med 3 eller fler grannar återupplivas
                    {
                        newGen[i, j] = "■";
                    }
                    else if (table[i, j] == "■" && (livecount == 2 || livecount == 3)) //Levande celler med 2 eller 3 grannar lever vidare
                    {
                        newGen[i, j] = "■";
                    }
                    else
                    {
                        newGen[i, j] = "□"; //Celler med none of the above fortsätter vara döda
                    }
                }
            }
            Array.Copy(newGen, table, newGen.Length);//Copierar nya arrayen till gamla när allt är klarräknad
        }


        static void Main(string[] args)
        {
            CurrentState = MenuState;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Menu menu= new Menu();
            menu.PrintMenu();
            gameBoard gameField = new gameBoard(25, 40);
            while (CurrentState != QuitState)
            {
                if (CurrentState == MenuState)
                {
                    menu.checkInput();
                    menu.PrintMenu();
                } else if (CurrentState == GameState)
                {
                    Console.Clear();
                    gameField.PrintTable();
                    ConsoleKeyInfo KeyInfo = Console.ReadKey();
                    if (KeyInfo.Key.ToString() == "Spacebar")
                    {
                        Console.Clear();
                        gameField.Step();
                        gameField.PrintTable(); //TODO implementera spelet på riktigt. bara fulkod här
                    }
                    
                    else if (KeyInfo.Key.ToString() == "Escape")
                    {
                        CurrentState = MenuState;
                        menu.PrintMenu();
                    }
                    
                } 
            }
        }
    }
    public class Menu
    {
        private string[] Options;
        private int SelectedOption;
        private readonly string selectedPrefixMarker = "-> ";

        public Menu()
        {
            SelectedOption = 0;
            Options = new string[]{"New Game","Load Game", "Quit"};
        }
        public void PrintMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();
            Console.CursorVisible = false;
            string emptySpace = "";
            for (int i = 0; i < selectedPrefixMarker.Length; i++)
            {
                emptySpace += " ";
            }

            for (int i = 0; i<Options.Length; i++)
            {
                if (i==SelectedOption)
                {
                    Console.WriteLine(selectedPrefixMarker + Options[i]);
                } else
                {

                    Console.WriteLine(emptySpace + Options[i]);
                }
            }
            Console.ResetColor();
        }
        private void IncrementSelectedOption()
        {
            if (SelectedOption < Options.Length-1) {
                SelectedOption++;
            }    
        }
        private void DecrementSelectedOption()
        {
            if (SelectedOption > 0)
            {
                SelectedOption--;
            }
        }
        public void checkInput()
        {
            ConsoleKeyInfo KeyInfo = Console.ReadKey();
            if (KeyInfo.Key.ToString() == "UpArrow") DecrementSelectedOption();
            if (KeyInfo.Key.ToString() == "DownArrow") IncrementSelectedOption();
            if (KeyInfo.Key.ToString() == "Enter") ApplySelectedOption();

        }
        private void ApplySelectedOption()
        {
            if (SelectedOption == 0)
            {
                Program.CurrentState = Program.GameState;
            }
            if (SelectedOption == 1)
            {
                //TODO lägg till ladda nytt spel från fil
            }
            if (SelectedOption == 2)
            {
                Program.CurrentState = Program.QuitState;
            }
        }
    }
}