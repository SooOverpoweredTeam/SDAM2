using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace SDAM2
{
    public class WindowManager
    {
        public WindowManager()
        { }
        public void Initialize()
        {
            Exchange exchange;
            // check if json data exist, if not then instantiate a new exchange
            if (File.Exists("jsonsaved.json"))
            {
                String jsonstring = File.ReadAllText("jsonsaved.json");
                exchange = JsonSerializer.Deserialize<Exchange>(jsonstring);
            }
            else
            {
                exchange = new Exchange();
            }

            // TEST DATA
            exchange.stockManager.addStock("AAPL", 15.25m, 10000);
            exchange.stockManager.addStock("AAPL", 14.00m, 10000);
            exchange.stockManager.addStock("BAPL", 13.25m, 10000);
            //


            //  Login check
            Login(exchange);
            // Main Menu
            MainMenu(exchange);

        }
        static void SaveData(Exchange exchange)
        {
            string jsonstring = JsonSerializer.Serialize<Exchange>(exchange);
            File.WriteAllText("jsonsaved.json", jsonstring);
        }
        static void Login(Exchange exchange) //Login
        {
            // Ask for bank name repeatedly until a correct name is provided
            while (true)
            {
                Console.Clear();
                Console.WriteLine("-----------Log In------------"); //For design purpose only
                Console.Write("Please enter your bank name (Leave blank if you are unregistered): "); //Ask for bank name
                String bankName = Console.ReadLine();
                if (bankName == "") //Bank name empty, prompts user to signup
                {
                    SignUp(exchange);
                }
                else if ((from bank in exchange.bankManager.BankList where bank.name == bankName select bank).Count() == 1) // Bank name exist
                {
                    Console.WriteLine("Authenticated");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;//Exit loop
                }
                else
                {
                    Console.WriteLine("The name you entered does not match any registered bank");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }

            }
        }
        static void SignUp(Exchange exchange)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("-----------Sign Up------------");
                Console.Write("Please enter your bank name : ");
                String bankName = Console.ReadLine();
                if (bankName == "") // Bank name empty
                {
                    Console.WriteLine("Bank name cannot be empty");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
                else if ((from bank in exchange.bankManager.BankList where bank.name == bankName select bank).Count() == 1) // Bank name exist
                {
                    Console.WriteLine("Bank already exist, please try another name");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
                else
                {
                    exchange.bankManager.addBank(bankName);
                    SaveData(exchange);
                    Console.WriteLine($"Signed up under {bankName} successfully");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    break;
                }
            }
        }
        static void MainMenu(Exchange exchange)
        {
            const String EXIT = "0";
            const String BUY = "1";
            bool flag = true;
            while (flag)
            {
                Console.Clear();
                Console.WriteLine("-----------MAIN MENU----------");
                Console.WriteLine("1. Trade");
                Console.WriteLine("2. Your Asset");
                Console.WriteLine("3. Transaction Log");
                Console.WriteLine("4. Financial Report");
                Console.WriteLine("0. EXIT PROGRAM");
                Console.WriteLine(new string('-', 29));
                Console.Write("Please choose an option: ");
                String choice = Console.ReadLine();
                switch (choice)
                {
                    case BUY:
                        StockExchangeMenu(exchange);
                        break;
                    case EXIT:
                        string jsonstring = JsonSerializer.Serialize<Exchange>(exchange);
                        File.WriteAllText("jsonsaved.json", jsonstring);
                        flag = false;
                        break;
                    default:
                        Console.WriteLine("Please choose from the listed options");
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }
        static void StockExchangeMenu(Exchange exchange)
        {
            const String EXIT = "0";
            bool flag = true;
            while (flag)
            {
                Console.Clear();
                Console.WriteLine("----------TRADE MENU----------");
                Console.WriteLine("\n0. BACK");
                Console.WriteLine("\n----------STOCK CODES---------");
                Console.WriteLine("{0,-8}{1,8}", "Name", "Price");
                Console.WriteLine("{0,-8}{1,8}", "----", "-----");
                List<String> stockcodes = new List<String>((from stock in exchange.stockManager.StockList orderby stock.stockCode select stock.stockCode).Distinct());
                foreach (String s in stockcodes)
                {
                    Stock st = exchange.stockManager.getStock(s)[0];
                    Console.WriteLine("{0, -8}{1,8:C2}", st.stockCode, st.price);
                }
                Console.Write("\nChoose one of the menu option or enter stock name: ");
                String choice = Console.ReadLine();
                switch (choice)
                {
                    case EXIT:
                        flag = false;
                        break;
                    default:
                        if (stockcodes.Contains(choice))
                        {
                            ExchangeMenu(exchange, choice);
                        }
                        else
                        {
                            Console.WriteLine("Please choose from the listed options");
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                        }
                        break;
                }
            }
        }
        static void ExchangeMenu(Exchange exchange, String stockCode)
        {
            const String EXIT = "0";
            const String BUY = "1";
            bool flag = true;
            while (flag)
            {
                Console.Clear();
                Console.WriteLine("----------TRADE MENU----------");
                Console.WriteLine("\n0. BACK\n");
                Console.WriteLine("1. BUY");
                Console.WriteLine("2. QUOTE");
                Console.WriteLine("\n----------STOCK LIST----------");
                Console.WriteLine("{0,-8}{1,8}{2,12}", "Name", "Price", "Volume");
                Console.WriteLine("{0,-8}{1,8}{2,12}", "----", "-----", "------");
                List<Stock> stocks = exchange.stockManager.getStock(stockCode);
                int count = 0;
                foreach (Stock s in stocks)
                {
                    Console.WriteLine("{0,-8}{1,8:C2}{2,12}", s.stockCode, s.price, s.volume);
                    count += 1;
                    if (count == 10)
                    {
                        break;
                    }
                }
                Console.Write("\nPlease choose an option: ");
                String choice = Console.ReadLine();
                switch (choice)
                {
                    case EXIT:
                        flag = false;
                        break;
                    case BUY:
                        Console.Write("Please enter price: ");
                        decimal user_price = Convert.ToDecimal(Console.ReadLine());
                        Console.Write("Please enter volume: ");
                        int user_vol = Convert.ToInt16(Console.ReadLine());
                        List<decimal> prices = new List<decimal>(from stock in stocks select stock.price);
                        if (prices.Contains(user_price))
                        {
                            int avail_stock = (from stock in stocks where stock.price == user_price select stock.volume).Sum();
                            if (avail_stock >= user_vol)
                            {
                                //Banks buy successful
                            }
                            else
                            {
                                //Send a quote
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No {stockCode} stock available at {user_price}");
                            Console.WriteLine($"The best price for {stockCode} is {stocks.First().price}");
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                        }
                        break;
                    default:
                        Console.WriteLine("Please choose from the listed options");
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}