#include <iostream>
#include <curses.h>

using namespace std;

int main()
{
    WINDOW* stdscr = initscr();
    cbreak();
    noecho();
    nonl();
    intrflush(stdscr, false);
    keypad(stdscr, true);
    start_color();
    mvaddstr(4, 3, "Test!");
    refresh();
    getch();
    endwin();
}
