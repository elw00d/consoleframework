using System;
using System.Runtime.InteropServices;

namespace ConsoleFramework
{
	public class LinuxConsoleApplication
	{
		/// <summary>
		/// Returns pointer to the WINDOW struct.
		/// If no terms has created manually returns stdscr.
		/// stdscr is used to set colors and attributes.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern IntPtr initscr();
		
		[DllImport("libncursesw.so.5")]
		internal static extern void refresh();
		
		[DllImport("libncursesw.so.5")]
		internal static extern void getch();
		
		[DllImport("libncursesw.so.5")]
		internal static extern void endwin();
		
		/// <summary>
		/// We should call this function right after initscr()
		/// to enable the color subsystem.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int start_color();
		
		[DllImport("libncursesw.so.5")]
		internal static extern int init_color(short color, short r, short g, short b);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int init_pair(short i, short foregroundColor, short backgroundColor);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int addstr(string str);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int attron(int attrs);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int attroff(int attrs);
		
		internal const short NCURSES_ATTR_SHIFT = 8;
		
		internal static short NCURSES_BITS(short mask, short shift) {
			return (short) (mask << (shift + NCURSES_ATTR_SHIFT));
		}
		
		internal static short COLOR_PAIR(short n) {
			return NCURSES_BITS(n, 0);
		}
		
		#region Predefined colors
		
		internal const short COLOR_BLACK  = 0;
        internal const short COLOR_RED    = 1;
        internal const short COLOR_GREEN  = 2;
        internal const short COLOR_YELLOW = 3;
        internal const short COLOR_BLUE   = 4;
        internal const short COLOR_MAGENTA= 5;
        internal const short COLOR_CYAN   = 6;
        internal const short COLOR_WHITE  = 7;
		
		#endregion
		
		#region Mouse-related stuff
		
		#endregion
		
		public LinuxConsoleApplication ()
		{
		}
		
		public void Run() {
			//PhysicalCanvas canvas = new PhysicalCanvas(100, 35);
			initscr();
			start_color();
			init_pair(1, COLOR_BLACK, 5);
			attron(COLOR_PAIR(1));
			addstr("Hello from C-sharp ! И немного русского текста.");
			refresh();
			getch();
			endwin();
		}
	}
}

