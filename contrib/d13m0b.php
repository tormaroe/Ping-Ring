<?php

$SHM_KEY = ftok( __FILE__, chr(1) );
$handle = sem_get( $SHM_KEY );
$buffer = shm_attach( $handle, 1024 );

$thisPort = $argv[1];
$otherPort = $argv[2];
$alarmTreshold = $argv[3];
$sendInitialPing = $argv[4];

$status = NULL;
$children = array();

echo "Starting up, listening on {$thisPort} and sending to {$otherPort}\n";

abstract class RingPingServer
{
	protected $thisPort;
	protected $otherPort;
	protected $alarmTreshold;
	protected $sendInitialPing;

	abstract function Process();

	function __construct( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing )
	{
		$this->thisPort = $thisPort;
		$this->otherPort = $otherPort;
		$this->alarmTreshold = $alarmTreshold;
		$this->sendInitialPing = $sendInitialPing;

		// Server has not received any ping when starting up
		setReceivedPing( false );
	}

	function SendPing( $port )
	{
		sleep( 1 );

		$message = "PING from {$this->thisPort}";
		if( false !== ($socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP)) && false !== @socket_connect($socket, "127.0.0.1", $port) )
		{
			socket_write( $socket, $message, strlen($message) );
			socket_close( $socket );
		}
		else
		{
			echo "*** Failed sending ping: {$message}\n";
		}
	}
}

class PingAlertProcesser extends RingPingServer
{
	function __construct( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing )
	{
		parent::__construct( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing );

		if( $this->sendInitialPing )
		{
			echo "Sending initial ping\n";
			$this->SendPing( $this->otherPort );
		}

		// Set ping time to equal startup time
		setLastPing( mktime() );
	}

	function Process()
	{
		while( true )
		{
			$lastPing = getLastPing();
			$delay = mktime() - $lastPing;

			if( $delay > $this->alarmTreshold )
			{
				echo "*** ALERT, RING BROKEN! No ping in {$delay} seconds!\n";

				if( hasReceivedPing() )
					$this->SendPing( $this->otherPort );
			}
			sleep( 1 );
		}
	}
}
 
class PingListener extends RingPingServer
{
	private $socket;

	function __construct( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing )
	{
		parent::__construct( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing );
   
		if( false == ($this->socket = @socket_create_listen($this->thisPort)) )
		{
			echo "Could not start listening on port {$this->thisPort}\n";
			exit();
		}
	}

	function ProcessPing( $string )
	{
		// Set new lastping
		setLastPing( mktime() );

		// We've received a ping
		setReceivedPing( true );

		// Relay ping to remote server
		$this->SendPing( $this->otherPort );
	}

	function Process()
	{
		while( true )
		{
			if( false !== ($client = socket_accept($this->socket)) )
			{
				$msg = socket_read( $client, 1024 );	    
				socket_close( $client );

				echo $msg."\n";
				$this->ProcessPing( $msg );
			}
		}
	}
}

$services = array();
$services[] = new PingAlertProcesser( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing );
$services[] = new PingListener( $thisPort, $otherPort, $alarmTreshold, $sendInitialPing );

for( $i=0; $i<sizeof($services); $i++ )
{
	$pid = pcntl_fork();
    
    if( $pid == -1 )
	{
		die( "Could not fork.\n" );
	}
    elseif( $pid )
    {
		$children[] = $pid;
    }
    else
    {
		$services[$i]->Process();
    }
}

while(true)
{
	sleep(1);
    
    while( pcntl_wait(&$status, WNOHANG || WUNTRACED) > 0 )
		usleep( 5000 );

    while( list($key, $val) = each($children) )
    {
		if( !posix_kill($val, 0) )
		{
			echo "Removing dead kid\n";
			unset( $children[$key] );
		}
    }
    $children = array_values($children);
  }

function hasReceivedPing()
{
	global $handle;
	global $buffer;

	sem_acquire( $handle );
	$hasReceivedPing = shm_get_var($buffer, 2 );
	sem_release( $handle );

	return $hasReceivedPing;
}

function setReceivedPing( $value )
{
	global $handle;
	global $buffer;

	sem_acquire( $handle );
	shm_put_var( $buffer, 2, $value );
	sem_release( $handle );
}

function getLastPing()
{
	global $handle;
	global $buffer;

	sem_acquire( $handle );
	$lastPing = shm_get_var($buffer, 1 );
	sem_release( $handle );

	return $lastPing;
}

function setLastPing( $value )
{
	global $handle;
	global $buffer;

	sem_acquire( $handle );
	shm_put_var( $buffer, 1, $value );
	sem_release( $handle );
}
?>

