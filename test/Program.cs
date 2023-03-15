namespace test
{
    public static partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main method is running");
            Program.HelloFrom("Willie");
        }

        static partial void HelloFrom(string name);
    }
}