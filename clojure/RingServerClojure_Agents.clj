(use 'clojure.contrib.server-socket 'clojure.contrib.duck-streams)
(import '(java.net Socket))

(def this-port    (Integer. (nth *command-line-args* 0)))
(def other-port   (Integer. (nth *command-line-args* 1)))
(def max-delay    (Integer. (nth *command-line-args* 2)))
(def initial-ping (Boolean. (nth *command-line-args* 3)))

(def has-received-ping? (atom false))

(defn ping []
      (future (Thread/sleep 1000)
              (try (spit 
                     (Socket. "127.0.0.1"  other-port) 
                     (str     "PING from " this-port))
                   (catch Exception e
                          (println "*** Failed sending ping!")))))

(defn listen-for-pings []
      (create-server this-port
                     (fn [in-stream _]
                         (reset! has-received-ping? true)
                         (println "Received" (slurp* in-stream))
                         (ping))))

(def alerter (agent 0)) ; value is number of alerts in a row

(defn send-alert [delay]
      (println "*** ALERT, RING BROKEN! No ping in" delay "seconds.")
      (ping))

(defn check-delay [alert-count]
      (let [new-count (inc alert-count)]
        (Thread/sleep (* max-delay 1000))
        (send-off *agent* check-delay) ; queue another check
        (if @has-received-ping? 
          (do (reset! has-received-ping? false) 0)
          (do (send-alert (* max-delay new-count)) new-count))))

(defn main []
     (println (format "**Clojure Ring Server (with Agents) (%s)" 
                      this-port))
     (when initial-ping (ping))
     (send-off alerter check-delay)
     (listen-for-pings))

(main)
