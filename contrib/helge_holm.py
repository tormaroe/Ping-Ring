import socket
import sys
import time
import traceback
from threading import Thread

def listen(myPort, nextPort, maxDelay):
  def receive_ping(s):
    conn, _ = s.accept()
    print "Received:", conn.recv(1024)
  s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  s.settimeout(maxDelay)
  s.bind(('127.0.0.1', myPort))
  s.listen(1)
  lastPing = time.time()
  while True:
    try:
      receive_ping(s)
      lastPing = time.time()
      ping(myPort, nextPort)
    except socket.timeout:
      print "*** ALERT, RING BROKEN! No ping in %0.2fs."%(time.time()-lastPing)

def ping(myPort, nextPort):
  time.sleep(1)
  s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  try:
    s.connect(('127.0.0.1', nextPort))
    s.send("PING from %d"%myPort)
  except:
    print "*** Failed sending ping!"
    traceback.print_exc()

this_port = int(sys.argv[1])
other_port = int(sys.argv[2])
max_delay = int(sys.argv[3])
initial_ping = sys.argv[4] == "true"

print "** Python Ring Server (", this_port, ")"

if initial_ping: ping(this_port, other_port)
Thread(target=listen, args=(this_port, other_port, max_delay)).start()

