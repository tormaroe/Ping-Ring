import sys
import time
import datetime
import socket
from threading import Thread

last_ping_time = datetime.datetime.now() # Global state 

class TcpThread(Thread):
	def __init__ (self, self_port, other_port):
		Thread.__init__(self)
		self.port = self_port
		self.other = other_port
	def get_socket(self):
		return socket.socket(socket.AF_INET, socket.SOCK_STREAM)

class Listener(TcpThread):
	def run(self):
		while 1:
			s = self.get_socket()
			s.bind(('127.0.0.1', self.port))
			s.listen(1)
			conn, addr = s.accept()
			ping = conn.recv(1024)
			self.process(ping)
			conn.close()
	def process(self, message):
		global last_ping_time
		last_ping_time = datetime.datetime.now()
		print "Received", message
		Pinger(self.port, self.other).start()

class Pinger(TcpThread):
	def run(self):
		time.sleep(1)
		s = self.get_socket()
		try:
			s.connect(('127.0.0.1', self.other))
			s.send('PING from %(port)s' % {'port': self.port})
			s.close()
		except Exception:
			print "*** Failed sending ping!"

class Alerter(TcpThread):
	def __init__ (self, self_port, other_port, max_delay):
		TcpThread.__init__(self, self_port, other_port)
		self.max_delay = datetime.timedelta(seconds=max_delay)
	def run(self):
		global last_ping_time
		while 1:
			time.sleep(5)
			ping_delay = datetime.datetime.now() - last_ping_time
			if ping_delay > self.max_delay:
				print '*** ALERT, RING BROKEN!' + \
						' No ping in %(delay)s.' % {'delay': ping_delay}
				Pinger(self.port, self.other).start()

this_port = int(sys.argv[1])
other_port = int(sys.argv[2])
max_delay = int(sys.argv[3])
initial_ping = sys.argv[4] == "true"

print "** Python Ring Server (", this_port, ")"

if initial_ping: Pinger(this_port, other_port).start()
Listener(this_port, other_port).start()
Alerter(this_port, other_port, max_delay).start()
