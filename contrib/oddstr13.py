#!/usr/bin/python
# -*- coding: utf-8 -*-

__author__="Oddstr13"
__date__ ="$05.sep.2010 22:32:25$"

import socket
import thread
import random
import sys
import os
import time
from subprocess import *
import pprint
import datetime
import smtplib

"""
settings you most likley will want to change is marked by <<<
"""

conf = {}
conf['ipv6'] = 0
conf['remote'] = {}
conf['remote']['host'] = "localhost"              # --remote    <<<
conf['remote']['port'] = 50000                    #             <<<
conf['bind'] = {}
conf['bind']['host'] = "0.0.0.0"
conf['bind']['port'] = 50000                      # --listen    <<<
conf['lsocket'] = 0
conf['lastping'] = 0
conf['lastpong'] = 0
conf['alarm'] = {}
conf['alarm']['timeout'] = 120                    # --timeout   <<<
conf['alarm']['spacing'] = 3600    #alert every <num> secound
conf['alarm']['ping'] = {
                         'trigged': 0,
                         'lastsendt': 0,
                         'tmplastping': 0,
                         'ip':"0.0.0.0",
                         'hostname':"none"
                        }
conf['alarm']['pong'] = {
                         'trigged': 0,
                         'lastsendt': 0,
                         'tmplastpong': 0,
                         'ip':"0.0.0.0",
                         'hostname':"none"
                        }
conf['mail'] = {}
conf['mail']['server'] = "smtp.example.com"      # --mailhost
conf['mail']['toaddr'] = "mail@example.com"      # --toaddr        <<<
conf['mail']['fromaddr'] = ""                    # --fromaddr,
#    defaults to PingRing@<name of machine>
conf['mail']['enabled'] = 0                      # --mail|--nomail <<<

if conf['ipv6']:
    v_af_inet_inet6 = socket.AF_INET6
else:
    v_af_inet_inet6 = socket.AF_INET

def connect(host, port, ipv6=0, bind="0.0.0.0", bindport=0):
    if ipv6:
        af_inet = socket.AF_INET6
    else:
        af_inet = socket.AF_INET
    if not bindport:
        bindport = random.choice(range(40000, 50000))
    s = None
    for res in socket.getaddrinfo(bind, bindport, af_inet, socket.SOCK_STREAM):
        vhost = res[4]
        for res in socket.getaddrinfo(host, port, af_inet, socket.SOCK_STREAM):
            af, socktype, proto, canonname, sa = res
            try:
                s = socket.socket(af, socktype, proto)
            except socket.error, e:
                s = None;error=e
                continue
            try:
                s.bind(vhost)
                s.connect(sa)
            except socket.error, e:
                s.close()
                s = None;error=e
                continue
            break
    if s == None:
        pass#print "[ERR]", error
    return s

def send(msg, s):
    if not s: raise Exception, "Expected socket, got", type(s)
    msg = msg + "\r\n"
    #print "<--", [msg]
    s.send(msg)

def cmd(cmd):
    p = Popen(cmd, shell=True, bufsize=0,
                    stdin=PIPE, stdout=PIPE, stderr=STDOUT, close_fds=True)
    result = p.stdout.read().strip()
    return result

def str_pop(input, popit):
    while popit in input:
        v0ind = input.find(popit)
        input = input[:v0ind] + input[v0ind+1:]
    return input

def strftime(formatstring, unixtime = 0):
    if not unixtime:
        unixtime = time.time()
    x = time.localtime(float(unixtime))
    date = datetime.datetime(x[0],x[1],x[2],x[3],x[4],x[5])
    return date.strftime(formatstring)
    #'06. 09. 2010 01:23:04'


def process_con(conn, addr):
    #print 'Connected by', addr, "in thread", thread.get_ident()
    conn.settimeout(300)
    try:
        while 1:
            data = conn.recv(1024)
            if not data: break
            for line in data.strip().split("\n"):
                #print "-->", [line]
                line = line.strip()
                if line.upper().startswith("PING"):
                    print "Ping from %s (%s),\
remote info: Platform: %s Name: %s Time: %s" %(
                        line.split(":")[1], addr[0], line.split(":")[2], 
                        line.split(":")[3], strftime("%Y-%m-%d %H:%M:%S", 
                        line.split(":")[4]))
                    send("PONG:%s:%s:%s:%i" %(socket.gethostname(), 
                        sys.platform, os.name, time.time()), conn)
                    conf['lastping'] = time.time()
                    conf['alarm']['ping']['ip'] = addr[0]
                    conf['alarm']['ping']['hostname'] = line.split(":")[1]
    except Exception, e:
        conn.close()
        #print "[ERR]", e
    conn.close()

