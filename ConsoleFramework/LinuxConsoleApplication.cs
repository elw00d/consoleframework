using System;
using System.Runtime.InteropServices;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Native;
using System.Collections.Generic;

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
		internal static extern int mvaddstr(int x, int y, string str);
		
		// doesn't work with UTF
		[DllImport("libncursesw.so.5")]
		internal static extern int mvaddch(int x, int y, char ch);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int attron(int attrs);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int attrset(int attrs);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int color_set(short color, IntPtr opts);
		
		[DllImport("libncursesw.so.5")]
		internal static extern int attroff(int attrs);
		
		internal const short NCURSES_ATTR_SHIFT = 8;
		
		internal static ulong NCURSES_BITS(ulong mask, short shift) {
			return (mask << (shift + NCURSES_ATTR_SHIFT));
		}
		
		internal static ulong COLOR_PAIR(short n) {
			return NCURSES_BITS((ulong) n, 0);
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
		
		internal static ulong A_STANDOUT =	NCURSES_BITS(1UL,8);
internal static ulong A_UNDERLINE = NCURSES_BITS(1UL,9);
internal static ulong A_REVERSE=	NCURSES_BITS(1UL,10);
internal static ulong A_BLINK=		NCURSES_BITS(1UL,11);
internal static ulong A_DIM=		NCURSES_BITS(1UL,12);
internal static ulong A_BOLD=		NCURSES_BITS(1UL,13);
		
		//
		
		#endregion
		
		/// <summary>
		/// Returns ncurses standard color for specified rgb combination.
		/// </summary>
		internal static int getStandardColor(bool r, bool g, bool b) {
			if (r) {
				if (g) {
					if (b)
						return COLOR_WHITE;
					else
						return COLOR_YELLOW; // must be brown ?
				} else {
					if (b)
						return COLOR_MAGENTA;
					else
						return COLOR_RED;
				}				
			} else {
				if (g) {
					if (b)
						return COLOR_CYAN;
					else
						return COLOR_GREEN;
				} else {
					if (b)
						return COLOR_BLUE;
					else
						return COLOR_BLACK;
				}
			}
		}
		
		/// <summary>
		/// Doesn't support background intensity and other
		/// extended windows attributes.
		/// </summary>
		internal static short winAttrsToNCursesAttrs(CHAR_ATTRIBUTES attrs, out bool fgIntensity) {
			bool fgRed = (attrs & CHAR_ATTRIBUTES.FOREGROUND_RED) == CHAR_ATTRIBUTES.FOREGROUND_RED;
			bool fgGreen = (attrs & CHAR_ATTRIBUTES.FOREGROUND_GREEN) == CHAR_ATTRIBUTES.FOREGROUND_GREEN;
			bool fgBlue = (attrs & CHAR_ATTRIBUTES.FOREGROUND_BLUE) == CHAR_ATTRIBUTES.FOREGROUND_BLUE;
			
			bool bgRed = (attrs & CHAR_ATTRIBUTES.BACKGROUND_RED) == CHAR_ATTRIBUTES.BACKGROUND_RED;
			bool bgGreen = (attrs & CHAR_ATTRIBUTES.BACKGROUND_GREEN) == CHAR_ATTRIBUTES.BACKGROUND_GREEN;
			bool bgBlue = (attrs & CHAR_ATTRIBUTES.BACKGROUND_BLUE) == CHAR_ATTRIBUTES.BACKGROUND_BLUE;
			
			fgIntensity = (attrs & CHAR_ATTRIBUTES.FOREGROUND_INTENSITY) == CHAR_ATTRIBUTES.FOREGROUND_INTENSITY;
			
			int fg = getStandardColor(fgRed, fgGreen, fgBlue);
			int bg = getStandardColor(bgRed, bgGreen, bgBlue);
			
			//short index = (short) (fg | (bg << 3));
			//if (!usedIndexes.Contains(index)) {
			//	init_pair(index, (short) fg, (short) bg);
			//	usedIndexes.Add(index);
			//}
			int index = (fg | (bg << 3));
			if (createdPairs.ContainsKey(index)) {
				return createdPairs[index];
			} else {
				short pairId = (short) (lastUsedPairId + 1);
				init_pair(pairId, (short) fg, (short) bg);
				createdPairs.Add(index, pairId);
				lastUsedPairId++;
				return pairId;
			}
		}
		
		private static short lastUsedPairId = 0;
		private static readonly Dictionary<int, short> createdPairs = new Dictionary<int, short>();
		
		#region Mouse-related stuff
		
		/// <summary>
		/// Поскольку в хедерах ncurses нет информации о типах аргументов,
		/// попробуем воспользоваться выводом типов для generic методов.
		/// </summary>
		private static UInt64 NCURSES_MOUSE_MASK(int b, UInt64 m) {
			return ((m) << (((b) - 1) * 5));
		}
		
		internal const UInt64 NCURSES_BUTTON_RELEASED = 001L;
		internal const UInt64 NCURSES_BUTTON_PRESSED = 002L;
		internal const UInt64 NCURSES_BUTTON_CLICKED = 004L;
		internal const UInt64 NCURSES_DOUBLE_CLICKED = 010L;
		internal const UInt64 NCURSES_TRIPLE_CLICKED = 020L;
		internal const UInt64 NCURSES_RESERVED_EVENT = 040L;
		
		internal static UInt64	BUTTON1_RELEASED	= NCURSES_MOUSE_MASK(1, NCURSES_BUTTON_RELEASED);
		
		internal static UInt64	BUTTON1_PRESSED	=	NCURSES_MOUSE_MASK(1, NCURSES_BUTTON_PRESSED);
		internal static UInt64	BUTTON1_CLICKED=		NCURSES_MOUSE_MASK(1, NCURSES_BUTTON_CLICKED);
		internal static UInt64	BUTTON1_DOUBLE_CLICKED=	NCURSES_MOUSE_MASK(1, NCURSES_DOUBLE_CLICKED);
		internal static UInt64	BUTTON1_TRIPLE_CLICKED=	NCURSES_MOUSE_MASK(1, NCURSES_TRIPLE_CLICKED);
		
		internal static UInt64	BUTTON2_RELEASED=	NCURSES_MOUSE_MASK(2, NCURSES_BUTTON_RELEASED);
		internal static UInt64	BUTTON2_PRESSED=		NCURSES_MOUSE_MASK(2, NCURSES_BUTTON_PRESSED);
		internal static UInt64	BUTTON2_CLICKED=		NCURSES_MOUSE_MASK(2, NCURSES_BUTTON_CLICKED);
		internal static UInt64	BUTTON2_DOUBLE_CLICKED=	NCURSES_MOUSE_MASK(2, NCURSES_DOUBLE_CLICKED);
		internal static UInt64	BUTTON2_TRIPLE_CLICKED=	NCURSES_MOUSE_MASK(2, NCURSES_TRIPLE_CLICKED);
		
		internal static UInt64	BUTTON3_RELEASED =	NCURSES_MOUSE_MASK(3, NCURSES_BUTTON_RELEASED);
		internal static UInt64	BUTTON3_PRESSED=		NCURSES_MOUSE_MASK(3, NCURSES_BUTTON_PRESSED);
		internal static UInt64	BUTTON3_CLICKED=		NCURSES_MOUSE_MASK(3, NCURSES_BUTTON_CLICKED);
		internal static UInt64	BUTTON3_DOUBLE_CLICKED=	NCURSES_MOUSE_MASK(3, NCURSES_DOUBLE_CLICKED);
		internal static UInt64	BUTTON3_TRIPLE_CLICKED=	NCURSES_MOUSE_MASK(3, NCURSES_TRIPLE_CLICKED);
		
		internal static UInt64	BUTTON4_RELEASED=	NCURSES_MOUSE_MASK(4, NCURSES_BUTTON_RELEASED);
		internal static UInt64	BUTTON4_PRESSED=		NCURSES_MOUSE_MASK(4, NCURSES_BUTTON_PRESSED);
		internal static UInt64	BUTTON4_CLICKED=		NCURSES_MOUSE_MASK(4, NCURSES_BUTTON_CLICKED);
		internal static UInt64	BUTTON4_DOUBLE_CLICKED=	NCURSES_MOUSE_MASK(4, NCURSES_DOUBLE_CLICKED);
		internal static UInt64	BUTTON4_TRIPLE_CLICKED=	NCURSES_MOUSE_MASK(4, NCURSES_TRIPLE_CLICKED);
				
		internal static UInt64	BUTTON_CTRL=		NCURSES_MOUSE_MASK(6, 0001L);
		internal static UInt64	BUTTON_SHIFT=		NCURSES_MOUSE_MASK(6, 0002L);
		internal static UInt64	BUTTON_ALT=		NCURSES_MOUSE_MASK(6, 0004L);
		internal static UInt64	REPORT_MOUSE_POSITION=	NCURSES_MOUSE_MASK(6, 0010L);
		
		internal static UInt64	ALL_MOUSE_EVENTS=	(REPORT_MOUSE_POSITION - 1);
		
		/* macros to extract single event-bits from masks */
		internal static bool 	BUTTON_RELEASE(UInt64 e, int x) {	return	((e) & NCURSES_MOUSE_MASK(x, 001)) != 0; }
		internal static bool	BUTTON_PRESS(UInt64 e, int x)	{ return	((e) & NCURSES_MOUSE_MASK(x, 002)) != 0; }
		internal static bool 	BUTTON_CLICK(UInt64 e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 004)) != 0; }
		internal static bool	BUTTON_DOUBLE_CLICK(UInt64 e, int x) {return	((e) & NCURSES_MOUSE_MASK(x, 010)) != 0; }
		internal static bool	BUTTON_TRIPLE_CLICK(UInt64 e, int x) { return	((e) & NCURSES_MOUSE_MASK(x, 020)) != 0; }
		internal static bool	BUTTON_RESERVED_EVENT( UInt64 e, int x) { return ((e) & NCURSES_MOUSE_MASK(x, 040)) != 0; }
		
		[DllImport("libncursesw.so.5")]
		internal static extern UInt64 mousemask(UInt64 mask, IntPtr currentMaskPtr);
		
		#endregion
		
		public LinuxConsoleApplication ()
		{
		}
		
	}
}

