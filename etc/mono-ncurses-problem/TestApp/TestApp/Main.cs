using System;
using System.Runtime.InteropServices;

namespace TestApp
{
	/// <summary>
	/// Program uses ncurses to demonstrate strange Mono behaviour on terminals
	/// (gnome-terminal and xterm are tested).
	/// </summary>
	class MainClass
	{
		public static void Main (string[] args)
		{
			// If uncomment next line - all will work OK
			// May be first access to Console.ReadKey(Line) or Console.KeyAvailable
			// has side effects and modifies current term settings
			
			//bool x = Console.KeyAvailable;
			
			IntPtr stdscr = initscr ();
			cbreak ();
			noecho ();
			nonl ();
			intrflush (stdscr, false);
			keypad (stdscr, true);
			start_color ();
			mvaddstr (4, 5, "Test string!");
			refresh ();
			
			// If remove this line and dont call reading methods inside ncurses code -
			// all will work OK. But after first call (if reading method called before ncurses init)
			// we can call it safe.
			Console.ReadLine (); // or Console.ReadK	ey()
			
			endwin ();
		}
		
		/// <summary>
		/// Returns pointer to the WINDOW struct.
		/// If no terms has created manually returns stdscr.
		/// stdscr is used to set colors and attributes.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern IntPtr initscr();
		
		/// <summary>
		/// Enables the keypad of the user's terminal.
		/// If enabled, the mouse events will be interpreted as mouse events
		/// (prefixed with KEY_MOUSE). Otherwise, mouse events cannot be
		/// correctly interpreted (garbage key codes in getch).
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int keypad(IntPtr window, bool bf);
		
		/// <summary>
		/// Enters the cbreak mode (no lines buffering in input).
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int cbreak();
				
		/// <summary>
		/// Should be called to disable echo.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int noecho();
		
		/// <summary>
		/// To avoid that addch('\') affects current symbol position.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int nonl();
		
		/// <summary>
		/// Set this option to true to avoid problems with keyboard input buffer
		/// flushing and inconsistent data displaying.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int intrflush(IntPtr window, bool bf);
		
		[DllImport("libncursesw.so.5")]
		internal static extern void refresh();
		
		[DllImport("libncursesw.so.5")]
		internal static extern int getch();
		
		[DllImport("libncursesw.so.5")]
		internal static extern void endwin();
		
		/// <summary>
		/// We should call this function right after initscr()
		/// to enable the color subsystem.
		/// </summary>
		[DllImport("libncursesw.so.5")]
		internal static extern int start_color();
		
		[DllImport("libncursesw.so.5")]
		internal static extern int mvaddstr(int x, int y, string str);
		
	}
}
