#!/bin/sh

while [ 1 -le 1 ]
do
  killall -HUP tor
  curl --socks5 127.0.0.1:9050 http://checkip.amazonaws.com/
  sleep 30s
done