def server_listen(bind="0.0.0.0", port=""):
    conf['lsocket'] = s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind((bind, port))
    s.listen(1)
    try:
        while 1:
            conn, addr = s.accept()
            thread.start_new_thread(process_con, (conn, addr))
        conn.close()
    except KeyboardInterrupt, e:
        pass
    s.close()
    s = None

def doping(s):
    s.settimeout(14)
    pong = 0

    send("PING:%s:%s:%s:%i" %(socket.gethostname(), sys.platform, os.name, 
        time.time()), s)

    try:
        fd = socket._fileobject(s)
        for line in fd:
            line = line.strip()
            #print "-->", [line]
            if line.upper().startswith("PONG"):
                print "Pong from %s, remote info: Platform: %s, Name: %s,\
Time: %s" %(line.split(":")[1], line.split(":")[2], 
                    line.split(":")[3], strftime("%Y-%m-%d %H:%M:%S", 
                        line.split(":")[4]))
                s.close()
                conf['lastpong'] = time.time()
                conf['alarm']['pong']['ip'] = conf['remote']['host']
                conf['alarm']['pong']['hostname'] = line.split(":")[1]
                return 1
    except Exception, e:
        #print e
        return 0
    return pong

def doalarm(t = ""):
    if not t:
        print "ALARM!"
    if t == "ping":
        print "ALARM! Last ping sendt %s secounds ago." %(
            time.time() - conf['lastping'])
        mail_alarm("%s Last ping to %s (%s) sucsessfully sendt %s\
 secounds ago." %(
            strftime("%Y-%m-%d %H:%M:%S", time.time()), 
            conf['alarm']['ping']['hostname'], conf['alarm']['ping']['ip'], 
            time.time() - conf['lastping']))
    elif t == "pong":
        print "ALARM! Last pong received %s secounds ago." %(
        time.time() - conf['lastpong'])
        mail_alarm("%s Last pong from %s (%s) received %s secounds ago." %(
        strftime("%Y-%m-%d %H:%M:%S", time.time()), 
        conf['alarm']['pong']['hostname'], conf['alarm']['pong']['ip'], 
        time.time() - conf['lastpong']))

def dook(t):
    if t == "ping":
        print "MESSAGE! ping sucsessfully sendt after being down."
        mail_alarm("%s %s (%s) is now up again, after being down in % \
secounds.\r\n(%s - %s)" %(
            strftime("%Y-%m-%d %H:%M:%S", time.time()), 
            conf['alarm']['ping']['hostname'], conf['alarm']['ping']['ip'], 
            time.time() - conf['alarm']['ping']['tmplastping'], 
            strftime("%Y-%m-%d %H:%M:%S", 
            conf['alarm']['ping']['tmplastping']), 
            strftime("%Y-%m-%d %H:%M:%S", time.time())))

    elif t == "pong":
        print "MESSAGE! pong received after being down."
        mail_alarm("%s %s (%s) is now up again, after being down in % \
secounds.\r\n(%s - %s)" %(
            strftime("%Y-%m-%d %H:%M:%S", time.time()), 
            conf['alarm']['pong']['hostname'], conf['alarm']['pong']['ip'], 
            time.time() - conf['alarm']['pong']['tmplastpong'], 
            strftime("%Y-%m-%d %H:%M:%S", 
                conf['alarm']['pong']['tmplastpong']), 
            strftime("%Y-%m-%d %H:%M:%S", time.time())))

