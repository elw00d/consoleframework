using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using Linux.Native;

namespace LibtermkeyTest
{
	class MainClass
	{
		private static int eventfd;
		
		public static void Main (string[] args)
		{
			Thread thread = new Thread(new ThreadStart(() => {
				Thread.Sleep(TimeSpan.FromSeconds(5));
				Console.WriteLine("Message from thread");
				int res = LibTermKey.writeInt64(eventfd, 1);
				Console.WriteLine("write(1) returned {0}\n", res);
				if (res == -1) {
					int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
					Console.WriteLine("Last error is {0}\n", lastError);
				}
			}));
			thread.IsBackground = true;
			thread.Start();
			
			IntPtr handle = LibTermKey.termkey_new(0, TermKeyFlag.TERMKEY_FLAG_SPACESYMBOL);
			Console.Write("\x1B[?1002h");
			pollfd fd = new pollfd();
			fd.fd = 0;
			fd.events = POLL_EVENTS.POLLIN;
			
			pollfd[] fds = new pollfd[2];
			fds[0] = fd;
			
			fds[1] = new pollfd();
			eventfd = LibTermKey.eventfd(0, EVENTFD_FLAGS.EFD_CLOEXEC);
			if (eventfd == -1) {
				Console.WriteLine("Cannot create eventfd\n");
				int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
				Console.WriteLine("Last error is {0}\n", lastError);
			}
			fds[1].fd = eventfd;
			fds[1].events = POLL_EVENTS.POLLIN;
			
			TermKeyKey key = new TermKeyKey();
			while (true) {
				int pollRes = LibTermKey.poll(fds, 2, -1);
				if (0 == pollRes) {
					// timed out
					Console.WriteLine("Timed out");
					if (LibTermKey.termkey_getkey_force(handle, ref key) == TermKeyResult.TERMKEY_RES_KEY) {
						Console.WriteLine("got TERMKEY_RES_KEY");
					}					
				} else if (-1 == pollRes) {
					int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
					Console.WriteLine(string.Format("ErrorCode = {0}", errorCode));
				}
				Console.WriteLine(string.Format("PollRes is {0}", pollRes));
				
				for (int i = 0; i < 2; i++) {
					if (fds[i].revents != POLL_EVENTS.NONE) {
						if (i == 1) {
							UInt64 u;
							LibTermKey.readInt64(fds[i].fd, out u);
							Console.WriteLine("Readed eventfd counter : {0}\n", u);
						}
					}
				}
				
				if ((fds[0].revents & POLL_EVENTS.POLLIN) == POLL_EVENTS.POLLIN ||
				    (fds[0].revents & POLL_EVENTS.POLLHUP) == POLL_EVENTS.POLLHUP ||
				    (fds[0].revents & POLL_EVENTS.POLLERR) == POLL_EVENTS.POLLERR)
				{
					// todo : log return value
					LibTermKey.termkey_advisereadable(handle);
				}
				
				TermKeyResult result;
				while ((result = LibTermKey.termkey_getkey(handle, ref key)) == TermKeyResult.TERMKEY_RES_KEY) {
					Console.WriteLine("Received some key.");
					string descr = String.Format("Type : {0} Modifiers: {1} Utf8 bytes: {2}{3}{4}{5}{6}{7}{8}",
					              key.type, key.modifiers, key.utf8_0,
					              key.utf8_1, key.utf8_2, key.utf8_3,
					              key.utf8_4, key.utf8_5, key.utf8_6);
					//dump the retrieved structure
					//byte[] buffer = new byte[30];
					//IntPtr nativeBuffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(30);
					//System.Runtime.InteropServices.Marshal.StructureToPtr(key, nativeBuffer, false);
					//System.Runtime.InteropServices.Marshal.Copy(nativeBuffer, buffer, 0, 30);
					//for (int i = 0; i < 30; i++ ) {
					//	Console.Write("{0} ", buffer[i]);
					//	if ((i + 1) % 10 == 0)
					//		Console.WriteLine();
					//}
					//System.Runtime.InteropServices.Marshal.FreeHGlobal(nativeBuffer);
					Console.WriteLine(descr);
					if (key.type == TermKeyType.TERMKEY_TYPE_UNICODE) {
						byte[] data = new byte[7];
						data[0] = key.utf8_0;
						data[1] = key.utf8_1;
						data[2] = key.utf8_2;
						data[3] = key.utf8_3;
						data[4] = key.utf8_4;
						data[5] = key.utf8_5;
						data[6] = key.utf8_6;
						string d = System.Text.Encoding.UTF8.GetString(data);
						Console.WriteLine(String.Format("Unicode symbol : {0}", d));
					} else if (key.type == TermKeyType.TERMKEY_TYPE_MOUSE) {
						TermKeyMouseEvent ev;
						int button;
						int line, col;
						LibTermKey.termkey_interpret_mouse(handle, ref key, out ev, out button, out line, out col);
						Console.WriteLine("MouseEvent : {0} (button {1}) at {2}:{3}", ev, button, line, col);
					}
				}
			}
			
			LibTermKey.termkey_destroy(handle);
			LibTermKey.close(eventfd);
		}
	}
}
