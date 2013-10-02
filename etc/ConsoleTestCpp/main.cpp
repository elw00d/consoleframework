#include <iostream>
#include <curses.h>
#include <unistd.h>
#include <signal.h>
#include <locale.h>
#include <termios.h>
#include <sys/ioctl.h>
#include <sys/eventfd.h>
#include <poll.h>

using namespace std;

void sig(int n)
{
}

int fd, fd2;

void *functionC(void*);

int main()
{
    SCREEN* screen = newterm("xterm-1003", stdin, stdout);
    set_term(screen);
    setlocale(LC_ALL, "");
    //signal(SIGINT, sig);
    cout << "Hello world!" << endl;
    WINDOW* window = initscr();
    cbreak();
    noecho();
    nonl();
    intrflush(stdscr, FALSE);
    keypad(stdscr, TRUE);
    //
    mousemask(ALL_MOUSE_EVENTS | REPORT_MOUSE_POSITION, NULL);
    start_color();
    init_pair(1, COLOR_BLUE, COLOR_GREEN);
    init_pair(2, COLOR_BLUE, COLOR_BLACK);
    attron(COLOR_PAIR(1));
    //char buffer[40];
    //sprintf(buffer, "%s", "Hello ncurses ! фывфыв russian text,");
    addstr("lksjdf ыловаыва\u2591");
    refresh();

    long c = '\u2591';
    cchar_t ch;
    ch.attr = 1;
    ch.chars[0] = ' ';
    ch.chars[1] = '\u2591';
    ch.chars[2] = 'f';
    ch.chars[3] = 'x';
    ch.chars[4] = 'n';
    //mvadd_wch(3, 3, &ch);
    attroff(COLOR_PAIR(2));//attron(COLOR_PAIR(2));
    void* _stdscr = stdscr;
//    if (window != _stdscr ) {
 //       printw("%s", "assertion failed");
   // }
    addstr("lksjdf ыловаыва\u2591");
    //mousemask(BUTTON1_CLICKED)
    //chtype
    //addwstr((const wchar_t*) "Hello ncurses ! ываыва");
    //char text[]="Русский UTF-8 текст\n";
    //printw("%s",text);
    refresh();
    int c1 = getch();
    refresh();
    int c2 = getch();
    refresh();
    int c3 = getch();
    refresh();
    int c4 = getch();
    refresh();
    int c5 = getch();
    refresh();
    int cc  = KEY_MOUSE;

    //getch();
    endwin();
    printf("%d %d %d %d %d %d", c1, c2, c3, c4, c5, cc);
    if (c1 == KEY_MOUSE) {
        printf("Mouse.");
    }
    return 0;

    printf("%x\n", EFD_NONBLOCK);
    fd = eventfd(0, EFD_CLOEXEC);
    if (fd == -1) {
        printf("%s", "eventfd returned -1");
        return -1;
    }
    printf("fd = %x\n", fd);
    pthread_t thread1;
    int rc1;
    if( (rc1=pthread_create( &thread1, NULL, &functionC, NULL)) ) {
        printf("Thread creation failed: %d\n", rc1);
    }

    fd2 = eventfd(0, EFD_CLOEXEC);
    printf("fd2 = %x\n", fd2);
    if (fd2 == -1) {
        printf("%s", "eventfd returned -1");
        return -1;
    }

    // осталось научиться определять, какой из файловых дескрипторов сработал
    pollfd pollfds[2];
    pollfds[0].fd = fd2;
    pollfds[0].events = POLLIN;
    pollfds[1].fd = fd;
    pollfds[1].events = POLLIN;

    bool flag = false;
    for (int k = 0; k < 10; k++) {
        int pollResult = poll(pollfds, 2, 1000);
        printf("poll returned %d\n", pollResult);
        // определим сработавшие файловые дескрипторы
        if (pollResult > 0) {
            for (int i = 0; i < 2; i++) {
                printf("pollfds[%d].revents = %X\n", i, pollfds[i].revents);
                if (pollfds[i].revents != 0 && flag && i == 0) {
                    printf("Reading 1 time to clear the eventfd counter..\n");
                    uint64_t u;
                    read(i == 0 ? fd2 : fd, &u, sizeof(uint64_t));
                }
            }
            if (pollfds[0].revents != 0 && pollfds[1].revents != 0)
                flag = true;
            if (!flag)
                sleep(1);
        }
    }

    /*int pollResult = poll(pollfds, 2, -1);
    printf("poll returned %d\n", pollResult);
    // определим сработавшие файловые дескрипторы
    if (pollResult > 0) {
        for (int i = 0; i < 2; i++) {
            printf("pollfds[%d].revents = %X\n", i, pollfds[i].revents);
        }
    }
    //
    printf("Reading 1 time to clear the eventfd counter..");
    uint64_t u;
    read(fd, &u, sizeof(uint64_t));
    //
    pollResult = poll(pollfds, 2, -1);
    printf("poll returned %d\n", pollResult);
    // определим сработавшие файловые дескрипторы
    if (pollResult > 0) {
        for (int i = 0; i < 2; i++) {
            printf("pollfds[%d].revents = %X\n", i, pollfds[i].revents);
        }
    }*/

    //pthread_join(thread1, NULL);
    //close(fd);
    //close(fd2);
    //return 0;
}

void *functionC(void* data) {
    sleep(1);
    {
        uint64_t u = 2;
        write(fd, &u, sizeof(uint64_t));
    }
    for (int i = 0; i < 5; i++) {
        printf("Thread message : %d\n", i);
        sleep(1);
        uint64_t u = 2;
        write(fd2, &u, sizeof(uint64_t));
    }
    {
        uint64_t u = 5;
        write(fd2, &u, sizeof(uint64_t));
    }
}
