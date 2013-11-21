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
        /// <summary>
		/// See the &lt;sys/poll.h&gt; and &lt;bits/poll.h&gt;
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int poll( pollfd[] fds, int fdsCount, int timeout);
		
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
		public const Int32 TIOCGWINSZ = 0x5413;
		
		/// <summary>
		/// Used in terminal size retrieving.
		/// </summary>
		[DllImport("libc.so.6", SetLastError = true)]
		public static extern int ioctl(int fd, int cmd, out winsize ws);
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
