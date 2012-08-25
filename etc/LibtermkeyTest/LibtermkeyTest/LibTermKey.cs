using System;
using System.Runtime.InteropServices;

namespace Linux.Native
{
	public static class LibTermKey
	{
		/// <summary>
		/// see the <sys/poll.h> and <bits/poll.h>
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int poll( pollfd[] fds, int fdsCount, int timeout);
		
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int eventfd(uint initval, EVENTFD_FLAGS flags);
		
		[DllImport("libc.so.6", SetLastError = true)]
		private static extern int read(int fd, out UInt64 buf, int count);
		
		[DllImport("libc.so.6", SetLastError = true)]
		private static extern int write(int fd, ref UInt64 buf, int count);
		
		public static int readInt64(int fd, out UInt64 res) {
			return read (fd, out res, sizeof(UInt64));
		}
		
		public static int writeInt64(int fd, UInt64 u) {
			return write(fd, ref u, sizeof(UInt64));
			//IntPtr buf = Marshal.AllocHGlobal(sizeof(UInt64));
			//try {
			//	Marshal.Copy(new long[] { (long) u }, 0, buf, 1);
			//	return write(fd, buf, sizeof(UInt64));
			//} finally {
			//	Marshal.FreeHGlobal(buf);
			//}
		}
		
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int close(int fd);
		
		// todo : expand flags into the enumeration
		[DllImport("libtermkey.so")]
		public static extern IntPtr termkey_new(int fd, TermKeyFlag flags);
		
		[DllImport("libtermkey.so")]
		public static extern TermKeyResult termkey_getkey_force(IntPtr termKey, ref TermKeyKey key);

		[DllImport("libtermkey.so")]
		public static extern TermKeyResult termkey_getkey(IntPtr termKey, ref TermKeyKey key);

		[DllImport("libtermkey.so")]
		public static extern TermKeyResult termkey_advisereadable(IntPtr termKey);
		
		[DllImport("libtermkey.so")]
		public static extern int termkey_get_waittime(IntPtr termkey);
		
		[DllImport("libtermkey.so")]
		public static extern void termkey_destroy(IntPtr termkey);
		
		[DllImport("libtermkey.so")]
		public static extern TermKeyResult termkey_interpret_mouse(IntPtr termKey, ref TermKeyKey key,
		                                                           out TermKeyMouseEvent ev,
		                                                           out int button,
		                                                           out int line,
		                                                           out int col);
	}
	
	[Flags]
	public enum EVENTFD_FLAGS : int {
		EFD_SEMAPHORE = 0x00000001,
		EFD_CLOEXEC =   0x00080000,
		EEFD_NONBLOCK = 0x00000800
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct pollfd {
		public int fd;
		public POLL_EVENTS events;
		public POLL_EVENTS revents;
	}
	
	
	[Flags]
	public enum POLL_EVENTS : ushort {
		NONE = 0x0000,
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
	
	[Flags]
	public enum TermKeyFlag {
		TERMKEY_FLAG_NOINTERPRET = 1 << 0, /* Do not interpret C0//DEL codes if possible */
		TERMKEY_FLAG_CONVERTKP   = 1 << 1, /* Convert KP codes to regular keypresses */
		TERMKEY_FLAG_RAW         = 1 << 2, /* Input is raw bytes, not UTF-8 */
		TERMKEY_FLAG_UTF8        = 1 << 3, /* Input is definitely UTF-8 */
		TERMKEY_FLAG_NOTERMIOS   = 1 << 4, /* Do not make initial termios calls on construction */
		TERMKEY_FLAG_SPACESYMBOL = 1 << 5, /* Sets TERMKEY_CANON_SPACESYMBOL */
		TERMKEY_FLAG_CTRLC       = 1 << 6, /* Allow Ctrl-C to be read as normal, disabling SIGINT */
		TERMKEY_FLAG_EINTR       = 1 << 7  /* Return ERROR on signal (EINTR) rather than retry */
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
	
	// why sizeof it is 8 ?
	[StructLayout(LayoutKind.Explicit)]
	public struct code {
		// NOT long ! actually int
		[FieldOffset(0)]
		public int codepoint; /* TERMKEY_TYPE_UNICODE */
		[FieldOffset(0)]
		public int number; /* TERMKEY_TYPE_FUNCTION */
		[FieldOffset(0)]
		public TermKeySym sym; /* TERMKEY_TYPE_KEYSYM */
		[FieldOffset(0)]
		public byte mouse0; /* TERMKEY_TYPE_MOUSE (char[4]) */
		[FieldOffset(1)]
		public byte mouse1;
		[FieldOffset(2)]
		public byte mouse2;
		[FieldOffset(3)]
		public byte mouse3;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public struct TermKeyKey {
		[FieldOffset(0)]
		public TermKeyType type;
		// sizeof(code) must be 4, but if use a Sequential layout
		// it will be 8, so we have to explicitly specify the offsets
		[FieldOffset(4)]
		public code code;
		[FieldOffset(8)]
		public int modifiers;
		
		/* char[7] = Any Unicode character can be UTF-8 encoded in no more than 6 bytes, plus terminating NUL */
		[FieldOffset(12 + 0)]
		public byte utf8_0;
		[FieldOffset(12 + 1)]
		public byte utf8_1;
		[FieldOffset(12 + 2)]
		public byte utf8_2;
		[FieldOffset(12 + 3)]
		public byte utf8_3;
		[FieldOffset(12 + 4)]
		public byte utf8_4;
		[FieldOffset(12 + 5)]
		public byte utf8_5;
		[FieldOffset(12 + 6)]
		public byte utf8_6;
	}
}

