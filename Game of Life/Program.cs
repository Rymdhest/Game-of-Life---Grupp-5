using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using static Game_of_Life.Program;
using System.Timers;
using ANSIConsole;

namespace Game_of_Life
{
    internal class Program
    {
        public static int CurrentState;
        public const int MenuState = 0;
        public const int GameState = 1;
        public const int QuitState = 2;
        public static bool hasActiveGame = false;
        public static gameBoard activeBoard;
        public static Coordinate cursor = new();
        /// <summary>
        /// public class Coordinate - a simple class storing two integers 'x' and 'y' to be used as a coordinate for the gameBoard.
        /// </summary>
        public class Coordinate
        {
            public int x = 0, y = 0;
            public void Set(int x, int y)
            {
                this.x = x; this.y = y;
            }
            public void Reset()
            {
                x = 0; y = 0;
            }
        }
        public static gameBoard LoadGameFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            int height = lines.Length;
            int width = lines[0].Length;

            gameBoard loadedGame = new gameBoard(height, width, true);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (lines[i][j] == '1')
                        loadedGame.SetActiveTableValue(i, j, true);
                    else
                        loadedGame.SetActiveTableValue(i, j, false);
                }
            }
            return loadedGame;
        }
        /// <summary>
        /// public class gameBoard - stores a two-dimensional bool array which are the cells for the Game of Life. Also stores an inactive copy and ints for the size of the array (size of the board).
        /// </summary>
        public class gameBoard
        {
            public void SetActiveTableValue(int i, int j, bool value)
            {
                activeTable[i, j] = value;
            }

            public bool GetActiveTableValue(int i, int j)
            {
                return activeTable[i, j];
            }

            public int height { get; private set; }
            public int width { get; private set; }
            private bool[,] activeTable;
            private bool[,] inactiveTable;

            public gameBoard(int height, int width, bool randomize = false)
            {
                //FIXME storleken på spelplanen får inte vara större än konsollrutan. då fungerar inte Console.clear() som vi vill.
                this.height = height;
                this.width = width;
                activeTable = new bool[height, width];
                inactiveTable = new bool[height, width];
                if (randomize) this.Randomize();
            }
            /// <summary>
            /// public void ClearActiveTable() - sets all cells in the active board to dead (false).
            /// </summary>
            public void ClearActiveTable()
            {
                activeTable = new bool[height, width];
            }
            /// <summary>
            /// public void Step() - calculates one tick of the Game of Life for all cells in activeTable and resets inactiveTable to all dead.
            /// </summary>
            public void Step()
            {
                calculateGeneration();
                activeTable = inactiveTable;
                inactiveTable = new bool[height, width];
            }
            /// <summary>
            /// public void Randomize() - Randomizes the values of all cells in activeTable between alive (true) and dead (false).
            /// </summary>
            public void Randomize()
            {
                Random rand = new Random();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                        activeTable[i, j] = rand.NextDouble() > 0.5;
                }
            }
            /// <summary>
            /// public void PrintTable() - Clears console and then prints activeTable to console as a matrix of chars. Also prints current cursor position and keymaps. Used in game mode.
            /// </summary>
            public void PrintTable()
            {
                Console.Clear();
                string output = "";
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (activeTable[i, j]) output += "■".Color(ConsoleColor.Yellow);
                        else output += "□".Color(ConsoleColor.DarkGreen);
                        if (i == cursor.y && j == cursor.x) output += "<".Color(ConsoleColor.Red);
                        else output += " ";
                    }
                    output += "\n";
                }

                output += "\n" +
                    "Controls: \n" +
                    "← → ↑ ↓  - Move marker                      A - Runs the simulation until cancelled with spacebar\n" +
                    "SPACEBAR - Runs the simulation one step     R - Randomize the gameboard\n" +
                    "C        - Clear the gameboard              S - Toggle dead/alive for selected square \n" +
                    "ESCAPE   - Return to menu\n";
                Console.WriteLine(output);
                Console.WindowTop = 0;
            }
            /// <summary>
            /// public void calculateGeneration() - iterates through all cells in activeTable and counts number of living neighbours. Outputs the resulting status (alive/dead) for next tick to inactiveTable.
            /// </summary>
            public void calculateGeneration()
            {
                for (int x = 0; x < height; x++)
                    for (int y = 0; y < width; y++)
                    {
                        int aliveNeighbours = GetNumberOfAliveNeighbours(x, y);
                        if (activeTable[x, y] == false && aliveNeighbours == 3) //Döda celler med 3 grannar återupplivas
                            inactiveTable[x, y] = true;
                        else if (activeTable[x, y] == true && (aliveNeighbours == 2 || aliveNeighbours == 3)) //Levande celler med 2 eller 3 grannar lever vidare
                            inactiveTable[x, y] = true;
                        else
                            inactiveTable[x, y] = false; //Celler med none of the above fortsätter vara döda
                    }
            }

            private int GetNumberOfAliveNeighbours(int x, int y)
            {
                int livecount = 0;
                for (int xOffset = x - 1; xOffset <= x + 1; xOffset++)
                {
                    for (int yOffset = y - 1; yOffset <= y + 1; yOffset++)
                    {
                        if (xOffset < 0 || yOffset < 0 || xOffset >= height || yOffset >= width || (xOffset == x && y == yOffset)) //denna rad skippar räkna med cellen man står på och cellerna utanför rutnätet
                            continue;
                        if (activeTable[xOffset, yOffset] == true)
                            livecount++;
                    }
                }
                return livecount;
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
                
                Options = new string[] { "New Game", "Load Game", "Quit" };
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

                for (int i = 0; i < Options.Length; i++)
                {
                    if (i == SelectedOption)
                    {
                        Console.WriteLine(selectedPrefixMarker + Options[i]);
                    }
                    else
                    {

                        Console.WriteLine(emptySpace + Options[i]);
                    }
                }
                Console.ResetColor();
            }
            private void IncrementSelectedOption()
            {
                if (SelectedOption < Options.Length - 1)
                {
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
            public void ApplySelectedOption()
            {
                if (Options[SelectedOption] == "New Game")
                {
                    if (Options.Length == 3) Options = new string[] { "Resume Game", "New Game", "Save Game", "Load Game", "Quit" };
                    cursor.Reset();
                    activeBoard = new gameBoard(22, 40);
                    Console.WindowTop = 0;
                    Program.CurrentState = Program.GameState;
                }
                if (Options[SelectedOption] == "Load Game")
                {
                    if (Options.Length == 3) Options = new string[] { "Resume Game", "New Game", "Save Game", "Load Game", "Quit" };
                    // NYI: Välj fil att ladda från.
                    cursor.Reset();
                    string filePath = Path.GetFullPath("..\\..\\..\\gameoflife2.txt");
                    Program.activeBoard = Program.LoadGameFromFile(filePath);
                    Program.CurrentState = Program.GameState;
                }
                if (Options[SelectedOption] == "Save Game")
                {
                    // NYI: Spara spel till fil.
                }
                if (Options[SelectedOption] == "Resume Game") //Återgår till spelet
                {
                    Program.CurrentState = Program.GameState;
                }
                if (Options[SelectedOption] == "Quit")
                {
                    Program.CurrentState = Program.QuitState;
                }
            }
        }

        static void Main(string[] args)
        {
            CurrentState = MenuState;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Menu menu = new Menu();
            menu.PrintMenu();
            while (CurrentState != QuitState)
            {
                if (CurrentState == MenuState)
                {
                    menu.checkInput();
                    menu.PrintMenu();
                }
                else if (CurrentState == GameState)
                {
                    //Updates the console by clearing it and printing activeTable and keymap:
                    activeBoard.PrintTable();
                    //REPL for game mode:
                    ConsoleKeyInfo KeyInfo = Console.ReadKey();
                    if (KeyInfo.Key.ToString() == "Spacebar") //Steps the simulation one tick forward in time.
                        activeBoard.Step();
                    else if (KeyInfo.Key.ToString() == "Escape") //Returns to menu mode
                        //FIXME: När man kör load game och sen escape så ser menyn lite konstigt ut
                    {
                        CurrentState = MenuState;
                        menu.PrintMenu();
                    }
                    //Arrow keys edits the position of the cursor
                    else if (KeyInfo.Key.ToString() == "RightArrow" && cursor.x < activeBoard.width - 1)
                        cursor.x++;
                    else if (KeyInfo.Key.ToString() == "LeftArrow" && cursor.x > 0)
                        cursor.x--;
                    else if (KeyInfo.Key.ToString() == "DownArrow" && cursor.y < activeBoard.height - 1)
                        cursor.y++;
                    else if (KeyInfo.Key.ToString() == "UpArrow" && cursor.y > 0)
                        cursor.y--;
                    else if (KeyInfo.Key.ToString().ToLower() == "s") //s key sets status of selected cell between alive and dead
                        activeBoard.SetActiveTableValue(cursor.y, cursor.x, !activeBoard.GetActiveTableValue(cursor.y, cursor.x));
                    else if (KeyInfo.Key.ToString().ToLower() == "c")
                        activeBoard.ClearActiveTable();
                    else if (KeyInfo.Key.ToString().ToLower() == "r")
                        activeBoard.Randomize();
                    else if (KeyInfo.Key.ToString().ToLower() == "a") //a sets simulation to automode. Makes the console run one tick each 100 ms until another key is pressed.
                        while (!Console.KeyAvailable)
                        {
                            activeBoard.Step();
                            activeBoard.PrintTable();
                            Thread.Sleep(100);
                        }
                }
            }
        }
    }
}