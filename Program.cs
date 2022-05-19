using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MouseColorFinder
{
    public struct PIXEL
    {
        public int X;
        public int Y;
        public int myColor;
    }

    internal class Program
    {
        // ASCII values
        const Int32 e = 69;
        const Int32 s = 83;

        // Point struct
        private struct POINT
        {
            public int X;
            public int Y;
        }

        // Converts point to 2 dimensional array
        private static int[] pointToArr(POINT p)
        {
            int[] arr = new int[] { p.X, p.Y };
            return arr;
        }

        #region
        // Function used to get key presses
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        // Functions to get cursor information
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT point);
        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int x, int y);
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Press E while reading to go to the next step. Press S to skip step and set values to 0");
            Console.WriteLine("Now reading: Party Button");
            PIXEL PartyPixel = ReadPixelData();
            Console.WriteLine($"Data read: {PartyPixel.X}, {PartyPixel.Y} Color: {PartyPixel.myColor}");

            Console.WriteLine("Now reading: Challenge Button");
            PIXEL ChallengePixel = ReadPixelData();
            Console.WriteLine($"Data read: {ChallengePixel.X}, {ChallengePixel.Y} Color: {ChallengePixel.myColor}");



            Console.WriteLine("Now reading: Position 1.");
            POINT Pos1 = ReadMousePos();
            Console.WriteLine($"Data read: {Pos1.X}, {Pos1.Y}");

            Console.WriteLine("Now reading: Position 2.");
            POINT Pos2 = ReadMousePos();
            Console.WriteLine($"Data read: {Pos2.X}, {Pos2.Y}");


            int[] cardsXs = new int[4];
            for (int i = 0; i < cardsXs.Length; i++)
            {
                Console.WriteLine($"Now reading: Card {i + 1}'s horizontal position");
                POINT card = ReadMousePos();
                Console.WriteLine($"Data read: {card.X}");
                cardsXs[i] = card.X;
            }
            Console.WriteLine("Now reading: Cards vertical position. " +
                "It is recommended to place cursor on the very top of the card");
            int cardsY = ReadMousePos().Y;
            Console.WriteLine($"Data read: {cardsY}");


            Console.WriteLine("Now reading: Ok end button. This will take a while...");
            PIXEL OkEndPixel = ReadPixelData();
            Console.WriteLine($"Data read: {OkEndPixel.X}, {OkEndPixel.Y} Color: {OkEndPixel.myColor}");

            Console.WriteLine("Now reading: Rewards Button. Press S to skip (Recommended).");
            PIXEL RewardsButton = ReadPixelData();
            Console.WriteLine($"Data read: {RewardsButton.X}, {RewardsButton.Y} Color: {RewardsButton.myColor}");

            Console.WriteLine("Now writing information to JSON file.");
            Config config = new Config(PartyPixel, ChallengePixel, OkEndPixel, RewardsButton, 
                pointToArr(Pos1), pointToArr(Pos2), cardsXs, cardsY);

            string fileName = "config.json";
            string jsonString = JsonSerializer.Serialize(config);
            File.WriteAllText(fileName, jsonString);

            Console.WriteLine(File.ReadAllText(fileName));

        }

        // Reads color from cursor and converts it to int
        private static int GetColorValue(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                (int)(pixel & 0x0000FF00) >> 8,
                (int)(pixel & 0x00FF0000) >> 16);
            return color.ToArgb();
        }

        // Function to wait for keypresses and carry action before continuing
        private static PIXEL ReadPixelData()
        {
            PIXEL pixel;

            while (true)
            {
                // Read and update cursor position
                POINT p;
                GetCursorPos(out p);
                pixel.X = p.X;
                pixel.Y = p.Y;
                // Read and update cursor pixel color
                pixel.myColor = GetColorValue(p.X, p.Y);

                // Break
                int keyStateE = GetAsyncKeyState(e);
                if (keyStateE == 1 || keyStateE == -32767) // Min number of regular int
                {
                    break;
                }
                // Skip
                int keyStateS = GetAsyncKeyState(s);
                if (keyStateS == 1 || keyStateS == -32767) // Min number of regular int
                {
                    pixel.X = 0;
                    pixel.Y = 0;
                    pixel.myColor = 0;
                    break;
                }
                Thread.Sleep(50); // Stability
            }

            return pixel;
        }

        private static POINT ReadMousePos()
        {
            POINT point;

            while (true)
            {
                // Read and update cursor position
                POINT p;
                GetCursorPos(out p);
                point.X = p.X;
                point.Y = p.Y;

                int keyStateE = GetAsyncKeyState(e);
                if (keyStateE == 1 || keyStateE == -32767) // Min number of regular int
                {
                    break;
                }
                // Skip
                int keyStateS = GetAsyncKeyState(s);
                if (keyStateS == 1 || keyStateS == -32767) // Min number of regular int
                {
                    point.X = 0;
                    point.Y = 0;
                    break;
                }
                Thread.Sleep(50); // Stability
            }

            return point;
        }
    }

    // Class to store configuration data
    public class Config
    {
        public Button PartyButton { get; set; }
        public Button ChallengeButton { get; set; }
        public Button OkEndButton { get; set; }
        public Button RewadsButton { get; set; }
        public int[] Pos1 { get; set; }
        public int[] Pos2 { get; set; }
        public int[] CardsXs { get; set; }
        public int CardsY { get; set; }

        public class Button
        {
            public int[] coords { get; set; }
            public int color { get; set; }

            public Button(PIXEL data)
            {
                this.coords = new int[] { data.X, data.Y };
                this.color = data.myColor;
            }
        }

        public Config(PIXEL pButton, PIXEL cButton, PIXEL oButton, PIXEL rButton, int[] pos1, int[] pos2, int[] cardsxs, int cardsy)
        {
            this.PartyButton = new Button(pButton);
            this.ChallengeButton = new Button(cButton);
            this.OkEndButton = new Button(oButton);
            this.RewadsButton = new Button(rButton);
            this.Pos1 = pos1;
            this.Pos2 = pos2;
            this.CardsXs = cardsxs;
            this.CardsY = cardsy;
        }
    }
}

