using System;
using System.Runtime.InteropServices;

namespace Linux.Native
{
	public static class LibTermKey
	{
		/// <summary>
		/// see the <sys/poll.h> and <bits/poll.h>
		/// </summary>
		[DllImport("libc.so")]
		public static extern int poll([MarshalAs(UnmanagedType.LPArray)] pollfd[] fds, int fdsCount, int timeout);
		
		// todo : expand flags into the enumeration
		[DllImport("libtermkey.so")]
		public static extern int termkey_new(int fd, int flags);
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct pollfd {
		int fd;
		POLL_EVENTS events;
		POLL_EVENTS revents;
	}
	
	
	[Flags]
	public enum POLL_EVENTS : ushort {
		POLLIN = 0x001,
		POLLPRI = 0x002,
		POLLOUT = 0x004,
		POLLMSG = 0x400,
		POLLREMOVE = 0x1000,
		POLLRDHUP = 0x2000,
		// output only
		POLLERR = 0x008,
		POLLHUP = 0x010,
		POLLNVAL = 0x020
	}
	
	public enum TermKeyResult {
		TERMKEY_RES_NONE,
		TERMKEY_RES_KEY,
		TERMKEY_RES_EOF,
		TERMKEY_RES_AGAIN,
		TERMKEY_RES_ERROR
	}
	
	public enum TermKeyType {
		TERMKEY_TYPE_UNICODE,
		TERMKEY_TYPE_FUNCTION,
		TERMKEY_TYPE_KEYSYM,
		TERMKEY_TYPE_MOUSE,
		TERMKEY_TYPE_POSITION
	}
	
	public enum TermKeySym {
		TERMKEY_SYM_UNKNOWN = -1,
		TERMKEY_SYM_NONE = 0,
		
		/* Special names in C0 */
		TERMKEY_SYM_BACKSPACE,
		TERMKEY_SYM_TAB,
		TERMKEY_SYM_ENTER,
		TERMKEY_SYM_ESCAPE,
		
		/* Special names in G0 */
		TERMKEY_SYM_SPACE,
		TERMKEY_SYM_DEL,
		
		/* Special keys */
		TERMKEY_SYM_UP,
		TERMKEY_SYM_DOWN,
		TERMKEY_SYM_LEFT,
		TERMKEY_SYM_RIGHT,
		TERMKEY_SYM_BEGIN,
		TERMKEY_SYM_FIND,
		TERMKEY_SYM_INSERT,
		TERMKEY_SYM_DELETE,
		TERMKEY_SYM_SELECT,
		TERMKEY_SYM_PAGEUP,
		TERMKEY_SYM_PAGEDOWN,
		TERMKEY_SYM_HOME,
		TERMKEY_SYM_END,
		
		/* Special keys from terminfo */
		TERMKEY_SYM_CANCEL,
		TERMKEY_SYM_CLEAR,
		TERMKEY_SYM_CLOSE,
		TERMKEY_SYM_COMMAND,
		TERMKEY_SYM_COPY,
		TERMKEY_SYM_EXIT,
		TERMKEY_SYM_HELP,
		TERMKEY_SYM_MARK,
		TERMKEY_SYM_MESSAGE,
		TERMKEY_SYM_MOVE,
		TERMKEY_SYM_OPEN,
		TERMKEY_SYM_OPTIONS,
		TERMKEY_SYM_PRINT,
		TERMKEY_SYM_REDO,
		TERMKEY_SYM_REFERENCE,
		TERMKEY_SYM_REFRESH,
		TERMKEY_SYM_REPLACE,
		TERMKEY_SYM_RESTART,
		TERMKEY_SYM_RESUME,
		TERMKEY_SYM_SAVE,
		TERMKEY_SYM_SUSPEND,
		TERMKEY_SYM_UNDO,
		
		/* Numeric keypad special keys */
		TERMKEY_SYM_KP0,
		TERMKEY_SYM_KP1,
		TERMKEY_SYM_KP2,
		TERMKEY_SYM_KP3,
		TERMKEY_SYM_KP4,
		TERMKEY_SYM_KP5,
		TERMKEY_SYM_KP6,
		TERMKEY_SYM_KP7,
		TERMKEY_SYM_KP8,
		TERMKEY_SYM_KP9,
		TERMKEY_SYM_KPENTER,
		TERMKEY_SYM_KPPLUS,
		TERMKEY_SYM_KPMINUS,
		TERMKEY_SYM_KPMULT,
		TERMKEY_SYM_KPDIV,
		TERMKEY_SYM_KPCOMMA,
		TERMKEY_SYM_KPPERIOD,
		TERMKEY_SYM_KPEQUALS,
		
		/* et cetera ad nauseum */
		TERMKEY_N_SYMS
	}
	
	public enum TermKeyMouseEvent {
		TERMKEY_MOUSE_UNKNOWN,
		TERMKEY_MOUSE_PRESS,
		TERMKEY_MOUSE_DRAG,
		TERMKEY_MOUSE_RELEASE
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct code {
		[FieldOffset(0)]
		public long codepoint; /* TERMKEY_TYPE_UNICODE */
		[FieldOffset(0)]
		public int number; /* TERMKEY_TYPE_FUNCTION */
		[FieldOffset(0)]
		public TermKeySym sym; /* TERMKEY_TYPE_KEYSYM */
		[FieldOffset(0)]
		public char[] mouse; /* TERMKEY_TYPE_MOUSE (char[4]) */
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct TermKeyKey {
		public TermKeyType type;
		public code code;
		int modifiers;
		
		/* char[7] = Any Unicode character can be UTF-8 encoded in no more than 6 bytes, plus terminating NUL */
		char[] utf8;
	}
}

