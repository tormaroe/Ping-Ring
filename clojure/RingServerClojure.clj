(use 'clojure.contrib.server-socket 'clojure.contrib.duck-streams)
(import '(java.net Socket) '(java.util Date))

(defstruct server-args :this-port :other-port :max-delay :initial-ping)

(def last-ping-time 
		 (atom (Date. (long 0)))) ; January 1, 1970, 00:00:00 GMT

(defn send-ping [args]
			(future 
				(Thread/sleep 1000)
				(try (spit ; is this the coolest function name or what?!
							 (Socket. "127.0.0.1"  (args :other-port)) 
							 (str     "PING from " (args :this-port)))
						 (catch Exception e
										(println "*** Failed sending ping!")))))

(defn listen-for-pings [args]
			(create-server (args :this-port)
										 (fn [in-stream _]
												 (reset! last-ping-time (Date.)) ; that's now!
												 (println "Received" (slurp* in-stream))
												 (send-ping args))))

(defn date-diff "get diff of two dates in seconds" [a b]
			(-> (- (.getTime a) (.getTime b))
					(/ 1000) int))

(defn ping-delay "get time since last ping in seconds" [] 
				(date-diff (Date.) @last-ping-time))

(defn ping-delayed? [max-delay]
			(> (ping-delay) max-delay))

(defn watch-for-missing-pings [args]
			(Thread/sleep 5000)
			(when (ping-delayed? (args :max-delay)) 
				(println "*** ALERT, RING BROKEN!"
								 "No ping in" (ping-delay) "seconds.")
				(send-ping args))
			(recur args)) ; AGAIN!

(defn main [args]
			(println (format "**Clojure Ring Server (%s)" (args :this-port)))
			(when (args :initial-ping) 
				(send-ping args))
			(future (listen-for-pings args))
			(future (watch-for-missing-pings args)))

(main (struct server-args ; parse command line args into struct
							(Integer. (nth *command-line-args* 0)) ; this-port
							(Integer. (nth *command-line-args* 1)) ; other-port
							(Integer. (nth *command-line-args* 2)) ; max-delay
							(Boolean. (nth *command-line-args* 3)))) ; initial-ping
