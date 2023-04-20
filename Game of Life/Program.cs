using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using static Game_of_Life.Program;
using System.Timers;
using ANSIConsole;
using System.Runtime.CompilerServices;

namespace Game_of_Life
{
    internal class Program
    {
        public static int CurrentState;
        public const int MenuState = 0;
        public const int GameState = 1;
        public const int QuitState = 2;
        public const int LoadState = 3;
        public const int SaveState = 4;
        public static bool hasActiveGame = false;
        public static gameBoard activeGame;
        public static Coordinate gameCursor = new();
        public static string savePath = "..\\..\\..\\savegames\\";
        /// <summary>
        /// public class Coordinate - a simple class storing two integers 'y' and 'x' to be used as a coordinate for the gameBoard.
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
            /// public void PrintTable() - Clears console and then prints activeTable to console as a matrix of chars. Also prints current gameCursor position and keymaps. Used in game mode.
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
                        if (i == gameCursor.y && j == gameCursor.x) output += "<".Color(ConsoleColor.Red);
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
            /// public void calculateGeneration() - iterates through all cells in activeTable and counts number of living neighbours with GetNumberOfAliveNeighbours. 
            /// Outputs the resulting status (alive/dead) for next tick to inactiveTable.
            /// </summary>
            public void calculateGeneration()
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int aliveNeighbours = GetNumberOfAliveNeighbours(y, x);
                        if (activeTable[y, x] == false && aliveNeighbours == 3) //Döda celler med 3 grannar återupplivas
                            inactiveTable[y, x] = true;
                        else if (activeTable[y, x] == true && (aliveNeighbours == 2 || aliveNeighbours == 3)) //Levande celler med 2 eller 3 grannar lever vidare
                            inactiveTable[y, x] = true;
                        else
                            inactiveTable[y, x] = false; //Celler med none of the above fortsätter vara döda
                    }
            }

            private int GetNumberOfAliveNeighbours(int x, int y)//Calculates sunm of neighboring live cells
            {
                int livecount = 0;
                for (int xOffset = x - 1; xOffset <= x + 1; xOffset++)
                {
                    for (int yOffset = y - 1; yOffset <= y + 1; yOffset++)
                    {
                        if (xOffset < 0 || yOffset < 0 || xOffset >= height || yOffset >= width || (xOffset == x && y == yOffset)) 
                            continue;
                        if (activeTable[xOffset, yOffset] == true)
                            livecount++;
                    }
                }
                return livecount;
            }
            public void SaveGameToFile(string filePath)
            {
                using(StreamWriter writer = new StreamWriter(filePath))
                {
                    string output = "";
                    for(int y = 0; y < height; y++)
                    {
                        if (y > 0) output += "\n";
                        for(int x = 0; x < width; x++)
                        {
                            if (activeTable[y, x]) output += "1";
                            else output += "0";
                        }
                    }
                    writer.Write(output);
                }
            }
        }
        public class Menu
        {
            private string[] Options, Files, Menus;
            private int SelectedOption;
            private readonly string selectedPrefixMarker = "-> ";

            public Menu()
            {
                SelectedOption = 0;
                Files = Directory.GetFiles(savePath);
                Menus = new string[] { "New Game", "Load Game", "Quit" };
                Options = Menus;
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
            /// <summary>
            /// public void UpdateMenuOptions() - Adds "Resume Game" and "Save Game" to menu options. Is called after the first game is started/loaded.
            /// </summary>
            public void UpdateMenuOptions()
            {
                if (Menus.Length == 3)
                    {
                        Menus = new string[] { "Resume Game", "New Game", "Save Game", "Load Game", "Quit" };
                        Options = Menus;
                    }
            }
            public void ResetMenuCursor() //resets menu cursor to 0 to having it out of scope if the menu options decrease.
            {
                SelectedOption = 0;
            }
            /// <summary>
            /// public void SaveLoadFeedbackMessage() - Prints a feedback message to console and pauses execution for half a second.
            /// </summary>
            public void SaveLoadFeedbackMessage()
            {
                string message;
                if (Options.Length == Files.Length + 1) //if loading...
                    message = "Loading game...";
                else message = "Game saved!";
                Console.WriteLine("\n" + message.Color(ConsoleColor.Green));
                Thread.Sleep(500);
            }
            public void ApplySelectedOption()
            {
                if (Options[SelectedOption] == "New Game") //Starts a new, empty 22x40 board.
                {
                    gameCursor.Reset();
                    activeGame = new gameBoard(22, 40);
                    UpdateMenuOptions();
                    Program.CurrentState = Program.GameState;
                }
                else if (Options[SelectedOption] == "Load Game") //Launches the file menu and loads selected file
                {
                    ResetMenuCursor();
                    Files = Directory.GetFiles(savePath, "*.sav");
                    Options = new string[Files.Length + 1];
                    Files.CopyTo(Options, 0);
                    Options[Options.Length - 1] = " - Return to menu";
                }
                else if (Options[SelectedOption] == "Save Game") //Launches the file menu and saves to selected file
                {
                    ResetMenuCursor();
                    Files = Directory.GetFiles(savePath, "*.sav");
                    Options = new string[Files.Length + 2];
                    Files.CopyTo(Options, 0);
                    Options[Options.Length - 2] = " - New file";
                    Options[Options.Length - 1] = " - Return to menu";
                }
                else if (Options[SelectedOption] == " - Return to menu") //Aborts load/save operations
                {
                    Options = Menus;
                    ResetMenuCursor();
                }
                else if (Options[SelectedOption] == " - New file") //Prompts the user to type in name and saves to that filename
                {
                    Console.Write("\nEnter filename (finish with 'enter') >".Color(ConsoleColor.DarkGreen));
                    activeGame.SaveGameToFile(Path.Combine(savePath, Console.ReadLine()));
                    SaveLoadFeedbackMessage();
                    Options = Menus;
                    ResetMenuCursor();
                }
                else if (Files.Contains(Options[SelectedOption])) //loading/saving
                {
                    if(Options.Length == Files.Length + 1) //if loading...
                    {
                        SaveLoadFeedbackMessage();
                        activeGame = LoadGameFromFile(Options[SelectedOption]);
                        CurrentState = GameState;
                        UpdateMenuOptions();
                        gameCursor.Reset();
                    }
                    else //if saving...
                    {
                        SaveLoadFeedbackMessage();
                        activeGame.SaveGameToFile(Options[SelectedOption]);
                    }
                    ResetMenuCursor();
                    Options = Menus;
                }
                else if (Options[SelectedOption] == "Resume Game") //Återgår till spelet
                {
                    Program.CurrentState = Program.GameState;
                }
                else if (Options[SelectedOption] == "Quit")
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
                else if(CurrentState == LoadState)
                {

                }
                else if(CurrentState == SaveState)
                {

                }
                else if (CurrentState == GameState)
                {
                    //Updates the console by clearing it and printing activeTable and keymap:
                    activeGame.PrintTable();
                    //REPL for game mode:
                    ConsoleKeyInfo KeyInfo = Console.ReadKey();
                    if (KeyInfo.Key.ToString() == "Spacebar") //Steps the simulation one tick forward in time.
                        activeGame.Step();
                    else if (KeyInfo.Key.ToString() == "Escape") //Returns to menu mode
                        //FIXME: When escaping from gameboard, selectedPrefixMarker = "-> " is not printed on the first image, it does get printed when pressing any key after that.
                        //Bug is probably in public void PrintMenu() function
                    {
                        CurrentState = MenuState;
                        menu.PrintMenu();
                    }
                    //Arrow keys edits the position of the gameCursor
                    else if (KeyInfo.Key.ToString() == "RightArrow" && gameCursor.x < activeGame.width - 1)
                        gameCursor.x++;
                    else if (KeyInfo.Key.ToString() == "LeftArrow" && gameCursor.x > 0)
                        gameCursor.x--;
                    else if (KeyInfo.Key.ToString() == "DownArrow" && gameCursor.y < activeGame.height - 1)
                        gameCursor.y++;
                    else if (KeyInfo.Key.ToString() == "UpArrow" && gameCursor.y > 0)
                        gameCursor.y--;
                    else if (KeyInfo.Key.ToString().ToLower() == "s") //s key sets status of selected cell between alive and dead
                        activeGame.SetActiveTableValue(gameCursor.y, gameCursor.x, !activeGame.GetActiveTableValue(gameCursor.y, gameCursor.x));
                    else if (KeyInfo.Key.ToString().ToLower() == "c")
                        activeGame.ClearActiveTable();
                    else if (KeyInfo.Key.ToString().ToLower() == "r")
                        activeGame.Randomize();
                    else if (KeyInfo.Key.ToString().ToLower() == "a") //a sets simulation to automode. Makes the console run one tick each 100 ms until another key is pressed.
                        while (!Console.KeyAvailable)
                        {
                            activeGame.Step();
                            activeGame.PrintTable();
                            Thread.Sleep(100);
                        }
                }
            }
        }
    }
}