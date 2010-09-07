require 'socket'

class RingServer
	def initialize
		@self_port = ARGV.shift
		@other_port = ARGV.shift
		@max_delay = ARGV.shift.to_i
		@should_send_startup_ping = ARGV.shift == "true"
		@last_ping_time = Time.now
		puts "** Ruby Ring Server (#{@self_port})"
	end
	def start
		send_delayed_ping if @should_send_startup_ping
		start_missing_ping_alert_thread
		start_ping_listener_thread
		Thread.list.each { |t| t.join unless t == Thread.main }
	end
	def send_delayed_ping
		Thread.new do
			sleep 1
			begin
				client = TCPSocket.new('127.0.0.1', @other_port)
				client.print "Ping from #{@self_port}"
			rescue
				puts "*** Failed sending ping: #{$!}"
			else
				client.close
			end			
		end
	end
	def start_missing_ping_alert_thread
		Thread.new do
			while true
				sleep 5
				ping_delay = Time.now - @last_ping_time
				if ping_delay > @max_delay
					puts "*** ALERT, RING BROKEN! No ping in #{ping_delay} seconds."
					send_delayed_ping # try to wake up ring
				end
			end	
		end
	end
	def start_ping_listener_thread
		Thread.new do
			listener = TCPServer.new('127.0.0.1', @self_port)
			while session = listener.accept
				process_incoming_ping session.gets
				session.close
			end
		end
	end
	def process_incoming_ping message
		@last_ping_time = Time.now
		puts "Received #{message}"
		send_delayed_ping
	end
end

RingServer.new.start
