using System;
using System.Runtime.InteropServices;

namespace Linux.Native
{
    /// <summary>
    /// Interop code for LibTermKey linux library.
    /// LibTermKey is used for handling keyboard and mouse input.
    /// </summary>
	public static class LibTermKey
	{
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


