#!/usr/bin/python
# vim: set encoding=utf-8
from functools import wraps
import sys
from time import time, sleep
from threading import Thread
import socket

if len(sys.argv) < 5:
    raise SystemExit("Too few arguments")

myport, nextport, timeout = [int(i) for i in sys.argv[1:4]]
startPinging = sys.argv[4] == 'true'
lastping = time()

def threading(f): # Lurer på om denne ikke finnes i et bibliotek.
    @wraps(f)     # (Får funksjonen til å kjøre i en tråd)
    def wrap():
        class AThread(Thread):
            run = f
        AThread().start()
    return wrap

@threading
def receiver(self):
    global lastping
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(("127.0.0.1", myport))
    sock.listen(5)
    while True:
        (s, _) = sock.accept()
        print s.recv(1024)
        lastping = time()
        sendPing()
        s.close()

@threading
def sendPing(self):
    sleep(1)
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.connect(('127.0.0.1', nextport))
        sock.send("Ping from " + str(myport))
        sock.close()
    except Exception:
        print "** Couldn't send ping."

@threading
def surveillance(self):
    while True:
        sleep(5)
        if time() - lastping > timeout:
            print "** No ping in %.1f seconds." % (time() - lastping)
            sendPing()

if __name__ == '__main__':
    receiver()
    surveillance()
    if startPinging:
        sendPing()

