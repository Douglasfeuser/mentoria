using System;

public class Program
{
    static Random random = new Random();
    static bool shouldExit = false;

    static int height;
    static int width;

    public static int playerX = 10;
    static int playerY = 0;

    static int foodX = 0;
    static int foodY = 0;

    static int enemyX = -1;
    static int enemyY = -1;
    static string enemy = "";

    static readonly string[] states = { "('.')", "(^-^)", "(X_X)" };
    static readonly string[] foods = { "@@@@@", "$$$$$", "0000" };
    static readonly string[] enemies = { "XXXX", "kkkk", "#####" };
    static string player = states[0];
    static int food = 0;

    static int score = 0;

    static void Main()
    {
        Console.CursorVisible = false;
        MainMenu();
    }

    static void MainMenu()
    {
        while (!shouldExit)
        {
            Console.Clear();
            Console.WriteLine("1. Iniciar");
            Console.WriteLine("2. Sair");
            Console.Write("Escolha uma opção: ");
            var choice = Console.ReadKey(true).Key;

            switch (choice)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Console.Clear();
                    UpdateDimensions();
                    InitializeGame();
                    GameLoop();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    shouldExit = true;
                    break;
                default:
                    Console.WriteLine("Opção inválida. Pressione qualquer tecla para tentar novamente.");
                    Console.ReadKey(true);
                    break;
            }
        }
    }

    static void GameLoop()
    {
        int enemyMoveCounter = 0;

        while (!shouldExit)
        {
            if (TerminalResized())
            {
                Console.Clear();
                Console.Write("Console was resized. Program exiting.");
                shouldExit = true;
            }
            else
            {
                if (PlayerIsFaster())
                {
                    Move(1, false);
                }
                else if (PlayerIsSick())
                {
                    FreezePlayer();
                }
                else
                {
                    Move(1, false);
                }

                if (GotFood())
                {
                    score++;
                    Console.SetCursorPosition(0, 0);
                    Console.Write($"Score: {score}");
                    ChangePlayerToHealthy();
                    ShowFood();
                }

                if (enemyY == -1 || enemyY >= height)
                {
                    SpawnEnemy();
                }

                if (enemyMoveCounter >= 10)
                {
                    MoveEnemy();
                    enemyMoveCounter = 0;
                }
                else
                {
                    enemyMoveCounter++;
                }

                DrawEnemy();

                if (HitByEnemy())
                {
                    ChangePlayerToSick();
                }
            }

            Thread.Sleep(20); 
        }
    }

    static void UpdateDimensions()
    {
        height = Console.WindowHeight - 1;
        width = Console.WindowWidth - 5;
    }

    static bool TerminalResized()
    {
        return height != Console.WindowHeight - 1 || width != Console.WindowWidth - 5;
    }

    static void ShowFood()
    {
        food = random.Next(foods.Length);
        do
        {
            foodX = random.Next(10, width - player.Length);
            foodY = random.Next(0, height);
        } while (foodX < 10 && foodY < 1);

        Console.SetCursorPosition(foodX, foodY);
        Console.Write(foods[food]);
    }

    static bool GotFood()
    {
        return playerY == foodY && playerX == foodX;
    }

    static bool PlayerIsSick()
    {
        return player.Equals(states[2]);
    }

    static bool PlayerIsFaster()
    {
        return player.Equals(states[1]);
    }

    static void ChangePlayerToHealthy()
    {
        player = states[0];
        DrawPlayer();
    }

    static void ChangePlayerToSick()
    {
        player = states[2];
        DrawPlayer();
    }

    static void FreezePlayer()
    {
        Thread.Sleep(1000);
        ChangePlayerToHealthy();
    }

    static void Move(int speed = 1, bool otherKeysExit = false)
    {
        int lastX = playerX;
        int lastY = playerY;

        var key = Console.ReadKey(true).Key;
        switch (key)
        {
            case ConsoleKey.UpArrow:
                playerY--;
                break;
            case ConsoleKey.DownArrow:
                playerY++;
                break;
            case ConsoleKey.LeftArrow:
                playerX -= speed;
                break;
            case ConsoleKey.RightArrow:
                playerX += speed;
                break;
            case ConsoleKey.Escape:
                PauseMenu();
                break;
            default:
                shouldExit = otherKeysExit;
                break;
        }

        ClearPlayer(lastX, lastY);
        KeepPlayerWithinBounds();
        DrawPlayer();
    }

    static void PauseMenu()
    {
        bool paused = true;
        while (paused)
        {
            Console.Clear();
            Console.WriteLine("1. Continuar");
            Console.WriteLine("2. Sair");
            Console.Write("Escolha uma opção: ");
            var choice = Console.ReadKey(true).Key;

            switch (choice)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    paused = false;
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.Write($"Score: {score}");
                    DrawPlayer();
                    ShowFood();
                    DrawEnemy();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    shouldExit = true;
                    paused = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida. Pressione qualquer tecla para tentar novamente.");
                    Console.ReadKey(true);
                    break;
            }
        }
    }

    static void ClearPlayer(int lastX, int lastY)
    {
        Console.SetCursorPosition(lastX, lastY);
        Console.Write(new string(' ', player.Length));
    }

    static void KeepPlayerWithinBounds()
    {
        playerX = Math.Max(10, Math.Min(playerX, width));
        playerY = Math.Max(0, Math.Min(playerY, height));
    }

    static void DrawPlayer()
    {
        Console.SetCursorPosition(playerX, playerY);
        Console.Write(player);
    }

    static void InitializeGame()
    {
        score = 0;
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        Console.Write($"Score: {score}");
        ShowFood();
        DrawPlayer();
    }

    static void SpawnEnemy()
    {
        int enemyIndex = random.Next(enemies.Length);
        enemyX = random.Next(10, width - enemies[enemyIndex].Length); // Ensure enemy doesn't spawn in the score area
        enemyY = 0;
        enemy = enemies[enemyIndex];
    }

    static void MoveEnemy()
    {
        if (enemyY >= 0 && enemyY < height)
        {
            ClearEnemy(enemyX, enemyY);
            enemyY++;
        }
    }

    static void DrawEnemy()
    {
        if (enemyY >= 0 && enemyY < height)
        {
            Console.SetCursorPosition(enemyX, enemyY);
            Console.Write(enemy);
        }
    }

    static void ClearEnemy(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.Write(new string(' ', enemy.Length));
    }

    static bool HitByEnemy()
    {
        return playerY == enemyY && playerX == enemyX;
    }
}