def alarm_thread():
    time.sleep(60) #we don't want it to trigger on startup
    while 1:
        if time.time() - conf['lastping'] >= conf['alarm']['timeout']:
            if time.time() - conf['alarm']['ping']['lastsendt'
                ] >= conf['alarm']['spacing']:
                doalarm("ping")
                conf['alarm']['ping']['trigged'] = 1
                conf['alarm']['ping']['lastsendt'] = time.time()
                conf['alarm']['ping']['tmplastping'] = conf['lastping']
        elif conf['alarm']['ping']['trigged']:
            conf['alarm']['ping']['trigged'] = 0
            dook("ping")
            
        if time.time() - conf['lastpong'] >= conf['alarm']['timeout']:
            if time.time() - conf['alarm']['pong']['lastsendt'
                ] >= conf['alarm']['spacing']:
                doalarm("pong")
                conf['alarm']['pong']['trigged'] = 1
                conf['alarm']['pong']['lastsendt'] = time.time()
                conf['alarm']['ping']['tmplastping'] = conf['lastping']
        elif conf['alarm']['pong']['trigged']:
            conf['alarm']['pong']['trigged'] = 0
            dook("pong")
        time.sleep(10)
def ping_thread():
    time.sleep(1)
    while 1:
        try:
            s = connect(conf['remote']['host'], conf['remote']['port'])
            if not s:
                raise Exception, "[ERR] Cannot connect to remote host."
            x = doping(s)
            if not x:
                raise Exception, "[ERR] No pong."    
        except Exception, e:
            pass#print e;#doalarm()
        try:
            s.close()
        except: pass
        s = None
        time.sleep(15)

def mail_alarm(message):
    if message.strip():
        if conf['mail']['enabled']:
            server = smtplib.SMTP(conf['mail']['server'])
            if not conf['mail']['fromaddr']:
                conf['mail']['fromaddr'] = "PingRing@%s" %(socket.gethostname())
            msg = ""
            msg += "From: %s\r" %(conf['mail']['fromaddr'])
            msg += "To: %s\r" %(conf['mail']['toaddr'])
            msg += "Subject: Alert from PingRing\r"
            msg += "\r\n"
            msg += message
            server.sendmail(conf['mail']['fromaddr'], conf['mail']['toaddr'], 
                msg)
            server.quit()

if len(sys.argv) > 1:
    for arg in sys.argv:
        if arg.startswith("--listen="):
            arg = arg.split("--listen=")[1]
            try:
                port = int(arg)
            except Exception, e:
                print "Listen port must be integer."
                sys.exit()
            conf['bind']['port'] = int(port)
                
        elif arg.startswith("--remote="):
            arg = arg.split("--remote=")[1]
            if ":" in arg:
                host, port = arg.split(":")
                try:
                    port = int(port)
                except Exception, e:
                    print "Port must be integer."
                    sys.exit()
                try:
                    x = socket.gethostbyname(host)
                except Exception, e:
                    print "cannot resolve suplyed hostname"
                    sys.exit()
                conf['remote']['host'] = host
                conf['remote']['port'] = int(port)
            else:
                host = arg
                try:
                    x = socket.gethostbyname(host)
                except Exception, e:
                    print "cannot resolve suplyed hostname"
                    sys.exit()
        elif arg.startswith("--timeout="):
            arg = arg.split("--timeout=")[1]
            try:
                timeout = int(arg)
            except Exception, e:
                print "Timeout must be integer."
                sys.exit()
            if int(arg) < 60:
                print "Timeout must be greater than 60 secounds."
                sys.exit()
            conf['alarm']['timeout'] = int(arg)
        elif arg.startswith("--help") or arg.startswith("-h") or\
            arg.startswith("/h"):
            print """Usage:
    %s [--listen=<port>] [--remote=<host>[:<port>]] [--timeout=<int>]
    
    --listen    Port to bind to.
    --remote    Host to connect to.
    --timeout   Time after last ping received before sending email.
    --mail      Send mail on ping timeout. (Default) (Not yet implemented)
    --nomail    Oposite of above.
    --mailto    Address to mail. (Not yet implemented)
    --mailfrom  Address mail apears to come from. (Not yet implemented)
    --mailhost  Adress to smtp server. (Not yet implemented)
    
    (Not yet implemented) simply means that the cli argument dosn't work
    the functionality is however there, but you have to edit the script.""" %(
                sys.argv[0])
            sys.exit()

thread.start_new_thread(server_listen, (conf['bind']['host'],
    conf['bind']['port']))
time.sleep(1)
thread.start_new_thread(ping_thread, ())
time.sleep(1)
thread.start_new_thread(alarm_thread, ())


try:
    while 1:
        time.sleep(2) # Just waiting for u'r ^C
except KeyboardInterrupt, e:
    print "Closing listening socket..."
    try:
        conf['lsocket'].close()
    except: pass
    print "Exiting!"
    sys.exit()

