using System;
using System.Runtime.InteropServices;

namespace ConsoleFramework.Native
{
    /// <summary>
    /// Interops for libc linux library.
    /// Used to interoperate with eventfd polling (linux analogue of WaitForMultipleObjects).
    /// </summary>
    public static class Libc
    {
	    public const int LC_ALL = 0;
	    
	    [DllImport("libc.so.6", SetLastError = true)]
	    public static extern string setlocale(int category, string locale);
	    
        /// <summary>
		/// See the &lt;sys/poll.h&gt; and &lt;bits/poll.h&gt;
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int poll( [In, Out] pollfd[] fds, int fdsCount, int timeout);

		/// <summary>
		/// Creates a pipe object. fds must be initialized array of 2 items.
		/// fds[0] will store descriptor for reading
		/// fds[1] will store descriptor for writing
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int pipe (int[] fds);

		/// <summary>
		/// Creates the eventfd kernel object. Returns file descriptor for
		/// created eventfd object.
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int eventfd(uint initval, EVENTFD_FLAGS flags);
		
		[DllImport("libc.so.6", SetLastError = true)]
		private static extern int read(int fd, out UInt64 buf, int count);
		
		[DllImport("libc.so.6", SetLastError = true)]
		private static extern int write(int fd, ref UInt64 buf, int count);
		
		/// <summary>
		/// Used to read from eventfd file descriptor.
		/// </summary>
		/// <returns>
		/// Number of bytes readed or -1 if error has occured.
		/// </returns>
		public static int readInt64(int fd, out UInt64 res) {
			return read (fd, out res, sizeof(UInt64));
		}
		
		/// <summary>
		/// Used to write to eventfd file descriptor.
		/// </summary>
		/// <returns>
		/// Number of bytes written or -1 if error has occured.
		/// </returns>
		public static int writeInt64(int fd, UInt64 u) {
			return write(fd, ref u, sizeof(UInt64));
		}
		
		/// <summary>
		/// Close the specified file descriptor.
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int close(int fd);
		
		// Used in terminal size retrieving
		public const Int32 STDIN_FILENO = 0;

		// For Linux it hardcoded to this constant
		public const Int32 TIOCGWINSZ_LINUX = 0x5413;

		// For Mac OS it is different
		// https://groups.google.com/forum/#!msg/golang-nuts/eZgB_2RUDmQ/nv9wgeIoja4J
		public const Int32 TIOCGWINSZ_DARWIN = 0x40087468;
		
		/// <summary>
		/// Used in terminal size retrieving.
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int ioctl(int fd, int cmd, out winsize ws);

        /// <summary>
        /// Interrupted system call. If after poll() error code is EINTR, this means
        /// that a signal was caught during poll().
        /// </summary>
        public const Int32 EINTR = 4;

        /// <summary>
        /// Returns actual terminal width and height.
        /// </summary>
        /// <param name="isDarwin">True if application is executed under Mac OS X.</param>
        /// <returns></returns>
        public static winsize GetTerminalSize( bool isDarwin ) {
            winsize ws;
            ioctl(STDIN_FILENO, isDarwin ? TIOCGWINSZ_DARWIN : TIOCGWINSZ_LINUX, out ws);
            return ws;
        }
	    
	    public delegate void SignalHandler(int arg);

	    [DllImport("libc.so.6", SetLastError = true)]
	    public static extern IntPtr signal(int signum, SignalHandler handler);

	    /// <summary>
	    /// Retrieves terminal parameters into termios structure.
	    /// </summary>
	    [DllImport("libc.so.6", SetLastError = true)]
	    public static extern int tcgetattr(int fd, [Out] out termios termios);
	    
	    /// <summary>
	    /// Upon successful completion, the functions tcgetattr() and tcsetattr() 
	    /// return a value of 0.  Otherwise, they return -1 and the global variable
	    /// errno is set to indicate the error.
	    /// </summary>
	    [DllImport("libc.so.6", SetLastError = true)]
	    public static extern int tcsetattr(int fd, int optional_actions, ref termios termios);
	    
		/// <summary>
		/// The change occurs immediately.
		/// </summary>
	    public const Int32 TCSANOW = 0;
	    
	    /// <summary>
	    /// The change occurs after all output written to fd has been transmitted.
	    /// This function should be used when changing parameters that affect output.
	    /// </summary>
	    public const Int32 TCSADRAIN = 1;
	    
	    /// <summary>
	    /// The change occurs after all output written to the object referred by fd has been transmitted,
	    /// and all input that has been received but not read will be discarded before the change is made.
	    /// </summary>
	    public const Int32 TCSAFLUSH = 2;
    }
    
	[StructLayout(LayoutKind.Sequential)]
	public struct termios
	{
		public UInt32 c_iflag;
		public UInt32 c_oflag;
		public UInt32 c_cflag;
		public UInt32 c_lflag;
		public Byte c_line;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public Byte[] c_cc;  // 32 items
		public UInt32 c_ispeed;
		public UInt32 c_ospeed;
	}
	
	/// <summary>
	/// Structure to retrieve terminal size.
	/// </summary>
	public struct winsize {
		public UInt16 ws_row;
		public UInt16 ws_col;
		public UInt16 ws_xpixel;
		public UInt16 ws_ypixel;
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
}
