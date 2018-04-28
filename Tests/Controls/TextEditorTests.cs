using ConsoleFramework.Controls;
using Xunit;

namespace Tests.Controls
{
    public class TextEditorTests
    {
        [Fact]
        public void TestInsertFirstLine()
        {
            TextHolder holder = new TextHolder("");
            holder.Insert(0, 0, "First line");
            Assert.Equal("First line", holder.Text);
        }

        [Fact]
        public void TestInsertSecondLine()
        {
            TextHolder holder = new TextHolder("First line");
            holder.Insert(0, "First line".Length, @"
Second line");
            Assert.Equal(@"First line
Second line", holder.Text);
        }

        [Fact]
        public void TestInsertFewLines()
        {
            TextHolder holder = new TextHolder(@"First line
Second line");
            holder.Insert(1, 0, @"Intermediate line 1
Intermediate line 2
Intermediate line 3
");
            Assert.Equal(@"First line
Intermediate line 1
Intermediate line 2
Intermediate line 3
Second line", holder.Text);
        }

        [Fact]
        public void TestSplitOneLine()
        {
            TextHolder holder = new TextHolder(@"First line
Second long line");
            holder.Insert(1, "Second lo".Length, @"1234
5678
9012");
            Assert.Equal(@"First line
Second lo1234
5678
9012ng line", holder.Text);
        }

        [Fact]
        public void TestWriteToWindow()
        {
            TextHolder holder = new TextHolder(@"Line 1
Line 2
Line 3
Line 4
Line 5");
            char[,] buf = new char[5, 6];
            holder.WriteToWindow(0, 0, 6, 5, buf);
            char[,] d = new char[,]
            {
                { 'L', 'i', 'n', 'e', ' ', '1'},
                { 'L', 'i', 'n', 'e', ' ', '2'},
                { 'L', 'i', 'n', 'e', ' ', '3'},
                { 'L', 'i', 'n', 'e', ' ', '4'},
                { 'L', 'i', 'n', 'e', ' ', '5'},
            };
            Assert.Equal(d, buf);
        }

        [Fact]
        public void TestWriteToWindow2()
        {
            TextHolder holder = new TextHolder(@"Line 1
Line 2
Line 3");
            char[,] buf = new char[7, 10];
            holder.WriteToWindow(-2, -3, 10, 7, buf);
            char[,] d = new char[,]
            {
                { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                { ' ', ' ', 'L', 'i', 'n', 'e', ' ', '1', ' ', ' '},
                { ' ', ' ', 'L', 'i', 'n', 'e', ' ', '2', ' ', ' '},
                { ' ', ' ', 'L', 'i', 'n', 'e', ' ', '3', ' ', ' '},
                { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
            };
            Assert.Equal(d, buf);
        }

        [Fact]
        public void TestWriteToWindow3()
        {
            TextHolder holder = new TextHolder(@"Line 1
Line 2
Line 3
Line 4
Line 5");
            char[,] buf = new char[3, 3];
            holder.WriteToWindow(3, 3, 3, 3, buf);
            char[,] d = new char[,]
            {
                { 'e', ' ', '4'},
                { 'e', ' ', '5'},
                { ' ', ' ', ' '},
            };
            Assert.Equal(d, buf);
        }
    }
}
