{-# LANGUAGE UnicodeSyntax, NamedFieldPuns, RecordWildCards #-}
module Main where

import Control.Concurrent
import Control.Monad ( when )
import Data.Monoid.Unicode
import Network
import Prelude.Unicode
import System.Environment ( getArgs, getProgName )
import System.Exit ( ExitCode(⋯), exitWith )
import System.IO ( hPutStrLn, stderr, hGetContents )
import System.Time ( ClockTime(⋯), getClockTime )

data State = State { timeout  ∷ Integer
                   , sendPing ∷ MVar ()
                   , lastPing ∷ MVar Integer
                   , myPort   ∷ PortNumber
                   , nextPort ∷ PortNumber
                   }

main ∷ IO ()
main = withSocketsDo $ do
    opts ← getArgs
    when ("-h" ∈ opts ∨ length opts < 4 ∨ opts !! 3 ∉ ["true", "false"]) $ do
         name ← getProgName
         hPutStrLn stderr ("Usage: " ⊕ name ⊕ " <own port> <next port>\
                                              \ <timeout> <start?>\n" ⊕
                           "Example: " ⊕ name ⊕ " 8000 8001 10 false")
         exitWith (ExitFailure 1)
    let myPort   = fromIntegral ∘ read $ opts !! 0
        nextPort = fromIntegral ∘ read $ opts !! 1
        timeout  = read $ opts !! 2
        start    = opts !! 3 ≡ "true"

    sendPing ← if start then newMVar () else newEmptyMVar
    lastPing ← getClockTimeMS >>= newMVar

    print myPort
    _ ← forkIO (pingSurveillance State {⋯})
    _ ← forkIO (pingReceiver State {⋯})
    pingSender State {⋯}

pingReceiver ∷ State → IO ()
pingReceiver State {myPort, sendPing, lastPing} = do
    sock ← listenOn (PortNumber myPort)
    loop sock
  where loop sock = do (h, _, _) ← accept sock
                       str ← hGetContents h
                       putStrLn str
                       putMVar sendPing ()
                       modifyMVar_ lastPing $ const getClockTimeMS
                       loop sock

pingSender ∷ State → IO ()
pingSender s@State {nextPort, myPort, sendPing} = do
    t ← getClockTimeMS
    sender t `catch` catcher
    pingSender s
  where sender t = do
            _ ← takeMVar sendPing
            now ← getClockTimeMS
            when (now - t < 10^6) $
                 threadDelay $ fromIntegral (10^6 - (now - t))
            sendTo "localhost" (PortNumber nextPort)
                   ("Ping from " ⊕ show nextPort)
        catcher _ = putStrLn $ "Couldn't send ping to " ⊕ show nextPort

pingSurveillance ∷ State → IO ()
pingSurveillance s@State {timeout, sendPing, lastPing} = do
    threadDelay 5000000
    t ← readMVar lastPing
    now ← getClockTimeMS
    when (now - t > timeout * 10^6) $ do
        putStrLn $ "ALARM! It has been " ⊕ show ((now - t) `div` 10^6)
                 ⊕ " seconds since last ping"
        putMVar sendPing ()
    pingSurveillance s

getClockTimeMS ∷ IO Integer
getClockTimeMS = do
    (TOD s p) ← getClockTime
    return $ fromIntegral (s * 1000000 + p `div` 10^6)