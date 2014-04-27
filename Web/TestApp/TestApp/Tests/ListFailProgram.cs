using System;
namespace TestApp.Tests
{
    public class ListFailProgram
    {
        public int X { get; set; }

        public static void Main()
        {
            ListFailProgram instance = new ListFailProgram(  );
            instance.X = 5;
            ( typeof ( ListFailProgram ) ).GetProperty( "X" )
                .GetSetMethod( ).Invoke( instance, new object[ ] { null } );
            Console.WriteLine("X = " + instance.X);
        }
    }
}
